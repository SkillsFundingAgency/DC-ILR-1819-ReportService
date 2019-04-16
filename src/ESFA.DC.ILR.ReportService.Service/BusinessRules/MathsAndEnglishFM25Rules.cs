using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.ReportService.Interface.Service;

namespace ESFA.DC.ILR.ReportService.Service.BusinessRules
{
    public class MathsAndEnglishFm25Rules : IMathsAndEnglishFm25Rules
    {
        private readonly List<string> _applicableFundLines = new List<string>
        {
            Constants.DirectFundedStudents1416FundLine,
            Constants.Students1619FundLine,
            Constants.HighNeedsStudents1619FundLine,
            Constants.StudentsWithEHCP1924FundLine,
            Constants.ContinuingStudents19PlusFundLine
        };

        /// <summary>
        /// Determines if the learner should be included in the report
        /// </summary>
        /// <param name="learner">The learners FM25 data</param>
        /// <returns>boolean indicating if they should be included or not</returns>
        public bool IsApplicableLearner(FM25Learner learner)
        {
            // BR1 – Applicable Learners
            if (!(learner.StartFund ?? false))
            {
                return false;
            }

            // BR2 – Applicable Funding Line Types
            return _applicableFundLines.Contains(learner.FundLine);
        }
    }
}
