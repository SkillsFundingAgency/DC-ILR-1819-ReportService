using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class MainOccupancyFM25Mapper : ClassMap<MainOccupancyFM25Model>, IClassMapper
    {
        public MainOccupancyFM25Mapper()
        {
            Map(m => m.LearnRefNumber).Index(0).Name("Learner reference number");
            Map(m => m.Uln).Index(1).Name("Unique learner number");
            Map(m => m.DateOfBirth).Index(2).Name("Date of birth");
            Map(m => m.PostcodePrior).Index(3).Name("Postcode prior to enrolment");
            Map(m => m.PmUkprn).Index(4).Name("Pre-merger UKPRN");
            Map(m => m.CampId).Index(5).Name("Campus identifier");
            Map(m => m.ProvSpecLearnMonA).Index(6).Name("Provider specified learner monitoring (A)");
            Map(m => m.ProvSpecLearnMonB).Index(7).Name("Provider specified learner monitoring (B)");
            Map(m => m.NatRate).Index(8).Name("Applicable funding rate");
            Map(m => m.FundModel).Index(9).Name("Funding model");
            Map(m => m.Period1OnProgPayment).Index(10).Name("August on programme earned cash");
            Map(m => m.Period2OnProgPayment).Index(11).Name("September on programme earned cash");
            Map(m => m.Period3OnProgPayment).Index(12).Name("October on programme earned cash");
            Map(m => m.Period4OnProgPayment).Index(13).Name("November on programme earned cash");
            Map(m => m.Period5OnProgPayment).Index(14).Name("December on programme earned cash");
            Map(m => m.Period6OnProgPayment).Index(15).Name("January on programme earned cash");
            Map(m => m.Period7OnProgPayment).Index(16).Name("February on programme earned cash");
            Map(m => m.Period8OnProgPayment).Index(17).Name("March on programme earned cash");
            Map(m => m.Period9OnProgPayment).Index(18).Name("April on programme earned cash");
            Map(m => m.Period10OnProgPayment).Index(19).Name("May on programme earned cash");
            Map(m => m.Period11OnProgPayment).Index(20).Name("June on programme earned cash");
            Map(m => m.Period12OnProgPayment).Index(21).Name("July on programme earned cash");
            Map(m => m.PeriodOnProgPaymentTotal).Index(22).Name("Total on programme earned cash");
            Map(m => m.Total).Index(23).Name("Total earned cash");
            Map(m => m.OfficalSensitive).Index(24).Name("OFFICIAL – SENSITIVE");
        }
    }
}
