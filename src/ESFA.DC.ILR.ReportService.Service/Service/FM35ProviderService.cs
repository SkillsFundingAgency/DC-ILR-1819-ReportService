using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ILR;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public class FM35ProviderService : IFM35ProviderService
    {
        private readonly ILogger _logger;
        private readonly IStreamableKeyValuePersistenceService _storage;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IIntUtilitiesService _intUtilitiesService;
        private readonly Func<IIlr1819ValidContext> _ilrValidContextFactory;
        private readonly Func<IIlr1819RulebaseContext> _ilrRulebaseContextFactory;
        private readonly SemaphoreSlim _getDataLock;
        private bool _loadedDataAlready;
        private FM35Global _fundingOutputs;

        public FM35ProviderService(
            ILogger logger,
            IStreamableKeyValuePersistenceService storage,
            IJsonSerializationService jsonSerializationService,
            IIntUtilitiesService intUtilitiesService,
            Func<IIlr1819ValidContext> ilrValidContextFactory,
            Func<IIlr1819RulebaseContext> ilrRulebaseContextFactory)
        {
            _logger = logger;
            _storage = storage;
            _jsonSerializationService = jsonSerializationService;
            _intUtilitiesService = intUtilitiesService;
            _ilrValidContextFactory = ilrValidContextFactory;
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
            _fundingOutputs = null;
            _getDataLock = new SemaphoreSlim(1, 1);
        }

        public async Task<FM35Global> GetFM35Data(
            IReportServiceContext reportServiceContext,
            CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _fundingOutputs;
                }

                cancellationToken.ThrowIfCancellationRequested();

                _loadedDataAlready = true;
                int ukPrn = reportServiceContext.Ukprn;

                if (string.Equals(reportServiceContext.CollectionName, "ILR1819", StringComparison.OrdinalIgnoreCase))
                {
                    string fm35Filename = reportServiceContext.FundingFM35OutputKey;
                    _logger.LogWarning($"Reading {fm35Filename}; Storage is {_storage}; CancellationToken is {cancellationToken}");
                    if (await _storage.ContainsAsync(fm35Filename, cancellationToken))
                    {
                        _logger.LogWarning($"Available {fm35Filename}");
                        using (MemoryStream ms = new MemoryStream())
                        {
                            await _storage.GetAsync(fm35Filename, ms, cancellationToken);
                            _logger.LogWarning($"Deserialising {fm35Filename} with {ms.Length}");
                            _fundingOutputs = _jsonSerializationService.Deserialize<FM35Global>(ms);
                        }
                    }

                    _logger.LogWarning($"Finished {fm35Filename}");
                }
                else
                {
                    FM35Global fm35Global = new FM35Global();
                    using (var ilrContext = _ilrRulebaseContextFactory())
                    {
                        var fm35GlobalDb = await ilrContext.FM35_globals.FirstOrDefaultAsync(x => x.UKPRN == ukPrn, cancellationToken);
                        FM35_LearningDelivery[] res = await ilrContext.FM35_LearningDeliveries.Where(x => x.UKPRN == ukPrn)
                            .Include(x => x.FM35_LearningDelivery_PeriodisedValues).ToArrayAsync(cancellationToken);

                        IGrouping<string, FM35_LearningDelivery>[] learners = res.GroupBy(x => x.LearnRefNumber).ToArray();

                        fm35Global.Learners = new List<FM35Learner>();

                        foreach (IGrouping<string, FM35_LearningDelivery> fm35LearningDeliveries in learners)
                        {
                            var learningDeliveryDto = new List<LearningDelivery>();
                            foreach (var ld in fm35LearningDeliveries)
                            {
                                var ldPeriodisedValues = ld.FM35_LearningDelivery_PeriodisedValues.Select(ldpv => new LearningDeliveryPeriodisedValue()
                                {
                                    AttributeName = ldpv.AttributeName,
                                    Period1 = ldpv.Period_1,
                                    Period2 = ldpv.Period_2,
                                    Period3 = ldpv.Period_3,
                                    Period4 = ldpv.Period_4,
                                    Period5 = ldpv.Period_5,
                                    Period6 = ldpv.Period_6,
                                    Period7 = ldpv.Period_7,
                                    Period8 = ldpv.Period_8,
                                    Period9 = ldpv.Period_9,
                                    Period10 = ldpv.Period_10,
                                    Period11 = ldpv.Period_11,
                                    Period12 = ldpv.Period_12
                                }).ToList();

                                learningDeliveryDto.Add(new LearningDelivery()
                                {
                                    AimSeqNumber = ld.AimSeqNumber,
                                    LearningDeliveryPeriodisedValues = ldPeriodisedValues,
                                    LearningDeliveryValue = new LearningDeliveryValue()
                                    {
                                        FundLine = ld.FundLine,
                                        AchApplicDate = ld.AchApplicDate // todo: finish the entire LearningDeliveryValue here
                                    }
                                });
                            }

                            FM35Learner learner = new FM35Learner()
                            {
                                LearnRefNumber = fm35LearningDeliveries.Key,
                                LearningDeliveries = learningDeliveryDto
                            };

                            fm35Global.Learners.Add(learner);
                        }

                        if (fm35GlobalDb != null)
                        {
                            fm35Global.LARSVersion = fm35GlobalDb.LARSVersion;
                            fm35Global.OrgVersion = fm35GlobalDb.OrgVersion;
                            fm35Global.PostcodeDisadvantageVersion = fm35GlobalDb.PostcodeDisadvantageVersion;
                            fm35Global.RulebaseVersion = fm35GlobalDb.RulebaseVersion;
                            fm35Global.UKPRN = fm35GlobalDb.UKPRN;
                        }
                    }

                    _fundingOutputs = fm35Global;
                }
            }
            finally
            {
                _getDataLock.Release();
            }

            return _fundingOutputs;
        }

        public async Task<List<FM35LearningDeliveryValues>> GetFM35DataFromDataStore(
            IReportServiceContext reportServiceContext,
            CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);
            var fm35LearningDeliveryPeriodisedValues = new List<FM35LearningDeliveryValues>();
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                var UkPrn = reportServiceContext.Ukprn;

                using (var ilrContext = _ilrRulebaseContextFactory())
                {
                    fm35LearningDeliveryPeriodisedValues = (from pv in ilrContext.FM35_LearningDelivery_PeriodisedValues
                                                            join ld in ilrContext.FM35_LearningDeliveries
                                                                on new { pv.LearnRefNumber, pv.AimSeqNumber, pv.UKPRN } equals new { ld.LearnRefNumber, ld.AimSeqNumber, ld.UKPRN }
                                                            where pv.UKPRN == UkPrn
                                                            select new FM35LearningDeliveryValues()
                                                            {
                                                                AttributeName = pv.AttributeName,
                                                                FundLine = ld.FundLine,
                                                                UKPRN = pv.UKPRN,
                                                                LearnRefNumber = pv.LearnRefNumber,
                                                                AimSeqNumber = pv.AimSeqNumber,
                                                                Period1 = pv.Period_1,
                                                                Period2 = pv.Period_2,
                                                                Period3 = pv.Period_3,
                                                                Period4 = pv.Period_4,
                                                                Period5 = pv.Period_5,
                                                                Period6 = pv.Period_6,
                                                                Period7 = pv.Period_7,
                                                                Period8 = pv.Period_8,
                                                                Period9 = pv.Period_9,
                                                                Period10 = pv.Period_10,
                                                                Period11 = pv.Period_11,
                                                                Period12 = pv.Period_12,
                                                            }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get FM35 Learning Delivery Periodised Values", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return fm35LearningDeliveryPeriodisedValues.ToList();
        }

        private List<string> GetValidLearners(int ukPrn)
        {
            List<string> learners;
            using (var ilrContext = _ilrValidContextFactory())
            {
                learners = ilrContext.Learners.Where(x => x.UKPRN == ukPrn).Select(x => x.LearnRefNumber).ToList();
            }

            return learners;
        }
    }
}