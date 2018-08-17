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
    public sealed class AllbOccupancyReport : AbstractReportBuilder, IReport
    {
        private const string AlbCode = "ALBCode";

        private const string AlbSupportPayment = "ALBSupportPayment";

        private const string AlbAreaUpliftBalPayment = "AreaUpliftBalPayment";

        private const string AlbAreaUpliftOnProgPayment = "AreaUpliftOnProgPayment";

        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IAllbProviderService _allbProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;

        public AllbOccupancyReport(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService blob,
            IIlrProviderService ilrProviderService,
            ILarsProviderService larsProviderService,
            IAllbProviderService allbProviderService,
            IValidLearnersService validLearnersService,
            IStringUtilitiesService stringUtilitiesService,
            IDateTimeProvider dateTimeProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider)
        {
            _logger = logger;
            _storage = blob;
            _ilrProviderService = ilrProviderService;
            _larsProviderService = larsProviderService;
            _allbProviderService = allbProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;

            ReportFileName = "ALLB Occupancy Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateAllbOccupancyReport;
        }

        public async Task GenerateReport(IJobContextMessage jobContextMessage, ZipArchive archive, CancellationToken cancellationToken)
        {
            var jobId = jobContextMessage.JobId;
            var ukPrn = jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString();
            var fileName = GetReportFilename(ukPrn, jobId);

            string csv = await GetCsv(jobContextMessage, cancellationToken);
            await _storage.SaveAsync($"{fileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(jobContextMessage, cancellationToken);
            Task<FundingOutputs> albDataTask = _allbProviderService.GetAllbData(jobContextMessage, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(jobContextMessage, cancellationToken);

            await Task.WhenAll(ilrFileTask, albDataTask, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (validLearnersTask.Result == null)
            {
                return null;
            }

            List<string> learnAimRefs = ilrFileTask.Result?.Learners?.Where(x => validLearnersTask.Result.Contains(x.LearnRefNumber))
                .SelectMany(x => x.LearningDeliveries).Select(x => x.LearnAimRef).ToList();

            Dictionary<string, ILarsLearningDelivery> larsLearningDeliveriesTask = await _larsProviderService.GetLearningDeliveries(learnAimRefs, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            List<string> ilrError = new List<string>();
            List<string> larsError = new List<string>();
            List<string> albLearnerError = new List<string>();

            List<AllbOccupancyModel> models = new List<AllbOccupancyModel>(validLearnersTask.Result.Count);
            foreach (string validLearnerRefNum in validLearnersTask.Result)
            {
                var learner = ilrFileTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);
                if (learner == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                if (!larsLearningDeliveriesTask.TryGetValue(validLearnerRefNum, out ILarsLearningDelivery larsModel))
                {
                    larsError.Add(validLearnerRefNum);
                    continue;
                }

                LearnerAttribute albLearner =
                    albDataTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);
                if (albLearner == null)
                {
                    albLearnerError.Add(validLearnerRefNum);
                    continue;
                }

                if (learner.LearningDeliveries == null)
                {
                    ilrError.Add(validLearnerRefNum);
                }

                foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                {
                    var albAttribs = albLearner?.LearningDeliveryAttributes
                        .SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber)
                        ?.LearningDeliveryAttributeDatas;
                    var albSupportPaymentObj =
                        albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbSupportPayment);
                    var albAreaUpliftOnProgPaymentObj =
                        albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbAreaUpliftOnProgPayment);
                    var albAreaUpliftBalPaymentObj =
                        albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbAreaUpliftBalPayment);

                    var alb = learningDelivery.LearningDeliveryFAMs.SingleOrDefault(x => x.LearnDelFAMType == "ALB");

                    models.Add(new AllbOccupancyModel
                    {
                        LearnRefNumber = learner.LearnRefNumber,
                        Uln = learner.ULN,
                        DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                        PreMergerUkprn = learner.PrevUKPRNNullable,
                        CampId = learner.CampId,
                        ProvSpecLearnMonA = learner.ProviderSpecLearnerMonitorings
                            ?.FirstOrDefault(x => x.ProvSpecLearnMonOccur == "A")?.ProvSpecLearnMon,
                        ProvSpecLearnMonB = learner.ProviderSpecLearnerMonitorings
                            ?.FirstOrDefault(x => x.ProvSpecLearnMonOccur == "B")?.ProvSpecLearnMon,
                        AimSeqNumber = learningDelivery.AimSeqNumber,
                        LearnAimRef = learningDelivery.LearnAimRef,
                        LearnAimRefTitle = larsModel.LearningAimTitle,
                        SwSupAimId = learningDelivery.SWSupAimId,
                        WeightedRate = albAttribs?.WeightedRate,
                        ApplicProgWeightFact = albAttribs?.ApplicProgWeightFact,
                        NotionalNvqLevelV2 = larsModel.NotionalNvqLevel,
                        SectorSubjectAreaTier2 = larsModel.Tier2SectorSubjectArea,
                        AimType = learningDelivery.AimType,
                        FundingModel = learningDelivery.FundModel,
                        PriorLearnFundAdj = learningDelivery.PriorLearnFundAdjNullable,
                        OtherFundAdj = learningDelivery.OtherFundAdjNullable,
                        OrigLearnStartDate = learningDelivery.OrigLearnStartDateNullable?.ToString("dd/MM/yyyy"),
                        LearnStartDate = learningDelivery.LearnStartDate.ToString("dd/MM/yyyy"),
                        LearnPlanEndDate = learningDelivery.LearnPlanEndDate.ToString("dd/MM/yyyy"),
                        CompStatus = learningDelivery.CompStatus,
                        LearnActEndDate = learningDelivery.LearnActEndDateNullable?.ToString("dd/MM/yyyy"),
                        Outcome = learningDelivery.OutcomeNullable,
                        LearnDelFamCodeAdl = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "ADL")?.LearnDelFAMCode,
                        AlbBursaryFunding = alb?.LearnDelFAMCode,
                        AlbDateFrom = alb?.LearnDelFAMDateFromNullable?.ToString("dd/MM/yyyy"),
                        AlbDateTo = alb?.LearnDelFAMDateToNullable?.ToString("dd/MM/yyyy"),
                        LearnDelMonA = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM1")?.LearnDelFAMCode,
                        LearnDelMonB = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM2")?.LearnDelFAMCode,
                        LearnDelMonC = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM3")?.LearnDelFAMCode,
                        LearnDelMonD = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x => x.LearnDelFAMType == "LDM4")?.LearnDelFAMCode,
                        ProvSpecDelMonA = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "A")?.ProvSpecDelMon,
                        ProvSpecDelMonB = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "B")?.ProvSpecDelMon,
                        ProvSpecDelMonC = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "C")?.ProvSpecDelMon,
                        ProvSpecDelMonD = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x => x.ProvSpecDelMonOccur == "D")?.ProvSpecDelMon,
                        PartnerUkprn = learningDelivery.PartnerUKPRNNullable,
                        DelLocPostCode = learningDelivery.DelLocPostCode,
                        AreaCodeFactAdj = albAttribs?.AreaCostFactAdj,
                        FundLine = albAttribs?.FundLine,
                        LiabilityDate = albAttribs?.LiabilityDate?.ToString("dd/MM/yyyy"),
                        PlannedNumOnProgInstalm = albAttribs?.PlannedNumOnProgInstalm,
                        ApplicFactDate = albAttribs?.ApplicFactDate?.ToString("dd/MM/yyyy"),
                        Period1AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period1,
                        Period1AlbPayment = albSupportPaymentObj?.Period1,
                        Period1AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period1 ?? 0,
                        Period1AlbAreaUplift = albAreaUpliftBalPaymentObj?.Period1 ?? 0,
                        Period1AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period1) ?? 0,
                        Period2AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period2 ?? 0,
                        Period2AlbPayment = albSupportPaymentObj?.Period2 ?? 0,
                        Period2AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period2 ?? 0,
                        Period2AlbBalPayment = albAreaUpliftBalPaymentObj?.Period2 ?? 0,
                        Period2AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period2) ?? 0,
                        Period3AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period3 ?? 0,
                        Period3AlbPayment = albSupportPaymentObj?.Period3 ?? 0,
                        Period3AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period3 ?? 0,
                        Period3AlbBalPayment = albAreaUpliftBalPaymentObj?.Period3 ?? 0,
                        Period3AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period3) ?? 0,
                        Period4AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period4 ?? 0,
                        Period4AlbPayment = albSupportPaymentObj?.Period4 ?? 0,
                        Period4AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period4 ?? 0,
                        Period4AlbBalPayment = albAreaUpliftBalPaymentObj?.Period4 ?? 0,
                        Period4AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period4) ?? 0,
                        Period5AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period5 ?? 0,
                        Period5AlbPayment = albSupportPaymentObj?.Period5 ?? 0,
                        Period5AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period5 ?? 0,
                        Period5AlbBalPayment = albAreaUpliftBalPaymentObj?.Period5 ?? 0,
                        Period5AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period5) ?? 0,
                        Period6AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period6 ?? 0,
                        Period6AlbPayment = albSupportPaymentObj?.Period6 ?? 0,
                        Period6AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period6 ?? 0,
                        Period6AlbBalPayment = albAreaUpliftBalPaymentObj?.Period6 ?? 0,
                        Period6AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period6) ?? 0,
                        Period7AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period7 ?? 0,
                        Period7AlbPayment = albSupportPaymentObj?.Period7 ?? 0,
                        Period7AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period7 ?? 0,
                        Period7AlbBalPayment = albAreaUpliftBalPaymentObj?.Period7 ?? 0,
                        Period7AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period7) ?? 0,
                        Period8AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period8 ?? 0,
                        Period8AlbPayment = albSupportPaymentObj?.Period8 ?? 0,
                        Period8AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period8 ?? 0,
                        Period8AlbBalPayment = albAreaUpliftBalPaymentObj?.Period8 ?? 0,
                        Period8AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period8) ?? 0,
                        Period9AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period9 ?? 0,
                        Period9AlbPayment = albSupportPaymentObj?.Period9 ?? 0,
                        Period9AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period9 ?? 0,
                        Period9AlbBalPayment = albAreaUpliftBalPaymentObj?.Period9 ?? 0,
                        Period9AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period9) ?? 0,
                        Period10AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period10 ?? 0,
                        Period10AlbPayment = albSupportPaymentObj?.Period10 ?? 0,
                        Period10AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period10 ?? 0,
                        Period10AlbBalPayment = albAreaUpliftBalPaymentObj?.Period10 ?? 0,
                        Period10AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period10) ?? 0,
                        Period11AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period11 ?? 0,
                        Period11AlbPayment = albSupportPaymentObj?.Period11 ?? 0,
                        Period11AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period11 ?? 0,
                        Period11AlbBalPayment = albAreaUpliftBalPaymentObj?.Period11 ?? 0,
                        Period11AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period11) ?? 0,
                        Period12AlbCode = albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbCode)?.Period12 ?? 0,
                        Period12AlbPayment = albSupportPaymentObj?.Period12 ?? 0,
                        Period12AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period12 ?? 0,
                        Period12AlbBalPayment = albAreaUpliftBalPaymentObj?.Period12 ?? 0,
                        Period12AlbTotal = albLearner?.LearnerPeriodisedAttributes.Where(x => x.AttributeName == AlbSupportPayment || x.AttributeName == AlbAreaUpliftOnProgPayment || x.AttributeName == AlbAreaUpliftBalPayment).Sum(x => x.Period12) ?? 0,
                        TotalAlbSupportPayment = (albSupportPaymentObj?.Period1 ?? 0) + (albSupportPaymentObj?.Period2 ?? 0) + (albSupportPaymentObj?.Period3 ?? 0) + (albSupportPaymentObj?.Period4 ?? 0) + (albSupportPaymentObj?.Period5 ?? 0) + (albSupportPaymentObj?.Period6 ?? 0) + (albSupportPaymentObj?.Period7 ?? 0)
                                                + (albSupportPaymentObj?.Period8 ?? 0) + (albSupportPaymentObj?.Period9 ?? 0) + (albSupportPaymentObj?.Period10 ?? 0) + (albSupportPaymentObj?.Period11 ?? 0) + (albSupportPaymentObj?.Period12 ?? 0),
                        TotalAlbAreaUplift = (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0)
                                            + (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0),
                        TotalAlbBalPayment = (albAreaUpliftBalPaymentObj?.Period1 ?? 0) + (albAreaUpliftBalPaymentObj?.Period2 ?? 0) + (albAreaUpliftBalPaymentObj?.Period3 ?? 0) + (albAreaUpliftBalPaymentObj?.Period4 ?? 0) + (albAreaUpliftBalPaymentObj?.Period5 ?? 0) + (albAreaUpliftBalPaymentObj?.Period6 ?? 0) + (albAreaUpliftBalPaymentObj?.Period7 ?? 0)
                                            + (albAreaUpliftBalPaymentObj?.Period8 ?? 0) + (albAreaUpliftBalPaymentObj?.Period9 ?? 0) + (albAreaUpliftBalPaymentObj?.Period10 ?? 0) + (albAreaUpliftBalPaymentObj?.Period11 ?? 0) + (albAreaUpliftBalPaymentObj?.Period12 ?? 0),
                        TotalEarnedCash = (albSupportPaymentObj?.Period1 ?? 0) + (albSupportPaymentObj?.Period2 ?? 0) + (albSupportPaymentObj?.Period3 ?? 0) + (albSupportPaymentObj?.Period4 ?? 0) + (albSupportPaymentObj?.Period5 ?? 0) + (albSupportPaymentObj?.Period6 ?? 0) + (albSupportPaymentObj?.Period7 ?? 0)
                                          + (albSupportPaymentObj?.Period8 ?? 0) + (albSupportPaymentObj?.Period9 ?? 0) + (albSupportPaymentObj?.Period10 ?? 0) + (albSupportPaymentObj?.Period11 ?? 0) + (albSupportPaymentObj?.Period12 ?? 0)
                                          + (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0)
                                          + (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0)
                                          + (albAreaUpliftBalPaymentObj?.Period1 ?? 0) + (albAreaUpliftBalPaymentObj?.Period2 ?? 0) + (albAreaUpliftBalPaymentObj?.Period3 ?? 0) + (albAreaUpliftBalPaymentObj?.Period4 ?? 0) + (albAreaUpliftBalPaymentObj?.Period5 ?? 0) + (albAreaUpliftBalPaymentObj?.Period6 ?? 0) + (albAreaUpliftBalPaymentObj?.Period7 ?? 0)
                                          + (albAreaUpliftBalPaymentObj?.Period8 ?? 0) + (albAreaUpliftBalPaymentObj?.Period9 ?? 0) + (albAreaUpliftBalPaymentObj?.Period10 ?? 0) + (albAreaUpliftBalPaymentObj?.Period11 ?? 0) + (albAreaUpliftBalPaymentObj?.Period12 ?? 0)
                    });
                }
            }

            CheckWarnings(ilrError, larsError, albLearnerError);
            return WriteResults(models);
        }

        private string WriteResults(IReadOnlyCollection<AllbOccupancyModel> models)
        {
            using (var ms = new MemoryStream())
            {
                BuildCsvReport<AllbOccupancyMapper, AllbOccupancyModel>(ms, models);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private void CheckWarnings(IReadOnlyCollection<string> ilrError, IReadOnlyCollection<string> larsError, List<string> albLearnerError)
        {
            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating {nameof(MathsAndEnglishReport)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
            }

            if (larsError.Any())
            {
                _logger.LogWarning($"Failed to get one or more LARS learners while generating {nameof(MathsAndEnglishReport)}: {_stringUtilitiesService.JoinWithMaxLength(larsError)}");
            }

            if (albLearnerError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ALB learners while generating {nameof(MathsAndEnglishReport)}: {_stringUtilitiesService.JoinWithMaxLength(albLearnerError)}");
            }
        }
    }
}
