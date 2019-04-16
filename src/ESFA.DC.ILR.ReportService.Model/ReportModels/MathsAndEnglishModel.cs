using System;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class MathsAndEnglishModel
    {
        public string FundLine { get; set; }

        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public string DateOfBirth { get; set; }

        public string CampId { get; set; }

        public string ConditionOfFundingMaths { get; set; }

        public string ConditionOfFundingEnglish { get; set; }

        public string RateBand { get; set; }

        public string OfficialSensitive { get; }

        public DateTime? LearnerStartDate { get; set; }

        public string ProvSpecLearnMonA { get; set; }

        public string ProvSpecLearnMonB { get; set; }
    }
}
