using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface IFm25Builder
    {
        FundingSummaryModel BuildWithFundLine(
            string title,
            FM25Global fm25Global,
            List<string> validLearners,
            string fundLine,
            int period);
    }
}
