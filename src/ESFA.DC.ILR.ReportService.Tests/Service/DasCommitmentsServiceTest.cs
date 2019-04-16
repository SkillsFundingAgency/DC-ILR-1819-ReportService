using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.Service.DataMatch;
using ESFA.DC.Logging.Interfaces;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Service
{
    public sealed class DasCommitmentsServiceTest
    {
        [Fact]
        public async Task DasCommitmentsServiceDbTest()
        {
            Mock<ILogger> logger = new Mock<ILogger>();

            DasCommitmentsService dasCommitmentsService = new DasCommitmentsService(
                new DasCommitmentsConfiguration()
                {
                    DasCommitmentsConnectionString = ConfigurationManager.AppSettings["DASCommitmentsConnectionString"]
                },
                new DasCommitmentBuilder(),
                logger.Object);

            await dasCommitmentsService.GetCommitments(
                10000534,
                new List<long>
                {
                    1111112345
                },
                CancellationToken.None);
        }
    }
}
