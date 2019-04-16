using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Mapper
{
    public sealed class SummaryOfFM35FundingMapper : ClassMap<SummaryOfFm35FundingModel>, IClassMapper
    {
        public SummaryOfFM35FundingMapper()
        {
            var i = 0;
            Map(m => m.FundingLineType).Index(i++).Name("Funding line type");
            Map(m => m.Period).Index(i++).Name("Period");
            Map(m => m.OnProgramme).Index(i++).Name("On-programme (£)");
            Map(m => m.Balancing).Index(i++).Name("Balancing (£)");
            Map(m => m.JobOutcomeAchievement).Index(i++).Name("Job Outcome Achievement (£)");
            Map(m => m.AimAchievement).Index(i++).Name("Aim Achievement (£)");
            Map(m => m.TotalAchievement).Index(i++).Name("Total Achievement (£)");
            Map(m => m.LearningSupport).Index(i++).Name("Learning Support (£)");
            Map(m => m.Total).Index(i++).Name("Total (£)");
            Map(m => m.OfficalSensitive).Index(i).Name("OFFICIAL - SENSITIVE");
        }
    }
}
