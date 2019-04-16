using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IAppsIndicativeCommand
    {
        void Execute(AppsIndicativeEarningsModel model, LearningDelivery learningDeliveryAttribute, PriceEpisode episodeAttribute);
    }
}