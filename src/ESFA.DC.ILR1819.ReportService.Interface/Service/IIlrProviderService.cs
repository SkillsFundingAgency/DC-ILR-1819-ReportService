using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.ILR1819.ReportService.Interface.Service
{
    public interface IIlrProviderService
    {
        Task<IMessage> GetIlrFile(IJobContextMessage jobContextMessage, CancellationToken cancellationToken);
    }
}
