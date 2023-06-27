// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a complex property is removed from a type-like object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IComplexPropertyRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after a complex property is removed from a type-like object.
    /// </summary>
    /// <param name="typeBaseBuilder">The builder for the type-like object.</param>
    /// <param name="property">The removed complex property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessComplexPropertyRemoved(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionComplexProperty property,
        IConventionContext<IConventionComplexProperty> context);
}
