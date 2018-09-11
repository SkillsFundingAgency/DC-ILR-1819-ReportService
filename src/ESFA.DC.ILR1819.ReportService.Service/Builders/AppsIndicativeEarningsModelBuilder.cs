﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.Data.LARS.Model;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Attribute;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Lars;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Builders
{
    public class AppsIndicativeEarningsModelBuilder
    {
        private readonly IList<IAppsIndicativeCommand> _commands;

        public AppsIndicativeEarningsModelBuilder(IList<IAppsIndicativeCommand> commands)
        {
            _commands = commands;
        }

        public AppsIndicativeEarningsModel BuildModel(
            ILearner learner,
            ILearningDelivery learningDelivery,
            LearningDeliveryAttribute fm36DeliveryAttribute,
            PriceEpisodeAttribute fm36EpisodeAttribute,
            LarsLearningDelivery larsLearningDelivery,
            LARS_Standard larsStandard,
            bool earliestEpisode,
            bool hasPriceEpisodes)
        {
            var employmentStatusDate = learner.LearnerEmploymentStatuses
                .Where(x => x.DateEmpStatApp <= learningDelivery.LearnStartDate).Max(x => x.DateEmpStatApp);
            var employmentStatus =
                learner.LearnerEmploymentStatuses.SingleOrDefault(x => x.DateEmpStatApp == employmentStatusDate);

            var model = new AppsIndicativeEarningsModel
            {
                LearnerReferenceNumber = learner.LearnRefNumber,
                UniqueLearnerNumber = learner.ULN,
                DateOfBirth = learner.DateOfBirthNullable,
                PostcodePriorToEnrollment = learner.PostcodePrior,
                CampusIdentifier = learner.CampId,
                ProviderSpecifiedLearnerMonitoringA = learner.ProviderSpecLearnerMonitorings
                    .SingleOrDefault(x => x.ProvSpecLearnMonOccur == "A")?.ProvSpecLearnMon,
                ProviderSpecifiedLearnerMonitoringB = learner.ProviderSpecLearnerMonitorings
                    .SingleOrDefault(x => x.ProvSpecLearnMonOccur == "B")?.ProvSpecLearnMon,
                AimSequenceNumber = learningDelivery.AimSeqNumber,
                LearningAimReference = learningDelivery.LearnAimRef,
                LearningAimTitle = larsLearningDelivery.LearningAimTitle,
                SoftwareSupplierAimIdentifier = learningDelivery.SWSupAimId,
                LARS1618FrameworkUplift = fm36DeliveryAttribute.LearningDeliveryAttributeDatas
                    .LearnDelApplicProv1618FrameworkUplift,
                NotionalNVQLevel = larsLearningDelivery.NotionalNvqLevel,
                StandardNotionalEndLevel = larsStandard.NotionalEndLevel,
                Tier2SectorSubjectArea = larsLearningDelivery.Tier2SectorSubjectArea,
                ProgrammeType = learningDelivery.ProgTypeNullable,
                StandardCode = learningDelivery.StdCodeNullable,
                FrameworkCode = learningDelivery.FworkCodeNullable,
                ApprenticeshipPathway = learningDelivery.PwayCodeNullable,
                AimType = learningDelivery.AimType,
                CommonComponentCode = larsLearningDelivery.FrameworkCommonComponent,
                FundingModel = learningDelivery.FundModel,
                OriginalLearningStartDate = learningDelivery.OrigLearnStartDateNullable,
                LearningStartDate = learningDelivery.LearnStartDate,
                LearningPlannedEndDate = learningDelivery.LearnPlanEndDate,
                CompletionStatus = learningDelivery.CompStatus,
                LearningActualEndDate = learningDelivery.LearnActEndDateNullable,
                Outcome = learningDelivery.OutcomeNullable,
                FundingAdjustmentForPriorLearning = learningDelivery.PriorLearnFundAdjNullable,
                OtherFundingAdjustment = learningDelivery.OtherFundAdjNullable,
                LearningDeliveryFAMTypeLearningSupportFunding = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLSF)?.LearnDelFAMCode,
                LearningDeliveryFAMTypeLSFDateAppliesFrom = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLSF)
                    ?.LearnDelFAMDateFromNullable,
                LearningDeliveryFAMTypeLSFDateAppliesTo = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLSF)
                    ?.LearnDelFAMDateToNullable,
                LearningDeliveryFAMTypeLearningDeliveryMonitoringA = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLDM1)?.LearnDelFAMCode,
                LearningDeliveryFAMTypeLearningDeliveryMonitoringB = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLDM2)?.LearnDelFAMCode,
                LearningDeliveryFAMTypeLearningDeliveryMonitoringC = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLDM3)?.LearnDelFAMCode,
                LearningDeliveryFAMTypeLearningDeliveryMonitoringD = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeLDM4)?.LearnDelFAMCode,
                LearningDeliveryFAMRestartIndicator = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeRES).LearnDelFAMCode,
                ProviderSpecifiedDeliveryMonitoringA = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => x.ProvSpecLearnMonOccur == "A")?.ProvSpecLearnMon,
                ProviderSpecifiedDeliveryMonitoringB = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => x.ProvSpecLearnMonOccur == "B")?.ProvSpecLearnMon,
                ProviderSpecifiedDeliveryMonitoringC = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => x.ProvSpecLearnMonOccur == "C")?.ProvSpecLearnMon,
                ProviderSpecifiedDeliveryMonitoringD = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => x.ProvSpecLearnMonOccur == "D")?.ProvSpecLearnMon,
                EndPointAssessmentOrganisation = learningDelivery.EPAOrgID,
                PlannedNoOfOnProgrammeInstallmentsForAim =
                    fm36DeliveryAttribute.LearningDeliveryAttributeDatas.PlannedNumOnProgInstalm,
                SubContractedOrPartnershipUKPRN = learningDelivery.PartnerUKPRNNullable,
                DeliveryLocationPostcode = learningDelivery.DelLocPostCode,
                EmployerIdentifier = employmentStatus?.EmpIdNullable,
                AgreementIdentifier = fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeAgreeId,
                EmploymentStatus = employmentStatus?.EmpStat,
                EmploymentStatusDate = employmentStatus?.DateEmpStatApp,
                EmpStatusMonitoringSmallEmployer = employmentStatus.EmploymentStatusMonitorings
                    .SingleOrDefault(x => x.ESMType == Constants.EmploymentStatusMonitoringTypeSEM)?.ESMCode,
                PriceEpisodeStartDate = fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.EpisodeStartDate,
                PriceEpisodeActualEndDate = fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeActualEndDate,
                FundingLineType = fm36DeliveryAttribute.LearningDeliveryAttributeDatas.LearnDelMathEng ?? false
                    ? fm36DeliveryAttribute?.LearningDeliveryAttributeDatas.LearnDelInitialFundLineType
                    : fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeFundLineType,
                TotalPriceApplicableToThisEpisode =
                    fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeTotalTNPPrice,
                FundingBandUpperLimit = fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeUpperBandLimit,
                PriceAmountAboveFundingBandLimit =
                    fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeUpperLimitAdjustment,
                PriceAmountRemainingStartOfEpisode = fm36EpisodeAttribute?.PriceEpisodeAttributeDatas
                    .PriceEpisodeCappedRemainingTNPAmount,
                CompletionElement = fm36EpisodeAttribute?.PriceEpisodeAttributeDatas.PriceEpisodeCompletionElement
            };

            CalculateApprenticeshipContractTypeFields(learningDelivery, model, fm36DeliveryAttribute, fm36EpisodeAttribute, hasPriceEpisodes);

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

        private void CalculateAppFinTotals(AppsIndicativeEarningsModel model, ILearningDelivery learningDelivery)
        {
            var firstOfAugust = new DateTime(2018, 8, 1);
            var endOfYear = new DateTime(2019, 7, 31, 23, 59, 59);

            var previousYearData = learningDelivery.AppFinRecords
                .Where(x => x.AFinDate < firstOfAugust && x.AFinType == "PMR").ToList();
            var currentYearData = learningDelivery.AppFinRecords
                .Where(x => x.AFinDate >= firstOfAugust && x.AFinDate <= endOfYear && x.AFinType == "PMR").ToList();

            model.TotalPRMPreviousFundingYear =
                previousYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                previousYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            model.TotalPRMThisFundingYear =
                currentYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);
        }

        private void CalculateApprenticeshipContractTypeFields(
            ILearningDelivery learningDelivery,
            AppsIndicativeEarningsModel model,
            LearningDeliveryAttribute fm36DeliveryAttribute,
            PriceEpisodeAttribute fm36PriceEpisodeAttribute,
            bool hasPriceEpisodes)
        {
            bool useDeliveryAttributeDate =
                fm36DeliveryAttribute.LearningDeliveryAttributeDatas.LearnDelMathEng ?? false ||
                (!(fm36DeliveryAttribute.LearningDeliveryAttributeDatas.LearnDelMathEng ?? false) && !hasPriceEpisodes);

            if (learningDelivery.LearningDeliveryFAMs.SingleOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeACT)
                    ?.LearnDelFAMDateFromNullable == null)
            {
                return;
            }

            var contractAppliesFrom = learningDelivery.LearningDeliveryFAMs
                .Where(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeACT &&
                            ((useDeliveryAttributeDate &&
                              learningDelivery.LearnStartDate >= x.LearnDelFAMDateFromNullable) ||
                             (!useDeliveryAttributeDate &&
                              fm36PriceEpisodeAttribute?.PriceEpisodeAttributeDatas.EpisodeStartDate >=
                              x.LearnDelFAMDateFromNullable)))
                .Max(x => x.LearnDelFAMDateFromNullable);

            var contractAppliesTo = learningDelivery.LearningDeliveryFAMs
                .Where(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeACT &&
                            ((useDeliveryAttributeDate &&
                              learningDelivery.LearnStartDate <= x.LearnDelFAMDateToNullable) ||
                             (!useDeliveryAttributeDate &&
                              fm36PriceEpisodeAttribute?.PriceEpisodeAttributeDatas.EpisodeStartDate <=
                              x.LearnDelFAMDateToNullable)))
                .Min(x => x.LearnDelFAMDateFromNullable);

            model.LearningDeliveryFAMTypeACTDateAppliesFrom = contractAppliesFrom;
            model.LearningDeliveryFAMTypeACTDateAppliesTo = contractAppliesTo;

            model.LearningDeliveryFAMTypeApprenticeshipContractType = learningDelivery.LearningDeliveryFAMs
                .FirstOrDefault(x => x.LearnDelFAMType == Constants.LearningDeliveryFAMCodeACT &&
                    x.LearnDelFAMDateFromNullable == contractAppliesFrom && x.LearnDelFAMDateToNullable == contractAppliesTo)?.LearnDelFAMCode;
        }

        private void GetTotals(AppsIndicativeEarningsModel model)
        {
            model.TotalOnProgrammeEarnings =
                model.AugustOnProgrammeEarnings +
                model.SeptemberOnProgrammeEarnings +
                model.OctoberOnProgrammeEarnings +
                model.NovemberOnProgrammeEarnings +
                model.DecemberOnProgrammeEarnings +
                model.JanuaryOnProgrammeEarnings +
                model.FebruaryOnProgrammeEarnings +
                model.MarchOnProgrammeEarnings +
                model.AprilOnProgrammeEarnings +
                model.MayOnProgrammeEarnings +
                model.JuneOnProgrammeEarnings +
                model.JulyOnProgrammeEarnings;

            model.TotalBalancingPaymentEarnings =
                model.AugustBalancingPaymentEarnings +
                model.SeptemberBalancingPaymentEarnings +
                model.OctoberBalancingPaymentEarnings +
                model.NovemberBalancingPaymentEarnings +
                model.DecemberBalancingPaymentEarnings +
                model.JanuaryBalancingPaymentEarnings +
                model.FebruaryBalancingPaymentEarnings +
                model.MarchBalancingPaymentEarnings +
                model.AprilBalancingPaymentEarnings +
                model.MayBalancingPaymentEarnings +
                model.JuneBalancingPaymentEarnings +
                model.JulyBalancingPaymentEarnings;

            model.TotalAimCompletionEarnings =
                model.AugustAimCompletionEarnings +
                model.SeptemberAimCompletionEarnings +
                model.OctoberAimCompletionEarnings +
                model.NovemberAimCompletionEarnings +
                model.DecemberAimCompletionEarnings +
                model.JanuaryAimCompletionEarnings +
                model.FebruaryAimCompletionEarnings +
                model.MarchAimCompletionEarnings +
                model.AprilAimCompletionEarnings +
                model.MayAimCompletionEarnings +
                model.JuneAimCompletionEarnings +
                model.JulyAimCompletionEarnings;

            model.TotalLearningSupportEarnings =
                model.AugustLearningSupportEarnings +
                model.SeptemberLearningSupportEarnings +
                model.OctoberLearningSupportEarnings +
                model.NovemberLearningSupportEarnings +
                model.DecemberLearningSupportEarnings +
                model.JanuaryLearningSupportEarnings +
                model.FebruaryLearningSupportEarnings +
                model.MarchLearningSupportEarnings +
                model.AprilLearningSupportEarnings +
                model.MayLearningSupportEarnings +
                model.JuneLearningSupportEarnings +
                model.JulyLearningSupportEarnings;

            model.TotalEnglishMathsOnProgrammeEarnings =
                model.AugustEnglishMathsOnProgrammeEarnings +
                model.SeptemberEnglishMathsOnProgrammeEarnings +
                model.OctoberEnglishMathsOnProgrammeEarnings +
                model.NovemberEnglishMathsOnProgrammeEarnings +
                model.DecemberEnglishMathsOnProgrammeEarnings +
                model.JanuaryEnglishMathsOnProgrammeEarnings +
                model.FebruaryEnglishMathsOnProgrammeEarnings +
                model.MarchEnglishMathsOnProgrammeEarnings +
                model.AprilEnglishMathsOnProgrammeEarnings +
                model.MayEnglishMathsOnProgrammeEarnings +
                model.JuneEnglishMathsOnProgrammeEarnings +
                model.JulyEnglishMathsOnProgrammeEarnings;

            model.TotalEnglishMathsBalancingPaymentEarnings =
                model.AugustEnglishMathsBalancingPaymentEarnings +
                model.SeptemberEnglishMathsBalancingPaymentEarnings +
                model.OctoberEnglishMathsBalancingPaymentEarnings +
                model.NovemberEnglishMathsBalancingPaymentEarnings +
                model.DecemberEnglishMathsBalancingPaymentEarnings +
                model.JanuaryEnglishMathsBalancingPaymentEarnings +
                model.FebruaryEnglishMathsBalancingPaymentEarnings +
                model.MarchEnglishMathsBalancingPaymentEarnings +
                model.AprilEnglishMathsBalancingPaymentEarnings +
                model.MayEnglishMathsBalancingPaymentEarnings +
                model.JuneEnglishMathsBalancingPaymentEarnings +
                model.JulyEnglishMathsBalancingPaymentEarnings;

            model.TotalDisadvantageEarnings =
                model.AugustDisadvantageEarnings +
                model.SeptemberDisadvantageEarnings +
                model.OctoberDisadvantageEarnings +
                model.NovemberDisadvantageEarnings +
                model.DecemberDisadvantageEarnings +
                model.JanuaryDisadvantageEarnings +
                model.FebruaryDisadvantageEarnings +
                model.MarchDisadvantageEarnings +
                model.AprilDisadvantageEarnings +
                model.MayDisadvantageEarnings +
                model.JuneDisadvantageEarnings +
                model.JulyDisadvantageEarnings;

            model.Total1618AdditionalPaymentForEmployers =
                model.August1618AdditionalPaymentForEmployers +
                model.September1618AdditionalPaymentForEmployers +
                model.October1618AdditionalPaymentForEmployers +
                model.November1618AdditionalPaymentForEmployers +
                model.December1618AdditionalPaymentForEmployers +
                model.January1618AdditionalPaymentForEmployers +
                model.February1618AdditionalPaymentForEmployers +
                model.March1618AdditionalPaymentForEmployers +
                model.April1618AdditionalPaymentForEmployers +
                model.May1618AdditionalPaymentForEmployers +
                model.June1618AdditionalPaymentForEmployers +
                model.July1618AdditionalPaymentForEmployers;

            model.Total1618AdditionalPaymentForProviders =
                model.August1618AdditionalPaymentForProviders +
                model.September1618AdditionalPaymentForProviders +
                model.October1618AdditionalPaymentForProviders +
                model.November1618AdditionalPaymentForProviders +
                model.December1618AdditionalPaymentForProviders +
                model.January1618AdditionalPaymentForProviders +
                model.February1618AdditionalPaymentForProviders +
                model.March1618AdditionalPaymentForProviders +
                model.April1618AdditionalPaymentForProviders +
                model.May1618AdditionalPaymentForProviders +
                model.June1618AdditionalPaymentForProviders +
                model.July1618AdditionalPaymentForProviders;

            model.TotalAdditionalPaymentsForApprentices =
                model.AugustAdditionalPaymentsForApprentices +
                model.SeptemberAdditionalPaymentsForApprentices +
                model.OctoberAdditionalPaymentsForApprentices +
                model.NovemberAdditionalPaymentsForApprentices +
                model.DecemberAdditionalPaymentsForApprentices +
                model.JanuaryAdditionalPaymentsForApprentices +
                model.FebruaryAdditionalPaymentsForApprentices +
                model.MarchAdditionalPaymentsForApprentices +
                model.AprilAdditionalPaymentsForApprentices +
                model.MayAdditionalPaymentsForApprentices +
                model.JuneAdditionalPaymentsForApprentices +
                model.JulyAdditionalPaymentsForApprentices;

            model.Total1618FrameworkUpliftOnProgrammePayment =
                model.August1618FrameworkUpliftOnProgrammePayment +
                model.September1618FrameworkUpliftOnProgrammePayment +
                model.October1618FrameworkUpliftOnProgrammePayment +
                model.November1618FrameworkUpliftOnProgrammePayment +
                model.December1618FrameworkUpliftOnProgrammePayment +
                model.January1618FrameworkUpliftOnProgrammePayment +
                model.February1618FrameworkUpliftOnProgrammePayment +
                model.March1618FrameworkUpliftOnProgrammePayment +
                model.April1618FrameworkUpliftOnProgrammePayment +
                model.May1618FrameworkUpliftOnProgrammePayment +
                model.June1618FrameworkUpliftOnProgrammePayment +
                model.July1618FrameworkUpliftOnProgrammePayment;

            model.Total1618FrameworkUpliftBalancingPayment =
                model.August1618FrameworkUpliftBalancingPayment +
                model.September1618FrameworkUpliftBalancingPayment +
                model.October1618FrameworkUpliftBalancingPayment +
                model.November1618FrameworkUpliftBalancingPayment +
                model.December1618FrameworkUpliftBalancingPayment +
                model.January1618FrameworkUpliftBalancingPayment +
                model.February1618FrameworkUpliftBalancingPayment +
                model.March1618FrameworkUpliftBalancingPayment +
                model.April1618FrameworkUpliftBalancingPayment +
                model.May1618FrameworkUpliftBalancingPayment +
                model.June1618FrameworkUpliftBalancingPayment +
                model.July1618FrameworkUpliftBalancingPayment;

            model.Total1618FrameworkUpliftCompletionPayment =
                model.August1618FrameworkUpliftCompletionPayment +
                model.September1618FrameworkUpliftCompletionPayment +
                model.October1618FrameworkUpliftCompletionPayment +
                model.November1618FrameworkUpliftCompletionPayment +
                model.December1618FrameworkUpliftCompletionPayment +
                model.January1618FrameworkUpliftCompletionPayment +
                model.February1618FrameworkUpliftCompletionPayment +
                model.March1618FrameworkUpliftCompletionPayment +
                model.April1618FrameworkUpliftCompletionPayment +
                model.May1618FrameworkUpliftCompletionPayment +
                model.June1618FrameworkUpliftCompletionPayment +
                model.July1618FrameworkUpliftCompletionPayment;
        }
    }
}