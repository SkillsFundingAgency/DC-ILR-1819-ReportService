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
using ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.ILR.ReportService.Service.Builders.PeriodEnd;
using ESFA.DC.ILR.ReportService.Service.Mapper.PeriodEnd;
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
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Reports.PeriodEnd.AppsAdditionalPayments
{
    public sealed class TestAppsAdditionalPaymentsReport
    {
        [Fact]
        public async Task TestAppsAdditionalPaymentsReportGeneration()
        {
            string csv = string.Empty;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10036143_1_Apps Additional Payments Report {dateTime:yyyyMMdd-HHmmss}";
            int ukPrn = 10036143;
            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(ukPrn);

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IIlrProviderService> ilrProviderServiceMock = new Mock<IIlrProviderService>();
            Mock<IDASPaymentsProviderService> dasPaymentProviderMock = new Mock<IDASPaymentsProviderService>();
            Mock<IFM36ProviderService> fm36ProviderServiceMock = new Mock<IFM36ProviderService>();
            IValueProvider valueProvider = new ValueProvider();
            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);

            var appsAdditionalPaymentIlrInfo = BuildILRModel(ukPrn);
            var appsAdditionalPaymentRulebaseInfo = BuildFm36Model(ukPrn);
            var appsAdditionalPaymentDasPaymentsInfo = BuildDasPaymentsModel(ukPrn);

            ilrProviderServiceMock.Setup(x => x.GetILRInfoForAppsAdditionalPaymentsReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(appsAdditionalPaymentIlrInfo);
            fm36ProviderServiceMock.Setup(x => x.GetFM36DataForAppsAdditionalPaymentReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(appsAdditionalPaymentRulebaseInfo);
            dasPaymentProviderMock.Setup(x => x.GetPaymentsInfoForAppsAdditionalPaymentsReportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(appsAdditionalPaymentDasPaymentsInfo);

            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            var appsAdditionalPaymentsModelBuilder = new AppsAdditionalPaymentsModelBuilder();

            var report = new ReportService.Service.Reports.PeriodEnd.AppsAdditionalPaymentsReport(logger.Object, storage.Object, ilrProviderServiceMock.Object, fm36ProviderServiceMock.Object, dateTimeProviderMock.Object, valueProvider, topicsAndTasks, dasPaymentProviderMock.Object, appsAdditionalPaymentsModelBuilder);

            await report.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            File.WriteAllText($"{filename}.csv", csv);
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new AppsAdditionalPaymentsMapper(), 1));
        }

        private AppsAdditionalPaymentILRInfo BuildILRModel(int ukPrn)
        {
           return new AppsAdditionalPaymentILRInfo()
            {
                UkPrn = ukPrn,
                Learners = new List<AppsAdditionalPaymentLearnerInfo>()
                {
                    new AppsAdditionalPaymentLearnerInfo()
                    {
                        LearnRefNumber = "A12345",
                        ULN = 12345,
                        LearningDeliveries = new List<AppsAdditionalPaymentLearningDeliveryInfo>()
                        {
                            new AppsAdditionalPaymentLearningDeliveryInfo()
                            {
                                UKPRN = ukPrn,
                                LearnRefNumber = "A12345",
                                LearnAimRef = "50117889",
                                AimType = 3,
                                AimSeqNumber = 1,
                                LearnStartDate = new DateTime(2017, 06, 28),
                                FundModel = 36,
                                ProgType = 1,
                                StdCode = 1,
                                FworkCode = 1,
                                PwayCode = 1
                            }
                        },
                        ProviderSpecLearnerMonitorings =
                            new List<AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo>()
                            {
                                new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                                {
                                    UKPRN = ukPrn,
                                    LearnRefNumber = "1",
                                    ProvSpecLearnMon = "A",
                                    ProvSpecLearnMonOccur = "T180400007"
                                },
                                new AppsAdditionalPaymentProviderSpecLearnerMonitoringInfo()
                                {
                                    UKPRN = ukPrn,
                                    LearnRefNumber = "1",
                                    ProvSpecLearnMon = "B",
                                    ProvSpecLearnMonOccur = "150563"
                                }
                            }
                    }
                }
            };
        }

        private AppsAdditionalPaymentRulebaseInfo BuildFm36Model(int ukPrn)
        {
            return new AppsAdditionalPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>(),
                AECApprenticeshipPriceEpisodePeriodisedValues =
                    new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>()
                    {
                        new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                        {
                            UKPRN = ukPrn,
                            LearnRefNumber = "A12345",
                            AimSeqNumber = 1,
                            PriceEpisodeIdentifier = "1",
                            Periods = new decimal[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 },
                            AttributeName = "PriceEpisodeFirstEmp1618Pay"
                        },
                        new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                        {
                            UKPRN = ukPrn,
                            LearnRefNumber = "A12345",
                            AimSeqNumber = 1,
                            PriceEpisodeIdentifier = "1",
                            Periods = new decimal[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 },
                            AttributeName = "PriceEpisodeSecondProv1618Pay"
                        }
                    }
            };
        }

        private AppsAdditionalPaymentDasPaymentsInfo BuildDasPaymentsModel(int ukPrn)
        {
            return new AppsAdditionalPaymentDasPaymentsInfo()
            {
                UkPrn = ukPrn,
                Payments = new List<DASPaymentInfo>()
                {
                    new DASPaymentInfo()
                    {
                        UkPrn = ukPrn,
                        LearnerReferenceNumber = "A12345",
                        LearningAimReference = "50117889",
                        LearnerUln = 12345,
                        LearningStartDate = new DateTime(2017, 06, 28),
                        LearningAimProgrammeType = 1,
                        LearningAimStandardCode = 1,
                        LearningAimFrameworkCode = 1,
                        LearningAimPathwayCode = 1,
                        FundingSource = 1,
                        TransactionType = 4,
                        AcademicYear = 1819,
                        Amount = 10,
                        ContractType = 2,
                        CollectionPeriod = 1,
                        DeliveryPeriod = 1,
                        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy",
                        TypeOfAdditionalPayment = "Apprentice",
                        EmployerName = "Employer1"
                    },
                    new DASPaymentInfo()
                    {
                        UkPrn = ukPrn,
                        LearnerReferenceNumber = "A12345",
                        LearningAimReference = "50117889",
                        LearnerUln = 12345,
                        LearningStartDate = new DateTime(2017, 06, 28),
                        LearningAimProgrammeType = 1,
                        LearningAimStandardCode = 1,
                        LearningAimFrameworkCode = 1,
                        LearningAimPathwayCode = 1,
                        FundingSource = 1,
                        TransactionType = 6,
                        AcademicYear = 1819,
                        Amount = 10,
                        ContractType = 2,
                        CollectionPeriod = 1,
                        DeliveryPeriod = 1,
                        LearningAimFundingLineType = "16-18 Apprenticeship Non-Levy",
                        TypeOfAdditionalPayment = "Apprentice",
                        EmployerName = "Employer1"
                    }
                }
            };
        }
    }
}