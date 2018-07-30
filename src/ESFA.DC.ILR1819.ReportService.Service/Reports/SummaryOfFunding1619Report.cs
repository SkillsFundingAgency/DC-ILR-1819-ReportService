﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using CsvHelper;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Reports;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Model.Report;
using ESFA.DC.ILR1819.ReportService.Service.Mapper;
using ESFA.DC.ILR1819.ReportService.Service.Model;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.ILR1819.ReportService.Service.Reports
{
    public sealed class SummaryOfFunding1619Report : IReport
    {
        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IKeyValuePersistenceService _redis;
        private readonly IXmlSerializationService _xmlSerializationService;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;

        public SummaryOfFunding1619Report(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService blob,
            [KeyFilter(PersistenceStorageKeys.Redis)] IKeyValuePersistenceService redis,
            IXmlSerializationService xmlSerializationService,
            IJsonSerializationService jsonSerializationService,
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IStringUtilitiesService stringUtilitiesService)
        {
            _logger = logger;
            _storage = blob;
            _redis = redis;
            _xmlSerializationService = xmlSerializationService;
            _jsonSerializationService = jsonSerializationService;
            _ilrProviderService = ilrProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;
        }

        public ReportType ReportType { get; } = ReportType.SummaryOfFunding1619;

        public async Task GenerateReport(IJobContextMessage jobContextMessage)
        {
            await _storage.SaveAsync("16-19 Summary of Funding by Student Report.csv", await GetCsv(jobContextMessage));
        }

        private async Task<string> GetCsv(IJobContextMessage jobContextMessage)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(jobContextMessage);
            Task<List<string>> validLearnersTask = _validLearnersService.GetValidLearnersAsync(jobContextMessage);

            await Task.WhenAll(ilrFileTask, validLearnersTask);

            List<string> ilrError = new List<string>();

            List<SummaryOfFunding1619Model> summaryOfFunding1619Models = new List<SummaryOfFunding1619Model>(validLearnersTask.Result.Count);
            foreach (string validLearnerRefNum in validLearnersTask.Result)
            {
                var learner =
                    ilrFileTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);
                if (learner == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                summaryOfFunding1619Models.Add(new SummaryOfFunding1619Model()
                {
                    FundLine = "Todo", // Todo
                    LearnRefNumber = learner.LearnRefNumber,
                    FamilyName = learner.FamilyName,
                    GivenNames = learner.GivenNames,
                    DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                    CampId = learner.CampId,
                    PlanLearnHours = learner.PlanLearnHoursNullable,
                    PlanEepHours = learner.PlanEEPHoursNullable,
                    TotalPlannedHours = (learner.PlanLearnHoursNullable ?? 0) + (learner.PlanEEPHoursNullable ?? 0),
                    RateBand = "Todo", // Todo
                    StartFund = "Todo", // Todo
                    OnProgPayment = "Todo" // Todo
                });
            }

            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating S{nameof(MathsAndEnglishReport)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
            }

            StringBuilder sb = new StringBuilder();

            using (TextWriter textWriter = new StringWriter(sb))
            {
                using (CsvWriter csvWriter = new CsvWriter(textWriter))
                {
                    csvWriter.Configuration.RegisterClassMap<SummaryOfFunding1619Mapper>();
                    csvWriter.WriteHeader<SummaryOfFunding1619Model>();
                    csvWriter.NextRecord();
                    csvWriter.WriteRecords(summaryOfFunding1619Models);
                }
            }

            return sb.ToString();
        }
    }
}