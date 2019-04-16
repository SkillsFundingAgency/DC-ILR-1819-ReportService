using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.ILR.ReportService.Model.ReportModels
{
    public class TrailblazerAppsOccupancyModel
    {
        public string EpaOrgID { get; set; }
        public int? PartnerUKPRN { get; set; }
        public bool? MathEngLSFFundStart { get; set; }
        public string LearnRefNumber { get; set; }

        public long UniqueLearnerNumber { get; set; }

        public string DateOfBirth { get; set; }

        public int? PMUkPrn { get; set; }

        public string CampId { get; set; }

        public string ProvSpecLearnMonA { get; set; }

        public string ProvSpecLearnMonB { get; set; }

        public int? AimSeqNumber { get; set; }

        public string LearnAimRef { get; set; }

        public string LearnAimRefTitle { get; set; }

        public string SwSupAimId { get; set; }

        public string NotionalNvqLevelV2 { get; set; }

        public int AimType { get; set; }

        public int? StdCode { get; set; }

        public int FundModel { get; set; }

        public int? PriorLearnFundAdj { get; set; }

        public int? OtherFundAdj { get; set; }

        public DateTime? OrigLearnStartDate { get; set; }

        public DateTime LearnStartDate { get; set; }

        public DateTime LearnPlanEndDate { get; set; }

        public int CompStatus { get; set; }

        public DateTime? LearnActEndDate { get; set; }

        public int? Outcome { get; set; }

        public string LearnDelFamCodeSof { get; set; }

        public string LearnDelFamCodeEef { get; set; }

        public string LearnDelFamCodeLsfHighest { get; set; }

        public DateTime? LearnDelFamCodeLsfEarliest { get; set; }

        public DateTime? LearnDelFamCodeLsfLatest { get; set; }

        public string LearnDelMonA { get; set; }

        public string LearnDelMonB { get; set; }

        public string LearnDelMonC { get; set; }

        public string LearnDelMonRestartIndicator { get; set; }

        public string LearnDelMonD { get; set; }

        public string ProvSpecDelMonA { get; set; }

        public string ProvSpecDelMonB { get; set; }

        public string ProvSpecDelMonC { get; set; }

        public string ProvSpecDelMonD { get; set; }

        public string FundLine { get; set; }

        public string OfficialSensitive { get; set; }
        public DateTime? AchDate { get; set; }
        public string DelLocPostCode { get; set; }
        public long? CoreGovContCapApplicVal { get; set; }
        public decimal? SmallBusApplicVal { get; set; }
        public decimal? YoungAppApplicVal { get; set; }
        public decimal? AchievementApplicVal { get; set; }
        public DateTime? ApplicFundValDate { get; set; }
        public int? EmpIdFirstDayStandard { get; set; }
        public int? EmpIdSmallBusDate { get; set; }
        public int? EmpIdFirstYoungAppDate { get; set; }
        public int? EmpIdSecondYoungAppDate { get; set; }
        public int? EmpIdAchDate { get; set; }
        public int? AgeStandardStart { get; set; }
        public bool? YoungAppEligible { get; set; }
        public bool? SmallBusEligible { get; set; }
        public DateTime? AchApplicDate { get; set; }
        public decimal? TotalNegotiatedPrice1 { get; set; }
        public decimal? TotalNegotiatedPrice2 { get; set; }

        public decimal? PMRSumBeforeFundingYear { get; set; }

        public decimal? Period1PMRSum { get; set; }
        public decimal? Period1CoreGovContPayment { get; set; }
        public decimal? Period1MathEngOnProgPayment { get; set; }
        public decimal? Period1MathEngBalPayment { get; set; }
        public decimal? Period1LearnSuppFundCash { get; set; }
        public decimal? Period1SmallBusPayment { get; set; }
        public decimal? Period1YoungAppPayment { get; set; }
        public decimal? Period1AchPayment { get; set; }

        public decimal? Period2PMRSum { get; set; }
        public decimal? Period2CoreGovContPayment { get; set; }
        public decimal? Period2MathEngOnProgPayment { get; set; }
        public decimal? Period2MathEngBalPayment { get; set; }
        public decimal? Period2LearnSuppFundCash { get; set; }
        public decimal? Period2SmallBusPayment { get; set; }
        public decimal? Period2YoungAppPayment { get; set; }
        public decimal? Period2AchPayment { get; set; }

        public decimal? Period3PMRSum { get; set; }
        public decimal? Period3CoreGovContPayment { get; set; }
        public decimal? Period3MathEngOnProgPayment { get; set; }
        public decimal? Period3MathEngBalPayment { get; set; }
        public decimal? Period3LearnSuppFundCash { get; set; }
        public decimal? Period3SmallBusPayment { get; set; }
        public decimal? Period3YoungAppPayment { get; set; }
        public decimal? Period3AchPayment { get; set; }

        public decimal? Period4PMRSum { get; set; }
        public decimal? Period4CoreGovContPayment { get; set; }
        public decimal? Period4MathEngOnProgPayment { get; set; }
        public decimal? Period4MathEngBalPayment { get; set; }
        public decimal? Period4LearnSuppFundCash { get; set; }
        public decimal? Period4SmallBusPayment { get; set; }
        public decimal? Period4YoungAppPayment { get; set; }
        public decimal? Period4AchPayment { get; set; }

        public decimal? Period5PMRSum { get; set; }
        public decimal? Period5CoreGovContPayment { get; set; }
        public decimal? Period5MathEngOnProgPayment { get; set; }
        public decimal? Period5MathEngBalPayment { get; set; }
        public decimal? Period5LearnSuppFundCash { get; set; }
        public decimal? Period5SmallBusPayment { get; set; }
        public decimal? Period5YoungAppPayment { get; set; }
        public decimal? Period5AchPayment { get; set; }

        public decimal? Period6PMRSum { get; set; }
        public decimal? Period6CoreGovContPayment { get; set; }
        public decimal? Period6MathEngOnProgPayment { get; set; }
        public decimal? Period6MathEngBalPayment { get; set; }
        public decimal? Period6LearnSuppFundCash { get; set; }
        public decimal? Period6SmallBusPayment { get; set; }
        public decimal? Period6YoungAppPayment { get; set; }
        public decimal? Period6AchPayment { get; set; }

        public decimal? Period7PMRSum { get; set; }
        public decimal? Period7CoreGovContPayment { get; set; }
        public decimal? Period7MathEngOnProgPayment { get; set; }
        public decimal? Period7MathEngBalPayment { get; set; }
        public decimal? Period7LearnSuppFundCash { get; set; }
        public decimal? Period7SmallBusPayment { get; set; }
        public decimal? Period7YoungAppPayment { get; set; }
        public decimal? Period7AchPayment { get; set; }

        public decimal? Period8PMRSum { get; set; }
        public decimal? Period8CoreGovContPayment { get; set; }
        public decimal? Period8MathEngOnProgPayment { get; set; }
        public decimal? Period8MathEngBalPayment { get; set; }
        public decimal? Period8LearnSuppFundCash { get; set; }
        public decimal? Period8SmallBusPayment { get; set; }
        public decimal? Period8YoungAppPayment { get; set; }
        public decimal? Period8AchPayment { get; set; }

        public decimal? Period9PMRSum { get; set; }
        public decimal? Period9CoreGovContPayment { get; set; }
        public decimal? Period9MathEngOnProgPayment { get; set; }
        public decimal? Period9MathEngBalPayment { get; set; }
        public decimal? Period9LearnSuppFundCash { get; set; }
        public decimal? Period9SmallBusPayment { get; set; }
        public decimal? Period9YoungAppPayment { get; set; }
        public decimal? Period9AchPayment { get; set; }

        public decimal? Period10PMRSum { get; set; }
        public decimal? Period10CoreGovContPayment { get; set; }
        public decimal? Period10MathEngOnProgPayment { get; set; }
        public decimal? Period10MathEngBalPayment { get; set; }
        public decimal? Period10LearnSuppFundCash { get; set; }
        public decimal? Period10SmallBusPayment { get; set; }
        public decimal? Period10YoungAppPayment { get; set; }
        public decimal? Period10AchPayment { get; set; }

        public decimal? Period11PMRSum { get; set; }
        public decimal? Period11CoreGovContPayment { get; set; }
        public decimal? Period11MathEngOnProgPayment { get; set; }
        public decimal? Period11MathEngBalPayment { get; set; }
        public decimal? Period11LearnSuppFundCash { get; set; }
        public decimal? Period11SmallBusPayment { get; set; }
        public decimal? Period11YoungAppPayment { get; set; }
        public decimal? Period11AchPayment { get; set; }

        public decimal? Period12PMRSum { get; set; }
        public decimal? Period12CoreGovContPayment { get; set; }
        public decimal? Period12MathEngOnProgPayment { get; set; }
        public decimal? Period12MathEngBalPayment { get; set; }
        public decimal? Period12LearnSuppFundCash { get; set; }
        public decimal? Period12SmallBusPayment { get; set; }
        public decimal? Period12YoungAppPayment { get; set; }
        public decimal? Period12AchPayment { get; set; }

        public decimal? TotalPMRSum { get; set; }
        public decimal? TotalCoreGovContPayment { get; set; }
        public decimal? TotalMathEngOnProgPayment { get; set; }
        public decimal? TotalMathEngBalPayment { get; set; }
        public decimal? TotalLearnSuppFundCash { get; set; }
        public decimal? TotalSmallBusPayment { get; set; }
        public decimal? TotalYoungAppPayment { get; set; }
        public decimal? TotalAchPayment { get; set; }
    }
}