using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Mapper
{
    public class HNSMapper : ClassMap<HNSModel>, IClassMapper
    {
        public HNSMapper()
        {
            int i = 0;
            Map(m => m.FundLine).Index(i++).Name("Funding line type");
            Map(m => m.LearnRefNumber).Index(i++).Name("Learner reference number");
            Map(m => m.FamilyName).Index(i++).Name("Family name");
            Map(m => m.GivenNames).Index(i++).Name("Given names");
            Map(m => m.CampId).Index(i++).Name("Campus Identifier");
            Map(m => m.LearnerFAM_A).Index(i++).Name("A - Students with an EHCP");
            Map(m => m.LearnerFAM_B).Index(i++).Name("B - Students without an EHCP");
            Map(m => m.LearnerFAM_C).Index(i++).Name("C - High Needs Students (HNS) without an EHCP");
            Map(m => m.LearnerFAM_D).Index(i++).Name("D - Students with an EHCP and HNS");
            Map(m => m.LearnerFAM_E).Index(i++).Name("E - Students with an EHCP but without HNS");
            Map(m => m.OfficalSensitive).Index(i++).Name("OFFICIAL-SENSITIVE");
        }
    }
}
