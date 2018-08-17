using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class FundingSummaryMapper : ClassMap<FundingSummaryModel>, IClassMapper
    {
        public FundingSummaryMapper()
        {
            Map(m => m.Title).Index(0).Name("Advanced Loans Bursary");
            Map(m => m.Period1).Index(1).Name("August 2018");
            Map(m => m.Period2).Index(2).Name("September 2018");
            Map(m => m.Period3).Index(3).Name("October 2018");
            Map(m => m.Period4).Index(4).Name("November 2018");
            Map(m => m.Period5).Index(5).Name("December 2018");
            Map(m => m.Period6).Index(6).Name("January 2019");
            Map(m => m.Period7).Index(7).Name("February 2019");
            Map(m => m.Period8).Index(8).Name("March 2019");
            Map(m => m.Period9).Index(9).Name("April 2019");
            Map(m => m.Period10).Index(10).Name("May 2019");
            Map(m => m.Period11).Index(11).Name("June 2019");
            Map(m => m.Period12).Index(12).Name("July 2019");
            Map(m => m.Period1_8).Index(13).Name("Aug-Mar");
            Map(m => m.Period9_12).Index(14).Name("Apr-Jul");
            Map(m => m.YearToDate).Index(15).Name("Year To Date");
            Map(m => m.Total).Index(16).Name("Total");
        }
    }
}
