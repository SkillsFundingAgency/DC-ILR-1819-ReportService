using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ILR;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public sealed class FundingClaim1619Report : AbstractReport, IReport
    {
        private const string ProviderCellName = "C3";
        private const string UKPrnCellName = "C4";
        private const string ILRFileNameCellName = "C5";
        private const string YearCellName = "H3";

        private const string RetentionFactorCellName = "D7";
        private const string ProgrammeCostWeightingCellName = "D8";
        private const string AreaCostAllowanceCellName = "D9";
        private const string DisAdvProportionCellName = "D10";
        private const string LargeProgrammeProportionCellName = "D11";

        private const string CofRemovalCellName = "F52";

        private const string ComponentSetVersionCellName = "C149";
        private const string ApplicationVersionCellName = "C151";
        private const string FilePreparationCellName = "C153";

        private const string LarsDataCellName = "G149";
        private const string OrganisationDataCellName = "G151";
        private const string PostcodeDataCellName = "G153";
        private const string LargeEmployerDataCellName = "G155";
        private const string CofRemovalDataCellName = "G157";

        private List<Tuple<string, string, string, string>> CellPositions = new List<Tuple<string, string, string, string>>()
        {
            new Tuple<string, string, string, string>("14-16 Direct Funded Students", "540+ hours (Band 5)", "E14", string.Empty),
            new Tuple<string, string, string, string>("14-16 Direct Funded Students", "450+ hours (Band 4a)", "E15", string.Empty),
            new Tuple<string, string, string, string>("14-16 Direct Funded Students", "450 to 539 hours (Band 4b)", "E16", string.Empty),
            new Tuple<string, string, string, string>("14-16 Direct Funded Students", "360 to 449 hours (Band 3)", "E17", string.Empty),
            new Tuple<string, string, string, string>("14-16 Direct Funded Students", "280 to 359 hours (Band 2)", "E18", string.Empty),
            new Tuple<string, string, string, string>("14-16 Direct Funded Students", "Up to 279 hours (Band 1)", "E19", string.Empty),

            new Tuple<string, string, string, string>("16-19 Students (including High Needs Students)", "540+ hours (Band 5)", "E23", "F23"),
            new Tuple<string, string, string, string>("16-19 Students (including High Needs Students)", "450+ hours (Band 4a)", "E24", "F24"),
            new Tuple<string, string, string, string>("16-19 Students (including High Needs Students)", "450 to 539 hours (Band 4b)", "E25", "F25"),
            new Tuple<string, string, string, string>("16-19 Students (including High Needs Students)", "360 to 449 hours (Band 3)", "E26", "F26"),
            new Tuple<string, string, string, string>("16-19 Students (including High Needs Students)", "280 to 359 hours (Band 2)", "E27", "F27"),
            new Tuple<string, string, string, string>("16-19 Students (including High Needs Students)", "Up to 279 hours (Band 1)", "E28", "F28"),

            new Tuple<string, string, string, string>("19-24 Students with an EHC plan", "540+ hours (Band 5)", "E33", "F33"),
            new Tuple<string, string, string, string>("19-24 Students with an EHC plan", "450+ hours (Band 4a)", "E34", "F34"),
            new Tuple<string, string, string, string>("19-24 Students with an EHC plan", "450 to 539 hours (Band 4b)", "E35", "F35"),
            new Tuple<string, string, string, string>("19-24 Students with an EHC plan", "360 to 449 hours (Band 3)", "E36", "F36"),
            new Tuple<string, string, string, string>("19-24 Students with an EHC plan", "280 to 359 hours (Band 2)", "E37", "F37"),
            new Tuple<string, string, string, string>("19-24 Students with an EHC plan", "Up to 279 hours (Band 1)", "E38", "F38"),

            new Tuple<string, string, string, string>("19+ Continuing Students (excluding EHC plans)", "540+ hours (Band 5)", "E42", "F42"),
            new Tuple<string, string, string, string>("19+ Continuing Students (excluding EHC plans)", "450+ hours (Band 4a)", "E43", "F43"),
            new Tuple<string, string, string, string>("19+ Continuing Students (excluding EHC plans)", "450 to 539 hours (Band 4b)", "E44", "F44"),
            new Tuple<string, string, string, string>("19+ Continuing Students (excluding EHC plans)", "360 to 449 hours (Band 3)", "E45", "F45"),
            new Tuple<string, string, string, string>("19+ Continuing Students (excluding EHC plans)", "280 to 359 hours (Band 2)", "E46", "F46"),
            new Tuple<string, string, string, string>("19+ Continuing Students (excluding EHC plans)", "Up to 279 hours (Band 1)", "E47", "F47")
        };

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIlrProviderService _ilrProviderService;
        private readonly IOrgProviderService _orgProviderService;
        private readonly IFM25ProviderService _fm25ProviderService;
        private readonly IPostcodeProviderService _postcodeProviderService;
        private readonly ILargeEmployerProviderService _largeEmployerProviderService;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IVersionInfo _versionInfo;

        private readonly Dictionary<string, string[]> _applicableFundingLineTypes = new Dictionary<string, string[]>
        {
            { "14-16 Direct Funded Students", new[] { "14-16 Direct Funded Students" } },
            { "16-19 Students (including High Needs Students)", new[] { "16-19 Students (excluding High Needs Students)", "16-19 High Needs Students" } },
            { "19-24 Students with an EHC plan", new[] { "19-24 Students with an EHCP" } },
            { "19+ Continuing Students (excluding EHC plans)", new[] { "19+ Continuing Students (excluding EHCP)" } }
        };

        public FundingClaim1619Report(
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            IIlrProviderService ilrProviderService,
            IOrgProviderService orgProviderService,
            IFM25ProviderService fm25ProviderService,
            IPostcodeProviderService postcodeProviderService,
            ILargeEmployerProviderService largeEmployerProviderService,
            ILarsProviderService larsProviderService,
            IVersionInfo versionInfo,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions)
            : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _dateTimeProvider = dateTimeProvider;
            _ilrProviderService = ilrProviderService;
            _orgProviderService = orgProviderService;
            _fm25ProviderService = fm25ProviderService;
            _postcodeProviderService = postcodeProviderService;
            _largeEmployerProviderService = largeEmployerProviderService;
            _larsProviderService = larsProviderService;
            _versionInfo = versionInfo;

            ReportFileName = "16-19 Funding Claim Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateFundingClaim1619Report;
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<FM25Global> fm25DataTask = _fm25ProviderService.GetFM25Data(reportServiceContext, cancellationToken);
            Task<string> providerNameTask = _orgProviderService.GetProviderName(reportServiceContext, cancellationToken);
            Task<decimal?> cofRemovalTask = _orgProviderService.GetCofRemoval(reportServiceContext, cancellationToken);
            Task<ILRSourceFileInfo> lastSubmittedIlrFileTask = _ilrProviderService.GetLastSubmittedIlrFile(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFileTask, fm25DataTask, providerNameTask, cofRemovalTask, lastSubmittedIlrFileTask);

            FundingClaim1619HeaderModel fundingSummaryHeaderModel = await GetHeaderAsync(reportServiceContext, ilrFileTask, lastSubmittedIlrFileTask, providerNameTask, cancellationToken, isFis);
            FundingClaim1619FooterModel fundingSummaryFooterModel = await GetFooterAsync(ilrFileTask, lastSubmittedIlrFileTask, cancellationToken);

            string[] applicableFundingLineTypes = _applicableFundingLineTypes.Values.SelectMany(x => x).ToArray();

            // Funding factors are at provider level but are output by the funding calculation against every learner record.
            // All learners in one provider will have the same funding factor value so the report can pull the minimum value
            // of each factor from the funding calc output at provider level to populate the factors table.
            FundingClaim1619FundingFactorModel fundingClaim1619FundingFactorModel = null;

            Dictionary<string, List<FundingClaim1619Model>> fundLineAndRateBandData = new Dictionary<string, List<FundingClaim1619Model>>();

            foreach (ILearner learner in ilrFileTask.Result.Learners)
            {
                FM25Learner learnerFm25Data =
                    fm25DataTask.Result?.Learners?.SingleOrDefault(l => string.Equals(l.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase));

                if (learnerFm25Data == null)
                {
                    continue;
                }

                if (!ApplicableLearner(learnerFm25Data, applicableFundingLineTypes))
                {
                    continue;
                }

                if (fundingClaim1619FundingFactorModel == null)
                {
                    fundingClaim1619FundingFactorModel = new FundingClaim1619FundingFactorModel
                    {
                        AreaCostFact1618Hist = learnerFm25Data.AreaCostFact1618Hist.GetValueOrDefault(0).ToString("N5"),
                        ProgWeightHist = learnerFm25Data.ProgWeightHist.GetValueOrDefault(0).ToString("N5"),
                        PrvDisadvPropnHist = learnerFm25Data.PrvDisadvPropnHist.GetValueOrDefault(0).ToString("N5"),
                        PrvHistLrgProgPropn = learnerFm25Data.PrvHistLrgProgPropn.GetValueOrDefault(0).ToString("N5"),
                        PrvRetentFactHist = learnerFm25Data.PrvRetentFactHist.GetValueOrDefault(0).ToString("N5")
                    };
                }

                if (learner.LearningDeliveries.Any(ApplicableLearningDelivery))
                {
                    GetFundLineAndBandRateData(learnerFm25Data, ref fundLineAndRateBandData);
                }
            }

            // probably don't need this as this will be calculated automatically through the template
            //FundingClaim1619Model subTotal = TotalFundLineAndBandRateData(ref fundLineAndRateBandData);

            // probably don't need this as this will be calculated automatically through the template
            //FundingClaim1619Model total = new FundingClaim1619Model
            //{
            //    StudentNumber = subTotal.StudentNumber,
            //    TotalFunding = 0 // Todo
            //};

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            long jobId = reportServiceContext.JobId;
            string ukPrn = reportServiceContext.Ukprn.ToString();
            var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
            var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("1619FundingClaimReportTemplate.xlsx"));
            var manifestResourceStream = assembly.GetManifestResourceStream(resourceName);
            Workbook workbook = new Workbook(manifestResourceStream);
            Worksheet worksheet = workbook.Worksheets[0];
            Cells cells = worksheet.Cells;
            InsertHeaderFooter(workbook, fundingSummaryHeaderModel, fundingSummaryFooterModel);
            PopulateAllocationValues(cells, fundingClaim1619FundingFactorModel);
            PopulateMainData(cells, fundLineAndRateBandData);
            PopulateCofRemoval(cells, cofRemovalTask.Result);
            workbook.CalculateFormula();
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Save(ms, SaveFormat.Xlsx);
                await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.xlsx", ms, cancellationToken);
                await WriteZipEntry(archive, $"{fileName}.xlsx", ms, cancellationToken);
            }
        }

        private void InsertHeaderFooter(Workbook workbook, FundingClaim1619HeaderModel headerModel, FundingClaim1619FooterModel footerModel)
        {
            PageSetup pageSetup = workbook.Worksheets[0].PageSetup;

            pageSetup.SetHeader(0, "&14&\"Bold\"16-19 Funding Claim Report");
            pageSetup.SetHeader(2, "&10&\"Bold\"OFFICIAL-SENSITIVE");

            var cells = workbook.Worksheets[0].Cells;

            cells[ProviderCellName].PutValue(headerModel.ProviderName);
            cells[UKPrnCellName].PutValue(headerModel.Ukprn);
            cells[ILRFileNameCellName].PutValue(headerModel.IlrFile);
            cells[YearCellName].PutValue(headerModel.Year);

            pageSetup.SetFooter(0, "&8" + footerModel.ReportGeneratedAt);
            pageSetup.SetFooter(2, "&8Page &P of &N");

            cells[ComponentSetVersionCellName].PutValue(footerModel.ComponentSetVersion);
            cells[ApplicationVersionCellName].PutValue(footerModel.ApplicationVersion);
            cells[FilePreparationCellName].PutValue(footerModel.FilePreparationDate);

            cells[LarsDataCellName].PutValue(footerModel.LarsData);
            cells[OrganisationDataCellName].PutValue(footerModel.OrganisationData);
            cells[PostcodeDataCellName].PutValue(footerModel.PostcodeData);
            cells[LargeEmployerDataCellName].PutValue(footerModel.LargeEmployerData);
            cells[CofRemovalDataCellName].PutValue(footerModel.CofRemovalData);
        }

        private void PopulateCofRemoval(Cells cells, decimal? cofRemoval)
        {
            // populating cof removal
            if (cofRemoval.HasValue)
            {
                cells[CofRemovalCellName].PutValue(-cofRemoval);
            }
        }

        private void PopulateAllocationValues(Cells cells, FundingClaim1619FundingFactorModel fundingClaimFactorModel)
        {
            if (fundingClaimFactorModel != null)
            {
                // populating allocation values
                cells[RetentionFactorCellName].PutValue(fundingClaimFactorModel.PrvRetentFactHist);
                cells[ProgrammeCostWeightingCellName].PutValue(fundingClaimFactorModel.ProgWeightHist);
                cells[AreaCostAllowanceCellName].PutValue(fundingClaimFactorModel.AreaCostFact1618Hist);
                cells[DisAdvProportionCellName].PutValue(fundingClaimFactorModel.PrvDisadvPropnHist);
                cells[LargeProgrammeProportionCellName].PutValue(fundingClaimFactorModel.PrvHistLrgProgPropn);
            }
        }

        private void PopulateMainData(Cells cells, Dictionary<string, List<FundingClaim1619Model>> fundingClaimModels)
        {
            foreach (var model in fundingClaimModels)
            {
                foreach (var singleClaim1619Model in model.Value)
                {
                    PopulateStudentNumberAndTotalFundingCells(cells, model.Key, singleClaim1619Model.Title, singleClaim1619Model.StudentNumber, singleClaim1619Model.TotalFunding);
                }
            }
        }

        private void PopulateStudentNumberAndTotalFundingCells(Cells cells, string fundline, string band, int studentNumber, decimal? totalFunding)
        {
            foreach (var cp in CellPositions)
            {
                if (string.Equals(fundline, cp.Item1, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(band, cp.Item2, StringComparison.OrdinalIgnoreCase))
                {
                    cells[cp.Item3].PutValue(studentNumber);
                    if (!string.IsNullOrWhiteSpace(cp.Item4))
                    {
                        if (totalFunding.HasValue)
                        {
                            cells[cp.Item4].PutValue(totalFunding);
                        }
                    }
                }
            }
        }

        private FundingClaim1619Model TotalFundLineAndBandRateData(ref Dictionary<string, List<FundingClaim1619Model>> fundLineAndRateBandData)
        {
            int studentNumberSubTotal = 0;
            decimal totalFundingSubTotal = 0;

            foreach (KeyValuePair<string, List<FundingClaim1619Model>> keyValuePair in fundLineAndRateBandData)
            {
                int studentNumber = 0;
                decimal totalFunding = 0;

                foreach (FundingClaim1619Model fundingClaim1619Model in keyValuePair.Value)
                {
                    studentNumber += fundingClaim1619Model.StudentNumber;
                    totalFunding += fundingClaim1619Model.TotalFunding;
                }

                keyValuePair.Value.Add(new FundingClaim1619Model
                {
                    Title = "Sub-Total",
                    StudentNumber = studentNumber,
                    TotalFunding = totalFunding
                });

                studentNumberSubTotal += studentNumber;
                totalFundingSubTotal += totalFunding;
            }

            return new FundingClaim1619Model
            {
                Title = "Sub-Total",
                StudentNumber = studentNumberSubTotal,
                TotalFunding = totalFundingSubTotal
            };
        }

        private void GetFundLineAndBandRateData(FM25Learner learnerFm25Data, ref Dictionary<string, List<FundingClaim1619Model>> fundLineAndRateBandData)
        {
            List<FundingClaim1619Model> models;
            string title = FundLineToTitle(learnerFm25Data.FundLine);
            if (!fundLineAndRateBandData.TryGetValue(title, out models))
            {
                models = new List<FundingClaim1619Model>();
                fundLineAndRateBandData[title] = models;
            }

            FundingClaim1619Model model = models.SingleOrDefault(x =>
                string.Equals(x.Title, learnerFm25Data.RateBand, StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                model = new FundingClaim1619Model
                {
                    Title = learnerFm25Data.RateBand
                };
                models.Add(model);
            }

            model.StudentNumber += 1;
            model.TotalFunding += learnerFm25Data.OnProgPayment.GetValueOrDefault(0);
        }

        private string FundLineToTitle(string fundLine)
        {
            foreach (KeyValuePair<string, string[]> applicableFundingLineType in _applicableFundingLineTypes)
            {
                if (applicableFundingLineType.Value.Any(x => string.Equals(x, fundLine, StringComparison.OrdinalIgnoreCase)))
                {
                    return applicableFundingLineType.Key;
                }
            }

            return string.Empty;
        }

        private bool ApplicableLearner(FM25Learner learnerFm25Data, string[] applicableFundingLineTypes)
        {
            return applicableFundingLineTypes.Any(x =>
                        string.Equals(x, learnerFm25Data.FundLine, StringComparison.OrdinalIgnoreCase))
                    && learnerFm25Data.StartFund.GetValueOrDefault(false);
        }

        private bool ApplicableLearningDelivery(ILearningDelivery learningDelivery)
        {
            return learningDelivery.FundModel == 25
                   && learningDelivery.LearningDeliveryFAMs.Any(x =>
                       string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeSOF, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(x.LearnDelFAMCode, Constants.LearningDeliveryFAMCode107, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<FundingClaim1619HeaderModel> GetHeaderAsync(IReportServiceContext reportServiceContext, Task<IMessage> ilrFileTask, Task<ILRSourceFileInfo> lastSubmittedIlrFileTask, Task<string> providerNameTask, CancellationToken cancellationToken, bool isFis)
        {
            var ilrFileName = reportServiceContext.OriginalFilename ?? reportServiceContext.Filename;

            FundingClaim1619HeaderModel fundingClaim1619HeaderModel = new FundingClaim1619HeaderModel
            {
                ProviderName = providerNameTask.Result ?? "Unknown",
                Ukprn = reportServiceContext.Ukprn,
                IlrFile = ilrFileName,
                Year = Constants.Year
            };

            return fundingClaim1619HeaderModel;
        }

        private async Task<FundingClaim1619FooterModel> GetFooterAsync(Task<IMessage> messageTask, Task<ILRSourceFileInfo> lastSubmittedIlrFileTask, CancellationToken cancellationToken)
        {
            DateTime dateTimeNowUtc = _dateTimeProvider.GetNowUtc();
            DateTime dateTimeNowUk = _dateTimeProvider.ConvertUtcToUk(dateTimeNowUtc);
            FundingClaim1619FooterModel fundingSummaryFooterModel = new FundingClaim1619FooterModel
            {
                ReportGeneratedAt = "Report generated at " + dateTimeNowUk.ToString("HH:mm:ss") + " on " + dateTimeNowUk.ToString("dd/MM/yyyy"),
                ApplicationVersion = _versionInfo.ServiceReleaseVersion,
                ComponentSetVersion = "NA",
                OrganisationData = await _orgProviderService.GetVersionAsync(cancellationToken),
                LargeEmployerData = await _largeEmployerProviderService.GetVersionAsync(cancellationToken),
                LarsData = await _larsProviderService.GetVersionAsync(cancellationToken),
                PostcodeData = await _postcodeProviderService.GetVersionAsync(cancellationToken),
                CofRemovalData = string.Empty // todo: populate this
            };

            if (messageTask.Result != null)
            {
                fundingSummaryFooterModel.FilePreparationDate = messageTask.Result.HeaderEntity.SourceEntity.DateTime.ToString("dd/MM/yyyy");
            }
            else
            {
                fundingSummaryFooterModel.FilePreparationDate = lastSubmittedIlrFileTask.Result != null ?
                    lastSubmittedIlrFileTask.Result.FilePreparationDate.GetValueOrDefault().ToString("dd/MM/yyyy") : string.Empty;
            }

            return fundingSummaryFooterModel;
        }
    }
}