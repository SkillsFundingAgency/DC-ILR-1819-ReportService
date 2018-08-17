using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Model.Configuration;
using ESFA.DC.ILR1819.ReportService.Stateless.Configuration;
using ESFA.DC.ServiceFabric.Helpers.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Tests.AutoFac
{
    public sealed class TestConfigurationHelper : IConfigurationHelper
    {
        public T GetSectionValues<T>(string sectionName)
        {
            switch (sectionName)
            {
                case "RedisSection":
                    return (T)(object)new RedisOptions();
                case "LarsSection":
                    return (T)(object)new LarsConfiguration();
                case "TopicAndTaskSection":
                    return (T)GetTopicsAndTasks();
                case "AzureStorageSection":
                    return (T)(object)new AzureStorageOptions()
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
                    return (T)(object)new VersionInfo()
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
                    return (T)(object)new CollectionsManagementConfiguration()
                    {
                        CollectionsManagementConnectionString = "Server=.;Database=myDataBase;User Id=myUsername;Password = myPassword;"
                    };
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
                TopicDeds = "TopicDeds",
                TopicDeds_TaskPersistDataToDeds = "TopicDeds_TaskPersistDataToDeds",
                TopicFunding = "TopicFunding",
                TopicReports = "TopicReports",
                TopicReports_TaskGenerateMainOccupancyReport = "TopicReports_TaskGenerateMainOccupancyReport",
                TopicReports_TaskGenerateSummaryOfFunding1619Report = "TopicReports_TaskGenerateSummaryOfFunding1619Report",
                TopicValidation = "TopicValidation",
                TopicReports_TaskGenerateMathsAndEnglishReport = "TopicReports_TaskGenerateMathsAndEnglishReport"
            };
        }
    }
}
