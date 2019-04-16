using System;
using System.Collections.Generic;

namespace ESFA.DC.ILR.ReportService.Interface.Context
{
    public interface IReportServiceContext
    {
        long JobId { get; }

        int Ukprn { get; }

        string Filename { get; }

        string OriginalFilename { get; }

        DateTime SubmissionDateTimeUtc { get; }

        long FileSizeInBytes { get; }

        int ValidLearnRefNumbersCount { get; }

        int InvalidLearnRefNumbersCount { get; }

        int ValidationTotalErrorCount { get; }

        int ValidationTotalWarningCount { get; }

        string ValidationErrorsKey { get; }

        string ValidationErrorsLookupsKey { get; }

        string FundingFM81OutputKey { get; }

        string FundingFM70OutputKey { get; }

        string FundingFM36OutputKey { get; }

        string FundingFM35OutputKey { get; }

        string FundingFM25OutputKey { get; }

        string FundingALBOutputKey { get; }

        string ValidLearnRefNumbersKey { get; }

        IEnumerable<string> Tasks { get; }

        string InvalidLearnRefNumbersKey { get; }

        string CollectionName { get; }

        int ReturnPeriod { get; }
    }
}
