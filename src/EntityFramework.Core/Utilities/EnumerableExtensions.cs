// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;

// ReSharper disable once CheckNamespace

namespace System.Collections.Generic
{
    [DebuggerStepThrough]
    internal static class EnumerableExtensions
    {
        public static IOrderedEnumerable<TSource> OrderByOrdinal<TSource>(
            this IEnumerable<TSource> source, Func<TSource, string> keySelector) 
            => source.OrderBy(keySelector, StringComparer.Ordinal);

        public static IEnumerable<T> Distinct<T>(
            this IEnumerable<T> source, Func<T, T, bool> comparer)
            where T : class 
            => source.Distinct(new DynamicEqualityComparer<T>(comparer));

        private sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            private readonly Func<T, T, bool> _func;

            public DynamicEqualityComparer(Func<T, T, bool> func)
            {
                _func = func;
            }

            public bool Equals(T x, T y) => _func(x, y);

            public int GetHashCode(T obj) => 0;
        }

        public static string Join(this IEnumerable<object> source, string separator = ", ") 
            => string.Join(separator, source);

        public static bool StructuralSequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            var firstEnumerator = first.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();

            while (firstEnumerator.MoveNext())
            {
                if (!secondEnumerator.MoveNext()
                    || !StructuralComparisons.StructuralEqualityComparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                {
                    return false;
                }
            }

            return !secondEnumerator.MoveNext();
        }
    }
}
