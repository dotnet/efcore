// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents property facet overrides for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyRelationalPropertyOverrides : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the property that the overrides are for.
    /// </summary>
    IReadOnlyProperty Property { get; }

    /// <summary>
    ///     The id of the table-like store object that these overrides are for.
    /// </summary>
    StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     Gets the column that the property maps to when targeting the specified table-like store object.
    /// </summary>
    string? ColumnName { get; }

    /// <summary>
    ///     Gets a value indicating whether the column name is overriden.
    /// </summary>
    bool ColumnNameOverridden { get; }
}
