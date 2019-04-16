using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Interface.Context;

namespace ESFA.DC.ILR.ReportService.Interface.Reports
{
    public interface IReport
    {
        string ReportTaskName { get; }

        string GetFilename(string ukPrn, long jobId, DateTime submissionDateTime);

        Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken);

        bool IsMatch(string reportTaskName);
    }
}
