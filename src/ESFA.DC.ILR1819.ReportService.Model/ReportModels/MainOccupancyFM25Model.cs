namespace ESFA.DC.ILR1819.ReportService.Model.ReportModels
{
    public sealed class MainOccupancyFM25Model
    {
        public string LearnRefNumber { get; set; }

        public long Uln { get; set; }

        public string DateOfBirth { get; set; }

        public string PostcodePrior { get; set; }

        public int? PmUkprn { get; set; }

        public string CampId { get; set; }

        public string ProvSpecLearnMonA { get; set; }

        public string ProvSpecLearnMonB { get; set; }

        public object NatRate { get; set; }

        public int FundModel { get; set; }

        public object LearnerStartDate { get; set; }

        public object LearnerPlanEndDate { get; set; }

        public object LearnerActEndDate { get; set; }

        public string FundLine { get; set; }

        public decimal? Period1OnProgPayment { get; set; }

        public decimal? Period2OnProgPayment { get; set; }

        public decimal? Period3OnProgPayment { get; set; }

        public decimal? Period4OnProgPayment { get; set; }

        public decimal? Period5OnProgPayment { get; set; }

        public decimal? Period6OnProgPayment { get; set; }

        public decimal? Period7OnProgPayment { get; set; }

        public decimal? Period8OnProgPayment { get; set; }

        public decimal? Period9OnProgPayment { get; set; }

        public decimal? Period10OnProgPayment { get; set; }

        public decimal? Period11OnProgPayment { get; set; }

        public decimal? Period12OnProgPayment { get; set; }

        public decimal? PeriodOnProgPaymentTotal { get; set; }

        public decimal? Total { get; set; }

        public string OfficalSensitive { get; }
    }
}
