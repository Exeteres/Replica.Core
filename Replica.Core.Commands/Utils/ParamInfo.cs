using System.Linq;
using System;
using System.Reflection;
using Replica.Core.Commands.Validation;
using Replica.Core.Contexts;
using Replica.Core.Extensions;

namespace Replica.Core.Commands.Utils
{
    internal class ParamInfo
    {
        public IValidator[] Validators { get; private set; }
        public IParameterRestriction[] Restrictions { get; private set; }
        public Type Type { get; private set; }
        public bool IsOptional { get; private set; }
        public string Name { get; private set; }

        public string Validate(Context context, object origin)
        {
            foreach (var validator in Validators)
            {
                var message = validator.Validate(context.GetLocalizer(), origin);
                if (message != null)
                    return message;
            }
            return null;
        }

        public string Check(Context context, object param)
        {
            foreach (var restriction in Restrictions)
            {
                var access = restriction.Check(context, param);
                if (!access) return context.GetLocalizer()["AccessDenied"];
            }
            return null;
        }

        public static ParamInfo FromParameterInfo(ParameterInfo info)
        {
            var (validators, restrictions) = info.GetCustomAttributes()
                .SelectTuple(x => x as IValidator, x => x as IParameterRestriction);

            foreach (var validator in validators)
                validator.CheckType(info.ParameterType);

            return new ParamInfo
            {
                Type = info.ParameterType,
                Name = info.Name,
                Validators = validators.ToArray(),
                Restrictions = restrictions.ToArray(),
                IsOptional = info.IsOptional
            };
        }
    }
}