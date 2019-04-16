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
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
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
using LearningDelivery = ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public sealed class AppsIndicativeEarningsReport : AbstractReport, IReport
    {
        private static readonly AppsIndicativeEarningsModelComparer AppsIndicativeEarningsModelComparer = new AppsIndicativeEarningsModelComparer();

        private readonly ILogger _logger;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IAppsIndicativeEarningsModelBuilder _modelBuilder;

        public AppsIndicativeEarningsReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IFM36ProviderService fm36ProviderService,
            ILarsProviderService larsProviderService,
            IAppsIndicativeEarningsModelBuilder modelBuilder,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;
            _ilrProviderService = ilrProviderService;
            _fm36ProviderService = fm36ProviderService;
            _validLearnersService = validLearnersService;
            _larsProviderService = larsProviderService;
            _modelBuilder = modelBuilder;

            ReportFileName = "Apps Indicative Earnings Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateAppsIndicativeEarningsReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            long jobId = reportServiceContext.JobId;
            string ukPrn = reportServiceContext.Ukprn.ToString();
            string externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            string fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            string csv = await GetCsv(reportServiceContext, cancellationToken);
            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);
            Task<FM36Global> fm36Task = _fm36ProviderService.GetFM36Data(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, validLearnersTask, fm36Task);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            List<AppsIndicativeEarningsModel> appsIndicativeEarningsModels = new List<AppsIndicativeEarningsModel>();

            if (ilrFileTask.Result?.Learners != null)
            {
                await GenerateRowsAsync(ilrFileTask.Result, fm36Task.Result, validLearnersTask.Result, appsIndicativeEarningsModels, cancellationToken);
            }

            using (var ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<AppsIndicativeEarningsMapper, AppsIndicativeEarningsModel>(csvWriter, appsIndicativeEarningsModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }

        private async Task GenerateRowsAsync(IMessage ilrFile, FM36Global fm36Data, List<string> validLearners, List<AppsIndicativeEarningsModel> appsIndicativeEarningsModels, CancellationToken cancellationToken)
        {
            ILearner[] learners = ilrFile.Learners.Where(x => validLearners.Contains(x.LearnRefNumber)).ToArray();
            string[] learnAimRefs = learners.SelectMany(x => x.LearningDeliveries).Select(x => x.LearnAimRef).Distinct().ToArray();
            Dictionary<string, LarsLearningDelivery> larsLearningDeliveries = await _larsProviderService.GetLearningDeliveriesAsync(learnAimRefs, cancellationToken);

            foreach (ILearner learner in learners)
            {
                FM36Learner fm36Learner = fm36Data?.Learners?.SingleOrDefault(x => string.Equals(x.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase));

                foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                {
                    if (learningDelivery.FundModel != 36)
                    {
                        continue;
                    }

                    string larsStandard = null;
                    if (learningDelivery.StdCodeNullable != null)
                    {
                        larsStandard = await _larsProviderService.GetStandardAsync(
                            learningDelivery.StdCodeNullable.Value,
                            cancellationToken);
                    }

                    LarsLearningDelivery larsDelivery = larsLearningDeliveries.SingleOrDefault(x => string.Equals(x.Key, learningDelivery.LearnAimRef, StringComparison.OrdinalIgnoreCase)).Value;

                    LearningDelivery fm36LearningDelivery = fm36Learner?.LearningDeliveries
                        ?.SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber);

                    if (fm36Learner?.PriceEpisodes.Any() ?? false)
                    {
                        List<PriceEpisode> episodesInRange = fm36Learner.PriceEpisodes
                            .Where(p => p.PriceEpisodeValues.EpisodeStartDate >= Constants.BeginningOfYear &&
                                        p.PriceEpisodeValues.EpisodeStartDate <= Constants.EndOfYear
                                        && learningDelivery.AimSeqNumber == p.PriceEpisodeValues.PriceEpisodeAimSeqNumber).ToList();

                        if (episodesInRange.Any())
                        {
                            DateTime earliestEpisodeDate = episodesInRange.Select(x => x.PriceEpisodeValues.EpisodeStartDate ?? DateTime.MaxValue).Min();

                            bool earliestEpisode = false;
                            foreach (PriceEpisode episodeAttribute in episodesInRange)
                            {
                                if (episodeAttribute.PriceEpisodeValues.EpisodeStartDate == earliestEpisodeDate)
                                {
                                    earliestEpisode = true;
                                }

                                appsIndicativeEarningsModels.Add(
                                    _modelBuilder.BuildModel(
                                        learner,
                                        learningDelivery,
                                        fm36LearningDelivery,
                                        episodeAttribute,
                                        larsDelivery,
                                        larsStandard,
                                        earliestEpisode,
                                        true));

                                earliestEpisode = false;
                            }

                            continue;
                        }
                    }

                    appsIndicativeEarningsModels.Add(
                        _modelBuilder.BuildModel(
                            learner,
                            learningDelivery,
                            fm36LearningDelivery,
                            null,
                            larsDelivery,
                            larsStandard,
                            false,
                            false));
                }
            }

            appsIndicativeEarningsModels.Sort(AppsIndicativeEarningsModelComparer);
        }
    }
}
