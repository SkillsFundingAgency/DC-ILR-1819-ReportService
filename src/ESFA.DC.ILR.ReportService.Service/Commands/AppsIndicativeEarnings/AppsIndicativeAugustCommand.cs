using System;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings
{
    public class AppsIndicativeAugustCommand : IAppsIndicativeCommand
    {
        public void Execute(AppsIndicativeEarningsModel model, LearningDelivery LearningDelivery, PriceEpisode episodeAttribute)
        {
            model.AugustOnProgrammeEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.AugustBalancingPaymentEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.AugustAimCompletionEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.AugustLearningSupportEarnings = LearningDelivery?.LearningDeliveryValues?.LearnDelMathEng ?? false
                    ? LearningDelivery.LearningDeliveryPeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LearnSuppFundCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0
                    : episodeAttribute?.PriceEpisodePeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.AugustEnglishMathsOnProgrammeEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.AugustEnglishMathsBalancingPaymentEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngBalPayment, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.AugustDisadvantageEarnings =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0);

            model.August1618AdditionalPaymentForEmployers =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0);

            model.August1618AdditionalPaymentForProviders =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0);

            model.AugustAdditionalPaymentsForApprentices =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.August1618FrameworkUpliftOnProgrammePayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.August1618FrameworkUpliftBalancingPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;

            model.August1618FrameworkUpliftCompletionPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period1 ?? 0;
        }
    }
}
