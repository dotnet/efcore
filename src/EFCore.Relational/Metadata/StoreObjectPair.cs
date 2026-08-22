// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Identifies a foreign key constraint by the pair of store objects it connects.
/// </summary>
/// <remarks>
///     A foreign key constraint name is a relationship between two tables, not a property of one:
///     a single model foreign key can materialize as several constraints, one per dependent table.
/// </remarks>
/// <param name="DependentStoreObject">The store object holding the constraint.</param>
/// <param name="PrincipalStoreObject">The store object the constraint references.</param>
public readonly record struct StoreObjectPair(
    StoreObjectIdentifier DependentStoreObject,
    StoreObjectIdentifier PrincipalStoreObject)
{
    /// <summary>
    ///     Returns a readable representation of the pair.
    /// </summary>
    /// <returns>The display string.</returns>
    public override string ToString()
        => $"{DependentStoreObject.DisplayName()} -> {PrincipalStoreObject.DisplayName()}";
}
