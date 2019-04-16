using System;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings
{
    public class AppsIndicativeMarchCommand : IAppsIndicativeCommand
    {
        public void Execute(AppsIndicativeEarningsModel model, LearningDelivery LearningDelivery, PriceEpisode episodeAttribute)
        {
            model.MarchOnProgrammeEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.MarchBalancingPaymentEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.MarchAimCompletionEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.MarchLearningSupportEarnings = LearningDelivery?.LearningDeliveryValues?.LearnDelMathEng ?? false
                    ? LearningDelivery.LearningDeliveryPeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LearnSuppFundCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0
                    : episodeAttribute?.PriceEpisodePeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.MarchEnglishMathsOnProgrammeEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.MarchEnglishMathsBalancingPaymentEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngBalPayment, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.MarchDisadvantageEarnings =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0);

            model.March1618AdditionalPaymentForEmployers =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0);

            model.March1618AdditionalPaymentForProviders =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0);

            model.MarchAdditionalPaymentsForApprentices =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.March1618FrameworkUpliftOnProgrammePayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.March1618FrameworkUpliftBalancingPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;

            model.March1618FrameworkUpliftCompletionPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period8 ?? 0;
        }
    }
}
