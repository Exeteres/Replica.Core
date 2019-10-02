using System;
namespace Replica.Core.Caching
{
    public class PersistentCache
    {
        private ICacheProvider _provider;

        public PersistentCache(ICacheProvider provider, string path)
        {
            _provider = provider;
            _provider.Open(path);
        }

        public string Get(string key)
        {
            return _provider.Get(key);
        }

        public string Set(string key, string value)
        {
            _provider.Set(key, value);
            return value;
        }

        public string GetOrAdd(string key, Func<string> action)
        {
            return Get(key) ?? Set(key, action.Invoke());
        }
    }
}