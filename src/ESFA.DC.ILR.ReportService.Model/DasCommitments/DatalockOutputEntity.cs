namespace ESFA.DC.ILR.ReportService.Model.DasCommitments
{
    public sealed class DatalockOutputEntity
    {
        public long Ukprn { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public long CommitmentId { get; set; }

        public string VersionId { get; set; }

        public int Period { get; set; }

        public bool Payable { get; set; }

        public int TransactionType { get; set; }

        public int TransactionTypesFlag { get; set; }

        public bool PayOnprog { get; set; }

        public bool PayFirst16To18Incentive { get; set; }

        public bool PaySecond16To18Incentive { get; set; }
    }
}
