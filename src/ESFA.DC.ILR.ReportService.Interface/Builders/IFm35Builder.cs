using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface IFm35Builder
    {
        IList<SummaryOfFm35FundingModel> BuildModel(LearningDelivery fundLineData);

        FundingSummaryModel BuildWithFundLine(string title, FM35Global fm35Global, List<string> validLearners, string[] fundLines, string[] attributes, int period);
    }
}