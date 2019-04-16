using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR.ReportService.Service.Extensions.DataMatch
{
    public static class PriceEpisodeMatchExtensions
    {
        public static bool MatchesEarning(this PriceEpisodeMatchEntity source, RawEarning rhs)
        {
            return source.LearnRefNumber == rhs.LearnRefNumber &&
                   source.PriceEpisodeIdentifier == rhs.PriceEpisodeIdentifier &&
                   source.Ukprn == rhs.Ukprn;
        }

        public static bool MatchesCommitment(this PriceEpisodeMatchEntity source, DasCommitment rhs)
        {
            return source.CommitmentId == rhs.CommitmentId;
        }

        public static bool DoesNotAlreadyContainEarningForCommitment(
            this List<PriceEpisodeMatchEntity> source,
            RawEarning earning,
            DasCommitment commitment,
            bool payable)
        {
            if (source.FirstOrDefault(x => x.MatchesEarning(earning) &&
                                           x.MatchesCommitment(commitment) &&
                                           x.IsSuccess == payable) == null)
            {
                return true;
            }

            return false;
        }
    }
}
