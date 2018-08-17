using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.CollectionsManagement.Services.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.JobContext.Interface;

namespace ESFA.DC.ILR1819.ReportService.Service.Service
{
    public sealed class PeriodProviderService : IPeriodProviderService
    {
        private const string CurrentCollection = "ILR1819";

        private readonly Dictionary<int, int> monthToCollection = new Dictionary<int, int>()
        {
            { 1, 6 },
            { 2, 7 },
            { 3, 8 },
            { 4, 9 },
            { 5, 10 },
            { 6, 11 },
            { 7, 12 },
            { 8, 1 },
            { 9, 2 },
            { 10, 3 },
            { 11, 4 },
            { 12, 5 }
        };

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly IReturnCalendarService _returnCalendarService;

        public PeriodProviderService(IDateTimeProvider dateTimeProvider, IReturnCalendarService returnCalendarService)
        {
            _dateTimeProvider = dateTimeProvider;
            _returnCalendarService = returnCalendarService;
        }

        public async Task<int> GetPeriod(IJobContextMessage jobContextMessage)
        {
            ReturnPeriod returnPeriod = await _returnCalendarService.GetCurrentPeriodAsync(CurrentCollection);

            System.DateTime dateTimeNowUtc = jobContextMessage.SubmissionDateTimeUtc;
            System.DateTime returnPeriodEndDateTimeUk = _dateTimeProvider.ConvertUtcToUk(returnPeriod.EndDateTimeUtc);
            System.DateTime dateTimeNowUk = _dateTimeProvider.ConvertUtcToUk(dateTimeNowUtc);

            int period;

            if (dateTimeNowUk > returnPeriodEndDateTimeUk)
            {
                period = 12;
            }
            else
            {
                if (dateTimeNowUk.Month == returnPeriodEndDateTimeUk.Month &&
                    dateTimeNowUk.Year == returnPeriodEndDateTimeUk.Year)
                {
                    if (dateTimeNowUk.Day < returnPeriodEndDateTimeUk.Day ||
                        (dateTimeNowUk.Day == returnPeriodEndDateTimeUk.Day &&
                         dateTimeNowUk.Hour < returnPeriodEndDateTimeUk.Hour &&
                         dateTimeNowUk.Minute < returnPeriodEndDateTimeUk.Minute))
                    {
                        period = monthToCollection[dateTimeNowUk.Month - 1];
                    }
                    else
                    {
                        period = monthToCollection[dateTimeNowUk.Month];
                    }
                }
                else
                {
                    period = monthToCollection[dateTimeNowUk.Month];
                }
            }

            return period;
        }
    }
}
