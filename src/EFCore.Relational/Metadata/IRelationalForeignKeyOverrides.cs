// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents foreign key facet overrides for a particular pair of table-like store objects.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IRelationalForeignKeyOverrides : IReadOnlyRelationalForeignKeyOverrides, IAnnotatable
{
    /// <summary>
    ///     Gets the foreign key that the overrides are for.
    /// </summary>
    new IForeignKey ForeignKey { get; }
}
