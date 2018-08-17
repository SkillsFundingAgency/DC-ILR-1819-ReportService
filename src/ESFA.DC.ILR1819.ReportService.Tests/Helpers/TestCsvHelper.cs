using System;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;
using FluentAssertions;
using Microsoft.VisualBasic.FileIO;
using Xunit;

namespace ESFA.DC.ILR1819.ReportService.Tests.Helpers
{
    public static class TestCsvHelper
    {
        public static void CheckCsv(string csv, ClassMap classMap)
        {
            try
            {
                using (TextReader textReader = new StringReader(csv))
                {
                    using (TextFieldParser textFieldParser = new TextFieldParser(textReader))
                    {
                        textFieldParser.TextFieldType = FieldType.Delimited;
                        textFieldParser.Delimiters = new[] { "," };
                        string[] currentRow = textFieldParser.ReadFields();
                        CheckHeader(currentRow, classMap);
                        currentRow = textFieldParser.ReadFields();
                        CheckRow(currentRow, classMap);
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Null(ex);
            }
        }

        private static void CheckRow(string[] currentRow, ClassMap classMap)
        {
            currentRow.Length.Should().Be(classMap.MemberMaps.Count);
        }

        private static void CheckHeader(string[] currentRow, ClassMap classMap)
        {
            string[] names = classMap.MemberMaps.OrderBy(x => x.Data.Index).Select(x => x.Data.Names[0]).ToArray();
            currentRow.Should().BeEquivalentTo(names);
        }
    }
}
