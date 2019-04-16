using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Mapper
{
    public class TrailblazerAppsOccupancyMapper : ClassMap<TrailblazerAppsOccupancyModel>, IClassMapper
    {
        public TrailblazerAppsOccupancyMapper()
        {
            int i = 0;
            Map(m => m.LearnRefNumber).Index(i).Name("Learner reference number");
            Map(m => m.UniqueLearnerNumber).Index(i++).Name("Unique learner number");
            Map(m => m.DateOfBirth).Index(i++).Name("Date of birth");
            Map(m => m.PMUkPrn).Index(i++).Name("Pre-merger UKPRN");
            Map(m => m.CampId).Index(i++).Name("Campus identifier");
            Map(m => m.ProvSpecLearnMonA).Index(i++).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProvSpecLearnMonB).Index(i++).Name("Provider specified learner monitoring (B)");
            Map(m => m.AimSeqNumber).Index(i++).Name("Aim sequence number");
            Map(m => m.LearnAimRef).Index(i++).Name("Learning aim reference");
            Map(m => m.LearnAimRefTitle).Index(i++).Name("Learning aim title");
            Map(m => m.SwSupAimId).Index(i++).Name("Software supplier aim identifier");
            Map(m => m.NotionalNvqLevelV2).Index(i++).Name("Notional NVQ level");
            Map(m => m.AimType).Index(i++).Name("Aim type");
            Map(m => m.StdCode).Index(i++).Name("Apprenticeship standard code");
            Map(m => m.FundModel).Index(i++).Name("Funding model");
            Map(m => m.PriorLearnFundAdj).Index(i++).Name("Funding adjustment for prior learning");
            Map(m => m.OtherFundAdj).Index(i++).Name("Other funding adjustment");
            Map(m => m.OrigLearnStartDate).Index(i++).Name("Original learning start date");
            Map(m => m.LearnStartDate).Index(i++).Name("Learning start date");
            Map(m => m.LearnPlanEndDate).Index(i++).Name("Learning planned end date");
            Map(m => m.CompStatus).Index(i++).Name("Completion status");
            Map(m => m.LearnActEndDate).Index(i++).Name("Learning actual end date");
            Map(m => m.Outcome).Index(i++).Name("Outcome");
            Map(m => m.AchDate).Index(i++).Name("Achievement date");
            Map(m => m.LearnDelFamCodeSof).Index(i++).Name("Learning delivery funding and monitoring type - source of funding");
            Map(m => m.LearnDelFamCodeEef).Index(i++).Name("Learning delivery funding and monitoring type – eligibility for enhanced apprenticeship funding");
            Map(m => m.LearnDelFamCodeLsfHighest).Index(i++).Name("Learning delivery funding and monitoring type - learning support funding (highest applicable)");
            Map(m => m.LearnDelFamCodeLsfEarliest).Index(i++).Name("Learning delivery funding and monitoring - LSF date applies from (earliest)");
            Map(m => m.LearnDelFamCodeLsfLatest).Index(i++).Name("Learning delivery funding and monitoring - LSF date applies from (latest)");

            Map(m => m.LearnDelMonA).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (A)");
            Map(m => m.LearnDelMonB).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (B)");
            Map(m => m.LearnDelMonC).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (C)");
            Map(m => m.LearnDelMonD).Index(i++).Name("Learning delivery funding and monitoring type - learning delivery monitoring (D)");

            Map(m => m.LearnDelMonRestartIndicator).Index(i++).Name("Learning delivery funding and monitoring type - restart indicator");

            Map(m => m.ProvSpecDelMonA).Index(i++).Name("Provider specified delivery monitoring (A)");
            Map(m => m.ProvSpecDelMonB).Index(i++).Name("Provider specified delivery monitoring (B)");
            Map(m => m.ProvSpecDelMonC).Index(i++).Name("Provider specified delivery monitoring (C)");
            Map(m => m.ProvSpecDelMonD).Index(i++).Name("Provider specified delivery monitoring (D)");

            Map(m => m.EpaOrgID).Index(i++).Name("End point assessment organisation");
            Map(m => m.PartnerUKPRN).Index(i++).Name("Sub contracted or partnership UKPRN");
            Map(m => m.DelLocPostCode).Index(i++).Name("Delivery location postcode");
            Map(m => m.CoreGovContCapApplicVal).Index(i++).Name("LARS maximum core government contribution (£)");
            Map(m => m.SmallBusApplicVal).Index(i++).Name("LARS small employer incentive (£)");
            Map(m => m.YoungAppApplicVal).Index(i++).Name("LARS 16-18 year-old apprentice incentive (£)");
            Map(m => m.AchievementApplicVal).Index(i++).Name("LARS achievement incentive (£)");
            Map(m => m.ApplicFundValDate).Index(i++).Name("Applicable funding value date");
            Map(m => m.FundLine).Index(i++).Name("Funding line type");
            Map(m => m.EmpIdFirstDayStandard).Index(i++).Name("Employer identifier on first day of standard");
            Map(m => m.EmpIdSmallBusDate).Index(i++).Name("Employer identifier on small employer threshold date");
            Map(m => m.EmpIdFirstYoungAppDate).Index(i++).Name("Employer identifier on first 16-18 threshold date");
            Map(m => m.EmpIdSecondYoungAppDate).Index(i++).Name("Employer identifier on small employer threshold date");
            Map(m => m.EmpIdAchDate).Index(i++).Name("Employer identifier on achievement date");
            Map(m => m.MathEngLSFFundStart).Index(i++).Name("Start indicator for maths, English and learning support");
            Map(m => m.AgeStandardStart).Index(i++).Name("Age at start of standard");
            Map(m => m.YoungAppEligible).Index(i++).Name("Eligible for 16-18 year-old apprentice incentive");
            Map(m => m.SmallBusEligible).Index(i++).Name("Eligible for small employer incentive");
            Map(m => m.AchApplicDate).Index(i++).Name("Applicable achievement date");
            Map(m => m.TotalNegotiatedPrice1).Index(i++).Name("Latest total negotiated price (TNP) 1 (£)");
            Map(m => m.TotalNegotiatedPrice2).Index(i++).Name("Latest total negotiated price (TNP) 2 (£)");
            Map(m => m.PMRSumBeforeFundingYear).Index(i++).Name("Sum of PMRs before this funding year (£)");

            Map(m => m.Period1PMRSum).Index(i++).Name("Sum of August payment records (PMRs) (£)");
            Map(m => m.Period1CoreGovContPayment).Index(i++).Name("August core government contribution (£)");
            Map(m => m.Period1MathEngOnProgPayment).Index(i++).Name("August maths and English on-programme earned cash (£)");
            Map(m => m.Period1MathEngBalPayment).Index(i++).Name("August maths and English balancing earned cash (£)");
            Map(m => m.Period1LearnSuppFundCash).Index(i++).Name("August learning support earned cash (£)");
            Map(m => m.Period1SmallBusPayment).Index(i++).Name("August small employer incentive (£)");
            Map(m => m.Period1YoungAppPayment).Index(i++).Name("August 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period1AchPayment).Index(i++).Name("August achievement incentive (£)");

            Map(m => m.Period2PMRSum).Index(i++).Name("Sum of September payment records (PMRs) (£)");
            Map(m => m.Period2CoreGovContPayment).Index(i++).Name("September core government contribution (£)");
            Map(m => m.Period2MathEngOnProgPayment).Index(i++).Name("September maths and English on-programme earned cash (£)");
            Map(m => m.Period2MathEngBalPayment).Index(i++).Name("September maths and English balancing earned cash (£)");
            Map(m => m.Period2LearnSuppFundCash).Index(i++).Name("September learning support earned cash (£)");
            Map(m => m.Period2SmallBusPayment).Index(i++).Name("September small employer incentive (£)");
            Map(m => m.Period2YoungAppPayment).Index(i++).Name("September 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period2AchPayment).Index(i++).Name("September achievement incentive (£)");

            Map(m => m.Period3PMRSum).Index(i++).Name("Sum of October payment records (PMRs) (£)");
            Map(m => m.Period3CoreGovContPayment).Index(i++).Name("October core government contribution (£)");
            Map(m => m.Period3MathEngOnProgPayment).Index(i++).Name("October maths and English on-programme earned cash (£)");
            Map(m => m.Period3MathEngBalPayment).Index(i++).Name("October maths and English balancing earned cash (£)");
            Map(m => m.Period3LearnSuppFundCash).Index(i++).Name("October learning support earned cash (£)");
            Map(m => m.Period3SmallBusPayment).Index(i++).Name("October small employer incentive (£)");
            Map(m => m.Period3YoungAppPayment).Index(i++).Name("October 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period3AchPayment).Index(i++).Name("October achievement incentive (£)");

            Map(m => m.Period4PMRSum).Index(i++).Name("Sum of November payment records (PMRs) (£)");
            Map(m => m.Period4CoreGovContPayment).Index(i++).Name("November core government contribution (£)");
            Map(m => m.Period4MathEngOnProgPayment).Index(i++).Name("November maths and English on-programme earned cash (£)");
            Map(m => m.Period4MathEngBalPayment).Index(i++).Name("November maths and English balancing earned cash (£)");
            Map(m => m.Period4LearnSuppFundCash).Index(i++).Name("November learning support earned cash (£)");
            Map(m => m.Period4SmallBusPayment).Index(i++).Name("November small employer incentive (£)");
            Map(m => m.Period4YoungAppPayment).Index(i++).Name("November 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period4AchPayment).Index(i++).Name("November achievement incentive (£)");

            Map(m => m.Period5PMRSum).Index(i++).Name("Sum of December payment records (PMRs) (£)");
            Map(m => m.Period5CoreGovContPayment).Index(i++).Name("December core government contribution (£)");
            Map(m => m.Period5MathEngOnProgPayment).Index(i++).Name("December maths and English on-programme earned cash (£)");
            Map(m => m.Period5MathEngBalPayment).Index(i++).Name("December maths and English balancing earned cash (£)");
            Map(m => m.Period5LearnSuppFundCash).Index(i++).Name("December learning support earned cash (£)");
            Map(m => m.Period5SmallBusPayment).Index(i++).Name("December small employer incentive (£)");
            Map(m => m.Period5YoungAppPayment).Index(i++).Name("December 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period5AchPayment).Index(i++).Name("December achievement incentive (£)");

            Map(m => m.Period6PMRSum).Index(i++).Name("Sum of January payment records (PMRs) (£)");
            Map(m => m.Period6CoreGovContPayment).Index(i++).Name("January core government contribution (£)");
            Map(m => m.Period6MathEngOnProgPayment).Index(i++).Name("January maths and English on-programme earned cash (£)");
            Map(m => m.Period6MathEngBalPayment).Index(i++).Name("January maths and English balancing earned cash (£)");
            Map(m => m.Period6LearnSuppFundCash).Index(i++).Name("January learning support earned cash (£)");
            Map(m => m.Period6SmallBusPayment).Index(i++).Name("January small employer incentive (£)");
            Map(m => m.Period6YoungAppPayment).Index(i++).Name("January 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period6AchPayment).Index(i++).Name("January achievement incentive (£)");

            Map(m => m.Period7PMRSum).Index(i++).Name("Sum of February payment records (PMRs) (£)");
            Map(m => m.Period7CoreGovContPayment).Index(i++).Name("February core government contribution (£)");
            Map(m => m.Period7MathEngOnProgPayment).Index(i++).Name("February maths and English on-programme earned cash (£)");
            Map(m => m.Period7MathEngBalPayment).Index(i++).Name("February maths and English balancing earned cash (£)");
            Map(m => m.Period7LearnSuppFundCash).Index(i++).Name("February learning support earned cash (£)");
            Map(m => m.Period7SmallBusPayment).Index(i++).Name("February small employer incentive (£)");
            Map(m => m.Period7YoungAppPayment).Index(i++).Name("February 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period7AchPayment).Index(i++).Name("February achievement incentive (£)");

            Map(m => m.Period8PMRSum).Index(i++).Name("Sum of March payment records (PMRs) (£)");
            Map(m => m.Period8CoreGovContPayment).Index(i++).Name("March core government contribution (£)");
            Map(m => m.Period8MathEngOnProgPayment).Index(i++).Name("March maths and English on-programme earned cash (£)");
            Map(m => m.Period8MathEngBalPayment).Index(i++).Name("March maths and English balancing earned cash (£)");
            Map(m => m.Period8LearnSuppFundCash).Index(i++).Name("March learning support earned cash (£)");
            Map(m => m.Period8SmallBusPayment).Index(i++).Name("March small employer incentive (£)");
            Map(m => m.Period8YoungAppPayment).Index(i++).Name("March 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period8AchPayment).Index(i++).Name("March achievement incentive (£)");

            Map(m => m.Period9PMRSum).Index(i++).Name("Sum of April payment records (PMRs) (£)");
            Map(m => m.Period9CoreGovContPayment).Index(i++).Name("April core government contribution (£)");
            Map(m => m.Period9MathEngOnProgPayment).Index(i++).Name("April maths and English on-programme earned cash (£)");
            Map(m => m.Period9MathEngBalPayment).Index(i++).Name("April maths and English balancing earned cash (£)");
            Map(m => m.Period9LearnSuppFundCash).Index(i++).Name("April learning support earned cash (£)");
            Map(m => m.Period9SmallBusPayment).Index(i++).Name("April small employer incentive (£)");
            Map(m => m.Period9YoungAppPayment).Index(i++).Name("April 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period9AchPayment).Index(i++).Name("April achievement incentive (£)");

            Map(m => m.Period10PMRSum).Index(i++).Name("Sum of May payment records (PMRs) (£)");
            Map(m => m.Period10CoreGovContPayment).Index(i++).Name("May core government contribution (£)");
            Map(m => m.Period10MathEngOnProgPayment).Index(i++).Name("May maths and English on-programme earned cash (£)");
            Map(m => m.Period10MathEngBalPayment).Index(i++).Name("May maths and English balancing earned cash (£)");
            Map(m => m.Period10LearnSuppFundCash).Index(i++).Name("May learning support earned cash (£)");
            Map(m => m.Period10SmallBusPayment).Index(i++).Name("May small employer incentive (£)");
            Map(m => m.Period10YoungAppPayment).Index(i++).Name("May 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period10AchPayment).Index(i++).Name("May achievement incentive (£)");

            Map(m => m.Period11PMRSum).Index(i++).Name("Sum of June payment records (PMRs) (£)");
            Map(m => m.Period11CoreGovContPayment).Index(i++).Name("June core government contribution (£)");
            Map(m => m.Period11MathEngOnProgPayment).Index(i++).Name("June maths and English on-programme earned cash (£)");
            Map(m => m.Period11MathEngBalPayment).Index(i++).Name("June maths and English balancing earned cash (£)");
            Map(m => m.Period11LearnSuppFundCash).Index(i++).Name("June learning support earned cash (£)");
            Map(m => m.Period11SmallBusPayment).Index(i++).Name("June small employer incentive (£)");
            Map(m => m.Period11YoungAppPayment).Index(i++).Name("June 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period11AchPayment).Index(i++).Name("June achievement incentive (£)");

            Map(m => m.Period12PMRSum).Index(i++).Name("Sum of July payment records (PMRs) (£)");
            Map(m => m.Period12CoreGovContPayment).Index(i++).Name("July core government contribution (£)");
            Map(m => m.Period12MathEngOnProgPayment).Index(i++).Name("July maths and English on-programme earned cash (£)");
            Map(m => m.Period12MathEngBalPayment).Index(i++).Name("July maths and English balancing earned cash (£)");
            Map(m => m.Period12LearnSuppFundCash).Index(i++).Name("July learning support earned cash (£)");
            Map(m => m.Period12SmallBusPayment).Index(i++).Name("July small employer incentive (£)");
            Map(m => m.Period12YoungAppPayment).Index(i++).Name("July 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period12AchPayment).Index(i++).Name("July achievement incentive (£)");

            Map(m => m.TotalPMRSum).Index(i++).Name("Sum of Total payment records (PMRs) (£)");
            Map(m => m.TotalCoreGovContPayment).Index(i++).Name("Total core government contribution (£)");
            Map(m => m.TotalMathEngOnProgPayment).Index(i++).Name("Total maths and English on-programme earned cash (£)");
            Map(m => m.TotalMathEngBalPayment).Index(i++).Name("Total maths and English balancing earned cash (£)");
            Map(m => m.TotalLearnSuppFundCash).Index(i++).Name("Total learning support earned cash (£)");
            Map(m => m.TotalSmallBusPayment).Index(i++).Name("Total small employer incentive (£)");
            Map(m => m.TotalYoungAppPayment).Index(i++).Name("Total 16-18 year-old apprentice incentive (£)");
            Map(m => m.TotalAchPayment).Index(i++).Name("Total achievement incentive (£)");

            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}