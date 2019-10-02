using System;
using System.Collections.Generic;

namespace Replica.Core.Extensions
{
    public static class DictionaryExtensions
    {
        // Спизжено сами знаете откуда
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default)
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static string Save<TV>(this Dictionary<string, TV> dict, TV value)
        {
            var key = Guid.NewGuid().ToString();
            dict[key] = value;
            return key;
        }
    }
}
