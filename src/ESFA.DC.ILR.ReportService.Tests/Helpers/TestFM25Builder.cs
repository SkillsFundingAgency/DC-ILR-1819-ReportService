using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM25.Model.Output;
using ESFA.DC.ILR.ReportService.Service;

namespace ESFA.DC.ILR.ReportService.Tests.Helpers
{
    public static class TestFM25Builder
    {
        public static FM25Global Build()
        {
            return new FM25Global
            {
                LARSVersion = "1",
                OrgVersion = "2",
                PostcodeDisadvantageVersion = "3",
                RulebaseVersion = "4",
                UKPRN = 123456,
                Learners = new List<FM25Learner>
                {
                    new FM25Learner
                    {
                        AcadMonthPayment = 0,
                        AcadProg = false,
                        ActualDaysILCurrYear = 4,
                        AreaCostFact1618Hist = 12M,
                        Block1DisadvUpliftNew = 0,
                        Block2DisadvElementsNew = 0,
                        ConditionOfFundingEnglish = string.Empty,
                        ConditionOfFundingMaths = string.Empty,
                        CoreAimSeqNumber = 0,
                        FullTimeEquiv = 0,
                        LearnRefNumber = "3fm9901",
                        StartFund = true,
                        FundLine = Constants.Students1619FundLine
                    }
                }
            };
        }
    }
}
