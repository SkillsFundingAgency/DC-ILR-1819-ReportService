using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class SummaryOfFunding1619Mapper : ClassMap<SummaryOfFunding1619Model>, IClassMapper
    {
        public SummaryOfFunding1619Mapper()
        {
            int i = 0;
            Map(m => m.FundLine).Index(i++).Name("Funding line type");
            Map(m => m.LearnRefNumber).Index(i++).Name("Learner reference number");
            Map(m => m.FamilyName).Index(i++).Name("Family name");
            Map(m => m.GivenNames).Index(i++).Name("Given names");
            Map(m => m.DateOfBirth).Index(i++).Name("Date of birth");
            Map(m => m.CampId).Index(i++).Name("Campus identifier");
            Map(m => m.PlanLearnHours).Index(i++).Name("Planned learning hours");
            Map(m => m.PlanEepHours).Index(i++).Name("Planned employability, enrichment and pastoral hours");
            Map(m => m.TotalPlannedHours).Index(i++).Name("Total planned hours");
            Map(m => m.RateBand).Index(i++).Name("Funding band");
            Map(m => m.StartFund).Index(i++).Name("Qualifies for funding");
            Map(m => m.OnProgPayment).Index(i++).Name("Total funding");
            Map(m => m.OfficalSensitive).Index(i++).Name("OFFICIAL-SENSITIVE");
        }
    }
}
