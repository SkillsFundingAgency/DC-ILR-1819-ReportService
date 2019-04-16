using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ReportService.Tests.Service
{
    public sealed class FM35ProviderServiceTest
    {
        [Fact]
        public async Task GetFM35DataTest()
        {
            string fm35OutputKey = "FundingFm35OutputEmpty.json";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IStreamableKeyValuePersistenceService> redis = new Mock<IStreamableKeyValuePersistenceService>();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            DataStoreConfiguration dataStoreConfiguration = new DataStoreConfiguration()
            {
                ILRDataStoreConnectionString = string.Empty,
                ILRDataStoreValidConnectionString = string.Empty
            };

            IIlr1819ValidContext IlrValidContextFactory()
            {
                var options = new DbContextOptionsBuilder<ILR1819_DataStoreEntitiesValid>().UseSqlServer(dataStoreConfiguration.ILRDataStoreValidConnectionString).Options;
                return new ILR1819_DataStoreEntitiesValid(options);
            }

            IIlr1819RulebaseContext IlrRulebaseContextFactory()
            {
                var options = new DbContextOptionsBuilder<ILR1819_DataStoreEntities>().UseSqlServer(dataStoreConfiguration.ILRDataStoreConnectionString).Options;
                return new ILR1819_DataStoreEntities(options);
            }

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns("ILR-10033670-1819-20180704-120055-03");
            reportServiceContextMock.SetupGet(x => x.FundingFM35OutputKey).Returns(fm35OutputKey);
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");

            redis.Setup(x => x.ContainsAsync(fm35OutputKey, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync(fm35OutputKey, It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead(@"Service\FundingFm35OutputEmpty.json").CopyTo(sr)).Returns(Task.CompletedTask);

            FM35ProviderService fm35ProviderService = new FM35ProviderService(
                logger.Object,
                redis.Object,
                jsonSerializationService,
                intUtilitiesService,
                IlrValidContextFactory,
                IlrRulebaseContextFactory);

            FM35Global fm35Data = await fm35ProviderService.GetFM35Data(reportServiceContextMock.Object, CancellationToken.None);

            fm35Data.Learners.Should().BeNullOrEmpty();
        }
    }
}
