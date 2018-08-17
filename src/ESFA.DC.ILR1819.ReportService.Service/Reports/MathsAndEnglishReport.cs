using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Reports;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;
using ESFA.DC.ILR1819.ReportService.Service.Mapper;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Reports
{
    public sealed class MathsAndEnglishReport : AbstractReportBuilder, IReport
    {
        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IFM25ProviderService _fm25ProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IMathsAndEnglishFm25Rules _mathsAndEnglishFm25Rules;
        private readonly IMathsAndEnglishModelBuilder _mathsAndEnglishModelBuilder;

        public MathsAndEnglishReport(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService storage,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IFM25ProviderService fm25ProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IDateTimeProvider dateTimeProvider,
            IMathsAndEnglishFm25Rules mathsAndEnglishFm25Rules,
            IMathsAndEnglishModelBuilder mathsAndEnglishModelBuilder,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider)
        {
            _logger = logger;
            _storage = storage;
            _ilrProviderService = ilrProviderService;
            _fm25ProviderService = fm25ProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;
            _mathsAndEnglishFm25Rules = mathsAndEnglishFm25Rules;
            _mathsAndEnglishModelBuilder = mathsAndEnglishModelBuilder;

            ReportFileName = "Maths and English Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateMathsAndEnglishReport;
        }

        public async Task GenerateReport(IJobContextMessage jobContextMessage, ZipArchive archive, CancellationToken cancellationToken)
        {
            var jobId = jobContextMessage.JobId;
            var ukPrn = jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString();
            var fileName = GetReportFilename(ukPrn, jobId);

            string csv = await GetCsv(jobContextMessage, cancellationToken);
            await _storage.SaveAsync($"{fileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(jobContextMessage, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(jobContextMessage, cancellationToken);
            Task<Learner> fm25Task = _fm25ProviderService.GetFM25Data(jobContextMessage, cancellationToken);

            await Task.WhenAll(ilrFileTask, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var ilrError = new List<string>();

            var mathsAndEnglishModels = new List<MathsAndEnglishModel>(validLearnersTask.Result.Count);
            foreach (string validLearnerRefNum in validLearnersTask.Result)
            {
                var learner =
                    ilrFileTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);

                var fm25Data = fm25Task.Result;
                if (learner == null || fm25Data == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                if (!_mathsAndEnglishFm25Rules.IsApplicableLearner(fm25Data))
                {
                    continue;
                }

                mathsAndEnglishModels.Add((MathsAndEnglishModel)_mathsAndEnglishModelBuilder.BuildModel(learner, fm25Data));
            }

            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating {nameof(MathsAndEnglishReport)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                BuildCsvReport<MathsAndEnglishMapper, MathsAndEnglishModel>(ms, mathsAndEnglishModels);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
