using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ReportService.Interface.Configuration;
using ESFA.DC.ILR.ReportService.Interface.Context;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.Configuration;
using ESFA.DC.ILR.ReportService.Model.ReportModels;
using ESFA.DC.ILR.ReportService.Service.Builders;
using ESFA.DC.ILR.ReportService.Service.BusinessRules;
using ESFA.DC.ILR.ReportService.Service.Comparer;
using ESFA.DC.ILR.ReportService.Service.Mapper;
using ESFA.DC.ILR.ReportService.Service.Reports;
using ESFA.DC.ILR.ReportService.Service.Service;
using ESFA.DC.ILR.ReportService.Tests.AutoFac;
using ESFA.DC.ILR.ReportService.Tests.Helpers;
using ESFA.DC.ILR.ReportService.Tests.Models;
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

namespace ESFA.DC.ILR.ReportService.Tests.Reports
{
    public sealed class TestMathsAndEnglishReport
    {
        [Fact]
        public async Task TestMathsAndEnglishReportGeneration()
        {
            string csv = string.Empty;
            System.DateTime dateTime = System.DateTime.UtcNow;
            string filename = $"10033670_1_Maths and English Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IKeyValuePersistenceService> redis = new Mock<IKeyValuePersistenceService>();
            IIntUtilitiesService intUtilitiesService = new IntUtilitiesService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
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

            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService, dateTimeProviderMock.Object, intUtilitiesService, IlrValidContextFactory, IlrRulebaseContextFactory);

            Mock<IReportServiceContext> reportServiceContextMock = new Mock<IReportServiceContext>();
            reportServiceContextMock.SetupGet(x => x.JobId).Returns(1);
            reportServiceContextMock.SetupGet(x => x.SubmissionDateTimeUtc).Returns(DateTime.UtcNow);
            reportServiceContextMock.SetupGet(x => x.Ukprn).Returns(10033670);
            reportServiceContextMock.SetupGet(x => x.Filename).Returns("ILR-10033670-1819-20180712-144437-03");
            reportServiceContextMock.SetupGet(x => x.FundingFM25OutputKey).Returns("FundingFm25Output");
            reportServiceContextMock.SetupGet(x => x.ValidLearnRefNumbersKey).Returns("ValidLearnRefNumbers");

            IValidLearnersService validLearnersService = new ValidLearnersService(logger.Object, redis.Object, jsonSerializationService, null);
            IFM25ProviderService fm25ProviderService = new FM25ProviderService(logger.Object, storage.Object, jsonSerializationService, intUtilitiesService, IlrRulebaseContextFactory);
            IStringUtilitiesService stringUtilitiesService = new StringUtilitiesService();
            IMathsAndEnglishFm25Rules reportFm25Rules = new MathsAndEnglishFm25Rules();
            IMathsAndEnglishModelBuilder builder = new MathsAndEnglishModelBuilder();
            IValueProvider valueProvider = new ValueProvider();

            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback<string, Stream, CancellationToken>((st, sr, ct) => File.OpenRead("ILR-10033670-1819-20180712-144437-03.xml").CopyTo(sr)).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            redis.Setup(x => x.GetAsync("ValidLearnRefNumbers", It.IsAny<CancellationToken>())).ReturnsAsync(jsonSerializationService.Serialize(
                new List<string>
                {
                    "0fm2501",
                    "3fm9901",
                    "5fm9901"
                }));
            redis.Setup(x => x.GetAsync("FundingFm25Output", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("Fm25.json"));
            redis.Setup(x => x.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<System.DateTime>())).Returns(dateTime);

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();

            var mathsAndEnglishReport = new MathsAndEnglishReport(
                logger.Object,
                storage.Object,
                ilrProviderService,
                validLearnersService,
                fm25ProviderService,
                stringUtilitiesService,
                dateTimeProviderMock.Object,
                valueProvider,
                reportFm25Rules,
                builder,
                topicsAndTasks);

            await mathsAndEnglishReport.GenerateReport(reportServiceContextMock.Object, null, false, CancellationToken.None);

            csv.Should().NotBeNullOrEmpty();
            TestCsvHelper.CheckCsv(csv, new CsvEntry(new MathsAndEnglishMapper(), 1));
        }

        [Fact]
        public async Task TestMathsAndEnglishComparer_ShouldSortModels_VerifyLearnRefNumber()
        {
            List<MathsAndEnglishModel> mathsAndEnglishModels = new List<MathsAndEnglishModel>
            {
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "321",
                    ConditionOfFundingEnglish = "A",
                    RateBand = "A"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "123",
                    ConditionOfFundingEnglish = "B",
                    RateBand = "B"
                }
            };

            mathsAndEnglishModels.Sort(new MathsAndEnglishModelComparer());
            Assert.Equal("123", mathsAndEnglishModels[0].LearnRefNumber);
        }

        [Fact]
        public async Task TestMathsAndEnglishComparer_ShouldSortModels_VerifyFundLineType()
        {
            List<MathsAndEnglishModel> mathsAndEnglishModels = new List<MathsAndEnglishModel>
            {
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "321",
                    FundLine = "19+ Continuing Students (excluding EHCP)",
                    RateBand = "A"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "123",
                    FundLine = "19-24 Students with an EHCP",
                    RateBand = "B"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "321",
                    FundLine = "16-19 Students (excluding High Needs Students)",
                    RateBand = "A"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "123",
                    FundLine = "14-16 Direct Funded Students",
                    RateBand = "B"
                }
            };

            mathsAndEnglishModels.Sort(new MathsAndEnglishModelComparer());
            Assert.Equal("14-16 Direct Funded Students", mathsAndEnglishModels[0].FundLine);
        }

        [Fact]
        public async Task TestMathsAndEnglishComparer_ShouldSortModels_VerifySortOrder()
        {
            List<MathsAndEnglishModel> mathsAndEnglishModels = new List<MathsAndEnglishModel>
            {
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "321",
                    FundLine = "19+ Continuing Students (excluding EHCP)",
                    RateBand = "A"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "123",
                    FundLine = "19-24 Students with an EHCP",
                    RateBand = "B"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "321",
                    FundLine = "19-24 Students with an EHCP",
                    RateBand = "A"
                },
                new MathsAndEnglishModel()
                {
                    LearnRefNumber = "123",
                    FundLine = "19+ Continuing Students (excluding EHCP)",
                    RateBand = "B"
                }
            };

            mathsAndEnglishModels.Sort(new MathsAndEnglishModelComparer());
            Assert.Equal("19-24 Students with an EHCP", mathsAndEnglishModels[0].FundLine);
            Assert.Equal("123", mathsAndEnglishModels[0].LearnRefNumber);
        }
    }
}