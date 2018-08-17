using System;
using System.Diagnostics;
using System.Threading;
using Autofac.Integration.ServiceFabric;
using ESFA.DC.ServiceFabric.Helpers;

namespace ESFA.DC.ILR1819.ReportService.Stateless
{
    public static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        public static void Main()
        {
            try
            {
                var builder = DIComposition.BuildContainer(new ConfigurationHelper());

                // Register the Autofac magic for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register the stateless service.
                builder.RegisterStatelessService<Stateless>("ESFA.DC.ILR1819.ReportService.StatelessType");

                using (var container = builder.Build())
                {
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(Stateless).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e + Environment.NewLine + (e.InnerException?.ToString() ?? "No inner exception"));
                throw;
            }
        }
    }
}
