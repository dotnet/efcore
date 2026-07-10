// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the embedded discriminator name for the model changes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IModelEmbeddedDiscriminatorNameConvention : IConvention
{
    /// <summary>
    ///     Called after <see cref="ModelBuilder.HasEmbeddedDiscriminatorName" /> has been called.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="newName">The new discriminator name.</param>
    /// <param name="oldName">The current discriminator name.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessEmbeddedDiscriminatorName(
        IConventionModelBuilder modelBuilder,
        string? newName,
        string? oldName,
        IConventionContext<string> context);
}
