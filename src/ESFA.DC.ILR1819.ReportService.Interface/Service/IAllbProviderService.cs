using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.ILR1819.ReportService.Interface.Service
{
    public interface IAllbProviderService
    {
        Task<FundingOutputs> GetAllbData(IJobContextMessage jobContextMessage, CancellationToken cancellationToken);
    }
}
