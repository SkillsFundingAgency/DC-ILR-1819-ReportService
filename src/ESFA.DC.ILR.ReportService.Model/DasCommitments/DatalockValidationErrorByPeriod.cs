namespace ESFA.DC.ILR.ReportService.Model.DasCommitments
{
    public sealed class DatalockValidationErrorByPeriod : IIdentifyCommitments
    {
        public long Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public string RuleId { get; set; }
        
        public string PriceEpisodeIdentifier { get; set; }
        
        public int Period { get; set; }
    }
}
