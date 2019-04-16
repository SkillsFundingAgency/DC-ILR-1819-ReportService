using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Service.Builders.PeriodEnd
{
    public class AppsDataMatchMonthEndModelBuilder : IAppsDataMatchMonthEndModelBuilder
    {
        public AppsDataMatchMonthEndModel BuildModel(ILearner learner)
        {
            return new AppsDataMatchMonthEndModel()
            {
                LearnerReferenceNumber = learner.LearnRefNumber,
                UniqueLearnerNumber = learner.ULN
            };
        }
    }
}
