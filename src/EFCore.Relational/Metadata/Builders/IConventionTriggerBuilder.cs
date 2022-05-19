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

    /// <summary>
    ///     Sets the database name of the trigger.
    /// </summary>
    /// <param name="name">The database name of the trigger.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The same builder instance if the configuration was applied, <see langword="null" /> otherwise.</returns>
    IConventionTriggerBuilder? HasName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns a value indicating whether the given name can be set for the trigger.
    /// </summary>
    /// <param name="name">The database name of the trigger.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the database name can be set for the trigger.</returns>
    bool CanSetName(string? name, bool fromDataAnnotation = false);
}
