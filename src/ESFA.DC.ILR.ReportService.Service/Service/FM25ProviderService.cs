using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public class FM25ProviderService : IFM25ProviderService
    {
        private readonly ILogger _logger;

        private readonly IKeyValuePersistenceService _storage;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IIntUtilitiesService _intUtilitiesService;
        private readonly Func<IIlr1819RulebaseContext> _ilrRulebaseContextFactory;
        private readonly SemaphoreSlim _getDataLock;
        private bool _loadedDataAlready;
        private FM25Global _fundingOutputs;

        public FM25ProviderService(
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

        public async Task<FM25Global> GetFM25Data(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
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
                    string fm25Filename = reportServiceContext.FundingFM25OutputKey;
                    string fm25 = await _storage.GetAsync(fm25Filename, cancellationToken);

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

                    using (var ilrContext = _ilrRulebaseContextFactory())
                    {
                        var fm25GlobalDb = await ilrContext.FM25_globals.FirstOrDefaultAsync(x => x.UKPRN == ukPrn, cancellationToken);
                        FM25_Learner[] learners = await ilrContext.FM25_Learners.Where(x => x.UKPRN == ukPrn).Include(x => x.FM25_FM35_Learner_PeriodisedValues).ToArrayAsync(cancellationToken);
                        fm25Global.Learners = new List<FM25Learner>();

                        foreach (FM25_Learner fm25Learner in learners)
                        {
                            List<LearnerPeriodisedValues> learnerPeriodisedValues = new List<LearnerPeriodisedValues>();
                            foreach (var learnerPeriodisedValue in fm25Learner.FM25_FM35_Learner_PeriodisedValues)
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

                        if (fm25GlobalDb != null)
                        {
                            fm25Global.LARSVersion = fm25GlobalDb.LARSVersion;
                            fm25Global.OrgVersion = fm25GlobalDb.OrgVersion;
                            fm25Global.PostcodeDisadvantageVersion = fm25GlobalDb.PostcodeDisadvantageVersion;
                            fm25Global.RulebaseVersion = fm25GlobalDb.RulebaseVersion;
                            fm25Global.UKPRN = fm25GlobalDb.UKPRN;
                        }
                    }

                    _fundingOutputs = fm25Global;
                }
            }
            finally
            {
                _getDataLock.Release();
            }

            return _fundingOutputs;
        }
    }
}