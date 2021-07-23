// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Entity type extension methods for SQL Server-specific metadata.
    /// </summary>
    public static class SqlServerEntityTypeExtensions
    {
        private const string DefaultHistoryTableNameSuffix = "History";

        /// <summary>
        ///     Returns a value indicating whether the entity type is mapped to a memory-optimized table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> <see langword="true" /> if the entity type is mapped to a memory-optimized table. </returns>
        public static bool IsMemoryOptimized(this IReadOnlyEntityType entityType)
            => entityType[SqlServerAnnotationNames.MemoryOptimized] as bool? ?? false;

        /// <summary>
        ///     Sets a value indicating whether the entity type is mapped to a memory-optimized table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="memoryOptimized"> The value to set. </param>
        public static void SetIsMemoryOptimized(this IMutableEntityType entityType, bool memoryOptimized)
            => entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.MemoryOptimized, memoryOptimized);

        /// <summary>
        ///     Sets a value indicating whether the entity type is mapped to a memory-optimized table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="memoryOptimized"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsMemoryOptimized(
            this IConventionEntityType entityType,
            bool? memoryOptimized,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.MemoryOptimized, memoryOptimized, fromDataAnnotation);

            return memoryOptimized;
        }

        /// <summary>
        ///     Gets the configuration source for the memory-optimized setting.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for the memory-optimized setting. </returns>
        public static ConfigurationSource? GetIsMemoryOptimizedConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(SqlServerAnnotationNames.MemoryOptimized)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value indicating whether the entity type is mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> <see langword="true" /> if the entity type is mapped to a temporal table. </returns>
        public static bool IsTemporal(this IReadOnlyEntityType entityType)
            => entityType[SqlServerAnnotationNames.IsTemporal] as bool? ?? false;

        /// <summary>
        ///     Sets a value indicating whether the entity type is mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="temporal"> The value to set. </param>
        public static void SetIsTemporal(this IMutableEntityType entityType, bool temporal)
            => entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.IsTemporal, temporal);

        /// <summary>
        ///     Sets a value indicating whether the entity type is mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="temporal"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsTemporal(
            this IConventionEntityType entityType,
            bool? temporal,
            bool fromDataAnnotation = false)
        {
            entityType.SetOrRemoveAnnotation(SqlServerAnnotationNames.IsTemporal, temporal, fromDataAnnotation);

            return temporal;
        }

        /// <summary>
        ///     Gets the configuration source for the temporal table setting.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for the temporal table setting. </returns>
        public static ConfigurationSource? GetIsTemporalConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(SqlServerAnnotationNames.IsTemporal)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value representing the name of the period start property of the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Name of the period start property. </returns>
        public static string? GetTemporalPeriodStartPropertyName(this IReadOnlyEntityType entityType)
            => entityType[SqlServerAnnotationNames.TemporalPeriodStartPropertyName] as string;

        /// <summary>
        ///     Sets a value representing the name of the period start property of the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="periodStartPropertyName"> The value to set. </param>
        public static void SetTemporalPeriodStartPropertyName(this IMutableEntityType entityType, string? periodStartPropertyName)
            => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalPeriodStartPropertyName, periodStartPropertyName);

        /// <summary>
        ///     Sets a value representing the name of the period start property of the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="periodStartPropertyName"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetTemporalPeriodStartPropertyName(
            this IConventionEntityType entityType,
            string? periodStartPropertyName,
            bool fromDataAnnotation = false)
        {
            entityType.SetAnnotation(
                SqlServerAnnotationNames.TemporalPeriodStartPropertyName,
                periodStartPropertyName,
                fromDataAnnotation);

            return periodStartPropertyName;
        }

        /// <summary>
        ///     Gets the configuration source for the temporal table period start property name setting.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for the temporal table period start property name setting. </returns>
        public static ConfigurationSource? GetTemporalPeriodStartPropertyNameConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalPeriodStartPropertyName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value representing the name of the period end property of the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Name of the period start property. </returns>
        public static string? GetTemporalPeriodEndPropertyName(this IReadOnlyEntityType entityType)
            => entityType[SqlServerAnnotationNames.TemporalPeriodEndPropertyName] as string;

        /// <summary>
        ///     Sets a value representing the name of the period end property of the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="periodEndPropertyName"> The value to set. </param>
        public static void SetTemporalPeriodEndPropertyName(this IMutableEntityType entityType, string? periodEndPropertyName)
            => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalPeriodEndPropertyName, periodEndPropertyName);

        /// <summary>
        ///     Sets a value representing the name of the period end property of the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="periodEndPropertyName"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetTemporalPeriodEndPropertyName(
            this IConventionEntityType entityType,
            string? periodEndPropertyName,
            bool fromDataAnnotation = false)
        {
            entityType.SetAnnotation(
                SqlServerAnnotationNames.TemporalPeriodEndPropertyName,
                periodEndPropertyName,
                fromDataAnnotation);

            return periodEndPropertyName;
        }

        /// <summary>
        ///     Gets the configuration source for the temporal table period end property name setting.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for the temporal table period end property name setting. </returns>
        public static ConfigurationSource? GetTemporalPeriodEndPropertyNameConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalPeriodEndPropertyName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value representing the name of the history table associated with the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Name of the history table. </returns>
        public static string? GetTemporalHistoryTableName(this IReadOnlyEntityType entityType)
            => entityType[SqlServerAnnotationNames.TemporalHistoryTableName] is string historyTableName
                ? historyTableName
                : entityType[SqlServerAnnotationNames.IsTemporal] as bool? == true
                    ? entityType.ShortName() + DefaultHistoryTableNameSuffix
                    : null;

        /// <summary>
        ///     Sets a value representing the name of the history table associated with the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="historyTableName"> The value to set. </param>
        public static void SetTemporalHistoryTableName(this IMutableEntityType entityType, string? historyTableName)
            => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalHistoryTableName, historyTableName);

        /// <summary>
        ///     Sets a value representing the name of the history table associated with the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="historyTableName"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetTemporalHistoryTableName(
            this IConventionEntityType entityType,
            string? historyTableName,
            bool fromDataAnnotation = false)
        {
            entityType.SetAnnotation(
                SqlServerAnnotationNames.TemporalHistoryTableName,
                historyTableName,
                fromDataAnnotation);

            return historyTableName;
        }

        /// <summary>
        ///     Gets the configuration source for the temporal history table name setting.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for the temporal history table name setting. </returns>
        public static ConfigurationSource? GetTemporalHistoryTableNameConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalHistoryTableName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value representing the schema of the history table associated with the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> Name of the history table. </returns>
        public static string? GetTemporalHistoryTableSchema(this IReadOnlyEntityType entityType)
            => entityType[SqlServerAnnotationNames.TemporalHistoryTableSchema] as string
                ?? entityType[RelationalAnnotationNames.Schema] as string;

        /// <summary>
        ///     Sets a value representing the schema of the history table associated with the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="historyTableSchema"> The value to set. </param>
        public static void SetTemporalHistoryTableSchema(this IMutableEntityType entityType, string? historyTableSchema)
            => entityType.SetAnnotation(SqlServerAnnotationNames.TemporalHistoryTableSchema, historyTableSchema);

        /// <summary>
        ///     Sets a value representing the schema of the history table associated with the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="historyTableSchema"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetTemporalHistoryTableSchema(
            this IConventionEntityType entityType,
            string? historyTableSchema,
            bool fromDataAnnotation = false)
        {
            entityType.SetAnnotation(
                SqlServerAnnotationNames.TemporalHistoryTableSchema,
                historyTableSchema,
                fromDataAnnotation);

            return historyTableSchema;
        }

        /// <summary>
        ///     Gets the configuration source for the temporal history table schema setting.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The configuration source for the temporal history table schema setting. </returns>
        public static ConfigurationSource? GetTemporalHistoryTableSchemaConfigurationSource(this IConventionEntityType entityType)
            => entityType.FindAnnotation(SqlServerAnnotationNames.TemporalHistoryTableSchema)?.GetConfigurationSource();
    }
}
