using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public sealed class InvalidLearnersService : BaseLearnersService, IInvalidLearnersService
    {
        public InvalidLearnersService(
            ILogger logger,
            IKeyValuePersistenceService storage,
            IJsonSerializationService jsonSerializationService,
            DataStoreConfiguration dataStoreConfiguration)
        : base(JobContextMessageKey.InvalidLearnRefNumbers, logger, storage, jsonSerializationService, dataStoreConfiguration)
        {
        }
    }
}
