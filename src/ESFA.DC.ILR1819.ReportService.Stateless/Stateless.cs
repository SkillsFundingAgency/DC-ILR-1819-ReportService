using System.Collections.Generic;
using System.Fabric;
using Autofac;
using ESFA.DC.JobContext;
using ESFA.DC.JobContextManager.Interface;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ESFA.DC.ILR1819.ReportService.Stateless
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public class Stateless : StatelessService
    {
        private readonly ILifetimeScope _parentLifetimeScope;

        public Stateless(StatelessServiceContext context, ILifetimeScope parentLifetimeScope)
            : base(context)
        {
            _parentLifetimeScope = parentLifetimeScope;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(
                context => _parentLifetimeScope.Resolve<IJobContextManager<JobContextMessage>>(),
                "ReportService-SBTopicListener");
        }
    }
}
