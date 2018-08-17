using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class MathsAndEnglishMapper : ClassMap<MathsAndEnglishModel>, IClassMapper
    {
        public MathsAndEnglishMapper()
        {
            int i = 0;
            Map(m => m.FundLine).Index(i++).Name("Funding line type");
            Map(m => m.LearnRefNumber).Index(i++).Name("Learner reference number");
            Map(m => m.FamilyName).Index(i++).Name("Family name");
            Map(m => m.GivenNames).Index(i++).Name("Given names");
            Map(m => m.DateOfBirth).Index(i++).Name("Date of birth");
            Map(m => m.CampId).Index(i++).Name("Campus identifier");
            Map(m => m.ConditionOfFundingMaths).Index(i++).Name("Maths GCSE status");
            Map(m => m.ConditionOfFundingEnglish).Index(i++).Name("English GCSE status");
            Map(m => m.RateBand).Index(i++).Name("Funding band");
            Map(m => m.OfficalSensitive).Index(i++).Name("OFFICIAL – SENSITIVE");
        }
    }
}
