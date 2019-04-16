using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public sealed class HNSModel
    {
        public string FundLine { get; set; }

        public string LearnRefNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public string CampId { get; set; }

        public string LearnerFAM_A { get; set; }

        public string LearnerFAM_B { get; set; }

        public string LearnerFAM_C { get; set; }

        public string LearnerFAM_D { get; set; }

        public string LearnerFAM_E { get; set; }

        public string OfficalSensitive { get; }
    }
}
