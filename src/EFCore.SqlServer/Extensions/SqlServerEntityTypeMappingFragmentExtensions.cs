// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for <see cref="IReadOnlyEntityTypeMappingFragment" />.
/// </summary>
public static class SqlServerEntityTypeMappingFragmentExtensions
{
    /// <summary>
    ///     Returns a value indicating whether to use the SQL OUTPUT clause when saving changes to the associated table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <returns>The configured value.</returns>
    public static bool IsSqlOutputClauseUsed(this IReadOnlyEntityTypeMappingFragment fragment)
        => fragment.FindAnnotation(SqlServerAnnotationNames.UseSqlOutputClause) is not { Value: false };

    /// <summary>
    ///     Sets whether to use the SQL OUTPUT clause when saving changes to the associated table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <param name="useSqlOutputClause">The value to set.</param>
    public static void UseSqlOutputClause(this IMutableEntityTypeMappingFragment fragment, bool? useSqlOutputClause)
        => fragment.SetAnnotation(SqlServerAnnotationNames.UseSqlOutputClause, useSqlOutputClause);

    /// <summary>
    ///     Sets whether to use the SQL OUTPUT clause when saving changes to the associated table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <param name="useSqlOutputClause">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? UseSqlOutputClause(
        this IConventionEntityTypeMappingFragment fragment,
        bool? useSqlOutputClause,
        bool fromDataAnnotation = false)
        => (bool?)fragment.SetAnnotation(SqlServerAnnotationNames.UseSqlOutputClause, useSqlOutputClause, fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the setting whether to use the SQL OUTPUT clause when saving changes to the associated table.
    /// </summary>
    /// <param name="fragment">The entity type mapping fragment.</param>
    /// <returns>The configuration source for the configured value.</returns>
    public static ConfigurationSource? GetUseSqlOutputClauseConfigurationSource(this IConventionEntityTypeMappingFragment fragment)
        => fragment.FindAnnotation(SqlServerAnnotationNames.UseSqlOutputClause)?.GetConfigurationSource();
}
