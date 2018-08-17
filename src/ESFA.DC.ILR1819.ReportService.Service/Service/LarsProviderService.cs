using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.Data.LARS.Model;
using ESFA.DC.Data.LARS.Model.Interfaces;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Model;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Lars;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public sealed class LarsProviderService : ILarsProviderService
    {
        private readonly ILogger _logger;

        private readonly ILarsConfiguration _larsConfiguration;

        private readonly SemaphoreSlim _getLearningDeliveriesLock;

        private readonly SemaphoreSlim _getFrameworkAimsLock;

        private readonly SemaphoreSlim _getVersionLock;

        private Dictionary<string, ILarsLearningDelivery> _loadedLearningDeliveries;

        private Dictionary<string, ILarsFrameworkAim> _loadedFrameworkAims;

        private string _version;

        public LarsProviderService(ILogger logger, ILarsConfiguration larsConfiguration)
        {
            _logger = logger;
            _larsConfiguration = larsConfiguration;
            _loadedLearningDeliveries = null;
            _loadedFrameworkAims = null;
            _version = null;
            _getLearningDeliveriesLock = new SemaphoreSlim(1, 1);
            _getFrameworkAimsLock = new SemaphoreSlim(1, 1);
            _getVersionLock = new SemaphoreSlim(1, 1);
    }

        public async Task<Dictionary<string, ILarsLearningDelivery>> GetLearningDeliveries(List<string> validLearnerAimRefs, CancellationToken cancellationToken)
        {
            await _getLearningDeliveriesLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (_loadedLearningDeliveries == null)
                {
                    ILARS larsContext = new LARS(_larsConfiguration.LarsConnectionString);
                    _loadedLearningDeliveries = await larsContext.LARS_LearningDelivery.Where(
                            x => validLearnerAimRefs.Contains(x.LearnAimRef)).ToDictionaryAsync(
                            k => k.LearnAimRef,
                            v => (ILarsLearningDelivery)new LarsLearningDelivery
                            {
                                LearningAimTitle = v.LearnAimRefTitle,
                                NotionalNvqLevel = v.NotionalNVQLevel,
                                Tier2SectorSubjectArea = v.SectorSubjectAreaTier2
                            },
                            cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS learning deliveries", ex);
            }
            finally
            {
                _getLearningDeliveriesLock.Release();
            }

            return _loadedLearningDeliveries;
        }

        public async Task<Dictionary<string, ILarsFrameworkAim>> GetFrameworkAims(List<string> validLearnerAimRefs, CancellationToken cancellationToken)
        {
            await _getFrameworkAimsLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (_loadedLearningDeliveries == null)
                {
                    ILARS larsContext = new LARS(_larsConfiguration.LarsConnectionString);
                    _loadedFrameworkAims = await larsContext.LARS_FrameworkAims
                        .Where(x => validLearnerAimRefs.Contains(x.LearnAimRef)).ToDictionaryAsync(
                            k => k.LearnAimRef, v => (ILarsFrameworkAim)new LarsFrameworkAim()
                            {
                                FrameworkComponentType = v.FrameworkComponentType
                            });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS framework aims", ex);
            }
            finally
            {
                _getFrameworkAimsLock.Release();
            }

            return _loadedFrameworkAims;
        }

        public async Task<string> GetVersionAsync(CancellationToken cancellationToken)
        {
            await _getVersionLock.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(_version))
                {
                    ILARS larsContext = new LARS(_larsConfiguration.LarsConnectionString);
                    _version = (await larsContext.Current_Version.SingleAsync(cancellationToken)).CurrentVersion;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get LARS version information", ex);
            }
            finally
            {
                _getVersionLock.Release();
            }

            return _version;
        }
    }
}
