using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Interface.Context;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IOrgProviderService
    {
        Task<string> GetProviderName(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);

        Task<string> GetVersionAsync(CancellationToken cancellationToken);

        Task<decimal?> GetCofRemoval(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
