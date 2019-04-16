using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public class FM36ProviderService : IFM36ProviderService
    {
        private readonly ILogger _logger;

        private readonly IKeyValuePersistenceService _storage;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IIntUtilitiesService _intUtilitiesService;
        private readonly Func<IIlr1819RulebaseContext> _ilrRulebaseContextFactory;
        private readonly SemaphoreSlim _getDataLock;
        private bool _loadedDataAlready;
        private FM36Global _fundingOutputs;

        public FM36ProviderService(
            ILogger logger,
            IKeyValuePersistenceService storage,
            IJsonSerializationService jsonSerializationService,
            IIntUtilitiesService intUtilitiesService,
            Func<IIlr1819RulebaseContext> ilrRulebaseContextFactory)
        {
            _logger = logger;
            _storage = storage;
            _jsonSerializationService = jsonSerializationService;
            _intUtilitiesService = intUtilitiesService;
            _ilrRulebaseContextFactory = ilrRulebaseContextFactory;
            _fundingOutputs = null;
            _getDataLock = new SemaphoreSlim(1, 1);
        }

        public async Task<FM36Global> GetFM36Data(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
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
                    string fm36Filename = reportServiceContext.FundingFM36OutputKey;
                    string fm36 = await _storage.GetAsync(fm36Filename, cancellationToken);

                    if (string.IsNullOrEmpty(fm36))
                    {
                        _fundingOutputs = null;
                        return _fundingOutputs;
                    }

                    // await _blob.SaveAsync($"{jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn]}_{jobContextMessage.JobId.ToString()}_Fm36.json", fm36, cancellationToken);
                    _fundingOutputs = _jsonSerializationService.Deserialize<FM36Global>(fm36);
                }
                else
                {
                    FM36Global fm36Global = new FM36Global();
                    using (var ilrContext = _ilrRulebaseContextFactory())
                    {
                        var fm36GlobalDb = await ilrContext.AEC_globals.FirstOrDefaultAsync(x => x.UKPRN == ukPrn, cancellationToken);
                        //AEC_LearningDelivery[] res = await ilrContext.AEC_LearningDelivery_Period.Where(x => x.UKPRN == ukPrn).Select(x => x.AEC_LearningDelivery)
                        //    .Include(x => x.AEC_LearningDelivery_PeriodisedValues).ToArrayAsync(cancellationToken);

                        AEC_LearningDelivery[] res = await ilrContext.AEC_LearningDeliveries.Where(x => x.UKPRN == ukPrn)
                            .Include(x => x.AEC_LearningDelivery_PeriodisedValues).ToArrayAsync(cancellationToken);

                        IGrouping<string, AEC_LearningDelivery>[] learners = res.GroupBy(x => x.LearnRefNumber).ToArray();

                        fm36Global.Learners = new List<FM36Learner>();

                        foreach (IGrouping<string, AEC_LearningDelivery> albLearningDeliveries in learners)
                        {
                            var learningDeliveryDto = new List<ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery>();
                            foreach (var ld in albLearningDeliveries)
                            {
                                var ldPeriodisedValues = ld.AEC_LearningDelivery_PeriodisedValues.Select(ldpv => new LearningDeliveryPeriodisedValues()
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
                                    LearningDeliveryValues = new LearningDeliveryValues()
                                    {
                                        LearnDelInitialFundLineType = ld.LearnDelInitialFundLineType // todo : rest of the properties
                                    }
                                });
                            }

                            FM36Learner learner = new FM36Learner()
                            {
                                LearnRefNumber = albLearningDeliveries.Key,
                                LearningDeliveries = learningDeliveryDto
                            };

                            fm36Global.Learners.Add(learner);
                        }

                        if (fm36GlobalDb != null)
                        {
                            fm36Global.LARSVersion = fm36GlobalDb.LARSVersion;
                            fm36Global.RulebaseVersion = fm36GlobalDb.RulebaseVersion;
                            fm36Global.UKPRN = fm36GlobalDb.UKPRN;
                        }
                    }

                    _fundingOutputs = fm36Global;
                }
            }
            finally
            {
                _getDataLock.Release();
            }

            return _fundingOutputs;
        }

        public async Task<AppsAdditionalPaymentRulebaseInfo> GetFM36DataForAppsAdditionalPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsAdditionalPaymentRulebaseInfo = new AppsAdditionalPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodePeriodisedValues = new List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>(),
                AECLearningDeliveries = new List<AECLearningDeliveryInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<AEC_ApprenticeshipPriceEpisode> aecApprenticeshipPriceEpisodes;
            List<AEC_LearningDelivery> aecLearningDeliveries;

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                aecApprenticeshipPriceEpisodes = await ilrContext.AEC_ApprenticeshipPriceEpisodes.Include(x => x.AEC_ApprenticeshipPriceEpisode_PeriodisedValues).Where(x => x.UKPRN == ukPrn).ToListAsync(cancellationToken);
                aecLearningDeliveries = await ilrContext.AEC_LearningDeliveries.Where(x => x.UKPRN == ukPrn).ToListAsync(cancellationToken);
            }

            foreach (var aecApprenticeshipPriceEpisode in aecApprenticeshipPriceEpisodes)
            {
                foreach (var aecApprenticeshipPriceEpisodePeriodisedValue in aecApprenticeshipPriceEpisode
                    .AEC_ApprenticeshipPriceEpisode_PeriodisedValues)
                {
                    var periodisedValue = new AECApprenticeshipPriceEpisodePeriodisedValuesInfo()
                    {
                        UKPRN = ukPrn,
                        LearnRefNumber = aecApprenticeshipPriceEpisodePeriodisedValue.LearnRefNumber,
                        AimSeqNumber = (int)aecApprenticeshipPriceEpisode.PriceEpisodeAimSeqNumber,
                        AttributeName = aecApprenticeshipPriceEpisodePeriodisedValue.AttributeName,
                        Periods = new[]
                        {
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_1.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_2.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_3.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_4.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_5.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_6.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_7.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_8.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_9.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_10.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_11.GetValueOrDefault(),
                            aecApprenticeshipPriceEpisodePeriodisedValue.Period_12.GetValueOrDefault(),
                        }
                    };
                    appsAdditionalPaymentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Add(periodisedValue);
                }
            }

            foreach (var aecLearningDelivery in aecLearningDeliveries)
            {
                var aecLearningDeliveryInfo = new AECLearningDeliveryInfo()
                {
                    UKPRN = ukPrn,
                    LearnRefNumber = aecLearningDelivery.LearnRefNumber,
                    AimSeqNumber = aecLearningDelivery.AimSeqNumber,
                    LearnDelEmpIdFirstAdditionalPaymentThreshold = aecLearningDelivery.LearnDelEmpIdFirstAdditionalPaymentThreshold,
                    LearnDelEmpIdSecondAdditionalPaymentThreshold = aecLearningDelivery.LearnDelEmpIdSecondAdditionalPaymentThreshold
                };

                appsAdditionalPaymentRulebaseInfo.AECLearningDeliveries.Add(aecLearningDeliveryInfo);
            }

            return appsAdditionalPaymentRulebaseInfo;
        }

        public async Task<AppsMonthlyPaymentRulebaseInfo> GetFM36DataForAppsMonthlyPaymentReportAsync(int ukPrn, CancellationToken cancellationToken)
        {
            var appsMonthlyPaymentRulebaseInfo = new AppsMonthlyPaymentRulebaseInfo()
            {
                UkPrn = ukPrn,
                AECApprenticeshipPriceEpisodes = new List<AECApprenticeshipPriceEpisodeInfo>()
            };

            cancellationToken.ThrowIfCancellationRequested();

            List<AEC_ApprenticeshipPriceEpisode> aecApprenticeshipPriceEpisodes;

            using (var ilrContext = _ilrRulebaseContextFactory())
            {
                aecApprenticeshipPriceEpisodes = await ilrContext.AEC_ApprenticeshipPriceEpisodes.Where(x => x.UKPRN == ukPrn).ToListAsync(cancellationToken);
            }

            foreach (var aecApprenticeshipPriceEpisode in aecApprenticeshipPriceEpisodes)
            {
                var periodisedValue = new AECApprenticeshipPriceEpisodeInfo()
                {
                    LearnRefNumber = aecApprenticeshipPriceEpisode.LearnRefNumber,
                    PriceEpisodeActualEndDate = aecApprenticeshipPriceEpisode.PriceEpisodeActualEndDate,
                    PriceEpisodeAgreeId = aecApprenticeshipPriceEpisode.PriceEpisodeAgreeId
                };
                appsMonthlyPaymentRulebaseInfo.AECApprenticeshipPriceEpisodes.Add(periodisedValue);
            }

            return appsMonthlyPaymentRulebaseInfo;
        }
    }
}