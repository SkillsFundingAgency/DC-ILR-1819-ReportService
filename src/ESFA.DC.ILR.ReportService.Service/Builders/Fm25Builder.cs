using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public sealed class Fm25Builder : IFm25Builder
    {
        // Singleton
        public Fm25Builder()
        {
        }

        public FundingSummaryModel BuildWithFundLine(
            string title,
            FM25Global fm25Global,
            List<string> validLearners,
            string fundLine,
            int period)
        {
            FundingSummaryModel fundingSummaryModel = new FundingSummaryModel(title);

            if (fm25Global?.Learners == null || validLearners == null)
            {
                return fundingSummaryModel;
            }

            FM25Learner[] fundLineObject = fm25Global.Learners.Where(x =>
                string.Equals(x.FundLine, fundLine, StringComparison.OrdinalIgnoreCase) &&
                validLearners.Contains(x.LearnRefNumber)).ToArray();

            LearnerPeriodisedValues[] periodisedValues = fundLineObject.SelectMany(x => x.LearnerPeriodisedValues)
                .Where(x => string.Equals(x.AttributeName, "LnrOnProgPay")).ToArray();

            fundingSummaryModel.Period1 = periodisedValues.Sum(x => x.Period1 ?? 0);
            fundingSummaryModel.Period2 = periodisedValues.Sum(x => x.Period2 ?? 0);
            fundingSummaryModel.Period3 = periodisedValues.Sum(x => x.Period3 ?? 0);
            fundingSummaryModel.Period4 = periodisedValues.Sum(x => x.Period4 ?? 0);
            fundingSummaryModel.Period5 = periodisedValues.Sum(x => x.Period5 ?? 0);
            fundingSummaryModel.Period6 = periodisedValues.Sum(x => x.Period6 ?? 0);
            fundingSummaryModel.Period7 = periodisedValues.Sum(x => x.Period7 ?? 0);
            fundingSummaryModel.Period8 = periodisedValues.Sum(x => x.Period8 ?? 0);
            fundingSummaryModel.Period9 = periodisedValues.Sum(x => x.Period9 ?? 0);
            fundingSummaryModel.Period10 = periodisedValues.Sum(x => x.Period10 ?? 0);
            fundingSummaryModel.Period11 = periodisedValues.Sum(x => x.Period11 ?? 0);
            fundingSummaryModel.Period12 = periodisedValues.Sum(x => x.Period12 ?? 0);

            fundingSummaryModel.Period1_8 =
                fundingSummaryModel.Period1 + fundingSummaryModel.Period2 + fundingSummaryModel.Period3 +
                fundingSummaryModel.Period4 + fundingSummaryModel.Period5 + fundingSummaryModel.Period6 +
                fundingSummaryModel.Period7 + fundingSummaryModel.Period8;
            fundingSummaryModel.Period9_12 =
                fundingSummaryModel.Period9 + fundingSummaryModel.Period10 + fundingSummaryModel.Period11 +
                fundingSummaryModel.Period12;
            fundingSummaryModel.YearToDate = GetYearToDate(fundingSummaryModel, period - 1);
            fundingSummaryModel.Total = GetYearToDate(fundingSummaryModel, 12);

            return fundingSummaryModel;
        }

        private decimal? GetYearToDate(FundingSummaryModel fundingSummaryModel, int period)
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
    }
}
