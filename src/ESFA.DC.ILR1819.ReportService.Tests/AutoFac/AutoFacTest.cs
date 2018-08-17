using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using ESFA.DC.ILR1819.ReportService.Stateless;
using ESFA.DC.ILR1819.ReportService.Stateless.Handlers;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using Xunit;

namespace ESFA.DC.ILR1819.ReportService.Tests.AutoFac
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
                        new ITopicItem[] { new TopicItem("SubscriptionName", "SubscriptionSqlFilterValue", new List<ITaskItem>()) },
                        0,
                        DateTime.UtcNow);
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.Cancel();

                ContainerBuilder containerBuilder = DIComposition.BuildContainer(new TestConfigurationHelper());

                IContainer c = containerBuilder.Build();
                using (var lifeTime = c.BeginLifetimeScope())
                {
                    var messageHandler = lifeTime.Resolve<IMessageHandler>();
                    bool ret = await messageHandler.Handle(jobContextMessage, cts.Token);
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
