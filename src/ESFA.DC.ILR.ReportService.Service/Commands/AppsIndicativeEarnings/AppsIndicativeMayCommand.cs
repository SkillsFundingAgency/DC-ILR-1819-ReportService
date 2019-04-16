using System;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings
{
    public class AppsIndicativeMayCommand : IAppsIndicativeCommand
    {
        public void Execute(AppsIndicativeEarningsModel model, LearningDelivery LearningDelivery, PriceEpisode episodeAttribute)
        {
            model.MayOnProgrammeEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.MayBalancingPaymentEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.MayAimCompletionEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.MayLearningSupportEarnings = LearningDelivery?.LearningDeliveryValues?.LearnDelMathEng ?? false
                    ? LearningDelivery.LearningDeliveryPeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LearnSuppFundCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0
                    : episodeAttribute?.PriceEpisodePeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.MayEnglishMathsOnProgrammeEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.MayEnglishMathsBalancingPaymentEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngBalPayment, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.MayDisadvantageEarnings =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0);

            model.May1618AdditionalPaymentForEmployers =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0);

            model.May1618AdditionalPaymentForProviders =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0);

            model.MayAdditionalPaymentsForApprentices =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.May1618FrameworkUpliftOnProgrammePayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.May1618FrameworkUpliftBalancingPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;

            model.May1618FrameworkUpliftCompletionPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period10 ?? 0;
        }
    }
}
