﻿using ESFA.DC.Queueing;

namespace ESFA.DC.ILR1819.ReportService.Stateless.Configuration
{
    public sealed class JobStatusQueueConfig : QueueConfiguration
    {
        public JobStatusQueueConfig(string connectionString, string queueName, int maxConcurrentCalls, int minimumBackoffSeconds = 5, int maximumBackoffSeconds = 50, int maximumRetryCount = 10)
            : base(connectionString, queueName, maxConcurrentCalls, minimumBackoffSeconds, maximumBackoffSeconds, maximumRetryCount)
        {
        }
    }
}
