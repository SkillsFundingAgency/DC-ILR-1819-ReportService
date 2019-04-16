using Aspose.Cells;
using ESFA.DC.ILR.ReportService.Model.Styling;

namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IExcelStyleProvider
    {
        CellStyle[] GetFundingSummaryStyles(Workbook workbook);

        CellStyle GetCellStyle(CellStyle[] cellStyles, int index);
    }
}
