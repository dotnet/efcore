// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Utilities
{
    [DebuggerStepThrough]
    internal static class EnumerableExtensions
    {
        public static IOrderedEnumerable<TSource> OrderByOrdinal<TSource>(
            [NotNull] this IEnumerable<TSource> source,
            [NotNull] Func<TSource, string> keySelector)
            => source.OrderBy(keySelector, StringComparer.Ordinal);

        public static IEnumerable<T> Distinct<T>(
            [NotNull] this IEnumerable<T> source,
            [NotNull] Func<T, T, bool> comparer)
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

            public bool Equals(T x, T y)
                => _func(x, y);

            public int GetHashCode(T obj)
                => 0;
        }

        public static string Join(
            [NotNull] this IEnumerable<object> source,
            [NotNull] string separator = ", ")
            => string.Join(separator, source);

        public static bool StructuralSequenceEqual<TSource>(
            [NotNull] this IEnumerable<TSource> first,
            [NotNull] IEnumerable<TSource> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            using var firstEnumerator = first.GetEnumerator();
            using var secondEnumerator = second.GetEnumerator();
            while (firstEnumerator.MoveNext())
            {
                if (!secondEnumerator.MoveNext()
                    || !StructuralComparisons.StructuralEqualityComparer
                        .Equals(firstEnumerator.Current, secondEnumerator.Current))
                {
                    return false;
                }
            }

            return !secondEnumerator.MoveNext();
        }

        public static bool StartsWith<TSource>(
            [NotNull] this IEnumerable<TSource> first,
            [NotNull] IEnumerable<TSource> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            using (var firstEnumerator = first.GetEnumerator())
            {
                using var secondEnumerator = second.GetEnumerator();
                while (secondEnumerator.MoveNext())
                {
                    if (!firstEnumerator.MoveNext()
                        || !Equals(firstEnumerator.Current, secondEnumerator.Current))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static int IndexOf<T>([NotNull] this IEnumerable<T> source, [NotNull] T item)
            => IndexOf(source, item, EqualityComparer<T>.Default);

        public static int IndexOf<T>(
            [NotNull] this IEnumerable<T> source,
            [NotNull] T item,
            [NotNull] IEqualityComparer<T> comparer)
            => source.Select(
                    (x, index) =>
                        comparer.Equals(item, x) ? index : -1)
                .FirstOr(x => x != -1, -1);

        public static T FirstOr<T>([NotNull] this IEnumerable<T> source, [NotNull] T alternate)
            => source.DefaultIfEmpty(alternate).First();

        public static T FirstOr<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<T, bool> predicate, [NotNull] T alternate)
            => source.Where(predicate).FirstOr(alternate);

        public static bool Any([NotNull] this IEnumerable source)
        {
            foreach (var _ in source)
            {
                return true;
            }

            return false;
        }

        public static async Task<List<TSource>> ToListAsync<TSource>(
            this IAsyncEnumerable<TSource> source,
            CancellationToken cancellationToken = default)
        {
            var list = new List<TSource>();
            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }

            return list;
        }

        public static List<TSource> ToList<TSource>(this IEnumerable source)
            => source.OfType<TSource>().ToList();

        public static string Format([NotNull] this IEnumerable<string> strings)
            => "{"
                + string.Join(
                    ", ",
                    strings.Select(s => "'" + s + "'"))
                + "}";
    }
}
