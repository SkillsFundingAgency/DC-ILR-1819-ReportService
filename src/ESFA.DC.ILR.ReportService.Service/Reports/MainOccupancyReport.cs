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
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using LearningDelivery = ESFA.DC.ILR.ReportService.Model.Lars.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public sealed class MainOccupancyReport : AbstractReport, IReport
    {
        private static readonly MainOccupancyModelComparer MainOccupancyModelComparer = new MainOccupancyModelComparer();

        private readonly ILogger _logger;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IFM25ProviderService _fm25ProviderService;
        private readonly IFM35ProviderService _fm35ProviderService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IMainOccupancyReportModelBuilder _mainOccupancyReportModelBuilder;

        public MainOccupancyReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrProviderService ilrProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IValidLearnersService validLearnersService,
            IFM25ProviderService fm25ProviderService,
            IFM35ProviderService fm35ProviderService,
            ILarsProviderService larsProviderService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IMainOccupancyReportModelBuilder mainOccupancyReportModelBuilder)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _ilrProviderService = ilrProviderService;
            _stringUtilitiesService = stringUtilitiesService;
            _validLearnersService = validLearnersService;
            _fm25ProviderService = fm25ProviderService;
            _fm35ProviderService = fm35ProviderService;
            _larsProviderService = larsProviderService;
            _mainOccupancyReportModelBuilder = mainOccupancyReportModelBuilder;

            ReportFileName = "Main Occupancy Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateMainOccupancyReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<FM25Global> fm25Task = _fm25ProviderService.GetFM25Data(reportServiceContext, cancellationToken);
            Task<FM35Global> fm35Task = _fm35ProviderService.GetFM35Data(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, fm25Task, fm35Task, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<ILearner> learners =
                ilrFileTask.Result?.Learners?.Where(x => validLearnersTask.Result.Contains(x.LearnRefNumber)).ToList();
            if (learners == null)
            {
                _logger.LogWarning("Failed to get learners for Main Occupancy Report");
                return;
            }

            string[] learnAimRefs = learners.SelectMany(x => x.LearningDeliveries).Select(x => x.LearnAimRef).Distinct().ToArray();

            Task<Dictionary<string, LarsLearningDelivery>> larsLearningDeliveriesTask = _larsProviderService.GetLearningDeliveriesAsync(learnAimRefs, cancellationToken);
            Task<List<LearnerAndDeliveries>> larsFrameworkAimsTask = _larsProviderService.GetFrameworkAimsAsync(learnAimRefs, learners, cancellationToken);

            await Task.WhenAll(larsLearningDeliveriesTask, larsFrameworkAimsTask);

            if (larsLearningDeliveriesTask.Result == null || larsFrameworkAimsTask.Result == null)
            {
                _logger.LogWarning("Failed to get LARS data for Main Occupancy Report");
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<string> larsErrors = new List<string>();

            List<MainOccupancyModel> mainOccupancyModels = new List<MainOccupancyModel>();
            foreach (var learner in learners)
            {
                if (learner.LearningDeliveries == null)
                {
                    continue;
                }

                FM25Global fm25Data = fm25Task.Result;
                FM35Global fm35Data = fm35Task.Result;

                foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                {
                    if (!CheckIsApplicableLearner(learningDelivery))
                    {
                        continue;
                    }

                    if (!larsLearningDeliveriesTask.Result.TryGetValue(learningDelivery.LearnAimRef, out LarsLearningDelivery larsModel))
                    {
                        larsErrors.Add(learningDelivery.LearnAimRef);
                        continue;
                    }

                    LearningDelivery frameworkAim = larsFrameworkAimsTask.Result?.SingleOrDefault(x => string.Equals(x.LearnerLearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase))
                        ?.LearningDeliveries?.SingleOrDefault(x => string.Equals(x.LearningDeliveryLearnAimRef, learningDelivery.LearnAimRef, StringComparison.OrdinalIgnoreCase)
                                                                   && x.LearningDeliveryAimSeqNumber == learningDelivery.AimSeqNumber);
                    if (frameworkAim == null)
                    {
                        larsErrors.Add(learningDelivery.LearnAimRef);
                        continue;
                    }

                    if (learningDelivery.FundModel == 35)
                    {
                        ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery learnerFm35Data = fm35Data
                            ?.Learners?.SingleOrDefault(l => string.Equals(l.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase))
                            ?.LearningDeliveries?.SingleOrDefault(l => l.AimSeqNumber == learningDelivery.AimSeqNumber);

                        if (learnerFm35Data != null)
                        {
                            mainOccupancyModels.Add(_mainOccupancyReportModelBuilder.BuildFm35Model(
                                learner,
                                learningDelivery,
                                larsModel,
                                frameworkAim,
                                learnerFm35Data,
                                _stringUtilitiesService));
                        }
                    }

                    if (learningDelivery.FundModel != 25)
                    {
                        continue;
                    }

                    FM25Learner learnerFm25Data =
                        fm25Data?.Learners?.SingleOrDefault(l => string.Equals(l.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase));

                    mainOccupancyModels.Add(_mainOccupancyReportModelBuilder.BuildFm25Model(
                        learner,
                        learningDelivery,
                        learnerFm25Data));
                }
            }

            LogWarnings(larsErrors);

            mainOccupancyModels.Sort(MainOccupancyModelComparer);

            string csv = GetReportCsv(mainOccupancyModels);

            var jobId = reportServiceContext.JobId;
            var ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private bool CheckIsApplicableLearner(ILearningDelivery learningDelivery)
        {
            return learningDelivery.FundModel == 35
                    || (learningDelivery.FundModel == 25 && learningDelivery.LearningDeliveryFAMs.Any(fam =>
                              string.Equals(fam.LearnDelFAMType, "SOF", StringComparison.OrdinalIgnoreCase) &&
                              string.Equals(fam.LearnDelFAMCode, "105", StringComparison.OrdinalIgnoreCase)));
        }

        private string GetReportCsv(List<MainOccupancyModel> mainOccupancyModels)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<MainOccupancyMapper, MainOccupancyModel>(csvWriter, mainOccupancyModels, new MainOccupancyMapper());
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private void LogWarnings(List<string> larsErrors)
        {
            if (larsErrors.Any())
            {
                _logger.LogWarning($"Failed to get LARS data while generating Main Occupancy Report: {_stringUtilitiesService.JoinWithMaxLength(larsErrors)}");
            }
        }
    }
}
