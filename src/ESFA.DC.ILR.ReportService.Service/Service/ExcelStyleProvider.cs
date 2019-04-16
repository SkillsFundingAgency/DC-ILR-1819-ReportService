using System.Collections.Generic;
using Aspose.Cells;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Styling;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public sealed class ExcelStyleProvider : IExcelStyleProvider
    {
        public CellStyle[] GetFundingSummaryStyles(Workbook workbook)
        {
            List<CellStyle> cellStyles = new List<CellStyle>();

            Style style1 = workbook.CreateStyle();
            style1.ForegroundColor = System.Drawing.Color.FromArgb(191, 191, 191);
            style1.Pattern = BackgroundType.Solid;
            style1.Font.Size = 13;
            style1.Font.IsBold = true;
            style1.Font.Name = "Arial";
            style1.SetCustom(Constants.FundingSummaryReportDecimalFormat, true);

            StyleFlag styleFlag1 = new StyleFlag();
            styleFlag1.CellShading = true;
            styleFlag1.Font = true;
            styleFlag1.NumberFormat = true;

            cellStyles.Add(new CellStyle(style1, styleFlag1));

            Style style2 = workbook.CreateStyle();
            style2.ForegroundColor = System.Drawing.Color.FromArgb(216, 216, 216);
            style2.Pattern = BackgroundType.Solid;
            style2.Font.Size = 12;
            style2.Font.IsBold = true;
            style2.Font.Name = "Arial";
            style2.SetCustom(Constants.FundingSummaryReportDecimalFormat, true);

            StyleFlag styleFlag2 = new StyleFlag();
            styleFlag2.NumberFormat = true;
            styleFlag2.CellShading = true;
            styleFlag2.Font = true;

            cellStyles.Add(new CellStyle(style2, styleFlag2));

            Style style3 = workbook.CreateStyle();
            style3.ForegroundColor = System.Drawing.Color.FromArgb(242, 242, 242);
            style3.Pattern = BackgroundType.Solid;
            style3.Font.Size = 11;
            style3.Font.IsBold = true;
            style3.Font.Name = "Arial";
            style3.SetCustom(Constants.FundingSummaryReportDecimalFormat, true);

            StyleFlag styleFlag3 = new StyleFlag();
            styleFlag3.NumberFormat = true;
            styleFlag3.CellShading = true;
            styleFlag3.Font = true;

            cellStyles.Add(new CellStyle(style3, styleFlag3));

            Style style4 = workbook.CreateStyle();
            style4.Font.Size = 11;
            style4.Font.IsBold = true;
            style4.Font.Name = "Arial";
            style4.SetCustom(Constants.FundingSummaryReportDecimalFormat, true);

            StyleFlag styleFlag4 = new StyleFlag();
            styleFlag4.NumberFormat = true;
            styleFlag4.CellShading = true;
            styleFlag4.Font = true;

            cellStyles.Add(new CellStyle(style4, styleFlag4));

            Style style5 = workbook.CreateStyle();
            style5.Font.Size = 10;
            style5.Font.Name = "Arial";
            style5.SetCustom(Constants.FundingSummaryReportDecimalFormat, true);

            StyleFlag styleFlag5 = new StyleFlag();
            styleFlag5.CellShading = true;
            styleFlag5.Font = true;
            styleFlag5.NumberFormat = true;

            cellStyles.Add(new CellStyle(style5, styleFlag5));

            Style style6 = workbook.CreateStyle();
            style6.Font.Size = 10;
            style6.Font.IsBold = true;
            style6.Font.Name = "Arial";

            StyleFlag styleFlag6 = new StyleFlag();
            styleFlag6.CellShading = true;
            styleFlag6.Font = true;

            cellStyles.Add(new CellStyle(style6, styleFlag6));

            return cellStyles.ToArray();
        }

        public CellStyle GetCellStyle(CellStyle[] cellStyles, int index)
        {
            if (index == -1)
            {
                return null;
            }

            return cellStyles[index];
        }
    }
}
