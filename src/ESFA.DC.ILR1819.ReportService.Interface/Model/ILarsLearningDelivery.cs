namespace ESFA.DC.ILR1819.ReportService.Interface.Model
{
    public interface ILarsLearningDelivery
    {
        string LearningAimTitle { get; set; }

        string NotionalNvqLevel { get; set; }

        decimal? Tier2SectorSubjectArea { get; set; }
    }
}
