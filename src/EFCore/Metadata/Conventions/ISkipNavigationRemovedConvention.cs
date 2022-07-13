// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a skip navigation is removed from the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface ISkipNavigationRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after a skip navigation is removed from the entity type.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type that contained the navigation.</param>
    /// <param name="navigation">The removed navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessSkipNavigationRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionSkipNavigation navigation,
        IConventionContext<IConventionSkipNavigation> context);
}
