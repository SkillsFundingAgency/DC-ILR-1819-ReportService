using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM35.FundingOutput.Model.Output;

namespace ESFA.DC.ILR.ReportService.Tests.Helpers
{
    public static class TestFM35Builder
    {
        public static FM35Global Build()
        {
            return new FM35Global
            {
                Learners = new List<FM35Learner>
                {
                    new FM35Learner
                    {
                        LearnRefNumber = "3fm9901",
                        LearningDeliveries = new List<LearningDelivery>
                        {
                            new LearningDelivery
                            {
                                AimSeqNumber = 1,
                                LearningDeliveryValue = new LearningDeliveryValue
                                {
                                    FundLine = "12345"
                                },
                                LearningDeliveryPeriodisedValues = new List<LearningDeliveryPeriodisedValue>
                                {
                                    new LearningDeliveryPeriodisedValue
                                    {
                                        AttributeName = "OnProgPayment",
                                        Period1 = 102.45M
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
