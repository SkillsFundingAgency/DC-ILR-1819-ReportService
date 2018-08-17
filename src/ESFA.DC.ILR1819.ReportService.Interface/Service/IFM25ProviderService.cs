using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.ILR1819.ReportService.Interface.Service
{
    public interface IFM25ProviderService
    {
        Task<Learner> GetFM25Data(IJobContextMessage jobContextMessage, CancellationToken cancellationToken);
    }
}