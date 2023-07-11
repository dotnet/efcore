// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a complex type member is ignored.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IComplexTypeMemberIgnoredConvention : IConvention
{
    /// <summary>
    ///     Called after a complex type member is ignored.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="name">The name of the ignored member.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessComplexTypeMemberIgnored(
        IConventionComplexTypeBuilder complexTypeBuilder,
        string name,
        IConventionContext<string> context);
}
