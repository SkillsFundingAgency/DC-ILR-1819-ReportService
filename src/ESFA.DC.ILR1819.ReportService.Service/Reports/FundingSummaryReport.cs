﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using CsvHelper;
using ESFA.DC.DateTime.Provider.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface.Attribute;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
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
    public sealed class FundingSummaryReport : AbstractReportBuilder, IReport
    {
        private const string AlbSupportPayment = "ALBSupportPayment";

        private const string AlbAreaUpliftBalPayment = "AreaUpliftBalPayment";

        private const string AlbAreaUpliftOnProgPayment = "AreaUpliftOnProgPayment";

        private readonly ILogger _logger;
        private readonly IKeyValuePersistenceService _storage;
        private readonly IJsonSerializationService _jsonSerializationService;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IOrgProviderService _orgProviderService;
        private readonly IAllbProviderService _allbProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly IPeriodProviderService _periodProviderService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IVersionInfo _versionInfo;

        public FundingSummaryReport(
            ILogger logger,
            [KeyFilter(PersistenceStorageKeys.Blob)] IKeyValuePersistenceService storage,
            IJsonSerializationService jsonSerializationService,
            IIlrProviderService ilrProviderService,
            IOrgProviderService orgProviderService,
            IAllbProviderService allbProviderService,
            IValidLearnersService validLearnersService,
            IStringUtilitiesService stringUtilitiesService,
            IPeriodProviderService periodProviderService,
            IDateTimeProvider dateTimeProvider,
            ILarsProviderService larsProviderService,
            IVersionInfo versionInfo)
        {
            _logger = logger;
            _storage = storage;
            _jsonSerializationService = jsonSerializationService;
            _ilrProviderService = ilrProviderService;
            _orgProviderService = orgProviderService;
            _allbProviderService = allbProviderService;
            _validLearnersService = validLearnersService;
            _stringUtilitiesService = stringUtilitiesService;
            _periodProviderService = periodProviderService;
            _dateTimeProvider = dateTimeProvider;
            _larsProviderService = larsProviderService;
            _versionInfo = versionInfo;
        }

        public ReportType ReportType { get; } = ReportType.FundingSummary;

        public async Task GenerateReport(IJobContextMessage jobContextMessage)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(jobContextMessage);
            Task<IFundingOutputs> albDataTask = _allbProviderService.GetAllbData(jobContextMessage);
            Task<List<string>> validLearnersTask = _validLearnersService.GetValidLearnersAsync(jobContextMessage);
            Task<string> providerNameTask = _orgProviderService.GetProviderName(jobContextMessage);
            Task<int> periodTask = _periodProviderService.GetPeriod(jobContextMessage);

            await Task.WhenAll(ilrFileTask, albDataTask, validLearnersTask, providerNameTask, periodTask);

            List<string> ilrError = new List<string>();
            List<string> albLearnerError = new List<string>();

            List<FundingSummaryModel> fundingSummaryModels = new List<FundingSummaryModel>();
            foreach (string validLearnerRefNum in validLearnersTask.Result)
            {
                var learner = ilrFileTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);
                if (learner == null)
                {
                    ilrError.Add(validLearnerRefNum);
                    continue;
                }

                ILearnerAttribute albLearner =
                    albDataTask.Result?.Learners?.SingleOrDefault(x => x.LearnRefNumber == validLearnerRefNum);
                if (albLearner == null)
                {
                    albLearnerError.Add(validLearnerRefNum);
                    continue;
                }

                //foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries)
                //{
                //var albAttribs = albLearner?.LearningDeliveryAttributes
                //    .SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber)
                //    ?.LearningDeliveryAttributeDatas;
                var albSupportPaymentObj =
                    albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbSupportPayment);
                var albAreaUpliftOnProgPaymentObj =
                    albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbAreaUpliftOnProgPayment);
                var albAreaUpliftBalPaymentObj =
                    albLearner?.LearnerPeriodisedAttributes.SingleOrDefault(x => x.AttributeName == AlbAreaUpliftBalPayment);

                fundingSummaryModels.Add(new FundingSummaryModel()
                {
                    Title = "ILR Advanced Loans Bursary Funding (£)",
                    Period1 = albSupportPaymentObj?.Period1 ?? 0,
                    Period2 = albSupportPaymentObj?.Period2 ?? 0,
                    Period3 = albSupportPaymentObj?.Period3 ?? 0,
                    Period4 = albSupportPaymentObj?.Period4 ?? 0,
                    Period5 = albSupportPaymentObj?.Period5 ?? 0,
                    Period6 = albSupportPaymentObj?.Period6 ?? 0,
                    Period7 = albSupportPaymentObj?.Period7 ?? 0,
                    Period8 = albSupportPaymentObj?.Period8 ?? 0,
                    Period9 = albSupportPaymentObj?.Period9 ?? 0,
                    Period10 = albSupportPaymentObj?.Period10 ?? 0,
                    Period11 = albSupportPaymentObj?.Period11 ?? 0,
                    Period12 = albSupportPaymentObj?.Period12 ?? 0,
                    Period1_8 = (albSupportPaymentObj?.Period1 ?? 0) + (albSupportPaymentObj?.Period2 ?? 0) +
                                (albSupportPaymentObj?.Period3 ?? 0) + (albSupportPaymentObj?.Period4 ?? 0) +
                                (albSupportPaymentObj?.Period5 ?? 0) + (albSupportPaymentObj?.Period6 ?? 0) +
                                (albSupportPaymentObj?.Period7 ?? 0) + (albSupportPaymentObj?.Period8 ?? 0),
                    Period9_12 = (albSupportPaymentObj?.Period9 ?? 0) + (albSupportPaymentObj?.Period10 ?? 0) +
                                 (albSupportPaymentObj?.Period11 ?? 0) + (albSupportPaymentObj?.Period12 ?? 0),
                    YearToDate = GetYearToDateTotal(albSupportPaymentObj, periodTask.Result),
                    Total = (albSupportPaymentObj?.Period1 ?? 0) + (albSupportPaymentObj?.Period2 ?? 0) +
                            (albSupportPaymentObj?.Period3 ?? 0) + (albSupportPaymentObj?.Period4 ?? 0) +
                            (albSupportPaymentObj?.Period5 ?? 0) + (albSupportPaymentObj?.Period6 ?? 0) +
                            (albSupportPaymentObj?.Period7 ?? 0) + (albSupportPaymentObj?.Period8 ?? 0) +
                            (albSupportPaymentObj?.Period9 ?? 0) + (albSupportPaymentObj?.Period10 ?? 0) +
                            (albSupportPaymentObj?.Period11 ?? 0) + (albSupportPaymentObj?.Period12 ?? 0)
                });

                fundingSummaryModels.Add(new FundingSummaryModel()
                {
                    Title = "ILR Advanced Loans Bursary Area Costs (£)",
                    Period1 = (albAreaUpliftBalPaymentObj?.Period1 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0),
                    Period2 = (albAreaUpliftBalPaymentObj?.Period2 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0),
                    Period3 = (albAreaUpliftBalPaymentObj?.Period3 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0),
                    Period4 = (albAreaUpliftBalPaymentObj?.Period4 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0),
                    Period5 = (albAreaUpliftBalPaymentObj?.Period5 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0),
                    Period6 = (albAreaUpliftBalPaymentObj?.Period6 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0),
                    Period7 = (albAreaUpliftBalPaymentObj?.Period7 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0),
                    Period8 = (albAreaUpliftBalPaymentObj?.Period8 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0),
                    Period9 = (albAreaUpliftBalPaymentObj?.Period9 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0),
                    Period10 = (albAreaUpliftBalPaymentObj?.Period10 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0),
                    Period11 = (albAreaUpliftBalPaymentObj?.Period11 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0),
                    Period12 = (albAreaUpliftBalPaymentObj?.Period12 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0),
                    Period9_12 = (albAreaUpliftBalPaymentObj?.Period9 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0)
                            + (albAreaUpliftBalPaymentObj?.Period10 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0)
                            + (albAreaUpliftBalPaymentObj?.Period11 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0)
                            + (albAreaUpliftBalPaymentObj?.Period12 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0),
                    Period1_8 = (albAreaUpliftBalPaymentObj?.Period1 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period2 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period3 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period4 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period5 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period6 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period7 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period8 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0),
                    YearToDate = GetYearToDateTotal(albAreaUpliftBalPaymentObj, albAreaUpliftOnProgPaymentObj, periodTask.Result),
                    Total = (albAreaUpliftBalPaymentObj?.Period1 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period2 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period3 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period4 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period5 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period6 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period7 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period8 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0) +
                            (albAreaUpliftBalPaymentObj?.Period9 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0)
                            + (albAreaUpliftBalPaymentObj?.Period10 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0)
                            + (albAreaUpliftBalPaymentObj?.Period11 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0)
                            + (albAreaUpliftBalPaymentObj?.Period12 ?? 0) + (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0)
                });
                //}
            }

            FundingSummaryHeaderModel fundingSummaryHeaderModel = GetHeader(jobContextMessage, ilrFileTask, providerNameTask);
            FundingSummaryFooterModel fundingSummaryFooterModel = await GetFooterAsync(jobContextMessage);

            LogWarnings(ilrError, albLearnerError);

            await _storage.SaveAsync("Funding_Summary_Report.csv", GetReportCsv(fundingSummaryModels, fundingSummaryHeaderModel, fundingSummaryFooterModel));
            await _storage.SaveAsync("Funding_Summary_Report.json", _jsonSerializationService.Serialize(fundingSummaryModels));
        }

        private string GetReportCsv(List<FundingSummaryModel> fundingSummaryModels, FundingSummaryHeaderModel fundingSummaryHeaderModel, FundingSummaryFooterModel fundingSummaryFooterModel)
        {
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    BuildReport<FundingSummaryHeaderMapper, FundingSummaryHeaderModel>(ms, fundingSummaryHeaderModel);
            //    BuildReport<FundingSummaryMapper, FundingSummaryModel>(ms, fundingSummaryModels);
            //    BuildReport<FundingSummaryFooterMapper, FundingSummaryFooterModel>(ms, fundingSummaryFooterModel);
            //    return ms.ToString();
            //}

            StringBuilder sb = new StringBuilder();

            using (TextWriter textWriter = new StringWriter(sb))
            {
                using (CsvWriter csvWriter = new CsvWriter(textWriter))
                {
                    csvWriter.Configuration.RegisterClassMap<FundingSummaryHeaderMapper>();
                    csvWriter.WriteHeader<FundingSummaryHeaderModel>();
                    csvWriter.NextRecord();
                    csvWriter.WriteRecord(fundingSummaryHeaderModel);

                    csvWriter.Configuration.RegisterClassMap<FundingSummaryFooterMapper>();
                    csvWriter.WriteHeader<FundingSummaryModel>();
                    csvWriter.NextRecord();
                    csvWriter.WriteRecords(fundingSummaryModels);

                    csvWriter.Configuration.RegisterClassMap<FundingSummaryMapper>();
                    csvWriter.WriteHeader<FundingSummaryFooterModel>();
                    csvWriter.NextRecord();
                    csvWriter.WriteRecord(fundingSummaryFooterModel);
                }
            }

            return sb.ToString();
        }

        private void LogWarnings(List<string> ilrError, List<string> albLearnerError)
        {
            if (ilrError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ILR learners while generating Allb Occupancy Report: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
            }

            if (albLearnerError.Any())
            {
                _logger.LogWarning($"Failed to get one or more ALB learners while generating Allb Occupancy Report: {_stringUtilitiesService.JoinWithMaxLength(albLearnerError)}");
            }
        }

        private FundingSummaryHeaderModel GetHeader(IJobContextMessage jobContextMessage, Task<IMessage> messageTask, Task<string> providerNameTask)
        {
            string ilrFilename = jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename].ToString();
            FundingSummaryHeaderModel fundingSummaryHeaderModel = new FundingSummaryHeaderModel()
            {
                IlrFile = ilrFilename,
                Ukprn = messageTask.Result.HeaderEntity.SourceEntity.UKPRN,
                ProviderName = providerNameTask.Result,
                LastEasUpdate = "Todo", // Todo
                LastIlrFileUpdate = _stringUtilitiesService.GetIlrFileDate(ilrFilename).ToString("yyyy-MM-dd HH:mm:ssy"),
                SecurityClassification = "OFFICIAL-SENSITIVE"
            };
            return fundingSummaryHeaderModel;
        }

        private async Task<FundingSummaryFooterModel> GetFooterAsync(IJobContextMessage jobContextMessage)
        {
            var dateTimeNowUtc = _dateTimeProvider.GetNowUtc();
            var dateTimeNowUk = _dateTimeProvider.ConvertUtcToUk(dateTimeNowUtc);

            string ilrFilename = jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename].ToString();
            FundingSummaryFooterModel fundingSummaryFooterModel = new FundingSummaryFooterModel()
            {
                ReportGeneratedAt = "Report generated at " + dateTimeNowUk.ToString("HH:mm:ss") + " on " + dateTimeNowUk.ToString("dd/MM/yyyy"),
                ApplicationVersion = _versionInfo.ServiceReleaseVersion,
                ComponentSetVersion = "NA",
                FilePreparationDate = _stringUtilitiesService.GetIlrFileDate(ilrFilename).ToString("dd/MM/yyyy"),
                OrganisationData = await _orgProviderService.GetVersionAsync(),
                LargeEmployerData = "Todo", // Todo
                LarsData = await _larsProviderService.GetVersionAsync(),
                PostcodeData = "Todo" // Todo
            };

            return fundingSummaryFooterModel;
        }

        private decimal? GetYearToDateTotal(ILearnerPeriodisedAttribute albSupportPaymentObj, int period)
        {
            decimal total = 0;
            for (int i = 0; i < period; i++)
            {
                switch (i)
                {
                    case 0:
                        total += albSupportPaymentObj?.Period1 ?? 0;
                        break;
                    case 1:
                        total += albSupportPaymentObj?.Period2 ?? 0;
                        break;
                    case 2:
                        total += albSupportPaymentObj?.Period3 ?? 0;
                        break;
                    case 3:
                        total += albSupportPaymentObj?.Period4 ?? 0;
                        break;
                    case 4:
                        total += albSupportPaymentObj?.Period5 ?? 0;
                        break;
                    case 5:
                        total += albSupportPaymentObj?.Period6 ?? 0;
                        break;
                    case 6:
                        total += albSupportPaymentObj?.Period7 ?? 0;
                        break;
                    case 7:
                        total += albSupportPaymentObj?.Period8 ?? 0;
                        break;
                    case 8:
                        total += albSupportPaymentObj?.Period9 ?? 0;
                        break;
                    case 9:
                        total += albSupportPaymentObj?.Period10 ?? 0;
                        break;
                    case 10:
                        total += albSupportPaymentObj?.Period11 ?? 0;
                        break;
                    case 11:
                        total += albSupportPaymentObj?.Period12 ?? 0;
                        break;
                }
            }

            return total;
        }

        private decimal? GetYearToDateTotal(ILearnerPeriodisedAttribute albAreaUpliftBalPaymentObj, ILearnerPeriodisedAttribute albAreaUpliftOnProgPaymentObj, int period)
        {
            decimal total = 0;
            for (int i = 0; i < period; i++)
            {
                switch (i)
                {
                    case 0:
                        total += (albAreaUpliftBalPaymentObj?.Period1 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0);
                        break;
                    case 1:
                        total += (albAreaUpliftBalPaymentObj?.Period2 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0);
                        break;
                    case 2:
                        total += (albAreaUpliftBalPaymentObj?.Period3 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0);
                        break;
                    case 3:
                        total += (albAreaUpliftBalPaymentObj?.Period4 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0);
                        break;
                    case 4:
                        total += (albAreaUpliftBalPaymentObj?.Period5 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0);
                        break;
                    case 5:
                        total += (albAreaUpliftBalPaymentObj?.Period6 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0);
                        break;
                    case 6:
                        total += (albAreaUpliftBalPaymentObj?.Period7 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0);
                        break;
                    case 7:
                        total += (albAreaUpliftBalPaymentObj?.Period8 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0);
                        break;
                    case 8:
                        total += (albAreaUpliftBalPaymentObj?.Period9 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0);
                        break;
                    case 9:
                        total += (albAreaUpliftBalPaymentObj?.Period10 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0);
                        break;
                    case 10:
                        total += (albAreaUpliftBalPaymentObj?.Period11 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0);
                        break;
                    case 11:
                        total += (albAreaUpliftBalPaymentObj?.Period12 ?? 0) +
                                 (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0);
                        break;
                }
            }

            return total;
        }
    }
}