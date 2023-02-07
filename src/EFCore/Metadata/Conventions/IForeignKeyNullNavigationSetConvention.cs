// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a navigation is set to <see langword="null" /> on a foreign key.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IForeignKeyNullNavigationSetConvention : IConvention
{
    /// <summary>
    ///     Called after a navigation is set to <see langword="null" /> on a foreign key.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="pointsToPrincipal">
    ///     A value indicating whether the <see langword="null" /> navigation would be pointing to the principal entity type.
    /// </param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessForeignKeyNullNavigationSet(
        IConventionForeignKeyBuilder relationshipBuilder,
        bool pointsToPrincipal,
        IConventionContext<IConventionNavigation> context);
}
