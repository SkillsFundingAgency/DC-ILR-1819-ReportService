using Autofac.Features.AttributeFilters;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public sealed class ValidLearnersService : BaseLearnersService, IValidLearnersService
    {
        public ValidLearnersService(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Redis)] IKeyValuePersistenceService redis,
            IJsonSerializationService jsonSerializationService)
        : base(JobContextMessageKey.ValidLearnRefNumbers, logger, redis, jsonSerializationService)
        {
        }
    }
}
