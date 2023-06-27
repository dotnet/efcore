// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a type is ignored.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface ITypeIgnoredConvention : IConvention
{
    /// <summary>
    ///     Called after an entity type is ignored.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="name">The name of the ignored type.</param>
    /// <param name="type">The ignored type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessTypeIgnored(
        IConventionModelBuilder modelBuilder,
        string name,
        Type? type,
        IConventionContext<string> context);
}
