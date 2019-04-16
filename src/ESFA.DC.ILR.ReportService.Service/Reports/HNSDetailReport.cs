using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
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
    public sealed class HNSDetailReport : AbstractReport, IReport
    {
        private static readonly HNSModelComparer _hnsModelComparer = new HNSModelComparer();

        private readonly ILogger _logger;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IFM25ProviderService _fm25ProviderService;
        private readonly IHNSReportModelBuilder _hnsReportModelBuilder;

        public HNSDetailReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IFM25ProviderService fm25ProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IHNSReportModelBuilder hnsReportModelBuilder)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _ilrProviderService = ilrProviderService;
            _validLearnersService = validLearnersService;
            _fm25ProviderService = fm25ProviderService;
            _hnsReportModelBuilder = hnsReportModelBuilder;

            ReportFileName = "High Needs Students Detail Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateHNSReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<FM25Global> fm25Task = _fm25ProviderService.GetFM25Data(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, fm25Task, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<ILearner> learners =
                ilrFileTask.Result?.Learners?.Where(x => validLearnersTask.Result.Contains(x.LearnRefNumber)).ToList();
            if (learners == null)
            {
                _logger.LogWarning("Failed to get learners for High Needs Students Detail Report");
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<HNSModel> hnsModels = new List<HNSModel>();
            foreach (var learner in learners)
            {
                if (learner.LearningDeliveries == null)
                {
                    continue;
                }

                FM25Global fm25Data = fm25Task.Result;

                foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                {
                    FM25Learner fm25Learner =
                        fm25Data?.Learners?.SingleOrDefault(l => string.Equals(l.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase));

                    if (fm25Learner == null)
                    {
                        continue;
                    }

                    if (!CheckIsApplicableLearner(learningDelivery, fm25Learner))
                    {
                        continue;
                    }

                    hnsModels.Add(_hnsReportModelBuilder.BuildModel(
                        learner,
                        learningDelivery,
                        fm25Learner));
                }
            }

            hnsModels.Sort(_hnsModelComparer);

            string csv = GetReportCsv(hnsModels);

            var jobId = reportServiceContext.JobId;
            var ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private bool CheckIsApplicableLearner(ILearningDelivery learningDelivery, FM25Learner fm25Learner)
        {
            return learningDelivery.FundModel == 25 && learningDelivery.LearningDeliveryFAMs.Any(fam =>
                       string.Equals(fam.LearnDelFAMType, Constants.LearningDeliveryFAMCodeSOF, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(fam.LearnDelFAMCode, Constants.LearningDeliveryFAMCode107, StringComparison.OrdinalIgnoreCase)) &&
                   (fm25Learner.StartFund.HasValue && fm25Learner.StartFund.Value);
        }

        private string GetReportCsv(List<HNSModel> hnsModels)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<HNSMapper, HNSModel>(csvWriter, hnsModels, new HNSMapper());
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
