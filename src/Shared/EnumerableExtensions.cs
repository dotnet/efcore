// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Utilities;

[DebuggerStepThrough]
internal static class EnumerableExtensions
{
    public static IOrderedEnumerable<TSource> OrderByOrdinal<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, string> keySelector)
        => source.OrderBy(keySelector, StringComparer.Ordinal);

    public static IEnumerable<T> Distinct<T>(
        this IEnumerable<T> source,
        Func<T?, T?, bool> comparer)
        where T : class
        => source.Distinct(new DynamicEqualityComparer<T>(comparer));

    public static IEnumerable<T> TakeUpTo<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            yield return item;
            if (predicate(item))
            {
                yield break;
            }
        }
    }

    private sealed class DynamicEqualityComparer<T>(Func<T?, T?, bool> func) : IEqualityComparer<T>
        where T : class
    {
        public bool Equals(T? x, T? y)
            => func(x, y);

        public int GetHashCode(T obj)
            => 0;
    }

    public static string Join(
        this IEnumerable<object> source,
        string separator = ", ")
        => string.Join(separator, source);

    public static bool StructuralSequenceEqual<TSource>(
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second)
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
        this IEnumerable<TSource> first,
        IEnumerable<TSource> second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        using var firstEnumerator = first.GetEnumerator();
        using var secondEnumerator = second.GetEnumerator();

        while (secondEnumerator.MoveNext())
        {
            if (!firstEnumerator.MoveNext()
                || !Equals(firstEnumerator.Current, secondEnumerator.Current))
            {
                return false;
            }
        }

        return true;
    }

    public static int IndexOf<T>(this IEnumerable<T> source, T item)
        => IndexOf(source, item, EqualityComparer<T>.Default);

    public static int IndexOf<T>(
        this IEnumerable<T> source,
        T item,
        IEqualityComparer<T> comparer)
        => source.Select(
                (x, index) =>
                    comparer.Equals(item, x) ? index : -1)
            .FirstOr(x => x != -1, -1);

    public static T FirstOr<T>(this IEnumerable<T> source, T alternate)
        => source.DefaultIfEmpty(alternate).First();

    public static T FirstOr<T>(this IEnumerable<T> source, Func<T, bool> predicate, T alternate)
        => source.Where(predicate).FirstOr(alternate);

    public static bool Any(this IEnumerable source)
    {
        foreach (var _ in source)
        {
            return true;
        }

        return false;
    }

    public static List<TSource> ToList<TSource>(this IEnumerable source)
        => source.OfType<TSource>().ToList();

    public static string Format(this IEnumerable<string> strings)
        => "{"
            + string.Join(
                ", ",
                strings.Select(s => "'" + s + "'"))
            + "}";
}
