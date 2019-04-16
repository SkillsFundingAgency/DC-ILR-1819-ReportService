namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class DataMatchModel
    {
        public string LearnRefNumber { get; set; }

        public long Uln { get; set; }

        public int AimSeqNumber { get; set; }

        public string RuleName { get; set; }

        public string Description { get; set; }

        public string ILRValue { get; set; }

        public string ApprenticeshipServiceValue { get; set; }

        public string PriceEpisodeStartDate { get; set; }

        public string PriceEpisodeActualEndDate { get; set; }

        public string OfficalSensitive { get; }

        // For duplicate removal
        public string PriceEpisodeIdentifier { get; set; }
    }
}
