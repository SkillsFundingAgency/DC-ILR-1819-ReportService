using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Autofac;
using Autofac.Integration.ServiceFabric;
using ESFA.DC.ILR1819.ReportService.Stateless.Configuration;
using ESFA.DC.ServiceFabric.Helpers;
using ESFA.DC.ServiceFabric.Helpers.Interfaces;

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
                IConfigurationHelper configHelper = new ConfigurationHelper();

                // License Aspose.Cells
                SoftwareLicenceSection softwareLicenceSection = configHelper.GetSectionValues<SoftwareLicenceSection>(nameof(SoftwareLicenceSection));
                if (!string.IsNullOrEmpty(softwareLicenceSection.AsposeLicence))
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(softwareLicenceSection.AsposeLicence.Replace("&lt;", "<").Replace("&gt;", ">"))))
                    {
                        new Aspose.Cells.License().SetLicense(ms);
                    }
                }

                // Setup Autofac
                ContainerBuilder builder = DIComposition.BuildContainer(configHelper);

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
