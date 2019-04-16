using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR.ReportService.Service.Extensions.DataMatch
{
    public static class ValidationErrorExtensions
    {
        public static bool IsEqualTo(this IIdentifyCommitments source, IIdentifyCommitments rhs)
        {
            return source.LearnRefNumber == rhs.LearnRefNumber &&
                   source.AimSeqNumber == rhs.AimSeqNumber &&
                   source.PriceEpisodeIdentifier == rhs.PriceEpisodeIdentifier &&
                   source.Ukprn == rhs.Ukprn;
        }

        public static bool DoesNotAlreadyContainEarningForThisError(this List<DatalockValidationError> source, IIdentifyCommitments earning, string error)
        {
            if (source.FirstOrDefault(x => x.IsEqualTo(earning) && x.RuleId == error) == null)
            {
                return true;
            }

            return false;
        }

        public static bool DoesNotAlreadyContainEarningForThisError(
            this List<DatalockValidationErrorByPeriod> source,
            RawEarning earning,
            string error)
        {
            if (source.FirstOrDefault(x => x.IsEqualTo(earning) &&
                                           x.RuleId == error &&
                                           x.Period == earning.Period) == null)
            {
                return true;
            }

            return false;
        }
    }
}
