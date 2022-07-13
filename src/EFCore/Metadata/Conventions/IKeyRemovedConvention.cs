// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a key is removed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IKeyRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after a key is removed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="key">The removed key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey key,
        IConventionContext<IConventionKey> context);
}
