using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Mapper
{
    public sealed class FundingSummaryMapper : ClassMap<FundingSummaryModel>, IClassMapper
    {
        private const string DecimalFormat = "0.00000";

        public FundingSummaryMapper()
        {
            Map(m => m.Title).Index(0).Name("NA");
            Map(m => m.Period1).Index(1).Name("Aug 2018").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period2).Index(2).Name("Sep 2018").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period3).Index(3).Name("Oct 2018").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period4).Index(4).Name("Nov 2018").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period5).Index(5).Name("Dec 2018").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period6).Index(6).Name("Jan 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period7).Index(7).Name("Feb 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period8).Index(8).Name("Mar 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period9).Index(9).Name("Apr 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period10).Index(10).Name("May 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period11).Index(11).Name("Jun 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period12).Index(12).Name("Jul 2019").TypeConverterOption.Format(DecimalFormat);
            Map(m => m.Period1_8).Index(13).Name("Aug-Mar").TypeConverterOption.Format(DecimalFormat).TypeConverterOption.NullValues(Constants.NotApplicable);
            Map(m => m.Period9_12).Index(14).Name("Apr-Jul").TypeConverterOption.Format(DecimalFormat).TypeConverterOption.NullValues(Constants.NotApplicable);
            Map(m => m.YearToDate).Index(15).Name("Year To Date").TypeConverterOption.Format(DecimalFormat).TypeConverterOption.NullValues(Constants.NotApplicable);
            Map(m => m.Total).Index(16).Name("Total").TypeConverterOption.Format(DecimalFormat).TypeConverterOption.NullValues(Constants.NotApplicable);
        }
    }
}
