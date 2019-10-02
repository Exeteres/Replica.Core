using System.Collections.Generic;
using SmartFormat;

namespace Replica.Core.Localization
{
    public class Localizer
    {
        private Dictionary<string, string> _dict;

        public string Language { get; private set; }

        public Localizer(Dictionary<string, string> dict, string lang)
        {
            _dict = dict;
            Language = lang;
        }

        public string this[string key]
            => _dict.TryGetValue(key, out var value) ? value.ToString() : key;

        public string Localize(string key, params object[] args)
            => Smart.Format(this[key], args);
    }
}
