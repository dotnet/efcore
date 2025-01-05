// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     <para>
///         An implementation of <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> to compare
///         <see cref="IReadOnlyEntityType" /> instances by the full unique name.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public sealed class EntityTypeFullNameComparer : IComparer<IReadOnlyEntityType>, IEqualityComparer<IReadOnlyEntityType>
{
    private EntityTypeFullNameComparer()
    {
    }

    /// <summary>
    ///     The singleton instance of the comparer to use.
    /// </summary>
    public static readonly EntityTypeFullNameComparer Instance = new();

    /// <summary>
    ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise.</returns>
    public int Compare(IReadOnlyEntityType? x, IReadOnlyEntityType? y)
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

        return StringComparer.Ordinal.Compare(x.Name, y.Name);
    }

    /// <summary>
    ///     Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns><see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.</returns>
    public bool Equals(IReadOnlyEntityType? x, IReadOnlyEntityType? y)
        => Compare(x, y) == 0;

    /// <summary>
    ///     Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    public int GetHashCode(IReadOnlyEntityType obj)
        => obj.Name.GetHashCode();
}
