using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Eas;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public sealed class EasBuilder : IEasBuilder
    {
        private readonly IEasProviderService _easProviderService;

        public EasBuilder(IEasProviderService easProviderService)
        {
            _easProviderService = easProviderService;
        }

        public FundingSummaryModel BuildWithEasSubValueLine(string title, List<EasSubmissionValues> easSubmissionValues, string paymentTypeName, int period)
        {
            FundingSummaryModel fundingSummaryModel = new FundingSummaryModel(title);

            List<EasSubmissionValues> paymentWiseSubmissionValues = easSubmissionValues
                .Where(sv => string.Equals(sv.PaymentTypeName, paymentTypeName, StringComparison.OrdinalIgnoreCase)).ToList();

            fundingSummaryModel.Period1 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 1).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period2 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 2).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period3 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 3).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period4 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 4).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period5 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 5).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period6 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 6).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period7 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 7).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period8 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 8).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period9 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 9).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period10 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 10).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period11 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 11).Sum(x => x.PaymentValue);
            fundingSummaryModel.Period12 = paymentWiseSubmissionValues.Where(x => x.CollectionPeriod == 12).Sum(x => x.PaymentValue);

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
