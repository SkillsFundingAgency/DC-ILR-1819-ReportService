using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Tests.AutoFac;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Reports.Main_Occupancy
{
    public sealed class TestMainOccupancyReportWithTestData
    {
#if DEBUG
        [Theory]
        //[InlineData(@"Reports\Main Occupancy\ILR-10033670-1819-20181030-101919-07.xml", @"Reports\Main Occupancy\FundingFm35Output_FM35TNP13.json", "fm35 TNP 13")]
        //[InlineData(@"Reports\Main Occupancy\ILR-10033670-1819-20181203-143338-25.xml", @"Reports\Main Occupancy\FundingFm35Output_fm25 19C 1.json", "fm25 19C 1")]
        [InlineData(@"Reports\Main Occupancy\ILR-10033670-1819-20181205-135040-25.xml", @"Reports\Main Occupancy\FundingFm35Output_fm25 19T 1.json", "fm25 19T 1")]
#endif
        public async Task TestMainOccupancyReportGeneration(string ilrFilename, string fm35Filename, string validLearner)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Main Occupancy Report {dateTime:yyyyMMdd-HHmmss}";
            List<string> validLearners = new List<string> { validLearner };

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IKeyValuePersistenceService> redis = new Mock<IKeyValuePersistenceService>();
            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
            IFM35ProviderService fm35ProviderService = new FM35ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, null, null);
            IFM25ProviderService fm25ProviderService = new FM25ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, null);
            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, null, null);
            LarsConfiguration larsConfiguration = new LarsConfiguration
            {
                LarsConnectionString = ConfigurationManager.AppSettings["LarsConnectionString"]
            };
            ILarsProviderService larsProviderService = new LarsProviderService(logger.Object, larsConfiguration);
            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();
            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            IMainOccupancyReportModelBuilder reportModelBuilder = new MainOccupancyReportModelBuilder();
            IValueProvider valueProvider = new ValueProvider();

            storage.Setup(x => x.GetAsync(ilrFilename, It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(ilrFilename).CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            storage.Setup(x => x.GetAsync("FundingFm35Output", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(fm35Filename).CopyTo(sr)).Returns(Task.CompletedTask);
            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(jsonSerializationService.Serialize(validLearners));

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns(ilrFilename);
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");
            reportServiceContextMock.SetupGet(x => x.FundingFM35OutputKey).Returns("FundingFm35Output");
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, null);

            var mainOccupancyReport = new MainOccupancyReport(
                logger.Object,
                storage.Object,
                ilrProviderService,
                stringUtilitiesService,
                validLearnersService,
                fm25ProviderService,
                fm35ProviderService,
                larsProviderService,
                dateTimeProviderMock.Object,
                valueProvider,
                topicsAndTasks,
                reportModelBuilder);

            await mainOccupancyReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();

            File.WriteAllText($"{filename}.csv", csv);
        }
    }
}
