// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQLite specific extension methods for <see cref="IReadOnlyEntityTypeMappingFragment" />.
/// </summary>
public static class SqliteEntityTypeMappingFragmentExtensions
{
    /// <summary>
    ///     Returns a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <returns>The configured value.</returns>
    public static bool IsSqlReturningClauseUsed(this IReadOnlyEntityTypeMappingFragment fragment)
        => fragment.FindAnnotation(SqliteAnnotationNames.UseSqlReturningClause) is not { Value: false };

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    public static void UseSqlReturningClause(this IMutableEntityTypeMappingFragment fragment, bool? useSqlReturningClause)
        => fragment.SetAnnotation(SqliteAnnotationNames.UseSqlReturningClause, useSqlReturningClause);

    /// <summary>
    ///     Sets a value indicating whether to use the SQL RETURNING clause when saving changes to the table.
    ///     The RETURNING clause is incompatible with certain Sqlite features, such as virtual tables or tables with AFTER triggers.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <param name="useSqlReturningClause">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? UseSqlReturningClause(
        this IConventionEntityTypeMappingFragment fragment,
        bool? useSqlReturningClause,
        bool fromDataAnnotation = false)
        => (bool?)fragment.SetAnnotation(SqliteAnnotationNames.UseSqlReturningClause, useSqlReturningClause, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for whether to use the SQL RETURNING clause when saving changes to the associated table.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <returns>The configuration source for the configured value.</returns>
    public static ConfigurationSource? GetUseSqlReturningClauseConfigurationSource(this IConventionEntityTypeMappingFragment fragment)
        => fragment.FindAnnotation(SqliteAnnotationNames.UseSqlReturningClause)?.GetConfigurationSource();
}
