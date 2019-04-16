using ESFA.DC.ILR.FundingService.FM25.Model.Output;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IMathsAndEnglishFm25Rules
    {
        bool IsApplicableLearner(FM25Learner learner);
    }
}