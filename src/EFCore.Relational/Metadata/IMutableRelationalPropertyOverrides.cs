// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents property facet overrides for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IMutableRelationalPropertyOverrides : IReadOnlyRelationalPropertyOverrides, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the property that the overrides are for.
    /// </summary>
    new IMutableProperty Property { get; }

    /// <summary>
    ///     Gets or sets the column that the property maps to when targeting the specified table-like store object.
    /// </summary>
    new string? ColumnName { get; set; }

    /// <summary>
    ///     Removes the column name override.
    /// </summary>
    void RemoveColumnNameOverride();
}
