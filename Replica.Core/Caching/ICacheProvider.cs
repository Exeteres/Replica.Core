namespace Replica.Core.Caching
{
    public interface ICacheProvider
    {
        void Open(string path);
        void Close();
        void Set(string key, string value);
        string Get(string key);
    }
}