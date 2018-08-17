using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class MainOccupancyFM35Mapper : ClassMap<MainOccupancyFM35Model>, IClassMapper
    {
        public MainOccupancyFM35Mapper()
        {
            Map(m => m.LearnRefNumber).Index(0).Name("Learner reference number");
            Map(m => m.Uln).Index(1).Name("Unique learner number");
            Map(m => m.DateOfBirth).Index(2).Name("Date of birth");
            Map(m => m.PostcodePrior).Index(3).Name("Postcode prior to enrolment");
            Map(m => m.PmUkprn).Index(4).Name("Pre-merger UKPRN");
            Map(m => m.CampId).Index(5).Name("Campus identifier");
            Map(m => m.ProvSpecLearnMonA).Index(6).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProvSpecLearnMonB).Index(7).Name("Provider specified learner monitoring (B)");
            Map(m => m.AimSeqNumber).Index(8).Name("Aim sequence number");
            Map(m => m.LearnAimRef).Index(9).Name("Learning aim reference");
            Map(m => m.LearnAimRefTitle).Index(10).Name("Learning aim title");
            Map(m => m.SwSupAimId).Index(11).Name("Software supplier aim identifier");
            Map(m => m.WeightedRateFromEsol).Index(12).Name("Applicable funding rate from ESOL hours");
            Map(m => m.ApplicWeightFundRate).Index(13).Name("Applicable funding rate");
            Map(m => m.ApplicProgWeightFact).Index(14).Name("Applicable programme weighting");
            Map(m => m.AimValue).Index(15).Name("Aim value");
            Map(m => m.NotionalNvqLevelV2).Index(16).Name("Notional NVQ level");
            Map(m => m.SectorSubjectAreaTier2).Index(17).Name("Tier 2 sector subject area");
            Map(m => m.ProgType).Index(18).Name("Programme type");
            Map(m => m.FworkCode).Index(19).Name("Framework code");
            Map(m => m.PwayCode).Index(20).Name("Apprenticeship pathway");
            Map(m => m.AimType).Index(21).Name("Aim type");
            Map(m => m.FrameworkComponentType).Index(22).Name("Framework component type code");
            Map(m => m.FundModel).Index(23).Name("Funding model");
            Map(m => m.PriorLearnFundAdj).Index(24).Name("Funding adjustment for prior learning");
            Map(m => m.OtherFundAdj).Index(25).Name("Other funding adjustment");
            Map(m => m.OrigLearnStartDate).Index(26).Name("Original learning start date");
            Map(m => m.LearnStartDate).Index(27).Name("Learning start date");
            Map(m => m.LearnPlanEndDate).Index(28).Name("Learning planned end date");
            Map(m => m.CompStatus).Index(29).Name("Completion status");
            Map(m => m.LearnActEndDate).Index(30).Name("Learning actual end date");
            Map(m => m.Outcome).Index(31).Name("Outcome");
            Map(m => m.AchDate).Index(32).Name("Achievement date");
            Map(m => m.AddHours).Index(33).Name("Additional delivery hours");
            Map(m => m.LearnDelFamCodeSof).Index(34)
                .Name("Learning delivery funding and monitoring type – source of funding");
            Map(m => m.LearnDelFamCodeFfi).Index(35)
                .Name("Learning delivery funding and monitoring type – full or co funding indicator");
            Map(m => m.LearnDelFamCodeEef).Index(36)
                .Name(
                    "Learning delivery funding and monitoring type – eligibility for enhanced apprenticeship funding");
            Map(m => m.LearnDelFamCodeLsfHighest).Index(37).Name(
                "Learning delivery funding and monitoring type – learning support funding (highest applicable)");
            Map(m => m.LearnDelFamCodeLsfEarliest).Index(38)
                .Name("Learning delivery funding and monitoring type - LSF date applies from (earliest)");
            Map(m => m.LearnDelFamCodeLsfLatest).Index(39)
                .Name("Learning delivery funding and monitoring type - LSF date applies to (latest)");
            Map(m => m.LearnDelFamCodeLdm1).Index(40)
                .Name("Learning delivery funding and monitoring type – learning delivery monitoring (A)");
            Map(m => m.LearnDelFamCodeLdm2).Index(41)
                .Name("Learning delivery funding and monitoring type – learning delivery monitoring (B)");
            Map(m => m.LearnDelFamCodeLdm3).Index(42)
                .Name("Learning delivery funding and monitoring type – learning delivery monitoring (C)");
            Map(m => m.LearnDelFamCodeLdm4).Index(43)
                .Name("Learning delivery funding and monitoring type – learning delivery monitoring (D)");
            Map(m => m.LearnDelFamCodeRes).Index(44)
                .Name("Learning delivery funding and monitoring type – restart indicator");
            Map(m => m.ProvSpecDelMonA).Index(45).Name("Provider specified delivery monitoring (A)");
            Map(m => m.ProvSpecDelMonB).Index(46).Name("Provider specified delivery monitoring (B)");
            Map(m => m.ProvSpecDelMonC).Index(47).Name("Provider specified delivery monitoring (C)");
            Map(m => m.ProvSpecDelMonD).Index(48).Name("Provider specified delivery monitoring (D)");
            Map(m => m.FundLine).Index(49).Name("Funding line type");
            Map(m => m.PlannedNumOnProgInstalm).Index(50).Name("Planned number of on programme instalments");
            Map(m => m.PlannedNumOnProgInstalmTrans).Index(51)
                .Name("Transitional planned number of programme instalments from 1 August 2013");
            Map(m => m.StartPropTrans).Index(52).Name("Transitional start proportion");
            Map(m => m.AchieveElement).Index(53).Name("Achievement element (potential or actual earned cash)");
            Map(m => m.AchievePercentage).Index(54).Name("Achievement percentage (aggregated maximum value)");
            Map(m => m.NonGovCont).Index(55).Name("Non-Govt contribution");
            Map(m => m.PartnerUkprn).Index(56).Name("Sub contracted or partnership UKPRN");
            Map(m => m.DelLocPostCode).Index(57).Name("Delivery location postcode");
            Map(m => m.AreaCostFactAdj).Index(58).Name("Area uplift");
            Map(m => m.DisUpFactAdj).Index(59).Name("Disadvantage uplift");
            Map(m => m.LargeEmployerID).Index(60).Name("Employer identifier");
            Map(m => m.LargeEmployerFM35Fctr).Index(61).Name("Large employer factor");
            Map(m => m.CapFactor).Index(62).Name("Capping factor");
            Map(m => m.TraineeWorkPlacement).Index(63).Name("Traineeship work placement or work preparation");
            Map(m => m.HigherApprentishipHeAim).Index(64).Name("Higher apprenticeship prescribed HE aim");
            Map(m => m.ApplicEmpFactDate).Index(65).Name("Date used for employment factor lookups");
            Map(m => m.ApplicFactDate).Index(66).Name("Date used for other factor lookups");

            Map(m => m.Period1OnProgPayment).Index(67).Name("August on programme earned cash");
            Map(m => m.Period1BalancePayment).Index(68).Name("August balancing payment earned cash");
            Map(m => m.Period1AchievePayment).Index(69).Name("August aim achievement earned cash");
            Map(m => m.Period1EmpOutcomePay).Index(70).Name("August job outcome earned cash");
            Map(m => m.Period1LearnSuppFundCash).Index(71).Name("August learning support earned cash");

            Map(m => m.Period2OnProgPayment).Index(72).Name("September on programme earned cash");
            Map(m => m.Period2BalancePayment).Index(73).Name("September balancing payment earned cash");
            Map(m => m.Period2AchievePayment).Index(74).Name("September aim achievement earned cash");
            Map(m => m.Period2EmpOutcomePay).Index(75).Name("September job outcome earned cash");
            Map(m => m.Period2LearnSuppFundCash).Index(76).Name("September learning support earned cash");

            Map(m => m.Period3OnProgPayment).Index(77).Name("October on programme earned cash");
            Map(m => m.Period3BalancePayment).Index(78).Name("October balancing payment earned cash");
            Map(m => m.Period3AchievePayment).Index(79).Name("October aim achievement earned cash");
            Map(m => m.Period3EmpOutcomePay).Index(80).Name("October job outcome earned cash");
            Map(m => m.Period3LearnSuppFundCash).Index(81).Name("October learning support earned cash");

            Map(m => m.Period4OnProgPayment).Index(82).Name("November on programme earned cash");
            Map(m => m.Period4BalancePayment).Index(83).Name("November balancing payment earned cash");
            Map(m => m.Period4AchievePayment).Index(84).Name("November aim achievement earned cash");
            Map(m => m.Period4EmpOutcomePay).Index(85).Name("November job outcome earned cash");
            Map(m => m.Period4LearnSuppFundCash).Index(86).Name("November learning support earned cash");

            Map(m => m.Period5OnProgPayment).Index(87).Name("December on programme earned cash");
            Map(m => m.Period5BalancePayment).Index(88).Name("December balancing payment earned cash");
            Map(m => m.Period5AchievePayment).Index(89).Name("December aim achievement earned cash");
            Map(m => m.Period5EmpOutcomePay).Index(90).Name("December job outcome earned cash");
            Map(m => m.Period5LearnSuppFundCash).Index(91).Name("December learning support earned cash");

            Map(m => m.Period6OnProgPayment).Index(92).Name("December on programme earned cash");
            Map(m => m.Period6BalancePayment).Index(93).Name("December balancing payment earned cash");
            Map(m => m.Period6AchievePayment).Index(94).Name("December aim achievement earned cash");
            Map(m => m.Period6EmpOutcomePay).Index(95).Name("December job outcome earned cash");
            Map(m => m.Period6LearnSuppFundCash).Index(96).Name("December learning support earned cash");

            Map(m => m.Period7OnProgPayment).Index(97).Name("December on programme earned cash");
            Map(m => m.Period7BalancePayment).Index(98).Name("December balancing payment earned cash");
            Map(m => m.Period7AchievePayment).Index(99).Name("December aim achievement earned cash");
            Map(m => m.Period7EmpOutcomePay).Index(100).Name("December job outcome earned cash");
            Map(m => m.Period7LearnSuppFundCash).Index(101).Name("December learning support earned cash");

            Map(m => m.Period8OnProgPayment).Index(92).Name("January on programme earned cash");
            Map(m => m.Period8BalancePayment).Index(93).Name("January balancing payment earned cash");
            Map(m => m.Period8AchievePayment).Index(94).Name("January aim achievement earned cash");
            Map(m => m.Period8EmpOutcomePay).Index(95).Name("January job outcome earned cash");
            Map(m => m.Period8LearnSuppFundCash).Index(96).Name("January learning support earned cash");

            Map(m => m.Period9OnProgPayment).Index(97).Name("February on programme earned cash");
            Map(m => m.Period9BalancePayment).Index(98).Name("February balancing payment earned cash");
            Map(m => m.Period9AchievePayment).Index(99).Name("February aim achievement earned cash");
            Map(m => m.Period9EmpOutcomePay).Index(100).Name("February job outcome earned cash");
            Map(m => m.Period9LearnSuppFundCash).Index(101).Name("February learning support earned cash");

            Map(m => m.Period10OnProgPayment).Index(102).Name("March on programme earned cash");
            Map(m => m.Period10BalancePayment).Index(103).Name("March balancing payment earned cash");
            Map(m => m.Period10AchievePayment).Index(104).Name("March aim achievement earned cash");
            Map(m => m.Period10EmpOutcomePay).Index(105).Name("March job outcome earned cash");
            Map(m => m.Period10LearnSuppFundCash).Index(106).Name("March learning support earned cash");

            Map(m => m.Period11OnProgPayment).Index(107).Name("April on programme earned cash");
            Map(m => m.Period11BalancePayment).Index(108).Name("April balancing payment earned cash");
            Map(m => m.Period11AchievePayment).Index(109).Name("April aim achievement earned cash");
            Map(m => m.Period11EmpOutcomePay).Index(110).Name("April job outcome earned cash");
            Map(m => m.Period11LearnSuppFundCash).Index(111).Name("April learning support earned cash");

            Map(m => m.Period12OnProgPayment).Index(112).Name("May on programme earned cash");
            Map(m => m.Period12BalancePayment).Index(113).Name("May balancing payment earned cash");
            Map(m => m.Period12AchievePayment).Index(114).Name("May aim achievement earned cash");
            Map(m => m.Period12EmpOutcomePay).Index(115).Name("May job outcome earned cash");
            Map(m => m.Period12LearnSuppFundCash).Index(116).Name("May learning support earned cash");

            Map(m => m.TotalOnProgPayment).Index(117).Name("Total on programme earned cash");
            Map(m => m.TotalBalancePayment).Index(118).Name("Total balancing payment earned cash");
            Map(m => m.TotalAchievePayment).Index(119).Name("Total aim achievement earned cash");
            Map(m => m.TotalEmpOutcomePay).Index(120).Name("Total job outcome earned cash");
            Map(m => m.TotalLearnSuppFundCash).Index(121).Name("Total learning support earned cash");
            Map(m => m.TotalEarnedCash).Index(122).Name("Total earned cash");

            Map(m => m.OfficalSensitive).Index(123).Name("OFFICIAL – SENSITIVE");
        }
    }
}
