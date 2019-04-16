namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class FundingClaim1619Model
    {
        public string Title { get; set; }

        public int StudentNumber { get; set; }

        public decimal TotalFunding { get; set; }

        public decimal? ExceptionalAdjustment { get; set; }

        public decimal? Total { get; set; }
    }
}
