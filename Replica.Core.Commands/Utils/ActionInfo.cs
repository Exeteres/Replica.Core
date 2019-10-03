using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using Replica.Core.Contexts;
using Replica.Core.Extensions;
using System.Collections;

namespace Replica.Core.Commands.Utils
{
    public class ActionInfo
    {
        private ParamInfo[] _parameters;
        private ParamInfo _params;
        public int Required { get; private set; }
        private MethodInfo _method;
        public string Usage { get; private set; }
        public string Name { get; private set; }
        public IRestriction Restriction { get; private set; }

        public IEnumerable<IParameterRestriction> GetRestrictions()
        {
            var result = _parameters.Select(x => x.Restrictions)
                .Cast<IEnumerable<IParameterRestriction>>();
            if (!result.Any()) return Enumerable.Empty<IParameterRestriction>();
            var result2 = result
                .Aggregate((a, b) => a.Concat(b))
                .ToList();
            if (_params != null)
                result2.AddRange(_params.Restrictions);
            return result2;
        }

        public static ActionInfo[] FromType(Type type, string name)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(x =>
                {
                    var p = x.GetParameters();
                    var pr = p.LastOrDefault();

                    var parameters = p.Select(y => ParamInfo.FromParameterInfo(y)).ToArray();
                    var paramst = pr != null ? pr.IsParams() ? ParamInfo.FromParameterInfo(pr) : null : null; // втф
                    return new ActionInfo
                    {
                        Name = x.Name,
                        _method = x,
                        _parameters = parameters,
                        _params = paramst,
                        Required = p.Count(y => !y.IsParams() && !y.IsOptional),
                        Restriction = x.GetAttribute<IRestriction>(),
                        // какая красивая строка
                        Usage = $"/{name}{(x.Name == "Default" ? "" : " " + x.Name.ToLower())} {string.Join(" ", parameters.Select(y => (y.IsOptional ? "[" : "<") + y.Name + (y.IsOptional ? "]" : ">")))}"
                    };
                })
                .ToArray();
        }

        public object Invoke(object instance, object[] args)
        {
            return _method.Invoke(instance, args);
        }

        private string WrapMessage(ParamInfo info, string msg)
        {
            return "[" + info.Name + ": " + msg + "]";
        }

        // TODO! REFACTORING
        // TODO! REFACTORING
        // TODO! FUCKING REFACTORING

        public string TryParseArguments(Context context, string[] args, out object[] parameters)
        {
            var localizer = context.GetLocalizer();

            parameters = null;
            if (args.Length < Required)
                return localizer["Mismatch"];

            string Convert(Type type, string arg, out object result)
            {
                result = null;
                type = type.IsArray ? type.GetElementType() : type;
                type = Nullable.GetUnderlyingType(type) ?? type;

                // TODO localization

                if (type == typeof(string))
                {
                    result = arg;
                    return null;
                }

                if (type == typeof(int))
                {
                    if (!int.TryParse(arg, out var res))
                        return "Not a number";
                    result = res;
                    return null;
                }

                if (type == typeof(long))
                {
                    if (!long.TryParse(arg, out var res))
                        return "Not a long";
                    result = res;
                    return null;
                }

                if (type == typeof(bool))
                {
                    if (!bool.TryParse(arg, out var res))
                        return "Not a bool";
                    result = res;
                    return null;
                }

                if (type == typeof(float))
                {
                    if (!bool.TryParse(arg, out var res))
                        return "Not a float";
                    result = res;
                    return null;
                }

                if (type.IsEnum)
                {
                    if (!EnumHelper.TryParse(type, arg, out var res))
                        return "Not a " + type.Name;
                    var values = type.GetEnumValues();
                    if (((int)res) < 0 || ((int)res) >= values.Length)
                        return "Not a " + type.Name;
                    result = res;
                    return null;
                }

                if (type == typeof(DateTime))
                {
                    if (!DateTime.TryParse(arg, out var res))
                        return "Not a DateTime";
                    result = res;
                    return null;
                }

                var converter = context.Core
                    .ResolveModule<CommandsModule>()
                    .ResolveConverter(type);
                return converter.TryConvert(context, arg, out result);
            }

            string Validate(ParamInfo param, object obj)
            {
                var msg = param.Validate(context, obj);
                if (msg != null) return msg;
                msg = param.Check(context, obj);
                if (msg != null) return msg;
                return null;
            }

            var ps = new List<object>();
            var prs = new List<object>();

            foreach (var (arg, i) in args.WithIndex())
            {
                string msg;
                object result;
                if ((_params != null ? _parameters.Length - 1 : _parameters.Length) <= i)
                {
                    if (_params == null) break;
                    msg = Convert(_params.Type, arg, out result);
                    if (msg != null) return WrapMessage(_params, msg);
                    prs.Add(result);
                    continue;
                }

                var param = _parameters[i];
                msg = Convert(param.Type, arg, out result);
                if (msg != null) return WrapMessage(param, msg);
                msg = Validate(param, result);
                if (msg != null) return WrapMessage(param, msg);
                ps.Add(result);
            }

            var missing = (_params != null ? _parameters.Length - 1 : _parameters.Length) - args.Length;
            if (missing > 0)
                ps.AddRange(Enumerable.Repeat<object>(null, missing));

            if (_params != null)
            {
                var dest = Array.CreateInstance(_params.Type.GetElementType(), prs.Count);
                Array.Copy(prs.ToArray(), dest, prs.Count);
                var msg = Validate(_params, dest);
                if (msg != null) return WrapMessage(_params, msg);
                ps.Add(dest);
            }
            parameters = ps.ToArray();
            return null;
        }
    }
}