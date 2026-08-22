// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents key facet overrides for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IMutableRelationalKeyOverrides : IReadOnlyRelationalKeyOverrides, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the key that the overrides are for.
    /// </summary>
    new IMutableKey Key { get; }

    /// <summary>
    ///     Gets or sets the key constraint name that the key maps to when targeting the specified table-like store object.
    /// </summary>
    new string? Name { get; set; }

    /// <summary>
    ///     Removes the key constraint name override.
    /// </summary>
    void RemoveNameOverride();
}
