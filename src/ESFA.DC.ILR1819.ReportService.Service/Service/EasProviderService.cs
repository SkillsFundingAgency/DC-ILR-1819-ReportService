﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.EAS1819.EF;
using ESFA.DC.EAS1819.EF.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Configuration;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public sealed class EasProviderService : IEasProviderService
    {
        private readonly ILogger _logger;

        private readonly EasConfiguration _easConfiguration;
        private readonly Dictionary<int, DateTime> _loadedLastEasUpdate;
        private List<EasSubmissionValues> _loadedEasSubmissionValuesList;
        private readonly SemaphoreSlim _getLastEastUpdateLock;

        public EasProviderService(ILogger logger, EasConfiguration easConfiguration)
        {
            _logger = logger;
            _easConfiguration = easConfiguration;
            _loadedLastEasUpdate = new Dictionary<int, DateTime>();
            _loadedEasSubmissionValuesList = new List<EasSubmissionValues>();

            _getLastEastUpdateLock = new SemaphoreSlim(1, 1);
        }

        public async Task<DateTime> GetLastEasUpdate(int ukprn, CancellationToken cancellationToken)
        {
            await _getLastEastUpdateLock.WaitAsync(cancellationToken);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_loadedLastEasUpdate.ContainsKey(ukprn))
                {
                    _loadedLastEasUpdate[ukprn] = DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS submission values", ex); // todo - error message text
            }
            finally
            {
                _getLastEastUpdateLock.Release();
            }

            return _loadedLastEasUpdate[ukprn];
        }

        public List<PaymentTypes> GetAllPaymentTypes()
        {
            var easDbContext = new EasdbContext(_easConfiguration.EasConnectionString);
            var paymentTypesList = easDbContext.PaymentTypes.OrderBy(s => s.PaymentId).ThenBy(s => s.PaymentName).ToList();
            return paymentTypesList;
        }

        public async Task<List<EasSubmissionValues>> GetEasSubmissionValuesAsync(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            await _getLastEastUpdateLock.WaitAsync(cancellationToken);
            string UkPrn = jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<EasSubmissionValues> easSubmissionValues = new List<EasSubmissionValues>();
                var easDbContext = new EasdbContext(_easConfiguration.EasConnectionString);
                var easSubmission = easDbContext.EasSubmission.Where(x => x.Ukprn == UkPrn)
                    .OrderByDescending(x => x.UpdatedOn).FirstOrDefault();
                if (easSubmission != null)
                {
                    easSubmissionValues = easDbContext.EasSubmissionValues
                        .Where(x => x.SubmissionId == easSubmission.SubmissionId).ToList();
                }

                _loadedEasSubmissionValuesList = easSubmissionValues;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get EAS submission values", ex);
            }
            finally
            {
                _getLastEastUpdateLock.Release();
            }

            return _loadedEasSubmissionValuesList;
        }
    }
}
