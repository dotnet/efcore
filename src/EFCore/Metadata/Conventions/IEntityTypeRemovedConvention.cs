// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when an entity type is removed from the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IEntityTypeRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after an entity type is removed from the model.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="entityType">The removed entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        IConventionContext<IConventionEntityType> context);
}
