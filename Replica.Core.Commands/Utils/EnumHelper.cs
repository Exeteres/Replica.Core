using System;
using System.Linq;
using System.Reflection;

namespace Replica.Core.Commands.Utils
{
    public static class EnumHelper
    {
        private static MethodInfo enumTryParse;

        static EnumHelper()
        {
            enumTryParse = typeof(Enum)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "TryParse" && m.GetParameters().First().ParameterType == typeof(Type))
                .First();
        }

        public static bool TryParse(
            Type enumType,
            string value,
            out object enumValue)
        {
            object[] args = new object[] { enumType, value, Activator.CreateInstance(enumType) };
            bool success = (bool)enumTryParse.Invoke(null, args);
            enumValue = args[2];
            return success;
        }
    }
}