﻿namespace ESFA.DC.ILR1819.ReportService.Model.Poco
{
    public sealed class ValidationErrorDetails
    {
        public string RuleName { get; }

        public string Message { get; set; }

        public string Severity { get; set; }

        public ValidationErrorDetails(string ruleName)
        {
            RuleName = ruleName;
        }
    }
}
