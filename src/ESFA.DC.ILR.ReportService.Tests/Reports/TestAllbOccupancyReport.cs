using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.Lars;
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
    public sealed class TestAllbOccupancyReport
    {
        [Fact]
        public async Task TestAllbOccupancyReportGeneration()
        {
            string csv = string.Empty;
            System.DateTime dateTime = System.DateTime.UtcNow;
            string filename = $"10033670_1_ALLB Occupancy Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();

            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IStreamableKeyValuePersistenceService> redis = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();

            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("ILR-10033670-1819-20180704-120055-03.xml").CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("FundingALBOutputKey", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("ALBOutput1000.json").CopyTo(sr)).Returns(Task.CompletedTask);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(jsonSerializationService.Serialize(
                new List<string>
                {
                    "3Addl103",
                    "4Addl103"
                }));
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
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

            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);
            Mock<ILarsProviderService> larsProviderService = new Mock<ILarsProviderService>();
            larsProviderService.Setup(x => x.GetLearningDeliveriesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, LarsLearningDelivery>()
                {
                    { "60133533", new LarsLearningDelivery { LearningAimTitle = "A", NotionalNvqLevel = "B", Tier2SectorSubjectArea = 3 } }
                });

            IAllbProviderService allbProviderService = new AllbProviderService(logger.Object, redis.Object, jsonSerializationService, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns("ILR-10033670-1819-20180704-120055-03");
            reportServiceContextMock.SetupGet(x => x.FundingALBOutputKey).Returns("FundingAlbOutput");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");

            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, null);

            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();

            IValueProvider valueProvider = new ValueProvider();

            IReport allbOccupancyReport = new AllbOccupancyReport(
                logger.Object,
                storage.Object,
                ilrProviderService,
                larsProviderService.Object,
                allbProviderService,
                validLearnersService,
                stringUtilitiesService,
                dateTimeProviderMock.Object,
                valueProvider,
                topicsAndTasks);

            await allbOccupancyReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AllbOccupancyMapper(), 1));
        }
    }
}
