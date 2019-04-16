using System.Collections.Generic;

namespace ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsAdditionalPayment
{
    public class AppsAdditionalPaymentILRInfo
    {
        public int UkPrn { get; set; }

        public List<AppsAdditionalPaymentLearnerInfo> Learners { get; set; }
    }
}
