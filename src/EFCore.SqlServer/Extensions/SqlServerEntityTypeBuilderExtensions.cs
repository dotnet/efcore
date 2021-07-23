// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class SqlServerEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder IsMemoryOptimized(
            this EntityTypeBuilder entityTypeBuilder,
            bool memoryOptimized = true)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.SetIsMemoryOptimized(memoryOptimized);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> IsMemoryOptimized<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            bool memoryOptimized = true)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)IsMemoryOptimized((EntityTypeBuilder)entityTypeBuilder, memoryOptimized);

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <param name="collectionOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder IsMemoryOptimized(
            this OwnedNavigationBuilder collectionOwnershipBuilder,
            bool memoryOptimized = true)
        {
            Check.NotNull(collectionOwnershipBuilder, nameof(collectionOwnershipBuilder));

            collectionOwnershipBuilder.OwnedEntityType.SetIsMemoryOptimized(memoryOptimized);

            return collectionOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="collectionOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> IsMemoryOptimized<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> collectionOwnershipBuilder,
            bool memoryOptimized = true)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)IsMemoryOptimized(
                (OwnedNavigationBuilder)collectionOwnershipBuilder, memoryOptimized);

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? IsMemoryOptimized(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            bool? memoryOptimized,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetIsMemoryOptimized(memoryOptimized, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetIsMemoryOptimized(memoryOptimized, fromDataAnnotation);
                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the mapped table can be configured as memory-optimized.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the mapped table can be configured as memory-optimized. </returns>
        public static bool CanSetIsMemoryOptimized(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            bool? memoryOptimized,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(SqlServerAnnotationNames.MemoryOptimized, memoryOptimized, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the table as temporal.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity being configured. </param>
        /// <param name="temporal"> A value indicating whether the table is temporal. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? IsTemporal(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            bool temporal = true,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetIsTemporal(temporal, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetIsTemporal(temporal, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the mapped table can be configured as temporal.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="temporal"> A value indicating whether the table is temporal. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the mapped table can be configured as temporal. </returns>
        public static bool CanSetIsTemporal(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            bool temporal = true,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(SqlServerAnnotationNames.IsTemporal, temporal, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures a history table name for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity being configured. </param>
        /// <param name="name"> The name of the history table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? WithHistoryTableName(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetHistoryTableName(name, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetTemporalHistoryTableName(name, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given history table name can be set for the entity.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the history table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the mapped table can have history table name. </returns>
        public static bool CanSetHistoryTableName(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(SqlServerAnnotationNames.TemporalHistoryTableName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures a history table schema for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity being configured. </param>
        /// <param name="schema"> The schema of the history table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? WithHistoryTableSchema(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? schema,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetHistoryTableSchema(schema, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetTemporalHistoryTableSchema(schema, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the mapped table can have history table schema.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the history table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the mapped table can have history table schema. </returns>
        public static bool CanSetHistoryTableSchema(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? schema,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(SqlServerAnnotationNames.TemporalHistoryTableSchema, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures a period start property for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity being configured. </param>
        /// <param name="propertyName"> The name of the period start property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? HasPeriodStart(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? propertyName,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetPeriodStart(propertyName, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetTemporalPeriodStartPropertyName(propertyName, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the mapped table can have period start property.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="propertyName"> The name of the period start property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the mapped table can have period start property. </returns>
        public static bool CanSetPeriodStart(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? propertyName,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(SqlServerAnnotationNames.TemporalPeriodStartPropertyName, propertyName, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures a period end property for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity being configured. </param>
        /// <param name="propertyName"> The name of the period end property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? HasPeriodEnd(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? propertyName,
            bool fromDataAnnotation = false)
        {
            if (entityTypeBuilder.CanSetPeriodEnd(propertyName, fromDataAnnotation))
            {
                entityTypeBuilder.Metadata.SetTemporalPeriodEndPropertyName(propertyName, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the mapped table can have period end property.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="propertyName"> The name of the period end property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the mapped table can have period end property. </returns>
        public static bool CanSetPeriodEnd(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? propertyName,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(SqlServerAnnotationNames.TemporalPeriodEndPropertyName, propertyName, fromDataAnnotation);
        }
    }
}
