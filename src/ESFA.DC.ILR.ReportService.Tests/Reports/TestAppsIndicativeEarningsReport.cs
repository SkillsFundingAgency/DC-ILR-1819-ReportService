using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings;
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
    public sealed class TestAppsIndicativeEarningsReport
    {
        [Theory]
        //[InlineData("ILR-10033670-1819-20181206-093952-03", "Fm36.json")]
        [InlineData("ILR-10033670-1819-20181212-103550-36", "66230-FundingFm36Output.json")]
        public async Task TestAppsIndicativeEarningsReportGeneration(string ilrFile, string fm36Output)
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Apps Indicative Earnings Report {dateTime:yyyyMMdd-HHmmss}";
            string ilr = ilrFile;
            DataStoreConfiguration dataStoreConfiguration = new DataStoreConfiguration()
            {
                ILRDataStoreConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreConnectionString,
                ILRDataStoreValidConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreValidConnectionString
            };

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns(ilr);
            reportServiceContextMock.SetupGet(x => x.FundingFM36OutputKey).Returns("FundingFm36Output");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IKeyValuePersistenceService> redis = new Mock<IKeyValuePersistenceService>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
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
            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, dataStoreConfiguration);
            IFM36ProviderService fm36ProviderService = new FM36ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrRulebaseContextFactory);
            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();
            ITotalBuilder totalBuilder = new TotalBuilder();

            List<IAppsIndicativeCommand> commands = new List<IAppsIndicativeCommand>()
            {
                new AppsIndicativeAugustCommand()
            };

            AppsIndicativeEarningsModelBuilder builder = new AppsIndicativeEarningsModelBuilder(commands, totalBuilder, stringUtilitiesService);

            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead($"{ilr}.xml").CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(jsonSerializationService.Serialize(
                new List<string>
                {
                    "3DOB01",
                    "0fm3501",
                    "1fm3501",
                    "2fm3501",
                    "3fm3501",
                    "4fm3501",
                    "5fm3501",
                    "6fm3501",
                    "7fm3501",
                    "8fm3501",
                    "9fm3501",
                    "Afm3501",
                    "Bfm3501",
                    "Cfm3501",
                    "Dfm3501",
                    "Efm3501",
                    "0fm3601",
                    "0DOB52",
                    "fm36 18 20"
                }));
            redis.Setup(x => x.ContainsAsync("ValidLearners", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.ContainsAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.ContainsAsync("FundingFm36Output", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("FundingFm36Output", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText(fm36Output));
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);

            IMessage message = await ilrProviderService.GetIlrFile(reportServiceContextMock.Object, CancellationToken.None);
            string validLearners = await redis.Object.GetAsync("ValidLearnRefNumbers");
            Dictionary<string, LarsLearningDelivery> learningDeliveriesDict = new Dictionary<string, LarsLearningDelivery>();

            foreach (ILearner messageLearner in message.Learners)
            {
                if (!validLearners.Contains(messageLearner.LearnRefNumber))
                {
                    continue;
                }

                foreach (ILearningDelivery learningDelivery in messageLearner.LearningDeliveries)
                {
                    learningDeliveriesDict[learningDelivery.LearnAimRef] = new LarsLearningDelivery()
                    {
                        LearningAimTitle = "A",
                        NotionalNvqLevel = "B",
                        Tier2SectorSubjectArea = 3
                    };
                }
            }

            larsProviderService.Setup(x => x.GetLearningDeliveriesAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(learningDeliveriesDict);
            larsProviderService.Setup(x => x.GetStandardAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync("NotionalEndLevel");

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            IValueProvider valueProvider = new ValueProvider();

            var report = new AppsIndicativeEarningsReport(
                logger.Object,
                storage.Object,
                ilrProviderService,
                validLearnersService,
                fm36ProviderService,
                larsProviderService.Object,
                builder,
                dateTimeProviderMock.Object,
                valueProvider,
                topicsAndTasks);

            await report.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllText($"{filename}.csv", csv);
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AppsIndicativeEarningsMapper(), 1));
        }
    }
}