﻿using System.Collections.Generic;
using System.Globalization;
using ESFA.DC.ILR1819.ReportService.Interface.Service;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
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
        public System.DateTime GetIlrFileDate(string ilrFilename)
        {
            string[] tokens = ilrFilename.Split('-');
            System.DateTime date = System.DateTime.ParseExact(tokens[3] + "-" + tokens[4], "yyyyMMdd-HHmmss", DateTimeFormatInfo.InvariantInfo);
            return date;
        }
    }
}
