using System;
using System.Reflection;

namespace Replica.Core.Extensions
{
    public static class ParameterInfoExtensions
    {
        public static bool IsParams(this ParameterInfo param)
        {
            return param.IsDefined(typeof(ParamArrayAttribute), false);
        }
    }
}
