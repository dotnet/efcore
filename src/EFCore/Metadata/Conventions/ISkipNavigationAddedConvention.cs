// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a skip navigation is added to the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface ISkipNavigationAddedConvention : IConvention
{
    /// <summary>
    ///     Called after a skip navigation is added to the entity type.
    /// </summary>
    /// <param name="skipNavigationBuilder">The builder for the skip navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionContext<IConventionSkipNavigationBuilder> context);
}
