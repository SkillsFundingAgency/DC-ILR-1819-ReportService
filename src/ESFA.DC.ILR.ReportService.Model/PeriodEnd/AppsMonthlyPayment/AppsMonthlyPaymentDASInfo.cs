using System.Collections.Generic;

namespace ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDASInfo
    {
        public long UkPrn { get; set; }

        public List<AppsMonthlyPaymentDASPaymentInfo> Payments { get; set; }
    }
}
