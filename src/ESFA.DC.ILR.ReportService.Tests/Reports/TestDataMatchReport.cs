using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Data.DAS.Model;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.DataMatch;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Service.Service.DataMatch;
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

namespace ESFA.DC.ILR.ReportService.Tests.Reports
{
    public sealed class TestDataMatchReport
    {
        [Fact]
        public async Task TestDataMatchReportGeneration()
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Apprenticeship Data Match Report {dateTime:yyyyMMdd-HHmmss}";
            long ukPrn = 10033670;
            string ilr = "ILR-10033670-1819-20180704-120055-03";
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

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns(ilr);
            reportServiceContextMock.SetupGet(x => x.FundingFM36OutputKey).Returns("FundingFm36Output");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IKeyValuePersistenceService> redis = new Mock<IKeyValuePersistenceService>();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IFM36ProviderService fm36ProviderService = new FM36ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrRulebaseContextFactory);
            Mock<IDasCommitmentsService> dasCommitmentsService = new Mock<IDasCommitmentsService>();
            Mock<IPeriodProviderService> periodProviderService = new Mock<IPeriodProviderService>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();

            List<DasCommitment> dasCommitments = new List<DasCommitment>
            {
                new DasCommitment(new DasCommitments
                {
                    Uln = 9900001906,
                    Ukprn = ukPrn,
                    //StandardCode = 0,
                    FrameworkCode = 421, // No match - 420
                    PathwayCode = 2, // No match - 1
                    ProgrammeType = 3, // No match - 2
                    AgreedCost = 1.80M, // No match?
                    StartDate = dateTime, // No match
                    PaymentStatus = (int)PaymentStatus.Active
                })
            };

            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            redis.Setup(x => x.GetAsync("FundingFm36Output", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("Fm36.json"));
            dasCommitmentsService
                .Setup(x => x.GetCommitments(It.IsAny<long>(), It.IsAny<List<long>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dasCommitments);
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            periodProviderService.Setup(x => x.MonthFromPeriod(It.IsAny<int>())).Returns(1);

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            IValueProvider valueProvider = new ValueProvider();
            IValidationStageOutputCache validationStageOutputCache = new ValidationStageOutputCache();
            IDatalockValidationResultBuilder datalockValidationResultBuilder = new DatalockValidationResultBuilder();
            ITotalBuilder totalBuilder = new TotalBuilder();

            var report = new DataMatchReport(
                logger.Object,
                fm36ProviderService,
                dasCommitmentsService.Object,
                periodProviderService.Object,
                storage.Object,
                dateTimeProviderMock.Object,
                valueProvider,
                topicsAndTasks,
                validationStageOutputCache,
                datalockValidationResultBuilder,
                totalBuilder);

            await report.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new DataMatchReportMapper(), 1));
        }

        [Fact]
        public async Task TestDataCommitmentsComparer()
        {
            List<DasCommitments> dasCommitments = new List<DasCommitments>
            {
                new DasCommitments
                {
                    CommitmentId = 1,
                    VersionId = "2"
                },
                new DasCommitments
                {
                    CommitmentId = 1,
                    VersionId = "2"
                }
            };

            dasCommitments = dasCommitments.Distinct(new DasCommitmentsComparer()).ToList();
            Assert.Single(dasCommitments);
        }

        [Fact]
        public async Task TestDataMatchModelComparer1()
        {
            List<DataMatchModel> dataMatchModels = new List<DataMatchModel>
            {
                new DataMatchModel
                {
                    LearnRefNumber = "321",
                    AimSeqNumber = 321,
                    RuleName = "Rule_2"
                },
                new DataMatchModel
                {
                    LearnRefNumber = "123",
                    AimSeqNumber = 123,
                    RuleName = "Rule_1"
                }
            };

            dataMatchModels.Sort(new DataMatchModelComparer());
            Assert.Equal("123", dataMatchModels[0].LearnRefNumber);
        }

        [Fact]
        public async Task TestDataMatchModelComparer2()
        {
            List<DataMatchModel> dataMatchModels = new List<DataMatchModel>
            {
                new DataMatchModel
                {
                    LearnRefNumber = "321",
                    AimSeqNumber = 321,
                    RuleName = "Rule_2"
                },
                new DataMatchModel
                {
                    LearnRefNumber = "321",
                    AimSeqNumber = 123,
                    RuleName = "Rule_1"
                }
            };

            dataMatchModels.Sort(new DataMatchModelComparer());
            Assert.Equal(123, dataMatchModels[0].AimSeqNumber);
        }

        [Fact]
        public async Task TestDataMatchModelComparer3()
        {
            List<DataMatchModel> dataMatchModels = new List<DataMatchModel>
            {
                new DataMatchModel
                {
                    LearnRefNumber = "321",
                    AimSeqNumber = 321,
                    RuleName = "Rule_2"
                },
                new DataMatchModel
                {
                    LearnRefNumber = "321",
                    AimSeqNumber = 321,
                    RuleName = "Rule_1"
                }
            };

            dataMatchModels.Sort(new DataMatchModelComparer());
            Assert.Equal("Rule_1", dataMatchModels[0].RuleName);
        }
    }
}