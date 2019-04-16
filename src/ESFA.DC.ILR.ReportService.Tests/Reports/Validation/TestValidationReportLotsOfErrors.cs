using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Poco;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Tests.AutoFac;
using ESFA.DC.ILR.ReportService.Tests.Helpers;
using ESFA.DC.ILR.ReportService.Tests.Models;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Reports.Validation
{
    public sealed class TestValidationReportLotsOfErrors
    {
        [Fact]
        public async Task TestValidationReportGeneration()
        {
            string csv = string.Empty;
            string json = string.Empty;
            byte[] xlsx = null;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10000020_1_Rule Violation Report {dateTime:yyyyMMdd-HHmmss}";
            string ilrFilename = @"Reports\Validation\ILR-10000020-1819-20181005-120953-03.xml";

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
            storage.Setup(x => x.GetAsync("ValidationErrors", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText(@"Reports\Validation\ValidationErrors.json"));
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            validationErrorsServiceMock.Setup(x => x.PopulateValidationErrors(It.IsAny<string[]>(), It.IsAny<List<ValidationErrorDetails>>(), It.IsAny<CancellationToken>())).Callback<string[], List<ValidationErrorDetails>, CancellationToken>(
                (s, v, c) =>
                {
                    List<ValidationErrorDetails> validationErrorDetails = jsonSerializationService.Deserialize<List<ValidationErrorDetails>>(File.ReadAllText(@"Reports\Validation\ValidationErrorsLookup.json"));
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

            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, null, null);

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
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10000020);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns(@"Reports\Validation\" + Path.GetFileNameWithoutExtension(ilrFilename));
            reportServiceContextMock.SetupGet(x => x.ValidationErrorsKey).Returns("ValidationErrors");
            reportServiceContextMock.SetupGet(x => x.ValidationErrorsLookupsKey).Returns("ValidationErrorsLookup");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersCount).Returns(2);
            reportServiceContextMock.SetupGet(x => x.InvalidLearnRefNumbersCount).Returns(3);
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            await validationErrorsReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            json.Should().NotBeNullOrEmpty();
            csv.Should().NotBeNullOrEmpty();
            xlsx.Should().NotBeNullOrEmpty();

#if DEBUG
            File.WriteAllBytes($"{filename}.xlsx", xlsx);
#endif

            ValidationErrorMapper helper = new ValidationErrorMapper();
            TestCsvHelper.CheckCsv(csv, new CsvEntry(helper, 1));
            TestXlsxHelper.CheckXlsx(xlsx, new XlsxEntry(helper, 5));
        }
    }
}