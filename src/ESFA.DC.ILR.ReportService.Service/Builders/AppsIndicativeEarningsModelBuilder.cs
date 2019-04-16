using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.Poco;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public class AppsIndicativeEarningsModelBuilder : IAppsIndicativeEarningsModelBuilder
    {
        private readonly LearningDeliveryFamSimple _blankFam;

        private readonly IList<IAppsIndicativeCommand> _commands;
        private readonly ITotalBuilder _totalBuilder;
        private readonly IStringUtilitiesService _stringUtilitiesService;

        public AppsIndicativeEarningsModelBuilder(IList<IAppsIndicativeCommand> commands, ITotalBuilder totalBuilder, IStringUtilitiesService stringUtilitiesService)
        {
            _commands = commands;
            _totalBuilder = totalBuilder;
            _stringUtilitiesService = stringUtilitiesService;

            _blankFam = new LearningDeliveryFamSimple(string.Empty, DateTime.MinValue, DateTime.MinValue);
        }

        public AppsIndicativeEarningsModel BuildModel(
            ILearner learner,
            ILearningDelivery learningDelivery,
            LearningDelivery fm36DeliveryAttribute,
            PriceEpisode fm36EpisodeAttribute,
            LarsLearningDelivery larsLearningDelivery,
            string notionalEndLevel,
            bool earliestEpisode,
            bool hasPriceEpisodes)
        {
            DateTime employmentStatusDate = learner.LearnerEmploymentStatuses?
                .Where(x => x.DateEmpStatApp <= learningDelivery.LearnStartDate).Select(x => x.DateEmpStatApp)
                .DefaultIfEmpty(DateTime.MinValue).Max() ?? DateTime.MinValue;
            ILearnerEmploymentStatus employmentStatus =
                learner.LearnerEmploymentStatuses?.SingleOrDefault(x => x.DateEmpStatApp == employmentStatusDate);
            LearningDeliveryFamSimple[] learningDeliveryFams = GetLearningDeliveryFams(learningDelivery);
            string fundingLineType = GetFundingType(fm36DeliveryAttribute?.LearningDeliveryValues, fm36EpisodeAttribute?.PriceEpisodeValues);
            var ldms = _stringUtilitiesService.GetArrayEntries(learningDelivery.LearningDeliveryFAMs?.Where(x => string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeLDM, StringComparison.OrdinalIgnoreCase)), 4);

            var model = new AppsIndicativeEarningsModel
            {
                LearnRefNumber = learner.LearnRefNumber,
                UniqueLearnerNumber = learner.ULN,
                DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                PostcodePriorToEnrollment = learner.PostcodePrior,
                CampusIdentifier = learner.CampId,
                ProviderSpecifiedLearnerMonitoringA = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                ProviderSpecifiedLearnerMonitoringB = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                AimSeqNumber = learningDelivery.AimSeqNumber,
                LearningAimReference = learningDelivery.LearnAimRef,
                LearningAimTitle = larsLearningDelivery?.LearningAimTitle,
                SoftwareSupplierAimIdentifier = learningDelivery.SWSupAimId,
                LARS1618FrameworkUplift = fm36DeliveryAttribute?.LearningDeliveryValues
                    ?.LearnDelApplicProv1618FrameworkUplift,
                NotionalNVQLevel = larsLearningDelivery?.NotionalNvqLevel,
                StandardNotionalEndLevel = notionalEndLevel,
                Tier2SectorSubjectArea = larsLearningDelivery?.Tier2SectorSubjectArea,
                ProgrammeType = learningDelivery.ProgTypeNullable,
                StandardCode = learningDelivery.StdCodeNullable,
                FrameworkCode = learningDelivery.FworkCodeNullable,
                ApprenticeshipPathway = learningDelivery.PwayCodeNullable,
                AimType = learningDelivery.AimType,
                CommonComponentCode = larsLearningDelivery?.FrameworkCommonComponent,
                FundingModel = learningDelivery.FundModel,
                OriginalLearningStartDate = learningDelivery.OrigLearnStartDateNullable?.ToString("dd/MM/yyyy"),
                LearningStartDate = learningDelivery.LearnStartDate.ToString("dd/MM/yyyy"),
                LearningPlannedEndDate = learningDelivery.LearnPlanEndDate.ToString("dd/MM/yyyy"),
                CompletionStatus = learningDelivery.CompStatus,
                LearningActualEndDate = learningDelivery.LearnActEndDateNullable?.ToString("dd/MM/yyyy"),
                Outcome = learningDelivery.OutcomeNullable,
                FundingAdjustmentForPriorLearning = learningDelivery.PriorLearnFundAdjNullable,
                OtherFundingAdjustment = learningDelivery.OtherFundAdjNullable,
                LearningDeliveryFAMTypeLearningSupportFunding = learningDeliveryFams.Select(x => x.Code).Max(),
                LearningDeliveryFAMTypeLSFDateAppliesFrom =
                    learningDeliveryFams.Select(x => x.From).Min() == DateTime.MinValue ? string.Empty : learningDeliveryFams.Select(x => x.From).Min().ToString("dd/MM/yyyy"),
                LearningDeliveryFAMTypeLSFDateAppliesTo =
                learningDeliveryFams.Select(x => x.To).Max() == DateTime.MinValue ? string.Empty : learningDeliveryFams.Select(x => x.To).Max().ToString("dd/MM/yyyy"),
            LearningDeliveryFAMTypeLearningDeliveryMonitoringA = ldms[0],
                LearningDeliveryFAMTypeLearningDeliveryMonitoringB = ldms[1],
                LearningDeliveryFAMTypeLearningDeliveryMonitoringC = ldms[2],
                LearningDeliveryFAMTypeLearningDeliveryMonitoringD = ldms[3],
                LearningDeliveryFAMRestartIndicator = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeRES, StringComparison.OrdinalIgnoreCase))?.LearnDelFAMCode,
                ProviderSpecifiedDeliveryMonitoringA = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecDelMonOccur, "A", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                ProviderSpecifiedDeliveryMonitoringB = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecDelMonOccur, "B", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                ProviderSpecifiedDeliveryMonitoringC = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecDelMonOccur, "C", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                ProviderSpecifiedDeliveryMonitoringD = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecDelMonOccur, "D", StringComparison.OrdinalIgnoreCase))?.ProvSpecDelMon,
                EndPointAssessmentOrganisation = learningDelivery.EPAOrgID,
                PlannedNoOfOnProgrammeInstallmentsForAim =
                    fm36DeliveryAttribute?.LearningDeliveryValues?.PlannedNumOnProgInstalm,
                SubContractedOrPartnershipUKPRN = learningDelivery.PartnerUKPRNNullable,
                DeliveryLocationPostcode = learningDelivery.DelLocPostCode,
                EmployerIdentifier = employmentStatus?.EmpIdNullable,
                AgreementIdentifier = fm36EpisodeAttribute?.PriceEpisodeValues?.PriceEpisodeAgreeId,
                EmploymentStatus = employmentStatus?.EmpStat,
                EmploymentStatusDate = employmentStatus?.DateEmpStatApp.ToString("dd/MM/yyyy"),
                EmpStatusMonitoringSmallEmployer = employmentStatus?.EmploymentStatusMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ESMType, Constants.EmploymentStatusMonitoringTypeSEM, StringComparison.OrdinalIgnoreCase))?.ESMCode,
                PriceEpisodeStartDate =
                    fm36EpisodeAttribute?.PriceEpisodeValues?.EpisodeStartDate,
                PriceEpisodeActualEndDate =
                    fm36EpisodeAttribute?.PriceEpisodeValues?.PriceEpisodeActualEndDate?.ToString("dd/MM/yyyy"),
                FundingLineType = fundingLineType,
                TotalPriceApplicableToThisEpisode =
                    fm36EpisodeAttribute?.PriceEpisodeValues?.PriceEpisodeTotalTNPPrice,
                FundingBandUpperLimit = fm36EpisodeAttribute?.PriceEpisodeValues?.PriceEpisodeUpperBandLimit,
                PriceAmountAboveFundingBandLimit =
                    fm36EpisodeAttribute?.PriceEpisodeValues?.PriceEpisodeUpperLimitAdjustment,
                PriceAmountRemainingStartOfEpisode = fm36EpisodeAttribute?.PriceEpisodeValues
                    ?.PriceEpisodeCappedRemainingTNPAmount,
                CompletionElement = fm36EpisodeAttribute?.PriceEpisodeValues?.PriceEpisodeCompletionElement
            };

            if (learningDelivery?.LearningDeliveryFAMs != null)
            {
                CalculateApprenticeshipContractTypeFields(
                    learningDelivery,
                    model,
                    fm36DeliveryAttribute,
                    fm36EpisodeAttribute,
                    hasPriceEpisodes);
            }

            if (earliestEpisode)
            {
                CalculateAppFinTotals(model, learningDelivery);
            }

            foreach (var command in _commands)
            {
                command.Execute(model, fm36DeliveryAttribute, fm36EpisodeAttribute);
            }

            GetTotals(model);

            return model;
        }

        private string GetFundingType(
            LearningDeliveryValues learningDeliveryValues,
            PriceEpisodeValues priceEpisodeValues)
        {
            if (learningDeliveryValues != null && learningDeliveryValues.LearnDelMathEng.GetValueOrDefault(false))
            {
                return learningDeliveryValues.LearnDelInitialFundLineType;
            }

            if (priceEpisodeValues != null)
            {
                return priceEpisodeValues.PriceEpisodeFundLineType;
            }

            return string.Empty;
        }

        /// <summary>
        /// Total employer contribution collected (PMR) in previous funding years &
        /// Total employer contribution collected (PMR) in this funding year
        /// </summary>
        /// <param name="model">The model to populate.</param>
        /// <param name="learningDelivery">The learning delivery.</param>
        private void CalculateAppFinTotals(AppsIndicativeEarningsModel model, ILearningDelivery learningDelivery)
        {
            if (learningDelivery.AppFinRecords == null)
            {
                return;
            }

            List<IAppFinRecord> previousYearData = learningDelivery.AppFinRecords
                .Where(x => x.AFinDate < Constants.BeginningOfYear &&
                            string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();
            List<IAppFinRecord> currentYearData = learningDelivery.AppFinRecords
                .Where(x => x.AFinDate >= Constants.BeginningOfYear && x.AFinDate <= Constants.EndOfYear &&
                            string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

            model.TotalPRMPreviousFundingYear =
                previousYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                previousYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            model.TotalPRMThisFundingYear =
                currentYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);
        }

        /// <summary>
        /// Learning delivery funding and monitoring type – apprenticeship contract type
        /// Learning delivery funding and monitoring type – ACT date applies from
        /// Learning delivery funding and monitoring type – ACT date applies to
        /// </summary>
        /// <param name="learningDelivery">The learning delivery which is not null along with the LearningDeliveryFAMs</param>
        /// <param name="model">The row model.</param>
        /// <param name="fm36DeliveryAttribute">The Fm36 delivery attribute.</param>
        /// <param name="fm36PriceEpisodeAttribute">The Fm36 price episode attribute.</param>
        /// <param name="hasPriceEpisodes">Flag indicating whether there are price episodes.</param>
        private void CalculateApprenticeshipContractTypeFields(
            ILearningDelivery learningDelivery,
            AppsIndicativeEarningsModel model,
            LearningDelivery fm36DeliveryAttribute,
            PriceEpisode fm36PriceEpisodeAttribute,
            bool hasPriceEpisodes)
        {
            bool learnDelMathEng = fm36DeliveryAttribute?.LearningDeliveryValues?.LearnDelMathEng ?? false;
            bool useDeliveryAttributeDate = learnDelMathEng || !hasPriceEpisodes;
            ILearningDeliveryFAM[] acts = learningDelivery.LearningDeliveryFAMs.Where(x =>
                string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeACT, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (acts.All(x => x.LearnDelFAMDateFromNullable == null))
            {
                return;
            }

            DateTime contractAppliesFrom = acts.Where(x =>
                (useDeliveryAttributeDate && learningDelivery.LearnStartDate >= x.LearnDelFAMDateFromNullable) ||
                (!useDeliveryAttributeDate && fm36PriceEpisodeAttribute?.PriceEpisodeValues.EpisodeStartDate >= x.LearnDelFAMDateFromNullable))
                .Select(x => x.LearnDelFAMDateFromNullable ?? DateTime.MinValue)
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();

            DateTime contractAppliesTo = acts.Where(x =>
                (useDeliveryAttributeDate && learningDelivery.LearnStartDate <= x.LearnDelFAMDateToNullable) ||
                (!useDeliveryAttributeDate && fm36PriceEpisodeAttribute?.PriceEpisodeValues.EpisodeStartDate <= x.LearnDelFAMDateToNullable))
                .Select(x => x.LearnDelFAMDateToNullable ?? DateTime.MinValue)
                .DefaultIfEmpty(DateTime.MinValue)
                .Min();

            model.LearningDeliveryFAMTypeACTDateAppliesFrom = contractAppliesFrom == DateTime.MinValue ? $"" : contractAppliesFrom.ToString("dd/MM/yyyy");
            model.LearningDeliveryFAMTypeACTDateAppliesTo = contractAppliesTo == DateTime.MinValue ? $"" : contractAppliesTo.ToString("dd/MM/yyyy");

            if (contractAppliesTo == DateTime.MinValue)
            {
                model.LearningDeliveryFAMTypeApprenticeshipContractType = acts.FirstOrDefault(x => x.LearnDelFAMDateToNullable == null)?.LearnDelFAMCode;
            }
            else
            {
                model.LearningDeliveryFAMTypeApprenticeshipContractType = acts.FirstOrDefault(x =>
                    x.LearnDelFAMDateFromNullable == contractAppliesFrom &&
                    x.LearnDelFAMDateToNullable == contractAppliesTo)?.LearnDelFAMCode;
            }
        }

        private void GetTotals(AppsIndicativeEarningsModel model)
        {
            model.TotalOnProgrammeEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustOnProgrammeEarnings,
                    model.SeptemberOnProgrammeEarnings,
                    model.OctoberOnProgrammeEarnings,
                    model.NovemberOnProgrammeEarnings,
                    model.DecemberOnProgrammeEarnings,
                    model.JanuaryOnProgrammeEarnings,
                    model.FebruaryOnProgrammeEarnings,
                    model.MarchOnProgrammeEarnings,
                    model.AprilOnProgrammeEarnings,
                    model.MayOnProgrammeEarnings,
                    model.JuneOnProgrammeEarnings,
                    model.JulyOnProgrammeEarnings);

            model.TotalBalancingPaymentEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustBalancingPaymentEarnings,
                    model.SeptemberBalancingPaymentEarnings,
                    model.OctoberBalancingPaymentEarnings,
                    model.NovemberBalancingPaymentEarnings,
                    model.DecemberBalancingPaymentEarnings,
                    model.JanuaryBalancingPaymentEarnings,
                    model.FebruaryBalancingPaymentEarnings,
                    model.MarchBalancingPaymentEarnings,
                    model.AprilBalancingPaymentEarnings,
                    model.MayBalancingPaymentEarnings,
                    model.JuneBalancingPaymentEarnings,
                    model.JulyBalancingPaymentEarnings);

            model.TotalAimCompletionEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustAimCompletionEarnings,
                    model.SeptemberAimCompletionEarnings,
                    model.OctoberAimCompletionEarnings,
                    model.NovemberAimCompletionEarnings,
                    model.DecemberAimCompletionEarnings,
                    model.JanuaryAimCompletionEarnings,
                    model.FebruaryAimCompletionEarnings,
                    model.MarchAimCompletionEarnings,
                    model.AprilAimCompletionEarnings,
                    model.MayAimCompletionEarnings,
                    model.JuneAimCompletionEarnings,
                    model.JulyAimCompletionEarnings);

            model.TotalLearningSupportEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustLearningSupportEarnings,
                    model.SeptemberLearningSupportEarnings,
                    model.OctoberLearningSupportEarnings,
                    model.NovemberLearningSupportEarnings,
                    model.DecemberLearningSupportEarnings,
                    model.JanuaryLearningSupportEarnings,
                    model.FebruaryLearningSupportEarnings,
                    model.MarchLearningSupportEarnings,
                    model.AprilLearningSupportEarnings,
                    model.MayLearningSupportEarnings,
                    model.JuneLearningSupportEarnings,
                    model.JulyLearningSupportEarnings);

            model.TotalEnglishMathsOnProgrammeEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustEnglishMathsOnProgrammeEarnings,
                    model.SeptemberEnglishMathsOnProgrammeEarnings,
                    model.OctoberEnglishMathsOnProgrammeEarnings,
                    model.NovemberEnglishMathsOnProgrammeEarnings,
                    model.DecemberEnglishMathsOnProgrammeEarnings,
                    model.JanuaryEnglishMathsOnProgrammeEarnings,
                    model.FebruaryEnglishMathsOnProgrammeEarnings,
                    model.MarchEnglishMathsOnProgrammeEarnings,
                    model.AprilEnglishMathsOnProgrammeEarnings,
                    model.MayEnglishMathsOnProgrammeEarnings,
                    model.JuneEnglishMathsOnProgrammeEarnings,
                    model.JulyEnglishMathsOnProgrammeEarnings);

            model.TotalEnglishMathsBalancingPaymentEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustEnglishMathsBalancingPaymentEarnings,
                    model.SeptemberEnglishMathsBalancingPaymentEarnings,
                    model.OctoberEnglishMathsBalancingPaymentEarnings,
                    model.NovemberEnglishMathsBalancingPaymentEarnings,
                    model.DecemberEnglishMathsBalancingPaymentEarnings,
                    model.JanuaryEnglishMathsBalancingPaymentEarnings,
                    model.FebruaryEnglishMathsBalancingPaymentEarnings,
                    model.MarchEnglishMathsBalancingPaymentEarnings,
                    model.AprilEnglishMathsBalancingPaymentEarnings,
                    model.MayEnglishMathsBalancingPaymentEarnings,
                    model.JuneEnglishMathsBalancingPaymentEarnings,
                    model.JulyEnglishMathsBalancingPaymentEarnings);

            model.TotalDisadvantageEarnings =
                _totalBuilder.TotalRecords(
                    model.AugustDisadvantageEarnings,
                    model.SeptemberDisadvantageEarnings,
                    model.OctoberDisadvantageEarnings,
                    model.NovemberDisadvantageEarnings,
                    model.DecemberDisadvantageEarnings,
                    model.JanuaryDisadvantageEarnings,
                    model.FebruaryDisadvantageEarnings,
                    model.MarchDisadvantageEarnings,
                    model.AprilDisadvantageEarnings,
                    model.MayDisadvantageEarnings,
                    model.JuneDisadvantageEarnings,
                    model.JulyDisadvantageEarnings);

            model.Total1618AdditionalPaymentForEmployers =
                _totalBuilder.TotalRecords(
                    model.August1618AdditionalPaymentForEmployers,
                    model.September1618AdditionalPaymentForEmployers,
                    model.October1618AdditionalPaymentForEmployers,
                    model.November1618AdditionalPaymentForEmployers,
                    model.December1618AdditionalPaymentForEmployers,
                    model.January1618AdditionalPaymentForEmployers,
                    model.February1618AdditionalPaymentForEmployers,
                    model.March1618AdditionalPaymentForEmployers,
                    model.April1618AdditionalPaymentForEmployers,
                    model.May1618AdditionalPaymentForEmployers,
                    model.June1618AdditionalPaymentForEmployers,
                    model.July1618AdditionalPaymentForEmployers);

            model.Total1618AdditionalPaymentForProviders =
                _totalBuilder.TotalRecords(
                    model.August1618AdditionalPaymentForProviders,
                    model.September1618AdditionalPaymentForProviders,
                    model.October1618AdditionalPaymentForProviders,
                    model.November1618AdditionalPaymentForProviders,
                    model.December1618AdditionalPaymentForProviders,
                    model.January1618AdditionalPaymentForProviders,
                    model.February1618AdditionalPaymentForProviders,
                    model.March1618AdditionalPaymentForProviders,
                    model.April1618AdditionalPaymentForProviders,
                    model.May1618AdditionalPaymentForProviders,
                    model.June1618AdditionalPaymentForProviders,
                    model.July1618AdditionalPaymentForProviders);

            model.TotalAdditionalPaymentsForApprentices =
                _totalBuilder.TotalRecords(
                    model.AugustAdditionalPaymentsForApprentices,
                    model.SeptemberAdditionalPaymentsForApprentices,
                    model.OctoberAdditionalPaymentsForApprentices,
                    model.NovemberAdditionalPaymentsForApprentices,
                    model.DecemberAdditionalPaymentsForApprentices,
                    model.JanuaryAdditionalPaymentsForApprentices,
                    model.FebruaryAdditionalPaymentsForApprentices,
                    model.MarchAdditionalPaymentsForApprentices,
                    model.AprilAdditionalPaymentsForApprentices,
                    model.MayAdditionalPaymentsForApprentices,
                    model.JuneAdditionalPaymentsForApprentices,
                    model.JulyAdditionalPaymentsForApprentices);

            model.Total1618FrameworkUpliftOnProgrammePayment =
                _totalBuilder.TotalRecords(
                    model.August1618FrameworkUpliftOnProgrammePayment,
                    model.September1618FrameworkUpliftOnProgrammePayment,
                    model.October1618FrameworkUpliftOnProgrammePayment,
                    model.November1618FrameworkUpliftOnProgrammePayment,
                    model.December1618FrameworkUpliftOnProgrammePayment,
                    model.January1618FrameworkUpliftOnProgrammePayment,
                    model.February1618FrameworkUpliftOnProgrammePayment,
                    model.March1618FrameworkUpliftOnProgrammePayment,
                    model.April1618FrameworkUpliftOnProgrammePayment,
                    model.May1618FrameworkUpliftOnProgrammePayment,
                    model.June1618FrameworkUpliftOnProgrammePayment,
                    model.July1618FrameworkUpliftOnProgrammePayment);

            model.Total1618FrameworkUpliftBalancingPayment =
                _totalBuilder.TotalRecords(
                    model.August1618FrameworkUpliftBalancingPayment,
                    model.September1618FrameworkUpliftBalancingPayment,
                    model.October1618FrameworkUpliftBalancingPayment,
                    model.November1618FrameworkUpliftBalancingPayment,
                    model.December1618FrameworkUpliftBalancingPayment,
                    model.January1618FrameworkUpliftBalancingPayment,
                    model.February1618FrameworkUpliftBalancingPayment,
                    model.March1618FrameworkUpliftBalancingPayment,
                    model.April1618FrameworkUpliftBalancingPayment,
                    model.May1618FrameworkUpliftBalancingPayment,
                    model.June1618FrameworkUpliftBalancingPayment,
                    model.July1618FrameworkUpliftBalancingPayment);

            model.Total1618FrameworkUpliftCompletionPayment =
                _totalBuilder.TotalRecords(
                    model.August1618FrameworkUpliftCompletionPayment,
                    model.September1618FrameworkUpliftCompletionPayment,
                    model.October1618FrameworkUpliftCompletionPayment,
                    model.November1618FrameworkUpliftCompletionPayment,
                    model.December1618FrameworkUpliftCompletionPayment,
                    model.January1618FrameworkUpliftCompletionPayment,
                    model.February1618FrameworkUpliftCompletionPayment,
                    model.March1618FrameworkUpliftCompletionPayment,
                    model.April1618FrameworkUpliftCompletionPayment,
                    model.May1618FrameworkUpliftCompletionPayment,
                    model.June1618FrameworkUpliftCompletionPayment,
                    model.July1618FrameworkUpliftCompletionPayment);
        }

        private LearningDeliveryFamSimple[] GetLearningDeliveryFams(ILearningDelivery learningDelivery)
        {
            List<LearningDeliveryFamSimple> ret = new List<LearningDeliveryFamSimple>();

            ILearningDeliveryFAM[] lsfs = learningDelivery.LearningDeliveryFAMs
                ?.Where(x =>
                    string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeLSF, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (lsfs == null || !lsfs.Any())
            {
                ret.Add(_blankFam);
            }
            else
            {
                foreach (ILearningDeliveryFAM learningDeliveryFam in lsfs)
                {
                    ret.Add(new LearningDeliveryFamSimple(learningDeliveryFam.LearnDelFAMCode, learningDeliveryFam.LearnDelFAMDateFromNullable.GetValueOrDefault(DateTime.MinValue), learningDeliveryFam.LearnDelFAMDateToNullable.GetValueOrDefault(DateTime.MinValue)));
                }
            }

            return ret.ToArray();
        }
    }
}