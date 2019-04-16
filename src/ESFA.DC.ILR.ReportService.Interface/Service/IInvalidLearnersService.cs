using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Interface.Context;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IInvalidLearnersService
    {
        Task<List<string>> GetLearnersAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
