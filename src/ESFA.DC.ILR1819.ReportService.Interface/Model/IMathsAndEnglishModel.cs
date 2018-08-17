namespace ESFA.DC.ILR1819.ReportService.Interface.Model
{
    public interface IMathsAndEnglishModel
    {
        string FundLine { get; set; }
        string LearnRefNumber { get; set; }
        string FamilyName { get; set; }
        string GivenNames { get; set; }
        string DateOfBirth { get; set; }
        string CampId { get; set; }
        string ConditionOfFundingMaths { get; set; }
        string ConditionOfFundingEnglish { get; set; }
        string RateBand { get; set; }
        string OfficalSensitive { get; }
    }
}