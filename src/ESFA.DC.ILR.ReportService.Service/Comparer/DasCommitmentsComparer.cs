using System.Collections.Generic;
using ESFA.DC.Data.DAS.Model;

namespace ESFA.DC.ILR.ReportService.Service.Comparer
{
    public sealed class DasCommitmentsComparer : IEqualityComparer<DasCommitments>
    {
        public bool Equals(DasCommitments x, DasCommitments y)
        {
            if (x == null && y != null)
            {
                return false;
            }

            if (x != null && y == null)
            {
                return false;
            }

            if (x == null && y == null)
            {
                return true;
            }

            return x.CommitmentId == y.CommitmentId && x.VersionId == y.VersionId;
        }

        public int GetHashCode(DasCommitments obj)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + obj.CommitmentId.GetHashCode();
                hash = (hash * 23) + obj.VersionId.GetHashCode();
                return hash;
            }
        }
    }
}
