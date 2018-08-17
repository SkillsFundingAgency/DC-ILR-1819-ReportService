using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class ValidationErrorMapper : ClassMap<ValidationErrorModel>, IClassMapper
    {
        public ValidationErrorMapper()
        {
            Map(m => m.Severity).Index(0).Name(@"Error\Warning");
            Map(m => m.LearnerReferenceNumber).Index(1).Name(@"Learner Ref");
            Map(m => m.RuleName).Index(2).Name(@"Rule Name");
            Map(m => m.FieldValues).Index(3).Name(@"Field Values");
            Map(m => m.ErrorMessage).Index(4).Name(@"Error Message");
            Map(m => m.AimSequenceNumber).Index(5).Name(@"Aim Sequence Number");
            Map(m => m.LearnAimRef).Index(6).Name(@"Aim Reference Number");
            Map(m => m.SWSupAimId).Index(7).Name(@"Software Supplier Aim ID");
            Map(m => m.FundModel).Index(8).Name(@"Funding Model");
            Map(m => m.PartnerUKPRN).Index(9).Name(@"Subcontracted UKPRN");
            Map(m => m.ProviderSpecLearnOccurA).Index(10).Name(@"Provider Specified Learner Monitoring A");
            Map(m => m.ProviderSpecLearnOccurB).Index(11).Name(@"Provider Specified Learner Monitoring B");
            Map(m => m.ProviderSpecDelOccurA).Index(12).Name(@"Provider Specified Learning Delivery Monitoring A");
            Map(m => m.ProviderSpecDelOccurB).Index(13).Name(@"Provider Specified Learning Delivery Monitoring B");
            Map(m => m.ProviderSpecDelOccurC).Index(14).Name(@"Provider Specified Learning Delivery Monitoring C");
            Map(m => m.ProviderSpecDelOccurD).Index(15).Name(@"Provider Specified Learning Delivery Monitoring D");
            Map(m => m.SecurityClassification).Index(16).Name(@"OFFICIAL-SENSITIVE");
        }
    }
}
