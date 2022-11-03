// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a check constraint on the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
/// </remarks>
public interface IMutableCheckConstraint : IReadOnlyCheckConstraint, IMutableAnnotatable
{
    /// <summary>
    ///     Gets the entity type on which this check constraint is defined.
    /// </summary>
    new IMutableEntityType EntityType { get; }

    /// <summary>
    ///     Gets or sets the name of the check constraint in the database.
    /// </summary>
    new string? Name { get; set; }
}
