﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.Model;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public sealed class IlrProviderService : IIlrProviderService
    {
        private readonly ILogger _logger;

        private readonly IStreamableKeyValuePersistenceService _storage;

        private readonly IXmlSerializationService _xmlSerializationService;

        private readonly SemaphoreSlim _getIlrLock;

        private Message _message;

        public IlrProviderService(
            ILogger logger,
            IStreamableKeyValuePersistenceService storage,
            IXmlSerializationService xmlSerializationService)
        {
            _logger = logger;
            _storage = storage;
            _xmlSerializationService = xmlSerializationService;
            _message = null;
            _getIlrLock = new SemaphoreSlim(1, 1);
        }

        public async Task<IMessage> GetIlrFile(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            await _getIlrLock.WaitAsync(cancellationToken);

            try
            {
                if (_message != null)
                {
                    return _message;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    await _storage.GetAsync(
                            jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename].ToString(),
                            ms,
                            cancellationToken);
                    ms.Seek(0, SeekOrigin.Begin);
                    _message = _xmlSerializationService.Deserialize<Message>(ms);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get and deserialise ILR from storage, key: {JobContextMessageKey.Filename}", ex);
            }
            finally
            {
                _getIlrLock.Release();
            }

            return _message;
        }
    }
}
