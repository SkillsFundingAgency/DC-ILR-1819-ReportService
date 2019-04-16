using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface IAllbBuilder
    {
        Task<List<FundingSummaryModel>> BuildAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken);
    }
}
