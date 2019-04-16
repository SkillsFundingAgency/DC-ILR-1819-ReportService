using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd
{
    public interface IAppsMonthlyPaymentModelBuilder
    {
        AppsMonthlyPaymentModel BuildModel(ILearner learner, FM36Learner learnerData);
    }
}