// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyEntityTypeMappingFragment : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the entity type for which the fragment is defined.
    /// </summary>
    IReadOnlyEntityType EntityType { get; }

    /// <summary>
    ///     Gets store object for which the configuration is applied.
    /// </summary>
    StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     Gets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <returns>A value indicating whether the associated table is ignored by Migrations.</returns>
    bool? IsTableExcludedFromMigrations { get; }
}
