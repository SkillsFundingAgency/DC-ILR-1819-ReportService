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
    public sealed class MathsAndEnglishReport : AbstractReport, IReport
    {
        private static readonly MathsAndEnglishModelComparer MathsAndEnglishModelComparer = new MathsAndEnglishModelComparer();

        private readonly ILogger _logger;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IFM25ProviderService _fm25ProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IMathsAndEnglishFm25Rules _mathsAndEnglishFm25Rules;
        private readonly IMathsAndEnglishModelBuilder _mathsAndEnglishModelBuilder;

        public MathsAndEnglishReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IFM25ProviderService fm25ProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IMathsAndEnglishFm25Rules mathsAndEnglishFm25Rules,
            IMathsAndEnglishModelBuilder mathsAndEnglishModelBuilder,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _ilrProviderService = ilrProviderService;
            _fm25ProviderService = fm25ProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;
            _mathsAndEnglishFm25Rules = mathsAndEnglishFm25Rules;
            _mathsAndEnglishModelBuilder = mathsAndEnglishModelBuilder;

            ReportFileName = "Maths and English Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateMathsAndEnglishReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            var jobId = reportServiceContext.JobId;
            var ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            string csv = await GetCsv(reportServiceContext, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);
            Task<FM25Global> fm25Task = _fm25ProviderService.GetFM25Data(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, validLearnersTask, fm25Task);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            List<string> ilrError = new List<string>();

            List<MathsAndEnglishModel> mathsAndEnglishModels = new List<MathsAndEnglishModel>();
            if (fm25Task.Result?.Learners != null)
            {
                foreach (string validLearnerRefNum in validLearnersTask.Result)
                {
                    ILearner learner =
                        ilrFileTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);

                    FM25Learner fm25Learner =
                        fm25Task.Result.Learners.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);

                    if (learner == null || fm25Learner == null)
                    {
                        ilrError.Add(validLearnerRefNum);
                        continue;
                    }

                    if (!_mathsAndEnglishFm25Rules.IsApplicableLearner(fm25Learner))
                    {
                        continue;
                    }

                    mathsAndEnglishModels.Add(_mathsAndEnglishModelBuilder.BuildModel(learner, fm25Learner));
                }
            }

            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating {nameof(MathsAndEnglishReport)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
            }

            mathsAndEnglishModels.Sort(MathsAndEnglishModelComparer);

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<MathsAndEnglishMapper, MathsAndEnglishModel>(csvWriter, mathsAndEnglishModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}
