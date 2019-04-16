namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface IValidationStageOutputCache
    {
        int DataMatchProblemCount { get; set; }

        int DataMatchProblemLearnersCount { get; set; }
    }
}
