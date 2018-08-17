using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Model;


namespace ESFA.DC.ILR1819.ReportService.Interface.Service
{
    public interface IMathsAndEnglishModelBuilder
    {
        IMathsAndEnglishModel BuildModel(ILearner learner, Learner fm25Data);
    }
}