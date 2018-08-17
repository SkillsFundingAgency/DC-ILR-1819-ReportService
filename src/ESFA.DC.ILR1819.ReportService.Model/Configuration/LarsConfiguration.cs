using ESFA.DC.ILR1819.ReportService.Interface.Configuration;

namespace ESFA.DC.ILR1819.ReportService.Model.Configuration
{
    public sealed class LarsConfiguration : ILarsConfiguration
    {
        public string LarsConnectionString { get; set; }
    }
}
