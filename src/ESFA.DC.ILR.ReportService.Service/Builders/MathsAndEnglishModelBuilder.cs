using System;
using System.Linq;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ReportService.Interface.Service;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Builders
{
    public class MathsAndEnglishModelBuilder : IMathsAndEnglishModelBuilder
    {
        public MathsAndEnglishModel BuildModel(ILearner learner, FM25Learner fm25Data)
        {
            return new MathsAndEnglishModel
            {
                FundLine = fm25Data.FundLine,
                LearnRefNumber = learner.LearnRefNumber,
                FamilyName = learner.FamilyName,
                GivenNames = learner.GivenNames,
                DateOfBirth = learner.DateOfBirthNullable?.ToString("dd/MM/yyyy"),
                CampId = learner.CampId,
                ConditionOfFundingMaths = fm25Data.ConditionOfFundingMaths,
                ConditionOfFundingEnglish = fm25Data.ConditionOfFundingEnglish,
                RateBand = fm25Data.RateBand,
                ProvSpecLearnMonA = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                ProvSpecLearnMonB = learner.ProviderSpecLearnerMonitorings
                    ?.SingleOrDefault(x => string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                LearnerStartDate = fm25Data.LearnerStartDate
            };
        }
    }
}
