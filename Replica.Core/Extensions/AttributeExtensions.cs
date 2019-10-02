using System;

namespace Replica.Core.Extensions
{
    public static class AttributeExtensions
    {
        public static bool TryCast<T>(this Attribute value, out T attr) where T : Attribute
        {
            if (value is T)
            {
                attr = (T)value;
                return true;
            }

            attr = null;
            return false;
        }
    }
}
