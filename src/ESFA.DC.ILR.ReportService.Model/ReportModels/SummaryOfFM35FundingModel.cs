namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public class SummaryOfFm35FundingModel
    {
        public string FundingLineType { get; set; }

        public decimal? Period { get; set; }

        public decimal? OnProgramme { get; set; }

        public decimal? Balancing { get; set; }

        public decimal? JobOutcomeAchievement { get; set; }

        public decimal? AimAchievement { get; set; }

        public decimal? TotalAchievement { get; set; }

        public decimal? LearningSupport { get; set; }

        public decimal? Total { get; set; }

        public string OfficalSensitive { get; }
    }
}
