using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM81.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public class TrailblazerEmployerIncentivesModelBuilder : ITrailblazerEmployerIncentivesModelBuilder
    {
        public TrailblazerEmployerIncentivesModel BuildTrailblazerEmployerIncentivesModel(long empIdentifier, LearningDelivery fm81Data)
        {
            var smallEmployerIncentive = fm81Data?.LearningDeliveryPeriodisedValues
                ?.SingleOrDefault(attr => string.Equals(attr.AttributeName, Constants.Fm81SmallBusPayment, StringComparison.OrdinalIgnoreCase));
            var apprentice1618Incentive = fm81Data?.LearningDeliveryPeriodisedValues
                ?.SingleOrDefault(attr => string.Equals(attr.AttributeName, Constants.Fm81YoungAppPayment, StringComparison.OrdinalIgnoreCase));
            var achievementIncentive = fm81Data?.LearningDeliveryPeriodisedValues
                ?.SingleOrDefault(attr => string.Equals(attr.AttributeName, Constants.Fm81AchPayment, StringComparison.OrdinalIgnoreCase));

            var trailblazerEmployerIncentiveModel = new TrailblazerEmployerIncentivesModel()
            {
                EmployerIdentifier = empIdentifier,
                Period1SmallEmployerIncentive = smallEmployerIncentive?.Period1,
                Period2SmallEmployerIncentive = smallEmployerIncentive?.Period2,
                Period3SmallEmployerIncentive = smallEmployerIncentive?.Period3,
                Period4SmallEmployerIncentive = smallEmployerIncentive?.Period4,
                Period5SmallEmployerIncentive = smallEmployerIncentive?.Period5,
                Period6SmallEmployerIncentive = smallEmployerIncentive?.Period6,
                Period7SmallEmployerIncentive = smallEmployerIncentive?.Period7,
                Period8SmallEmployerIncentive = smallEmployerIncentive?.Period8,
                Period9SmallEmployerIncentive = smallEmployerIncentive?.Period9,
                Period10SmallEmployerIncentive = smallEmployerIncentive?.Period10,
                Period11SmallEmployerIncentive = smallEmployerIncentive?.Period11,
                Period12SmallEmployerIncentive = smallEmployerIncentive?.Period12,

                SmallEmployerIncentiveTotal = smallEmployerIncentive?.Period1 + smallEmployerIncentive?.Period2 + smallEmployerIncentive?.Period3 +
                                              smallEmployerIncentive?.Period4 + smallEmployerIncentive?.Period5 + smallEmployerIncentive?.Period6 +
                                              smallEmployerIncentive?.Period7 + smallEmployerIncentive?.Period8 + smallEmployerIncentive?.Period9 +
                                              smallEmployerIncentive?.Period10 + smallEmployerIncentive?.Period11 + smallEmployerIncentive?.Period12,

                Period11618ApprenticeIncentive = apprentice1618Incentive?.Period1,
                Period21618ApprenticeIncentive = apprentice1618Incentive?.Period2,
                Period31618ApprenticeIncentive = apprentice1618Incentive?.Period3,
                Period41618ApprenticeIncentive = apprentice1618Incentive?.Period4,
                Period51618ApprenticeIncentive = apprentice1618Incentive?.Period5,
                Period61618ApprenticeIncentive = apprentice1618Incentive?.Period6,
                Period71618ApprenticeIncentive = apprentice1618Incentive?.Period7,
                Period81618ApprenticeIncentive = apprentice1618Incentive?.Period8,
                Period91618ApprenticeIncentive = apprentice1618Incentive?.Period9,
                Period101618ApprenticeIncentive = apprentice1618Incentive?.Period10,
                Period111618ApprenticeIncentive = apprentice1618Incentive?.Period11,
                Period121618ApprenticeIncentive = apprentice1618Incentive?.Period12,

                Apprentice1618IncentiveTotal = apprentice1618Incentive?.Period1 + apprentice1618Incentive?.Period2 + apprentice1618Incentive?.Period3 +
                                                apprentice1618Incentive?.Period4 + apprentice1618Incentive?.Period5 + apprentice1618Incentive?.Period6 +
                                              apprentice1618Incentive?.Period7 + apprentice1618Incentive?.Period8 + apprentice1618Incentive?.Period9 +
                                              apprentice1618Incentive?.Period10 + apprentice1618Incentive?.Period11 + apprentice1618Incentive?.Period12,

                Period1AchievementIncentive = achievementIncentive?.Period1,
                Period2AchievementIncentive = achievementIncentive?.Period2,
                Period3AchievementIncentive = achievementIncentive?.Period3,
                Period4AchievementIncentive = achievementIncentive?.Period4,
                Period5AchievementIncentive = achievementIncentive?.Period5,
                Period6AchievementIncentive = achievementIncentive?.Period6,
                Period7AchievementIncentive = achievementIncentive?.Period7,
                Period8AchievementIncentive = achievementIncentive?.Period8,
                Period9AchievementIncentive = achievementIncentive?.Period9,
                Period10AchievementIncentive = achievementIncentive?.Period10,
                Period11AchievementIncentive = achievementIncentive?.Period11,
                Period12AchievementIncentive = achievementIncentive?.Period12,

                AchievementTotal = achievementIncentive?.Period1 + achievementIncentive?.Period2 + achievementIncentive?.Period3 +
                                               achievementIncentive?.Period4 + achievementIncentive?.Period5 + achievementIncentive?.Period6 +
                                               achievementIncentive?.Period7 + achievementIncentive?.Period8 + achievementIncentive?.Period9 +
                                               achievementIncentive?.Period10 + achievementIncentive?.Period11 + achievementIncentive?.Period12,

                Period1Total = smallEmployerIncentive?.Period1 + apprentice1618Incentive?.Period1 + achievementIncentive?.Period1,
                Period2Total = smallEmployerIncentive?.Period2 + apprentice1618Incentive?.Period2 + achievementIncentive?.Period2,
                Period3Total = smallEmployerIncentive?.Period3 + apprentice1618Incentive?.Period3 + achievementIncentive?.Period3,
                Period4Total = smallEmployerIncentive?.Period4 + apprentice1618Incentive?.Period4 + achievementIncentive?.Period4,
                Period5Total = smallEmployerIncentive?.Period5 + apprentice1618Incentive?.Period5 + achievementIncentive?.Period5,
                Period6Total = smallEmployerIncentive?.Period6 + apprentice1618Incentive?.Period6 + achievementIncentive?.Period6,
                Period7Total = smallEmployerIncentive?.Period7 + apprentice1618Incentive?.Period7 + achievementIncentive?.Period7,
                Period8Total = smallEmployerIncentive?.Period8 + apprentice1618Incentive?.Period8 + achievementIncentive?.Period8,
                Period9Total = smallEmployerIncentive?.Period9 + apprentice1618Incentive?.Period9 + achievementIncentive?.Period9,
                Period10Total = smallEmployerIncentive?.Period10 + apprentice1618Incentive?.Period10 + achievementIncentive?.Period10,
                Period11Total = smallEmployerIncentive?.Period11 + apprentice1618Incentive?.Period11 + achievementIncentive?.Period11,
                Period12Total = smallEmployerIncentive?.Period12 + apprentice1618Incentive?.Period12 + achievementIncentive?.Period12
            };

            trailblazerEmployerIncentiveModel.GrandTotal =
                trailblazerEmployerIncentiveModel.Period1Total + trailblazerEmployerIncentiveModel.Period2Total +
                trailblazerEmployerIncentiveModel.Period3Total + trailblazerEmployerIncentiveModel.Period4Total +
                trailblazerEmployerIncentiveModel.Period5Total + trailblazerEmployerIncentiveModel.Period6Total +
                trailblazerEmployerIncentiveModel.Period7Total + trailblazerEmployerIncentiveModel.Period8Total +
                trailblazerEmployerIncentiveModel.Period9Total + trailblazerEmployerIncentiveModel.Period10Total +
                trailblazerEmployerIncentiveModel.Period11Total + trailblazerEmployerIncentiveModel.Period12Total;

            return trailblazerEmployerIncentiveModel;
        }

        public TrailblazerEmployerIncentivesModel BuildTrailblazerEmployerIncentivesModel(long empIdentifier, Dictionary<string, int> employerIdentifier, List<LearningDelivery> fm81Data)
        {
            var smallBusPaymentList = fm81Data?.Where(x => x.LearningDeliveryValues?.EmpIdSmallBusDate == empIdentifier).FirstOrDefault()?.LearningDeliveryPeriodisedValues.Where(l => l.AttributeName == Constants.Fm81AchPayment).ToList();
            var youngAppFirstPaymentList = fm81Data?.Where(x => x.LearningDeliveryValues?.EmpIdFirstYoungAppDate == empIdentifier).FirstOrDefault()?.LearningDeliveryPeriodisedValues.Where(l => l.AttributeName == Constants.Fm81YoungAppPayment).ToList();
            var youngAppSecondPaymentList = fm81Data?.Where(x => x.LearningDeliveryValues?.EmpIdSecondYoungAppDate == empIdentifier).FirstOrDefault()?.LearningDeliveryPeriodisedValues.Where(l => l.AttributeName == "YoungAppSecondPayment").ToList();
            var achPaymentList = fm81Data?.Where(x => x.LearningDeliveryValues?.EmpIdAchDate == empIdentifier).FirstOrDefault()?.LearningDeliveryPeriodisedValues.Where(l => l.AttributeName == Constants.Fm81AchPayment).ToList();

            var trailblazerEmployerIncentiveModel = new TrailblazerEmployerIncentivesModel()
            {
                EmployerIdentifier = empIdentifier,
                Period1SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period1) ?? decimal.Zero,
                Period2SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period2) ?? decimal.Zero,
                Period3SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period3) ?? decimal.Zero,
                Period4SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period4) ?? decimal.Zero,
                Period5SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period5) ?? decimal.Zero,
                Period6SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period6) ?? decimal.Zero,
                Period7SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period7) ?? decimal.Zero,
                Period8SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period8) ?? decimal.Zero,
                Period9SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period9) ?? decimal.Zero,
                Period10SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period10) ?? decimal.Zero,
                Period11SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period11) ?? decimal.Zero,
                Period12SmallEmployerIncentive = smallBusPaymentList?.Sum(x => x.Period12) ?? decimal.Zero,

                Period11618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period1) + youngAppSecondPaymentList?.Sum(x => x.Period1) ?? decimal.Zero,
                Period21618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period2) + youngAppSecondPaymentList?.Sum(x => x.Period2) ?? decimal.Zero,
                Period31618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period3) + youngAppSecondPaymentList?.Sum(x => x.Period3) ?? decimal.Zero,
                Period41618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period4) + youngAppSecondPaymentList?.Sum(x => x.Period4) ?? decimal.Zero,
                Period51618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period5) + youngAppSecondPaymentList?.Sum(x => x.Period5) ?? decimal.Zero,
                Period61618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period6) + youngAppSecondPaymentList?.Sum(x => x.Period6) ?? decimal.Zero,
                Period71618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period7) + youngAppSecondPaymentList?.Sum(x => x.Period7) ?? decimal.Zero,
                Period81618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period8) + youngAppSecondPaymentList?.Sum(x => x.Period8) ?? decimal.Zero,
                Period91618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period9) + youngAppSecondPaymentList?.Sum(x => x.Period9) ?? decimal.Zero,
                Period101618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period10) + youngAppSecondPaymentList?.Sum(x => x.Period10) ?? decimal.Zero,
                Period111618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period11) + youngAppSecondPaymentList?.Sum(x => x.Period11) ?? decimal.Zero,
                Period121618ApprenticeIncentive = youngAppFirstPaymentList?.Sum(x => x.Period12) + youngAppSecondPaymentList?.Sum(x => x.Period12) ?? decimal.Zero,

                Period1AchievementIncentive = achPaymentList?.Sum(x => x.Period1) ?? decimal.Zero,
                Period2AchievementIncentive = achPaymentList?.Sum(x => x.Period2) ?? decimal.Zero,
                Period3AchievementIncentive = achPaymentList?.Sum(x => x.Period3) ?? decimal.Zero,
                Period4AchievementIncentive = achPaymentList?.Sum(x => x.Period4) ?? decimal.Zero,
                Period5AchievementIncentive = achPaymentList?.Sum(x => x.Period5) ?? decimal.Zero,
                Period6AchievementIncentive = achPaymentList?.Sum(x => x.Period6) ?? decimal.Zero,
                Period7AchievementIncentive = achPaymentList?.Sum(x => x.Period7) ?? decimal.Zero,
                Period8AchievementIncentive = achPaymentList?.Sum(x => x.Period8) ?? decimal.Zero,
                Period9AchievementIncentive = achPaymentList?.Sum(x => x.Period9) ?? decimal.Zero,
                Period10AchievementIncentive = achPaymentList?.Sum(x => x.Period10) ?? decimal.Zero,
                Period11AchievementIncentive = achPaymentList?.Sum(x => x.Period11) ?? decimal.Zero,
                Period12AchievementIncentive = achPaymentList?.Sum(x => x.Period12) ?? decimal.Zero
            };

            trailblazerEmployerIncentiveModel.Period1Total = trailblazerEmployerIncentiveModel.Period1SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period11618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period1AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period2Total = trailblazerEmployerIncentiveModel.Period2SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period21618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period2AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period3Total = trailblazerEmployerIncentiveModel.Period3SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period31618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period3AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period4Total = trailblazerEmployerIncentiveModel.Period4SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period41618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period4AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period5Total = trailblazerEmployerIncentiveModel.Period5SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period51618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period5AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period6Total = trailblazerEmployerIncentiveModel.Period6SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period61618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period6AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period7Total = trailblazerEmployerIncentiveModel.Period7SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period71618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period7AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period8Total = trailblazerEmployerIncentiveModel.Period8SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period81618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period8AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period9Total = trailblazerEmployerIncentiveModel.Period9SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period91618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period9AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period10Total = trailblazerEmployerIncentiveModel.Period10SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period101618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period10AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period11Total = trailblazerEmployerIncentiveModel.Period10SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period111618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period11AchievementIncentive;
            trailblazerEmployerIncentiveModel.Period12Total = trailblazerEmployerIncentiveModel.Period12SmallEmployerIncentive + trailblazerEmployerIncentiveModel.Period121618ApprenticeIncentive + trailblazerEmployerIncentiveModel.Period12AchievementIncentive;

            trailblazerEmployerIncentiveModel.SmallEmployerIncentiveTotal =
                trailblazerEmployerIncentiveModel.Period1SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period2SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period3SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period4SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period5SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period6SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period7SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period8SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period9SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period10SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period11SmallEmployerIncentive +
                trailblazerEmployerIncentiveModel.Period12SmallEmployerIncentive;

            trailblazerEmployerIncentiveModel.Apprentice1618IncentiveTotal =
                trailblazerEmployerIncentiveModel.Period11618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period21618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period31618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period41618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period51618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period61618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period71618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period81618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period91618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period101618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period111618ApprenticeIncentive +
                trailblazerEmployerIncentiveModel.Period121618ApprenticeIncentive;

            trailblazerEmployerIncentiveModel.AchievementTotal =
                trailblazerEmployerIncentiveModel.Period1AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period2AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period3AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period4AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period5AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period6AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period7AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period8AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period9AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period10AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period11AchievementIncentive +
                trailblazerEmployerIncentiveModel.Period12AchievementIncentive;

            trailblazerEmployerIncentiveModel.GrandTotal =
                trailblazerEmployerIncentiveModel.Period1Total + trailblazerEmployerIncentiveModel.Period2Total +
                trailblazerEmployerIncentiveModel.Period3Total + trailblazerEmployerIncentiveModel.Period4Total +
                trailblazerEmployerIncentiveModel.Period5Total + trailblazerEmployerIncentiveModel.Period6Total +
                trailblazerEmployerIncentiveModel.Period7Total + trailblazerEmployerIncentiveModel.Period8Total +
                trailblazerEmployerIncentiveModel.Period9Total + trailblazerEmployerIncentiveModel.Period10Total +
                trailblazerEmployerIncentiveModel.Period11Total + trailblazerEmployerIncentiveModel.Period12Total;

            return trailblazerEmployerIncentiveModel;
        }

        private decimal? GetPeriodValueForEmployerIdentifier(KeyValuePair<string, int> empIdentifier, int period, LearningDeliveryPeriodisedValue ldPeriodisedValue)
        {
            if (empIdentifier.Key == Constants.Fm81SmallBusPayment)
            {
                return ldPeriodisedValue.Period1;
            }

            return 0;
        }
    }
}