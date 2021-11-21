// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Extension methods for <see cref="ValueComparer" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-comparers">EF Core value comparers</see> for more information and examples.
/// </remarks>
public static class ValueComparerExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the given <see cref="ValueComparer" /> is a default EF Core implementation.
    /// </summary>
    /// <param name="valueComparer">The value comparer.</param>
    /// <returns><see langword="true" /> if the value comparer is the default; <see langword="false" /> otherwise.</returns>
    public static bool IsDefault(this ValueComparer valueComparer)
        => valueComparer.GetType().IsGenericType
            && valueComparer.GetType().GetGenericTypeDefinition() == typeof(ValueComparer.DefaultValueComparer<>);
}
