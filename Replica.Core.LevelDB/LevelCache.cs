using System.IO;
using System;
using LevelDB;
using Replica.Core.Caching;

namespace Replica.Core.LevelDB
{
    public class LevelCache : ICacheProvider
    {
        private DB _db;

        public void Close() => _db.Close();
        public string Get(string key) => _db.Get(key);

        public void Open(string path)
        {
            _db = new DB(new Options { CreateIfMissing = true }, Path.GetFullPath(path));
        }

        public void Set(string key, string value)
        {
            _db.Put(key, value);
        }
    }
}
