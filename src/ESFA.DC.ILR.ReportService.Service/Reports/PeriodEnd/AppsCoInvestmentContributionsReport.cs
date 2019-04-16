using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.ILR.ReportService.Service.Mapper.PeriodEnd;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Reports.PeriodEnd
{
    public sealed class AppsCoInvestmentContributionsReport : AbstractReport, IReport
    {
        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IDASPaymentsProviderService _dasPaymentsProviderService;

        public AppsCoInvestmentContributionsReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IIlrProviderService ilrProviderService,
            IDASPaymentsProviderService dasPaymentsProviderService)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _ilrProviderService = ilrProviderService;
            _dasPaymentsProviderService = dasPaymentsProviderService;
            ReportFileName = "Apps Co-Investment Contributions Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateAppsCoInvestmentContributionsReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            long jobId = reportServiceContext.JobId;
            string ukPrn = reportServiceContext.Ukprn.ToString();
            string externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            string fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            var appsCoInvestmentIlrInfo = await _ilrProviderService.GetILRInfoForAppsCoInvestmentReportAsync(reportServiceContext.Ukprn, cancellationToken);
            var appsCoInvestmentPaymentsInfo = await _dasPaymentsProviderService.GetPaymentsInfoForAppsCoInvestmentReportAsync(reportServiceContext.Ukprn, cancellationToken);
            string csv = await GetCsv(reportServiceContext, cancellationToken);
            await _storage.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<AppsCoInvestmentContributionsModel> appsCoInvestmentContributionsModels = new List<AppsCoInvestmentContributionsModel>();

            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsCoInvestmentContributionsMapper, AppsCoInvestmentContributionsModel>(csvWriter, appsCoInvestmentContributionsModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
