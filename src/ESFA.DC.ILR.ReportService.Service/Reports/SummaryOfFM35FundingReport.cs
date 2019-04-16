using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public class SummaryOfFm35FundingReport : AbstractReport, IReport
    {
        private static readonly SummaryOfFm35FundingModelComparer comparer = new SummaryOfFm35FundingModelComparer();

        private readonly IFM35ProviderService _fm35ProviderService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IKeyValuePersistenceService _storage;
        private readonly ILogger _logger;
        private readonly IFm35Builder _summaryOfFm35FundingModelBuilder;

        public SummaryOfFm35FundingReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IFM35ProviderService fm35ProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IFm35Builder builder)
            : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _fm35ProviderService = fm35ProviderService;
            _stringUtilitiesService = stringUtilitiesService;
            _summaryOfFm35FundingModelBuilder = builder;

            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateSummaryOfFM35FundingReport;
            ReportFileName = "Summary of Funding Model 35 Funding Report";
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            var jobId = reportServiceContext.JobId;
            var ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            IList<SummaryOfFm35FundingModel> summaryOfFm35FundingModels = await GetSummaryOfFm35FundingModels(reportServiceContext, cancellationToken);
            if (summaryOfFm35FundingModels == null)
            {
                return;
            }

            string csv = GetCsv(summaryOfFm35FundingModels);
            await _storage.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private string GetCsv(IList<SummaryOfFm35FundingModel> summaryOfFm35FundingModels)
        {
            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<SummaryOfFM35FundingMapper, SummaryOfFm35FundingModel>(csvWriter, summaryOfFm35FundingModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private async Task<IList<SummaryOfFm35FundingModel>> GetSummaryOfFm35FundingModels(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            Task<FM35Global> fm35Task = _fm35ProviderService.GetFM35Data(reportServiceContext, cancellationToken);

            await Task.WhenAll(fm35Task);

            cancellationToken.ThrowIfCancellationRequested();

            List<SummaryOfFm35FundingModel> summaryOfFm35FundingModels = new List<SummaryOfFm35FundingModel>();

            FM35Global fm35Data = fm35Task.Result;
            if (fm35Data?.Learners == null)
            {
                _logger.LogWarning($"No Fm35 data for {nameof(SummaryOfFm35FundingReport)}");
                return summaryOfFm35FundingModels;
            }

            foreach (FM35Learner learnerAttribute in fm35Data.Learners)
            {
                foreach (LearningDelivery fundLineData in learnerAttribute.LearningDeliveries)
                {
                    summaryOfFm35FundingModels.AddRange(_summaryOfFm35FundingModelBuilder.BuildModel(fundLineData));
                }
            }

            summaryOfFm35FundingModels.Sort(comparer);
            return summaryOfFm35FundingModels;
        }
    }
}
