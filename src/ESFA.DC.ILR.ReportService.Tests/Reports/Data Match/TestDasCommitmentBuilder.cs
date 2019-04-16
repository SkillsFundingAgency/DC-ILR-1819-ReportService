using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.Data.DAS.Model;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Model.DasCommitments;
using ESFA.DC.ILR.ReportService.Service.Builders;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Reports.Data_Match
{
    public sealed class TestDasCommitmentBuilder
    {
#if DEBUG
        [Fact]
#endif
        public async Task TestDasCommitmentBuilderBuild()
        {
            List<DasCommitments> dasCommitments = new List<DasCommitments>
            {
                new DasCommitments
                {
                    CommitmentId = 1,
                    VersionId = "2"
                },
                new DasCommitments
                {
                    CommitmentId = 1,
                    VersionId = "2"
                }
            };

            IDasCommitmentBuilder dasCommitmentBuilder = new DasCommitmentBuilder();
            List<DasCommitment> commitments = dasCommitmentBuilder.Build(dasCommitments);

            Assert.Equal(2, commitments.Count);
            Assert.True(commitments[0].IsVersioned);
        }
    }
}
