using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface IFm81Builder
    {
        FundingSummaryModel BuildWithFundLine(
            string title,
            FM81Global fm81Global,
            List<string> validLearners,
            string fundLine,
            string[] attributes,
            int period);
    }
}
