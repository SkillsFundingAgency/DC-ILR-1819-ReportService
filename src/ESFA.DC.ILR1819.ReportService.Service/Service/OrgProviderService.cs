using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Data.Organisatons.Model;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public sealed class OrgProviderService : IOrgProviderService
    {
        private readonly ILogger _logger;

        private readonly IOrgConfiguration _orgConfiguration;

        private readonly SemaphoreSlim _getDataLock = new SemaphoreSlim(1, 1);

        private bool _loadedDataAlready;

        private string _loadedData;

        private string _version;

        public OrgProviderService(ILogger logger, IOrgConfiguration orgConfiguration)
        {
            _logger = logger;
            _orgConfiguration = orgConfiguration;
        }

        public async Task<string> GetProviderName(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
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
                string ukPrnStr = jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString();
                long ukPrn = Convert.ToInt64(ukPrnStr);
                Organisations organisations = new Organisations(_orgConfiguration.OrgConnectionString);
                _loadedData = organisations.Org_Details.Where(x => x.UKPRN == ukPrn).Select(x => x.Name).SingleOrDefault();
                _version = (await organisations.Current_Version.SingleAsync(cancellationToken)).CurrentVersion;
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
                    Organisations organisations = new Organisations(_orgConfiguration.OrgConnectionString);
                    _version = (await organisations.Current_Version.SingleAsync(cancellationToken)).CurrentVersion;
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
    }
}
