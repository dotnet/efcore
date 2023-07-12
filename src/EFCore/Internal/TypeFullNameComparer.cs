// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class TypeFullNameComparer : IComparer<Type>, IEqualityComparer<Type>
{
    private TypeFullNameComparer()
    {
    }

    /// <summary>
    ///     The singleton instance of the comparer to use.
    /// </summary>
    public static readonly TypeFullNameComparer Instance = new();

    /// <summary>
    ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise.</returns>
    public int Compare(Type? x, Type? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x == null)
        {
            return -1;
        }

        if (y == null)
        {
            return 1;
        }

        return StringComparer.Ordinal.Compare(x.FullName, y.FullName);
    }

    /// <summary>
    ///     Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns><see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.</returns>
    public bool Equals(Type? x, Type? y)
        => Compare(x, y) == 0;

    /// <summary>
    ///     Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    public int GetHashCode(Type obj)
        => obj.Name.GetHashCode();
}
