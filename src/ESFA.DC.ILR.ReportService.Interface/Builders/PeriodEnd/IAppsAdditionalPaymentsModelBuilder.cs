using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Interface.Builders.PeriodEnd
{
   public interface IAppsAdditionalPaymentsModelBuilder
    {
        List<AppsAdditionalPaymentsModel> BuildModel(AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo, AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo, AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo);
    }
}