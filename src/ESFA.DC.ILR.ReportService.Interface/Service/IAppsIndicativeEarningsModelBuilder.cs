using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IAppsIndicativeEarningsModelBuilder
    {
        AppsIndicativeEarningsModel BuildModel(
            ILearner learner,
            ILearningDelivery learningDelivery,
            LearningDelivery fm36DeliveryAttribute,
            PriceEpisode fm36EpisodeAttribute,
            LarsLearningDelivery larsLearningDelivery,
            string notionalEndLevel,
            bool earliestEpisode,
            bool hasPriceEpisodes);
    }
}