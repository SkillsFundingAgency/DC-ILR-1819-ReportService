namespace ESFA.DC.ILR.ReportService.Interface.Configuration
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

        string TopicReports_TaskGenerateAdultFundingClaimReport { get; set; }

        string TopicDeds_TaskPersistDataToDeds { get; set; }

        string TopicReports_TaskGenerateMainOccupancyReport { get; set; }

        string TopicReports_TaskGenerateSummaryOfFunding1619Report { get; set; }

        string TopicReports_TaskGenerateMathsAndEnglishReport { get; set; }

        string TopicReports_TaskGenerateSummaryOfFM35FundingReport { get; set; }

        string TopicReports_TaskGenerateAppsAdditionalPaymentsReport { get; set; }

        string TopicReports_TaskGenerateAppsIndicativeEarningsReport { get; set; }

        string TopicReports_TaskGenerateAppsCoInvestmentContributionsReport { get; set; }

        string TopicReports_TaskGenerateAppsMonthlyPaymentReport { get; set; }

        string TopicReports_TaskGenerateAppsDataMatchMonthEndReport { get; set; }

        string TopicReports_TaskGenerateFundingSummaryPeriodEndReport { get; set; }

        string TopicReports_TaskGenerateDataMatchReport { get; set; }

        string TopicReports_TaskGenerateTrailblazerEmployerIncentivesReport { get; set; }

        string TopicReports_TaskGenerateFundingClaim1619Report { get; set; }

        string TopicReports_TaskGenerateTrailblazerAppsOccupancyReport { get; set; }

        string TopicReports_TaskGenerateHNSReport { get; set; }
    }
}
