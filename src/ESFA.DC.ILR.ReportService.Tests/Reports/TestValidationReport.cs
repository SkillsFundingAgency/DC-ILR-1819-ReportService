using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.Poco;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Tests.AutoFac;
using ESFA.DC.ILR.ReportService.Tests.Helpers;
using ESFA.DC.ILR.ReportService.Tests.Models;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Reports
{
    public sealed class TestValidationReport
    {
        [Fact]
        public async Task TestValidationReportGeneration()
        {
            string csv = string.Empty;
            string json = string.Empty;
            byte[] xlsx = null;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Rule Violation Report {dateTime:yyyyMMdd-HHmmss}";
            string ilrFilename = @"ILR-10033670-1819-20180712-144437-03.xml";

            DataStoreConfiguration dataStoreConfiguration = new DataStoreConfiguration()
            {
                ILRDataStoreConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreConnectionString,
                ILRDataStoreValidConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreValidConnectionString
            };

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            IValueProvider valueProvider = new ValueProvider();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            Mock<IValidationErrorsService> validationErrorsServiceMock = new Mock<IValidationErrorsService>();

            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText(ilrFilename));
            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(ilrFilename).CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.json", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => json = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.xlsx", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>(
                (key, value, ct) =>
                {
                    value.Seek(0, SeekOrigin.Begin);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        value.CopyTo(ms);
                        xlsx = ms.ToArray();
                    }
                })
                .Returns(Task.CompletedTask);
            storage.Setup(x => x.GetAsync("ValidationErrors", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("ValidationErrors.json"));
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            validationErrorsServiceMock.Setup(x => x.PopulateValidationErrors(It.IsAny<string[]>(), It.IsAny<List<ValidationErrorDetails>>(), It.IsAny<CancellationToken>())).Callback<string[], List<ValidationErrorDetails>, CancellationToken>(
                (s, v, c) =>
                {
                    List<ValidationErrorDetails> validationErrorDetails = jsonSerializationService.Deserialize<List<ValidationErrorDetails>>(File.ReadAllText("ValidationErrorsLookup.json"));
                    foreach (ValidationErrorDetails veds in v)
                    {
                        ValidationErrorDetails rule = validationErrorDetails.SingleOrDefault(x => string.Equals(x.RuleName, veds.RuleName, StringComparison.OrdinalIgnoreCase));
                        if (rule != null)
                        {
                            veds.Message = rule.Message;
                            veds.Severity = rule.Severity;
                        }
                    }
                }).Returns(Task.CompletedTask);

            IIlr1819ValidContext IlrValidContextFactory()
            {
                var options = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>().UseSqlServer(dataStoreConfiguration.ILRDataStoreValidConnectionString).Options;
                return new ILR1819_DataStoreEntitiesValid(options);
            }

            IIlr1819RulebaseContext IlrRulebaseContextFactory()
            {
                var options = new DbContextOptionsBuilder<ILR1819_DataStoreEntities>().UseSqlServer(dataStoreConfiguration.ILRDataStoreConnectionString).Options;
                return new ILR1819_DataStoreEntities(options);
            }

            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            IValidationStageOutputCache validationStageOutputCache = new ValidationStageOutputCache();

            IReport validationErrorsReport = new ValidationErrorsReport(
                logger.Object,
                storage.Object,
                jsonSerializationService,
                ilrProviderService,
                dateTimeProviderMock.Object,
                valueProvider,
                topicsAndTasks,
                validationErrorsServiceMock.Object,
                validationStageOutputCache);

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns("ILR-10033670-1819-20180712-144437-03");
            reportServiceContextMock.SetupGet(x => x.ValidationErrorsKey).Returns("ValidationErrors");
            reportServiceContextMock.SetupGet(x => x.ValidationErrorsLookupsKey).Returns("ValidationErrorsLookup");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersCount).Returns(2);
            reportServiceContextMock.SetupGet(x => x.InvalidLearnRefNumbersCount).Returns(3);
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            await validationErrorsReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            json.Should().NotBeNullOrEmpty();
            csv.Should().NotBeNullOrEmpty();
            xlsx.Should().NotBeNullOrEmpty();

            ValidationErrorMapper helper = new ValidationErrorMapper();
            TestCsvHelper.CheckCsv(csv, new CsvEntry(helper, 1));
            TestXlsxHelper.CheckXlsx(xlsx, new XlsxEntry(helper, 5));
        }

        [Fact]
        public async Task TestSorter()
        {
            List<ValidationErrorModel> validationErrorModels = new EditableList<ValidationErrorModel>
            {
                new ValidationErrorModel("E", "0R99", "AFinType_13", "...", "...", 2),
                new ValidationErrorModel("E", "3AddHr06", "AddHours_02", "...", "...", 1),
                new ValidationErrorModel("E", "1R99", "UKPRN_10", "...", "...", 3),
                new ValidationErrorModel("W", "0AddHr05", "AddHours_06", "...", "...", 5),
                new ValidationErrorModel("W", "0AddHr06", "AddHours_05", "...", "...", 4),
                new ValidationErrorModel("W", "0AddHr07", "EmpStat_12", "...", "...", 6),
            };

            validationErrorModels.Sort(new ValidationErrorsModelComparer());

            int pointer = 1;
            foreach (ValidationErrorModel validationErrorModel in validationErrorModels)
            {
                validationErrorModel.AimSequenceNumber.Should().Be(pointer++);
            }
        }
    }
}