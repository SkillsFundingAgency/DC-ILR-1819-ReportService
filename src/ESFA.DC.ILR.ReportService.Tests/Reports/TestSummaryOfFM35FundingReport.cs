using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Builders;
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
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Tests.Reports
{
    public class TestSummaryOfFM35FundingReport
    {
        [Fact]
        public async Task TestSummaryOfFM35FundingReportGeneration()
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Summary of Funding Model 35 Funding Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IStreamableKeyValuePersistenceService> redis = new Mock<IStreamableKeyValuePersistenceService>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            ITotalBuilder totalBuilder = new TotalBuilder();
            IFm35Builder fm35Builder = new Fm35Builder(totalBuilder, new CacheProviderService<LearningDelivery[]>());
            IValueProvider valueProvider = new ValueProvider();

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns("ILR-10033670-1819-20180704-120055-03");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");
            reportServiceContextMock.SetupGet(x => x.FundingFM35OutputKey).Returns("FundingFm35Output");

            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("ValidLearnRefNumbers.json"));
            redis.Setup(x => x.GetAsync("FundingFm35Output", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("Fm35.json").CopyTo(sr)).Returns(Task.CompletedTask);

            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("ILR-10033670-1819-20180704-120055-03.xml"));
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);

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

            IFM35ProviderService fm35ProviderService = new FM35ProviderService(logger.Object, redis.Object, jsonSerializationService, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);
            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();

            var summaryOfFm35FundingReport = new SummaryOfFm35FundingReport(
                logger.Object,
                storage.Object,
                fm35ProviderService,
                stringUtilitiesService,
                dateTimeProviderMock.Object,
                valueProvider,
                topicsAndTasks,
                fm35Builder);

            await summaryOfFm35FundingReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();

            TestCsvHelper.CheckCsv(csv, new CsvEntry(new SummaryOfFM35FundingMapper(), 1));
        }

        [Theory]
        [InlineData("AEB – Other Learning (non-procured)", 5, "AEB – Other Learning (procured from Nov 2017)", 5, "AEB – Other Learning (procured from Nov 2017)", 5, "AEB – Other Learning (non-procured)", 5)]
        [InlineData("AEB – Other Learning (non-procured)", 6, "AEB – Other Learning (non-procured)", 5, "AEB – Other Learning (non-procured)", 5, "AEB – Other Learning (non-procured)", 6)]
        [InlineData("AEB – Other Learning (procured from Nov 2017)", 1, "AEB – Other Learning (non-procured)", 1, "19-24 Traineeship (procured from Nov 2017)", 1, "19-24 Traineeship (procured from Nov 2017)", 1, "AEB – Other Learning (non-procured)", 1, "AEB – Other Learning (procured from Nov 2017)", 1)]
        public void TestSummaryOfFm35FundingModelComparer(params object[] inputs)
        {
            SummaryOfFm35FundingModelComparer summaryOfFm35FundingModelComparer = new SummaryOfFm35FundingModelComparer();

            List<SummaryOfFm35FundingModel> SummaryOfFm35FundingModels = new List<SummaryOfFm35FundingModel>();
            List<string> expectedFundingLine = new List<string>();
            List<int> expectedPeriod = new List<int>();

            int resultsPointer = inputs.Length - (inputs.Length / 2);

            for (int i = 0; i < (inputs.Length / 2) / 2; i++)
            {
                SummaryOfFm35FundingModels.Add(new SummaryOfFm35FundingModel
                {
                    FundingLineType = inputs[i * 2].ToString(),
                    Period = Convert.ToInt32(inputs[(i * 2) + 1])
                });

                expectedFundingLine.Add(inputs[resultsPointer + (i * 2)].ToString());
                expectedPeriod.Add(Convert.ToInt32(inputs[resultsPointer + (i * 2) + 1]));
            }

            SummaryOfFm35FundingModels.Sort(summaryOfFm35FundingModelComparer);

            SummaryOfFm35FundingModels.Select(x => x.FundingLineType).Should().BeEquivalentTo(expectedFundingLine);
            SummaryOfFm35FundingModels.Select(x => x.Period).Should().BeEquivalentTo(expectedPeriod);
        }
    }
}
