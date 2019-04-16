using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Lars;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output.LearningDelivery;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public class TrailblazerAppsOccupancyModelBuilder : ITrailblazerAppsOccupancyModelBuilder
    {
        public TrailblazerAppsOccupancyModel BuildTrailblazerAppsOccupancyModel(
            ILearner learner,
            ILearningDelivery learningDelivery,
            LarsLearningDelivery larsModel,
            LearningDelivery ruleBaseLearningDelivery)
        {
            List<IAppFinRecord> currentYearData = learningDelivery.AppFinRecords?
                .Where(x => x.AFinDate >= Constants.BeginningOfYear && x.AFinDate <= Constants.EndOfYear &&
                            string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

            List<IAppFinRecord> previousYearData = learningDelivery.AppFinRecords?
                .Where(x => x.AFinDate <= Constants.BeginningOfYear &&
                            string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

            var returnModel = new TrailblazerAppsOccupancyModel()
            {
                LearnRefNumber = learner.LearnRefNumber,
                UniqueLearnerNumber = learner.ULN,
                DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                PMUkPrn = learner.PMUKPRNNullable,
                CampId = learner.CampId,
                ProvSpecLearnMonA = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x =>
                        string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))
                    ?.ProvSpecLearnMon,
                ProvSpecLearnMonB = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x =>
                        string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))
                    ?.ProvSpecLearnMon,
                AimSeqNumber = learningDelivery.AimSeqNumber,
                LearnAimRef = learningDelivery.LearnAimRef,
                LearnAimRefTitle = larsModel.LearningAimTitle,
                SwSupAimId = learningDelivery.SWSupAimId,
                NotionalNvqLevelV2 =
                    larsModel.NotionalNvqLevel,
                AimType = learningDelivery.AimType,
                StdCode = learningDelivery.StdCodeNullable,
                FundModel = learningDelivery.FundModel,
                PriorLearnFundAdj = learningDelivery.PriorLearnFundAdjNullable,
                OtherFundAdj = learningDelivery.OtherFundAdjNullable,
                OrigLearnStartDate = learningDelivery.OrigLearnStartDateNullable,
                LearnStartDate = learningDelivery.LearnStartDate,
                LearnPlanEndDate = learningDelivery.LearnPlanEndDate,
                CompStatus = learningDelivery.CompStatus,
                LearnActEndDate = learningDelivery.LearnActEndDateNullable,
                Outcome = learningDelivery.OutcomeNullable,
                AchDate = learningDelivery.AchDateNullable,
                LearnDelFamCodeSof = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeSOF, StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelFamCodeEef = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, "EEF", StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelFamCodeLsfHighest = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeLSF, StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelFamCodeLsfEarliest = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeLSF, StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMDateFromNullable,
                LearnDelFamCodeLsfLatest = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, Constants.LearningDeliveryFAMCodeLSF, StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMDateToNullable,
                LearnDelMonA = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, "LDM1", StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelMonB = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, "LDM2", StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelMonC = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, "LDM3", StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelMonD = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, "LDM4", StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                LearnDelMonRestartIndicator = learningDelivery.LearningDeliveryFAMs
                    ?.SingleOrDefault(x => string.Equals(x.LearnDelFAMType, "RES", StringComparison.OrdinalIgnoreCase))
                    ?.LearnDelFAMCode,
                ProvSpecDelMonA = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(
                        x => string.Equals(x.ProvSpecDelMonOccur, "A", StringComparison.OrdinalIgnoreCase))
                    ?.ProvSpecDelMon,
                ProvSpecDelMonB = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(
                        x => string.Equals(x.ProvSpecDelMonOccur, "B", StringComparison.OrdinalIgnoreCase))
                    ?.ProvSpecDelMon,
                ProvSpecDelMonC = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(
                        x => string.Equals(x.ProvSpecDelMonOccur, "C", StringComparison.OrdinalIgnoreCase))
                    ?.ProvSpecDelMon,
                ProvSpecDelMonD = learningDelivery.ProviderSpecDeliveryMonitorings
                    ?.SingleOrDefault(
                        x => string.Equals(x.ProvSpecDelMonOccur, "D", StringComparison.OrdinalIgnoreCase))
                    ?.ProvSpecDelMon,
                EpaOrgID = learningDelivery.EPAOrgID,
                PartnerUKPRN = learningDelivery.PartnerUKPRNNullable,
                DelLocPostCode = learningDelivery.DelLocPostCode,
                CoreGovContCapApplicVal = ruleBaseLearningDelivery?.LearningDeliveryValues?.CoreGovContCapApplicVal,
                SmallBusApplicVal = ruleBaseLearningDelivery?.LearningDeliveryValues?.SmallBusApplicVal,
                YoungAppApplicVal = ruleBaseLearningDelivery?.LearningDeliveryValues?.YoungAppApplicVal,
                AchievementApplicVal = ruleBaseLearningDelivery?.LearningDeliveryValues?.AchievementApplicVal,
                ApplicFundValDate = ruleBaseLearningDelivery?.LearningDeliveryValues?.ApplicFundValDate,
                FundLine = ruleBaseLearningDelivery?.LearningDeliveryValues?.FundLine,
                EmpIdFirstDayStandard = ruleBaseLearningDelivery?.LearningDeliveryValues?.EmpIdFirstDayStandard,
                EmpIdSmallBusDate = ruleBaseLearningDelivery?.LearningDeliveryValues?.EmpIdSmallBusDate,
                EmpIdFirstYoungAppDate = ruleBaseLearningDelivery?.LearningDeliveryValues?.EmpIdFirstYoungAppDate,
                EmpIdSecondYoungAppDate = ruleBaseLearningDelivery?.LearningDeliveryValues?.EmpIdSecondYoungAppDate,
                EmpIdAchDate = ruleBaseLearningDelivery?.LearningDeliveryValues?.EmpIdAchDate,
                MathEngLSFFundStart = ruleBaseLearningDelivery?.LearningDeliveryValues?.MathEngLSFFundStart,
                AgeStandardStart = ruleBaseLearningDelivery?.LearningDeliveryValues?.AgeStandardStart,
                YoungAppEligible = ruleBaseLearningDelivery?.LearningDeliveryValues?.YoungAppEligible,
                SmallBusEligible = ruleBaseLearningDelivery?.LearningDeliveryValues?.SmallBusEligible,
                AchApplicDate = ruleBaseLearningDelivery?.LearningDeliveryValues?.AchApplicDate
            };

            if (learningDelivery.AimType == 1)
            {
                returnModel.TotalNegotiatedPrice1 = learningDelivery.AppFinRecords?
                    .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                x.AFinCode == 1).OrderByDescending(x => x.AFinDate).FirstOrDefault().AFinAmount;

                returnModel.TotalNegotiatedPrice1 = learningDelivery.AppFinRecords?
                    .Where(x => string.Equals(x.AFinType, "TNP", StringComparison.OrdinalIgnoreCase) &&
                                x.AFinCode == 2).OrderByDescending(x => x.AFinDate).FirstOrDefault().AFinAmount;
            }

            returnModel.PMRSumBeforeFundingYear =
                previousYearData?.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                previousYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            var coreGovContPaymentList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81CoreGovContPayment, StringComparison.OrdinalIgnoreCase)).ToList();
            var mathEngOnProgPaymentList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81MathEngOnProgPayment, StringComparison.OrdinalIgnoreCase)).ToList();
            var mathEngBalPaymentList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81MathEngBalPayment, StringComparison.OrdinalIgnoreCase)).ToList();
            var learnSuppFundCashList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81LearnSuppFundCash, StringComparison.OrdinalIgnoreCase)).ToList();
            var smallBusPaymentList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81MathEngOnProgPayment, StringComparison.OrdinalIgnoreCase)).ToList();
            var youngAppPaymentList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81MathEngBalPayment, StringComparison.OrdinalIgnoreCase)).ToList();
            var achPaymentList = ruleBaseLearningDelivery?.LearningDeliveryPeriodisedValues.Where(l => string.Equals(l.AttributeName, Constants.Fm81MathEngBalPayment, StringComparison.OrdinalIgnoreCase)).ToList();

            returnModel.Period1CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period1) ?? decimal.Zero;
            returnModel.Period1MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period1) ?? decimal.Zero;
            returnModel.Period1MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period1) ?? decimal.Zero;
            returnModel.Period1LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period1) ?? decimal.Zero;
            returnModel.Period1SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period1) ?? decimal.Zero;
            returnModel.Period1YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period1) ?? decimal.Zero;
            returnModel.Period1AchPayment = achPaymentList?.Sum(x => x.Period1) ?? decimal.Zero;

            returnModel.Period2CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period2) ?? decimal.Zero;
            returnModel.Period2MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period2) ?? decimal.Zero;
            returnModel.Period2MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period2) ?? decimal.Zero;
            returnModel.Period2LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period2) ?? decimal.Zero;
            returnModel.Period2SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period2) ?? decimal.Zero;
            returnModel.Period2YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period2) ?? decimal.Zero;
            returnModel.Period2AchPayment = achPaymentList?.Sum(x => x.Period2) ?? decimal.Zero;

            returnModel.Period3CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period3) ?? decimal.Zero;
            returnModel.Period3MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period3) ?? decimal.Zero;
            returnModel.Period3MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period3) ?? decimal.Zero;
            returnModel.Period3LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period3) ?? decimal.Zero;
            returnModel.Period3SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period3) ?? decimal.Zero;
            returnModel.Period3YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period3) ?? decimal.Zero;
            returnModel.Period3AchPayment = achPaymentList?.Sum(x => x.Period3) ?? decimal.Zero;

            returnModel.Period4CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period4) ?? decimal.Zero;
            returnModel.Period4MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period4) ?? decimal.Zero;
            returnModel.Period4MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period4) ?? decimal.Zero;
            returnModel.Period4LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period4) ?? decimal.Zero;
            returnModel.Period4SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period4) ?? decimal.Zero;
            returnModel.Period4YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period4) ?? decimal.Zero;
            returnModel.Period4AchPayment = achPaymentList?.Sum(x => x.Period4) ?? decimal.Zero;

            returnModel.Period5CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period5) ?? decimal.Zero;
            returnModel.Period5MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period5) ?? decimal.Zero;
            returnModel.Period5MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period5) ?? decimal.Zero;
            returnModel.Period5LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period5) ?? decimal.Zero;
            returnModel.Period5SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period5) ?? decimal.Zero;
            returnModel.Period5YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period5) ?? decimal.Zero;
            returnModel.Period5AchPayment = achPaymentList?.Sum(x => x.Period5) ?? decimal.Zero;

            returnModel.Period6CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period6) ?? decimal.Zero;
            returnModel.Period6MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period6) ?? decimal.Zero;
            returnModel.Period6MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period6) ?? decimal.Zero;
            returnModel.Period6LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period6) ?? decimal.Zero;
            returnModel.Period6SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period6) ?? decimal.Zero;
            returnModel.Period6YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period6) ?? decimal.Zero;
            returnModel.Period6AchPayment = achPaymentList?.Sum(x => x.Period6) ?? decimal.Zero;

            returnModel.Period7CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period7) ?? decimal.Zero;
            returnModel.Period7MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period7) ?? decimal.Zero;
            returnModel.Period7MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period7) ?? decimal.Zero;
            returnModel.Period7LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period7) ?? decimal.Zero;
            returnModel.Period7SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period7) ?? decimal.Zero;
            returnModel.Period7YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period7) ?? decimal.Zero;
            returnModel.Period7AchPayment = achPaymentList?.Sum(x => x.Period7) ?? decimal.Zero;

            returnModel.Period8CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period8) ?? decimal.Zero;
            returnModel.Period8MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period8) ?? decimal.Zero;
            returnModel.Period8MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period8) ?? decimal.Zero;
            returnModel.Period8LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period8) ?? decimal.Zero;
            returnModel.Period8SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period8) ?? decimal.Zero;
            returnModel.Period8YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period8) ?? decimal.Zero;
            returnModel.Period8AchPayment = achPaymentList?.Sum(x => x.Period8) ?? decimal.Zero;

            returnModel.Period9CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period9) ?? decimal.Zero;
            returnModel.Period9MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period9) ?? decimal.Zero;
            returnModel.Period9MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period9) ?? decimal.Zero;
            returnModel.Period9LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period9) ?? decimal.Zero;
            returnModel.Period9SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period9) ?? decimal.Zero;
            returnModel.Period9YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period9) ?? decimal.Zero;
            returnModel.Period9AchPayment = achPaymentList?.Sum(x => x.Period9) ?? decimal.Zero;

            returnModel.Period10CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period10) ?? decimal.Zero;
            returnModel.Period10MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period10) ?? decimal.Zero;
            returnModel.Period10MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period10) ?? decimal.Zero;
            returnModel.Period10LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period10) ?? decimal.Zero;
            returnModel.Period10SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period10) ?? decimal.Zero;
            returnModel.Period10YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period10) ?? decimal.Zero;
            returnModel.Period10AchPayment = achPaymentList?.Sum(x => x.Period10) ?? decimal.Zero;

            returnModel.Period11CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period11) ?? decimal.Zero;
            returnModel.Period11MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period11) ?? decimal.Zero;
            returnModel.Period11MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period11) ?? decimal.Zero;
            returnModel.Period11LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period11) ?? decimal.Zero;
            returnModel.Period11SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period11) ?? decimal.Zero;
            returnModel.Period11YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period11) ?? decimal.Zero;
            returnModel.Period11AchPayment = achPaymentList?.Sum(x => x.Period11) ?? decimal.Zero;

            returnModel.Period12CoreGovContPayment = coreGovContPaymentList?.Sum(x => x.Period12) ?? decimal.Zero;
            returnModel.Period12MathEngOnProgPayment = mathEngOnProgPaymentList?.Sum(x => x.Period12) ?? decimal.Zero;
            returnModel.Period12MathEngBalPayment = mathEngBalPaymentList?.Sum(x => x.Period12) ?? decimal.Zero;
            returnModel.Period12LearnSuppFundCash = learnSuppFundCashList?.Sum(x => x.Period12) ?? decimal.Zero;
            returnModel.Period12SmallBusPayment = smallBusPaymentList?.Sum(x => x.Period12) ?? decimal.Zero;
            returnModel.Period12YoungAppPayment = youngAppPaymentList?.Sum(x => x.Period12) ?? decimal.Zero;
            returnModel.Period12AchPayment = achPaymentList?.Sum(x => x.Period12) ?? decimal.Zero;

            returnModel.Period1PMRSum =
                GetAppFinRecordListByPeriods(learningDelivery, 8, Constants.BeginningOfYear.Year)?
                    .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period2PMRSum =
                GetAppFinRecordListByPeriods(learningDelivery, 9, Constants.BeginningOfYear.Year)?
                    .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period3PMRSum =
                GetAppFinRecordListByPeriods(learningDelivery, 10, Constants.BeginningOfYear.Year)?
                    .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period4PMRSum =
                GetAppFinRecordListByPeriods(learningDelivery, 11, Constants.BeginningOfYear.Year)?
                    .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period5PMRSum =
                GetAppFinRecordListByPeriods(learningDelivery, 12, Constants.BeginningOfYear.Year)?
                    .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period6PMRSum =
                GetAppFinRecordListByPeriods(learningDelivery, 1, Constants.EndOfYear.Year)?
                    .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period7PMRSum =
               GetAppFinRecordListByPeriods(learningDelivery, 2, Constants.EndOfYear.Year)?
                   .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
               currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period8PMRSum =
               GetAppFinRecordListByPeriods(learningDelivery, 3, Constants.EndOfYear.Year)?
                   .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
               currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period9PMRSum =
               GetAppFinRecordListByPeriods(learningDelivery, 4, Constants.EndOfYear.Year)?
                   .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
               currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period10PMRSum =
               GetAppFinRecordListByPeriods(learningDelivery, 5, Constants.EndOfYear.Year)?
                   .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
               currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period11PMRSum =
               GetAppFinRecordListByPeriods(learningDelivery, 6, Constants.EndOfYear.Year)?
                   .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
               currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.Period12PMRSum =
               GetAppFinRecordListByPeriods(learningDelivery, 7, Constants.EndOfYear.Year)?
                   .Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
               currentYearData?.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);

            returnModel.TotalCoreGovContPayment =
                returnModel.Period1CoreGovContPayment + returnModel.Period2CoreGovContPayment +
                returnModel.Period3CoreGovContPayment + returnModel.Period4CoreGovContPayment +
                returnModel.Period5CoreGovContPayment + returnModel.Period6CoreGovContPayment +
                returnModel.Period7CoreGovContPayment + returnModel.Period8CoreGovContPayment +
                returnModel.Period9CoreGovContPayment + returnModel.Period10CoreGovContPayment +
                returnModel.Period11CoreGovContPayment + returnModel.Period12CoreGovContPayment;

            returnModel.TotalMathEngOnProgPayment =
                returnModel.Period1MathEngOnProgPayment + returnModel.Period2MathEngOnProgPayment +
                returnModel.Period3MathEngOnProgPayment + returnModel.Period4MathEngOnProgPayment +
                returnModel.Period5MathEngOnProgPayment + returnModel.Period6MathEngOnProgPayment +
                returnModel.Period7MathEngOnProgPayment + returnModel.Period8MathEngOnProgPayment +
                returnModel.Period9MathEngOnProgPayment + returnModel.Period10MathEngOnProgPayment +
                returnModel.Period11MathEngOnProgPayment + returnModel.Period12MathEngOnProgPayment;

            returnModel.TotalMathEngBalPayment =
                returnModel.Period1MathEngBalPayment + returnModel.Period2MathEngBalPayment +
                returnModel.Period3MathEngBalPayment + returnModel.Period4MathEngBalPayment +
                returnModel.Period5MathEngBalPayment + returnModel.Period6MathEngBalPayment +
                returnModel.Period7MathEngBalPayment + returnModel.Period8MathEngBalPayment +
                returnModel.Period9MathEngBalPayment + returnModel.Period10MathEngBalPayment +
                returnModel.Period11MathEngBalPayment + returnModel.Period12MathEngBalPayment;

            returnModel.TotalLearnSuppFundCash =
                returnModel.Period1LearnSuppFundCash + returnModel.Period2LearnSuppFundCash +
                returnModel.Period3LearnSuppFundCash + returnModel.Period4LearnSuppFundCash +
                returnModel.Period5LearnSuppFundCash + returnModel.Period6LearnSuppFundCash +
                returnModel.Period7LearnSuppFundCash + returnModel.Period8LearnSuppFundCash +
                returnModel.Period9LearnSuppFundCash + returnModel.Period10LearnSuppFundCash +
                returnModel.Period11LearnSuppFundCash + returnModel.Period12LearnSuppFundCash;

            returnModel.TotalSmallBusPayment =
                returnModel.Period1SmallBusPayment + returnModel.Period2SmallBusPayment +
                returnModel.Period3SmallBusPayment + returnModel.Period4SmallBusPayment +
                returnModel.Period5SmallBusPayment + returnModel.Period6SmallBusPayment +
                returnModel.Period7SmallBusPayment + returnModel.Period8SmallBusPayment +
                returnModel.Period9SmallBusPayment + returnModel.Period10SmallBusPayment +
                returnModel.Period11SmallBusPayment + returnModel.Period12SmallBusPayment;

            returnModel.TotalYoungAppPayment =
                returnModel.Period1YoungAppPayment + returnModel.Period2YoungAppPayment +
                returnModel.Period3YoungAppPayment + returnModel.Period4YoungAppPayment +
                returnModel.Period5YoungAppPayment + returnModel.Period6YoungAppPayment +
                returnModel.Period7YoungAppPayment + returnModel.Period8YoungAppPayment +
                returnModel.Period9YoungAppPayment + returnModel.Period10YoungAppPayment +
                returnModel.Period11YoungAppPayment + returnModel.Period12YoungAppPayment;

            returnModel.TotalAchPayment =
                returnModel.Period1AchPayment + returnModel.Period2AchPayment +
                returnModel.Period3AchPayment + returnModel.Period4AchPayment +
                returnModel.Period5AchPayment + returnModel.Period6AchPayment +
                returnModel.Period7AchPayment + returnModel.Period8AchPayment +
                returnModel.Period9AchPayment + returnModel.Period10AchPayment +
                returnModel.Period11AchPayment + returnModel.Period12AchPayment;

            returnModel.TotalPMRSum =
                returnModel.Period1PMRSum + returnModel.Period2PMRSum +
                returnModel.Period3PMRSum + returnModel.Period4PMRSum +
                returnModel.Period5PMRSum + returnModel.Period6PMRSum +
                returnModel.Period7PMRSum + returnModel.Period8PMRSum +
                returnModel.Period9PMRSum + returnModel.Period10PMRSum +
                returnModel.Period11PMRSum + returnModel.Period12PMRSum;

            return returnModel;
        }

        private List<IAppFinRecord> GetAppFinRecordListByPeriods(ILearningDelivery learningDelivery, int month, int year)
        {
            List<IAppFinRecord> result = learningDelivery.AppFinRecords?
                .Where(x => x.AFinDate.Month == month && x.AFinDate.Year == year &&
                            string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

            return result;
        }
    }
}