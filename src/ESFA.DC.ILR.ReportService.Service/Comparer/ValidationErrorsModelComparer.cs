using System;
using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Comparer
{
    public sealed class ValidationErrorsModelComparer : IComparer<ValidationErrorModel>
    {
        public int Compare(ValidationErrorModel x, ValidationErrorModel y)
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

            if (IsError(x) && !IsError(y))
            {
                return -1;
            }

            if (!IsError(x) && IsError(y))
            {
                return 1;
            }

            return string.Compare(x.RuleName, y.RuleName, StringComparison.OrdinalIgnoreCase);

            //if ((IsError(x) && IsError(y)) || (IsWarning(x) && IsWarning(y)))
            //{

            //}
        }

        private bool IsError(ValidationErrorModel x)
        {
            return string.Equals(x.Severity, "E", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(x.Severity, "F", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsWarning(ValidationErrorModel x)
        {
            return string.Equals(x.Severity, "W", StringComparison.OrdinalIgnoreCase);
        }
    }
}
