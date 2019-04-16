using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Service.Builders.PeriodEnd
{
    public class FundingSummaryPeriodEndModelBuilder : IFundingSummaryPeriodEndModelBuilder
    {
        public FundingSummaryPeriodEndModel BuildModel(ILearner learner)
        {
            return new FundingSummaryPeriodEndModel()
            {
            };
        }
    }
}
