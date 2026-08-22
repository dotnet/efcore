// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents foreign key facet overrides for a particular pair of table-like store objects.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IConventionRelationalForeignKeyOverrides : IReadOnlyRelationalForeignKeyOverrides, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the foreign key that the overrides are for.
    /// </summary>
    new IConventionForeignKey ForeignKey { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this function.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the function has been removed from the model.</exception>
    new IConventionRelationalForeignKeyOverridesBuilder Builder { get; }

    /// <summary>
    ///     Returns the configuration source for these overrides.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the foreign key constraint name that the foreign key maps to when targeting the specified pair of
    ///     table-like store objects.
    /// </summary>
    /// <param name="name"> The foreign key constraint name. </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the foreign key constraint name override.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A value indicating whether the foreign key constraint name override was removed. </returns>
    bool RemoveNameOverride(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyRelationalForeignKeyOverrides.Name" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyRelationalForeignKeyOverrides.Name" />.</returns>
    ConfigurationSource? GetNameConfigurationSource();
}
