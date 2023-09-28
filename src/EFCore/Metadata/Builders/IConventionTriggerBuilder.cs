// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides an API point for provider-specific extensions for configuring a <see cref="IConventionTrigger" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface IConventionTriggerBuilder : IConventionAnnotatableBuilder
{
    /// <summary>
    ///     The trigger being configured.
    /// </summary>
    new IConventionTrigger Metadata { get; }
}
