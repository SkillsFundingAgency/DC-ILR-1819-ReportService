using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.DataMatch;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Extensions;
using ESFA.DC.ILR.ReportService.Service.Extensions.DataMatch;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.ReferenceData;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public sealed class DataMatchReport : AbstractReport, IReport
    {
        private static readonly DataMatchModelComparer DataMatchModelComparer = new DataMatchModelComparer();

        private readonly ILogger _logger;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IDasCommitmentsService _dasCommitmentsService;
        private readonly IPeriodProviderService _periodProviderService;
        private readonly IValidationStageOutputCache _validationStageOutputCache;
        private readonly IDatalockValidationResultBuilder _datalockValidationResultBuilder;
        private readonly ITotalBuilder _totalBuilder;

        private readonly List<DataMatchModel> dataMatchModels;

        public DataMatchReport(
            ILogger logger,
            IFM36ProviderService fm36ProviderService,
            IDasCommitmentsService dasCommitmentsService,
            IPeriodProviderService periodProviderService,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IValidationStageOutputCache validationStageOutputCache,
            IDatalockValidationResultBuilder datalockValidationResultBuilder,
            ITotalBuilder totalBuilder)
            : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _fm36ProviderService = fm36ProviderService;
            _dasCommitmentsService = dasCommitmentsService;
            _periodProviderService = periodProviderService;
            _validationStageOutputCache = validationStageOutputCache;
            _datalockValidationResultBuilder = datalockValidationResultBuilder;
            _totalBuilder = totalBuilder;

            dataMatchModels = new List<DataMatchModel>();
            ReportFileName = "Apprenticeship Data Match Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateDataMatchReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            FM36Global fm36Data = await _fm36ProviderService.GetFM36Data(reportServiceContext, cancellationToken).ConfigureAwait(false);

            if (fm36Data?.Learners == null)
            {
                _logger.LogWarning($"No FM36 learner data for {nameof(DataMatchReport)}. It will be empty.");
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();

                List<RawEarning> rawEarnings = new List<RawEarning>();
                List<long> ulns = await GetUlnsForValidLearners(
                    fm36Data,
                    rawEarnings,
                    reportServiceContext,
                    cancellationToken);

                List<DasCommitment> commitments = await _dasCommitmentsService.GetCommitments(
                    reportServiceContext.Ukprn,
                    ulns,
                    cancellationToken);

                _logger.LogWarning($"Found {commitments.Count} commitments against UKPRN {reportServiceContext.Ukprn} and {ulns.Count} ULNs.");

                cancellationToken.ThrowIfCancellationRequested();

                BuildReportData(rawEarnings, commitments, reportServiceContext.Ukprn);

                _validationStageOutputCache.DataMatchProblemCount = dataMatchModels.Count;
                _validationStageOutputCache.DataMatchProblemLearnersCount = dataMatchModels.DistinctBy(x => x.LearnRefNumber).Count();
            }

            var jobId = reportServiceContext.JobId;
            var externalFileName = GetExternalFilename(reportServiceContext.Ukprn.ToString(), jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(reportServiceContext.Ukprn.ToString(), jobId, reportServiceContext.SubmissionDateTimeUtc);

            string csv = WriteResults();
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private void BuildReportData(List<RawEarning> rawEarnings, List<DasCommitment> commitments, long ukPrn)
        {
            foreach (RawEarning rawEarning in rawEarnings)
            {
                var commitmentsToMatch = commitments.Where(x => x.Uln == rawEarning.Uln).ToList();

                if (!commitmentsToMatch.Any())
                {
                    AddError(DataLockValidationMessages.DLOCK_02, rawEarning, rawEarning.Uln.ToString());
                    AddError(DataLockValidationMessages.DLOCK_02, rawEarning, commitmentsToMatch);
                    continue;
                }

                var commitmentsToMatch2 = commitmentsToMatch.Where(x => x.Ukprn == rawEarning.Ukprn).ToList();

                if (!commitmentsToMatch2.Any())
                {
                    AddError(DataLockValidationMessages.DLOCK_01, rawEarning, rawEarning.Ukprn.ToString(), ukPrn.ToString());
                    AddError(DataLockValidationMessages.DLOCK_01, rawEarning, commitmentsToMatch);
                    continue;
                }

                commitmentsToMatch = commitmentsToMatch2;

                var hasStandardCode = rawEarning.StandardCode > 0 ||
                                      commitmentsToMatch.Any(x => x.StandardCode.HasValue && x.StandardCode > 0);

                if (hasStandardCode)
                {
                    commitmentsToMatch2 = commitmentsToMatch.Where(c => c.StandardCode.HasValue &&
                                                                        c.StandardCode.Value == rawEarning.StandardCode)
                        .ToList();

                    if (!commitmentsToMatch2.Any())
                    {
                        AddError(DataLockValidationMessages.DLOCK_03, rawEarning, commitmentsToMatch);
                        foreach (DasCommitment dasCommitment in commitmentsToMatch)
                        {
                            AddError(
                                DataLockValidationMessages.DLOCK_03,
                                rawEarning,
                                rawEarning.StandardCode.ToString(),
                                dasCommitment.StandardCode.GetValueOrDefault(0).ToString());
                        }
                    }
                    else
                    {
                        commitmentsToMatch = commitmentsToMatch2;
                    }
                }

                var hasFrameworkCode = rawEarning.FrameworkCode > 0 ||
                                       commitmentsToMatch.Any(x => x.FrameworkCode.HasValue && x.FrameworkCode > 0);

                if (hasFrameworkCode)
                {
                    commitmentsToMatch2 = commitmentsToMatch.Where(c => c.FrameworkCode.HasValue &&
                                                                        rawEarning.FrameworkCode != 0 &&
                                                                        c.FrameworkCode.Value == rawEarning.FrameworkCode)
                        .ToList();

                    if (!commitmentsToMatch2.Any())
                    {
                        AddError(DataLockValidationMessages.DLOCK_04, rawEarning, commitmentsToMatch);
                        foreach (DasCommitment dasCommitment in commitmentsToMatch)
                        {
                            AddError(
                                DataLockValidationMessages.DLOCK_04,
                                rawEarning,
                                rawEarning.FrameworkCode?.ToString(),
                                dasCommitment.FrameworkCode.GetValueOrDefault(0).ToString());
                        }
                    }
                    else
                    {
                        commitmentsToMatch = commitmentsToMatch2;
                    }
                }

                var hasPathwayCode = rawEarning.PathwayCode > 0 ||
                                     commitmentsToMatch.Any(x => x.PathwayCode.HasValue && x.PathwayCode > 0);

                if (hasPathwayCode)
                {
                    commitmentsToMatch2 = commitmentsToMatch.Where(c => c.PathwayCode.HasValue &&
                                                                        rawEarning.PathwayCode > 0 &&
                                                                        c.PathwayCode.Value == rawEarning.PathwayCode).ToList();

                    if (!commitmentsToMatch2.Any())
                    {
                        AddError(DataLockValidationMessages.DLOCK_06, rawEarning, commitmentsToMatch);
                        foreach (DasCommitment dasCommitment in commitmentsToMatch)
                        {
                            AddError(
                                DataLockValidationMessages.DLOCK_06,
                                rawEarning,
                                rawEarning.PathwayCode?.ToString(),
                                dasCommitment.PathwayCode.GetValueOrDefault(0).ToString());
                        }
                    }
                }

                var hasProgrammeType = rawEarning.ProgrammeType > 0 ||
                                       commitmentsToMatch.Any(x => x.ProgrammeType.HasValue && x.ProgrammeType > 0);

                if (hasProgrammeType)
                {
                    commitmentsToMatch2 = commitmentsToMatch.Where(c => c.ProgrammeType.HasValue &&
                                                                        rawEarning.ProgrammeType != 0 &&
                                                                        c.ProgrammeType.Value == rawEarning.ProgrammeType)
                        .ToList();

                    if (!commitmentsToMatch2.Any())
                    {
                        AddError(DataLockValidationMessages.DLOCK_05, rawEarning, commitmentsToMatch);
                        foreach (DasCommitment dasCommitment in commitmentsToMatch)
                        {
                            AddError(
                                DataLockValidationMessages.DLOCK_05,
                                rawEarning,
                                rawEarning.ProgrammeType?.ToString(),
                                dasCommitment.ProgrammeType.GetValueOrDefault(0).ToString());
                        }
                    }
                    else
                    {
                        commitmentsToMatch = commitmentsToMatch2;
                    }
                }

                commitmentsToMatch2 = commitmentsToMatch.Where(c => c.AgreedCost == rawEarning.AgreedPrice).ToList();

                if (!commitmentsToMatch2.Any())
                {
                    AddError(DataLockValidationMessages.DLOCK_07, rawEarning, commitmentsToMatch);
                    foreach (DasCommitment dasCommitment in commitmentsToMatch)
                    {
                        AddError(
                            DataLockValidationMessages.DLOCK_07,
                            rawEarning,
                            rawEarning.AgreedPrice?.ToString() ?? string.Empty,
                            dasCommitment.AgreedCost.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    commitmentsToMatch = commitmentsToMatch2;
                }

                commitmentsToMatch2 = commitmentsToMatch.Where(c =>
                {
                    if (c.IsVersioned)
                    {
                        return c.EffectiveFromDate <= rawEarning.EpisodeEffectiveTnpStartDate;
                    }

                    return c.StartDate <= rawEarning.EpisodeEffectiveTnpStartDate;
                }).ToList();

                if (!commitmentsToMatch2.Any())
                {
                    AddError(
                        DataLockValidationMessages.DLOCK_09,
                        rawEarning,
                        rawEarning.LearningDeliveryStartDate.ToString("dd/MM/yyyy HH:mm"));
                    AddError(DataLockValidationMessages.DLOCK_09, rawEarning, commitmentsToMatch);
                }
                else
                {
                    commitmentsToMatch = commitmentsToMatch2;
                }

                var censusDate = CalculateOnProgCensusDate(rawEarning);

                var withdrawnCommitments = commitmentsToMatch
                    .Where(x => x.PaymentStatus == (int)PaymentStatus.Withdrawn || x.WithdrawnOnDate.HasValue)
                    .ToList();
                var activeWithdrawnCommitments = withdrawnCommitments
                    .Where(x => x.WithdrawnOnDate >= censusDate)
                    .ToList();

                if (withdrawnCommitments.Any() && !activeWithdrawnCommitments.Any())
                {
                    AddError(DataLockValidationMessages.DLOCK_10, rawEarning);
                    AddError(DataLockValidationMessages.DLOCK_10, rawEarning, commitmentsToMatch);
                }

                if (commitmentsToMatch.Any(x => x.PaymentStatus == (int)PaymentStatus.Paused))
                {
                    AddError(DataLockValidationMessages.DLOCK_12, rawEarning);
                    AddError(DataLockValidationMessages.DLOCK_12, rawEarning, commitmentsToMatch);
                }
            }

            RemoveErrorsInAdditionToDLOCK_09();

            dataMatchModels.Sort(DataMatchModelComparer);
        }

        private void AddError(string ruleName, RawEarning rawEarning, List<DasCommitment> dasCommitments)
        {
            if (rawEarning.HasNonIncentiveEarnings())
            {
                foreach (DasCommitment dasCommitment in dasCommitments)
                {
                    _datalockValidationResultBuilder.Add(
                        rawEarning,
                        new List<string> { ruleName },
                        TransactionTypesFlag.AllLearning,
                        dasCommitment);
                }
            }

            if (rawEarning.HasCompletionPayment())
            {
                foreach (DasCommitment dasCommitment in dasCommitments)
                {
                    _datalockValidationResultBuilder.Add(
                        rawEarning,
                        new List<string> { ruleName },
                        TransactionTypesFlag.Completion,
                        dasCommitment);
                }
            }

            if (rawEarning.HasFirstIncentive())
            {
                foreach (DasCommitment dasCommitment in dasCommitments)
                {
                    _datalockValidationResultBuilder.Add(
                        rawEarning,
                        new List<string> { ruleName },
                        TransactionTypesFlag.FirstEmployerProviderIncentives,
                        dasCommitment);
                }
            }

            if (rawEarning.HasSecondIncentive())
            {
                foreach (DasCommitment dasCommitment in dasCommitments)
                {
                    _datalockValidationResultBuilder.Add(
                        rawEarning,
                        new List<string> { ruleName },
                        TransactionTypesFlag.SecondEmployerProviderIncentives,
                        dasCommitment);
                }
            }

            if (rawEarning.HasCareLeaverApprenticePayment())
            {
                foreach (DasCommitment dasCommitment in dasCommitments)
                {
                    _datalockValidationResultBuilder.Add(
                        rawEarning,
                        new List<string> { ruleName },
                        TransactionTypesFlag.CareLeaverApprenticePayments,
                        dasCommitment);
                }
            }
        }

        private void AddError(string ruleName, RawEarning rawEarning, string ilrValue = "", string calculatedValue = "")
        {
            dataMatchModels.Add(new DataMatchModel
            {
                LearnRefNumber = rawEarning.LearnRefNumber,
                Uln = rawEarning.Uln,
                AimSeqNumber = rawEarning.AimSeqNumber,
                RuleName = ruleName,
                Description = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId == ruleName)?.ErrorMessage ?? "Description not found",
                ILRValue = ilrValue,
                ApprenticeshipServiceValue = calculatedValue,
                PriceEpisodeStartDate = rawEarning.EpisodeStartDate?.ToString("dd/MM/yyyy HH:mm") ?? rawEarning.EpisodeEffectiveTnpStartDate?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                PriceEpisodeActualEndDate = rawEarning.EndDate?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty,
                PriceEpisodeIdentifier = rawEarning.PriceEpisodeIdentifier
            });
        }

        private async Task<List<long>> GetUlnsForValidLearners(
            FM36Global fm36Data,
            List<RawEarning> rawEarnings,
            IReportServiceContext reportServiceContext,
            CancellationToken cancellationToken)
        {
            List<long> ulns = new List<long>();

            foreach (FM36Learner fm36DataLearner in fm36Data.Learners)
            {
                foreach (LearningDelivery learningDelivery in fm36DataLearner.LearningDeliveries)
                {
                    if (fm36DataLearner.PriceEpisodes != null)
                    {
                        List<PriceEpisode> priceEpisode = fm36DataLearner.PriceEpisodes.Where(x => x.PriceEpisodeValues.PriceEpisodeAimSeqNumber == learningDelivery.AimSeqNumber && x.PriceEpisodeValues.EpisodeStartDate > Constants.BeginningOfYear && x.PriceEpisodeValues.EpisodeStartDate < Constants.EndOfYear && string.Equals(x.PriceEpisodeValues.PriceEpisodeContractType, Constants.Fm36PriceEpisodeContractTypeLevyContract, StringComparison.OrdinalIgnoreCase)).ToList();

                        if (!priceEpisode.Any())
                        {
                            _logger.LogWarning($"Can't find any price episodes for learner {fm36DataLearner.LearnRefNumber} with learning delivery aim sequence no. {learningDelivery.AimSeqNumber} skipping {nameof(DataMatchReport)} {nameof(RawEarning)} model");
                            continue;
                        }

                        foreach (PriceEpisode episode in priceEpisode)
                        {
                            RawEarning rawEarning = new RawEarning();
                            rawEarning.Uln = fm36DataLearner.ULN;
                            rawEarning.LearnRefNumber = fm36DataLearner.LearnRefNumber;
                            rawEarning.AimSeqNumber = learningDelivery.AimSeqNumber;
                            rawEarning.Ukprn = fm36Data.UKPRN;
                            rawEarning.EpisodeStartDate = episode.PriceEpisodeValues.EpisodeStartDate;
                            rawEarning.EpisodeEffectiveTnpStartDate =
                                episode.PriceEpisodeValues.EpisodeEffectiveTNPStartDate;
                            rawEarning.EndDate = episode.PriceEpisodeValues.PriceEpisodeActualEndDate ??
                                                 episode.PriceEpisodeValues.PriceEpisodePlannedEndDate;
                            rawEarning.Period = reportServiceContext.ReturnPeriod;
                            rawEarning.ProgrammeType = learningDelivery.LearningDeliveryValues.ProgType;
                            rawEarning.FrameworkCode = learningDelivery.LearningDeliveryValues.FworkCode;
                            rawEarning.PathwayCode = learningDelivery.LearningDeliveryValues.PwayCode;
                            rawEarning.StandardCode = learningDelivery.LearningDeliveryValues.StdCode;
                            rawEarning.AgreedPrice = episode.PriceEpisodeValues.PriceEpisodeTotalTNPPrice;

                            rawEarning.LearningDeliveryStartDate = learningDelivery.LearningDeliveryValues.LearnStartDate;

                            rawEarning.PriceEpisodeIdentifier = episode.PriceEpisodeIdentifier;

                            rawEarning.TransactionType01 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType02 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType03 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType04 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType05 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType06 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType07 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType08 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType09 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType10 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType11 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType12 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType15 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.TransactionType16 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
                            rawEarning.FirstIncentiveCensusDate = episode.PriceEpisodeValues.PriceEpisodeFirstAdditionalPaymentThresholdDate;
                            rawEarning.SecondIncentiveCensusDate = episode.PriceEpisodeValues.PriceEpisodeSecondAdditionalPaymentThresholdDate;
                            rawEarning.LearnerAdditionalPaymentsDate = episode.PriceEpisodeValues.PriceEpisodeLearnerAdditionalPaymentThresholdDate;

                            rawEarnings.Add(rawEarning);
                        }
                    }
                }
            }

            //foreach (ILearner learner in message.Learners)
            //{
            //    FM36Learner fm36Entry =
            //        fm36Data.Learners.SingleOrDefault(x => string.Equals(x.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase));

            //    if (fm36Entry == null)
            //    {
            //        continue;
            //    }

            //    ulns.Add(learner.ULN);

            //    ILearningDelivery[] learningDeliveries = learner.LearningDeliveries.Where(x =>
            //        x.FundModel == 36 && x.LearningDeliveryFAMs.Any(y =>
            //            string.Equals(y.LearnDelFAMType, Constants.Fm36LearnDelFAMTypeAct, StringComparison.OrdinalIgnoreCase) &&
            //            string.Equals(y.LearnDelFAMCode, Constants.Fm36LearnDelFAMCodeOne, StringComparison.OrdinalIgnoreCase))).ToArray();

            //    _logger.LogWarning($"Found {learningDeliveries.Length} FM36 {Constants.Fm36LearnDelFAMTypeAct} {Constants.Fm36LearnDelFAMCodeOne} learning deliveries for learner {learner.LearnRefNumber}");

            //    foreach (ILearningDelivery learnerLearningDelivery in learningDeliveries)
            //    {
            //        List<PriceEpisode> priceEpisode = fm36Entry.PriceEpisodes.Where(x => x.PriceEpisodeValues.PriceEpisodeAimSeqNumber == learnerLearningDelivery.AimSeqNumber && string.Equals(x.PriceEpisodeValues.PriceEpisodeContractType, Constants.Fm36PriceEpisodeContractTypeLevyContract, StringComparison.OrdinalIgnoreCase)).ToList();

            //        if (!priceEpisode.Any())
            //        {
            //            _logger.LogWarning($"Can't find any price episodes for learner {learner.LearnRefNumber} with learning delivery aim sequence no. {learnerLearningDelivery.AimSeqNumber} skipping {nameof(DataMatchReport)} {nameof(RawEarning)} model");
            //            continue;
            //        }

            //        foreach (PriceEpisode episode in priceEpisode)
            //        {
            //            RawEarning rawEarning = new RawEarning();
            //            rawEarning.Uln = learner.ULN;
            //            rawEarning.LearnRefNumber = learner.LearnRefNumber;
            //            rawEarning.AimSeqNumber = learnerLearningDelivery.AimSeqNumber;
            //            rawEarning.Ukprn = message.HeaderEntity.SourceEntity.UKPRN;
            //            rawEarning.EpisodeStartDate = episode.PriceEpisodeValues.EpisodeStartDate;
            //            rawEarning.EpisodeEffectiveTnpStartDate =
            //                episode.PriceEpisodeValues.EpisodeEffectiveTNPStartDate;
            //            rawEarning.EndDate = episode.PriceEpisodeValues.PriceEpisodeActualEndDate ??
            //                                 episode.PriceEpisodeValues.PriceEpisodePlannedEndDate;
            //            rawEarning.Period = period;
            //            rawEarning.ProgrammeType = learnerLearningDelivery.ProgTypeNullable;
            //            rawEarning.FrameworkCode = learnerLearningDelivery.FworkCodeNullable;
            //            rawEarning.PathwayCode = learnerLearningDelivery.PwayCodeNullable;
            //            rawEarning.StandardCode = learnerLearningDelivery.StdCodeNullable;
            //            rawEarning.AgreedPrice = episode.PriceEpisodeValues.PriceEpisodeTotalTNPPrice;

            //            rawEarning.LearningDeliveryStartDate = learnerLearningDelivery.LearnStartDate;

            //            rawEarning.PriceEpisodeIdentifier = episode.PriceEpisodeIdentifier;

            //            rawEarning.TransactionType01 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType02 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType03 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType04 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType05 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType06 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType07 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType08 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType09 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType10 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType11 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType12 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType15 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.TransactionType16 = _totalBuilder.TotalRecords(episode.PriceEpisodePeriodisedValues.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase)));
            //            rawEarning.FirstIncentiveCensusDate = episode.PriceEpisodeValues.PriceEpisodeFirstAdditionalPaymentThresholdDate;
            //            rawEarning.SecondIncentiveCensusDate = episode.PriceEpisodeValues.PriceEpisodeSecondAdditionalPaymentThresholdDate;
            //            rawEarning.LearnerAdditionalPaymentsDate = episode.PriceEpisodeValues.PriceEpisodeLearnerAdditionalPaymentThresholdDate;

            //            rawEarnings.Add(rawEarning);
            //        }
            //    }
            //}

            return ulns;
        }

        private DateTime CalculateOnProgCensusDate(RawEarning earning)
        {
            int month = _periodProviderService.MonthFromPeriod(earning.Period);
            int year = YearFromPeriod(earning.Period, earning.EpisodeStartDate ?? new DateTime(9999, 01, 01));

            int lastDayOfMonth = DateTime.DaysInMonth(year, month);
            return new DateTime(year, month, lastDayOfMonth);
        }

        private int YearFromPeriod(int period, DateTime episodeStartDate)
        {
            int month = _periodProviderService.MonthFromPeriod(period);
            DateTime startOfAcademicYear = StartOfAcademicYearFromEpisodeStartDate(episodeStartDate);
            if (month < 8)
            {
                return startOfAcademicYear.Year + 1;
            }

            return startOfAcademicYear.Year;
        }

        private DateTime StartOfAcademicYearFromEpisodeStartDate(DateTime episodeStartDate)
        {
            int month = episodeStartDate.Month;
            if (month < 8)
            {
                return new DateTime(episodeStartDate.Year - 1, 8, 1);
            }

            return new DateTime(episodeStartDate.Year, 8, 1);
        }

        private string WriteResults()
        {
            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<DataMatchReportMapper, DataMatchModel>(csvWriter, dataMatchModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private void RemoveErrorsInAdditionToDLOCK_09()
        {
            var groupedErrors = dataMatchModels.GroupBy(x => x.PriceEpisodeIdentifier);
            foreach (var groupedError in groupedErrors)
            {
                if (groupedError.Any(x => x.RuleName == DataLockValidationMessages.DLOCK_09))
                {
                    dataMatchModels.RemoveAll(x => x.PriceEpisodeIdentifier == groupedError.Key &&
                                                    x.RuleName != DataLockValidationMessages.DLOCK_09);
                }
            }
        }
    }
}
