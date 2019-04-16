using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public sealed class AllbBuilder : IAllbBuilder
    {
        private const string AlbSupportPayment = "ALBSupportPayment";
        private const string AlbAreaUpliftBalPayment = "AreaUpliftBalPayment";
        private const string AlbAreaUpliftOnProgPayment = "AreaUpliftOnProgPayment";

        private readonly IIlrProviderService _ilrProviderService;
        private readonly IValidLearnersService _validLearnersService;
        private readonly IAllbProviderService _allbProviderService;
        private readonly IStringUtilitiesService _stringUtilitiesService;
        private readonly ILogger _logger;

        public AllbBuilder(
            IIlrProviderService ilrProviderService,
            IValidLearnersService validLearnersService,
            IAllbProviderService allbProviderService,
            IStringUtilitiesService stringUtilitiesService,
            ILogger logger)
        {
            _ilrProviderService = ilrProviderService;
            _validLearnersService = validLearnersService;
            _allbProviderService = allbProviderService;
            _stringUtilitiesService = stringUtilitiesService;
            _logger = logger;
        }

        public async Task<List<FundingSummaryModel>> BuildAsync(IReportServiceContext reportServiceContext, CancellationToken cancellationToken)
        {
            FundingSummaryModel fundingSummaryModelAlbFunding = new FundingSummaryModel()
            {
                Title = "ILR Advanced Loans Bursary Funding (£)"
            };

            FundingSummaryModel fundingSummaryModelAlbAreaCosts = new FundingSummaryModel()
            {
                Title = "ILR Advanced Loans Bursary Area Costs (£)"
            };

            List<FundingSummaryModel> fundingSummaryModels = new List<FundingSummaryModel>()
            {
                fundingSummaryModelAlbFunding,
                fundingSummaryModelAlbAreaCosts
            };

            Task<IMessage> ilrFile = _ilrProviderService.GetIlrFile(reportServiceContext, cancellationToken);
            Task<List<string>> validLearners = _validLearnersService.GetLearnersAsync(reportServiceContext, cancellationToken);
            Task<ALBGlobal> albData = _allbProviderService.GetAllbData(reportServiceContext, cancellationToken);

            await Task.WhenAll(ilrFile, validLearners, albData);

            List<string> ilrError = new List<string>();
            List<string> albLearnerError = new List<string>();

            try
            {
                ILearner[] learners = ilrFile.Result?.Learners?.Where(x => validLearners.Result.Contains(x.LearnRefNumber)).ToArray();

                foreach (ILearner learner in learners ?? Enumerable.Empty<ILearner>())
                {
                    ALBLearner albLearner = albData.Result?.Learners?.SingleOrDefault(x => string.Equals(x.LearnRefNumber, learner.LearnRefNumber, StringComparison.OrdinalIgnoreCase));
                    if (albLearner == null)
                    {
                        albLearnerError.Add(learner.LearnRefNumber);
                        continue;
                    }

                    foreach (ILearningDelivery learningDelivery in learner.LearningDeliveries ?? Enumerable.Empty<ILearningDelivery>())
                    {
                        LearningDelivery albLearningDeliveryAreaCosts = albLearner.LearningDeliveries?.Where(x => x.LearningDeliveryValue.FundLine == "Advanced Learner Loans Bursary").SingleOrDefault(x => x.AimSeqNumber == learningDelivery.AimSeqNumber);

                        if (learningDelivery.LearningDeliveryFAMs != null && learningDelivery.LearningDeliveryFAMs.Any(x =>
                                !(string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeLDM, StringComparison.OrdinalIgnoreCase) && string.Equals(x.LearnDelFAMCode, "359", StringComparison.OrdinalIgnoreCase))))
                        {
                            TotalAlbFunding(albLearningDeliveryAreaCosts?.LearningDeliveryPeriodisedValues, reportServiceContext.ReturnPeriod, AlbSupportPayment, fundingSummaryModelAlbFunding);
                        }

                        TotalAlbAreaCosts(albLearningDeliveryAreaCosts?.LearningDeliveryPeriodisedValues, reportServiceContext.ReturnPeriod, fundingSummaryModelAlbAreaCosts);
                    }
                }

                if (ilrError.Any())
                {
                    _logger.LogWarning($"Failed to get one or more ILR learners while {nameof(AllbBuilder)}.{nameof(BuildAsync)}: {_stringUtilitiesService.JoinWithMaxLength(ilrError)}");
                }

                if (albLearnerError.Any())
                {
                    _logger.LogWarning($"Failed to get one or more ALB learners while {nameof(AllbBuilder)}.{nameof(BuildAsync)}: {_stringUtilitiesService.JoinWithMaxLength(albLearnerError)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("AlbBuilder BuildAsync failed with Exception: ", ex);
            }

            return fundingSummaryModels;
        }

        private void TotalAlbFunding(
            List<LearningDeliveryPeriodisedValue> periodisedValues,
            int period,
            string attributeName,
            FundingSummaryModel fundingSummaryModelAlbFunding)
        {
            if (periodisedValues == null)
            {
                return;
            }

            LearningDeliveryPeriodisedValue[] periodisedValue = periodisedValues.Where(x => string.Equals(x.AttributeName, attributeName, StringComparison.OrdinalIgnoreCase)).ToArray();

            if (periodisedValue.Length == 0)
            {
                return;
            }

            fundingSummaryModelAlbFunding.Period1 += periodisedValue.Sum(x => x.Period1 ?? 0);
            fundingSummaryModelAlbFunding.Period2 += periodisedValue.Sum(x => x.Period2 ?? 0);
            fundingSummaryModelAlbFunding.Period3 += periodisedValue.Sum(x => x.Period3 ?? 0);
            fundingSummaryModelAlbFunding.Period4 += periodisedValue.Sum(x => x.Period4 ?? 0);
            fundingSummaryModelAlbFunding.Period5 += periodisedValue.Sum(x => x.Period5 ?? 0);
            fundingSummaryModelAlbFunding.Period6 += periodisedValue.Sum(x => x.Period6 ?? 0);
            fundingSummaryModelAlbFunding.Period7 += periodisedValue.Sum(x => x.Period7 ?? 0);
            fundingSummaryModelAlbFunding.Period8 += periodisedValue.Sum(x => x.Period8 ?? 0);
            fundingSummaryModelAlbFunding.Period9 += periodisedValue.Sum(x => x.Period9 ?? 0);
            fundingSummaryModelAlbFunding.Period10 += periodisedValue.Sum(x => x.Period10 ?? 0);
            fundingSummaryModelAlbFunding.Period11 += periodisedValue.Sum(x => x.Period11 ?? 0);
            fundingSummaryModelAlbFunding.Period12 += periodisedValue.Sum(x => x.Period12 ?? 0);
            fundingSummaryModelAlbFunding.Period1_8 =
                fundingSummaryModelAlbFunding.Period1 + fundingSummaryModelAlbFunding.Period2 + fundingSummaryModelAlbFunding.Period3 +
                fundingSummaryModelAlbFunding.Period4 + fundingSummaryModelAlbFunding.Period5 + fundingSummaryModelAlbFunding.Period6 +
                fundingSummaryModelAlbFunding.Period7 + fundingSummaryModelAlbFunding.Period8;
            fundingSummaryModelAlbFunding.Period9_12 =
                fundingSummaryModelAlbFunding.Period9 + fundingSummaryModelAlbFunding.Period10 + fundingSummaryModelAlbFunding.Period11 +
                fundingSummaryModelAlbFunding.Period12;

            fundingSummaryModelAlbFunding.YearToDate = GetYearToDateTotal(fundingSummaryModelAlbFunding, period - 1);
            fundingSummaryModelAlbFunding.Total = GetYearToDateTotal(fundingSummaryModelAlbFunding, 12);
        }

        private void TotalAlbAreaCosts(
            List<LearningDeliveryPeriodisedValue> periodisedValues,
            int period,
            FundingSummaryModel fundingSummaryModelAlbAreaCosts)
        {
            if (periodisedValues == null)
            {
                return;
            }

            var progPaymentPeriodisedValue = periodisedValues.Where(x => string.Equals(x.AttributeName, AlbAreaUpliftOnProgPayment, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (progPaymentPeriodisedValue.Length > 0)
            {
                fundingSummaryModelAlbAreaCosts.Period1 += progPaymentPeriodisedValue.Sum(x => x.Period1 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period2 += progPaymentPeriodisedValue.Sum(x => x.Period2 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period3 += progPaymentPeriodisedValue.Sum(x => x.Period3 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period4 += progPaymentPeriodisedValue.Sum(x => x.Period4 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period5 += progPaymentPeriodisedValue.Sum(x => x.Period5 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period6 += progPaymentPeriodisedValue.Sum(x => x.Period6 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period7 += progPaymentPeriodisedValue.Sum(x => x.Period7 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period8 += progPaymentPeriodisedValue.Sum(x => x.Period8 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period9 += progPaymentPeriodisedValue.Sum(x => x.Period9 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period10 += progPaymentPeriodisedValue.Sum(x => x.Period10 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period11 += progPaymentPeriodisedValue.Sum(x => x.Period11 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period12 += progPaymentPeriodisedValue.Sum(x => x.Period12 ?? 0);
            }

            var areaUpliftPeriodisedValue = periodisedValues.Where(x => string.Equals(x.AttributeName, AlbAreaUpliftBalPayment, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (areaUpliftPeriodisedValue.Length > 0)
            {
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period1 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period2 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period3 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period4 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period5 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period6 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period7 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period8 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period9 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period10 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period11 ?? 0);
                fundingSummaryModelAlbAreaCosts.Period1 += areaUpliftPeriodisedValue.Sum(x => x.Period12 ?? 0);
            }

            fundingSummaryModelAlbAreaCosts.Period1_8 =
                fundingSummaryModelAlbAreaCosts.Period1 + fundingSummaryModelAlbAreaCosts.Period2 + fundingSummaryModelAlbAreaCosts.Period3 +
                fundingSummaryModelAlbAreaCosts.Period4 + fundingSummaryModelAlbAreaCosts.Period5 + fundingSummaryModelAlbAreaCosts.Period6 +
                fundingSummaryModelAlbAreaCosts.Period7 + fundingSummaryModelAlbAreaCosts.Period8;
            fundingSummaryModelAlbAreaCosts.Period9_12 =
                fundingSummaryModelAlbAreaCosts.Period9 + fundingSummaryModelAlbAreaCosts.Period10 + fundingSummaryModelAlbAreaCosts.Period11 +
                fundingSummaryModelAlbAreaCosts.Period12;

            fundingSummaryModelAlbAreaCosts.YearToDate = GetYearToDateTotal(fundingSummaryModelAlbAreaCosts, period);
            fundingSummaryModelAlbAreaCosts.Total = GetYearToDateTotal(fundingSummaryModelAlbAreaCosts, 12);
        }

        //private void TotalAlb(
        //    ALBLearner albLearner,
        //    FundingSummaryModel fundingSummaryModelAlbFunding,
        //    int period,
        //    FundingSummaryModel fundingSummaryModelAlbAreaCosts)
        //{
        //    var learningDeliveries = albLearner.LearningDeliveries.Where(x =>
        //        x.LearningDeliveryValue.FundLine == "Advanced Learner Loans Bursary");

        //    //var albSupportPaymentObj = learningDeliveries.SelectMany(x => x.LearningDeliveryPeriodisedValues).SingleOrDefault(x => string.Equals(x.AttributeName, AlbSupportPayment, StringComparison.OrdinalIgnoreCase));
        //    var albSupportPaymentLearningDeliveries = new List<LearningDelivery>();
        //    foreach (ILearningDelivery learningDelivery in albLearner.LearningDeliveries)
        //    {
        //        if (learningDelivery.LearningDeliveryFAMs.Count(x => !x.LearnDelFAMType.Contains("LDM") && !x.LearnDelFAMCode.Contains("359")) > 0)
        //        {
        //            albSupportPaymentLearningDeliveries.Add(new LearningDelivery()
        //            {
        //                LearningDeliveryPeriodisedValues = albLearner.LearningDeliveries.Where(x => x.AimSeqNumber == learningDelivery.AimSeqNumber).SelectMany(x => x.LearningDeliveryPeriodisedValues).ToList()
        //            });
        //        }
        //    }

        //    var albSupportPaymentObj = albSupportPaymentLearningDeliveries.SelectMany(x => x.LearningDeliveryPeriodisedValues).SingleOrDefault(x => string.Equals(x.AttributeName, AlbSupportPayment, StringComparison.OrdinalIgnoreCase));

        //    var albAreaUpliftOnProgPaymentObj = learningDeliveries.SelectMany(x => x.LearningDeliveryPeriodisedValues).SingleOrDefault(x => string.Equals(x.AttributeName, AlbAreaUpliftOnProgPayment, StringComparison.OrdinalIgnoreCase));

        //    var albAreaUpliftBalPaymentObj = learningDeliveries.SelectMany(x => x.LearningDeliveryPeriodisedValues).SingleOrDefault(x => string.Equals(x.AttributeName, AlbAreaUpliftBalPayment, StringComparison.OrdinalIgnoreCase));

        //    fundingSummaryModelAlbFunding.Period1 =
        //        fundingSummaryModelAlbFunding.Period1 + albSupportPaymentObj?.Period1 ?? 0;
        //    fundingSummaryModelAlbFunding.Period2 =
        //        fundingSummaryModelAlbFunding.Period2 + albSupportPaymentObj?.Period2 ?? 0;
        //    fundingSummaryModelAlbFunding.Period3 =
        //        fundingSummaryModelAlbFunding.Period3 + albSupportPaymentObj?.Period3 ?? 0;
        //    fundingSummaryModelAlbFunding.Period4 =
        //        fundingSummaryModelAlbFunding.Period1 + albSupportPaymentObj?.Period4 ?? 0;
        //    fundingSummaryModelAlbFunding.Period5 =
        //        fundingSummaryModelAlbFunding.Period5 + albSupportPaymentObj?.Period5 ?? 0;
        //    fundingSummaryModelAlbFunding.Period6 =
        //        fundingSummaryModelAlbFunding.Period6 + albSupportPaymentObj?.Period6 ?? 0;
        //    fundingSummaryModelAlbFunding.Period7 =
        //        fundingSummaryModelAlbFunding.Period7 + albSupportPaymentObj?.Period7 ?? 0;
        //    fundingSummaryModelAlbFunding.Period8 =
        //        fundingSummaryModelAlbFunding.Period8 + albSupportPaymentObj?.Period8 ?? 0;
        //    fundingSummaryModelAlbFunding.Period9 =
        //        fundingSummaryModelAlbFunding.Period9 + albSupportPaymentObj?.Period9 ?? 0;
        //    fundingSummaryModelAlbFunding.Period10 =
        //        fundingSummaryModelAlbFunding.Period10 + albSupportPaymentObj?.Period10 ?? 0;
        //    fundingSummaryModelAlbFunding.Period11 =
        //        fundingSummaryModelAlbFunding.Period11 + albSupportPaymentObj?.Period11 ?? 0;
        //    fundingSummaryModelAlbFunding.Period12 =
        //        fundingSummaryModelAlbFunding.Period12 + albSupportPaymentObj?.Period12 ?? 0;
        //    fundingSummaryModelAlbFunding.Period1_8 =
        //        _totalBuilder.TotalRecords(
        //            fundingSummaryModelAlbFunding.Period1_8,
        //            fundingSummaryModelAlbFunding.Period1,
        //            fundingSummaryModelAlbFunding.Period2,
        //            fundingSummaryModelAlbFunding.Period3,
        //            fundingSummaryModelAlbFunding.Period4,
        //            fundingSummaryModelAlbFunding.Period5,
        //            fundingSummaryModelAlbFunding.Period6,
        //            fundingSummaryModelAlbFunding.Period7,
        //            fundingSummaryModelAlbFunding.Period8);
        //    fundingSummaryModelAlbFunding.Period9_12 =
        //        _totalBuilder.TotalRecords(
        //            fundingSummaryModelAlbFunding.Period9_12,
        //            fundingSummaryModelAlbFunding.Period9,
        //            fundingSummaryModelAlbFunding.Period10,
        //            fundingSummaryModelAlbFunding.Period11,
        //            fundingSummaryModelAlbFunding.Period12);
        //    fundingSummaryModelAlbFunding.YearToDate = fundingSummaryModelAlbFunding.YearToDate +
        //                                               GetYearToDateTotal(albSupportPaymentObj, period);
        //    fundingSummaryModelAlbFunding.Total = _totalBuilder.TotalRecords(
        //        fundingSummaryModelAlbFunding.Total,
        //        fundingSummaryModelAlbFunding.Period1_8,
        //        fundingSummaryModelAlbFunding.Period9_12);

        //    fundingSummaryModelAlbAreaCosts.Period1 = fundingSummaryModelAlbAreaCosts.Period1 +
        //                                              (albAreaUpliftBalPaymentObj?.Period1 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period1 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period2 = fundingSummaryModelAlbAreaCosts.Period2 +
        //                                              (albAreaUpliftBalPaymentObj?.Period2 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period2 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period3 = fundingSummaryModelAlbAreaCosts.Period3 +
        //                                              (albAreaUpliftBalPaymentObj?.Period3 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period3 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period4 = fundingSummaryModelAlbAreaCosts.Period4 +
        //                                              (albAreaUpliftBalPaymentObj?.Period4 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period4 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period5 = fundingSummaryModelAlbAreaCosts.Period5 +
        //                                              (albAreaUpliftBalPaymentObj?.Period5 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period5 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period6 = fundingSummaryModelAlbAreaCosts.Period6 +
        //                                              (albAreaUpliftBalPaymentObj?.Period6 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period6 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period7 = fundingSummaryModelAlbAreaCosts.Period7 +
        //                                              (albAreaUpliftBalPaymentObj?.Period7 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period7 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period8 = fundingSummaryModelAlbAreaCosts.Period8 +
        //                                              (albAreaUpliftBalPaymentObj?.Period8 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period8 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period9 = fundingSummaryModelAlbAreaCosts.Period9 +
        //                                              (albAreaUpliftBalPaymentObj?.Period9 ?? 0) +
        //                                              (albAreaUpliftOnProgPaymentObj?.Period9 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period10 = fundingSummaryModelAlbAreaCosts.Period10 +
        //                                               (albAreaUpliftBalPaymentObj?.Period10 ?? 0) +
        //                                               (albAreaUpliftOnProgPaymentObj?.Period10 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period11 = fundingSummaryModelAlbAreaCosts.Period11 +
        //                                               (albAreaUpliftBalPaymentObj?.Period11 ?? 0) +
        //                                               (albAreaUpliftOnProgPaymentObj?.Period11 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period12 = fundingSummaryModelAlbAreaCosts.Period12 +
        //                                               (albAreaUpliftBalPaymentObj?.Period12 ?? 0) +
        //                                               (albAreaUpliftOnProgPaymentObj?.Period12 ?? 0);
        //    fundingSummaryModelAlbAreaCosts.Period1_8 =
        //        _totalBuilder.TotalRecords(
        //            fundingSummaryModelAlbAreaCosts.Period1_8,
        //            fundingSummaryModelAlbAreaCosts.Period1,
        //            fundingSummaryModelAlbAreaCosts.Period2,
        //            fundingSummaryModelAlbAreaCosts.Period3,
        //            fundingSummaryModelAlbAreaCosts.Period4,
        //            fundingSummaryModelAlbAreaCosts.Period5,
        //            fundingSummaryModelAlbAreaCosts.Period6,
        //            fundingSummaryModelAlbAreaCosts.Period7,
        //            fundingSummaryModelAlbAreaCosts.Period8);
        //    fundingSummaryModelAlbAreaCosts.Period9_12 =
        //        _totalBuilder.TotalRecords(
        //            fundingSummaryModelAlbAreaCosts.Period9_12,
        //            fundingSummaryModelAlbAreaCosts.Period9,
        //            fundingSummaryModelAlbAreaCosts.Period10,
        //            fundingSummaryModelAlbAreaCosts.Period11,
        //            fundingSummaryModelAlbAreaCosts.Period12);
        //    fundingSummaryModelAlbAreaCosts.YearToDate = fundingSummaryModelAlbAreaCosts.YearToDate +
        //                                                 GetYearToDateTotal(albAreaUpliftBalPaymentObj, period) +
        //                                                 GetYearToDateTotal(albAreaUpliftOnProgPaymentObj, period);
        //    fundingSummaryModelAlbAreaCosts.Total =
        //        _totalBuilder.TotalRecords(
        //            fundingSummaryModelAlbAreaCosts.Total,
        //            fundingSummaryModelAlbAreaCosts.Period1_8,
        //            fundingSummaryModelAlbAreaCosts.Period9_12);
        //}

        private decimal? GetYearToDateTotal(FundingSummaryModel fundingSummaryModel, int period)
        {
            decimal total = 0;
            for (int i = 0; i < period; i++)
            {
                switch (i)
                {
                    case 0:
                        total += fundingSummaryModel.Period1 ?? 0;
                        break;
                    case 1:
                        total += fundingSummaryModel.Period2 ?? 0;
                        break;
                    case 2:
                        total += fundingSummaryModel.Period3 ?? 0;
                        break;
                    case 3:
                        total += fundingSummaryModel.Period4 ?? 0;
                        break;
                    case 4:
                        total += fundingSummaryModel.Period5 ?? 0;
                        break;
                    case 5:
                        total += fundingSummaryModel.Period6 ?? 0;
                        break;
                    case 6:
                        total += fundingSummaryModel.Period7 ?? 0;
                        break;
                    case 7:
                        total += fundingSummaryModel.Period8 ?? 0;
                        break;
                    case 8:
                        total += fundingSummaryModel.Period9 ?? 0;
                        break;
                    case 9:
                        total += fundingSummaryModel.Period10 ?? 0;
                        break;
                    case 10:
                        total += fundingSummaryModel.Period11 ?? 0;
                        break;
                    case 11:
                        total += fundingSummaryModel.Period12 ?? 0;
                        break;
                }
            }

            return total;
        }

        private decimal? GetYearToDateTotal(LearnerPeriodisedValue albAreaUpliftBalPaymentObj, LearnerPeriodisedValue albAreaUpliftOnProgPaymentObj, int period)
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
