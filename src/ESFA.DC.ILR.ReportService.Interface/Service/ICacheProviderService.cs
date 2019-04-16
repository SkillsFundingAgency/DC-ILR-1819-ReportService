namespace ESFA.DC.ILR.ReportService.Interface.Service
{
    public interface ICacheProviderService<T>
    {
        T Get(string key);

        void Set(string key, T value);

        T Get(int key);

        void Set(int key, T value);
    }
}
