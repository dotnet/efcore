// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class StringDictionaryComparer<TDictionary, TElement> : ValueComparer<object>, IInfrastructure<ValueComparer>
{
    private static readonly MethodInfo CompareMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(Compare), BindingFlags.Static | BindingFlags.NonPublic, [typeof(object), typeof(object), typeof(Func<TElement, TElement, bool>)])!;

    private static readonly MethodInfo GetHashCodeMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(GetHashCode), BindingFlags.Static | BindingFlags.NonPublic, [typeof(IEnumerable), typeof(Func<TElement, int>)])!;

    private static readonly MethodInfo SnapshotMethod = typeof(StringDictionaryComparer<TDictionary, TElement>).GetMethod(
        nameof(Snapshot), BindingFlags.Static | BindingFlags.NonPublic, [typeof(object), typeof(Func<TElement, TElement>)])!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public StringDictionaryComparer(ValueComparer elementComparer)
        : base(
            CompareLambda(elementComparer),
            GetHashCodeLambda(elementComparer),
            SnapshotLambda(elementComparer))
        => ElementComparer = elementComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ValueComparer ElementComparer { get; }

    ValueComparer IInfrastructure<ValueComparer>.Instance
        => ElementComparer;

    private static Expression<Func<object?, object?, bool>> CompareLambda(ValueComparer elementComparer)
    {
        var prm1 = Expression.Parameter(typeof(object), "a");
        var prm2 = Expression.Parameter(typeof(object), "b");

        return Expression.Lambda<Func<object?, object?, bool>>(
            Expression.Call(
                CompareMethod,
                prm1,
                prm2,
                elementComparer.EqualsExpression),
            prm1,
            prm2);
    }

    private static Expression<Func<object, int>> GetHashCodeLambda(ValueComparer elementComparer)
    {
        var prm = Expression.Parameter(typeof(object), "o");

        return Expression.Lambda<Func<object, int>>(
            Expression.Call(
                GetHashCodeMethod,
                Expression.Convert(
                    prm,
                    typeof(IEnumerable)),
                    elementComparer.HashCodeExpression),
            prm);
    }

    private static Expression<Func<object, object>> SnapshotLambda(ValueComparer elementComparer)
    {
        var prm = Expression.Parameter(typeof(object), "source");

        return Expression.Lambda<Func<object, object>>(
            Expression.Call(
                SnapshotMethod,
                prm,
                elementComparer.SnapshotExpression),
            prm);
    }

    private static bool Compare(object? a, object? b, Func<TElement?, TElement?, bool> elementCompare)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null)
        {
            return b is null;
        }

        if (b is null)
        {
            return false;
        }

        if (a is IReadOnlyDictionary<string, TElement?> aDictionary && b is IReadOnlyDictionary<string, TElement?> bDictionary)
        {
            if (aDictionary.Count != bDictionary.Count)
            {
                return false;
            }

            foreach (var pair in aDictionary)
            {
                if (!bDictionary.TryGetValue(pair.Key, out var bValue)
                    || !elementCompare(pair.Value, bValue))
                {
                    return false;
                }
            }

            return true;
        }

        throw new InvalidOperationException(
            CosmosStrings.BadDictionaryType(
                (a is IDictionary<string, TElement?> ? b : a).GetType().ShortDisplayName(),
                typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(TElement)).ShortDisplayName()));
    }

    private static int GetHashCode(IEnumerable source, Func<TElement?, int> elementGetHashCode)
    {
        if (source is not IReadOnlyDictionary<string, TElement?> sourceDictionary)
        {
            throw new InvalidOperationException(
                CosmosStrings.BadDictionaryType(
                    source.GetType().ShortDisplayName(),
                    typeof(IList<>).MakeGenericType(typeof(TElement)).ShortDisplayName()));
        }

        var hash = new HashCode();

        foreach (var pair in sourceDictionary)
        {
            hash.Add(pair.Key);
            hash.Add(pair.Value == null ? 0 : elementGetHashCode(pair.Value));
        }

        return hash.ToHashCode();
    }

    private static IReadOnlyDictionary<string, TElement?> Snapshot(object source, Func<TElement?, TElement?> elementSnapshot)
    {
        if (source is not IReadOnlyDictionary<string, TElement?> sourceDictionary)
        {
            throw new InvalidOperationException(
                CosmosStrings.BadDictionaryType(
                    source.GetType().ShortDisplayName(),
                    typeof(IDictionary<,>).MakeGenericType(typeof(string), typeof(TElement)).ShortDisplayName()));
        }

        var snapshot = new Dictionary<string, TElement?>();
        foreach (var pair in sourceDictionary)
        {
            snapshot[pair.Key] = pair.Value == null ? default : (TElement?)elementSnapshot(pair.Value);
        }

        return snapshot;
    }
}
