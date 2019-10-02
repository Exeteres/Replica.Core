using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Replica.Core.Handlers;

namespace Replica.Core.Utils
{
    public static class Helpers
    {
        public static T ExtractMetaInfo<T>(Type type) where T : new()
        {
            return (T)ExtractMetaInfo(typeof(T), type);
        }

        public static IEnumerable<FieldInfo> ExtractAllFields<T>()
        {
            return typeof(T)
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static object ExtractMetaInfo(Type infoT, Type type)
        {
            var info = Activator.CreateInstance(infoT);
            foreach (var attr in type.GetCustomAttributes(true))
            {
                var infoP = attr.GetType().GetProperty("Info", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (infoP?.PropertyType != infoT) continue;
                var source = infoP.GetValue(attr);
                foreach (var prop in infoT.GetProperties())
                {
                    var val = prop.GetValue(source);
                    if (val != null && prop.CanWrite)
                        prop.SetValue(info, val);
                }
            }

            return info;
        }

        public static IHandler LinkHandlers(IEnumerable<IHandler> handlers)
        {
            handlers.Aggregate((a, b) => a.SetNext(b));
            return handlers.First();
        }

        public static string GenerateString(int length)
        {
            using var rng = new RNGCryptoServiceProvider();
            var bit_count = (length * 6);
            var byte_count = ((bit_count + 7) / 8);
            var bytes = new byte[byte_count];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("=", "a")
                .Replace("+", "b")
                .Replace("/", "c"); // lol
        }
    }
}
