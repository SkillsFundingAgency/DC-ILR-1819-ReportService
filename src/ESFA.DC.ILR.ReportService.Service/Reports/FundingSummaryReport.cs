using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using CsvHelper;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Output;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Reports;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Generation;
using ESFA.DC.ILR.ReportService.Model.ILR;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Model.Styling;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports.Abstract;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using EasSubmissionValues = ESFA.DC.ILR.ReportService.Model.Eas.EasSubmissionValues;

namespace ESFA.DC.ILR.ReportService.Service.Reports
{
    public sealed class FundingSummaryReport : AbstractReport, IReport
    {
        private readonly FundingSummaryMapper _fundingSummaryMapper;

        private readonly ModelProperty[] _cachedModelProperties;

        private readonly List<FundingSummaryModel> _fundingSummaryModels;

        private readonly IIlrProviderService _ilrProviderService;
        private readonly IOrgProviderService _orgProviderService;
        private readonly IAllbProviderService _allbProviderService;
        private readonly IFM25ProviderService _fm25ProviderService;
        private readonly IFM35ProviderService _fm35ProviderService;
        private readonly IFM36ProviderService _fm36ProviderService;
        private readonly IFM81TrailBlazerProviderService _fm81TrailBlazerProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILarsProviderService _larsProviderService;
        private readonly IEasProviderService _easProviderService;
        private readonly IPostcodeProviderService _postcodeProviderService;
        private readonly ILargeEmployerProviderService _largeEmployerProviderService;
        private readonly IAllbBuilder _allbBuilder;
        private readonly IFm25Builder _fm25Builder;
        private readonly IFm35Builder _fm35Builder;
        private readonly IFm36Builder _fm36Builder;
        private readonly IFm81Builder _fm81Builder;
        private readonly ITotalBuilder _totalBuilder;
        private readonly IVersionInfo _versionInfo;
        private readonly IExcelStyleProvider _excelStyleProvider;
        private readonly IEasBuilder _easBuilder;
        private readonly ILogger _logger;

        public FundingSummaryReport(
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IIlrProviderService ilrProviderService,
            IOrgProviderService orgProviderService,
            IAllbProviderService allbProviderService,
            IFM25ProviderService fm25ProviderService,
            IFM35ProviderService fm35ProviderService,
            IFM36ProviderService fm36ProviderService,
            IFM81TrailBlazerProviderService fm81TrailBlazerProviderService,
            IValidLearnersService validLearnersService,
            IDateTimeProvider dateTimeProvider,
            IValueProvider valueProvider,
            ILarsProviderService larsProviderService,
            IEasProviderService easProviderService,
            IPostcodeProviderService postcodeProviderService,
            ILargeEmployerProviderService largeEmployerProviderService,
            IAllbBuilder allbBuilder,
            IFm25Builder fm25Builder,
            IFm35Builder fm35Builder,
            IFm36Builder fm36Builder,
            IFm81Builder fm81Builder,
            ITotalBuilder totalBuilder,
            IVersionInfo versionInfo,
            IExcelStyleProvider excelStyleProvider,
            ITopicAndTaskSectionOptions topicAndTaskSectionOptions,
            IEasBuilder easBuilder,
            ILogger logger)
            : base(dateTimeProvider, valueProvider, streamableKeyValuePersistenceService)
        {
            _ilrProviderService = ilrProviderService;
            _orgProviderService = orgProviderService;
            _allbProviderService = allbProviderService;
            _fm25ProviderService = fm25ProviderService;
            _fm35ProviderService = fm35ProviderService;
            _fm36ProviderService = fm36ProviderService;
            _fm81TrailBlazerProviderService = fm81TrailBlazerProviderService;
            _validLearnersService = validLearnersService;
            _larsProviderService = larsProviderService;
            _easProviderService = easProviderService;
            _postcodeProviderService = postcodeProviderService;
            _largeEmployerProviderService = largeEmployerProviderService;
            _allbBuilder = allbBuilder;
            _fm25Builder = fm25Builder;
            _fm35Builder = fm35Builder;
            _fm36Builder = fm36Builder;
            _fm81Builder = fm81Builder;
            _totalBuilder = totalBuilder;
            _versionInfo = versionInfo;
            _excelStyleProvider = excelStyleProvider;
            _dateTimeProvider = dateTimeProvider;
            _easBuilder = easBuilder;
            _logger = logger;

            ReportFileName = "Funding Summary Report";
            ReportTaskName = topicAndTaskSectionOptions.TopicReports_TaskGenerateFundingSummaryReport;

            _fundingSummaryModels = new List<FundingSummaryModel>();
            _fundingSummaryMapper = new FundingSummaryMapper();
            _cachedModelProperties = _fundingSummaryMapper.MemberMaps.OrderBy(x => x.Data.Index).Select(x => new ModelProperty(x.Data.Names.Names.ToArray(), (PropertyInfo)x.Data.Member)).ToArray();
        }

        public async Task GenerateReport(IReportServiceContext reportServiceContext, ZipArchive archive, bool isFis, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInfo("Funding Summary Report Start");

                Task<IMessage> ilrFileTask = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
                Task<ALBGlobal> albDataTask = _allbProviderService.GetAllbData(reportServiceContext, cancellationToken);
                Task<FM25Global> fm25Task = _fm25ProviderService.GetFM25Data(reportServiceContext, cancellationToken);
                Task<FM35Global> fm35Task = _fm35ProviderService.GetFM35Data(reportServiceContext, cancellationToken);
                Task<FM36Global> fm36Task = _fm36ProviderService.GetFM36Data(reportServiceContext, cancellationToken);
                Task<FM81Global> fm81Task = _fm81TrailBlazerProviderService.GetFM81Data(reportServiceContext, cancellationToken);
                Task<List<string>> validLearnersTask = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);
                Task<string> providerNameTask = _orgProviderService.GetProviderName(reportServiceContext, cancellationToken);
                Task<ILRSourceFileInfo> lastSubmittedIlrFileTask = _ilrProviderService.GetLastSubmittedIlrFile(reportServiceContext, cancellationToken);

                var returnPeriod = reportServiceContext.ReturnPeriod;

                Task<List<EasSubmissionValues>> easSubmissionsValuesTask = null;
                if (!isFis)
                {
                    easSubmissionsValuesTask = _easProviderService.GetEasSubmissionValuesAsync(reportServiceContext, cancellationToken);
                }

                _logger.LogInfo("Data Provider Tasks Start");

                await Task.WhenAll(ilrFileTask, albDataTask, fm25Task, fm35Task, fm36Task, fm81Task, validLearnersTask, providerNameTask, easSubmissionsValuesTask ?? Task.CompletedTask, lastSubmittedIlrFileTask);

                _logger.LogInfo("Data Provider Tasks End");

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                FundingSummaryHeaderModel fundingSummaryHeaderModel = await GetHeader(reportServiceContext, ilrFileTask, lastSubmittedIlrFileTask, providerNameTask, cancellationToken, isFis);

                _logger.LogInfo("Header Model Created");

                FundingSummaryFooterModel fundingSummaryFooterModel = await GetFooterAsync(ilrFileTask, lastSubmittedIlrFileTask, cancellationToken);

                _logger.LogInfo("Footer Model Created");

                // Todo: Check keys & titles
                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Traineeships Budget", HeaderType.TitleOnly, 0));
                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Traineeships", HeaderType.All, 2));
                FundingSummaryModel traineeships1618 = _fm25Builder.BuildWithFundLine("ILR 16-18 Traineeships Programme Funding (£)", fm25Task.Result, validLearnersTask.Result, "16-18 Traineeships (Adult Funded)", returnPeriod);
                _fundingSummaryModels.Add(traineeships1618);
                FundingSummaryModel traineeships1924 = _fm25Builder.BuildWithFundLine("ILR 19-24 Traineeships (16-19 Model) Programme Funding (£)", fm25Task.Result, validLearnersTask.Result, "19+ Traineeships (Adult Funded)", returnPeriod);
                _fundingSummaryModels.Add(traineeships1924);
                FundingSummaryModel ilrTraineeshipsTotal = _totalBuilder.TotalRecords("ILR Total 16-18 Traineeships (£)", traineeships1618, traineeships1924);

                FundingSummaryModel easTraineeshipsTotal = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    ilrTraineeshipsTotal.ExcelHeaderStyle = 3;
                    ilrTraineeshipsTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(ilrTraineeshipsTotal);

                    // EAS
                    FundingSummaryModel eas1618TraineeshipsAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Traineeships Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618TraineeshipsAuditAdjustments);

                    FundingSummaryModel eas1618TraineeshipsAuthorisedClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Traineeships Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618TraineeshipsAuthorisedClaims);

                    FundingSummaryModel eas1618TraineeshipsExcessLearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Traineeships Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 16-18 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618TraineeshipsExcessLearningSupport);

                    FundingSummaryModel eas1619TraineeshipsVulStudentBursery = _easBuilder.BuildWithEasSubValueLine("EAS 16-19 Traineeships Bursary Vulnerable Student Bursary (£)", easSubmissionsValuesTask.Result, "Vulnerable Bursary: 16-19 Traineeships Bursary", returnPeriod);
                    _fundingSummaryModels.Add(eas1619TraineeshipsVulStudentBursery);

                    FundingSummaryModel eas1619TraineeshipsFreeMealsBursery = _easBuilder.BuildWithEasSubValueLine("EAS 16-19 Traineeships Bursary Free Meals (£)", easSubmissionsValuesTask.Result, "Free Meals: 16-19 Traineeships Bursary", returnPeriod);
                    _fundingSummaryModels.Add(eas1619TraineeshipsFreeMealsBursery);

                    FundingSummaryModel eas1619TraineeshipsDiscretionaryBursary = _easBuilder.BuildWithEasSubValueLine("EAS 16-19 Traineeships Bursary Discretionary Bursary (£)", easSubmissionsValuesTask.Result, "Discretionary Bursary: 16-19 Traineeships Bursary", returnPeriod);
                    _fundingSummaryModels.Add(eas1619TraineeshipsDiscretionaryBursary);

                    easTraineeshipsTotal = _totalBuilder.TotalRecords("EAS Total 16-18 Traineeships Earnings Adjustment (£)", eas1618TraineeshipsAuditAdjustments, eas1618TraineeshipsAuthorisedClaims, eas1618TraineeshipsExcessLearningSupport, eas1619TraineeshipsVulStudentBursery, eas1619TraineeshipsFreeMealsBursery, eas1619TraineeshipsDiscretionaryBursary);
                    easTraineeshipsTotal.ExcelHeaderStyle = 3;
                    easTraineeshipsTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTraineeshipsTotal);
                }

                FundingSummaryModel traineeshipsTotal = _totalBuilder.TotalRecords("Total 16-18 Traineeships (£)", ilrTraineeshipsTotal, easTraineeshipsTotal);
                traineeshipsTotal.ExcelHeaderStyle = 2;
                traineeshipsTotal.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(traineeshipsTotal);
                FundingSummaryModel traineeshipsTotalBudget = (FundingSummaryModel)traineeshipsTotal.Clone();
                traineeshipsTotalBudget.Title = "Total 16-18 Traineeships Budget (£)";
                traineeshipsTotalBudget.ExcelHeaderStyle = 0;
                traineeshipsTotalBudget.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(traineeshipsTotalBudget);
                FundingSummaryModel traineeshipsTotalBudgetCumulative = _totalBuilder.TotalRecordsCumulative("Total 16-18 Traineeships Budget Cumulative (£)", traineeshipsTotalBudget);
                traineeshipsTotalBudgetCumulative.ExcelHeaderStyle = 0;
                traineeshipsTotalBudgetCumulative.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(traineeshipsTotalBudgetCumulative);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Carry-in Apprenticeships Budget (for starts before 1 May 2017 and non-procured delivery)", HeaderType.TitleOnly, 0));
                _fundingSummaryModels.Add(new FundingSummaryModel());

                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Apprenticeship Frameworks for starts before 1 May 2017", HeaderType.All, 2));
                FundingSummaryModel ilrApprenticeshipProgramme = _fm35Builder.BuildWithFundLine("ILR 16-18 Apprenticeship Frameworks Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(ilrApprenticeshipProgramme);
                FundingSummaryModel ilrApprenticeshipFrameworks = _fm35Builder.BuildWithFundLine("ILR 16-18 Apprenticeship Frameworks Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(ilrApprenticeshipFrameworks);
                FundingSummaryModel ilrTotal1618Apprenticeship = _totalBuilder.TotalRecords("ILR Total 16-18 Apprenticeship Frameworks (£)", ilrApprenticeshipProgramme, ilrApprenticeshipFrameworks);
                FundingSummaryModel easTotal1618Apprenticeship = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    ilrTotal1618Apprenticeship.ExcelHeaderStyle = 3;
                    ilrTotal1618Apprenticeship.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(ilrTotal1618Apprenticeship);

                    // EAS
                    FundingSummaryModel eas1618ApprenticeshipAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Apprenticeship Frameworks Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618ApprenticeshipAuditAdjustments);

                    FundingSummaryModel eas1618ApprenticeshipAuthorisedClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Apprenticeship Frameworks Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618ApprenticeshipAuthorisedClaims);

                    FundingSummaryModel eas1618ApprenticeshipExcessLearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Apprenticeship Frameworks Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 16-18 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618ApprenticeshipExcessLearningSupport);

                    FundingSummaryModel eas1618ApprenticeshipLearnerSupport = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Apprenticeship Frameworks Learner Support (£)", easSubmissionsValuesTask.Result, "Learner Support: 16-18 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618ApprenticeshipLearnerSupport);

                    easTotal1618Apprenticeship = _totalBuilder.TotalRecords("EAS Total 16-18 Apprenticeship Frameworks Earnings Adjustment (£)", eas1618ApprenticeshipAuditAdjustments, eas1618ApprenticeshipAuthorisedClaims, eas1618ApprenticeshipExcessLearningSupport, eas1618ApprenticeshipLearnerSupport);
                    easTotal1618Apprenticeship.ExcelHeaderStyle = 3;
                    easTotal1618Apprenticeship.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTotal1618Apprenticeship);
                }

                FundingSummaryModel totalApprenticeships1618 = _totalBuilder.TotalRecords("Total 16-18 Apprenticeship Frameworks for starts before 1 May 2017 (£)", ilrTotal1618Apprenticeship, easTotal1618Apprenticeship);
                totalApprenticeships1618.ExcelHeaderStyle = 2;
                totalApprenticeships1618.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalApprenticeships1618);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Trailblazer Apprenticeships for starts before 1 May 2017", HeaderType.All, 2));
                FundingSummaryModel trailblazer1618Programme = _fm81Builder.BuildWithFundLine("ILR 16-18 Trailblazer Apprenticeships Programme Funding (Core Government Contribution, Maths and English) (£)", fm81Task.Result, validLearnersTask.Result, "16-18 Trailblazer Apprenticeship", new[] { Constants.Fm81CoreGovContPayment, Constants.Fm81MathEngBalPayment, Constants.Fm81MathEngOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer1618Programme);
                FundingSummaryModel trailblazer1618Incentive = _fm81Builder.BuildWithFundLine("ILR 16-18 Trailblazer Apprenticeships Employer Incentive Payments (Achievement, Small Employer, 16-18) (£)", fm81Task.Result, validLearnersTask.Result, "16-18 Trailblazer Apprenticeship", new[] { Constants.Fm81AchPayment, Constants.Fm81SmallBusPayment, Constants.Fm81YoungAppPayment }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer1618Incentive);
                FundingSummaryModel trailblazer1618Support = _fm81Builder.BuildWithFundLine("ILR 16-18 Trailblazer Apprenticeships Learning Support (£)", fm81Task.Result, validLearnersTask.Result, "16-18 Trailblazer Apprenticeship", new[] { Constants.Fm81LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer1618Support);
                FundingSummaryModel trailblazerTotal = _totalBuilder.TotalRecords("ILR Total 16-18 Trailblazer Apprenticeships (£)", trailblazer1618Programme, trailblazer1618Incentive, trailblazer1618Support);
                FundingSummaryModel easTotal1618TrailblazerApprenticeship = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    trailblazerTotal.ExcelHeaderStyle = 3;
                    trailblazerTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(trailblazerTotal);

                    // EAS
                    FundingSummaryModel eas1618TrailblazerAppAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Trailblazer Apprenticeships Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618TrailblazerAppAuditAdjustments);

                    FundingSummaryModel eas1618TrailblazerAppAuthorisedClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Trailblazer Apprenticeships Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618TrailblazerAppAuthorisedClaims);

                    FundingSummaryModel eas1618TrailblazerAppExcessLearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Trailblazer Apprenticeships Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 16-18 Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1618TrailblazerAppExcessLearningSupport);

                    easTotal1618TrailblazerApprenticeship = _totalBuilder.TotalRecords("EAS Total 16-18 Trailblazer Apprenticeships Earnings Adjustment (£)", eas1618TrailblazerAppAuditAdjustments, eas1618TrailblazerAppAuthorisedClaims, eas1618TrailblazerAppExcessLearningSupport);
                    easTotal1618TrailblazerApprenticeship.ExcelHeaderStyle = 3;
                    easTotal1618TrailblazerApprenticeship.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTotal1618TrailblazerApprenticeship);
                }

                FundingSummaryModel totalTrailblazer1618Before = _totalBuilder.TotalRecords("Total 16-18 Trailblazer Apprenticeships for starts before 1 May 2017 (£)", trailblazerTotal, easTotal1618TrailblazerApprenticeship); // EAS
                totalTrailblazer1618Before.ExcelHeaderStyle = 2;
                totalTrailblazer1618Before.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalTrailblazer1618Before);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Non-Levy Contracted Apprenticeships - Non-procured delivery", HeaderType.All, 2));
                _fundingSummaryModels.Add(_fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Programme Aim Indicative Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36ProgrammeAimOnProgPayment, Constants.Fm36ProgrammeAimBalPayment, Constants.Fm36ProgrammeAimCompletionPayment }, returnPeriod));
                FundingSummaryModel nonLevyCoInvest = _fm36Builder.BuildWithFundLine("...of which Indicative Government Co-Investment Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36ProgrammeAimProgFundIndMinCoInvest }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyCoInvest);
                FundingSummaryModel nonLevyMathsAndEnglish = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Maths and English Programme Funding (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36MathEngOnProgPayment, Constants.Fm36MathEngBalPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyMathsAndEnglish);
                FundingSummaryModel nonLevyUplift = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Framework Uplift (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, Constants.Fm36LDApplic1618FrameworkUpliftCompletionPayment, Constants.Fm36LDApplic1618FrameworkUpliftOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyUplift);
                FundingSummaryModel nonLevyDisadvantage = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Disadvantage Payments (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36DisadvFirstPayment, Constants.Fm36DisadvSecondPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyDisadvantage);
                FundingSummaryModel nonLevyProviders = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LearnDelFirstProv1618Pay, Constants.Fm36LearnDelSecondProv1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyProviders);
                FundingSummaryModel nonLevyEmployers = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Employers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LearnDelFirstEmp1618Pay, Constants.Fm36LearnDelSecondEmp1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyEmployers);
                FundingSummaryModel nonLevySupport = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Learning Support (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Non-Levy Contract", "16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(nonLevySupport);
                FundingSummaryModel nonLevyTotal = _totalBuilder.TotalRecords("ILR Total 16-18 Non-Levy Contracted Apprenticeships (£)", nonLevyCoInvest, nonLevyMathsAndEnglish, nonLevyUplift, nonLevyDisadvantage, nonLevyProviders, nonLevyEmployers, nonLevySupport);

                FundingSummaryModel easTotal1618NonLevy = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    nonLevyTotal.ExcelHeaderStyle = 3;
                    nonLevyTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(nonLevyTotal);

                    // EAS
                    FundingSummaryModel eas1618NonLevyTrainingAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Training Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyTrainingAuditAdjustments);

                    FundingSummaryModel eas1618NonLevyTrainingAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Training Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyTrainingAuthClaims);

                    FundingSummaryModel eas1618NonLevyProviderAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyProviderAuditAdjustments);

                    FundingSummaryModel eas1618NonLevyProviderAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyProviderAuthClaims);

                    FundingSummaryModel eas1618NonLevyProviderExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 16-18 Non-Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyProviderExcessLearning);

                    FundingSummaryModel eas1618NonLevyEmployerAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Employers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyEmployerAuditAdjustments);

                    FundingSummaryModel eas1618NonLevyEmployerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Employers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(eas1618NonLevyEmployerAuthClaims);

                    easTotal1618NonLevy = _totalBuilder.TotalRecords("EAS Total 16-18 Non-Levy Contracted Apprenticeships Earnings Adjustment (£)", eas1618NonLevyTrainingAuditAdjustments, eas1618NonLevyTrainingAuthClaims, eas1618NonLevyProviderAuditAdjustments, eas1618NonLevyProviderAuthClaims, eas1618NonLevyProviderExcessLearning, eas1618NonLevyEmployerAuditAdjustments, eas1618NonLevyEmployerAuthClaims);
                    easTotal1618NonLevy.ExcelHeaderStyle = 3;
                    easTotal1618NonLevy.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTotal1618NonLevy);
                }

                FundingSummaryModel total1618NonLevyNonProcured = _totalBuilder.TotalRecords("Total 16-18 Non-Levy Contracted Apprenticeships – Non-procured delivery (£)", nonLevyTotal, easTotal1618NonLevy); // EAS
                total1618NonLevyNonProcured.ExcelHeaderStyle = 2;
                total1618NonLevyNonProcured.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(total1618NonLevyNonProcured);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("19-23 Apprenticeship Frameworks for starts before 1 May 2017", HeaderType.All, 2));
                FundingSummaryModel apprenticeshipProgramme = _fm35Builder.BuildWithFundLine("ILR 19-23 Apprenticeship Frameworks Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "19-23 Apprenticeship" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(apprenticeshipProgramme);
                FundingSummaryModel apprenticeshipSupport = _fm35Builder.BuildWithFundLine("ILR 19-23 Apprenticeship Frameworks Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "19-23 Apprenticeship" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(apprenticeshipSupport);
                FundingSummaryModel apprenticeshipTotal = _totalBuilder.TotalRecords("ILR Total 19-23 Apprenticeship Frameworks (£)", apprenticeshipProgramme, apprenticeshipSupport);

                FundingSummaryModel easTotal1923Frameworks = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    apprenticeshipTotal.ExcelHeaderStyle = 3;
                    apprenticeshipTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(apprenticeshipTotal);

                    // EAS
                    FundingSummaryModel eas1923FrameworksAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Apprenticeship Frameworks Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 19-23 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923FrameworksAuditAdjustments);

                    FundingSummaryModel eas1923FrameworksAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Apprenticeship Frameworks Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 19-23 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923FrameworksAuthClaims);

                    FundingSummaryModel eas1923FrameworksExcessLearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Apprenticeship Frameworks Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 19-23 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923FrameworksExcessLearningSupport);

                    FundingSummaryModel eas1923FrameworksLearnerSupport = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Apprenticeship Frameworks Learner Support (£)", easSubmissionsValuesTask.Result, "Learner Support: 19-23 Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923FrameworksLearnerSupport);

                    easTotal1923Frameworks = _totalBuilder.TotalRecords("EAS Total 19-23 Apprenticeship Frameworks Earnings Adjustment (£)", eas1923FrameworksAuditAdjustments, eas1923FrameworksAuthClaims, eas1923FrameworksExcessLearningSupport, eas1923FrameworksLearnerSupport);
                    easTotal1923Frameworks.ExcelHeaderStyle = 3;
                    easTotal1923Frameworks.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTotal1923Frameworks);
                }

                FundingSummaryModel totalApprenticeship1923Before = _totalBuilder.TotalRecords("Total 19-23 Apprenticeship Frameworks for starts before 1 May 2017 (£)", apprenticeshipTotal, easTotal1923Frameworks); // EAS
                totalApprenticeship1923Before.ExcelHeaderStyle = 2;
                totalApprenticeship1923Before.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalApprenticeship1923Before);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("19-23 Trailblazer Apprenticeships for starts before 1 May 2017", HeaderType.All, 2));
                FundingSummaryModel trailblazer1923Funding = _fm81Builder.BuildWithFundLine("ILR 19-23 Trailblazer Apprenticeships Programme Funding (Core Government Contribution, Maths and English) (£)", fm81Task.Result, validLearnersTask.Result, "19-23 Trailblazer Apprenticeship", new[] { Constants.Fm81CoreGovContPayment, Constants.Fm81MathEngBalPayment, Constants.Fm81MathEngOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer1923Funding);
                FundingSummaryModel trailblazer1923Payment = _fm81Builder.BuildWithFundLine("ILR 19-23 Trailblazer Apprenticeships Employer Incentive Payments (Achievement, Small Employer, 16-18) (£)", fm81Task.Result, validLearnersTask.Result, "19-23 Trailblazer Apprenticeship", new[] { Constants.Fm81AchPayment, Constants.Fm81SmallBusPayment, Constants.Fm81YoungAppPayment }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer1923Payment);
                FundingSummaryModel trailblazer1923Support = _fm81Builder.BuildWithFundLine("ILR 19-23 Trailblazer Apprenticeships Learning Support (£)", fm81Task.Result, validLearnersTask.Result, "19-23 Trailblazer Apprenticeship", new[] { Constants.Fm81LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer1923Support);
                FundingSummaryModel trailblazer1923Total = _totalBuilder.TotalRecords("ILR Total 19-23 Trailblazer Apprenticeships (£)", trailblazer1923Funding, trailblazer1923Payment, trailblazer1923Support);

                FundingSummaryModel easTotal1923Trailblazer = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    trailblazer1923Total.ExcelHeaderStyle = 3;
                    trailblazer1923Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(trailblazer1923Total);

                    // EAS
                    FundingSummaryModel eas1923TrailblazerAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Trailblazer Apprenticeships Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 19-23 Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923TrailblazerAuditAdjustments);

                    FundingSummaryModel eas1923TrailblazerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Trailblazer Apprenticeships Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 19-23 Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923TrailblazerAuthClaims);

                    FundingSummaryModel eas1923TrailblazerExcessLearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 19-23 Trailblazer Apprenticeships Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 19-23 Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas1923TrailblazerExcessLearningSupport);

                    easTotal1923Trailblazer = _totalBuilder.TotalRecords("EAS Total 19-23 Trailblazer Apprenticeships Earnings Adjustment (£)", eas1923TrailblazerAuditAdjustments, eas1923TrailblazerAuthClaims, eas1923TrailblazerExcessLearningSupport);
                    easTotal1923Trailblazer.ExcelHeaderStyle = 3;
                    easTotal1923Trailblazer.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTotal1923Trailblazer);
                }

                FundingSummaryModel totalTrailblazer1923Before = _totalBuilder.TotalRecords("Total 19-23 Trailblazer Apprenticeships for starts before 1 May 2017 (£)", trailblazer1923Total, easTotal1923Trailblazer); // EAS
                totalTrailblazer1923Before.ExcelHeaderStyle = 2;
                totalTrailblazer1923Before.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalTrailblazer1923Before);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("24+ Apprenticeship Frameworks for starts before 1 May 2017", HeaderType.All, 2));
                FundingSummaryModel apprenticeship24Programme = _fm35Builder.BuildWithFundLine("ILR 24+ Apprenticeship Frameworks Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "24+ Apprenticeship" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(apprenticeship24Programme);
                FundingSummaryModel apprenticeship24Support = _fm35Builder.BuildWithFundLine("ILR 24+ Apprenticeship Frameworks Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "24+ Apprenticeship" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(apprenticeship24Support);
                FundingSummaryModel apprenticeship24Total = _totalBuilder.TotalRecords("ILR Total 24+ Apprenticeship Frameworks (£)", apprenticeship24Programme, apprenticeship24Support);

                FundingSummaryModel easTotal24AppFramework = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    apprenticeship24Total.ExcelHeaderStyle = 3;
                    apprenticeship24Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(apprenticeship24Total);

                    // EAS
                    FundingSummaryModel eas24AppFrameworkAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Apprenticeship Frameworks Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 24+ Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas24AppFrameworkAuditAdjustments);

                    FundingSummaryModel eas24AppFrameworkAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Apprenticeship Frameworks Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 24+ Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas24AppFrameworkAuthClaims);

                    FundingSummaryModel eas24AppFrameworkExcessLearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Apprenticeship Frameworks Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 24+ Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas24AppFrameworkExcessLearningSupport);

                    FundingSummaryModel eas24AppFrameworkLearnerSupport = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Apprenticeship Frameworks Learner Support (£)", easSubmissionsValuesTask.Result, "Learner Support: 24+ Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(eas24AppFrameworkLearnerSupport);

                    easTotal24AppFramework = _totalBuilder.TotalRecords("EAS Total 24+ Apprenticeship Frameworks Earnings Adjustment (£)", eas24AppFrameworkAuditAdjustments, eas24AppFrameworkAuthClaims, eas24AppFrameworkExcessLearningSupport, eas24AppFrameworkLearnerSupport);
                    easTotal24AppFramework.ExcelHeaderStyle = 3;
                    easTotal24AppFramework.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTotal24AppFramework);
                }

                FundingSummaryModel totalApprenticeship24Before = _totalBuilder.TotalRecords("Total 24+ Apprenticeship Frameworks for starts before 1 May 2017 (£)", apprenticeship24Total, easTotal24AppFramework); // EAS
                totalApprenticeship24Before.ExcelHeaderStyle = 2;
                totalApprenticeship24Before.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalApprenticeship24Before);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("24+ Trailblazer Apprenticeships for starts before 1 May 2017", HeaderType.All, 2));
                FundingSummaryModel trailblazer24Funding = _fm81Builder.BuildWithFundLine("ILR 24+ Trailblazer Apprenticeships Programme Funding (Core Government Contribution, Maths and English) (£)", fm81Task.Result, validLearnersTask.Result, "24+ Trailblazer Apprenticeship", new[] { Constants.Fm81CoreGovContPayment, Constants.Fm81MathEngBalPayment, Constants.Fm81MathEngOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer24Funding);
                FundingSummaryModel trailblazer24Payment = _fm81Builder.BuildWithFundLine("ILR 24+ Trailblazer Apprenticeships Employer Incentive Payments (Achievement, Small Employer, 16-18) (£)", fm81Task.Result, validLearnersTask.Result, "24+ Trailblazer Apprenticeship", new[] { Constants.Fm81AchPayment, Constants.Fm81SmallBusPayment, Constants.Fm81YoungAppPayment }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer24Payment);
                FundingSummaryModel trailblazer24Support = _fm81Builder.BuildWithFundLine("ILR 24+ Trailblazer Apprenticeships Learning Support (£)", fm81Task.Result, validLearnersTask.Result, "24+ Trailblazer Apprenticeship", new[] { Constants.Fm81LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(trailblazer24Support);
                FundingSummaryModel trailblazer24Total = _totalBuilder.TotalRecords("ILR Total 24+ Trailblazer Apprenticeships (£)", trailblazer24Funding, trailblazer24Payment, trailblazer24Support);

                FundingSummaryModel easTrailblazer24Total = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    trailblazer24Total.ExcelHeaderStyle = 3;
                    trailblazer24Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(trailblazer24Total);

                    // EAS
                    FundingSummaryModel easTrailblazer24AuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Trailblazer Apprenticeships Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 24+ Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(easTrailblazer24AuditAdjustments);

                    FundingSummaryModel easTrailblazer24AuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Trailblazer Apprenticeships Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 24+ Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(easTrailblazer24AuthClaims);

                    FundingSummaryModel easTrailblazer24LearningSupport = _easBuilder.BuildWithEasSubValueLine("EAS 24+ Trailblazer Apprenticeships Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 24+ Trailblazer Apprenticeships", returnPeriod);
                    _fundingSummaryModels.Add(easTrailblazer24LearningSupport);

                    easTrailblazer24Total = _totalBuilder.TotalRecords("EAS Total 24+ Trailblazer Apprenticeships Earnings Adjustment (£)", easTrailblazer24AuditAdjustments, easTrailblazer24AuthClaims, easTrailblazer24LearningSupport);
                    easTrailblazer24Total.ExcelHeaderStyle = 3;
                    easTrailblazer24Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTrailblazer24Total);
                }

                FundingSummaryModel totalTrailblazer24Before = _totalBuilder.TotalRecords("Total 24+ Trailblazer Apprenticeships for starts before 1 May 2017 (£)", trailblazer24Total, easTrailblazer24Total); // EAS
                totalTrailblazer24Before.ExcelHeaderStyle = 2;
                totalTrailblazer24Before.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalTrailblazer24Before);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Adult Non-Levy Contracted Apprenticeships – Non-procured delivery", HeaderType.All, 2));
                _fundingSummaryModels.Add(_fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Programme Aim Indicative Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36ProgrammeAimOnProgPayment, Constants.Fm36ProgrammeAimBalPayment, Constants.Fm36ProgrammeAimCompletionPayment }, returnPeriod));
                FundingSummaryModel nonLevyCoInvestAdult = _fm36Builder.BuildWithFundLine("...of which Indicative Government Co-Investment Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36ProgrammeAimProgFundIndMinCoInvest }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyCoInvestAdult);
                FundingSummaryModel nonLevyMathsAndEnglishAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Maths and English Programme Funding (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36MathEngOnProgPayment, Constants.Fm36MathEngBalPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyMathsAndEnglishAdult);
                FundingSummaryModel nonLevyUpliftAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Framework Uplift (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, Constants.Fm36LDApplic1618FrameworkUpliftCompletionPayment, Constants.Fm36LDApplic1618FrameworkUpliftOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyUpliftAdult);
                FundingSummaryModel nonLevyDisadvantageAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Disadvantage Payments (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36DisadvFirstPayment, Constants.Fm36DisadvSecondPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyDisadvantageAdult);
                FundingSummaryModel nonLevyProvidersAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LearnDelFirstProv1618Pay, Constants.Fm36LearnDelSecondProv1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyProvidersAdult);
                FundingSummaryModel nonLevyEmployersAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Additional Payments for Employers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LearnDelFirstEmp1618Pay, Constants.Fm36LearnDelSecondEmp1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyEmployersAdult);
                FundingSummaryModel nonLevySupportAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Learning Support (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Non-Levy Contract", "19+ Apprenticeship (From May 2017) Non-Levy Contract (non-procured)" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(nonLevySupportAdult);
                FundingSummaryModel nonLevyTotalAdult = _totalBuilder.TotalRecords("ILR Total Adult Non-Levy Contracted Apprenticeships (£)", nonLevyCoInvestAdult, nonLevyMathsAndEnglishAdult, nonLevyUpliftAdult, nonLevyDisadvantageAdult, nonLevyProvidersAdult, nonLevyEmployersAdult, nonLevySupportAdult);

                FundingSummaryModel easNonLevyTotalAdult = new FundingSummaryModel(); // EAS
                if (!isFis)
                {
                    nonLevyTotalAdult.ExcelHeaderStyle = 3;
                    nonLevyTotalAdult.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(nonLevyTotalAdult);

                    // EAS
                    FundingSummaryModel easNonLevyAdultTrainingAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Training Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultTrainingAuditAdjustment);

                    FundingSummaryModel easNonLevyAdultTrainingAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Training Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultTrainingAuthClaims);

                    FundingSummaryModel easNonLevyAdultProviderAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProviderAuditAdjustment);

                    FundingSummaryModel easNonLevyAdultProviderAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProviderAuthClaims);

                    FundingSummaryModel easNonLevyAdultProviderExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: Adult Non-Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProviderExcessLearning);

                    FundingSummaryModel easNonLevyAdultEmployerAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Employers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultEmployerAuditAdjustment);

                    FundingSummaryModel easNonLevyAdultEmployerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Employers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultEmployerAuthClaims);

                    easNonLevyTotalAdult = _totalBuilder.TotalRecords("EAS Total Adult Non-Levy Contracted Apprenticeships Earnings Adjustment (£)", easNonLevyAdultTrainingAuditAdjustment, easNonLevyAdultTrainingAuthClaims, easNonLevyAdultProviderAuditAdjustment, easNonLevyAdultProviderAuthClaims, easNonLevyAdultProviderExcessLearning, easNonLevyAdultEmployerAuditAdjustment, easNonLevyAdultEmployerAuthClaims);
                    easNonLevyTotalAdult.ExcelHeaderStyle = 3;
                    easNonLevyTotalAdult.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easNonLevyTotalAdult);
                }

                FundingSummaryModel totalNonLevyNonProcuredAdult = _totalBuilder.TotalRecords("Total Adult Non-Levy Contracted Apprenticeships – Non-procured delivery (£)", nonLevyTotalAdult, easNonLevyTotalAdult); // EAS
                totalNonLevyNonProcuredAdult.ExcelHeaderStyle = 2;
                totalNonLevyNonProcuredAdult.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalNonLevyNonProcuredAdult);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                FundingSummaryModel totalCarryInBefore = _totalBuilder.TotalRecords("Total Carry-in Apprenticeships Budget (£) (for starts before 1 May 2017 and non-procured delivery)", totalApprenticeships1618, totalTrailblazer1618Before, total1618NonLevyNonProcured, totalApprenticeship1923Before, totalTrailblazer1923Before, totalApprenticeship24Before, totalTrailblazer24Before, totalNonLevyNonProcuredAdult);
                totalCarryInBefore.ExcelHeaderStyle = 0;
                totalCarryInBefore.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalCarryInBefore);
                FundingSummaryModel totalCarryInCumulativeBefore = _totalBuilder.TotalRecordsCumulative("Total Carry-in Apprenticeships Budget Cumulative (£) (for starts before 1 May 2017 and non-procured delivery)", totalCarryInBefore);
                totalCarryInCumulativeBefore.ExcelHeaderStyle = 0;
                totalCarryInCumulativeBefore.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalCarryInCumulativeBefore);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Levy Contracted Apprenticeships Budget for starts on or after 1 May 2017", HeaderType.TitleOnly, 0));
                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Levy Contracted Apprenticeships", HeaderType.All, 2));

                FundingSummaryModel levyAimIndicative = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Programme Aim Indicative Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36ProgrammeAimOnProgPayment, Constants.Fm36ProgrammeAimBalPayment, Constants.Fm36ProgrammeAimCompletionPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyAimIndicative);
                FundingSummaryModel levyMathsAndEnglish1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Maths and English Programme Funding (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36MathEngOnProgPayment, Constants.Fm36MathEngBalPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyMathsAndEnglish1618);
                FundingSummaryModel levyUplift1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Framework Uplift (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, Constants.Fm36LDApplic1618FrameworkUpliftCompletionPayment, Constants.Fm36LDApplic1618FrameworkUpliftOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyUplift1618);
                FundingSummaryModel levyDisadvantage1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Disadvantage Payments (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36DisadvFirstPayment, Constants.Fm36DisadvSecondPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyDisadvantage1618);
                FundingSummaryModel levyProviders1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Additional Payments for Providers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnDelFirstProv1618Pay, Constants.Fm36LearnDelSecondProv1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(levyProviders1618);
                FundingSummaryModel levyEmployers1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Additional Payments for Employers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnDelFirstEmp1618Pay, Constants.Fm36LearnDelSecondEmp1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(levyEmployers1618);
                FundingSummaryModel levyApprentice1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Additional Payments for Apprentices (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnDelLearnAddPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyApprentice1618);
                FundingSummaryModel levySupport1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Levy Contracted Apprenticeships Learning Support (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(levySupport1618);
                FundingSummaryModel levyTotal1618 = _totalBuilder.TotalRecords("ILR Total 16-18 Levy Contracted Apprenticeships (£)", levyAimIndicative, levyMathsAndEnglish1618, levyUplift1618, levyDisadvantage1618, levyProviders1618, levyEmployers1618, levyApprentice1618, levySupport1618);

                FundingSummaryModel easLevyTotal1618 = new FundingSummaryModel();
                if (!isFis)
                {
                    levyTotal1618.ExcelHeaderStyle = 3;
                    levyTotal1618.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(levyTotal1618);

                    // EAS
                    FundingSummaryModel easLevy1618TrainingAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Training Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618TrainingAuditAdjustments);

                    FundingSummaryModel easLevy1618TrainingAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Training Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618TrainingAuthClaims);

                    FundingSummaryModel easLevy1618ProviderAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Providers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618ProviderAuditAdjustment);

                    FundingSummaryModel easLevy1618ProviderAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Providers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618ProviderAuthClaims);

                    FundingSummaryModel easLevy1618ProviderExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Providers Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 16-18 Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618ProviderExcessLearning);

                    FundingSummaryModel easLevy1618EmployerAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Employers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618EmployerAuditAdjustment);

                    FundingSummaryModel easLevy1618EmployerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Employers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618EmployerAuthClaims);

                    FundingSummaryModel easLevy1618ApprenticeAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Apprentices Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618ApprenticeAuditAdjustments);

                    FundingSummaryModel easLevy1618ApprenticeAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Levy Contracted Apprenticeships Additional Payments for Apprentices Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easLevy1618ApprenticeAuthClaims);

                    easLevyTotal1618 = _totalBuilder.TotalRecords("EAS Total 16-18 Levy Contracted Apprenticeships Earnings Adjustment (£)", easLevy1618TrainingAuditAdjustments, easLevy1618TrainingAuthClaims, easLevy1618ProviderAuditAdjustment, easLevy1618ProviderAuthClaims, easLevy1618ProviderExcessLearning, easLevy1618ApprenticeAuditAdjustments, easLevy1618ApprenticeAuthClaims);
                    easLevyTotal1618.ExcelHeaderStyle = 3;
                    easLevyTotal1618.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easLevyTotal1618);
                }

                FundingSummaryModel totalLevyNonProcured1618 = _totalBuilder.TotalRecords("Total 16-18 Levy Contracted Apprenticeships (£)", levyTotal1618, easLevyTotal1618); // EAS
                totalLevyNonProcured1618.ExcelHeaderStyle = 2;
                totalLevyNonProcured1618.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalLevyNonProcured1618);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Adult Levy Contracted Apprenticeships", HeaderType.All, 2));
                FundingSummaryModel levyIndicativeAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Programme Aim Indicative Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36ProgrammeAimOnProgPayment, Constants.Fm36ProgrammeAimBalPayment, Constants.Fm36ProgrammeAimCompletionPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyIndicativeAdult);
                FundingSummaryModel levyMathsAndEnglishAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Maths and English Programme Funding (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36MathEngOnProgPayment, Constants.Fm36MathEngBalPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyMathsAndEnglishAdult);
                FundingSummaryModel levyUpliftAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Framework Uplift (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, Constants.Fm36LDApplic1618FrameworkUpliftCompletionPayment, Constants.Fm36LDApplic1618FrameworkUpliftOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyUpliftAdult);
                FundingSummaryModel levyDisadvantageAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Disadvantage Payments (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36DisadvFirstPayment, Constants.Fm36DisadvSecondPayment }, returnPeriod);
                _fundingSummaryModels.Add(levyDisadvantageAdult);
                FundingSummaryModel levyProvidersAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Additional Payments for Providers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnDelFirstProv1618Pay, Constants.Fm36LearnDelSecondProv1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(levyProvidersAdult);
                FundingSummaryModel levyEmployersAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Additional Payments for Employers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnDelFirstEmp1618Pay, Constants.Fm36LearnDelSecondEmp1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(levyEmployersAdult);
                FundingSummaryModel levyApprenticesAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Additional Payments for Apprentices (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(levyApprenticesAdult);
                FundingSummaryModel levySupportAdult = _fm36Builder.BuildWithFundLine("ILR Adult Levy Contracted Apprenticeships Learning Support (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship (From May 2017) Levy Contract" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(levySupportAdult);
                FundingSummaryModel levyTotalAdultIlr = _totalBuilder.TotalRecords("ILR Total Adult Levy Contracted Apprenticeships (£)", levyIndicativeAdult, levyMathsAndEnglishAdult, levyUpliftAdult, levyDisadvantageAdult, levyProvidersAdult, levyEmployersAdult, levyApprenticesAdult, levySupportAdult);

                FundingSummaryModel easLevyTotalAdult = new FundingSummaryModel();
                if (!isFis)
                {
                    levyTotalAdultIlr.ExcelHeaderStyle = 3;
                    levyTotalAdultIlr.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(levyTotalAdultIlr);

                    // EAS
                    FundingSummaryModel easLevyAdultTrainingAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Training Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultTrainingAuditAdjustments);

                    FundingSummaryModel easLevyAdultTrainingAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Training Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Levy Apprenticeships - Training", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultTrainingAuthClaims);

                    FundingSummaryModel easLevyAdultProviderAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Providers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultProviderAuditAdjustment);

                    FundingSummaryModel easLevyAdultProviderAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Providers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultProviderAuthClaims);

                    FundingSummaryModel easLevyAdultProviderExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Providers Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: Adult Levy Apprenticeships - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultProviderExcessLearning);

                    FundingSummaryModel easLevyAdultEmployerAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Employers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultEmployerAuditAdjustment);

                    FundingSummaryModel easLevyAdultEmployerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Employers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Levy Apprenticeships - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultEmployerAuthClaims);

                    FundingSummaryModel easLevyAdultApprenticeAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Apprentices Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Levy Apprenticeships - Apprentice", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultApprenticeAuditAdjustments);

                    FundingSummaryModel easLevyAdultApprenticeAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Levy Contracted Apprenticeships Additional Payments for Apprentices Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Levy Apprenticeships - Apprentice", returnPeriod);
                    _fundingSummaryModels.Add(easLevyAdultApprenticeAuthClaims);

                    easLevyTotalAdult = _totalBuilder.TotalRecords("EAS Total Adult Levy Contracted Apprenticeships Earnings Adjustment (£)", easLevyAdultTrainingAuditAdjustments, easLevyAdultTrainingAuthClaims, easLevyAdultProviderAuditAdjustment, easLevyAdultProviderAuthClaims, easLevyAdultProviderExcessLearning, easLevyAdultApprenticeAuditAdjustments, easLevyAdultApprenticeAuthClaims);
                    easLevyTotalAdult.ExcelHeaderStyle = 3;
                    easLevyTotalAdult.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easLevyTotalAdult);
                }

                FundingSummaryModel totalLevyNonProcuredAdult = _totalBuilder.TotalRecords("Total Adult Levy Contracted Apprenticeships (£)", levyTotalAdultIlr, easLevyTotalAdult); // EAS
                totalLevyNonProcuredAdult.ExcelHeaderStyle = 2;
                totalLevyNonProcuredAdult.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalLevyNonProcuredAdult);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                FundingSummaryModel totalLevyApprenticeshipAfter = _totalBuilder.TotalRecords("Total Levy Contracted Apprenticeships Budget for starts on or after 1 May 2017 (£)", levyTotalAdultIlr, totalLevyNonProcured1618);
                totalLevyApprenticeshipAfter.ExcelHeaderStyle = 0;
                totalLevyApprenticeshipAfter.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalLevyApprenticeshipAfter);
                FundingSummaryModel totalLevyNonProcuredAdultCumulative = _totalBuilder.TotalRecordsCumulative("Total Levy Contracted Apprenticeships Budget Cumulative for starts on or after 1 May 2017 (£)", totalLevyApprenticeshipAfter);
                totalLevyNonProcuredAdultCumulative.ExcelHeaderStyle = 0;
                totalLevyNonProcuredAdultCumulative.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalLevyNonProcuredAdultCumulative);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Non-Levy Contracted Apprenticeships Budget - Procured delivery", HeaderType.TitleOnly, 0));

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("16-18 Non-Levy Contracted Apprenticeships", HeaderType.All, 2));
                _fundingSummaryModels.Add(_fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Programme Aim Indicative Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36ProgrammeAimOnProgPayment, Constants.Fm36ProgrammeAimBalPayment, Constants.Fm36ProgrammeAimCompletionPayment }, returnPeriod));
                FundingSummaryModel nonLevyCoInvest1618 = _fm36Builder.BuildWithFundLine("...of which Indicative Government Co-Investment Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36ProgrammeAimProgFundIndMinCoInvest }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyCoInvest1618);
                FundingSummaryModel nonLevyMathsAndEnglish1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Maths and English Programme Funding (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36MathEngOnProgPayment, Constants.Fm36MathEngBalPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyMathsAndEnglish1618);
                FundingSummaryModel nonLevyUplift1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Framework Uplift (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, Constants.Fm36LDApplic1618FrameworkUpliftCompletionPayment, Constants.Fm36LDApplic1618FrameworkUpliftOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyUplift1618);
                FundingSummaryModel nonLevyDisadvantage1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Disadvantage Payments (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36DisadvFirstPayment, Constants.Fm36DisadvSecondPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyDisadvantage1618);
                FundingSummaryModel nonLevyProviders1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnDelFirstProv1618Pay, Constants.Fm36LearnDelSecondProv1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyProviders1618);
                FundingSummaryModel nonLevyEmployers1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Employers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnDelFirstEmp1618Pay, Constants.Fm36LearnDelSecondEmp1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyEmployers1618);
                FundingSummaryModel nonLevyApprentices1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Apprentices (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnDelLearnAddPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyApprentices1618);
                FundingSummaryModel nonLevySupport1618 = _fm36Builder.BuildWithFundLine("ILR 16-18 Non-Levy Contracted Apprenticeships Learning Support (£)", fm36Task.Result, validLearnersTask.Result, new[] { "16-18 Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(nonLevySupport1618);
                FundingSummaryModel nonLevyTotal1618 = _totalBuilder.TotalRecords("ILR Total 16-18 Non-Levy Contracted Apprenticeships (£)", nonLevyCoInvest1618, nonLevyMathsAndEnglish1618, nonLevyUplift1618, nonLevyDisadvantage1618, nonLevyProviders1618, nonLevyEmployers1618, nonLevyApprentices1618, nonLevySupport1618);

                FundingSummaryModel easNonLevyTotal1618 = new FundingSummaryModel();
                if (!isFis)
                {
                    nonLevyTotal1618.ExcelHeaderStyle = 3;
                    nonLevyTotal1618.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(nonLevyTotal1618);

                    // EAS
                    FundingSummaryModel easNonLevy1618TrainingAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Training Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships (procured) - Training", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618TrainingAuditAdjustments);

                    FundingSummaryModel easNonLevy1618TrainingAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Training Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships (procured) - Training", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618TrainingAuthClaims);

                    FundingSummaryModel easNonLevy1618ProviderAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships (procured) - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618ProviderAuditAdjustment);

                    FundingSummaryModel easNonLevy1618ProviderAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships (procured) - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618ProviderAuthClaims);

                    FundingSummaryModel easNonLevy1618ProviderExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Providers Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 16-18 Non-Levy Apprenticeships (procured) - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618ProviderExcessLearning);

                    FundingSummaryModel easNonLevy1618EmployerAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Employers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships (procured) - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618EmployerAuditAdjustment);

                    FundingSummaryModel easNonLevy1618EmployerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Employers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships (procured) - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618EmployerAuthClaims);

                    FundingSummaryModel easNonLevy1618ApprenticeAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Apprentices Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 16-18 Non-Levy Apprenticeships (procured) - Apprentice", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618ApprenticeAuditAdjustments);

                    FundingSummaryModel easNonLevy1618ApprenticeAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 16-18 Non-Levy Contracted Apprenticeships Additional Payments for Apprentices Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 16-18 Non-Levy Apprenticeships (procured) - Apprentice", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevy1618ApprenticeAuthClaims);

                    easNonLevyTotal1618 = _totalBuilder.TotalRecords("EAS Total 16-18 Non-Levy Contracted Apprenticeships Earnings Adjustment (£)", easNonLevy1618TrainingAuditAdjustments, easNonLevy1618TrainingAuthClaims, easNonLevy1618ProviderAuditAdjustment, easNonLevy1618ProviderAuthClaims, easNonLevy1618ProviderExcessLearning, easNonLevy1618ApprenticeAuditAdjustments, easNonLevy1618ApprenticeAuthClaims);
                    easNonLevyTotal1618.ExcelHeaderStyle = 3;
                    easNonLevyTotal1618.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easNonLevyTotal1618);
                }

                FundingSummaryModel totalNonLevyNonProcured1618 = _totalBuilder.TotalRecords("Total 16-18 Non-Levy Contracted Apprenticeships (£)", nonLevyTotal1618, easNonLevyTotal1618); // EAS
                totalNonLevyNonProcured1618.ExcelHeaderStyle = 2;
                totalNonLevyNonProcured1618.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalNonLevyNonProcured1618);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Adult Non-Levy Contracted Apprenticeships", HeaderType.All, 2));
                _fundingSummaryModels.Add(_fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Programme Aim Indicative Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36ProgrammeAimOnProgPayment, Constants.Fm36ProgrammeAimBalPayment, Constants.Fm36ProgrammeAimCompletionPayment }, returnPeriod));
                FundingSummaryModel nonLevyCoInvestProcuredAdult = _fm36Builder.BuildWithFundLine("...of which Indicative Government Co-Investment Earnings (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36ProgrammeAimProgFundIndMinCoInvest }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyCoInvestProcuredAdult);
                FundingSummaryModel nonLevyMathsAndEnglishProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Maths and English Programme Funding (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36MathEngOnProgPayment, Constants.Fm36MathEngBalPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyMathsAndEnglishProcuredAdult);
                FundingSummaryModel nonLevyUpliftProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Framework Uplift (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LDApplic1618FrameworkUpliftBalancingPayment, Constants.Fm36LDApplic1618FrameworkUpliftCompletionPayment, Constants.Fm36LDApplic1618FrameworkUpliftOnProgPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyUpliftProcuredAdult);
                FundingSummaryModel nonLevyDisadvantageProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Disadvantage Payments (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36DisadvFirstPayment, Constants.Fm36DisadvSecondPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyDisadvantageProcuredAdult);
                FundingSummaryModel nonLevyProvidersProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnDelFirstProv1618Pay, Constants.Fm36LearnDelSecondProv1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyProvidersProcuredAdult);
                FundingSummaryModel nonLevyEmployersProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Additional Payments for Employers (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnDelFirstEmp1618Pay, Constants.Fm36LearnDelSecondEmp1618Pay }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyEmployersProcuredAdult);
                FundingSummaryModel nonLevyApprenticesProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Additional Payments for Apprentices (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnDelLearnAddPayment }, returnPeriod);
                _fundingSummaryModels.Add(nonLevyApprenticesProcuredAdult);
                FundingSummaryModel nonLevySupportProcuredAdult = _fm36Builder.BuildWithFundLine("ILR Adult Non-Levy Contracted Apprenticeships Learning Support (£)", fm36Task.Result, validLearnersTask.Result, new[] { "19+ Apprenticeship Non-Levy Contract (procured)" }, new[] { Constants.Fm36LearnSuppFundCash }, returnPeriod);
                _fundingSummaryModels.Add(nonLevySupportProcuredAdult);
                FundingSummaryModel nonLevyTotalProcuredAdult = _totalBuilder.TotalRecords("ILR Total Adult Non-Levy Contracted Apprenticeships (£)", nonLevyCoInvestProcuredAdult, nonLevyMathsAndEnglishProcuredAdult, nonLevyUpliftProcuredAdult, nonLevyDisadvantageProcuredAdult, nonLevyProvidersProcuredAdult, nonLevyEmployersProcuredAdult, nonLevyApprenticesProcuredAdult, nonLevySupportProcuredAdult);

                FundingSummaryModel easNonLevyTotalProcuredAdult = new FundingSummaryModel();
                if (!isFis)
                {
                    nonLevyTotalProcuredAdult.ExcelHeaderStyle = 3;
                    nonLevyTotalProcuredAdult.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(nonLevyTotalProcuredAdult);

                    // EAS
                    FundingSummaryModel easNonLevyAdultProcuredTrainingAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Training Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships (procured) - Training", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredTrainingAuditAdjustments);

                    FundingSummaryModel easNonLevyAdultProcuredTrainingAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Training Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships (procured) - Training", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredTrainingAuthClaims);

                    FundingSummaryModel easNonLevyAdultProcuredProviderAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships (procured) - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredProviderAuditAdjustment);

                    FundingSummaryModel easNonLevyAdultProcuredProviderAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships (procured) - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredProviderAuthClaims);

                    FundingSummaryModel easNonLevyAdultProcuredProviderExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Providers Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: Adult Non-Levy Apprenticeships (procured) - Provider", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredProviderExcessLearning);

                    FundingSummaryModel easNonLevyAdultProcuredEmployerAuditAdjustment = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Employers Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships (procured) - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredEmployerAuditAdjustment);

                    FundingSummaryModel easNonLevyAdultProcuredEmployerAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Employers Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships (procured) - Employer", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredEmployerAuthClaims);

                    FundingSummaryModel easNonLevyAdultProcuredApprenticeAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Apprentices Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Adult Non-Levy Apprenticeships (procured) - Apprentice", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredApprenticeAuditAdjustments);

                    FundingSummaryModel easNonLevyAdultProcuredApprenticeAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS Adult Non-Levy Contracted Apprenticeships Additional Payments for Apprentices Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: Adult Non-Levy Apprenticeships (procured) - Apprentice", returnPeriod);
                    _fundingSummaryModels.Add(easNonLevyAdultProcuredApprenticeAuthClaims);

                    easNonLevyTotalProcuredAdult = _totalBuilder.TotalRecords("EAS Total Adult Non-Levy Contracted Apprenticeships Earnings Adjustment (£)", easNonLevyAdultProcuredTrainingAuditAdjustments, easNonLevyAdultProcuredTrainingAuthClaims, easNonLevyAdultProcuredProviderAuditAdjustment, easNonLevyAdultProcuredProviderAuthClaims, easNonLevyAdultProcuredProviderExcessLearning, easNonLevyAdultProcuredApprenticeAuditAdjustments, easNonLevyAdultProcuredApprenticeAuthClaims);
                    easNonLevyTotalProcuredAdult.ExcelHeaderStyle = 3;
                    easNonLevyTotalProcuredAdult.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easNonLevyTotalProcuredAdult);
                }

                FundingSummaryModel totalNonLevyProcuredAdult = _totalBuilder.TotalRecords("Total Adult Non-Levy Contracted Apprenticeships (£)", nonLevyTotalProcuredAdult, easNonLevyTotalProcuredAdult); // EAS
                totalNonLevyProcuredAdult.ExcelHeaderStyle = 2;
                totalNonLevyProcuredAdult.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalNonLevyProcuredAdult);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                FundingSummaryModel totalNonLevyProcuredBudget = _totalBuilder.TotalRecords("Total Non-Levy Contracted Apprenticeships Budget - Procured delivery (£)", totalNonLevyNonProcured1618, totalNonLevyProcuredAdult);
                totalNonLevyProcuredBudget.ExcelHeaderStyle = 0;
                totalNonLevyProcuredBudget.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalNonLevyProcuredBudget);

                FundingSummaryModel totalNonLevyApprenticeProcuredCumulative = _totalBuilder.TotalRecordsCumulative("Total Non-Levy Contracted Apprenticeships Budget Cumulative - Procured delivery (£)", totalNonLevyProcuredBudget);
                totalNonLevyApprenticeProcuredCumulative.ExcelHeaderStyle = 0;
                totalNonLevyApprenticeProcuredCumulative.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalNonLevyApprenticeProcuredCumulative);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Adult Education Budget – Non-procured delivery", HeaderType.TitleOnly, 0));
                _fundingSummaryModels.Add(new FundingSummaryModel());

                _fundingSummaryModels.Add(new FundingSummaryModel("19-24 Traineeships", HeaderType.All, 2));
                FundingSummaryModel traineeship1924Funding = _fm35Builder.BuildWithFundLine("ILR 19-24 Traineeships Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "19-24 Traineeship", "19-24 Traineeship (non-procured)" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(traineeship1924Funding);
                FundingSummaryModel traineeship1924Support = _fm35Builder.BuildWithFundLine("ILR 19-24 Traineeships Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "19-24 Traineeship", "19-24 Traineeship (non-procured)" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(traineeship1924Support);
                FundingSummaryModel traineeship1924Total = _totalBuilder.TotalRecords("ILR Total 19-24 Traineeships (£)", traineeship1924Funding, traineeship1924Support);

                FundingSummaryModel easTraineeship1924Total = new FundingSummaryModel();
                if (!isFis)
                {
                    traineeship1924Total.ExcelHeaderStyle = 3;
                    traineeship1924Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(traineeship1924Total);

                    // EAS
                    FundingSummaryModel easTraineeship1924AuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 19-24 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924AuditAdjustments);

                    FundingSummaryModel easTraineeship1924AuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 19-24 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924AuthClaims);

                    FundingSummaryModel easTraineeship1924ExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 19-24 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924ExcessLearning);

                    FundingSummaryModel easTraineeship1924LearnerSupport = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Learner Support (£)", easSubmissionsValuesTask.Result, "Learner Support: 19-24 Traineeships", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924LearnerSupport);

                    easTraineeship1924Total = _totalBuilder.TotalRecords("EAS Total 19-24 Traineeships Earnings Adjustment (£)", easTraineeship1924AuditAdjustments, easTraineeship1924AuthClaims, easTraineeship1924ExcessLearning, easTraineeship1924LearnerSupport);
                    easTraineeship1924Total.ExcelHeaderStyle = 3;
                    easTraineeship1924Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTraineeship1924Total);
                }

                FundingSummaryModel totalTraineeship1924 = _totalBuilder.TotalRecords("Total 19-24 Traineeships (£)", traineeship1924Total, easTraineeship1924Total); // EAS
                totalTraineeship1924.ExcelHeaderStyle = 2;
                totalTraineeship1924.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalTraineeship1924);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("AEB - Other Learning", HeaderType.All, 2));
                FundingSummaryModel aebFunding = _fm35Builder.BuildWithFundLine("ILR AEB - Other Learning Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "AEB - Other Learning", "AEB - Other Learning (non-procured)" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(aebFunding);
                FundingSummaryModel aebSupport = _fm35Builder.BuildWithFundLine("ILR AEB - Other Learning Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "AEB - Other Learning", "AEB - Other Learning (non-procured)" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(aebSupport);
                FundingSummaryModel aebTotal = _totalBuilder.TotalRecords("ILR Total AEB - Other Learning (£)", aebFunding, aebSupport);

                FundingSummaryModel easAebTotal = new FundingSummaryModel();
                if (!isFis)
                {
                    aebTotal.ExcelHeaderStyle = 3;
                    aebTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(aebTotal);

                    // EAS
                    FundingSummaryModel easAebOtherAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS AEB - Other Learning Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: AEB-Other Learning", returnPeriod);
                    _fundingSummaryModels.Add(easAebOtherAuditAdjustments);

                    FundingSummaryModel easAebOtherAuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS AEB - Other Learning Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: AEB-Other Learning", returnPeriod);
                    _fundingSummaryModels.Add(easAebOtherAuthClaims);

                    FundingSummaryModel easAebOtherExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS AEB - Other Learning Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: AEB-Other Learning", returnPeriod);
                    _fundingSummaryModels.Add(easAebOtherExcessLearning);

                    easAebTotal = _totalBuilder.TotalRecords("EAS Total AEB - Other Learning Earnings Adjustment (£)", easAebOtherAuditAdjustments, easAebOtherAuthClaims, easAebOtherExcessLearning);
                    easAebTotal.ExcelHeaderStyle = 3;
                    easAebTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easAebTotal);
                }

                FundingSummaryModel totalAeb = _totalBuilder.TotalRecords("Total AEB - Other Learning (£)", aebTotal, easAebTotal); // EAS
                totalAeb.ExcelHeaderStyle = 2;
                totalAeb.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalAeb);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                FundingSummaryModel totalNonProcuredAdult = _totalBuilder.TotalRecords("Total Adult Education Budget – Non-procured delivery (£)", traineeship1924Total, totalAeb);
                totalNonProcuredAdult.ExcelHeaderStyle = 0;
                totalNonProcuredAdult.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalNonProcuredAdult);
                FundingSummaryModel totalNonProcuredAdultCumulative = _totalBuilder.TotalRecordsCumulative("Total Adult Education Budget Cumulative – Non-procured delivery (£)", totalNonProcuredAdult);
                totalNonProcuredAdultCumulative.ExcelHeaderStyle = 0;
                totalNonProcuredAdultCumulative.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalNonProcuredAdultCumulative);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Adult Education Budget – Procured delivery from 1 Nov 2017", HeaderType.TitleOnly, 0));

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("19-24 Traineeships", HeaderType.All, 2));
                FundingSummaryModel traineeship1924FundingProcured = _fm35Builder.BuildWithFundLine("ILR 19-24 Traineeships Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "19-24 Traineeship (procured from Nov 2017)" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(traineeship1924FundingProcured);
                FundingSummaryModel traineeship1924SupportProcured = _fm35Builder.BuildWithFundLine("ILR 19-24 Traineeships Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "19-24 Traineeship (procured from Nov 2017)" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(traineeship1924SupportProcured);
                FundingSummaryModel traineeship1924ProcuredTotal = _totalBuilder.TotalRecords("ILR Total 19-24 Traineeships (£)", traineeship1924FundingProcured, traineeship1924SupportProcured);

                FundingSummaryModel easTraineeship1924Nov2017Total = new FundingSummaryModel();
                if (!isFis)
                {
                    traineeship1924ProcuredTotal.ExcelHeaderStyle = 3;
                    traineeship1924ProcuredTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(traineeship1924ProcuredTotal);

                    // EAS
                    FundingSummaryModel easTraineeship1924Nov2017AuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: 19-24 Traineeships (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924Nov2017AuditAdjustments);

                    FundingSummaryModel easTraineeship1924Nov2017AuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: 19-24 Traineeships (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924Nov2017AuthClaims);

                    FundingSummaryModel easTraineeship1924Nov2017ExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: 19-24 Traineeships (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924Nov2017ExcessLearning);

                    FundingSummaryModel easTraineeship1924Nov2017LearnerSupport = _easBuilder.BuildWithEasSubValueLine("EAS 19-24 Traineeships Learner Support (£)", easSubmissionsValuesTask.Result, "Learner Support: 19-24 Traineeships (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easTraineeship1924Nov2017LearnerSupport);

                    easTraineeship1924Nov2017Total = _totalBuilder.TotalRecords("EAS Total 19-24 Traineeships Earnings Adjustment (£)", easTraineeship1924Nov2017AuditAdjustments, easTraineeship1924Nov2017AuthClaims, easTraineeship1924Nov2017ExcessLearning, easTraineeship1924Nov2017LearnerSupport);
                    easTraineeship1924Nov2017Total.ExcelHeaderStyle = 3;
                    easTraineeship1924Nov2017Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easTraineeship1924Nov2017Total);
                }

                FundingSummaryModel totalTraineeship1924Procured = _totalBuilder.TotalRecords("Total 19-24 Traineeships (£)", traineeship1924ProcuredTotal, easTraineeship1924Nov2017Total); // EAS
                totalTraineeship1924Procured.ExcelHeaderStyle = 2;
                totalTraineeship1924Procured.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalTraineeship1924Procured);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("AEB - Other Learning", HeaderType.All, 2));
                FundingSummaryModel aebFundingProcured = _fm35Builder.BuildWithFundLine("ILR AEB - Other Learning Programme Funding (£)", fm35Task.Result, validLearnersTask.Result, new[] { "AEB - Other Learning (procured from Nov 2017)" }, new[] { Constants.Fm35OnProgrammeAttributeName, Constants.Fm35AimAchievementAttributeName, Constants.Fm35JobOutcomeAchievementAttributeName, Constants.Fm35BalancingAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(aebFundingProcured);
                FundingSummaryModel aebSupportProcured = _fm35Builder.BuildWithFundLine("ILR AEB - Other Learning Learning Support (£)", fm35Task.Result, validLearnersTask.Result, new[] { "AEB - Other Learning (procured from Nov 2017)" }, new[] { Constants.Fm35LearningSupportAttributeName }, returnPeriod);
                _fundingSummaryModels.Add(aebSupportProcured);
                FundingSummaryModel aebProcuredTotal = _totalBuilder.TotalRecords("ILR Total AEB - Other Learning (£)", aebFundingProcured, aebSupportProcured);

                FundingSummaryModel easAebNov2017Total = new FundingSummaryModel();
                if (!isFis)
                {
                    aebProcuredTotal.ExcelHeaderStyle = 3;
                    aebProcuredTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(aebProcuredTotal);

                    // EAS
                    FundingSummaryModel easAebNov2017AuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS AEB - Other Learning Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: AEB-Other Learning (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easAebNov2017AuditAdjustments);

                    FundingSummaryModel easAebNov2017AuthClaims = _easBuilder.BuildWithEasSubValueLine("EAS AEB - Other Learning Authorised Claims (£)", easSubmissionsValuesTask.Result, "Authorised Claims: AEB-Other Learning (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easAebNov2017AuthClaims);

                    FundingSummaryModel easAebNov2017ExcessLearning = _easBuilder.BuildWithEasSubValueLine("EAS AEB - Other Learning Excess Learning Support (£)", easSubmissionsValuesTask.Result, "Excess Learning Support: AEB-Other Learning (From Nov 2017)", returnPeriod);
                    _fundingSummaryModels.Add(easAebNov2017ExcessLearning);

                    easAebNov2017Total = _totalBuilder.TotalRecords("EAS Total AEB - Other Learning Earnings Adjustment (£)", easAebNov2017AuditAdjustments, easAebNov2017AuthClaims, easAebNov2017ExcessLearning);
                    easAebNov2017Total.ExcelHeaderStyle = 3;
                    easAebNov2017Total.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easAebNov2017Total);
                }

                FundingSummaryModel totalAebProcured = _totalBuilder.TotalRecords("Total AEB - Other Learning (£)", aebProcuredTotal, easAebNov2017Total); // EAS
                totalAebProcured.ExcelHeaderStyle = 2;
                totalAebProcured.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalAebProcured);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                FundingSummaryModel totalProcuredAdult = _totalBuilder.TotalRecords("Total Adult Education Budget – Procured delivery from 1 Nov 2017 (£)", traineeship1924Total, totalAebProcured); // EAS
                totalProcuredAdult.ExcelHeaderStyle = 0;
                totalProcuredAdult.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalProcuredAdult);
                FundingSummaryModel totalProcuredAdultCumulative = _totalBuilder.TotalRecordsCumulative("Total Adult Education Budget Cumulative – Procured delivery from 1 Nov 2017 (£)", totalProcuredAdult);
                totalProcuredAdultCumulative.ExcelHeaderStyle = 0;
                totalProcuredAdultCumulative.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalProcuredAdultCumulative);

                _fundingSummaryModels.Add(new FundingSummaryModel());
                _fundingSummaryModels.Add(new FundingSummaryModel("Advanced Loans Bursary Budget", HeaderType.TitleOnly, 0));
                _fundingSummaryModels.Add(new FundingSummaryModel("Advanced Loans Bursary", HeaderType.All, 2));
                List<FundingSummaryModel> albModels = await _allbBuilder.BuildAsync(reportServiceContext, cancellationToken);
                _fundingSummaryModels.AddRange(albModels);
                FundingSummaryModel albIlrTotal = _totalBuilder.TotalRecords("ILR Total Advanced Loans Bursary (£)", albModels[0], albModels[1]);

                FundingSummaryModel easAdvLoansBurseryTotal = new FundingSummaryModel();
                if (!isFis)
                {
                    albIlrTotal.ExcelHeaderStyle = 3;
                    albIlrTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(albIlrTotal);

                    FundingSummaryModel easAdvLoansBurseryExcessSupport = _easBuilder.BuildWithEasSubValueLine("EAS Advanced Loans Bursary Excess Support (£)", easSubmissionsValuesTask.Result, "Excess Support: Advanced Learner Loans Bursary", returnPeriod);
                    _fundingSummaryModels.Add(easAdvLoansBurseryExcessSupport);

                    FundingSummaryModel easAdvLoansBurseryAuditAdjustments = _easBuilder.BuildWithEasSubValueLine("EAS Advanced Loans Bursary Area Costs Audit Adjustments (£)", easSubmissionsValuesTask.Result, "Audit Adjustments: Advanced Learner Loans Bursary", returnPeriod);
                    _fundingSummaryModels.Add(easAdvLoansBurseryAuditAdjustments);

                    easAdvLoansBurseryTotal = _totalBuilder.TotalRecords("EAS Total Advanced Loans Bursary Earnings Adjustment (£)", easAdvLoansBurseryExcessSupport, easAdvLoansBurseryAuditAdjustments);
                    easAdvLoansBurseryTotal.ExcelHeaderStyle = 3;
                    easAdvLoansBurseryTotal.ExcelRecordStyle = 3;
                    _fundingSummaryModels.Add(easAdvLoansBurseryTotal);
                }

                FundingSummaryModel totalAlb = _totalBuilder.TotalRecords("Total Advanced Loans Bursary (£)", albIlrTotal, easAdvLoansBurseryTotal); // EAS
                totalAlb.ExcelHeaderStyle = 2;
                totalAlb.ExcelRecordStyle = 2;
                _fundingSummaryModels.Add(totalAlb);

                FundingSummaryModel totalAlbBudget = (FundingSummaryModel)totalAlb.Clone();
                totalAlbBudget.Title = "Total Advanced Loans Bursary Budget (£)";
                totalAlbBudget.ExcelHeaderStyle = 0;
                totalAlbBudget.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalAlbBudget);

                FundingSummaryModel totalAlbBudgetCumulative = _totalBuilder.TotalRecordsCumulative("Total Advanced Loans Bursary Budget Cumulative (£)", totalAlbBudget);
                totalAlbBudgetCumulative.ExcelHeaderStyle = 0;
                totalAlbBudgetCumulative.ExcelRecordStyle = 0;
                _fundingSummaryModels.Add(totalAlbBudgetCumulative);

                _fundingSummaryModels.Add(new FundingSummaryModel(Constants.ALBInfoText, HeaderType.TitleOnly));
                _fundingSummaryModels.Add(new FundingSummaryModel());

                var jobId = reportServiceContext.JobId;
                var ukPrn = reportServiceContext.Ukprn.ToString();
                var externalFileName = GetExternalFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);
                var fileName = GetFilename(ukPrn, jobId, reportServiceContext.SubmissionDateTimeUtc);

                _logger.LogInfo("CSV Report Start");

                string csv = GetReportCsv(fundingSummaryHeaderModel, fundingSummaryFooterModel);
                await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.csv", csv, cancellationToken);

                _logger.LogInfo("CSV Report End");

                _logger.LogInfo("Excel Report Start");

                Workbook workbook = GetWorkbookReport(fundingSummaryHeaderModel, fundingSummaryFooterModel);

                using (MemoryStream ms = new MemoryStream())
                {
                    workbook.Worksheets[0].Name = "Funding Summary Report";

                    ApplyEuBranding(workbook);

                    workbook.Save(ms, SaveFormat.Xlsx);
                    await _streamableKeyValuePersistenceService.SaveAsync($"{externalFileName}.xlsx", ms, cancellationToken);
                    await WriteZipEntry(archive, $"{fileName}.xlsx", ms, cancellationToken);
                }

                _logger.LogInfo("Excel Report End");
            }
            catch (Exception e)
            {
                _logger.LogError("Funding Summary Report Fail", e);
                throw;
            }
        }

        private Workbook ApplyEuBranding(Workbook workbook)
        {
            _logger.LogInfo("Apply EU Branding Start");

            var assembly = Assembly.GetExecutingAssembly();
            string euFlagBmp = assembly.GetManifestResourceNames().Single(str => str.EndsWith("EuFlag.bmp"));
            Stream manifestResourceStream = assembly.GetManifestResourceStream(euFlagBmp);
            workbook.Worksheets[0].Pictures.Add(1, 22, manifestResourceStream);

            _logger.LogInfo("Apply EU Branding End");

            return workbook;
        }

        private string GetReportCsv(FundingSummaryHeaderModel fundingSummaryHeaderModel, FundingSummaryFooterModel fundingSummaryFooterModel)
        {
            _logger.LogInfo("CSV Report Content Creating");

            using (MemoryStream ms = new MemoryStream())
            {
                UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
                using (TextWriter textWriter = new StreamWriter(ms, utF8Encoding))
                {
                    using (CsvWriter csvWriter = new CsvWriter(textWriter))
                    {
                        WriteCsvRecords<FundingSummaryHeaderMapper, FundingSummaryHeaderModel>(csvWriter, fundingSummaryHeaderModel);
                        foreach (FundingSummaryModel fundingSummaryModel in _fundingSummaryModels)
                        {
                            if (string.IsNullOrEmpty(fundingSummaryModel.Title))
                            {
                                WriteCsvRecords(csvWriter);
                                continue;
                            }

                            if (fundingSummaryModel.HeaderType == HeaderType.TitleOnly)
                            {
                                WriteCsvRecords(csvWriter, (object)fundingSummaryModel.Title);
                                continue;
                            }

                            if (fundingSummaryModel.HeaderType == HeaderType.All)
                            {
                                _fundingSummaryMapper.MemberMaps.Single(x => x.Data.Index == 0).Name(fundingSummaryModel.Title);
                                WriteCsvRecords(csvWriter, _fundingSummaryMapper);
                                continue;
                            }

                            WriteCsvRecords(csvWriter, _fundingSummaryMapper, _cachedModelProperties, fundingSummaryModel);
                        }

                        WriteCsvRecords<FundingSummaryFooterMapper, FundingSummaryFooterModel>(csvWriter, fundingSummaryFooterModel);

                        csvWriter.Flush();
                        textWriter.Flush();

                        var csvOutput = Encoding.UTF8.GetString(ms.ToArray());

                        _logger.LogInfo("CSV Report Content Created");

                        return csvOutput;
                    }
                }
            }
        }

        private Workbook GetWorkbookReport(
            FundingSummaryHeaderModel fundingSummaryHeaderModel,
            FundingSummaryFooterModel fundingSummaryFooterModel)
        {
            _logger.LogInfo("Excel Work Book Content Start");

            Workbook workbook = new Workbook();
            CellStyle[] cellStyles = _excelStyleProvider.GetFundingSummaryStyles(workbook);
            Worksheet sheet = workbook.Worksheets[0];

            sheet.Cells.StandardWidth = 19;
            sheet.Cells.Columns[0].Width = 63.93;

            WriteExcelRecords(sheet, new FundingSummaryHeaderMapper(), new List<FundingSummaryHeaderModel> { fundingSummaryHeaderModel }, cellStyles[5], cellStyles[5], true);
            foreach (FundingSummaryModel fundingSummaryModel in _fundingSummaryModels)
            {
                if (string.IsNullOrEmpty(fundingSummaryModel.Title))
                {
                    WriteExcelRecords(sheet);
                    continue;
                }

                CellStyle excelHeaderStyle = _excelStyleProvider.GetCellStyle(cellStyles, fundingSummaryModel.ExcelHeaderStyle);

                if (fundingSummaryModel.HeaderType == HeaderType.TitleOnly)
                {
                    WriteExcelRecords(sheet, fundingSummaryModel.Title, excelHeaderStyle, 17);
                    continue;
                }

                if (fundingSummaryModel.HeaderType == HeaderType.All)
                {
                    _fundingSummaryMapper.MemberMaps.Single(x => x.Data.Index == 0).Name(fundingSummaryModel.Title);
                    WriteExcelRecords(sheet, _fundingSummaryMapper, excelHeaderStyle);
                    continue;
                }

                CellStyle excelRecordStyle = _excelStyleProvider.GetCellStyle(cellStyles, fundingSummaryModel.ExcelRecordStyle);

                WriteExcelRecords(sheet, _fundingSummaryMapper, _cachedModelProperties, fundingSummaryModel, excelRecordStyle);
            }

            WriteExcelRecords(sheet, new FundingSummaryFooterMapper(), new List<FundingSummaryFooterModel> { fundingSummaryFooterModel }, cellStyles[5], cellStyles[5], true);

            _logger.LogInfo("Excell Workbook Content Finished");

            return workbook;
        }

        private async Task<FundingSummaryHeaderModel> GetHeader(IReportServiceContext reportServiceContext, Task<IMessage> messageTask, Task<ILRSourceFileInfo> lastSubmittedIlrFileTask, Task<string> providerNameTask, CancellationToken cancellationToken, bool isFis)
        {
            var fileName = reportServiceContext.OriginalFilename ?? reportServiceContext.Filename;
            var fundingSummaryHeaderModel = new FundingSummaryHeaderModel
            {
                IlrFile = string.Equals(reportServiceContext.CollectionName, "ILR1819", StringComparison.OrdinalIgnoreCase) ? fileName : "N/A",
                Ukprn = reportServiceContext.Ukprn,
                ProviderName = providerNameTask.Result ?? "Unknown",
                LastEasUpdate = !isFis ? (await _easProviderService.GetLastEasUpdate(reportServiceContext.Ukprn, cancellationToken)).ToString("dd/MM/yyyy") : null,
                SecurityClassification = "OFFICIAL - SENSITIVE"
            };

            if (messageTask.Result != null)
            {
                fundingSummaryHeaderModel.LastIlrFileUpdate =
                    messageTask.Result.HeaderEntity.SourceEntity.DateTime.ToString("dd/MM/yyyy");
            }
            else
            {
                fundingSummaryHeaderModel.LastIlrFileUpdate = lastSubmittedIlrFileTask.Result != null
                    ? lastSubmittedIlrFileTask.Result.SubmittedTime.GetValueOrDefault().ToString("dd/MM/yyyy")
                    : string.Empty;
            }

            return fundingSummaryHeaderModel;
        }

        private async Task<FundingSummaryFooterModel> GetFooterAsync(Task<IMessage> messageTask, Task<ILRSourceFileInfo> lastSubmittedIlrFileTask, CancellationToken cancellationToken)
        {
            var dateTimeNowUtc = _dateTimeProvider.GetNowUtc();
            var dateTimeNowUk = _dateTimeProvider.ConvertUtcToUk(dateTimeNowUtc);
            var fundingSummaryFooterModel = new FundingSummaryFooterModel
            {
                ReportGeneratedAt = "Report generated at " + dateTimeNowUk.ToString("HH:mm:ss") + " on " + dateTimeNowUk.ToString("dd/MM/yyyy"),
                ApplicationVersion = _versionInfo.ServiceReleaseVersion,
                ComponentSetVersion = "NA",
                OrganisationData = await _orgProviderService.GetVersionAsync(cancellationToken),
                LargeEmployerData = await _largeEmployerProviderService.GetVersionAsync(cancellationToken),
                LarsData = await _larsProviderService.GetVersionAsync(cancellationToken),
                PostcodeData = await _postcodeProviderService.GetVersionAsync(cancellationToken)
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