using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface ITrailblazerAppsOccupancyModelBuilder
    {
        TrailblazerAppsOccupancyModel BuildTrailblazerAppsOccupancyModel(
            ILearner learner,
            ILearningDelivery learningDelivery,
            LarsLearningDelivery larsModel,
            LearningDelivery ruleBaseLearningDelivery);
    }
}