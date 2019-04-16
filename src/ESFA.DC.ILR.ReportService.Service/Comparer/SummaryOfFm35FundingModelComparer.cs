using System;
using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Comparer
{
    public sealed class SummaryOfFm35FundingModelComparer : IComparer<SummaryOfFm35FundingModel>
    {
        private readonly string[] stringOrder =
        {
            "16-18 Apprenticeship",
            "19-23 Apprenticeship",
            "24+ Apprenticeship",
            "19-24 Traineeship (non-procured)",
            "19-24 Traineeship (procured from Nov 2017)",
            "AEB – Other Learning (non-procured)",
            "AEB – Other Learning (procured from Nov 2017)"
        };

        public int Compare(SummaryOfFm35FundingModel x, SummaryOfFm35FundingModel y)
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

            foreach (string str in stringOrder)
            {
                int cmp = Compare(x, y, str);
                if (cmp == 0)
                {
                    break;
                }

                if (cmp == 1)
                {
                    return 1;
                }

                if (cmp == -1)
                {
                    return -1;
                }
            }

            if (x.Period > y.Period)
            {
                return 1;
            }

            if (x.Period < y.Period)
            {
                return -1;
            }

            return 0;
        }

        private int Compare(SummaryOfFm35FundingModel x, SummaryOfFm35FundingModel y, string str)
        {
            bool x1 = string.Equals(x.FundingLineType, str, StringComparison.OrdinalIgnoreCase);
            bool y1 = string.Equals(y.FundingLineType, str, StringComparison.OrdinalIgnoreCase);

            if (x1 && y1)
            {
                return 0;
            }

            if (x1)
            {
                return 1;
            }

            if (y1)
            {
                return -1;
            }

            return int.MaxValue;
        }
    }
}
