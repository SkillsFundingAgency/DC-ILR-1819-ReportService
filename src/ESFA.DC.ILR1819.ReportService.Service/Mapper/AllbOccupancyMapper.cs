using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class AllbOccupancyMapper : ClassMap<AllbOccupancyModel>, IClassMapper
    {
        public AllbOccupancyMapper()
        {
            Map(m => m.LearnRefNumber).Index(0).Name("Learner reference number");
            Map(m => m.Uln).Index(1).Name("Unique learner number");
            Map(m => m.DateOfBirth).Index(2).Name("Date of birth");
            Map(m => m.PreMergerUkprn).Index(3).Name("Pre-merger UKPRN");
            Map(m => m.CampId).Index(4).Name("Campus identifier");
            Map(m => m.ProvSpecLearnMonA).Index(5).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProvSpecLearnMonB).Index(6).Name("Provider specified learner monitoring (B)");
            Map(m => m.AimSeqNumber).Index(7).Name("Aim sequence number");
            Map(m => m.LearnAimRef).Index(8).Name("Learning aim reference");
            Map(m => m.LearnAimRefTitle).Index(9).Name("Learning aim title");
            Map(m => m.SwSupAimId).Index(10).Name("Software supplier aim identifier");
            Map(m => m.WeightedRate).Index(11).Name("Applicable funding rate");
            Map(m => m.ApplicProgWeightFact).Index(12).Name("Applicable programme weighting");
            Map(m => m.NotionalNvqLevelV2).Index(13).Name("Notional NVQ level");
            Map(m => m.SectorSubjectAreaTier2).Index(14).Name("Tier 2 sector subject area");
            Map(m => m.AimType).Index(15).Name("Aim type");
            Map(m => m.FundingModel).Index(16).Name("Funding model");
            Map(m => m.PriorLearnFundAdj).Index(17).Name("Funding adjustment for prior learning");
            Map(m => m.OtherFundAdj).Index(18).Name("Other funding adjustment");
            Map(m => m.OrigLearnStartDate).Index(19).Name("Original learning start date");
            Map(m => m.LearnStartDate).Index(20).Name("Learning start date");
            Map(m => m.LearnPlanEndDate).Index(21).Name("Learning planned end date");
            Map(m => m.CompStatus).Index(22).Name("Completion status");
            Map(m => m.LearnActEndDate).Index(23).Name("Learning actual end date");
            Map(m => m.Outcome).Index(24).Name("Outcome");
            Map(m => m.LearnDelFamCodeAdl).Index(25)
                .Name("Learning delivery funding and monitoring type - Advanced Learner Loans indicator");
            Map(m => m.AlbBursaryFunding).Index(26)
                .Name("Learning delivery funding and monitoring type - Advanced Learner Loans Bursary funding");
            Map(m => m.AlbDateFrom).Index(27).Name("Learning delivery funding and monitoring - ALB date applies from");
            Map(m => m.AlbDateTo).Index(28).Name("Learning delivery funding and monitoring - ALB date applies to");
            Map(m => m.LearnDelMonA).Index(29)
                .Name("Learning delivery funding and monitoring type - learning delivery monitoring (A)");
            Map(m => m.LearnDelMonB).Index(30)
                .Name("Learning delivery funding and monitoring type - learning delivery monitoring (B)");
            Map(m => m.LearnDelMonC).Index(31)
                .Name("Learning delivery funding and monitoring type - learning delivery monitoring (C)");
            Map(m => m.LearnDelMonD).Index(32)
                .Name("Learning delivery funding and monitoring type - learning delivery monitoring (D)");
            Map(m => m.ProvSpecDelMonA).Index(33).Name("Provider specified delivery monitoring (A)");
            Map(m => m.ProvSpecDelMonB).Index(34).Name("Provider specified delivery monitoring (B)");
            Map(m => m.ProvSpecDelMonC).Index(35).Name("Provider specified delivery monitoring (C)");
            Map(m => m.ProvSpecDelMonD).Index(36).Name("Provider specified delivery monitoring (D)");
            Map(m => m.PartnerUkprn).Index(37).Name("Sub contracted or partnership UKPRN");
            Map(m => m.DelLocPostCode).Index(38).Name("Delivery location postcode");
            Map(m => m.AreaCodeFactAdj).Index(39).Name("Area uplift");
            Map(m => m.FundLine).Index(40).Name("Funding line type");
            Map(m => m.LiabilityDate).Index(41).Name("First liability date");
            Map(m => m.PlannedNumOnProgInstalm).Index(42).Name("Planned number of instalments");
            Map(m => m.ApplicFactDate).Index(43).Name("Date used for factor lookups");
            Map(m => m.Period1AlbCode).Index(44).Name("August ALB code used");
            Map(m => m.Period1AlbPayment).Index(45).Name("August ALB support payment earned cash");
            Map(m => m.Period1AlbOnProgPayment).Index(46).Name("August loans bursary for area costs on programme earned cash");
            Map(m => m.Period1AlbAreaUplift).Index(47)
                .Name("August loans bursary for area costs balancing earned cash");
            Map(m => m.Period1AlbTotal).Index(48).Name("August loans bursary total earned cash");

            Map(m => m.Period2AlbCode).Index(49).Name("September ALB code used");
            Map(m => m.Period2AlbPayment).Index(50).Name("September ALB support payment earned cash");
            Map(m => m.Period2AlbOnProgPayment).Index(51).Name("September loans bursary for area costs on programme earned cash");
            Map(m => m.Period2AlbBalPayment).Index(52)
                .Name("September loans bursary for area costs balancing earned cash");
            Map(m => m.Period2AlbTotal).Index(53).Name("September loans bursary total earned cash");

            Map(m => m.Period3AlbCode).Index(54).Name("October ALB code used");
            Map(m => m.Period3AlbPayment).Index(55).Name("October ALB support payment earned cash");
            Map(m => m.Period3AlbOnProgPayment).Index(56).Name("October loans bursary for area costs on programme earned cash");
            Map(m => m.Period3AlbBalPayment).Index(57)
                .Name("October loans bursary for area costs balancing earned cash");
            Map(m => m.Period3AlbTotal).Index(58).Name("October loans bursary total earned cash");

            Map(m => m.Period4AlbCode).Index(59).Name("November ALB code used");
            Map(m => m.Period4AlbPayment).Index(60).Name("November ALB support payment earned cash");
            Map(m => m.Period4AlbOnProgPayment).Index(61).Name("November loans bursary for area costs on programme earned cash");
            Map(m => m.Period4AlbBalPayment).Index(62)
                .Name("November loans bursary for area costs balancing earned cash");
            Map(m => m.Period4AlbTotal).Index(63).Name("November loans bursary total earned cash");

            Map(m => m.Period5AlbCode).Index(64).Name("December ALB code used");
            Map(m => m.Period5AlbPayment).Index(65).Name("December ALB support payment earned cash");
            Map(m => m.Period5AlbOnProgPayment).Index(66).Name("December loans bursary for area costs on programme earned cash");
            Map(m => m.Period5AlbBalPayment).Index(67)
                .Name("December loans bursary for area costs balancing earned cash");
            Map(m => m.Period5AlbTotal).Index(68).Name("December loans bursary total earned cash");

            Map(m => m.Period6AlbCode).Index(69).Name("January ALB code used");
            Map(m => m.Period6AlbPayment).Index(70).Name("January ALB support payment earned cash");
            Map(m => m.Period6AlbOnProgPayment).Index(71).Name("January loans bursary for area costs on programme earned cash");
            Map(m => m.Period6AlbBalPayment).Index(72)
                .Name("January loans bursary for area costs balancing earned cash");
            Map(m => m.Period6AlbTotal).Index(73).Name("January loans bursary total earned cash");

            Map(m => m.Period7AlbCode).Index(74).Name("February ALB code used");
            Map(m => m.Period7AlbPayment).Index(75).Name("February ALB support payment earned cash");
            Map(m => m.Period7AlbOnProgPayment).Index(76).Name("February loans bursary for area costs on programme earned cash");
            Map(m => m.Period7AlbBalPayment).Index(77)
                .Name("February loans bursary for area costs balancing earned cash");
            Map(m => m.Period7AlbTotal).Index(78).Name("February loans bursary total earned cash");

            Map(m => m.Period8AlbCode).Index(79).Name("March ALB code used");
            Map(m => m.Period8AlbPayment).Index(80).Name("March ALB support payment earned cash");
            Map(m => m.Period8AlbOnProgPayment).Index(81).Name("March loans bursary for area costs on programme earned cash");
            Map(m => m.Period8AlbBalPayment).Index(82)
                .Name("March loans bursary for area costs balancing earned cash");
            Map(m => m.Period8AlbTotal).Index(83).Name("March loans bursary total earned cash");

            Map(m => m.Period9AlbCode).Index(84).Name("April ALB code used");
            Map(m => m.Period9AlbPayment).Index(85).Name("April ALB support payment earned cash");
            Map(m => m.Period9AlbOnProgPayment).Index(86).Name("April loans bursary for area costs on programme earned cash");
            Map(m => m.Period9AlbBalPayment).Index(87)
                .Name("April loans bursary for area costs balancing earned cash");
            Map(m => m.Period9AlbTotal).Index(88).Name("April loans bursary total earned cash");

            Map(m => m.Period10AlbCode).Index(89).Name("May ALB code used");
            Map(m => m.Period10AlbPayment).Index(90).Name("May ALB support payment earned cash");
            Map(m => m.Period10AlbOnProgPayment).Index(91).Name("May loans bursary for area costs on programme earned cash");
            Map(m => m.Period10AlbBalPayment).Index(92)
                .Name("May loans bursary for area costs balancing earned cash");
            Map(m => m.Period10AlbTotal).Index(93).Name("May loans bursary total earned cash");

            Map(m => m.Period11AlbCode).Index(94).Name("June ALB code used");
            Map(m => m.Period11AlbPayment).Index(95).Name("June ALB support payment earned cash");
            Map(m => m.Period11AlbOnProgPayment).Index(96).Name("June loans bursary for area costs on programme earned cash");
            Map(m => m.Period11AlbBalPayment).Index(97)
                .Name("June loans bursary for area costs balancing earned cash");
            Map(m => m.Period11AlbTotal).Index(98).Name("June loans bursary total earned cash");

            Map(m => m.Period12AlbCode).Index(99).Name("July ALB code used");
            Map(m => m.Period12AlbPayment).Index(100).Name("July ALB support payment earned cash");
            Map(m => m.Period12AlbOnProgPayment).Index(101).Name("July loans bursary for area costs on programme earned cash");
            Map(m => m.Period12AlbBalPayment).Index(102)
                .Name("July loans bursary for area costs balancing earned cash");
            Map(m => m.Period12AlbTotal).Index(103).Name("July loans bursary total earned cash");

            Map(m => m.TotalAlbSupportPayment).Index(104).Name("Total ALB support payment earned cash");
            Map(m => m.TotalAlbAreaUplift).Index(105).Name("Total loans bursary for area costs on programme earned cash");
            Map(m => m.TotalAlbBalPayment).Index(106).Name("Total loans bursary for area costs balancing earned cash");
            Map(m => m.TotalEarnedCash).Index(107).Name("Total earned cash");

            Map(m => m.OfficalSensitive).Index(108).Name("OFFICIAL – SENSITIVE");
        }
    }
}
