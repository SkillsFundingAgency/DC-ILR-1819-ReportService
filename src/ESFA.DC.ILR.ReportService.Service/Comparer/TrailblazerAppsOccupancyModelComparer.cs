using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Comparer
{
    public class TrailblazerAppsOccupancyModelComparer : IComparer<TrailblazerAppsOccupancyModel>
    {
        public int Compare(TrailblazerAppsOccupancyModel x, TrailblazerAppsOccupancyModel y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (x == y)
            {
                return 0;
            }

            int cmp = string.CompareOrdinal(x.LearnRefNumber, y.LearnRefNumber);
            if (cmp != 0)
            {
                return cmp;
            }

            if (x.AimSeqNumber > y.AimSeqNumber)
            {
                return 1;
            }

            if (x.AimSeqNumber < y.AimSeqNumber)
            {
                return -1;
            }

            return 0;
        }
    }
}