using System;
using System.IO;
using System.Linq;
using Aspose.Cells;
using CsvHelper.Configuration;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.ILR1819.ReportService.Tests.Helpers
{
    public static class TestXlsxHelper
    {
        /// <summary>
        /// Checks the Excel XLSX file for a header and checks a number of columns on the first data row.
        /// </summary>
        /// <param name="xlsx">The xlsx file as a byte array.</param>
        /// <param name="classMap">The classmap for the model stored in the XLSX.</param>
        /// <param name="numberOfColumnsOfData">The number of columns on the first row of data expected to be non-null.</param>
        public static void CheckXlsx(byte[] xlsx, ClassMap classMap, int numberOfColumnsOfData)
        {
            try
            {
                object[] names = classMap.MemberMaps.OrderBy(x => x.Data.Index).Select(x => (object)x.Data.Names[0]).ToArray();
                LoadOptions loadOptions = new LoadOptions(LoadFormat.Xlsx);

                Workbook workbook;
                using (MemoryStream ms = new MemoryStream(xlsx))
                {
                    workbook = new Workbook(ms, loadOptions);
                }

                Worksheet worksheet = workbook.Worksheets[0];
                CheckHeader(names, worksheet);
                CheckRow(names, worksheet, numberOfColumnsOfData);
            }
            catch (Exception ex)
            {
                Assert.Null(ex);
            }
        }

        private static void CheckRow(object[] names, Worksheet worksheet, int numberOfColumnsOfData)
        {
            object[,] cells = worksheet.Cells.ExportArray(1, 0, 1, names.Length);
            int pointer = 0;
            foreach (object cell in cells)
            {
                cell.Should().NotBeNull();
                if (pointer++ == numberOfColumnsOfData)
                {
                    // End of mandatory data
                    break;
                }
            }
        }

        private static void CheckHeader(object[] names, Worksheet worksheet)
        {
            object[,] cells = worksheet.Cells.ExportArray(0, 0, 1, names.Length);
            int column = 0;
            foreach (object cell in cells)
            {
                cell.Should().Be(names[column++]);
            }
        }
    }
}
