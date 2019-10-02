using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Replica.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, Task<TResult>> method)
        {
            return await Task.WhenAll(source.Select(async s => await method(s)));
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));

        public static (IEnumerable<A>, IEnumerable<B>) SelectTuple<T, A, B>(this IEnumerable<T> source, Func<T, A> fm, Func<T, B> sm)
        {
            var first = new List<A>();
            var second = new List<B>();
            foreach (var item in source)
            {
                var result = fm(item);
                if (result != null)
                {
                    first.Add(result);
                    continue;
                }

                var result2 = sm(item);
                if (result2 != null)
                    second.Add(result2);
            }
            return (first, second);
        }
    }
}