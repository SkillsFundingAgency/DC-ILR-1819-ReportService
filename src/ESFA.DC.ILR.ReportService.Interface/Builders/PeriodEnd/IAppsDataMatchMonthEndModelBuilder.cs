using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd
{
    public interface IAppsDataMatchMonthEndModelBuilder
    {
        AppsDataMatchMonthEndModel BuildModel(ILearner learner);
    }
}