using System.Collections.Generic;
using ESFA.DC.ILR.ReportService.Interface.Service;

namespace ESFA.DC.ILR.ReportService.Service.Service
{
    public sealed class CacheProviderService<T> : ICacheProviderService<T>
    {
        private readonly Dictionary<string, T> _cache;

        public CacheProviderService()
        {
            _cache = new Dictionary<string, T>();
        }

        public T Get(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }

            return default(T);
        }

        public void Set(string key, T value)
        {
            _cache[key] = value;
        }

        public T Get(int key)
        {
            return Get(key.ToString());
        }

        public void Set(int key, T value)
        {
            Set(key.ToString(), value);
        }
    }
}