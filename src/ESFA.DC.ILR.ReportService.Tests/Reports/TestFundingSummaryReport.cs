using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Builders;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Tests.AutoFac;
using ESFA.DC.ILR1819.DataStore.EF;
using ESFA.DC.ILR1819.DataStore.EF.Interface;
using ESFA.DC.ILR1819.DataStore.EF.Valid;
using ESFA.DC.ILR1819.DataStore.EF.Valid.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using LearningDelivery = ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output.LearningDelivery;
using VersionInfo = ESFA.DC.ILR1819.ReportService.Stateless.Configuration.VersionInfo;

namespace ESFA.DC.ILR.ReportService.Tests.Reports
{
    public sealed class TestFundingSummaryReport
    {
        [Fact]
        public async Task TestFundingSummaryReportGeneration()
        {
            string csv = string.Empty;
            byte[] xlsx = null;
            DateTime dateTime = DateTime.UtcNow;
            string filename = $"10033670_1_Funding Summary Report {dateTime:yyyyMMdd-HHmmss}";
            DataStoreConfiguration dataStoreConfiguration = new DataStoreConfiguration()
            {
                ILRDataStoreConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreConnectionString,
                ILRDataStoreValidConnectionString = new TestConfigurationHelper().GetSectionValues<DataStoreConfiguration>("DataStoreSection").ILRDataStoreValidConnectionString
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

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IStreamableKeyValuePersistenceService> redis = new Mock<IStreamableKeyValuePersistenceService>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);

            IOrgProviderService orgProviderService = new OrgProviderService(logger.Object, new OrgConfiguration() { OrgConnectionString = ConfigurationManager.AppSettings["OrgConnectionString"] });

            ILarsProviderService larsProviderService = new LarsProviderService(logger.Object, new LarsConfiguration() { LarsConnectionString = ConfigurationManager.AppSettings["LarsConnectionString"] });

            IEasProviderService easProviderService = new EasProviderService(logger.Object, new EasConfiguration() { EasConnectionString = new TestConfigurationHelper().GetSectionValues<EasConfiguration>("EasSection").EasConnectionString });
            Mock<IPostcodeProviderService> postcodeProverServiceMock = new Mock<IPostcodeProviderService>();
            Mock<ILargeEmployerProviderService> largeEmployerProviderService = new Mock<ILargeEmployerProviderService>();
            IAllbProviderService allbProviderService = new AllbProviderService(logger.Object, redis.Object, jsonSerializationService, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);
            IFM35ProviderService fm35ProviderService = new FM35ProviderService(logger.Object, redis.Object, jsonSerializationService, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);
            IFM25ProviderService fm25ProviderService = new FM25ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrRulebaseContextFactory);
            IFM36ProviderService fm36ProviderService = new FM36ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrRulebaseContextFactory);
            IFM81TrailBlazerProviderService fm81TrailBlazerProviderService = new FM81TrailBlazerProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns("ILR-10033670-1819-20180704-120055-03");
            reportServiceContextMock.SetupGet(x => x.FundingALBOutputKey).Returns("FundingAlbOutput");
            reportServiceContextMock.SetupGet(x => x.FundingFM35OutputKey).Returns("FundingFm35Output");
            reportServiceContextMock.SetupGet(x => x.FundingFM25OutputKey).Returns("FundingFm25Output");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");
            reportServiceContextMock.SetupGet(x => x.CollectionName).Returns("ILR1819");
            reportServiceContextMock.SetupGet(x => x.ReturnPeriod).Returns(12);

            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, dataStoreConfiguration);
            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();
            IVersionInfo versionInfo = new VersionInfo { ServiceReleaseVersion = "1.2.3.4.5" };
            ITotalBuilder totalBuilder = new TotalBuilder();
            IFm25Builder fm25Builder = new Fm25Builder();
            IFm35Builder fm35Builder = new Fm35Builder(totalBuilder, new CacheProviderService<LearningDelivery[]>());
            IFm36Builder fm36Builder = new Fm36Builder(totalBuilder, new CacheProviderService<ILR.FundingService.FM36.FundingOutput.Model.Output.LearningDelivery[]>());
            IFm81Builder fm81Builder = new Fm81Builder(totalBuilder, new CacheProviderService<ILR.FundingService.FM81.FundingOutput.Model.Output.LearningDelivery[]>());
            IAllbBuilder allbBuilder = new AllbBuilder(ilrProviderService, validLearnersService, allbProviderService, stringUtilitiesService, logger.Object);
            IExcelStyleProvider excelStyleProvider = new ExcelStyleProvider();

            IEasBuilder easBuilder = new EasBuilder(easProviderService);

            storage.Setup(x => x.GetAsync("ILR-10033670-1819-20180704-120055-03", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("ILR-10033670-1819-20180704-120055-03.xml").CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.xlsx", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>(
                    (key, value, ct) =>
                    {
                        value.Seek(0, SeekOrigin.Begin);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            value.CopyTo(ms);
                            xlsx = ms.ToArray();
                        }
                    })
                .Returns(Task.CompletedTask);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("FundingALBOutputKey", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("ALBOutput1000.json").CopyTo(sr)).Returns(Task.CompletedTask);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(jsonSerializationService.Serialize(
                new List<string>
                {
                    "3Addl103",
                    "4Addl103"
                }));
            redis.Setup(x => x.GetAsync("FundingFm35Output", It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("Fm35.json").CopyTo(sr)).Returns(Task.CompletedTask);
            redis.Setup(x => x.GetAsync("FundingFm25Output", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("Fm25.json"));
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<DateTime>())).Returns(dateTime);
            largeEmployerProviderService.Setup(x => x.GetVersionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("NA");
            postcodeProverServiceMock.Setup(x => x.GetVersionAsync(It.IsAny<CancellationToken>())).ReturnsAsync("NA");

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();
            IValueProvider valueProvider = new ValueProvider();

            FundingSummaryReport fundingSummaryReport = new FundingSummaryReport(
                storage.Object,
                ilrProviderService,
                orgProviderService,
                allbProviderService,
                fm25ProviderService,
                fm35ProviderService,
                fm36ProviderService,
                fm81TrailBlazerProviderService,
                validLearnersService,
                dateTimeProviderMock.Object,
                valueProvider,
                larsProviderService,
                easProviderService,
                postcodeProverServiceMock.Object,
                largeEmployerProviderService.Object,
                allbBuilder,
                fm25Builder,
                fm35Builder,
                fm36Builder,
                fm81Builder,
                totalBuilder,
                versionInfo,
                excelStyleProvider,
                topicsAndTasks,
                easBuilder,
                logger.Object);

            await fundingSummaryReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            xlsx.Should().NotBeNullOrEmpty();

#if DEBUG
            File.WriteAllBytes($"{filename}.xlsx", xlsx);
#endif

            //var fundingSummaryHeaderMapper = new FundingSummaryHeaderMapper();
            //var fundingSummaryMapper = new FundingSummaryMapper();
            //var fundingSummaryFooterMapper = new FundingSummaryFooterMapper();
            //TestCsvHelper.CheckCsv(
            //    csv,
            //    new CsvEntry(fundingSummaryHeaderMapper, 1),
            //    new CsvEntry(fundingSummaryMapper, 0, "16-18 Traineeships Budget", 1),
            //    new CsvEntry(fundingSummaryMapper, 3, "16-18 Traineeships"),
            //    new CsvEntry(fundingSummaryMapper, 1, "16-18 Trailblazer Apprenticeships for starts before 1 May 2017", 1),
            //    new CsvEntry(fundingSummaryMapper, 0, "Advanced Loans Bursary Budget", 1),
            //    new CsvEntry(fundingSummaryMapper, 3, "Advanced Loans Bursary"),
            //    new CsvEntry(fundingSummaryMapper, 0, Constants.ALBInfoText),
            //    new CsvEntry(fundingSummaryFooterMapper, 1, blankRowsBefore: 1));

            //var fundingSummaryHeaderMapper = new FundingSummaryHeaderMapper();
            //var fundingSummaryMapper = new FundingSummaryMapper();
            //TestXlsxHelper.CheckXlsxWithTitles(
            //    xlsx,
            //    new XlsxEntry(fundingSummaryHeaderMapper, fundingSummaryHeaderMapper.GetMaxIndex(), true),
            //    new XlsxEntry(fundingSummaryMapper, 0, "16-18 Traineeships Budget", 1),
            //    new XlsxEntry(fundingSummaryMapper, 13, "16-18 Traineeships", 0),
            //    new XlsxEntry(fundingSummaryMapper, 0, "Carry-in Apprenticeships Budget (for starts before 1 May 2017 and non-procured delivery)", 1),
            //    new XlsxEntry(fundingSummaryMapper, 9, "16-18 Apprenticeship Frameworks for starts before 1 May 2017", 1),
            //    new XlsxEntry(fundingSummaryMapper, 9, "16-18 Trailblazer Apprenticeships for starts before 1 May 2017", 1),
            //    new XlsxEntry(fundingSummaryMapper, 18, "16-18 Non-Levy Contracted Apprenticeships - Non-procured delivery", 1),
            //    new XlsxEntry(fundingSummaryMapper, 9, "19-23 Apprenticeship Frameworks for starts before 1 May 2017", 1),
            //    new XlsxEntry(fundingSummaryMapper, 9, "19-23 Trailblazer Apprenticeships for starts before 1 May 2017", 0),
            //    new XlsxEntry(fundingSummaryMapper, 9, "24+ Apprenticeship Frameworks for starts before 1 May 2017", 1));
        }
    }
}