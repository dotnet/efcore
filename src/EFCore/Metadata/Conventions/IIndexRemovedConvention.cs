// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when an index is removed from the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IIndexRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after an index is removed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="index">The removed index.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessIndexRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionIndex index,
        IConventionContext<IConventionIndex> context);
}
