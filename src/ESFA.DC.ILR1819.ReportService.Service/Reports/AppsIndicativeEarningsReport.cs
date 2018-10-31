﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using CsvHelper;
using ESFA.DC.Data.LARS.Model;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Reports;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Lars;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;
using ESFA.DC.ILR1819.ReportService.Service.Comparer;
using ESFA.DC.ILR1819.ReportService.Service.Mapper;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.JobContextManager.Model.Interface;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Reports
{
    public sealed class AppsIndicativeEarningsReport : AbstractReportBuilder, IReport
    {
        private static readonly AppsIndicativeEarningsModelComparer AppsIndicativeEarningsModelComparer = new AppsIndicativeEarningsModelComparer();

        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IAppsIndicativeEarningsModelBuilder _modelBuilder;

        public AppsIndicativeEarningsReport(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService storage,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IFM36ProviderService fm36ProviderService,
            ILarsProviderService larsProviderService,
            IStringUtilitiesService stringUtilitiesService,
            IAppsIndicativeEarningsModelBuilder modelBuilder,
            IDateTimeProvider dateTimeProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
        : base(dateTimeProvider)
        {
            _logger = logger;
            _storage = storage;
            _ilrProviderService = ilrProviderService;
            _fm36ProviderService = fm36ProviderService;
            _validLearnersService = validLearnersService;
            _larsProviderService = larsProviderService;
            _stringUtilitiesService = stringUtilitiesService;
            _modelBuilder = modelBuilder;

            ReportFileName = "Apps Indicative Earnings Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateAppsIndicativeEarningsReport;
        }

        public async Task GenerateReport(IJobContextMessage jobContextMessage, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            var jobId = jobContextMessage.JobId;
            var ukPrn = jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn].ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, jobContextMessage.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, jobContextMessage.SubmissionDateTimeUtc);

            string csv = await GetCsv(jobContextMessage, cancellationToken);
            await _storage.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private async Task<string> GetCsv(IJobContextMessage jobContextMessage, CancellationToken cancellationToken)
        {
            var firstOfAugust = new DateTime(2018, 8, 1);
            var endOfYear = new DateTime(2019, 7, 31, 23, 59, 59);

            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(jobContextMessage, cancellationToken);
            Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(jobContextMessage, cancellationToken);
            Task<FM36Global> fm36Task = _fm36ProviderService.GetFM36Data(jobContextMessage, cancellationToken);

            await Task.WhenAll(ilrFileTask, validLearnersTask, fm36Task);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            var validLearners = validLearnersTask.Result;
            Dictionary<string, LarsLearningDelivery> larsLearningDeliveries = await _larsProviderService.GetLearningDeliveries(validLearners.ToArray(), cancellationToken);

            var ilrError = new List<string>();

            var appsIndicativeEarningsModels = new List<AppsIndicativeEarningsModel>();
            foreach (string validLearnerRefNum in validLearners)
            {
                var learner = ilrFileTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);
                var larsDelivery = larsLearningDeliveries.SingleOrDefault(x => x.Key == validLearnerRefNum).Value;
                var fm36Learner = fm36Task.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);

                if (learner == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                foreach (var learningDelivery in learner.LearningDeliveries)
                {
                    LARS_Standard larsStandard = learningDelivery.StdCodeNullable == null
                        ? null
                        : await _larsProviderService.GetStandard(
                            learningDelivery.StdCodeNullable ?? 0,
                            cancellationToken);

                    var fm36LearningDelivery = fm36Learner?.LearningDeliveries
                        ?.SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber);

                    if (fm36Learner?.PriceEpisodes.Any() ?? false)
                    {
                        var episodesInRange = fm36Learner.PriceEpisodes
                            .Where(p => p.PriceEpisodeValues.EpisodeStartDate >= firstOfAugust &&
                                        p.PriceEpisodeValues.EpisodeStartDate <= endOfYear).ToList();

                        if (episodesInRange.Any())
                        {
                            var earliestEpisodeDate =
                                episodesInRange.Min(x =>
                                    x.PriceEpisodeValues.EpisodeStartDate);

                            var earliestEpisode = false;
                            foreach (var episodeAttribute in episodesInRange)
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

            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating {nameof(AppsIndicativeEarningsReport)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
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
    }
}
