using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Context;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IFM25ProviderService
    {
        Task<FM25Global> GetFM25Data(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}