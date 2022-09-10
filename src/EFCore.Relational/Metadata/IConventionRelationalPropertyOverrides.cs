// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents property facet overrides for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IConventionRelationalPropertyOverrides : IReadOnlyRelationalPropertyOverrides, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the property that the overrides are for.
    /// </summary>
    new IConventionProperty Property { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this function.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the function has been removed from the model.</exception>
    new IConventionRelationalPropertyOverridesBuilder Builder { get; }

    /// <summary>
    ///     Returns the configuration source for these overrides.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets the column that the property maps to when targeting the specified table-like store object.
    /// </summary>
    /// <param name="name"> The column name. </param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    string? SetColumnName(string? name, bool fromDataAnnotation = false);

    /// <summary>
    ///     Removes the column name override.
    /// </summary>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>A value indicating whether the column name override was removed. </returns>
    bool RemoveColumnNameOverride(bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlyRelationalPropertyOverrides.ColumnName" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlyRelationalPropertyOverrides.ColumnName" />.</returns>
    ConfigurationSource? GetColumnNameConfigurationSource();
}
