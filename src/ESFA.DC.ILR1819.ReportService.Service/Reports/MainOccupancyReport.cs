using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Attribute;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Model;
using ESFA.DC.ILR1819.ReportService.Interface.Reports;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;
using ESFA.DC.ILR1819.ReportService.Service.Mapper;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Reports
{
    public sealed class MainOccupancyReport : AbstractReportBuilder, IReport
    {
        private const string Fm25OnProgPayment = "OnProgPayment";

        private const string Fm35OnProgPayment = "OnProgPayment";

        private const string Fm35BalPayment = "BalancePayment";

        private const string Fm35EmpOutcomePay = "EmpOutcomePay";

        private const string Fm35AchievePayment = "AchievePayment";

        private const string Fm35LearnSuppFundCash = "LearnSuppFundCash";

        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IAllbProviderService _allbProviderService;
        private readonly ILarsProviderService _larsProviderService;

        public MainOccupancyReport(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService storage,
            IIlrProviderService ilrProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IValidLearnersService validLearnersService,
            IAllbProviderService allbProviderService,
            ILarsProviderService larsProviderService,
            IDateTimeProvider dateTimeProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider)
        {
            _logger = logger;
            _storage = storage;
            _ilrProviderService = ilrProviderService;
            _stringUtilitiesService = stringUtilitiesService;
            _validLearnersService = validLearnersService;
            _allbProviderService = allbProviderService;
            _larsProviderService = larsProviderService;

            ReportFileName = "Main Occupancy Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateMainOccupancyReport;
        }

        public async Task GenerateReport(IJobContextMessage jobContextMessage, ZipArchive archive, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(jobContextMessage, cancellationToken);
            Task<FundingOutputs> albDataTask = _allbProviderService.GetAllbData(jobContextMessage, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(jobContextMessage, cancellationToken);

            await Task.WhenAll(ilrFileTask, albDataTask, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<ILearner> learners =
                ilrFileTask.Result?.Learners?.Where(x => validLearnersTask.Result.Contains(x.LearnRefNumber)).ToList();
            if (learners == null)
            {
                _logger.LogWarning("Failed to get learners for Main Occupancy Report");
                return;
            }

            List<string> learnAimRefs = learners.SelectMany(x => x.LearningDeliveries).Select(x => x.LearnAimRef).ToList();

            Task<Dictionary<string, ILarsLearningDelivery>> larsLearningDeliveriesTask = _larsProviderService.GetLearningDeliveries(learnAimRefs, cancellationToken);
            Task<Dictionary<string, ILarsFrameworkAim>> larsFrameworkAimsTask = _larsProviderService.GetFrameworkAims(learnAimRefs, cancellationToken);

            await Task.WhenAll(larsLearningDeliveriesTask, larsFrameworkAimsTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<string> larsError = new List<string>();
            List<string> albLearnerError = new List<string>();

            List<MainOccupancyFM25Model> mainOccupancyFM25Models = new List<MainOccupancyFM25Model>(learnAimRefs.Count);
            List<MainOccupancyFM35Model> mainOccupancyFM35Models = new List<MainOccupancyFM35Model>(learnAimRefs.Count);
            foreach (var learner in learners)
            {
                if (!larsLearningDeliveriesTask.Result.TryGetValue(learner.LearnRefNumber, out ILarsLearningDelivery larsModel))
                {
                    larsError.Add(learner.LearnRefNumber);
                    continue;
                }

                if (!larsFrameworkAimsTask.Result.TryGetValue(learner.LearnRefNumber, out ILarsFrameworkAim frameworkAim))
                {
                    larsError.Add(learner.LearnRefNumber);
                    continue;
                }

                LearnerAttribute albLearner =
                    albDataTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == learner.LearnRefNumber);
                if (albLearner == null)
                {
                    albLearnerError.Add(learner.LearnRefNumber);
                    continue;
                }

                var albLearnerOnProgPayment =
                    albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == Fm25OnProgPayment);

                var onProgPayment = albLearner.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == Fm35OnProgPayment);
                var balancePayment = albLearner.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == Fm35BalPayment);
                var achievePayment = albLearner.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == Fm35AchievePayment);
                var empOutcomePayment = albLearner.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == Fm35EmpOutcomePay);
                var learnSuppFundCash = albLearner.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == Fm35LearnSuppFundCash);

                foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                {
                    var albLearningDeliveryAttributeDatas = albLearner.LearningDeliveryAttributes
                        .SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber)
                        ?.LearningDeliveryAttributeDatas;
                    var albLearningDeliveryPeriodisedValues = albLearner.LearningDeliveryAttributes
                        .SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber)
                        ?.LearningDeliveryPeriodisedAttributes;

                    mainOccupancyFM35Models.Add(new MainOccupancyFM35Model()
                    {
                        LearnRefNumber = learner.LearnRefNumber,
                        Uln = learner.ULN,
                        DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                        PostcodePrior = learner.PostcodePrior,
                        PmUkprn = learner.PMUKPRNNullable,
                        CampId = learner.CampId,
                        ProvSpecLearnMonA = learner.ProviderSpecLearnerMonitorings
                            .SingleOrDefault(x => x.ProvSpecLearnMonOccur == "A")?.ProvSpecLearnMon,
                        ProvSpecLearnMonB = learner.ProviderSpecLearnerMonitorings
                            .SingleOrDefault(x => x.ProvSpecLearnMonOccur == "B")?.ProvSpecLearnMon,
                        AimSeqNumber = learningDelivery.AimSeqNumber,
                        LearnAimRef = learningDelivery.LearnAimRef,
                        LearnAimRefTitle = larsModel.LearningAimTitle,
                        SwSupAimId = learningDelivery.SWSupAimId,
                        WeightedRateFromEsol = "Todo", // Todo
                        ApplicWeightFundRate = "Todo", // Todo
                        ApplicProgWeightFact = albLearningDeliveryAttributeDatas?.ApplicProgWeightFact,
                        AimValue = "Todo", // Todo
                        NotionalNvqLevelV2 = larsModel.NotionalNvqLevel,
                        SectorSubjectAreaTier2 = larsModel.Tier2SectorSubjectArea,
                        ProgType = learningDelivery.ProgTypeNullable,
                        FworkCode = learningDelivery.FworkCodeNullable,
                        PwayCode = learningDelivery.PwayCodeNullable,
                        AimType = learningDelivery.AimType,
                        FrameworkComponentType = frameworkAim.FrameworkComponentType,
                        FundModel = learningDelivery.FundModel,
                        PriorLearnFundAdj = learningDelivery.PriorLearnFundAdjNullable,
                        OtherFundAdj = learningDelivery.OtherFundAdjNullable,
                        OrigLearnStartDate = learningDelivery.OrigLearnStartDateNullable,
                        LearnStartDate = learningDelivery.LearnStartDate,
                        LearnPlanEndDate = learningDelivery.LearnPlanEndDate,
                        CompStatus = learningDelivery.CompStatus,
                        LearnActEndDate = learningDelivery.LearnActEndDateNullable,
                        Outcome = learningDelivery.OutcomeNullable,
                        AchDate = learningDelivery.AchDateNullable,
                        AddHours = learningDelivery.AddHoursNullable,
                        LearnDelFamCodeSof = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "SOF")?.LearnDelFAMCode,
                        LearnDelFamCodeFfi = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "FFI")?.LearnDelFAMCode,
                        LearnDelFamCodeEef = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "EEF")?.LearnDelFAMCode,
                        LearnDelFamCodeLsfHighest = learningDelivery.LearningDeliveryFAMs?.Where(x => x.LearnDelFAMType == "LSF").Max(x => x.LearnDelFAMCode),
                        LearnDelFamCodeLsfEarliest = learningDelivery.LearningDeliveryFAMs?.Where(x => x.LearnDelFAMType == "LSF").Min(x => x.LearnDelFAMDateFromNullable)?.ToString("dd/MM/yyyy"),
                        LearnDelFamCodeLsfLatest = learningDelivery.LearningDeliveryFAMs?.Where(x => x.LearnDelFAMType == "LSF").Min(x => x.LearnDelFAMDateToNullable)?.ToString("dd/MM/yyyy"),
                        LearnDelFamCodeLdm1 = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM1")?.LearnDelFAMCode,
                        LearnDelFamCodeLdm2 = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM2")?.LearnDelFAMCode,
                        LearnDelFamCodeLdm3 = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM3")?.LearnDelFAMCode,
                        LearnDelFamCodeLdm4 = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM4")?.LearnDelFAMCode,
                        LearnDelFamCodeRes = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "RES")?.LearnDelFAMCode,
                        ProvSpecDelMonA = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "A")?.ProvSpecDelMon,
                        ProvSpecDelMonB = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "B")?.ProvSpecDelMon,
                        ProvSpecDelMonC = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "C")?.ProvSpecDelMon,
                        ProvSpecDelMonD = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "D")?.ProvSpecDelMon,
                        FundLine = albLearningDeliveryAttributeDatas?.FundLine,
                        PlannedNumOnProgInstalm = albLearningDeliveryAttributeDatas?.PlannedNumOnProgInstalm,
                        PlannedNumOnProgInstalmTrans = albLearningDeliveryAttributeDatas?.PlannedNumOnProgInstalm,
                        StartPropTrans = "Todo", // Todo
                        AchieveElement = "Todo", // Todo
                        AchievePercentage = GetMaxPeriod(albLearningDeliveryPeriodisedValues?.SingleOrDefault(x => x.AttributeName == "AchievePayPct")),
                        NonGovCont = "Todo", // Todo
                        PartnerUkprn = learningDelivery.PartnerUKPRNNullable,
                        DelLocPostCode = learningDelivery.DelLocPostCode,
                        AreaCostFactAdj = albLearningDeliveryAttributeDatas?.AreaCostFactAdj,
                        DisUpFactAdj = "Todo", // Todo
                        LargeEmployerID = "Todo", // Todo
                        LargeEmployerFM35Fctr = "Todo", // Todo
                        CapFactor = "Todo", // Todo
                        TraineeWorkPlacement = "Todo", // Todo (albLearningDeliveryAttributeDatas?.TrnWorkPlaceAim || albLearningDeliveryAttributeDatas?.TrnWorkPrepAim) ? "Yes" : "No",
                        HigherApprentishipHeAim = "Todo", // Todo albLearningDeliveryAttributeDatas?.PrscHEAim ? "Yes" : "No",
                        ApplicEmpFactDate = "Todo", // Todo albLearningDeliveryAttributeDatas?.ApplicEmpFactDate,
                        ApplicFactDate = albLearningDeliveryAttributeDatas?.ApplicFactDate,
                        Period1OnProgPayment = onProgPayment?.Period1,
                        Period1BalancePayment = balancePayment?.Period1,
                        Period1AchievePayment = achievePayment?.Period1,
                        Period1EmpOutcomePay = empOutcomePayment?.Period1,
                        Period1LearnSuppFundCash = learnSuppFundCash?.Period1,

                        Period2OnProgPayment = onProgPayment?.Period2,
                        Period2BalancePayment = balancePayment?.Period2,
                        Period2AchievePayment = achievePayment?.Period2,
                        Period2EmpOutcomePay = empOutcomePayment?.Period2,
                        Period2LearnSuppFundCash = learnSuppFundCash?.Period2,

                        Period3OnProgPayment = onProgPayment?.Period3,
                        Period3BalancePayment = balancePayment?.Period3,
                        Period3AchievePayment = achievePayment?.Period3,
                        Period3EmpOutcomePay = empOutcomePayment?.Period3,
                        Period3LearnSuppFundCash = learnSuppFundCash?.Period3,

                        Period4OnProgPayment = onProgPayment?.Period4,
                        Period4BalancePayment = balancePayment?.Period4,
                        Period4AchievePayment = achievePayment?.Period4,
                        Period4EmpOutcomePay = empOutcomePayment?.Period4,
                        Period4LearnSuppFundCash = learnSuppFundCash?.Period4,

                        Period5OnProgPayment = onProgPayment?.Period5,
                        Period5BalancePayment = balancePayment?.Period5,
                        Period5AchievePayment = achievePayment?.Period5,
                        Period5EmpOutcomePay = empOutcomePayment?.Period5,
                        Period5LearnSuppFundCash = learnSuppFundCash?.Period5,

                        Period6OnProgPayment = onProgPayment?.Period6,
                        Period6BalancePayment = balancePayment?.Period6,
                        Period6AchievePayment = achievePayment?.Period6,
                        Period6EmpOutcomePay = empOutcomePayment?.Period6,
                        Period6LearnSuppFundCash = learnSuppFundCash?.Period6,

                        Period7OnProgPayment = onProgPayment?.Period7,
                        Period7BalancePayment = balancePayment?.Period7,
                        Period7AchievePayment = achievePayment?.Period7,
                        Period7EmpOutcomePay = empOutcomePayment?.Period7,
                        Period7LearnSuppFundCash = learnSuppFundCash?.Period7,

                        Period8OnProgPayment = onProgPayment?.Period8,
                        Period8BalancePayment = balancePayment?.Period8,
                        Period8AchievePayment = achievePayment?.Period8,
                        Period8EmpOutcomePay = empOutcomePayment?.Period8,
                        Period8LearnSuppFundCash = learnSuppFundCash?.Period8,

                        Period9OnProgPayment = onProgPayment?.Period9,
                        Period9BalancePayment = balancePayment?.Period9,
                        Period9AchievePayment = achievePayment?.Period9,
                        Period9EmpOutcomePay = empOutcomePayment?.Period9,
                        Period9LearnSuppFundCash = learnSuppFundCash?.Period9,

                        Period10OnProgPayment = onProgPayment?.Period10,
                        Period10BalancePayment = balancePayment?.Period10,
                        Period10AchievePayment = achievePayment?.Period10,
                        Period10EmpOutcomePay = empOutcomePayment?.Period10,
                        Period10LearnSuppFundCash = learnSuppFundCash?.Period10,

                        Period11OnProgPayment = onProgPayment?.Period11,
                        Period11BalancePayment = balancePayment?.Period11,
                        Period11AchievePayment = achievePayment?.Period11,
                        Period11EmpOutcomePay = empOutcomePayment?.Period11,
                        Period11LearnSuppFundCash = learnSuppFundCash?.Period11,

                        Period12OnProgPayment = onProgPayment?.Period12,
                        Period12BalancePayment = balancePayment?.Period12,
                        Period12AchievePayment = achievePayment?.Period12,
                        Period12EmpOutcomePay = empOutcomePayment?.Period12,
                        Period12LearnSuppFundCash = learnSuppFundCash?.Period12,

                        TotalOnProgPayment = (onProgPayment?.Period1 ?? 0) + (onProgPayment?.Period2 ?? 0) + (onProgPayment?.Period3 ?? 0) + (onProgPayment?.Period4 ?? 0) + (onProgPayment?.Period5 ?? 0) + (onProgPayment?.Period6 ?? 0)
                                              + (onProgPayment?.Period7 ?? 0) + (onProgPayment?.Period8 ?? 0) + (onProgPayment?.Period9 ?? 0) + (onProgPayment?.Period10 ?? 0) + (onProgPayment?.Period11 ?? 0) + (onProgPayment?.Period12 ?? 0),
                        TotalBalancePayment = (balancePayment?.Period1 ?? 0) + (balancePayment?.Period2 ?? 0) + (balancePayment?.Period3 ?? 0) + (balancePayment?.Period4 ?? 0) + (balancePayment?.Period5 ?? 0) + (balancePayment?.Period6 ?? 0)
                                              + (balancePayment?.Period7 ?? 0) + (balancePayment?.Period8 ?? 0) + (balancePayment?.Period9 ?? 0) + (balancePayment?.Period10 ?? 0) + (balancePayment?.Period11 ?? 0) + (balancePayment?.Period12 ?? 0),
                        TotalAchievePayment = (achievePayment?.Period1 ?? 0) + (achievePayment?.Period2 ?? 0) + (achievePayment?.Period3 ?? 0) + (achievePayment?.Period4 ?? 0) + (achievePayment?.Period5 ?? 0) + (achievePayment?.Period6 ?? 0)
                                              + (achievePayment?.Period7 ?? 0) + (achievePayment?.Period8 ?? 0) + (achievePayment?.Period9 ?? 0) + (achievePayment?.Period10 ?? 0) + (achievePayment?.Period11 ?? 0) + (achievePayment?.Period12 ?? 0),
                        TotalEmpOutcomePay = (empOutcomePayment?.Period1 ?? 0) + (empOutcomePayment?.Period2 ?? 0) + (empOutcomePayment?.Period3 ?? 0) + (empOutcomePayment?.Period4 ?? 0) + (empOutcomePayment?.Period5 ?? 0) + (empOutcomePayment?.Period6 ?? 0)
                                             + (empOutcomePayment?.Period7 ?? 0) + (empOutcomePayment?.Period8 ?? 0) + (empOutcomePayment?.Period9 ?? 0) + (empOutcomePayment?.Period10 ?? 0) + (empOutcomePayment?.Period11 ?? 0) + (empOutcomePayment?.Period12 ?? 0),
                        TotalLearnSuppFundCash = (learnSuppFundCash?.Period1 ?? 0) + (learnSuppFundCash?.Period2 ?? 0) + (learnSuppFundCash?.Period3 ?? 0) + (learnSuppFundCash?.Period4 ?? 0) + (learnSuppFundCash?.Period5 ?? 0) + (learnSuppFundCash?.Period6 ?? 0)
                                                 + (learnSuppFundCash?.Period7 ?? 0) + (learnSuppFundCash?.Period8 ?? 0) + (learnSuppFundCash?.Period9 ?? 0) + (learnSuppFundCash?.Period10 ?? 0) + (learnSuppFundCash?.Period11 ?? 0) + (learnSuppFundCash?.Period12 ?? 0),

                        TotalEarnedCash = (onProgPayment?.Period1 ?? 0) + (onProgPayment?.Period2 ?? 0) + (onProgPayment?.Period3 ?? 0) + (onProgPayment?.Period4 ?? 0) + (onProgPayment?.Period5 ?? 0) + (onProgPayment?.Period6 ?? 0)
                                              + (onProgPayment?.Period7 ?? 0) + (onProgPayment?.Period8 ?? 0) + (onProgPayment?.Period9 ?? 0) + (onProgPayment?.Period10 ?? 0) + (onProgPayment?.Period11 ?? 0) + (onProgPayment?.Period12 ?? 0)
                                            + (balancePayment?.Period1 ?? 0) + (balancePayment?.Period2 ?? 0) + (balancePayment?.Period3 ?? 0) + (balancePayment?.Period4 ?? 0) + (balancePayment?.Period5 ?? 0) + (balancePayment?.Period6 ?? 0)
                                              + (balancePayment?.Period7 ?? 0) + (balancePayment?.Period8 ?? 0) + (balancePayment?.Period9 ?? 0) + (balancePayment?.Period10 ?? 0) + (balancePayment?.Period11 ?? 0) + (balancePayment?.Period12 ?? 0)
                                            + (achievePayment?.Period1 ?? 0) + (achievePayment?.Period2 ?? 0) + (achievePayment?.Period3 ?? 0) + (achievePayment?.Period4 ?? 0) + (achievePayment?.Period5 ?? 0) + (achievePayment?.Period6 ?? 0)
                                              + (achievePayment?.Period7 ?? 0) + (achievePayment?.Period8 ?? 0) + (achievePayment?.Period9 ?? 0) + (achievePayment?.Period10 ?? 0) + (achievePayment?.Period11 ?? 0) + (achievePayment?.Period12 ?? 0)
                                            + (empOutcomePayment?.Period1 ?? 0) + (empOutcomePayment?.Period2 ?? 0) + (empOutcomePayment?.Period3 ?? 0) + (empOutcomePayment?.Period4 ?? 0) + (empOutcomePayment?.Period5 ?? 0) + (empOutcomePayment?.Period6 ?? 0)
                                             + (empOutcomePayment?.Period7 ?? 0) + (empOutcomePayment?.Period8 ?? 0) + (empOutcomePayment?.Period9 ?? 0) + (empOutcomePayment?.Period10 ?? 0) + (empOutcomePayment?.Period11 ?? 0) + (empOutcomePayment?.Period12 ?? 0)
                                            + (learnSuppFundCash?.Period1 ?? 0) + (learnSuppFundCash?.Period2 ?? 0) + (learnSuppFundCash?.Period3 ?? 0) + (learnSuppFundCash?.Period4 ?? 0) + (learnSuppFundCash?.Period5 ?? 0) + (learnSuppFundCash?.Period6 ?? 0)
                                            + (learnSuppFundCash?.Period7 ?? 0) + (learnSuppFundCash?.Period8 ?? 0) + (learnSuppFundCash?.Period9 ?? 0) + (learnSuppFundCash?.Period10 ?? 0) + (learnSuppFundCash?.Period11 ?? 0) + (learnSuppFundCash?.Period12 ?? 0),
                    });

                    mainOccupancyFM25Models.Add(new MainOccupancyFM25Model()
                    {
                        LearnRefNumber = learner.LearnRefNumber,
                        Uln = learner.ULN,
                        DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                        PostcodePrior = learner.PostcodePrior,
                        PmUkprn = learner.PMUKPRNNullable,
                        CampId = learner.CampId,
                        ProvSpecLearnMonA = learner.ProviderSpecLearnerMonitorings
                            .SingleOrDefault(x => x.ProvSpecLearnMonOccur == "A")?.ProvSpecLearnMon,
                        ProvSpecLearnMonB = learner.ProviderSpecLearnerMonitorings
                            .SingleOrDefault(x => x.ProvSpecLearnMonOccur == "B")?.ProvSpecLearnMon,
                        NatRate = "Todo", // Todo albLearningDeliveryAttributeDatas.NatRate,
                        FundModel = learningDelivery.FundModel,
                        LearnerStartDate = "Todo", // Todo albLearningDeliveryAttributeDatas?.LearnerStartDate,
                        LearnerPlanEndDate = "Todo", // Todo albLearningDeliveryAttributeDatas?.LearnerPlanEndDate,
                        LearnerActEndDate = "Todo", // Todo albLearningDeliveryAttributeDatas?.LearnerActEndDate,
                        FundLine = albLearningDeliveryAttributeDatas?.FundLine,
                        Period1OnProgPayment = albLearnerOnProgPayment?.Period1,
                        Period2OnProgPayment = albLearnerOnProgPayment?.Period2,
                        Period3OnProgPayment = albLearnerOnProgPayment?.Period3,
                        Period4OnProgPayment = albLearnerOnProgPayment?.Period4,
                        Period5OnProgPayment = albLearnerOnProgPayment?.Period5,
                        Period6OnProgPayment = albLearnerOnProgPayment?.Period6,
                        Period7OnProgPayment = albLearnerOnProgPayment?.Period7,
                        Period8OnProgPayment = albLearnerOnProgPayment?.Period8,
                        Period9OnProgPayment = albLearnerOnProgPayment?.Period9,
                        Period10OnProgPayment = albLearnerOnProgPayment?.Period10,
                        Period11OnProgPayment = albLearnerOnProgPayment?.Period11,
                        Period12OnProgPayment = albLearnerOnProgPayment?.Period12,
                        PeriodOnProgPaymentTotal = albLearnerOnProgPayment?.Period1 + albLearnerOnProgPayment?.Period2
                                                      + albLearnerOnProgPayment?.Period3 + albLearnerOnProgPayment?.Period4
                                                      + albLearnerOnProgPayment?.Period5 + albLearnerOnProgPayment?.Period6
                                                      + albLearnerOnProgPayment?.Period7 + albLearnerOnProgPayment?.Period8
                                                      + albLearnerOnProgPayment?.Period9 + albLearnerOnProgPayment?.Period10
                                                      + albLearnerOnProgPayment?.Period11 + albLearnerOnProgPayment?.Period12,
                        Total = albLearnerOnProgPayment?.Period1 + albLearnerOnProgPayment?.Period2
                                                                                      + albLearnerOnProgPayment?.Period3 + albLearnerOnProgPayment?.Period4
                                                                                      + albLearnerOnProgPayment?.Period5 + albLearnerOnProgPayment?.Period6
                                                                                      + albLearnerOnProgPayment?.Period7 + albLearnerOnProgPayment?.Period8
                                                                                      + albLearnerOnProgPayment?.Period9 + albLearnerOnProgPayment?.Period10
                                                                                      + albLearnerOnProgPayment?.Period11 + albLearnerOnProgPayment?.Period12
                    });
                }
            }

            LogWarnings(larsError, albLearnerError);

            string csv = GetReportCsv(mainOccupancyFM25Models, mainOccupancyFM35Models);
            await _storage.SaveAsync("Main-Occupancy-Report.csv", csv, cancellationToken);
            await WriteZipEntry(archive, "Main-Occupancy-Report.csv", csv);
        }

        private string GetReportCsv(List<MainOccupancyFM25Model> mainOccupancyFm25Models, List<MainOccupancyFM35Model> mainOccupancyFm35Models)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BuildCsvReport<MainOccupancyFM25Mapper, MainOccupancyFM25Model>(ms, mainOccupancyFm25Models);
                BuildCsvReport<MainOccupancyFM35Mapper, MainOccupancyFM35Model>(ms, mainOccupancyFm35Models);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private void LogWarnings(List<string> larsError, List<string> albLearnerError)
        {
            if (larsError.Any())
            {
                _logger.LogWarning($"Failed to get LARS data while generating Allb Occupancy Report: {_stringUtilitiesService.JoinWithMaxLength(larsError)}");
            }

            if (albLearnerError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ALB learners while generating Allb Occupancy Report: {_stringUtilitiesService.JoinWithMaxLength(albLearnerError)}");
            }
        }

        private decimal GetMaxPeriod(LearningDeliveryPeriodisedAttribute learningDeliveryPeriodisedAttribute)
        {
            decimal max = int.MinValue;
            if (learningDeliveryPeriodisedAttribute.Period1 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period1;
            }

            if (learningDeliveryPeriodisedAttribute.Period2 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period2;
            }

            if (learningDeliveryPeriodisedAttribute.Period3 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period3;
            }

            if (learningDeliveryPeriodisedAttribute.Period4 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period4;
            }

            if (learningDeliveryPeriodisedAttribute.Period5 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period5;
            }

            if (learningDeliveryPeriodisedAttribute.Period6 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period6;
            }

            if (learningDeliveryPeriodisedAttribute.Period7 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period7;
            }

            if (learningDeliveryPeriodisedAttribute.Period8 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period8;
            }

            if (learningDeliveryPeriodisedAttribute.Period9 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period9;
            }

            if (learningDeliveryPeriodisedAttribute.Period10 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period10;
            }

            if (learningDeliveryPeriodisedAttribute.Period11 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period11;
            }

            if (learningDeliveryPeriodisedAttribute.Period12 > max)
            {
                max = learningDeliveryPeriodisedAttribute.Period12;
            }

            return max;
        }
    }
}
