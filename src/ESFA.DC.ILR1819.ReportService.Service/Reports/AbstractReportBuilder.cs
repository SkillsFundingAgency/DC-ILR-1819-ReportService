using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Cells;
using CsvHelper;
using CsvHelper.Configuration;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.Generation;

namespace ESFA.DC.ILR1819.ReportService.Service.Reports
{
    public abstract class AbstractReportBuilder
    {
        protected string ReportFileName;

        private readonly IDateTimeProvider _dateTimeProvider;

        protected AbstractReportBuilder(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public string ReportTaskName { get; set; }

        public bool IsMatch(string reportTaskName)
        {
            return reportTaskName == ReportTaskName;
        }

        public string GetReportFilename(string ukPrn, long jobId)
        {
            DateTime dateTime = _dateTimeProvider.ConvertUtcToUk(_dateTimeProvider.GetNowUtc());
            return $"{ukPrn}_{jobId.ToString()}_{ReportFileName} {dateTime:yyyyMMdd-HHmmss}";
        }

        /// <summary>
        /// Builds a CSV report using the specified mapper as the list of column names.
        /// </summary>
        /// <typeparam name="TMapper">The mapper.</typeparam>
        /// <typeparam name="TModel">The model.</typeparam>
        /// <param name="writer">The memory stream to write to.</param>
        /// <param name="records">The records to persist.</param>
        protected void BuildCsvReport<TMapper, TModel>(MemoryStream writer, IEnumerable<TModel> records)
            where TMapper : ClassMap, IClassMapper
            where TModel : class
        {
            UTF8Encoding utF8Encoding = new UTF8Encoding(false, true);
            using (TextWriter textWriter = new StreamWriter(writer, utF8Encoding, 1024, true))
            {
                using (CsvWriter csvWriter = new CsvWriter(textWriter))
                {
                    csvWriter.Configuration.RegisterClassMap<TMapper>();
                    csvWriter.WriteHeader<TModel>();
                    csvWriter.NextRecord();
                    csvWriter.WriteRecords(records);
                }
            }
        }

        /// <summary>
        /// Builds a CSV report using the specified mapper as the list of column names.
        /// </summary>
        /// <typeparam name="TMapper">The mapper.</typeparam>
        /// <typeparam name="TModel">The model.</typeparam>
        /// <param name="writer">The memory stream to write to.</param>
        /// <param name="record">The record to persist.</param>
        protected void BuildCsvReport<TMapper, TModel>(MemoryStream writer, TModel record)
            where TMapper : ClassMap, IClassMapper
            where TModel : class
        {
            BuildCsvReport<TMapper, TModel>(writer, new[] { record });
        }

        /// <summary>
        /// Builds an Excel report using the specified mapper as the list of column names.
        /// </summary>
        /// <typeparam name="TMapper">The mapper.</typeparam>
        /// <typeparam name="TModel">The model.</typeparam>
        /// <param name="writer">The open memory stream to write to.</param>
        /// <param name="classMap">The class mapper to use to write the headers, and to get the properties references from.</param>
        /// <param name="records">The records to write.</param>
        protected void BuildXlsReport<TMapper, TModel>(MemoryStream writer, TMapper classMap, IEnumerable<TModel> records)
            where TMapper : ClassMap, IClassMapper
            where TModel : class
        {
            ModelProperty[] names = classMap.MemberMaps.OrderBy(x => x.Data.Index).Select(x => new ModelProperty(x.Data.Names[0], (PropertyInfo)x.Data.Member)).ToArray();

            Workbook wb = new Workbook();
            Worksheet sheet = wb.Worksheets[0];
            sheet.Cells.ImportObjectArray(names.Select(x => x.Name).ToArray(), 0, 0, false);
            int row = 1;
            object[] values = new object[names.Length];
            foreach (TModel record in records)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    values[i] = names[i].MethodInfo.GetValue(record);
                }

                sheet.Cells.ImportObjectArray(values, row++, 0, false);
            }

            wb.Save(writer, SaveFormat.Xlsx);
        }

        /// <summary>
        /// Writes the data to the zip file with the specified filename.
        /// </summary>
        /// <param name="archive">Archive to write to.</param>
        /// <param name="filename">Filename to use in zip file.</param>
        /// <param name="data">Data to write.</param>
        /// <returns>Awaitable task.</returns>
        protected async Task WriteZipEntry(ZipArchive archive, string filename, string data)
        {
            if (archive == null)
            {
                return;
            }

            ZipArchiveEntry archivedFile = archive.CreateEntry(filename, CompressionLevel.Optimal);
            using (StreamWriter sw = new StreamWriter(archivedFile.Open()))
            {
                await sw.WriteAsync(data);
            }
        }

        /// <summary>
        /// Writes the stream to the zip file with the specified filename.
        /// </summary>
        /// <param name="archive">Archive to write to.</param>
        /// <param name="filename">Filename to use in zip file.</param>
        /// <param name="data">Data to write.</param>
        /// <param name="cancellationToken">Cancellation token for cancelling copy operation.</param>
        /// <returns>Awaitable task.</returns>
        protected async Task WriteZipEntry(ZipArchive archive, string filename, Stream data, CancellationToken cancellationToken)
        {
            if (archive == null)
            {
                return;
            }

            ZipArchiveEntry archivedFile = archive.CreateEntry(filename, CompressionLevel.Optimal);
            using (Stream sw = archivedFile.Open())
            {
                data.Seek(0, SeekOrigin.Begin);
                await data.CopyToAsync(sw, 81920, cancellationToken);
            }
        }
    }
}
