// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the primary key for an entity type is changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IEntityTypePrimaryKeyChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the primary key for an entity type is changed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newPrimaryKey">The new primary key.</param>
    /// <param name="previousPrimaryKey">The old primary key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessEntityTypePrimaryKeyChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey? newPrimaryKey,
        IConventionKey? previousPrimaryKey,
        IConventionContext<IConventionKey> context);
}
