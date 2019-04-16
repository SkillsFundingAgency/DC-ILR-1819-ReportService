using CsvHelper.Configuration;
using ESFA.DC.ILR.ReportService.Interface;
using ESFA.DC.ILR.ReportService.Model.ReportModels.PeriodEnd;

namespace ESFA.DC.ILR.ReportService.Service.Mapper.PeriodEnd
{
    public class AppsDataMatchMonthEndMapper : ClassMap<AppsDataMatchMonthEndModel>, IClassMapper
    {
        public AppsDataMatchMonthEndMapper()
        {
            int i = 0;
            Map(m => m.LearnerReferenceNumber).Index(i++).Name("Learner reference number");
            Map(m => m.UniqueLearnerNumber).Index(i++).Name("Unique learner number");
            Map(m => m.AimSeqNumber).Index(i++).Name("Aim sequence number");
            Map(m => m.RuleName).Index(i++).Name("Rule name");
            Map(m => m.Description).Index(i++).Name("Description");
            Map(m => m.ILRValue).Index(i++).Name("ILR value");
            Map(m => m.ApprenticeshipServiceValue).Index(i++).Name("Apprenticeship service value");
            Map(m => m.PriceEpisodeStartDate).Index(i++).Name("Price episode start date");
            Map(m => m.PriceEpisodeActualEndDate).Index(i++).Name("Price episode actual end date");
            Map(m => m.AgreementIdentifier).Index(i++).Name("Agreement identifier from ILR");
            Map(m => m.EmployerName).Index(i++).Name("Employer name from apprenticeship service");
            Map(m => m.OfficialSensitive).Index(i++).Name("OFFICIAL - SENSITIVE");
        }
    }
}
