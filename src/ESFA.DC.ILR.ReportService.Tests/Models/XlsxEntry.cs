using CsvHelper.Configuration;

namespace ESFA.DC.ILR.ReportService.Tests.Models
{
    public sealed class XlsxEntry
    {
        public XlsxEntry(ClassMap mapper, int dataRows, bool pivot = false)
        {
            Mapper = mapper;
            DataRows = dataRows;
            Pivot = pivot;
        }

        public XlsxEntry(ClassMap mapper, int dataRows, string title, int blankRowsBefore)
        {
            Mapper = mapper;
            DataRows = dataRows;
            Title = title;
            BlankRowsBefore = blankRowsBefore;
        }

        public ClassMap Mapper { get; }

        public int DataRows { get; }

        public bool Pivot { get; }

        public string Title { get; }

        public int BlankRowsBefore { get; }
    }
}
