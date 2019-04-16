using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Interface.Service;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public sealed class PeriodProviderService : IPeriodProviderService
    {
        private readonly Dictionary<int, int> CollectionToMonthDictionary = new Dictionary<int, int>
        {
            { 6, 1 },
            { 7, 2 },
            { 8, 3 },
            { 9, 4 },
            { 10, 5 },
            { 11, 6 },
            { 12, 7 },
            { 1, 8 },
            { 2, 9 },
            { 3, 10 },
            { 4, 11 },
            { 5, 12 }
        };

        public int MonthFromPeriod(int period)
        {
            return CollectionToMonthDictionary[period];
        }
    }
}
