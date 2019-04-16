using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using LearningDelivery = ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public sealed class AllbOccupancyReport : AbstractReport, IReport
    {
        private const string AlbCode = "ALBCode";

        private const string AlbSupportPayment = "ALBSupportPayment";

        private const string AlbAreaUpliftBalPayment = "AreaUpliftBalPayment";

        private const string AlbAreaUpliftOnProgPayment = "AreaUpliftOnProgPayment";

        private static readonly AllbOccupancyModelComparer AllbOccupancyModelComparer = new AllbOccupancyModelComparer();

        private readonly ILogger _logger;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IAllbProviderService _allbProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;

        public AllbOccupancyReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrProviderService ilrProviderService,
            ILarsProviderService larsProviderService,
            IAllbProviderService allbProviderService,
            IValidLearnersService validLearnersService,
            IStringUtilitiesService stringUtilitiesService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _ilrProviderService = ilrProviderService;
            _larsProviderService = larsProviderService;
            _allbProviderService = allbProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;

            ReportFileName = "ALLB Occupancy Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateAllbOccupancyReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            var jobId = reportServiceContext.JobId;
            var ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            string csv = await GetCsv(reportServiceContext, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<ALBGlobal> albDataTask = _allbProviderService.GetAllbData(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, albDataTask, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if (validLearnersTask.Result == null)
            {
                return null;
            }

            string[] learnAimRefs = ilrFileTask.Result?.Learners?.Where(x => validLearnersTask.Result.Contains(x.LearnRefNumber))
                .SelectMany(x => x.LearningDeliveries).Select(x => x.LearnAimRef).Distinct().ToArray();

            Dictionary<string, LarsLearningDelivery> larsLearningDeliveries = await _larsProviderService.GetLearningDeliveriesAsync(learnAimRefs, cancellationToken);
            if (larsLearningDeliveries == null)
            {
                return null;
            }

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
                var learner = ilrFileTask.Result?.Learners?.SingleOrDefault(x => string.Equals(x.LearnRefNumber, validLearnerRefNum, StringComparison.OrdinalIgnoreCase));
                if (learner == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                ALBLearner albLearner =
                    albDataTask.Result?.Learners?.SingleOrDefault(x => string.Equals(x.LearnRefNumber, validLearnerRefNum, StringComparison.OrdinalIgnoreCase));
                if (albLearner == null)
                {
                    albLearnerError.Add(validLearnerRefNum);
                    continue;
                }

                if (learner.LearningDeliveries == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                {
                    LearningDelivery albLearningDelivery = albLearner?.LearningDeliveries
                        ?.SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber);

                    if (!ValidAllbLearningDelivery(learningDelivery, albLearningDelivery))
                    {
                        continue;
                    }

                    if (!larsLearningDeliveries.TryGetValue(learningDelivery.LearnAimRef, out LarsLearningDelivery larsModel))
                    {
                        larsError.Add(validLearnerRefNum);
                        continue;
                    }

                    LearningDeliveryValue albLearningDeliveryValue = albLearningDelivery?.LearningDeliveryValue;
                    LearningDeliveryPeriodisedValue albSupportPaymentObj =
                        albLearningDelivery?.LearningDeliveryPeriodisedValues?.SingleOrDefault(x =>
                            string.Equals(x.AttributeName, AlbSupportPayment, StringComparison.OrdinalIgnoreCase));
                    LearningDeliveryPeriodisedValue albAreaUpliftOnProgPaymentObj =
                        albLearningDelivery?.LearningDeliveryPeriodisedValues?.SingleOrDefault(x =>
                            string.Equals(x.AttributeName, AlbAreaUpliftOnProgPayment, StringComparison.OrdinalIgnoreCase));
                    LearningDeliveryPeriodisedValue albAreaUpliftBalPaymentObj =
                        albLearningDelivery?.LearningDeliveryPeriodisedValues?.SingleOrDefault(x =>
                            string.Equals(x.AttributeName, AlbAreaUpliftBalPayment, StringComparison.OrdinalIgnoreCase));
                    ILearningDeliveryFAM[] alb = learningDelivery.LearningDeliveryFAMs?.Where(x =>
                        string.Equals(x.LearnDelFAMType, "ALB", StringComparison.OrdinalIgnoreCase)).ToArray();
                    LearningDeliveryPeriodisedValue[] payments = albLearningDelivery?.LearningDeliveryPeriodisedValues?.Where(IsPayment).ToArray();
                    LearningDeliveryPeriodisedValue albCode = albLearningDelivery?.LearningDeliveryPeriodisedValues?.SingleOrDefault(x =>
                        string.Equals(x.AttributeName, AlbCode, StringComparison.OrdinalIgnoreCase));
                    var ldms = _stringUtilitiesService.GetArrayEntries(learningDelivery.LearningDeliveryFAMs?.Where(x => string.Equals(x.LearnDelFAMType, "LDM", StringComparison.OrdinalIgnoreCase)), 4);

                    string albBursaryFunding = string.Empty, albDateFrom = string.Empty, albDateTo = string.Empty;
                    if (alb != null && alb.Any())
                    {
                        albBursaryFunding = alb.Max(x => _stringUtilitiesService.TryGetInt(x.LearnDelFAMCode, 0)).ToString();
                        albDateFrom = _stringUtilitiesService.GetDateTimeAsString(alb.Min(x => x.LearnDelFAMDateFromNullable ?? DateTime.MinValue), string.Empty, DateTime.MinValue);
                        albDateTo = _stringUtilitiesService.GetDateTimeAsString(alb.Max(x => x.LearnDelFAMDateToNullable ?? DateTime.MinValue), string.Empty, DateTime.MinValue);
                    }

                    models.Add(new AllbOccupancyModel
                    {
                        LearnRefNumber = learner.LearnRefNumber,
                        Uln = learner.ULN,
                        DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                        PreMergerUkprn = learner.PrevUKPRNNullable,
                        CampId = learner.CampId,
                        ProvSpecLearnMonA = learner.ProviderSpecLearnerMonitorings
                            ?.FirstOrDefault(x => string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                        ProvSpecLearnMonB = learner.ProviderSpecLearnerMonitorings
                            ?.FirstOrDefault(x => string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                        AimSeqNumber = learningDelivery.AimSeqNumber,
                        LearnAimRef = learningDelivery.LearnAimRef,
                        LearnAimRefTitle = larsModel.LearningAimTitle,
                        SwSupAimId = learningDelivery.SWSupAimId,
                        WeightedRate = albLearningDeliveryValue?.WeightedRate,
                        ApplicProgWeightFact = albLearningDeliveryValue?.ApplicProgWeightFact,
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
                        LearnDelFamCodeAdl = learningDelivery.LearningDeliveryFAMs?.SingleOrDefault(x =>
                            string.Equals(x.LearnDelFAMType, "ADL", StringComparison.OrdinalIgnoreCase))?.LearnDelFAMCode,
                        AlbBursaryFunding = albBursaryFunding,
                        AlbDateFrom = albDateFrom,
                        AlbDateTo = albDateTo,
                        LearnDelMonA = ldms[0],
                        LearnDelMonB = ldms[1],
                        LearnDelMonC = ldms[2],
                        LearnDelMonD = ldms[3],
                        ProvSpecDelMonA = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x =>
                            string.Equals(x.ProvSpecDelMonOccur, "A", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                        ProvSpecDelMonB = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x =>
                            string.Equals(x.ProvSpecDelMonOccur, "B", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                        ProvSpecDelMonC = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x =>
                            string.Equals(x.ProvSpecDelMonOccur, "C", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                        ProvSpecDelMonD = learningDelivery.ProviderSpecDeliveryMonitorings?.SingleOrDefault(x =>
                            string.Equals(x.ProvSpecDelMonOccur, "D", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                        PartnerUkprn = learningDelivery.PartnerUKPRNNullable,
                        DelLocPostCode = learningDelivery.DelLocPostCode,
                        AreaCodeFactAdj = albLearningDeliveryValue?.AreaCostFactAdj,
                        FundLine = albLearningDeliveryValue?.FundLine,
                        LiabilityDate = albLearningDeliveryValue?.LiabilityDate?.ToString("dd/MM/yyyy"),
                        PlannedNumOnProgInstalm = albLearningDeliveryValue?.PlannedNumOnProgInstalm,
                        ApplicFactDate = albLearningDeliveryValue?.ApplicFactDate?.ToString("dd/MM/yyyy"),

                        Period1AlbCode = albCode?.Period1,
                        Period1AlbPayment = albSupportPaymentObj?.Period1,
                        Period1AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period1 ?? 0,
                        Period1AlbAreaUplift = albAreaUpliftBalPaymentObj?.Period1 ?? 0,
                        Period1AlbTotal = payments?.Sum(x => x.Period1) ?? 0,

                        Period2AlbCode = albCode?.Period2 ?? 0,
                        Period2AlbPayment = albSupportPaymentObj?.Period2 ?? 0,
                        Period2AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period2 ?? 0,
                        Period2AlbBalPayment = albAreaUpliftBalPaymentObj?.Period2 ?? 0,
                        Period2AlbTotal = payments?.Sum(x => x.Period2) ?? 0,

                        Period3AlbCode = albCode?.Period3 ?? 0,
                        Period3AlbPayment = albSupportPaymentObj?.Period3 ?? 0,
                        Period3AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period3 ?? 0,
                        Period3AlbBalPayment = albAreaUpliftBalPaymentObj?.Period3 ?? 0,
                        Period3AlbTotal = payments?.Sum(x => x.Period3) ?? 0,

                        Period4AlbCode = albCode?.Period4 ?? 0,
                        Period4AlbPayment = albSupportPaymentObj?.Period4 ?? 0,
                        Period4AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period4 ?? 0,
                        Period4AlbBalPayment = albAreaUpliftBalPaymentObj?.Period4 ?? 0,
                        Period4AlbTotal = payments?.Sum(x => x.Period4) ?? 0,

                        Period5AlbCode = albCode?.Period5 ?? 0,
                        Period5AlbPayment = albSupportPaymentObj?.Period5 ?? 0,
                        Period5AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period5 ?? 0,
                        Period5AlbBalPayment = albAreaUpliftBalPaymentObj?.Period5 ?? 0,
                        Period5AlbTotal = payments?.Sum(x => x.Period5) ?? 0,

                        Period6AlbCode = albCode?.Period6 ?? 0,
                        Period6AlbPayment = albSupportPaymentObj?.Period6 ?? 0,
                        Period6AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period6 ?? 0,
                        Period6AlbBalPayment = albAreaUpliftBalPaymentObj?.Period6 ?? 0,
                        Period6AlbTotal = payments?.Sum(x => x.Period6) ?? 0,

                        Period7AlbCode = albCode?.Period7 ?? 0,
                        Period7AlbPayment = albSupportPaymentObj?.Period7 ?? 0,
                        Period7AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period7 ?? 0,
                        Period7AlbBalPayment = albAreaUpliftBalPaymentObj?.Period7 ?? 0,
                        Period7AlbTotal = payments?.Sum(x => x.Period7) ?? 0,

                        Period8AlbCode = albCode?.Period8 ?? 0,
                        Period8AlbPayment = albSupportPaymentObj?.Period8 ?? 0,
                        Period8AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period8 ?? 0,
                        Period8AlbBalPayment = albAreaUpliftBalPaymentObj?.Period8 ?? 0,
                        Period8AlbTotal = payments?.Sum(x => x.Period8) ?? 0,

                        Period9AlbCode = albCode?.Period9 ?? 0,
                        Period9AlbPayment = albSupportPaymentObj?.Period9 ?? 0,
                        Period9AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period9 ?? 0,
                        Period9AlbBalPayment = albAreaUpliftBalPaymentObj?.Period9 ?? 0,
                        Period9AlbTotal = payments?.Sum(x => x.Period9) ?? 0,

                        Period10AlbCode = albCode?.Period10 ?? 0,
                        Period10AlbPayment = albSupportPaymentObj?.Period10 ?? 0,
                        Period10AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period10 ?? 0,
                        Period10AlbBalPayment = albAreaUpliftBalPaymentObj?.Period10 ?? 0,
                        Period10AlbTotal = payments?.Sum(x => x.Period10) ?? 0,

                        Period11AlbCode = albCode?.Period11 ?? 0,
                        Period11AlbPayment = albSupportPaymentObj?.Period11 ?? 0,
                        Period11AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period11 ?? 0,
                        Period11AlbBalPayment = albAreaUpliftBalPaymentObj?.Period11 ?? 0,
                        Period11AlbTotal = payments?.Sum(x => x.Period11) ?? 0,

                        Period12AlbCode = albCode?.Period12 ?? 0,
                        Period12AlbPayment = albSupportPaymentObj?.Period12 ?? 0,
                        Period12AlbOnProgPayment = albAreaUpliftOnProgPaymentObj?.Period12 ?? 0,
                        Period12AlbBalPayment = albAreaUpliftBalPaymentObj?.Period12 ?? 0,
                        Period12AlbTotal = payments?.Sum(x => x.Period12) ?? 0,

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
            models.Sort(AllbOccupancyModelComparer);
            return WriteResults(models);
        }

        private bool ValidAllbLearningDelivery(ILearningDelivery learningDelivery, LearningDelivery albLearningDelivery)
        {
            return learningDelivery.FundModel == 99
                   && learningDelivery.LearningDeliveryFAMs != null
                   && learningDelivery.LearningDeliveryFAMs.Any(x => string.Equals(x.LearnDelFAMType, "ADL", StringComparison.OrdinalIgnoreCase))
                   && (learningDelivery.LearningDeliveryFAMs.Any(x => string.Equals(x.LearnDelFAMType, "ALB", StringComparison.OrdinalIgnoreCase))
                       || albLearningDelivery.LearningDeliveryValue.AreaCostFactAdj > 0)
                   && !learningDelivery.LearningDeliveryFAMs.Any(x => string.Equals(x.LearnDelFAMType, "LDM", StringComparison.OrdinalIgnoreCase)
                                                                      && string.Equals(x.LearnDelFAMCode, "359", StringComparison.OrdinalIgnoreCase));
        }

        private bool IsPayment(LearningDeliveryPeriodisedValue x)
        {
            return string.Equals(x.AttributeName, AlbSupportPayment, StringComparison.OrdinalIgnoreCase) || string.Equals(x.AttributeName, AlbAreaUpliftOnProgPayment, StringComparison.OrdinalIgnoreCase) || string.Equals(x.AttributeName, AlbAreaUpliftBalPayment, StringComparison.OrdinalIgnoreCase);
        }

        private string WriteResults(IReadOnlyCollection<AllbOccupancyModel> models)
        {
            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AllbOccupancyMapper, AllbOccupancyModel>(csvWriter, models);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
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
