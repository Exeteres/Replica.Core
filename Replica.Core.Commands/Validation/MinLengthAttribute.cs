using System;
using Replica.Core.Exceptions;
using Replica.Core.Localization;

namespace Replica.Core.Commands.Validation
{
    public class MinLengthAttribute : Attribute, IValidator
    {
        public MinLengthAttribute(int value)
        {
            Value = value;
        }

        public int Value { get; }

        void IValidator.CheckType(Type type)
        {
            if (!type.IsArray)
                throw new InvalidAttributeUsageException("MinLength attribute must only apply to arrays");
        }

        string IValidator.Validate(Localizer localizer, object origin)
        {
            if ((origin as Array).Length < Value)
                return localizer.Localize("MustContainAtLeast", Value);
            return null;
        }
    }
}
