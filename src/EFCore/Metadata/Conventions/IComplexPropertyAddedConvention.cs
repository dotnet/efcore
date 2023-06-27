// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a complex property is added to an type-like object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IComplexPropertyAddedConvention : IConvention
{
    /// <summary>
    ///     Called after a complex property is added to a type-like object.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the complex property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context);
}
