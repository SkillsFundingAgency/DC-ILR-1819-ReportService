using System.Collections.Generic;

namespace ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class AppsCoInvestmentPaymentsInfo
    {
        public long UkPrn { get; set; }

        public List<PaymentInfo> Payments { get; set; }
    }
}
