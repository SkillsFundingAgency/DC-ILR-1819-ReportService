using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.EAS1819.EF;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.Eas;
using ESFA.DC.ILR.ReportService.Service.Extensions.Eas;
using ESFA.DC.Logging.Interfaces;
using EasSubmissionValues = ESFA.DC.ILR.ReportService.Model.Eas.EasSubmissionValues;

namespace ESFA.DC.ILR.ReportService.Service.Service
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

                var easDbContext = new EasdbContext(_easConfiguration.EasConnectionString);
                var easSubmission = easDbContext.EasSubmission.Where(x => x.Ukprn == ukprn.ToString())
                    .OrderByDescending(x => x.UpdatedOn).FirstOrDefault();

                _loadedLastEasUpdate[ukprn] = easSubmission?.UpdatedOn ?? DateTime.MinValue;
            }
            finally
            {
                _getLastEastUpdateLock.Release();
            }

            return _loadedLastEasUpdate[ukprn];
        }

        public List<EasPaymentType> GetAllPaymentTypes()
        {
            var easDbContext = new EasdbContext(_easConfiguration.EasConnectionString);
            var paymentTypesList = easDbContext.PaymentTypes.OrderBy(s => s.PaymentId).ThenBy(s => s.PaymentName).ToList();
            List<EasPaymentType> easPaymentTypeList = paymentTypesList.Select(x => x.ToEasPaymentTypes()).ToList();
            return easPaymentTypeList;
        }

        public async Task<List<EasSubmissionValues>> GetEasSubmissionValuesAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            await _getLastEastUpdateLock.WaitAsync(cancellationToken);
            string UkPrn = reportServiceContext.Ukprn.ToString();
            var allPaymentTypes = GetAllPaymentTypes();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<EasSubmissionValues> easSubmissionValues = new List<EasSubmissionValues>();
                var easDbContext = new EasdbContext(_easConfiguration.EasConnectionString);
                var easSubmission = easDbContext.EasSubmission.Where(x => x.Ukprn == UkPrn)
                    .OrderByDescending(x => x.UpdatedOn).FirstOrDefault();
                if (easSubmission != null)
                {
                    var easSubmissionValuesList = easDbContext.EasSubmissionValues.Where(x => x.SubmissionId == easSubmission.SubmissionId).ToList();
                    easSubmissionValues = easSubmissionValuesList.Select(x => new EasSubmissionValues()
                    {
                        PaymentTypeName = GetPaymentNameById(x.PaymentId, allPaymentTypes),
                        PaymentId = x.PaymentId,
                        CollectionPeriod = x.CollectionPeriod,
                        PaymentValue = x.PaymentValue
                    }).ToList();
                }

                _loadedEasSubmissionValuesList = easSubmissionValues;
            }
            finally
            {
                _getLastEastUpdateLock.Release();
            }

            return _loadedEasSubmissionValuesList;
        }

        private string GetPaymentNameById(int paymentId, List<EasPaymentType> allPaymentTypes)
        {
            var payment = allPaymentTypes.FirstOrDefault(pt => pt.PaymentId == paymentId);
            if (payment != null)
            {
                return payment.PaymentName;
            }

            return string.Empty;
        }
    }
}
