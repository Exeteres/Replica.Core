using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Replica.Core.Exceptions;

namespace Replica.Core.Utils
{
    internal class JsonResourceManager
    {
        private string _default;
        private Assembly _asm;
        private Dictionary<string, Dictionary<string, string>> _loaded = new Dictionary<string, Dictionary<string, string>>();

        public JsonResourceManager(Assembly asm, string df)
        {
            _default = df;
            _asm = asm;
            _ = Load(_default)
                ?? throw new ResourceNotFoundException("Default resource " + _default + " not found in calling assembly");
        }

        private Dictionary<string, string> Load(string key)
        {
            var stream = _asm.GetManifestResourceStream($"{_asm.GetName().Name}.Resources.locale-{key}.json");
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            return _loaded[key] = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
        }

        public Dictionary<string, string> Resolve(string key)
        {
            if (_loaded.ContainsKey(key))
                return _loaded[key];

            return Load(key) ?? _loaded[default];
        }
    }
}