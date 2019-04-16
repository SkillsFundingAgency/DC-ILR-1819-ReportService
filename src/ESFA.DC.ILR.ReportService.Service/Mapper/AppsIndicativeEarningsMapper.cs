using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Mapper
{
    public sealed class AppsIndicativeEarningsMapper : ClassMap<AppsIndicativeEarningsModel>, IClassMapper
    {
        public AppsIndicativeEarningsMapper()
        {
            int i = 0;
            Map(m => m.LearnRefNumber).Index(i++).Name("Learner reference number");
            Map(m => m.UniqueLearnerNumber).Index(i++).Name("Unique learner number");
            Map(m => m.DateOfBirth).Index(i++).Name("Date of birth");
            Map(m => m.PostcodePriorToEnrollment).Index(i++).Name("Postcode prior to enrolment");
            Map(m => m.CampusIdentifier).Index(i++).Name("Campus identifier");
            Map(m => m.ProviderSpecifiedLearnerMonitoringA).Index(i++).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProviderSpecifiedLearnerMonitoringB).Index(i++).Name("Provider specified learner monitoring (B)");
            Map(m => m.AimSeqNumber).Index(i++).Name("Aim sequence number");
            Map(m => m.LearningAimReference).Index(i++).Name("Learning aim reference");
            Map(m => m.LearningAimTitle).Index(i++).Name("Learning aim title");
            Map(m => m.SoftwareSupplierAimIdentifier).Index(i++).Name("Software supplier aim identifier");
            Map(m => m.LARS1618FrameworkUplift).Index(i++).Name("LARS 16-18 framework uplift").TypeConverterOption.Format("0.00000");
            Map(m => m.NotionalNVQLevel).Index(i++).Name("Notional NVQ level");
            Map(m => m.StandardNotionalEndLevel).Index(i++).Name("Standard notional end level");

            Map(m => m.Tier2SectorSubjectArea).Index(i++).Name("Tier 2 sector subject area");
            Map(m => m.ProgrammeType).Index(i++).Name("Programme type");
            Map(m => m.StandardCode).Index(i++).Name("Standard code");
            Map(m => m.FrameworkCode).Index(i++).Name("Framework code");
            Map(m => m.ApprenticeshipPathway).Index(i++).Name("Apprenticeship pathway");
            Map(m => m.AimType).Index(i++).Name("Aim type");
            Map(m => m.CommonComponentCode).Index(i++).Name("Common component code");
            Map(m => m.FundingModel).Index(i++).Name("Funding model");
            Map(m => m.OriginalLearningStartDate).Index(i++).Name("Original learning start date");
            Map(m => m.LearningStartDate).Index(i++).Name("Learning start date");
            Map(m => m.LearningPlannedEndDate).Index(i++).Name("Learning planned end date");
            Map(m => m.CompletionStatus).Index(i++).Name("Completion status");
            Map(m => m.LearningActualEndDate).Index(i++).Name("Learning actual end date");
            Map(m => m.Outcome).Index(i++).Name("Outcome");
            Map(m => m.FundingAdjustmentForPriorLearning).Index(i++).Name("Funding adjustment for prior learning");
            Map(m => m.OtherFundingAdjustment).Index(i++).Name("Other funding adjustment");
            Map(m => m.LearningDeliveryFAMTypeLearningSupportFunding).Index(i++).Name("Learning delivery funding and monitoring type - learning support funding (if applicable)");
            Map(m => m.LearningDeliveryFAMTypeLSFDateAppliesFrom).Index(i++).Name("Learning delivery funding and monitoring type - LSF date applies from (earliest)");
            Map(m => m.LearningDeliveryFAMTypeLSFDateAppliesTo).Index(i++).Name("Learning delivery funding and monitoring type - LSF date applies to (latest)");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringA).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (A)");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringB).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (B)");
            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringC).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (C)");

            Map(m => m.LearningDeliveryFAMTypeLearningDeliveryMonitoringD).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (D)");
            Map(m => m.LearningDeliveryFAMRestartIndicator).Index(i++).Name("Learning delivery funding and monitoring type - restart indicator");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringA).Index(i++).Name("Provider specified delivery monitoring (A)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringB).Index(i++).Name("Provider specified delivery monitoring (B)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringC).Index(i++).Name("Provider specified delivery monitoring (C)");
            Map(m => m.ProviderSpecifiedDeliveryMonitoringD).Index(i++).Name("Provider specified delivery monitoring (D)");
            Map(m => m.EndPointAssessmentOrganisation).Index(i++).Name("End point assessment organisation");
            Map(m => m.PlannedNoOfOnProgrammeInstallmentsForAim).Index(i++).Name("Planned number of on programme instalments for aim");
            Map(m => m.SubContractedOrPartnershipUKPRN).Index(i++).Name("Sub contracted or partnership UKPRN");
            Map(m => m.DeliveryLocationPostcode).Index(i++).Name("Delivery location postcode");
            Map(m => m.EmployerIdentifier).Index(i++).Name("Employer identifier");
            Map(m => m.AgreementIdentifier).Index(i++).Name("Agreement identifier");
            Map(m => m.EmploymentStatus).Index(i++).Name("Employment status");
            Map(m => m.EmploymentStatusDate).Index(i++).Name("Employment status date");
            Map(m => m.EmpStatusMonitoringSmallEmployer).Index(i++).Name("Employment status monitoring - small employer");
            Map(m => m.PriceEpisodeStartDate).Index(i++).Name("Price episode start date").TypeConverterOption.Format("dd/MM/yyyy");
            Map(m => m.PriceEpisodeActualEndDate).Index(i++).Name("Price episode actual end date");

            Map(m => m.FundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.TotalPriceApplicableToThisEpisode).Index(i++).Name("Total price applicable to this episode").TypeConverterOption.Format("0.00000");
            Map(m => m.FundingBandUpperLimit).Index(i++).Name("Funding band upper limit").TypeConverterOption.Format("0.00000");
            Map(m => m.PriceAmountAboveFundingBandLimit).Index(i++).Name("Price amount above funding band limit").TypeConverterOption.Format("0.00000");
            Map(m => m.PriceAmountRemainingStartOfEpisode).Index(i++).Name("Price amount remaining (with upper limit applied) at start of this episode").TypeConverterOption.Format("0.00000");
            Map(m => m.CompletionElement).Index(i++).Name("Completion element (potential or actual earnings)").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalPRMPreviousFundingYear).Index(i++).Name("Total employer contribution collected (PMR) in previous funding years").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalPRMThisFundingYear).Index(i++).Name("Total employer contribution collected (PMR) in this funding year").TypeConverterOption.Format("0.00000");

            Map(m => m.LearningDeliveryFAMTypeApprenticeshipContractType).Index(i++).Name("Learning delivery funding and monitoring type - apprenticeship contract type");
            Map(m => m.LearningDeliveryFAMTypeACTDateAppliesFrom).Index(i++).Name("Learning delivery funding and monitoring type - ACT date applies from");
            Map(m => m.LearningDeliveryFAMTypeACTDateAppliesTo).Index(i++).Name("Learning delivery funding and monitoring type - ACT date applies to");

            Map(m => m.AugustOnProgrammeEarnings).Index(i++).Name("August on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustBalancingPaymentEarnings).Index(i++).Name("August balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustAimCompletionEarnings).Index(i++).Name("August aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustLearningSupportEarnings).Index(i++).Name("August learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustEnglishMathsOnProgrammeEarnings).Index(i++).Name("August English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustEnglishMathsBalancingPaymentEarnings).Index(i++).Name("August English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustDisadvantageEarnings).Index(i++).Name("August disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.August1618AdditionalPaymentForEmployers).Index(i++).Name("August 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.August1618AdditionalPaymentForProviders).Index(i++).Name("August 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.AugustAdditionalPaymentsForApprentices).Index(i++).Name("August additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.August1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("August 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.August1618FrameworkUpliftBalancingPayment).Index(i++).Name("August 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.August1618FrameworkUpliftCompletionPayment).Index(i++).Name("August 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.SeptemberOnProgrammeEarnings).Index(i++).Name("September on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberBalancingPaymentEarnings).Index(i++).Name("September balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberAimCompletionEarnings).Index(i++).Name("September aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberLearningSupportEarnings).Index(i++).Name("September learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberEnglishMathsOnProgrammeEarnings).Index(i++).Name("September English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberEnglishMathsBalancingPaymentEarnings).Index(i++).Name("September English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberDisadvantageEarnings).Index(i++).Name("September disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.September1618AdditionalPaymentForEmployers).Index(i++).Name("September 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.September1618AdditionalPaymentForProviders).Index(i++).Name("September 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.SeptemberAdditionalPaymentsForApprentices).Index(i++).Name("September additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.September1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("September 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.September1618FrameworkUpliftBalancingPayment).Index(i++).Name("September 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.September1618FrameworkUpliftCompletionPayment).Index(i++).Name("September 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.OctoberOnProgrammeEarnings).Index(i++).Name("October on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberBalancingPaymentEarnings).Index(i++).Name("October balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberAimCompletionEarnings).Index(i++).Name("October aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberLearningSupportEarnings).Index(i++).Name("October learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberEnglishMathsOnProgrammeEarnings).Index(i++).Name("October English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberEnglishMathsBalancingPaymentEarnings).Index(i++).Name("October English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberDisadvantageEarnings).Index(i++).Name("October disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.October1618AdditionalPaymentForEmployers).Index(i++).Name("October 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.October1618AdditionalPaymentForProviders).Index(i++).Name("October 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.OctoberAdditionalPaymentsForApprentices).Index(i++).Name("October additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.October1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("October 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.October1618FrameworkUpliftBalancingPayment).Index(i++).Name("October 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.October1618FrameworkUpliftCompletionPayment).Index(i++).Name("October 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.NovemberOnProgrammeEarnings).Index(i++).Name("November on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberBalancingPaymentEarnings).Index(i++).Name("November balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberAimCompletionEarnings).Index(i++).Name("November aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberLearningSupportEarnings).Index(i++).Name("November learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberEnglishMathsOnProgrammeEarnings).Index(i++).Name("November English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberEnglishMathsBalancingPaymentEarnings).Index(i++).Name("November English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberDisadvantageEarnings).Index(i++).Name("November disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.November1618AdditionalPaymentForEmployers).Index(i++).Name("November 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.November1618AdditionalPaymentForProviders).Index(i++).Name("November 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.NovemberAdditionalPaymentsForApprentices).Index(i++).Name("November additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.November1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("November 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.November1618FrameworkUpliftBalancingPayment).Index(i++).Name("November 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.November1618FrameworkUpliftCompletionPayment).Index(i++).Name("November 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.DecemberOnProgrammeEarnings).Index(i++).Name("December on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberBalancingPaymentEarnings).Index(i++).Name("December balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberAimCompletionEarnings).Index(i++).Name("December aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberLearningSupportEarnings).Index(i++).Name("December learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberEnglishMathsOnProgrammeEarnings).Index(i++).Name("December English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberEnglishMathsBalancingPaymentEarnings).Index(i++).Name("December English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberDisadvantageEarnings).Index(i++).Name("December disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.December1618AdditionalPaymentForEmployers).Index(i++).Name("December 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.December1618AdditionalPaymentForProviders).Index(i++).Name("December 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.DecemberAdditionalPaymentsForApprentices).Index(i++).Name("December additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.December1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("December 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.December1618FrameworkUpliftBalancingPayment).Index(i++).Name("December 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.December1618FrameworkUpliftCompletionPayment).Index(i++).Name("December 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.JanuaryOnProgrammeEarnings).Index(i++).Name("January on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryBalancingPaymentEarnings).Index(i++).Name("January balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryAimCompletionEarnings).Index(i++).Name("January aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryLearningSupportEarnings).Index(i++).Name("January learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryEnglishMathsOnProgrammeEarnings).Index(i++).Name("January English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryEnglishMathsBalancingPaymentEarnings).Index(i++).Name("January English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryDisadvantageEarnings).Index(i++).Name("January disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.January1618AdditionalPaymentForEmployers).Index(i++).Name("January 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.January1618AdditionalPaymentForProviders).Index(i++).Name("January 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.JanuaryAdditionalPaymentsForApprentices).Index(i++).Name("January additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.January1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("January 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.January1618FrameworkUpliftBalancingPayment).Index(i++).Name("January 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.January1618FrameworkUpliftCompletionPayment).Index(i++).Name("January 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.FebruaryOnProgrammeEarnings).Index(i++).Name("February on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryBalancingPaymentEarnings).Index(i++).Name("February balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryAimCompletionEarnings).Index(i++).Name("February aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryLearningSupportEarnings).Index(i++).Name("February learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryEnglishMathsOnProgrammeEarnings).Index(i++).Name("February English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryEnglishMathsBalancingPaymentEarnings).Index(i++).Name("February English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryDisadvantageEarnings).Index(i++).Name("February disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.February1618AdditionalPaymentForEmployers).Index(i++).Name("February 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.February1618AdditionalPaymentForProviders).Index(i++).Name("February 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.FebruaryAdditionalPaymentsForApprentices).Index(i++).Name("February additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.February1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("February 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.February1618FrameworkUpliftBalancingPayment).Index(i++).Name("February 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.February1618FrameworkUpliftCompletionPayment).Index(i++).Name("February 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.MarchOnProgrammeEarnings).Index(i++).Name("March on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchBalancingPaymentEarnings).Index(i++).Name("March balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchAimCompletionEarnings).Index(i++).Name("March aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchLearningSupportEarnings).Index(i++).Name("March learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchEnglishMathsOnProgrammeEarnings).Index(i++).Name("March English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchEnglishMathsBalancingPaymentEarnings).Index(i++).Name("March English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchDisadvantageEarnings).Index(i++).Name("March disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.March1618AdditionalPaymentForEmployers).Index(i++).Name("March 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.March1618AdditionalPaymentForProviders).Index(i++).Name("March 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.MarchAdditionalPaymentsForApprentices).Index(i++).Name("March additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.March1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("March 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.March1618FrameworkUpliftBalancingPayment).Index(i++).Name("March 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.March1618FrameworkUpliftCompletionPayment).Index(i++).Name("March 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.AprilOnProgrammeEarnings).Index(i++).Name("April on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilBalancingPaymentEarnings).Index(i++).Name("April balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilAimCompletionEarnings).Index(i++).Name("April aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilLearningSupportEarnings).Index(i++).Name("April learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilEnglishMathsOnProgrammeEarnings).Index(i++).Name("April English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilEnglishMathsBalancingPaymentEarnings).Index(i++).Name("April English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilDisadvantageEarnings).Index(i++).Name("April disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.April1618AdditionalPaymentForEmployers).Index(i++).Name("April 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.April1618AdditionalPaymentForProviders).Index(i++).Name("April 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.AprilAdditionalPaymentsForApprentices).Index(i++).Name("April additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.April1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("April 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.April1618FrameworkUpliftBalancingPayment).Index(i++).Name("April 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.April1618FrameworkUpliftCompletionPayment).Index(i++).Name("April 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.MayOnProgrammeEarnings).Index(i++).Name("May on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MayBalancingPaymentEarnings).Index(i++).Name("May balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MayAimCompletionEarnings).Index(i++).Name("May aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MayLearningSupportEarnings).Index(i++).Name("May learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MayEnglishMathsOnProgrammeEarnings).Index(i++).Name("May English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MayEnglishMathsBalancingPaymentEarnings).Index(i++).Name("May English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.MayDisadvantageEarnings).Index(i++).Name("May disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.May1618AdditionalPaymentForEmployers).Index(i++).Name("May 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.May1618AdditionalPaymentForProviders).Index(i++).Name("May 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.MayAdditionalPaymentsForApprentices).Index(i++).Name("May additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.May1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("May 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.May1618FrameworkUpliftBalancingPayment).Index(i++).Name("May 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.May1618FrameworkUpliftCompletionPayment).Index(i++).Name("May 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.JuneOnProgrammeEarnings).Index(i++).Name("June on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneBalancingPaymentEarnings).Index(i++).Name("June balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneAimCompletionEarnings).Index(i++).Name("June aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneLearningSupportEarnings).Index(i++).Name("June learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneEnglishMathsOnProgrammeEarnings).Index(i++).Name("June English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneEnglishMathsBalancingPaymentEarnings).Index(i++).Name("June English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneDisadvantageEarnings).Index(i++).Name("June disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.June1618AdditionalPaymentForEmployers).Index(i++).Name("June 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.June1618AdditionalPaymentForProviders).Index(i++).Name("June 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.JuneAdditionalPaymentsForApprentices).Index(i++).Name("June additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.June1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("June 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.June1618FrameworkUpliftBalancingPayment).Index(i++).Name("June 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.June1618FrameworkUpliftCompletionPayment).Index(i++).Name("June 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.JulyOnProgrammeEarnings).Index(i++).Name("July on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyBalancingPaymentEarnings).Index(i++).Name("July balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyAimCompletionEarnings).Index(i++).Name("July aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyLearningSupportEarnings).Index(i++).Name("July learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyEnglishMathsOnProgrammeEarnings).Index(i++).Name("July English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyEnglishMathsBalancingPaymentEarnings).Index(i++).Name("July English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyDisadvantageEarnings).Index(i++).Name("July disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.July1618AdditionalPaymentForEmployers).Index(i++).Name("July 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.July1618AdditionalPaymentForProviders).Index(i++).Name("July 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.JulyAdditionalPaymentsForApprentices).Index(i++).Name("July additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.July1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("July 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.July1618FrameworkUpliftBalancingPayment).Index(i++).Name("July 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.July1618FrameworkUpliftCompletionPayment).Index(i++).Name("July 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.TotalOnProgrammeEarnings).Index(i++).Name("Total on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalBalancingPaymentEarnings).Index(i++).Name("Total balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalAimCompletionEarnings).Index(i++).Name("Total aim completion earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalLearningSupportEarnings).Index(i++).Name("Total learning support earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalEnglishMathsOnProgrammeEarnings).Index(i++).Name("Total English and maths on programme earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalEnglishMathsBalancingPaymentEarnings).Index(i++).Name("Total English and maths balancing payment earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalDisadvantageEarnings).Index(i++).Name("Total disadvantage earnings").TypeConverterOption.Format("0.00000");
            Map(m => m.Total1618AdditionalPaymentForEmployers).Index(i++).Name("Total 16-18 additional payments for employers").TypeConverterOption.Format("0.00000");
            Map(m => m.Total1618AdditionalPaymentForProviders).Index(i++).Name("Total 16-18 additional payments for providers").TypeConverterOption.Format("0.00000");
            Map(m => m.TotalAdditionalPaymentsForApprentices).Index(i++).Name("Total additional payments for apprentices").TypeConverterOption.Format("0.00000");
            Map(m => m.Total1618FrameworkUpliftOnProgrammePayment).Index(i++).Name("Total 16-18 framework uplift on programme payment").TypeConverterOption.Format("0.00000");
            Map(m => m.Total1618FrameworkUpliftBalancingPayment).Index(i++).Name("Total 16-18 framework uplift balancing payment").TypeConverterOption.Format("0.00000");
            Map(m => m.Total1618FrameworkUpliftCompletionPayment).Index(i++).Name("Total 16-18 framework uplift completion payment").TypeConverterOption.Format("0.00000");

            Map(m => m.OfficialSensitive).Index(i).Name("OFFICIAL - SENSITIVE");
        }
    }
}
