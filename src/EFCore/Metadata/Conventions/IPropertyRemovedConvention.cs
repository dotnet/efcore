// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a property is removed from the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IPropertyRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after a property is removed from the entity type.
    /// </summary>
    /// <param name="typeBaseBuilder">The builder for the entity type that contained the property.</param>
    /// <param name="property">The removed property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property,
        IConventionContext<IConventionProperty> context);
}
