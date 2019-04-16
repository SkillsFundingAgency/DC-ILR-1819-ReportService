namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class FundingSummaryHeaderModel
    {
        public string ProviderName { get; set; }

        public int Ukprn { get; set; }

        public string IlrFile { get; set; }

        public string LastIlrFileUpdate { get; set; }

        public string LastEasUpdate { get; set; }

        public string SecurityClassification { get; set; }
    }
}
