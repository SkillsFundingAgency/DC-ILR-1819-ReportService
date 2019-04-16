using System;
using ESFA.DC.ILR.ReportService.Model.Styling;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class FundingSummaryModel : ICloneable
    {
        public FundingSummaryModel(int excelHeaderStyle = 4, int excelRecordStyle = 4)
        {
            ExcelHeaderStyle = excelHeaderStyle;
            ExcelRecordStyle = excelRecordStyle;

            Period1 = 0;
            Period2 = 0;
            Period3 = 0;
            Period4 = 0;
            Period5 = 0;
            Period6 = 0;
            Period7 = 0;
            Period8 = 0;
            Period9 = 0;
            Period10 = 0;
            Period11 = 0;
            Period12 = 0;
            Period1_8 = 0;
            Period9_12 = 0;
            YearToDate = 0;
            Total = 0;
        }

        public FundingSummaryModel(string title, HeaderType headerType = HeaderType.None, int excelHeaderStyle = 4)
        {
            ExcelHeaderStyle = excelHeaderStyle;
            ExcelRecordStyle = 4;
            Title = title;
            HeaderType = headerType;

            Period1 = 0;
            Period2 = 0;
            Period3 = 0;
            Period4 = 0;
            Period5 = 0;
            Period6 = 0;
            Period7 = 0;
            Period8 = 0;
            Period9 = 0;
            Period10 = 0;
            Period11 = 0;
            Period12 = 0;
            Period1_8 = 0;
            Period9_12 = 0;
            YearToDate = 0;
            Total = 0;
        }

        public string Title { get; set; }

        public decimal? Period1 { get; set; }

        public decimal? Period2 { get; set; }

        public decimal? Period3 { get; set; }

        public decimal? Period4 { get; set; }

        public decimal? Period5 { get; set; }

        public decimal? Period6 { get; set; }

        public decimal? Period7 { get; set; }

        public decimal? Period8 { get; set; }

        public decimal? Period9 { get; set; }

        public decimal? Period10 { get; set; }

        public decimal? Period11 { get; set; }

        public decimal? Period12 { get; set; }

        public decimal? Period1_8 { get; set; }

        public decimal? Period9_12 { get; set; }

        public decimal? YearToDate { get; set; }

        public decimal? Total { get; set; }

        public int ExcelHeaderStyle { get; set; }

        public int ExcelRecordStyle { get; set; }

        public HeaderType HeaderType { get; }

        /// <summary>
        /// Shallow copies this model (which is enough as it should only have value types)
        /// </summary>
        /// <returns>A shallow copy of this object.</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}