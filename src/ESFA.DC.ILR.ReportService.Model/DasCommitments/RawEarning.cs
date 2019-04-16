using System;

namespace ESFA.DC.ILR.ReportService.Model.DasCommitments
{
    public sealed class RawEarning : IIdentifyCommitments
    {
        public string PriceEpisodeIdentifier { get; set; }

        public long Ukprn { get; set; }

        public string LearnRefNumber { get; set; }

        public long Uln { get; set; }

        public int AimSeqNumber { get; set; }

        public DateTime? EpisodeStartDate { get; set; }

        public DateTime? EpisodeEffectiveTnpStartDate { get; set; }

        public int Period { get; set; }

        public int? ProgrammeType { get; set; }

        public int? FrameworkCode { get; set; }

        public int? PathwayCode { get; set; }

        public int? StandardCode { get; set; }

        public decimal? AgreedPrice { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime LearningDeliveryStartDate { get; set; }

        public decimal TransactionType01 { get; set; }
        public decimal TransactionType02 { get; set; }
        public decimal TransactionType03 { get; set; }
        public decimal TransactionType04 { get; set; }
        public decimal TransactionType05 { get; set; }
        public decimal TransactionType06 { get; set; }
        public decimal TransactionType07 { get; set; }
        public decimal TransactionType08 { get; set; }
        public decimal TransactionType09 { get; set; }
        public decimal TransactionType10 { get; set; }
        public decimal TransactionType11 { get; set; }
        public decimal TransactionType12 { get; set; }

        public decimal TransactionType15 { get; set; }
        public decimal TransactionType16 { get; set; }

        public DateTime? FirstIncentiveCensusDate { get; set; }
        public DateTime? SecondIncentiveCensusDate { get; set; }
        public DateTime? LearnerAdditionalPaymentsDate { get; set; }
    }
}
