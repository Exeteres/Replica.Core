using System;
using Replica.Core.Contexts;

namespace Replica.Core.Commands
{
    public abstract class ConverterBase<T> : IConverter
    {
        public Type Type => typeof(T);

        string IConverter.TryConvert(Context context, string origin, out object result)
        {
            // каво
            var message = TryConvert(context, origin, out var r);
            result = r;
            return message;
        }

        public abstract string TryConvert(Context context, string origin, out T result);
    }
}
