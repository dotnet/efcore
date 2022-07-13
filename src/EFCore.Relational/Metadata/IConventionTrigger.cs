// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a database sequence in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public interface IConventionTrigger : IReadOnlyTrigger, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the builder that can be used to configure this trigger.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the trigger has been removed from the model.</exception>
    new IConventionTriggerBuilder Builder { get; }

    /// <summary>
    ///     Gets the <see cref="IConventionEntityType" /> on which this trigger is defined.
    /// </summary>
    new IConventionEntityType EntityType { get; }

    /// <summary>
    ///     Gets the configuration source for this trigger.
    /// </summary>
    /// <returns>The configuration source for this trigger.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the name of the trigger in the database.
    /// </summary>
    /// <param name="name">The name of the trigger in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for the database name.
    /// </summary>
    /// <returns>The configuration source for the database name.</returns>
    ConfigurationSource? GetNameConfigurationSource();
}
