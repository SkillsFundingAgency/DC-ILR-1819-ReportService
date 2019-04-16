using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IHNSReportModelBuilder
    {
        HNSModel BuildModel(
            ILearner learner,
            ILearningDelivery learningDelivery,
            FM25Learner fm25Data);
    }
}
