using System;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public class AppsIndicativeEarningsModel
    {
        public string LearnRefNumber { get; set; }

        public long UniqueLearnerNumber { get; set; }

        public string DateOfBirth { get; set; }

        public string PostcodePriorToEnrollment { get; set; }

        public string CampusIdentifier { get; set; }

        public string ProviderSpecifiedLearnerMonitoringA { get; set; }

        public string ProviderSpecifiedLearnerMonitoringB { get; set; }

        public int? AimSeqNumber { get; set; }

        public string LearningAimReference { get; set; }

        public string LearningAimTitle { get; set; }

        public string SoftwareSupplierAimIdentifier { get; set; }

        public decimal? LARS1618FrameworkUplift { get; set; }

        public string NotionalNVQLevel { get; set; }

        public string StandardNotionalEndLevel { get; set; }

        public decimal? Tier2SectorSubjectArea { get; set; }

        public int? ProgrammeType { get; set; }

        public int? StandardCode { get; set; }

        public int? FrameworkCode { get; set; }

        public int? ApprenticeshipPathway { get; set; }

        public int AimType { get; set; }

        public int? CommonComponentCode { get; set; }

        public int FundingModel { get; set; }

        public string OriginalLearningStartDate { get; set; }

        public string LearningStartDate { get; set; }

        public string LearningPlannedEndDate { get; set; }

        public int CompletionStatus { get; set; }

        public string LearningActualEndDate { get; set; }

        public int? Outcome { get; set; }

        public int? FundingAdjustmentForPriorLearning { get; set; }

        public int? OtherFundingAdjustment { get; set; }

        public string LearningDeliveryFAMTypeLearningSupportFunding { get; set; }

        public string LearningDeliveryFAMTypeLSFDateAppliesFrom { get; set; }

        public string LearningDeliveryFAMTypeLSFDateAppliesTo { get; set; }

        public string LearningDeliveryFAMTypeLearningDeliveryMonitoringA { get; set; }

        public string LearningDeliveryFAMTypeLearningDeliveryMonitoringB { get; set; }

        public string LearningDeliveryFAMTypeLearningDeliveryMonitoringC { get; set; }

        public string LearningDeliveryFAMTypeLearningDeliveryMonitoringD { get; set; }

        public string LearningDeliveryFAMRestartIndicator { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringA { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringB { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringC { get; set; }

        public string ProviderSpecifiedDeliveryMonitoringD { get; set; }

        public string EndPointAssessmentOrganisation { get; set; }

        public int? PlannedNoOfOnProgrammeInstallmentsForAim { get; set; }

        public int? SubContractedOrPartnershipUKPRN { get; set; }

        public string DeliveryLocationPostcode { get; set; }

        public int? EmployerIdentifier { get; set; }

        public string AgreementIdentifier { get; set; }

        public int? EmploymentStatus { get; set; }

        public string EmploymentStatusDate { get; set; }

        public int? EmpStatusMonitoringSmallEmployer { get; set; }

        public DateTime? PriceEpisodeStartDate { get; set; }

        public string PriceEpisodeActualEndDate { get; set; }

        public string FundingLineType { get; set; }

        public decimal? TotalPriceApplicableToThisEpisode { get; set; }

        public decimal? FundingBandUpperLimit { get; set; }

        public decimal? PriceAmountAboveFundingBandLimit { get; set; }

        public decimal? PriceAmountRemainingStartOfEpisode { get; set; }

        public decimal? CompletionElement { get; set; }

        public decimal TotalPRMPreviousFundingYear { get; set; }

        public decimal TotalPRMThisFundingYear { get; set; }

        public string LearningDeliveryFAMTypeApprenticeshipContractType { get; set; }

        public string LearningDeliveryFAMTypeACTDateAppliesFrom { get; set; }

        public string LearningDeliveryFAMTypeACTDateAppliesTo { get; set; }

        public decimal? AugustOnProgrammeEarnings { get; set; }

        public decimal? AugustBalancingPaymentEarnings { get; set; }

        public decimal? AugustAimCompletionEarnings { get; set; }

        public decimal? AugustLearningSupportEarnings { get; set; }

        public decimal? AugustEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? AugustEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? AugustDisadvantageEarnings { get; set; }

        public decimal? August1618AdditionalPaymentForEmployers { get; set; }

        public decimal? August1618AdditionalPaymentForProviders { get; set; }

        public decimal? AugustAdditionalPaymentsForApprentices { get; set; }

        public decimal? August1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? August1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? August1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? SeptemberOnProgrammeEarnings { get; set; }

        public decimal? SeptemberBalancingPaymentEarnings { get; set; }

        public decimal? SeptemberAimCompletionEarnings { get; set; }

        public decimal? SeptemberLearningSupportEarnings { get; set; }

        public decimal? SeptemberEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? SeptemberEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? SeptemberDisadvantageEarnings { get; set; }

        public decimal? September1618AdditionalPaymentForEmployers { get; set; }

        public decimal? September1618AdditionalPaymentForProviders { get; set; }

        public decimal? SeptemberAdditionalPaymentsForApprentices { get; set; }

        public decimal? September1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? September1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? September1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? OctoberOnProgrammeEarnings { get; set; }

        public decimal? OctoberBalancingPaymentEarnings { get; set; }

        public decimal? OctoberAimCompletionEarnings { get; set; }

        public decimal? OctoberLearningSupportEarnings { get; set; }

        public decimal? OctoberEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? OctoberEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? OctoberDisadvantageEarnings { get; set; }

        public decimal? October1618AdditionalPaymentForEmployers { get; set; }

        public decimal? October1618AdditionalPaymentForProviders { get; set; }

        public decimal? OctoberAdditionalPaymentsForApprentices { get; set; }

        public decimal? October1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? October1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? October1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? NovemberOnProgrammeEarnings { get; set; }

        public decimal? NovemberBalancingPaymentEarnings { get; set; }

        public decimal? NovemberAimCompletionEarnings { get; set; }

        public decimal? NovemberLearningSupportEarnings { get; set; }

        public decimal? NovemberEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? NovemberEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? NovemberDisadvantageEarnings { get; set; }

        public decimal? November1618AdditionalPaymentForEmployers { get; set; }

        public decimal? November1618AdditionalPaymentForProviders { get; set; }

        public decimal? NovemberAdditionalPaymentsForApprentices { get; set; }

        public decimal? November1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? November1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? November1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? DecemberOnProgrammeEarnings { get; set; }

        public decimal? DecemberBalancingPaymentEarnings { get; set; }

        public decimal? DecemberAimCompletionEarnings { get; set; }

        public decimal? DecemberLearningSupportEarnings { get; set; }

        public decimal? DecemberEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? DecemberEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? DecemberDisadvantageEarnings { get; set; }

        public decimal? December1618AdditionalPaymentForEmployers { get; set; }

        public decimal? December1618AdditionalPaymentForProviders { get; set; }

        public decimal? DecemberAdditionalPaymentsForApprentices { get; set; }

        public decimal? December1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? December1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? December1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? JanuaryOnProgrammeEarnings { get; set; }

        public decimal? JanuaryBalancingPaymentEarnings { get; set; }

        public decimal? JanuaryAimCompletionEarnings { get; set; }

        public decimal? JanuaryLearningSupportEarnings { get; set; }

        public decimal? JanuaryEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? JanuaryEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? JanuaryDisadvantageEarnings { get; set; }

        public decimal? January1618AdditionalPaymentForEmployers { get; set; }

        public decimal? January1618AdditionalPaymentForProviders { get; set; }

        public decimal? JanuaryAdditionalPaymentsForApprentices { get; set; }

        public decimal? January1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? January1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? January1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? FebruaryOnProgrammeEarnings { get; set; }

        public decimal? FebruaryBalancingPaymentEarnings { get; set; }

        public decimal? FebruaryAimCompletionEarnings { get; set; }

        public decimal? FebruaryLearningSupportEarnings { get; set; }

        public decimal? FebruaryEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? FebruaryEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? FebruaryDisadvantageEarnings { get; set; }

        public decimal? February1618AdditionalPaymentForEmployers { get; set; }

        public decimal? February1618AdditionalPaymentForProviders { get; set; }

        public decimal? FebruaryAdditionalPaymentsForApprentices { get; set; }

        public decimal? February1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? February1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? February1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? MarchOnProgrammeEarnings { get; set; }

        public decimal? MarchBalancingPaymentEarnings { get; set; }

        public decimal? MarchAimCompletionEarnings { get; set; }

        public decimal? MarchLearningSupportEarnings { get; set; }

        public decimal? MarchEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? MarchEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? MarchDisadvantageEarnings { get; set; }

        public decimal? March1618AdditionalPaymentForEmployers { get; set; }

        public decimal? March1618AdditionalPaymentForProviders { get; set; }

        public decimal? MarchAdditionalPaymentsForApprentices { get; set; }

        public decimal? March1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? March1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? March1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? AprilOnProgrammeEarnings { get; set; }

        public decimal? AprilBalancingPaymentEarnings { get; set; }

        public decimal? AprilAimCompletionEarnings { get; set; }

        public decimal? AprilLearningSupportEarnings { get; set; }

        public decimal? AprilEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? AprilEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? AprilDisadvantageEarnings { get; set; }

        public decimal? April1618AdditionalPaymentForEmployers { get; set; }

        public decimal? April1618AdditionalPaymentForProviders { get; set; }

        public decimal? AprilAdditionalPaymentsForApprentices { get; set; }

        public decimal? April1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? April1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? April1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? MayOnProgrammeEarnings { get; set; }

        public decimal? MayBalancingPaymentEarnings { get; set; }

        public decimal? MayAimCompletionEarnings { get; set; }

        public decimal? MayLearningSupportEarnings { get; set; }

        public decimal? MayEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? MayEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? MayDisadvantageEarnings { get; set; }

        public decimal? May1618AdditionalPaymentForEmployers { get; set; }

        public decimal? May1618AdditionalPaymentForProviders { get; set; }

        public decimal? MayAdditionalPaymentsForApprentices { get; set; }

        public decimal? May1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? May1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? May1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? JuneOnProgrammeEarnings { get; set; }

        public decimal? JuneBalancingPaymentEarnings { get; set; }

        public decimal? JuneAimCompletionEarnings { get; set; }

        public decimal? JuneLearningSupportEarnings { get; set; }

        public decimal? JuneEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? JuneEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? JuneDisadvantageEarnings { get; set; }

        public decimal? June1618AdditionalPaymentForEmployers { get; set; }

        public decimal? June1618AdditionalPaymentForProviders { get; set; }

        public decimal? JuneAdditionalPaymentsForApprentices { get; set; }

        public decimal? June1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? June1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? June1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? JulyOnProgrammeEarnings { get; set; }

        public decimal? JulyBalancingPaymentEarnings { get; set; }

        public decimal? JulyAimCompletionEarnings { get; set; }

        public decimal? JulyLearningSupportEarnings { get; set; }

        public decimal? JulyEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? JulyEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? JulyDisadvantageEarnings { get; set; }

        public decimal? July1618AdditionalPaymentForEmployers { get; set; }

        public decimal? July1618AdditionalPaymentForProviders { get; set; }

        public decimal? JulyAdditionalPaymentsForApprentices { get; set; }

        public decimal? July1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? July1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? July1618FrameworkUpliftCompletionPayment { get; set; }

        public decimal? TotalOnProgrammeEarnings { get; set; }

        public decimal? TotalBalancingPaymentEarnings { get; set; }

        public decimal? TotalAimCompletionEarnings { get; set; }

        public decimal? TotalLearningSupportEarnings { get; set; }

        public decimal? TotalEnglishMathsOnProgrammeEarnings { get; set; }

        public decimal? TotalEnglishMathsBalancingPaymentEarnings { get; set; }

        public decimal? TotalDisadvantageEarnings { get; set; }

        public decimal? Total1618AdditionalPaymentForEmployers { get; set; }

        public decimal? Total1618AdditionalPaymentForProviders { get; set; }

        public decimal? TotalAdditionalPaymentsForApprentices { get; set; }

        public decimal? Total1618FrameworkUpliftOnProgrammePayment { get; set; }

        public decimal? Total1618FrameworkUpliftBalancingPayment { get; set; }

        public decimal? Total1618FrameworkUpliftCompletionPayment { get; set; }

        public string OfficialSensitive { get; }
    }
}
