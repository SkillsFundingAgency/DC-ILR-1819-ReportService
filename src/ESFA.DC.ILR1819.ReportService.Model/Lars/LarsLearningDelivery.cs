using ESFA.DC.ILR1819.ReportService.Interface.Model;

namespace ESFA.DC.ILR1819.ReportService.Model.Lars
{
    public sealed class LarsLearningDelivery : ILarsLearningDelivery
    {
        public string LearningAimTitle { get; set; }

        public string NotionalNvqLevel { get; set; }

        public decimal? Tier2SectorSubjectArea { get; set; }
    }
}
