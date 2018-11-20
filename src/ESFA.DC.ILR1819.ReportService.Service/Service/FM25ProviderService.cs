﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Configuration;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public class FM25ProviderService : IFM25ProviderService
    {
        private readonly ILogger _logger;

        private readonly IKeyValuePersistenceService _redis;
        private readonly IKeyValuePersistenceService _blob;

        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IIntUtilitiesService _intUtilitiesService;

        private readonly DataStoreConfiguration _dataStoreConfiguration;

        private readonly SemaphoreSlim _getDataLock;

        private bool _loadedDataAlready;

        private FM25Global _fundingOutputs;

        public FM25ProviderService(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Redis)] IKeyValuePersistenceService redis,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService blob,
            IJsonSerializationService jsonSerializationService,
            IIntUtilitiesService intUtilitiesService,
            DataStoreConfiguration dataStoreConfiguration)
        {
            _logger = logger;
            _redis = redis;
            _blob = blob;
            _jsonSerializationService = jsonSerializationService;
            _intUtilitiesService = intUtilitiesService;
            _dataStoreConfiguration = dataStoreConfiguration;
            _fundingOutputs = null;
            _getDataLock = new SemaphoreSlim(1, 1);
        }

        public async Task<FM25Global> GetFM25Data(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _fundingOutputs;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                _loadedDataAlready = true;
                string fm25Filename = jobContextMessage.KeyValuePairs[JobContextMessageKey.FundingFm25Output].ToString();
                int ukPrn = _intUtilitiesService.ObjectToInt(jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn]);
                if (await _redis.ContainsAsync(fm25Filename, cancellationToken))
                {
                    string fm25 = await _redis.GetAsync(fm25Filename, cancellationToken);

                    if (string.IsNullOrEmpty(fm25))
                    {
                        _fundingOutputs = null;
                        return _fundingOutputs;
                    }

                    _fundingOutputs = _jsonSerializationService.Deserialize<FM25Global>(fm25);
                }
                else
                {
                    FM25Global fm25Global = new FM25Global();

                    using (var ilrContext = new ILR1819_DataStoreEntities(_dataStoreConfiguration.ILRDataStoreConnectionString))
                    {
                        FM25_Learner[] learners = await ilrContext.FM25_Learner.Where(x => x.UKPRN == ukPrn).Include(x => x.FM25_FM35_Learner_PeriodisedValues).ToArrayAsync(cancellationToken);
                        foreach (FM25_Learner fm25Learner in learners)
                        {
                            List<LearnerPeriodisedValues> learnerPeriodisedValues = new List<LearnerPeriodisedValues>();
                            foreach (FM25_FM35_Learner_PeriodisedValues learnerPeriodisedValue in fm25Learner.FM25_FM35_Learner_PeriodisedValues)
                            {
                                learnerPeriodisedValues.Add(new LearnerPeriodisedValues
                                {
                                    AttributeName = learnerPeriodisedValue.AttributeName,
                                    LearnRefNumber = learnerPeriodisedValue.LearnRefNumber,
                                    Period1 = learnerPeriodisedValue.Period_1,
                                    Period2 = learnerPeriodisedValue.Period_2,
                                    Period3 = learnerPeriodisedValue.Period_3,
                                    Period4 = learnerPeriodisedValue.Period_4,
                                    Period5 = learnerPeriodisedValue.Period_5,
                                    Period6 = learnerPeriodisedValue.Period_6,
                                    Period7 = learnerPeriodisedValue.Period_7,
                                    Period8 = learnerPeriodisedValue.Period_8,
                                    Period9 = learnerPeriodisedValue.Period_9,
                                    Period10 = learnerPeriodisedValue.Period_10,
                                    Period11 = learnerPeriodisedValue.Period_11,
                                    Period12 = learnerPeriodisedValue.Period_12
                                });
                            }

                            fm25Global.Learners.Add(new FM25Learner
                            {
                                FundLine = fm25Learner.FundLine,
                                LearnerPeriodisedValues = learnerPeriodisedValues
                            });
                        }
                    }

                    _fundingOutputs = fm25Global;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get & deserialise FM25 funding data", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _fundingOutputs;
        }
    }
}
