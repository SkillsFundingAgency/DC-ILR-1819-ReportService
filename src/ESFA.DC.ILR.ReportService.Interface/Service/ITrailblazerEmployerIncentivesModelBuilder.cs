using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface ITrailblazerEmployerIncentivesModelBuilder
    {
        TrailblazerEmployerIncentivesModel BuildTrailblazerEmployerIncentivesModel(
            long empIdentifier,
            LearningDelivery fm81Data);

        TrailblazerEmployerIncentivesModel BuildTrailblazerEmployerIncentivesModel(
            long empIdentifier,
            Dictionary<string, int> employerIdentifier,
            List<LearningDelivery> fm81Data);
    }
}