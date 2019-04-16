using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Generation;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Service;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Service
{
    public sealed class ValueProviderServiceTest
    {
        public decimal TestDecimal { get; set; }

        public decimal? TestNullableDecimal { get; set; }

        public string TestDateTime { get; set; }

        public int TestInt { get; set; }

        [Fact]
        public void DecimalPointsPaddingTest()
        {
            TestDecimal = (decimal)12.34;
            List<object> expected = new List<object> { "12.34000" };
            List<object> values = new List<object>();
            MainOccupancyMapper mapper = new MainOccupancyMapper();

            IValueProvider valueProvider = new ValueProvider();
            valueProvider.GetFormattedValue(
                values,
                TestDecimal,
                mapper,
                new ModelProperty(
                    mapper.MemberMaps.Single(x => string.Equals(x.Data.Member.Name, nameof(MainOccupancyModel.TotalEarnedCash), StringComparison.Ordinal)).Data.Names.Names.ToArray(),
                    GetType().GetProperty(nameof(TestDecimal))));

            values.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ZeroDecimalPointsPaddingTest()
        {
            TestDecimal = 0;
            List<object> expected = new List<object> { "0.00" };
            List<object> values = new List<object>();
            FundingSummaryMapper mapper = new FundingSummaryMapper();

            IValueProvider valueProvider = new ValueProvider();
            valueProvider.GetFormattedValue(
                values,
                TestDecimal,
                mapper,
                new ModelProperty(
                    mapper.MemberMaps.Single(x => string.Equals(x.Data.Member.Name, nameof(FundingSummaryModel.Total), StringComparison.Ordinal)).Data.Names.Names.ToArray(),
                    GetType().GetProperty(nameof(TestDecimal))));

            values.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void NullDecimalPointsPaddingTest()
        {
            TestNullableDecimal = null;
            List<object> expected = new List<object> { "0.00000" };
            List<object> values = new List<object>();
            MainOccupancyMapper mapper = new MainOccupancyMapper();

            IValueProvider valueProvider = new ValueProvider();
            valueProvider.GetFormattedValue(
                values,
                TestNullableDecimal,
                mapper,
                new ModelProperty(
                    mapper.MemberMaps.Single(x => string.Equals(x.Data.Member.Name, nameof(MainOccupancyModel.TotalEarnedCash), StringComparison.Ordinal)).Data.Names.Names.ToArray(),
                    GetType().GetProperty(nameof(TestNullableDecimal))));

            values.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void NullCoercedDateTimeTest()
        {
            TestDateTime = DateTime.MinValue.ToString("dd/MM/yyyy");

            List<object> expected = new List<object> { string.Empty };
            List<object> values = new List<object>();
            MainOccupancyMapper mapper = new MainOccupancyMapper();

            IValueProvider valueProvider = new ValueProvider();
            valueProvider.GetFormattedValue(
                values,
                TestDateTime,
                mapper,
                new ModelProperty(
                    mapper.MemberMaps.Single(x => string.Equals(x.Data.Member.Name, nameof(MainOccupancyModel.DateOfBirth), StringComparison.Ordinal)).Data.Names.Names.ToArray(),
                    GetType().GetProperty(nameof(TestDateTime))));

            values.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ZeroIntTest()
        {
            TestInt = 0;

            List<object> expected = new List<object> { string.Empty };
            List<object> values = new List<object>();
            MainOccupancyMapper mapper = new MainOccupancyMapper();

            IValueProvider valueProvider = new ValueProvider();
            valueProvider.GetFormattedValue(
                values,
                TestInt,
                mapper,
                new ModelProperty(
                    mapper.MemberMaps.Single(x => string.Equals(x.Data.Member.Name, nameof(MainOccupancyModel.CompStatus), StringComparison.Ordinal)).Data.Names.Names.ToArray(),
                    GetType().GetProperty(nameof(TestInt))));

            values.Should().BeEquivalentTo(expected);
        }
    }
}
