using ESFA.DC.ILR.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR.ReportService.Service.Extensions.DataMatch
{
    public static class EarningExtensions
    {
        public static bool HasFirstIncentive(this RawEarning earning)
        {
            return (earning.TransactionType04 != 0 || earning.TransactionType05 != 0) &&
                   earning.FirstIncentiveCensusDate.HasValue;
        }

        public static bool HasSecondIncentive(this RawEarning earning)
        {
            return (earning.TransactionType06 != 0 || earning.TransactionType07 != 0) &&
                   earning.SecondIncentiveCensusDate.HasValue;
        }

        public static bool HasCompletionPayment(this RawEarning earning)
        {
            return (earning.TransactionType02 != 0 ||
                     earning.TransactionType03 != 0 ||
                     earning.TransactionType09 != 0 ||
                     earning.TransactionType10 != 0) &&
                    earning.EndDate.HasValue;
        }

        public static bool HasCareLeaverApprenticePayment(this RawEarning earning)
        {
            return earning.TransactionType16 != 0 &&
                   earning.LearnerAdditionalPaymentsDate.HasValue;
        }

        public static bool HasNonIncentiveEarnings(this RawEarning earning)
        {
            return earning.TransactionType01 != 0 ||
                    earning.TransactionType08 != 0 ||
                    earning.TransactionType11 != 0 ||
                    earning.TransactionType12 != 0 ||
                    earning.TransactionType15 != 0 ||
                    (
                        earning.Period == 1 &&
                        !earning.HasFirstIncentive() &&
                        !earning.HasSecondIncentive() &&
                        !earning.HasCareLeaverApprenticePayment());
        }
    }
}
