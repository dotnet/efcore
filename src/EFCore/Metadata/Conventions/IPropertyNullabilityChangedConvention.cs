// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the nullability for a property is changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IPropertyNullabilityChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the nullability for a property is changed.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessPropertyNullabilityChanged(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<bool?> context);
}
