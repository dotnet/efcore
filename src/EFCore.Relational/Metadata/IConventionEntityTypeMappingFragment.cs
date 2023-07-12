// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IConventionEntityTypeMappingFragment : IReadOnlyEntityTypeMappingFragment, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the entity type for which the fragment is defined.
    /// </summary>
    new IConventionEntityType EntityType { get; }

    /// <summary>
    ///     Gets the builder that can be used to configure this fragment.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the fragment has been removed from the model.</exception>
    new IConventionEntityTypeMappingFragmentBuilder Builder { get; }

    /// <summary>
    ///     Returns the configuration source for this fragment.
    /// </summary>
    /// <returns>The configuration source.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <param name="excluded">A value indicating whether the associated table is ignored by Migrations.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    bool? SetIsTableExcludedFromMigrations(bool? excluded, bool fromDataAnnotation = false);

    /// <summary>
    ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IReadOnlyEntityTypeMappingFragment.IsTableExcludedFromMigrations" />.
    /// </summary>
    /// <returns>The <see cref="ConfigurationSource" /> for <see cref="IReadOnlyEntityTypeMappingFragment.IsTableExcludedFromMigrations" />.</returns>
    ConfigurationSource? GetIsTableExcludedFromMigrationsConfigurationSource();
}
