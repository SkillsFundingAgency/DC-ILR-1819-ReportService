using System;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
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

namespace ESFA.DC.ILR.ReportService.Tests.Reports
{
    public class TestTrailblazerEmployerIncentiveReport
    {
        // Used to ensure EF is initialised
        private static SqlProviderServices instance = SqlProviderServices.Instance;

        [Theory]
        [InlineData("ILR-10033670-1819-20180831-094549-03.xml", "ValidLearnRefNumbers.json", "Fm81.json")]
        public async Task TestTrailblazerEmployerIncentiveReportGeneration(string ilrFilename, string validLearnRefNumbersFilename, string fm81FileName)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Trailblazer Apprenticeships Employer Incentives Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            Mock<IKeyValuePersistenceService> redis = new Mock<IKeyValuePersistenceService>();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
            IFM81TrailBlazerProviderService fm81TrailBlazerProviderService = new FM81TrailBlazerProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, null, null);
            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, null, intUtilitiesService, null, null);

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns(ilrFilename);
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");
            reportServiceContextMock.SetupGet(x => x.FundingFM81OutputKey).Returns("FundingFm81Output");
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, null);
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            ITrailblazerEmployerIncentivesModelBuilder reportModelBuilder = new TrailblazerEmployerIncentivesModelBuilder();
            IValueProvider valueProvider = new ValueProvider();

            var validLearnersStr = File.ReadAllText(validLearnRefNumbersFilename);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(ilrFilename).CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(ilrFilename).CopyTo(sr)).Returns(Task.CompletedTask);

            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("FundingFm81Output", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText(fm81FileName));
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(validLearnersStr);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            var trailblazerEmployerIncentivesReport = new TrailblazerEmployerIncentivesReport(
                logger.Object,
                storage.Object,
                fm81TrailBlazerProviderService,
                ilrProviderService,
                validLearnersService,
                reportModelBuilder,
                topicsAndTasks,
                valueProvider,
                dateTimeProviderMock.Object);

            await trailblazerEmployerIncentivesReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllBytes($"{filename}.csv", Encoding.ASCII.GetBytes(csv));

            //TestCsvHelper.CheckCsv(csv, new CsvEntry(new MainOccupancyFM25Mapper(), 14), new CsvEntry(new MainOccupancyFM35Mapper(), 14));
        }
    }
}