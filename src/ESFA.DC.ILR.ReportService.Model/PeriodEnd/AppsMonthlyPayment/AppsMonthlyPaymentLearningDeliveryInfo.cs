
using System.Collections.Generic;

namespace ESFA.DC.ILR.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentLearningDeliveryInfo
    {
        public int UKPRN { get; set; }

        public string LearnRefNumber { get; set; }

        public string LearnAimRef { get; set; }

        public int AimType { get; set; }
       
        public string SWSupAimId { get; set; }

        public ICollection<AppsMonthlyPaymentProviderSpecDeliveryMonitoringInfo> ProviderSpecDeliveryMonitorings { get; set; }

        public ICollection<AppsMonthlyPaymentLearningDeliveryFAMInfo> LearningDeliveryFams { get; set; }

    }
}