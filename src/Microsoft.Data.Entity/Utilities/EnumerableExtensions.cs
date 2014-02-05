// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace Microsoft.Data.Entity.Utilities
{
    [DebuggerStepThrough]
    internal static class EnumerableExtensions
    {
        public static IOrderedEnumerable<TSource> OrderByOrdinal<TSource>(
            this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(keySelector, "keySelector");

            return source.OrderBy(keySelector, StringComparer.Ordinal);
        }

        public static IEnumerable<T> Distinct<T>(
            this IEnumerable<T> source, Func<T, T, bool> comparer)
            where T : class
        {
            Check.NotNull(source, "source");
            Check.NotNull(comparer, "comparer");

            return source.Distinct(new DynamicEqualityComparer<T>(comparer));
        }

        private sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            private readonly Func<T, T, bool> _func;

            public DynamicEqualityComparer(Func<T, T, bool> func)
            {
                DebugCheck.NotNull(func);

                _func = func;
            }

            public bool Equals(T x, T y)
            {
                return _func(x, y);
            }

            public int GetHashCode(T obj)
            {
                return 0; // force Equals
            }
        }

        public static string Join(this IEnumerable<object> source, string separator = ", ")
        {
            Check.NotNull(source, "source");

            return string.Join(separator, source);
        }

        public static async Task<IEnumerable<T>> SelectAsync<T>(
            this IEnumerable<T> source, Func<T, Task<T>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            var results = Enumerable.Empty<T>();

            foreach (var item in source)
            {
                results = results.Concat(new[] { await selector(item) });
            }

            return results;
        }

        public static async Task<IEnumerable<T>> SelectManyAsync<T>(
            this IEnumerable<T> source, Func<T, Task<IEnumerable<T>>> selector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(selector, "selector");

            var results = Enumerable.Empty<T>();

            foreach (var item in source)
            {
                results = results.Concat(await selector(item));
            }

            return results;
        }

        public static async Task<IEnumerable<T>> WhereAsync<T>(
            this IEnumerable<T> source, Func<T, Task<bool>> predicate)
        {
            Check.NotNull(source, "source");
            Check.NotNull(predicate, "predicate");

            var results = Enumerable.Empty<T>();

            foreach (var item in source)
            {
                if (await predicate(item))
                {
                    results = results.Concat(new[] { item });
                }
            }

            return results;
        }
    }
}
