using System;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings
{
    public class AppsIndicativeSeptemberCommand : IAppsIndicativeCommand
    {
        public void Execute(AppsIndicativeEarningsModel model, LearningDelivery LearningDelivery, PriceEpisode episodeAttribute)
        {
            model.SeptemberOnProgrammeEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.SeptemberBalancingPaymentEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.SeptemberAimCompletionEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.SeptemberLearningSupportEarnings = LearningDelivery?.LearningDeliveryValues?.LearnDelMathEng ?? false
                    ? LearningDelivery.LearningDeliveryPeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LearnSuppFundCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0
                    : episodeAttribute?.PriceEpisodePeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.SeptemberEnglishMathsOnProgrammeEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.SeptemberEnglishMathsBalancingPaymentEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngBalPayment, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.SeptemberDisadvantageEarnings =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0);

            model.September1618AdditionalPaymentForEmployers =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0);

            model.September1618AdditionalPaymentForProviders =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0);

            model.SeptemberAdditionalPaymentsForApprentices =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.September1618FrameworkUpliftOnProgrammePayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.September1618FrameworkUpliftBalancingPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;

            model.September1618FrameworkUpliftCompletionPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period2 ?? 0;
        }
    }
}
