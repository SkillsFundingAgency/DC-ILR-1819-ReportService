using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface IFm36Builder
    {
        FundingSummaryModel BuildWithFundLine(
            string title,
            FM36Global fm36Global,
            List<string> validLearners,
            string[] fundLines,
            string[] attributes,
            int period);
    }
}
