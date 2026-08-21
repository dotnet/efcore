// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a SQL Server full-text catalog in the model in a form that
///     can be mutated while building the model.
/// </summary>
/// <remarks>
///     See <see href="https://learn.microsoft.com/sql/relational-databases/search/full-text-search">Full-Text Search</see>
///     for more information on SQL Server full-text search.
/// </remarks>
public interface IConventionSqlServerFullTextCatalog : IReadOnlySqlServerFullTextCatalog, IConventionAnnotatable
{
    /// <summary>
    ///     Gets the <see cref="IConventionModel" /> in which this full-text catalog is defined.
    /// </summary>
    new IConventionModel Model { get; }

    /// <summary>
    ///     Gets the configuration source for this <see cref="IConventionSqlServerFullTextCatalog" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IConventionSqlServerFullTextCatalog" />.</returns>
    ConfigurationSource GetConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether this is the default full-text catalog for the database.
    /// </summary>
    /// <param name="default">Whether this is the default catalog.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsDefault(bool? @default, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlySqlServerFullTextCatalog.IsDefault" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySqlServerFullTextCatalog.IsDefault" />.</returns>
    ConfigurationSource? GetIsDefaultConfigurationSource();

    /// <summary>
    ///     Sets a value indicating whether the full-text catalog is accent-sensitive.
    /// </summary>
    /// <param name="accentSensitive">Whether the catalog is accent-sensitive.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    bool? SetIsAccentSensitive(bool? accentSensitive, bool fromDataAnnotation = false);

    /// <summary>
    ///     Returns the configuration source for <see cref="IReadOnlySqlServerFullTextCatalog.IsAccentSensitive" />.
    /// </summary>
    /// <returns>The configuration source for <see cref="IReadOnlySqlServerFullTextCatalog.IsAccentSensitive" />.</returns>
    ConfigurationSource? GetIsAccentSensitiveConfigurationSource();
}
