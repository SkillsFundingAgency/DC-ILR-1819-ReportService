using System;
using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Comparer
{
    public sealed class MathsAndEnglishModelComparer : IComparer<MathsAndEnglishModel>
    {
        private readonly string[] stringOrder = {
            "14-16 Direct Funded Students",
            "16-19 Students (excluding High Needs Students)",
            "16-19 High Needs Students",
            "19-24 Students with an EHCP",
            "19+ Continuing Students (excluding EHCP)"
        };

        public int Compare(MathsAndEnglishModel x, MathsAndEnglishModel y)
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

            return string.Compare(x.LearnRefNumber, y.LearnRefNumber, StringComparison.OrdinalIgnoreCase);
        }

        private int Compare(MathsAndEnglishModel x, MathsAndEnglishModel y, string str)
        {
            bool x1 = string.Equals(x.FundLine, str, StringComparison.OrdinalIgnoreCase);
            bool y1 = string.Equals(y.FundLine, str, StringComparison.OrdinalIgnoreCase);

            if (x1 && y1)
            {
                return 0;
            }

            if (x1)
            {
                return -1;
            }

            if (y1)
            {
                return 1;
            }

            return int.MaxValue;
        }
    }
}