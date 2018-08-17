using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public abstract class BaseLearnersService
    {
        private readonly string _messageKey;
        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _redis;
        private readonly IJsonSerializationService _jsonSerializationService;

        private readonly SemaphoreSlim _getDataLock;

        private bool _loadedDataAlready;

        private List<string> _loadedData;

        protected BaseLearnersService(
            string key,
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Redis)] IKeyValuePersistenceService redis,
            IJsonSerializationService jsonSerializationService)
        {
            _messageKey = key;
            _logger = logger;
            _redis = redis;
            _jsonSerializationService = jsonSerializationService;
            _loadedData = null;
            _getDataLock = new SemaphoreSlim(1, 1);
        }

        public async Task<List<string>> GetLearnersAsync(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _loadedData;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                _loadedDataAlready = true;
                string learnersValidStr = await _redis.GetAsync(jobContextMessage.KeyValuePairs[_messageKey].ToString(), cancellationToken);
                _loadedData = _jsonSerializationService.Deserialize<List<string>>(learnersValidStr);
            }
            catch (Exception ex)
            {
                // Todo: Check behaviour
                _logger.LogError($"Failed to get & deserialise {_messageKey}", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _loadedData;
        }
    }
}
