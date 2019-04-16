using System;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd
{
    public class AppsDataMatchMonthEndModel
    {
        public string LearnerReferenceNumber { get; set; }

        public long UniqueLearnerNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public string RuleName { get; set; }

        public string Description { get; set; }

        public decimal ILRValue { get; set; }

        public string ApprenticeshipServiceValue { get; set; }

        public string PriceEpisodeStartDate { get; set; }

        public string PriceEpisodeActualEndDate { get; set; }

        public string AgreementIdentifier { get; set; }

        public string EmployerName { get; set; }

        public string OfficialSensitive { get; }
    }
}