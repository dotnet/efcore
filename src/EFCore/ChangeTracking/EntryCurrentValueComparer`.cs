// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     <para>
///         An implementation of <see cref="IComparer{T}" /> and <see cref="IEqualityComparer{T}" /> to compare current values
///         contained in <see cref="IUpdateEntry" /> internal tracking entities.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
/// </remarks>
/// <typeparam name="TProperty">The type of the property.</typeparam>
public sealed class EntryCurrentValueComparer<TProperty> : IComparer<IUpdateEntry>, IEqualityComparer<IUpdateEntry>
{
    private readonly IPropertyBase _property;
    private readonly IComparer<TProperty> _underlyingComparer;

    /// <summary>
    ///     Creates a new <see cref="EntryCurrentValueComparer" /> instance using a the default comparer for the property type.
    /// </summary>
    /// <param name="property">The property to use for comparisons.</param>
    public EntryCurrentValueComparer(IPropertyBase property)
    {
        _property = property;
        _underlyingComparer = Comparer<TProperty>.Default;
    }

    /// <summary>
    ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>A negative number if 'x' is less than 'y'; a positive number if 'x' is greater than 'y'; zero otherwise.</returns>
    public int Compare(IUpdateEntry? x, IUpdateEntry? y)
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

        return _underlyingComparer.Compare(
            x.GetCurrentValue<TProperty>(_property),
            y.GetCurrentValue<TProperty>(_property));
    }

    /// <summary>
    ///     Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns><see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.</returns>
    public bool Equals(IUpdateEntry? x, IUpdateEntry? y)
        => Compare(x, y) == 0;

    /// <summary>
    ///     Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    public int GetHashCode(IUpdateEntry obj)
        => obj.GetCurrentValue<TProperty>(_property)?.GetHashCode() ?? 0;
}
