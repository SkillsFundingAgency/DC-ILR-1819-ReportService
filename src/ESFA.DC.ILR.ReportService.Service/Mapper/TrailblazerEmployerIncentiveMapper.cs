using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR.ReportService.Service.Mapper
{
    public sealed class TrailblazerEmployerIncentiveMapper : ClassMap<TrailblazerEmployerIncentivesModel>, IClassMapper
    {
        public TrailblazerEmployerIncentiveMapper()
        {
            int i = 0;
            Map(m => m.EmployerIdentifier).Index(i).Name("Learner reference number");

            Map(m => m.Period1SmallEmployerIncentive).Index(i++).Name("August small employer incentive (£)");
            Map(m => m.Period11618ApprenticeIncentive).Index(i++).Name("August 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period1AchievementIncentive).Index(i++).Name("August achievement incentive (£)");
            Map(m => m.Period1Total).Index(i++).Name("August total (£)");

            Map(m => m.Period2SmallEmployerIncentive).Index(i++).Name("September small employer incentive (£)");
            Map(m => m.Period21618ApprenticeIncentive).Index(i++).Name("September 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period2AchievementIncentive).Index(i++).Name("September achievement incentive (£)");
            Map(m => m.Period2Total).Index(i++).Name("September total (£)");

            Map(m => m.Period3SmallEmployerIncentive).Index(i++).Name("October small employer incentive (£)");
            Map(m => m.Period31618ApprenticeIncentive).Index(i++).Name("October 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period3AchievementIncentive).Index(i++).Name("October achievement incentive (£)");
            Map(m => m.Period3Total).Index(i++).Name("October total (£)");

            Map(m => m.Period4SmallEmployerIncentive).Index(i++).Name("November small employer incentive (£)");
            Map(m => m.Period41618ApprenticeIncentive).Index(i++).Name("November 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period4AchievementIncentive).Index(i++).Name("November achievement incentive (£)");
            Map(m => m.Period4Total).Index(i++).Name("November total (£)");

            Map(m => m.Period5SmallEmployerIncentive).Index(i++).Name("December small employer incentive (£)");
            Map(m => m.Period51618ApprenticeIncentive).Index(i++).Name("December 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period5AchievementIncentive).Index(i++).Name("December achievement incentive (£)");
            Map(m => m.Period5Total).Index(i++).Name("December total (£)");

            Map(m => m.Period6SmallEmployerIncentive).Index(i++).Name("January small employer incentive (£)");
            Map(m => m.Period61618ApprenticeIncentive).Index(i++).Name("January 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period6AchievementIncentive).Index(i++).Name("January achievement incentive (£)");
            Map(m => m.Period6Total).Index(i++).Name("January total (£)");

            Map(m => m.Period7SmallEmployerIncentive).Index(i++).Name("February small employer incentive (£)");
            Map(m => m.Period71618ApprenticeIncentive).Index(i++).Name("February 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period7AchievementIncentive).Index(i++).Name("February achievement incentive (£)");
            Map(m => m.Period7Total).Index(i++).Name("February total (£)");

            Map(m => m.Period8SmallEmployerIncentive).Index(i++).Name("March small employer incentive (£)");
            Map(m => m.Period81618ApprenticeIncentive).Index(i++).Name("March 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period8AchievementIncentive).Index(i++).Name("March achievement incentive (£)");
            Map(m => m.Period8Total).Index(i++).Name("March total (£)");

            Map(m => m.Period9SmallEmployerIncentive).Index(i++).Name("April small employer incentive (£)");
            Map(m => m.Period91618ApprenticeIncentive).Index(i++).Name("April 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period9AchievementIncentive).Index(i++).Name("April achievement incentive (£)");
            Map(m => m.Period9Total).Index(i++).Name("April total (£)");

            Map(m => m.Period10SmallEmployerIncentive).Index(i++).Name("May small employer incentive (£)");
            Map(m => m.Period101618ApprenticeIncentive).Index(i++).Name("May 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period10AchievementIncentive).Index(i++).Name("May achievement incentive (£)");
            Map(m => m.Period10Total).Index(i++).Name("May total (£)");

            Map(m => m.Period11SmallEmployerIncentive).Index(i++).Name("June small employer incentive (£)");
            Map(m => m.Period111618ApprenticeIncentive).Index(i++).Name("June 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period11AchievementIncentive).Index(i++).Name("June achievement incentive (£)");
            Map(m => m.Period11Total).Index(i++).Name("June total (£)");

            Map(m => m.Period12SmallEmployerIncentive).Index(i++).Name("July small employer incentive (£)");
            Map(m => m.Period121618ApprenticeIncentive).Index(i++).Name("July 16-18 year-old apprentice incentive (£)");
            Map(m => m.Period12AchievementIncentive).Index(i++).Name("July achievement incentive (£)");
            Map(m => m.Period12Total).Index(i++).Name("July total (£)");

            Map(m => m.SmallEmployerIncentiveTotal).Index(i++).Name("Total small employer incentive (£)");
            Map(m => m.Apprentice1618IncentiveTotal).Index(i++).Name("Total 16-18 year-old apprentice incentive (£)");
            Map(m => m.AchievementTotal).Index(i++).Name("Total achievement incentive (£)");
            Map(m => m.GrandTotal).Index(i++).Name("Grand total (£)");

            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}