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
using ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output;
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
    public sealed class TrailblazerEmployerIncentivesReport : AbstractReport, IReport
    {
        private readonly ILogger _logger;
        private readonly IFM81TrailBlazerProviderService _fm81TrailBlazerProviderService;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IValidLearnersService _validLearnersService;

        private readonly ITrailblazerEmployerIncentivesModelBuilder _trailblazerEmployerIncentivesModelBuilder;

        public TrailblazerEmployerIncentivesReport(
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IFM81TrailBlazerProviderService fm81TrailBlazerProviderService,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            ITrailblazerEmployerIncentivesModelBuilder trailblazerEmployerIncentivesModelBuilder,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IValueProvider valueProvider,
            IDateTimeProvider dateTimeProvider)
            : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _logger = logger;

            _fm81TrailBlazerProviderService = fm81TrailBlazerProviderService;
            _validLearnersService = validLearnersService;
            _ilrProviderService = ilrProviderService;

            _trailblazerEmployerIncentivesModelBuilder = trailblazerEmployerIncentivesModelBuilder;

            ReportFileName = "Trailblazer Apprenticeships Employer Incentives Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateTrailblazerEmployerIncentivesReport;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<FM81Global> fm81Task =
                _fm81TrailBlazerProviderService.GetFM81Data(reportServiceContext, cancellationToken);
            Task<List<string>> validLearnersTask =
                _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, fm81Task, validLearnersTask);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<ILearner> learners =
                ilrFileTask.Result?.Learners?.Where(x => validLearnersTask.Result.Contains(x.LearnRefNumber)).ToList();

            if (learners == null)
            {
                _logger.LogWarning("Failed to get learners for Trailblazer Employer Incentives Report");
                return;
            }

            var fm81Data = fm81Task.Result;
            var trailblazerEmployerIncentivesModels = new List<TrailblazerEmployerIncentivesModel>();

            var fm81EmployerIdentifierList = fm81Data?.Learners?.Select(l => l.LearningDeliveries?.Select(x =>
                string.Join(",", x.LearningDeliveryValues.EmpIdSmallBusDate, x.LearningDeliveryValues.EmpIdFirstYoungAppDate, x.LearningDeliveryValues.EmpIdSecondYoungAppDate, x.LearningDeliveryValues.EmpIdAchDate))).FirstOrDefault();

            if (fm81EmployerIdentifierList != null)
            {
                var fm81EmployerIdentifierUniqueList = fm81EmployerIdentifierList.First().Split(',').ToArray()
                    .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();

                foreach (string empIdentifier in fm81EmployerIdentifierUniqueList)
                {
                    var learnerFm81Data = fm81Data?.Learners?.SelectMany(x => x.LearningDeliveries).ToList();

                    trailblazerEmployerIncentivesModels.Add(
                        _trailblazerEmployerIncentivesModelBuilder.BuildTrailblazerEmployerIncentivesModel(
                            Convert.ToInt32(empIdentifier),
                            null,
                            learnerFm81Data));
                }
            }

            trailblazerEmployerIncentivesModels.Sort(new TrailblazerEmployerIncentivesModelComparer());

            var csv = GetReportCsv(trailblazerEmployerIncentivesModels);

            var jobId = reportServiceContext.JobId;
            var ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);
            await WriteZipEntry(archive, $"{fileName}.csv", csv);
        }

        private bool CheckIsApplicableLearner(ILearningDelivery learningDelivery)
        {
            return learningDelivery.FundModel == 81;
        }

        private string GetReportCsv(List<TrailblazerEmployerIncentivesModel> listModels)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<TrailblazerEmployerIncentiveMapper, TrailblazerEmployerIncentivesModel>(csvWriter, listModels);
                        csvWriter.Flush();
                        textWriter.Flush();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
        }
    }
}