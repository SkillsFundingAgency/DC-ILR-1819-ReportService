using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR1819.ReportService.Interface.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface.Reports;
using ESFA.DC.ILR1819.ReportService.Interface.Service;
using ESFA.DC.ILR1819.ReportService.Service.Mapper;
using ESFA.DC.ILR1819.ReportService.Service.Reports;
using ESFA.DC.ILR1819.ReportService.Service.Service;
using ESFA.DC.ILR1819.ReportService.Tests.AutoFac;
using ESFA.DC.ILR1819.ReportService.Tests.Helpers;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobContext;
using ESFA.DC.JobContext.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using ESFA.DC.Serialization.Json;
using ESFA.DC.Serialization.Xml;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR1819.ReportService.Tests.Reports
{
    public sealed class TestValidationReport
    {
        [Fact]
        public async Task TestValidationReportGeneration()
        {
            string csv = string.Empty;
            string json = string.Empty;
            byte[] xlsx = null;
            System.DateTime dateTime = System.DateTime.UtcNow;
            string filename = $"10033670_1_Validation Errors Report {dateTime:yyyyMMdd-HHmmss}";

            Mock<ILogger> logger = new Mock<ILogger>();
            Mock<IStreamableKeyValuePersistenceService> storage = new Mock<IStreamableKeyValuePersistenceService>();
            Mock<IKeyValuePersistenceService> redis = new Mock<IKeyValuePersistenceService>();
            IXmlSerializationService xmlSerializationService = new XmlSerializationService();
            IJsonSerializationService jsonSerializationService = new JsonSerializationService();
            Mock<IDateTimeProvider> dateTimeProviderMock = new Mock<IDateTimeProvider>();

            storage.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("ILR-10033670-1819-20180712-144437-03.xml"));
            storage.Setup(x => x.SaveAsync($"{filename}.csv", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => csv = value).Returns(Task.CompletedTask);
            storage.Setup(x => x.SaveAsync($"{filename}.json", It.IsAny<string>(), It.IsAny<CancellationToken>())).Callback<string, string, CancellationToken>((key, value, ct) => json = value).Returns(Task.CompletedTask);
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
            redis.Setup(x => x.GetAsync("ValidationErrors", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("ValidationErrors.json"));
            redis.Setup(x => x.GetAsync("ValidationErrorsLookup", It.IsAny<CancellationToken>())).ReturnsAsync(File.ReadAllText("ValidationErrorsLookup.json"));
            dateTimeProviderMock.Setup(x => x.GetNowUtc()).Returns(dateTime);
            dateTimeProviderMock.Setup(x => x.ConvertUtcToUk(It.IsAny<System.DateTime>())).Returns(dateTime);

            IIlrProviderService ilrProviderService = new IlrProviderService(logger.Object, storage.Object, xmlSerializationService);

            ITopicAndTaskSectionOptions topicsAndTasks = TestConfigurationHelper.GetTopicsAndTasks();

            IReport validationErrorsReport = new ValidationErrorsReport(
                logger.Object,
                redis.Object,
                storage.Object,
                jsonSerializationService,
                ilrProviderService,
                dateTimeProviderMock.Object,
                topicsAndTasks);

            IJobContextMessage jobContextMessage = new JobContextMessage(1, new ITopicItem[0], 0, System.DateTime.UtcNow);
            jobContextMessage.KeyValuePairs[JobContextMessageKey.UkPrn] = "10033670";
            jobContextMessage.KeyValuePairs[JobContextMessageKey.Filename] = "ILR-10033670-1819-20180712-144437-03";
            jobContextMessage.KeyValuePairs[JobContextMessageKey.ValidationErrors] = "ValidationErrors";
            jobContextMessage.KeyValuePairs[JobContextMessageKey.ValidationErrorLookups] = "ValidationErrorsLookup";

            await validationErrorsReport.GenerateReport(jobContextMessage, null, CancellationToken.None);

            json.Should().NotBeNullOrEmpty();
            csv.Should().NotBeNullOrEmpty();
            xlsx.Should().NotBeNullOrEmpty();

            ValidationErrorMapper helper = new ValidationErrorMapper();
            TestCsvHelper.CheckCsv(csv, helper);
            TestXlsxHelper.CheckXlsx(xlsx, helper, 6);
        }
    }
}
