namespace ESFA.DC.ILR1819.ReportService.Interface.Configuration
{
    public interface ITopicAndTaskSectionOptions
    {
        string TopicValidation { get; set; }

        string TopicFunding { get; set; }

        string TopicDeds { get; set; }

        string TopicReports { get; set; }

        string TopicReports_TaskGenerateValidationReport { get; set; }

        string TopicReports_TaskGenerateAllbOccupancyReport { get; set; }

        string TopicReports_TaskGenerateFundingSummaryReport { get; set; }

        string TopicDeds_TaskPersistDataToDeds { get; set; }

        string TopicReports_TaskGenerateMainOccupancyReport { get; set; }

        string TopicReports_TaskGenerateSummaryOfFunding1619Report { get; set; }

        string TopicReports_TaskGenerateMathsAndEnglishReport { get; set; }
    }
}
