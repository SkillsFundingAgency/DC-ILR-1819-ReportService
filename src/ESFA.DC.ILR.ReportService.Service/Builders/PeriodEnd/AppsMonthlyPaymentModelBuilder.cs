using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Service.Builders.PeriodEnd
{
    public class AppsMonthlyPaymentModelBuilder : IAppsMonthlyPaymentModelBuilder
    {
        public AppsMonthlyPaymentModel BuildModel(ILearner learner, FM36Learner learnerData)
        {
            return new AppsMonthlyPaymentModel()
            {
                LearnerReferenceNumber = learner.LearnRefNumber,
                UniqueLearnerNumber = learner.ULN
            };
        }
    }
}
