using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Service.Builders;
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
    public class TestHNSReport
    {
        [Theory]
        [InlineData("ILR-10033670-1819-20180704-120055-03.xml", "Fm25.json")]
        public async Task TestHNSReportGeneration(string ilrFilename, string fm25Filename)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_High Needs Students Detail Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns(ilrFilename);
            reportServiceContextMock.SetupGet(x => x.FundingFM25OutputKey).Returns("FundingFm25Output");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            DataStoreConfiguration dataStoreConfiguration = new DataStoreConfiguration()
            {
                ILRDataStoreConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreConnectionString,
                ILRDataStoreValidConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreValidConnectionString
            };
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

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IStreamableKeyValuePersistenceService> redis = new Mock<IStreamableKeyValuePersistenceService>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();

            IFM25ProviderService fm25ProviderService = new FM25ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrRulebaseContextFactory);
            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);
            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, dataStoreConfiguration);
            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();
            IValueProvider valueProvider = new ValueProvider();
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions = TestConfigurationHelper.GetTopicsAndTasks();
            IHNSReportModelBuilder reportModelBuilder = new HNSReportModelBuilder();

            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(ilrFilename).CopyTo(sr))
                .Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value)
                .Returns(Task.CompletedTask);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(jsonSerializationService.Serialize(
                new List<string>
                {
                    "0Addl103",
                    "0ContPr01",
                    "1ContPr01",
                    "2ContPr01"
                }));

            storage.Setup(x => x.ContainsAsync("FundingFm25Output", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("FundingFm25Output", It.IsAny<CancellationToken>()))
                .ReturnsAsync(File.ReadAllText(fm25Filename));
            storage.Setup(x => x.ContainsAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            var hnsReport = new HNSDetailReport(
                logger.Object,
                storage.Object,
                ilrProviderService,
                validLearnersService,
                fm25ProviderService,
                dateTimeProviderMock.Object,
                valueProvider,
                topicAndTaskSectionOptions,
                reportModelBuilder);

            await hnsReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();

#if DEBUG
            File.WriteAllText($"{filename}.csv", csv);
#endif

            TestCsvHelper.CheckCsv(csv, new CsvEntry(new HNSMapper(), 1));
        }
    }
}
