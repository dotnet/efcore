// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the <see cref="IElementType" /> for a property is changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IPropertyElementTypeChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the element type for a property is changed.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="newElementType">The new element type.</param>
    /// <param name="oldElementType">The old element type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessPropertyElementTypeChanged(
        IConventionPropertyBuilder propertyBuilder,
        IElementType? newElementType,
        IElementType? oldElementType,
        IConventionContext<IElementType> context);
}
