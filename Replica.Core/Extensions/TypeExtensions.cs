using System.Reflection;
using System;
using System.Linq;

namespace Replica.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool TryGetAttribute<T>(this Type type, out T attr) where T : Attribute
        {
            return (attr = type.GetCustomAttribute<T>()) != null;
        }
    }
}