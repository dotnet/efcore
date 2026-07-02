// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public readonly struct UnnamedIndexKey : IEquatable<UnnamedIndexKey>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public UnnamedIndexKey(
        IReadOnlyList<IReadOnlyPropertyBase> properties,
        IReadOnlyList<IReadOnlyList<int?>?>? collectionIndices = null)
    {
        Properties = properties;
        CollectionIndices = collectionIndices;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<IReadOnlyPropertyBase> Properties { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<int?>?>? CollectionIndices { get; }

    /// <inheritdoc />
    public bool Equals(UnnamedIndexKey other)
        => Comparer.Compare(this, other) == 0;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is UnnamedIndexKey other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var property in Properties)
        {
            hash.Add(property);
        }

        if (CollectionIndices is null)
        {
            return hash.ToHashCode();
        }

        foreach (var entry in CollectionIndices)
        {
            if (entry is null)
            {
                continue;
            }

            foreach (var value in entry)
            {
                hash.Add(value);
            }
        }

        return hash.ToHashCode();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly UnnamedIndexKeyComparer Comparer = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Returns a negative value when <paramref name="x" /> sorts before <paramref name="y" />, zero when
    ///     they are equal, and a positive value otherwise. <see langword="null" /> sorts before any non-null
    ///     value so that "plain" indexes (no collection traversal) come first.
    /// </remarks>
    public static int CompareCollectionIndices(
        IReadOnlyList<IReadOnlyList<int?>?>? x,
        IReadOnlyList<IReadOnlyList<int?>?>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var countDiff = x.Count - y.Count;
        if (countDiff != 0)
        {
            return countDiff;
        }

        for (var i = 0; i < x.Count; i++)
        {
            var innerResult = CompareInner(x[i], y[i]);
            if (innerResult != 0)
            {
                return innerResult;
            }
        }

        return 0;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Equivalent to <c><see cref="CompareCollectionIndices" />(left, right) == 0</c>; provided as a
    ///     dedicated entry point for hot paths that only need an equality answer.
    /// </remarks>
    public static bool CollectionIndicesEqual(
        IReadOnlyList<IReadOnlyList<int?>?>? left,
        IReadOnlyList<IReadOnlyList<int?>?>? right)
        => CompareCollectionIndices(left, right) == 0;

    private static int CompareInner(IReadOnlyList<int?>? x, IReadOnlyList<int?>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var countDiff = x.Count - y.Count;
        if (countDiff != 0)
        {
            return countDiff;
        }

        for (var i = 0; i < x.Count; i++)
        {
            var a = x[i];
            var b = y[i];
            if (a is null != b is null)
            {
                return a is null ? -1 : 1;
            }

            if (a is not null && b is not null && a.Value != b.Value)
            {
                return a.Value < b.Value ? -1 : 1;
            }
        }

        return 0;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public sealed class UnnamedIndexKeyComparer : IComparer<UnnamedIndexKey>
    {
        /// <inheritdoc />
        public int Compare(UnnamedIndexKey x, UnnamedIndexKey y)
        {
            var result = PropertyListComparer.Instance.Compare(x.Properties, y.Properties);
            if (result != 0)
            {
                return result;
            }

            return CompareCollectionIndices(x.CollectionIndices, y.CollectionIndices);
        }
    }
}
