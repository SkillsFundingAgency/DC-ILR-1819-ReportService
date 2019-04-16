using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Service.Service;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Service.Tests
{
    public class PeriodProviderServiceTests
    {
        [Theory]
        [InlineData(6, 1)]
        [InlineData(7, 2)]
        [InlineData(8, 3)]
        [InlineData(9, 4)]
        [InlineData(10, 5)]
        [InlineData(11, 6)]
        [InlineData(12, 7)]
        [InlineData(1, 8)]
        [InlineData(2, 9)]
        [InlineData(3, 10)]
        [InlineData(4, 11)]
        [InlineData(5, 12)]
        public void MonthFromPeriod(int period, int month)
        {
            NewService().MonthFromPeriod(period).Should().Be(month);
        }

        private PeriodProviderService NewService()
        {
            return new PeriodProviderService();
        }
    }
}
