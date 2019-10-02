using System;
using Replica.Core.Localization;

namespace Replica.Core.Commands.Validation
{
    internal interface IValidator
    {
        void CheckType(Type type);
        string Validate(Localizer lang, object origin);
    }
}