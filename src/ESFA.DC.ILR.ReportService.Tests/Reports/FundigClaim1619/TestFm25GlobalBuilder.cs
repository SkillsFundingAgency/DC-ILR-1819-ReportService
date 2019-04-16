using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;

namespace ESFA.DC.ILR.ReportService.Tests.Reports.FundigClaim1619
{
    public static class TestFm25GlobalBuilder
    {
        public static FM25Global BuildTestModel()
        {
            var fm25GlobalObject = new FM25Global();

            fm25GlobalObject.LARSVersion = "Lars - 1.0.0.0";
            fm25GlobalObject.OrgVersion = "Org - 1.0.0.0";
            fm25GlobalObject.PostcodeDisadvantageVersion = "Postcode - 1.0.0.0";
            fm25GlobalObject.RulebaseVersion = "Rulebase - 1.0.0.0";
            fm25GlobalObject.UKPRN = 10033670;

            var learners = new List<FM25Learner>();
            learners.Add(
                new FM25Learner
                {
                    LearnRefNumber = "0fm2501",
                    FundLine = "19-24 Students with an EHCP",
                    StartFund = true,
                    AreaCostFact1618Hist = 10,
                    ProgWeightHist = 10,
                    PrvDisadvPropnHist = 10,
                    PrvHistLrgProgPropn = 10,
                    PrvRetentFactHist = 10,
                    RateBand = "540+ hours (Band 5)",
                    OnProgPayment = 500
                });

            fm25GlobalObject.Learners = learners;

            return fm25GlobalObject;
        }
    }
}