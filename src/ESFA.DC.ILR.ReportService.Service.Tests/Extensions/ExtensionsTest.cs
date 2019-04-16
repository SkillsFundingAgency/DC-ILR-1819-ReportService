using System;
using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Service.Extensions;
using FluentAssertions;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Service.Tests.Extensions
{
    public sealed class ExtensionsTest
    {
        [Fact]
        public void TestDistinctByCount()
        {
            IEnumerable<Tuple<string>> tokens = new List<Tuple<string>>()
            {
                new Tuple<string>("A"),
                new Tuple<string>("B"),
                new Tuple<string>(string.Empty),
                new Tuple<string>("C"),
                new Tuple<string>("A"),
                new Tuple<string>("D"),
            };

            tokens.DistinctByCount(x => x.Item1).Should().Be(4);
        }
    }
}
