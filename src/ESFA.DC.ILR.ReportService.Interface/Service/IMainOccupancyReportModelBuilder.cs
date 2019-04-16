using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IMainOccupancyReportModelBuilder
    {
        MainOccupancyModel BuildFm35Model(
            ILearner learner,
            ILearningDelivery learningDelivery,
            LarsLearningDelivery larsModel,
            LearningDelivery frameworkAim,
            ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery fm35Data,
            IStringUtilitiesService stringUtilitiesService);

        MainOccupancyModel BuildFm25Model(
            ILearner learner,
            ILearningDelivery learningDelivery,
            FM25Learner fm25Data);
    }
}