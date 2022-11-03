// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class NullableSingleDimensionalArrayComparer<TElement> : ValueComparer<TElement?[]>
    where TElement : struct
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NullableSingleDimensionalArrayComparer(ValueComparer elementComparer)
        : base(
            (a, b) => Compare(a, b, (ValueComparer<TElement>)elementComparer),
            o => GetHashCode(o, (ValueComparer<TElement>)elementComparer),
            source => Snapshot(source, (ValueComparer<TElement>)elementComparer))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type
        => typeof(TElement?[]);

    private static bool Compare(TElement?[]? a, TElement?[]? b, ValueComparer<TElement> elementComparer)
    {
        if (a is null)
        {
            return b is null;
        }

        if (b is null || a.Length != b.Length)
        {
            return false;
        }

        if (ReferenceEquals(a, b))
        {
            return true;
        }

        for (var i = 0; i < a.Length; i++)
        {
            var (aElement, bElement) = (a[i], b[i]);
            if (aElement is null)
            {
                if (bElement is null)
                {
                    continue;
                }

                return false;
            }

            if (bElement is null || !elementComparer.Equals(aElement, bElement))
            {
                return false;
            }
        }

        return true;
    }

    private static int GetHashCode(TElement?[] source, ValueComparer<TElement> elementComparer)
    {
        var nullableEqualityComparer = new NullableEqualityComparer<TElement>(elementComparer);
        var hash = new HashCode();
        foreach (var el in source)
        {
            hash.Add(el, nullableEqualityComparer);
        }

        return hash.ToHashCode();
    }

    private static TElement?[] Snapshot(TElement?[] source, ValueComparer<TElement> elementComparer)
    {
        var snapshot = new TElement?[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            snapshot[i] = source[i] is { } value ? elementComparer.Snapshot(value) : null;
        }

        return snapshot;
    }
}
