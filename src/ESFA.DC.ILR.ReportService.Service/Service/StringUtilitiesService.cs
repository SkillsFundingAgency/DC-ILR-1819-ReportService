using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Service;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public sealed class StringUtilitiesService : IStringUtilitiesService
    {
        public string JoinWithMaxLength(IEnumerable<string> list)
        {
            string ret = string.Join(", ", list);

            if (ret.Length > 200)
            {
                ret = ret.Substring(0, 200) + ";(message was truncated)";
            }

            return ret;
        }

        // ILR-90000064-1819-20180619-120127-02.xml
        public DateTime? GetIlrFileDate(string ilrFilename)
        {
            string[] tokens = ilrFilename.Split('-');

            return DateTime.TryParseExact(
                tokens[3] + "-" + tokens[4],
                "yyyyMMdd-HHmmss",
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out var date) ? date : (DateTime?)null;
        }

        public int TryGetInt(string value, int def)
        {
            if (string.IsNullOrEmpty(value))
            {
                return def;
            }

            if (!int.TryParse(value, out int res))
            {
                return def;
            }

            return res;
        }

        public string GetDateTimeAsString(DateTime? dateTime, string def, DateTime? replace = null)
        {
            if (dateTime == null)
            {
                return def;
            }

            if (replace != null && dateTime.Value == replace.Value)
            {
                return def;
            }

            return dateTime.Value.ToString("dd/MM/yyyy");
        }

        public string[] GetArrayEntries(IEnumerable<ILearningDeliveryFAM> availableValues, int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size), $"{nameof(size)} should be greater than 0");
            }

            string[] values = new string[size];
            int pointer = 0;
            foreach (ILearningDeliveryFAM learningDeliveryFam in availableValues ?? Enumerable.Empty<ILearningDeliveryFAM>())
            {
                values[pointer++] = learningDeliveryFam.LearnDelFAMCode;
                if (pointer == size)
                {
                    break;
                }
            }

            return values;
        }
    }
}
