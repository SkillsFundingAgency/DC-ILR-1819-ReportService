using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR.ReportService.Service.Extensions.DataMatch
{
    public static class PriceEpisodePeriodMatchExtensions
    {
        public static bool MatchesEarning(this PriceEpisodePeriodMatchEntity source, RawEarning rhs)
        {
            return source.AimSeqNumber == rhs.AimSeqNumber &&
                   source.LearnRefNumber == rhs.LearnRefNumber &&
                   source.PriceEpisodeIdentifier == rhs.PriceEpisodeIdentifier &&
                   source.Ukprn == rhs.Ukprn &&
                   source.Period == rhs.Period;
        }

        public static bool MatchesCommitment(this PriceEpisodePeriodMatchEntity source, DasCommitment rhs)
        {
            return source.CommitmentId == rhs.CommitmentId &&
                   source.VersionId == rhs.VersionId;
        }

        public static bool DoesNotContainEarningForCommitmentAndPaymentType(
            this List<PriceEpisodePeriodMatchEntity> source,
            RawEarning earning,
            DasCommitment commitment,
            TransactionTypesFlag paymentType)
        {
            if (source.FirstOrDefault(x => x.MatchesEarning(earning) &&
                                           x.MatchesCommitment(commitment) &&
                                           x.Payable &&
                                           x.TransactionTypesFlag == paymentType) == null)
            {
                return true;
            }

            return false;
        }
    }
}
