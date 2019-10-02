using System;
using Replica.Core.Contexts;

namespace Replica.Core.Commands
{
    public interface IConverter
    {
        Type Type { get; }
        string TryConvert(Context context, string origin, out object result);
    }
}
