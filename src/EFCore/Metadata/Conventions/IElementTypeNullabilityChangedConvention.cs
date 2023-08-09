// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when the nullability on the elements of a collection property has changed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IElementTypeNullabilityChangedConvention : IConvention
{
    /// <summary>
    ///     Called after the nullability for an <see cref="IConventionElementType" /> is changed.
    /// </summary>
    /// <param name="builder">The builder for the element.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessElementTypeNullabilityChanged(
        IConventionElementTypeBuilder builder,
        IConventionContext<bool?> context);
}
