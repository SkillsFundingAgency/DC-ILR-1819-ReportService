using ESFA.DC.ILR.FundingService.FM25.Model.Output;

namespace ESFA.DC.ILR1819.ReportService.Interface.Service
{
    public interface IMathsAndEnglishFm25Rules
    {
        bool IsApplicableLearner(Learner learner);
    }
}