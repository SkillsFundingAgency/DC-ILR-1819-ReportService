using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Interface.Builders
{
    public interface ITotalBuilder
    {
        FundingSummaryModel TotalRecords(string title, params FundingSummaryModel[] fundingSummaryModels);

        decimal TotalRecords(ILR.FundingService.FM36.FundingOutput.Model.Output.PriceEpisodePeriodisedValues priceEpisodePeriodisedValues);

        decimal TotalRecords(params decimal?[] values);

        decimal? Total(decimal? original, decimal? value);

        FundingSummaryModel TotalRecordsCumulative(string title, FundingSummaryModel sourceFundingSummaryModel);
    }
}
