// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IConventionRelationalKeyOverrides" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionRelationalKeyOverridesBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The overrides being configured.
    /// </summary>
    new IConventionRelationalKeyOverrides Metadata { get; }
}
