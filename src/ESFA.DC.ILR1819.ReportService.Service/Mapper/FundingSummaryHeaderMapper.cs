using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class FundingSummaryHeaderMapper : ClassMap<FundingSummaryHeaderModel>, IClassMapper
    {
        public FundingSummaryHeaderMapper()
        {
            Map(m => m.ProviderName).Index(0).Name("Provider Name");
            Map(m => m.Ukprn).Index(1).Name("UKPRN");
            Map(m => m.IlrFile).Index(2).Name("ILR File");
            Map(m => m.LastIlrFileUpdate).Index(3).Name("Last ILR File Update");
            Map(m => m.LastEasUpdate).Index(4).Name("Last EAS Update");
            Map(m => m.SecurityClassification).Index(5).Name("Security Classification");
        }
    }
}
