using System.Threading;
using System.Threading.Tasks;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface ILargeEmployerProviderService
    {
        Task<string> GetVersionAsync(CancellationToken cancellationToken);
    }
}
