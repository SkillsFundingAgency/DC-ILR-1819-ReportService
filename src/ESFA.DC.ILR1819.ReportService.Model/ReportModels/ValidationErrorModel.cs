using ESFA.DC.ILR.ValidationErrors.Interface.Models;

namespace ESFA.DC.ILR1819.ReportService.Model.ReportModels
{
    public sealed class ValidationErrorModel : ValidationErrorDto
    {
        public ValidationErrorModel(string severity, string learnerReferenceNumber, string ruleName, string fieldValues, string errorMessage, long? aimSequenceNumber)
        {
            Severity = severity;
            LearnerReferenceNumber = learnerReferenceNumber;
            RuleName = ruleName;
            FieldValues = fieldValues;
            ErrorMessage = errorMessage;
            AimSequenceNumber = aimSequenceNumber;
        }

        public ValidationErrorModel(string severity, string learnerReferenceNumber, string ruleName, string fieldValues, string errorMessage, long? aimSequenceNumber, string learnAimRef, string sWSupAimId, int fundModel, int? partnerUkprn, string provSpecLearnMon1, string provSpecLearnMon2, string provSpecDelMon1, string provSpecDelMon2, string provSpecDelMon3, string provSpecDelMon4)
            : this(severity, learnerReferenceNumber, ruleName, fieldValues, errorMessage, aimSequenceNumber)
        {
            LearnAimRef = learnAimRef;
            SWSupAimId = sWSupAimId;
            FundModel = fundModel;
            PartnerUKPRN = partnerUkprn;
            ProviderSpecLearnOccurA = provSpecLearnMon1;
            ProviderSpecLearnOccurB = provSpecLearnMon2;
            ProviderSpecDelOccurA = provSpecDelMon1;
            ProviderSpecDelOccurB = provSpecDelMon2;
            ProviderSpecDelOccurC = provSpecDelMon3;
            ProviderSpecDelOccurD = provSpecDelMon4;
        }

        public string LearnAimRef { get; set; }

        public string SWSupAimId { get; set; }

        public int FundModel { get; set; }

        public int? PartnerUKPRN { get; set; }

        public string ProviderSpecLearnOccurA { get; set; }

        public string ProviderSpecLearnOccurB { get; set; }

        public string ProviderSpecDelOccurA { get; set; }

        public string ProviderSpecDelOccurB { get; set; }

        public string ProviderSpecDelOccurC { get; set; }

        public string ProviderSpecDelOccurD { get; set; }

        public string SecurityClassification { get; }
    }
}
