using System;

namespace ESFA.DC.ILR.ReportService.Service.Extensions
{
    public static class DateTimeExtensions
    {
        public static int DaysInMonth(this DateTime value)
        {
            return DateTime.DaysInMonth(value.Year, value.Month);
        }

        public static DateTime LastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.DaysInMonth());
        }
    }
}
