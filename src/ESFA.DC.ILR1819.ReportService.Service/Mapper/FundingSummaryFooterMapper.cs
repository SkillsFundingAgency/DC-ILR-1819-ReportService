using CsvHelper.Configuration;
using ESFA.DC.ILR1819.ReportService.Interface;
using ESFA.DC.ILR1819.ReportService.Model.ReportModels;

namespace ESFA.DC.ILR1819.ReportService.Service.Mapper
{
    public sealed class FundingSummaryFooterMapper : ClassMap<FundingSummaryFooterModel>, IClassMapper
    {
        public FundingSummaryFooterMapper()
        {
            Map(m => m.ComponentSetVersion).Index(0).Name("Component Set Version");
            Map(m => m.ApplicationVersion).Index(1).Name("Application Version");
            Map(m => m.FilePreparationDate).Index(2).Name("File Preparation Date");
            Map(m => m.LarsData).Index(3).Name("LARS Data");
            Map(m => m.PostcodeData).Index(4).Name("Postcode Data");
            Map(m => m.OrganisationData).Index(5).Name("Organisation Data");
            Map(m => m.LargeEmployerData).Index(6).Name("Large Employer Data");
            Map(m => m.ReportGeneratedAt).Index(7).Name("Report generated at");
        }
    }
}
