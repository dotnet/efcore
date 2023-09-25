// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a discriminator property is set for an entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IDiscriminatorPropertySetConvention : IConvention
{
    /// <summary>
    ///     Called after a discriminator property is set.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The name of the discriminator property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessDiscriminatorPropertySet(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string? name,
        IConventionContext<string?> context);
}
