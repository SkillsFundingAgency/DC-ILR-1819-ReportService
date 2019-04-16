using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IDasCommitmentsService
    {
        Task<List<DasCommitment>> GetCommitments(long ukPrn, List<long> ulns, CancellationToken cancellationToken);
    }
}
