using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.ILR1819.ReportService.Stateless;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Interface;
using ESFA.DC.JobContextManager.Model;
using ESFA.DC.JobContextManager.Model.Interface;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.AutoFac
{
    public sealed class AutoFacTest
    {
        [Fact]
        public async Task TestRegistrations()
        {
            try
            {
                JobContextMessage jobContextMessage =
                    new JobContextMessage(
                        1,
                        new ITopicItem[] { new TopicItem("SubscriptionName", new List<ITaskItem>()) },
                        0,
                        DateTime.UtcNow);
                jobContextMessage.KeyValuePairs[JobContextMessageKey.Container] = "Container";
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Cancel();

                ContainerBuilder containerBuilder = DIComposition.BuildContainer(new TestConfigurationHelper());

                IContainer c = containerBuilder.Build();
                using (var lifeTime = c.BeginLifetimeScope())
                {
                    var messageHandler = lifeTime.Resolve<IMessageHandler<JobContextMessage>>();
                    bool ret = await messageHandler.HandleAsync(jobContextMessage, cts.Token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Assert.Null(ex);
            }
        }
    }
}
