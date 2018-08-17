using ESFA.DC.ILR1819.ReportService.Interface.Configuration;

namespace ESFA.DC.ILR1819.ReportService.Model.Configuration
{
    public sealed class OrgConfiguration : IOrgConfiguration
    {
        public string OrgConnectionString { get; set; }
    }
}
