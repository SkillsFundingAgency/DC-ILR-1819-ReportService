using System;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Commands.AppsIndicativeEarnings
{
    public class AppsIndicativeAprilCommand : IAppsIndicativeCommand
    {
        public void Execute(AppsIndicativeEarningsModel model, LearningDelivery LearningDelivery, PriceEpisode episodeAttribute)
        {
            model.AprilOnProgrammeEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.AprilBalancingPaymentEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm3PriceEpisodeBalancePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.AprilAimCompletionEarnings =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.AprilLearningSupportEarnings = LearningDelivery?.LearningDeliveryValues?.LearnDelMathEng ?? false
                    ? LearningDelivery.LearningDeliveryPeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36LearnSuppFundCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0
                    : episodeAttribute?.PriceEpisodePeriodisedValues
                          ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLSFCashAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.AprilEnglishMathsOnProgrammeEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36MathEngOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.AprilEnglishMathsBalancingPaymentEarnings =
                LearningDelivery?.LearningDeliveryPeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36MathEngBalPayment, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.AprilDisadvantageEarnings =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0);

            model.April1618AdditionalPaymentForEmployers =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0);

            model.April1618AdditionalPaymentForProviders =
                (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0)
                + (episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0);

            model.AprilAdditionalPaymentsForApprentices =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att => string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.April1618FrameworkUpliftOnProgrammePayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.April1618FrameworkUpliftBalancingPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;

            model.April1618FrameworkUpliftCompletionPayment =
                episodeAttribute?.PriceEpisodePeriodisedValues
                    ?.SingleOrDefault(att =>
                        string.Equals(att.AttributeName, Constants.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Period9 ?? 0;
        }
    }
}
