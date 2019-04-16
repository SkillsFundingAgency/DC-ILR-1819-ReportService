using System;
using ESFA.DC.ILR.ReportService.Model.Styling;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class AdultFundingClaimModel : ICloneable
    {
        //Header
        public string ProviderName { get; set; }

        public int Ukprn { get; set; }

        public string IlrFile { get; set; }

        public string Year { get; set; }

        // Body
        public decimal OtherLearningProgrammeFunding6Months { get; set; }

        public decimal OtherLearningProgrammeFunding12Months { get; set; }

        public decimal OtherLearningLearningSupport6Months { get; set; }

        public decimal OtherLearningLearningSupport12Months { get; set; }

        public decimal Traineeships1924ProgrammeFunding6Months {get;set;}

        public decimal Traineeships1924ProgrammeFunding12Months {get;set;}

        public decimal Traineeships1924LearningSupport6Months { get; set; }

        public decimal Traineeships1924LearningSupport12Months { get; set; }

        public decimal Traineeships1924LearnerSupport6Months { get; set; }

        public decimal Traineeships1924LearnerSupport12Months { get; set; }

        public decimal LoansBursaryFunding6Months { get; set; }

        public decimal LoansBursaryFunding12Months { get; set; }

        public decimal LoansAreaCosts6Months { get; set; }

        public decimal LoansAreaCosts12Months { get; set; }

        public decimal LoansExcessSupport6Months { get; set; }

        public decimal LoansExcessSupport12Months { get; set; }

        
        // Footer

        public string ComponentSetVersion { get; set; }

        public string ApplicationVersion { get; set; }

        public string FilePreparationDate { get; set; }

        public string LarsData { get; set; }

        public string PostcodeData { get; set; }

        public string OrganisationData { get; set; }

        public string LargeEmployerData { get; set; }

        public string ReportGeneratedAt { get; set; }

        /// <summary>
        /// Shallow copies this model (which is enough as it should only have value types)
        /// </summary>
        /// <returns>A shallow copy of this object.</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}