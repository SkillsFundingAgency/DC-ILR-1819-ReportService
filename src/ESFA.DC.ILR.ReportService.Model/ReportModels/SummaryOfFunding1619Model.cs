namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class SummaryOfFunding1619Model
    {
        public string FundLine { get; set; }

        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public string DateOfBirth { get; set; }

        public string CampId { get; set; }

        public int? PlanLearnHours { get; set; }

        public int? PlanEepHours { get; set; }

        public int TotalPlannedHours { get; set; }

        public string RateBand { get; set; }

        public bool StartFund { get; set; }

        public decimal? OnProgPayment { get; set; }

        public string OfficalSensitive { get; }
    }
}
