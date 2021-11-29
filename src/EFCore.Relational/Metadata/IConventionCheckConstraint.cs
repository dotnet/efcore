// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a check constraint on the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
/// </remarks>
public interface IConventionCheckConstraint : IReadOnlyCheckConstraint, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the builder that can be used to configure this check constraint.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the check constraint has been removed from the model.</exception>
    new IConventionCheckConstraintBuilder Builder { get; }

    /// <summary>
    ///     Gets the entity type on which this check constraint is defined.
    /// </summary>
    new IConventionEntityType EntityType { get; }

    /// <summary>
    ///     Gets the configuration source for this check constraint.
    /// </summary>
    /// <returns>The configuration source for this check constraint.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the name of the check constraint in the database.
    /// </summary>
    /// <param name="name">The name of the check constraint in the database.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the configuration source for the database name.
    /// </summary>
    /// <returns>The configuration source for the database name.</returns>
    ConfigurationSource? GetNameConfigurationSource();
}
