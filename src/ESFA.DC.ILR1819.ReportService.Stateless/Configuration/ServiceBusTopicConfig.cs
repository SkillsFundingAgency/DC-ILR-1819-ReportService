using ESFA.DC.Queueing;

namespace ESFA.DC.ILR1819.ReportService.Stateless.Configuration
{
    public class ServiceBusTopicConfig : TopicConfiguration
    {
        public ServiceBusTopicConfig(string connectionString, string topicName, string subscriptionName, int maxConcurrentCalls, int minimumBackoffSeconds = 5, int maximumBackoffSeconds = 50, int maximumRetryCount = 10)
            : base(connectionString, topicName, subscriptionName, maxConcurrentCalls, minimumBackoffSeconds, maximumBackoffSeconds, maximumRetryCount)
        {
        }
    }
}
