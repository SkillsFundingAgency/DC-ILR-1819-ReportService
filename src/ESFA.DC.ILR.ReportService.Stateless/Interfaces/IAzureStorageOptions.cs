namespace ESFA.DC.ILR1819.ReportService.Stateless.Interfaces
{
    public interface IAzureStorageOptions
    {
        string AzureBlobConnectionString { get; set; }

        string AzureBlobContainerName { get; set; }
    }
}