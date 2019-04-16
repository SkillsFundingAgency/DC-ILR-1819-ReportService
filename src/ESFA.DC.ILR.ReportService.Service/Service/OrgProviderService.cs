using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.ReferenceData.Organisations.Model;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public sealed class OrgProviderService : IOrgProviderService
    {
        private readonly ILogger _logger;

        private readonly OrgConfiguration _orgConfiguration;

        private readonly SemaphoreSlim _getDataLock = new SemaphoreSlim(1, 1);

        private bool _loadedDataAlready;

        private string _loadedData;

        private string _version;

        private decimal? _cofRemoval;

        public OrgProviderService(ILogger logger, OrgConfiguration orgConfiguration)
        {
            _logger = logger;
            _orgConfiguration = orgConfiguration;
        }

        public async Task<string> GetProviderName(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (_loadedDataAlready)
                {
                    return _loadedData;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                _loadedDataAlready = true;
                string ukPrnStr = reportServiceContext.Ukprn.ToString();
                long ukPrn = Convert.ToInt64(ukPrnStr);
                DbContextOptions<OrganisationsContext> options = new DbContextOptionsBuilder<OrganisationsContext>().UseSqlServer(_orgConfiguration.OrgConnectionString).Options;
                using (OrganisationsContext organisations = new OrganisationsContext(options))
                {
                    _loadedData = organisations.OrgDetails.Where(x => x.Ukprn == ukPrn).Select(x => x.Name)
                        .SingleOrDefault();
                    await GetVersion(organisations, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get org provider name", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _loadedData;
        }

        public async Task<string> GetVersionAsync(CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(_version))
                {
                    DbContextOptions<OrganisationsContext> options = new DbContextOptionsBuilder<OrganisationsContext>().UseSqlServer(_orgConfiguration.OrgConnectionString).Options;
                    using (OrganisationsContext organisations = new OrganisationsContext(options))
                    {
                        await GetVersion(organisations, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get org version", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _version;
        }

        public async Task<decimal?> GetCofRemoval(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            await _getDataLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                string ukPrnStr = reportServiceContext.Ukprn.ToString();
                long ukPrn = Convert.ToInt64(ukPrnStr);
                DbContextOptions<OrganisationsContext> options = new DbContextOptionsBuilder<OrganisationsContext>().UseSqlServer(_orgConfiguration.OrgConnectionString).Options;
                using (OrganisationsContext organisations = new OrganisationsContext(options))
                {
                    _cofRemoval = organisations.ConditionOfFundingRemovals.Where(x => x.Ukprn == ukPrn).OrderByDescending(x => x.EffectiveFrom).Select(x => x.CoFremoval).SingleOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get org provider name", ex);
            }
            finally
            {
                _getDataLock.Release();
            }

            return _cofRemoval;
        }

        private async Task GetVersion(OrganisationsContext organisations, CancellationToken cancellationToken)
        {
            string version = "Unknown";
            OrgVersion versionObj = await organisations.OrgVersions.OrderByDescending(x => x.CreatedOn)
                .FirstOrDefaultAsync(cancellationToken);
            if (versionObj != null)
            {
                version = $"{versionObj.MajorNumber}.{versionObj.MinorNumber}.{versionObj.MaintenanceNumber}";
            }

            _version = version;
        }
    }
}