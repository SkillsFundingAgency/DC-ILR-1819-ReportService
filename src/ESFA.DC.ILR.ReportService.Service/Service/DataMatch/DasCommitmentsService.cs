using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Data.DAS.Model;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.Logging.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.ILR.ReportService.Service.Service.DataMatch
{
    public sealed class DasCommitmentsService : IDasCommitmentsService
    {
        private static readonly IEqualityComparer<DasCommitments> Comparer = new DasCommitmentsComparer();

        private readonly DasCommitmentsConfiguration _dasCommitmentsConfiguration;
        private readonly IDasCommitmentBuilder _dasCommitmentBuilder;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _getDataLock;
        private bool _loadedDataAlready;
        private List<DasCommitment> _dasCommitments;

        public DasCommitmentsService(
            DasCommitmentsConfiguration dasCommitmentsConfiguration,
            IDasCommitmentBuilder dasCommitmentBuilder,
            ILogger logger)
        {
            _dasCommitmentsConfiguration = dasCommitmentsConfiguration;
            _dasCommitmentBuilder = dasCommitmentBuilder;
            _logger = logger;

            _getDataLock = new SemaphoreSlim(1, 1);
        }

        public async Task<List<DasCommitment>> GetCommitments(
            long ukPrn,
            List<long> ulns,
            CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _dasCommitments;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                _loadedDataAlready = true;

                List<DasCommitments> dasCommitments = new List<DasCommitments>();

                DbContextOptions<DAS_commitmentsRefContext> options = new DbContextOptionsBuilder<DAS_commitmentsRefContext>().UseSqlServer(_dasCommitmentsConfiguration.DasCommitmentsConnectionString).Options;

                using (DAS_commitmentsRefContext dasCommitmentsRefContext = new DAS_commitmentsRefContext(options))
                {
                    int pages = (int)Math.Ceiling(ulns.Count / 1000M);

                    for (int i = 0; i < pages; i++)
                    {
                        long[] ulnsPage = ulns.Skip(i * 1000).Take(1000).ToArray();
                        DasCommitments[] results = await dasCommitmentsRefContext.DasCommitments
                            .Where(x => (i == 0 && x.Ukprn == ukPrn) || ulnsPage.Contains(x.Uln))
                            .ToArrayAsync(cancellationToken);
                        dasCommitments.AddRange(results);
                    }

                    _dasCommitments = _dasCommitmentBuilder.Build(dasCommitments.Distinct(Comparer));
                }
            }
            catch (Exception ex)
            {
                // Todo: Check behaviour
                _logger.LogError("Failed to get DAS Commitments data", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _dasCommitments;
        }
    }
}
