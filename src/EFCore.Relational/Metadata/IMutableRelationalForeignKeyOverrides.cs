// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents foreign key facet overrides for a particular pair of table-like store objects.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IMutableRelationalForeignKeyOverrides : IReadOnlyRelationalForeignKeyOverrides, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the foreign key that the overrides are for.
    /// </summary>
    new IMutableForeignKey ForeignKey { get; }

    /// <summary>
    ///     Gets or sets the foreign key constraint name that the foreign key maps to when targeting the specified pair of
    ///     table-like store objects.
    /// </summary>
    new string? Name { get; set; }

    /// <summary>
    ///     Removes the foreign key constraint name override.
    /// </summary>
    void RemoveNameOverride();
}
