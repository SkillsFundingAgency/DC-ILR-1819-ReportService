using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR1819.ReportService.Stateless.Configuration;
using ESFA.DC.ServiceFabric.Helpers.Interfaces;

namespace ESFA.DC.ILR.ReportService.Tests.AutoFac
{
    public sealed class TestConfigurationHelper : IConfigurationHelper
    {
        public T GetSectionValues<T>(string sectionName)
        {
            switch (sectionName)
            {
                case "LarsSection":
                    return (T)(object)new LarsConfiguration();
                case "TopicAndTaskSection":
                    return (T)GetTopicsAndTasks();
                case "AzureStorageSection":
                    return (T)(object)new AzureStorageOptions
                    {
                        AzureBlobConnectionString = "AzureBlobConnectionString",
                        AzureBlobContainerName = "AzureBlobContainerName"
                    };
                case "ServiceBusSettings":
                    return (T)(object)new ServiceBusOptions()
                    {
                        AuditQueueName = "AuditQueueName",
                        ReportingSubscriptionName = "ReportingSubscriptionName",
                        ServiceBusConnectionString = "ServiceBusConnectionString",
                        TopicName = "TopicName"
                    };
                case "VersionSection":
                    return (T)(object)new VersionInfo
                    {
                        ServiceReleaseVersion = "ServiceReleaseVersion"
                    };
                case "LoggerSection":
                    return (T)(object)new LoggerOptions
                    {
                        LoggerConnectionstring = "Server=.;Database=myDataBase;User Id=myUsername;Password = myPassword;"
                    };
                case "JobStatusSection":
                    return (T)(object)new JobStatusQueueOptions();
                case "OrgSection":
                    return (T)(object)new OrgConfiguration();
                case "CollectionsManagementSection":
                    return (T)(object)new CollectionsManagementConfiguration
                    {
                        CollectionsManagementConnectionString = "Server=.;Database=myDataBase;User Id=myUsername;Password = myPassword;"
                    };
                case "DasCommitmentsSection":
                    return (T)(object)new DasCommitmentsConfiguration
                    {
                        DasCommitmentsConnectionString =
                            "Server=.;Database=myDataBase;User Id=myUsername;Password = myPassword;"
                    };
                case "EasSection":
                    return (T)(object)new EasConfiguration()
                    {
                        EasConnectionString = "data source=(local);initial catalog=Easdb;integrated security=True;multipleactiveresultsets=True;Connect Timeout=90"
                    };
                case "LargeEmployerSection":
                    return (T)(object)new LargeEmployerConfiguration
                    {
                        LargeEmployerConnectionString =
                            "Server=.;Database=myDataBase;User Id=myUsername;Password = myPassword;"
                    };
                case "DASPaymentsSection":
                    return (T)(object)new DASPaymentsConfiguration
                    {
                        DASPaymentsConnectionString =
                            "Server=.;Database=DASPayments;integrated security=True;"
                    };
                case "PostcodeSection":
                    return (T)(object)new PostcodeConfiguration
                    {
                        PostcodeConnectionString =
                            "Server=.;Database=myDataBase;User Id=myUsername;Password = myPassword;"
                    };
                case "DataStoreSection":
                    return (T)(object)new DataStoreConfiguration()
                    {
                        ILRDataStoreConnectionString =
                            "Server=.;Database=ilr1819_DataStore;integrated security=True;",
                        ILRDataStoreValidConnectionString =
                            "Server=.;Database=ilr1819_DataStore;integrated security=True;"
                    };
                case "IlrValidationErrorsSection":
                    return (T)(object)new IlrValidationErrorsConfiguration();
            }

            return default(T);
        }

        public static ITopicAndTaskSectionOptions GetTopicsAndTasks()
        {
            return new TopicAndTaskSectionOptions()
            {
                TopicReports_TaskGenerateAllbOccupancyReport = "TopicReports_TaskGenerateAllbOccupancyReport",
                TopicReports_TaskGenerateValidationReport = "TopicReports_TaskGenerateValidationReport",
                TopicReports_TaskGenerateFundingSummaryReport = "TopicReports_TaskGenerateFundingSummaryReport",
                TopicReports_TaskGenerateAdultFundingClaimReport = "TopicReports_TaskGenerateAdultFundingClaimReport",
                TopicDeds = "TopicDeds",
                TopicDeds_TaskPersistDataToDeds = "TopicDeds_TaskPersistDataToDeds",
                TopicFunding = "TopicFunding",
                TopicReports = "TopicReports",
                TopicReports_TaskGenerateMainOccupancyReport = "TopicReports_TaskGenerateMainOccupancyReport",
                TopicReports_TaskGenerateSummaryOfFunding1619Report = "TopicReports_TaskGenerateSummaryOfFunding1619Report",
                TopicValidation = "TopicValidation",
                TopicReports_TaskGenerateMathsAndEnglishReport = "TopicReports_TaskGenerateMathsAndEnglishReport",
                TopicReports_TaskGenerateAppsAdditionalPaymentsReport = "TopicReports_TaskGenerateAppsAdditionalPaymentsReport",
                TopicReports_TaskGenerateAppsIndicativeEarningsReport = "TopicReports_TaskGenerateAppsIndicativeEarningsReport",
                TopicReports_TaskGenerateAppsCoInvestmentContributionsReport = "TopicReports_TaskGenerateAppsCoInvestmentContributionsReport",
                TopicReports_TaskGenerateAppsMonthlyPaymentReport = "TopicReports_TaskGenerateAppsMonthlyPaymentReport",
                TopicReports_TaskGenerateDataMatchReport = "TopicReports_TaskGenerateDataMatchReport",
                TopicReports_TaskGenerateTrailblazerEmployerIncentivesReport = "TopicReports_TaskGenerateTrailblazerEmployerIncentivesReport",
                TopicReports_TaskGenerateTrailblazerAppsOccupancyReport = "TopicReports_TaskGenerateTrailblazerAppsOccupancyReport",
                TopicReports_TaskGenerateFundingClaim1619Report = "TopicReports_TaskGenerateFundingClaim1619Report",
                TopicReports_TaskGenerateHNSReport = "TopicReports_TaskGenerateHNSReport"
            };
        }
    }
}