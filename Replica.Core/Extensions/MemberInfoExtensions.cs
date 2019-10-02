using System.Linq;
using System.Reflection;

namespace Replica.Core.Extensions
{
    public static class MemberInfoExtensions
    {
        public static T GetAttribute<T>(this MemberInfo type, bool inherit = false)
            => type.GetCustomAttributes(inherit).OfType<T>().FirstOrDefault();
    }
}