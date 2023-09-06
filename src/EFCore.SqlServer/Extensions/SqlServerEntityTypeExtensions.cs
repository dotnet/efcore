// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Entity type extension methods for SQL Server-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerEntityTypeExtensions
{
    private const string DefaultHistoryTableNameSuffix = "History";

    #region Memory-optimized table

    /// <summary>
    ///     Returns a value indicating whether the entity type is mapped to a memory-optimized table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns><see langword="true" /> if the entity type is mapped to a memory-optimized table.</returns>
    public static bool IsMemoryOptimized(this IReadOnlyEntityType entityType)
        => entityType[SqlServerAnnotationNames.MemoryOptimized] as bool? ?? false;

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a memory-optimized table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="memoryOptimized">The value to set.</param>
    public static void SetIsMemoryOptimized(this IMutableEntityType entityType, bool memoryOptimized)
        => entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.MemoryOptimized, memoryOptimized);

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a memory-optimized table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="memoryOptimized">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsMemoryOptimized(
        this IConventionEntityType entityType,
        bool? memoryOptimized,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
            SqlServerAnnotationNames.MemoryOptimized,
            memoryOptimized,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the memory-optimized setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the memory-optimized setting.</returns>
    public static ConfigurationSource? GetIsMemoryOptimizedConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.MemoryOptimized)?.GetConfigurationSource();

    #endregion Memory-optimized table

    #region Temporal table

    /// <summary>
    ///     Returns a value indicating whether the entity type is mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns><see langword="true" /> if the entity type is mapped to a temporal table.</returns>
    public static bool IsTemporal(this IReadOnlyEntityType entityType)
        => entityType[SqlServerAnnotationNames.IsTemporal] as bool? ?? false;

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="temporal">The value to set.</param>
    public static void SetIsTemporal(this IMutableEntityType entityType, bool temporal)
        => entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.IsTemporal, temporal);

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="temporal">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsTemporal(
        this IConventionEntityType entityType,
        bool? temporal,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
            SqlServerAnnotationNames.IsTemporal,
            temporal,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the temporal table setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the temporal table setting.</returns>
    public static ConfigurationSource? GetIsTemporalConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.IsTemporal)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value representing the name of the period start property of the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Name of the period start property.</returns>
    public static string? GetPeriodStartPropertyName(this IReadOnlyEntityType entityType)
        => (entityType is RuntimeEntityType)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : entityType[SqlServerAnnotationNames.TemporalPeriodStartPropertyName] as string;

    /// <summary>
    ///     Sets a value representing the name of the period start property of the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="periodStartPropertyName">The value to set.</param>
    public static void SetPeriodStartPropertyName(this IMutableEntityType entityType, string? periodStartPropertyName)
        => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalPeriodStartPropertyName, periodStartPropertyName);

    /// <summary>
    ///     Sets a value representing the name of the period start property of the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="periodStartPropertyName">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetPeriodStartPropertyName(
        this IConventionEntityType entityType,
        string? periodStartPropertyName,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetAnnotation(
            SqlServerAnnotationNames.TemporalPeriodStartPropertyName,
            periodStartPropertyName,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the temporal table period start property name setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the temporal table period start property name setting.</returns>
    public static ConfigurationSource? GetPeriodStartPropertyNameConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalPeriodStartPropertyName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value representing the name of the period end property of the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Name of the period start property.</returns>
    public static string? GetPeriodEndPropertyName(this IReadOnlyEntityType entityType)
        => (entityType is RuntimeEntityType)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : entityType[SqlServerAnnotationNames.TemporalPeriodEndPropertyName] as string;

    /// <summary>
    ///     Sets a value representing the name of the period end property of the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="periodEndPropertyName">The value to set.</param>
    public static void SetPeriodEndPropertyName(this IMutableEntityType entityType, string? periodEndPropertyName)
        => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalPeriodEndPropertyName, periodEndPropertyName);

    /// <summary>
    ///     Sets a value representing the name of the period end property of the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="periodEndPropertyName">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetPeriodEndPropertyName(
        this IConventionEntityType entityType,
        string? periodEndPropertyName,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetAnnotation(
            SqlServerAnnotationNames.TemporalPeriodEndPropertyName,
            periodEndPropertyName,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the temporal table period end property name setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the temporal table period end property name setting.</returns>
    public static ConfigurationSource? GetPeriodEndPropertyNameConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalPeriodEndPropertyName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value representing the name of the history table associated with the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Name of the history table.</returns>
    public static string? GetHistoryTableName(this IReadOnlyEntityType entityType)
        => (entityType is RuntimeEntityType)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : entityType[SqlServerAnnotationNames.TemporalHistoryTableName] is string historyTableName
                ? historyTableName
                : entityType[SqlServerAnnotationNames.IsTemporal] as bool? == true
                    ? entityType.GetTableName() is string tableName
                        ? tableName + DefaultHistoryTableNameSuffix
                        : null
                    : null;

    /// <summary>
    ///     Sets a value representing the name of the history table associated with the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="historyTableName">The value to set.</param>
    public static void SetHistoryTableName(this IMutableEntityType entityType, string? historyTableName)
        => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalHistoryTableName, historyTableName);

    /// <summary>
    ///     Sets a value representing the name of the history table associated with the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="historyTableName">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetHistoryTableName(
        this IConventionEntityType entityType,
        string? historyTableName,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetAnnotation(
            SqlServerAnnotationNames.TemporalHistoryTableName,
            historyTableName,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the temporal history table name setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the temporal history table name setting.</returns>
    public static ConfigurationSource? GetHistoryTableNameConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalHistoryTableName)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value representing the schema of the history table associated with the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>Name of the history table.</returns>
    public static string? GetHistoryTableSchema(this IReadOnlyEntityType entityType)
        => (entityType is RuntimeEntityType)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : entityType[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string ?? entityType.GetSchema();

    /// <summary>
    ///     Sets a value representing the schema of the history table associated with the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="historyTableSchema">The value to set.</param>
    public static void SetHistoryTableSchema(this IMutableEntityType entityType, string? historyTableSchema)
        => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalHistoryTableSchema, historyTableSchema);

    /// <summary>
    ///     Sets a value representing the schema of the history table associated with the entity mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="historyTableSchema">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static string? SetHistoryTableSchema(
        this IConventionEntityType entityType,
        string? historyTableSchema,
        bool fromDataAnnotation = false)
        => (string?)entityType.SetAnnotation(
            SqlServerAnnotationNames.TemporalHistoryTableSchema,
            historyTableSchema,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the temporal history table schema setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the temporal history table schema setting.</returns>
    public static ConfigurationSource? GetHistoryTableSchemaConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalHistoryTableSchema)?.GetConfigurationSource();

    #endregion Temporal table

    #region SQL OUTPUT clause

    /// <summary>
    ///     Returns a value indicating whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns><see langword="true" /> if the SQL OUTPUT clause is used to save changes to the table.</returns>
    public static bool IsSqlOutputClauseUsed(this IReadOnlyEntityType entityType)
    {
        if (entityType.FindAnnotation(SqlServerAnnotationNames.UseSqlOutputClause) is { Value: bool useSqlOutputClause })
        {
            return useSqlOutputClause;
        }

        if (entityType.FindOwnership() is { } ownership
            && StoreObjectIdentifier.Create(entityType, StoreObjectType.Table) is { } tableIdentifier
            && ownership.FindSharedObjectRootForeignKey(tableIdentifier) is { } rootForeignKey)
        {
            return rootForeignKey.PrincipalEntityType.IsSqlOutputClauseUsed();
        }

        if (entityType.BaseType is not null && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        {
            return entityType.GetRootType().IsSqlOutputClauseUsed();
        }

        return true;
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlOutputClause">The value to set.</param>
    public static void UseSqlOutputClause(this IMutableEntityType entityType, bool? useSqlOutputClause)
        => entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.UseSqlOutputClause, useSqlOutputClause);

    /// <summary>
    ///     Sets a value indicating whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlOutputClause">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? UseSqlOutputClause(
        this IConventionEntityType entityType,
        bool? useSqlOutputClause,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
            SqlServerAnnotationNames.UseSqlOutputClause,
            useSqlOutputClause,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for whether to use the SQL OUTPUT clause when saving changes to the table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the memory-optimized setting.</returns>
    public static ConfigurationSource? GetUseSqlOutputClauseConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SqlServerAnnotationNames.UseSqlOutputClause)?.GetConfigurationSource();

    /// <summary>
    ///     Returns a value indicating whether to use the SQL OUTPUT clause when saving changes to the specified table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <returns>A value indicating whether the SQL OUTPUT clause is used to save changes to the associated table.</returns>
    public static bool IsSqlOutputClauseUsed(this IReadOnlyEntityType entityType, in StoreObjectIdentifier storeObject)
    {
        if (entityType.FindMappingFragment(storeObject) is { } overrides
            && overrides.FindAnnotation(SqlServerAnnotationNames.UseSqlOutputClause) is { Value: bool useSqlOutputClause })
        {
            return useSqlOutputClause;
        }

        if (StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject)
        {
            return entityType.IsSqlOutputClauseUsed();
        }

        if (entityType.FindOwnership() is { } ownership
            && ownership.FindSharedObjectRootForeignKey(storeObject) is { } rootForeignKey)
        {
            return rootForeignKey.PrincipalEntityType.IsSqlOutputClauseUsed(storeObject);
        }

        if (entityType.BaseType is not null && entityType.GetMappingStrategy() == RelationalAnnotationNames.TphMappingStrategy)
        {
            return entityType.GetRootType().IsSqlOutputClauseUsed(storeObject);
        }

        return true;
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlOutputClause">The value to set.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    public static void UseSqlOutputClause(
        this IMutableEntityType entityType,
        bool? useSqlOutputClause,
        in StoreObjectIdentifier storeObject)
    {
        if (StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject)
        {
            entityType.UseSqlOutputClause(useSqlOutputClause);
            return;
        }

        entityType
            .GetOrCreateMappingFragment(storeObject)
            .UseSqlOutputClause(useSqlOutputClause);
    }

    /// <summary>
    ///     Sets a value indicating whether to use the SQL OUTPUT clause when saving changes to the table.
    ///     The OUTPUT clause is incompatible with certain SQL Server features, such as tables with triggers.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="useSqlOutputClause">The value to set.</param>
    /// <param name="storeObject">The identifier of the table-like store object.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? UseSqlOutputClause(
        this IConventionEntityType entityType,
        bool? useSqlOutputClause,
        in StoreObjectIdentifier storeObject,
        bool fromDataAnnotation = false)
        => StoreObjectIdentifier.Create(entityType, storeObject.StoreObjectType) == storeObject
            ? entityType.UseSqlOutputClause(useSqlOutputClause, fromDataAnnotation)
            : entityType
                .GetOrCreateMappingFragment(storeObject, fromDataAnnotation)
                .UseSqlOutputClause(useSqlOutputClause, fromDataAnnotation);

    #endregion SQL OUTPUT clause
}
