// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class NullableListComparer<TElement, TCollection> : ValueComparer<TCollection>
    where TCollection : class, IEnumerable<TElement?>
    where TElement : struct
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NullableListComparer(ValueComparer elementComparer, bool readOnly)
        : base(
            (a, b) => Compare(a, b, (ValueComparer<TElement>)elementComparer),
            o => GetHashCode(o, (ValueComparer<TElement>)elementComparer),
            source => Snapshot(source, (ValueComparer<TElement>)elementComparer, readOnly))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type Type
        => typeof(TCollection);

    private static bool Compare(TCollection? a, TCollection? b, ValueComparer<TElement> elementComparer)
    {
        if (a is not IReadOnlyList<TElement?> aList)
        {
            return b is not IReadOnlyList<TElement?>;
        }

        if (b is not IReadOnlyList<TElement?> bList || aList.Count != bList.Count)
        {
            return false;
        }

        if (ReferenceEquals(aList, bList))
        {
            return true;
        }

        for (var i = 0; i < aList.Count; i++)
        {
            var (aElement, bElement) = (aList[i], bList[i]);
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

    private static int GetHashCode(TCollection source, ValueComparer<TElement> elementComparer)
    {
        var nullableEqualityComparer = new NullableEqualityComparer<TElement>(elementComparer);
        var hash = new HashCode();
        foreach (var el in source)
        {
            hash.Add(el, nullableEqualityComparer);
        }

        return hash.ToHashCode();
    }

    private static TCollection Snapshot(TCollection source, ValueComparer<TElement> elementComparer, bool readOnly)
    {
        if (readOnly)
        {
            return source;
        }

        var snapshot = new List<TElement?>(((IReadOnlyList<TElement?>)source).Count);
        foreach (var e in source)
        {
            snapshot.Add(e is null ? null : elementComparer.Snapshot(e.Value));
        }

        return (TCollection)(object)snapshot;
    }
}
