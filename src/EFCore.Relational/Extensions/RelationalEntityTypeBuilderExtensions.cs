// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class RelationalEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            this EntityTypeBuilder entityTypeBuilder,
            string? name)
            => entityTypeBuilder.ToTable(name, (string?)null);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            this EntityTypeBuilder entityTypeBuilder,
            Action<TableBuilder> buildAction)
        {
            buildAction(new TableBuilder(null, null, entityTypeBuilder));

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            this EntityTypeBuilder entityTypeBuilder,
            string name,
            Action<TableBuilder> buildAction)
            => entityTypeBuilder.ToTable(name, null, buildAction);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name)
            where TEntity : class
            => entityTypeBuilder.ToTable(name, (string?)null);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            Action<TableBuilder<TEntity>> buildAction)
            where TEntity : class
        {
            buildAction(new TableBuilder<TEntity>(null, null, entityTypeBuilder));

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            Action<TableBuilder> buildAction)
            where TEntity : class
            => entityTypeBuilder.ToTable(name, null, buildAction);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            Action<TableBuilder<TEntity>> buildAction)
            where TEntity : class
            => entityTypeBuilder.ToTable(name, null, buildAction);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            this EntityTypeBuilder entityTypeBuilder,
            string? name,
            string? schema)
        {
            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            this EntityTypeBuilder entityTypeBuilder,
            string name,
            string? schema,
            Action<TableBuilder> buildAction)
        {
            Check.NotNull(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            buildAction(new TableBuilder(name, schema, entityTypeBuilder));
            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            string? schema,
            Action<TableBuilder> buildAction)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)entityTypeBuilder).ToTable(name, schema, buildAction);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name,
            string? schema)
            where TEntity : class
        {
            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="buildAction"> An action that performs configuration of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            string? schema,
            Action<TableBuilder<TEntity>> buildAction)
            where TEntity : class
        {
            Check.NotNull(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            buildAction(new TableBuilder<TEntity>(name, schema, entityTypeBuilder));
            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            this OwnedNavigationBuilder referenceOwnershipBuilder,
            string? name)
            => ToTable(referenceOwnershipBuilder, name, schema: null, excludedFromMigrations: null);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="excludedFromMigrations"> A value indicating whether the table should be managed by migrations. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            this OwnedNavigationBuilder referenceOwnershipBuilder,
            string? name,
            bool excludedFromMigrations)
            => referenceOwnershipBuilder.ToTable(name, null, excludedFromMigrations);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            string? name)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable(
                referenceOwnershipBuilder, name, schema: null, excludedFromMigrations: null);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="excludedFromMigrations"> A value indicating whether the table should be managed by migrations. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            string? name,
            bool excludedFromMigrations)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable(
                (OwnedNavigationBuilder)referenceOwnershipBuilder, name, excludedFromMigrations);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            this OwnedNavigationBuilder referenceOwnershipBuilder,
            string? name,
            string? schema)
            => ToTable(referenceOwnershipBuilder, name, schema, excludedFromMigrations: null);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="excludedFromMigrations"> A value indicating whether the table should be managed by migrations. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            this OwnedNavigationBuilder referenceOwnershipBuilder,
            string? name,
            string? schema,
            bool excludedFromMigrations)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return ToTable(referenceOwnershipBuilder, name, schema, (bool?)excludedFromMigrations);
        }

        private static OwnedNavigationBuilder ToTable(
            OwnedNavigationBuilder referenceOwnershipBuilder,
            string? name,
            string? schema,
            bool? excludedFromMigrations)
        {
            referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
            referenceOwnershipBuilder.OwnedEntityType.SetSchema(schema);

            if (excludedFromMigrations.HasValue)
            {
                referenceOwnershipBuilder.OwnedEntityType.SetIsTableExcludedFromMigrations(excludedFromMigrations.Value);
            }

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            string? name,
            string? schema)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable(
                referenceOwnershipBuilder, name, schema, excludedFromMigrations: null);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="excludedFromMigrations"> A value indicating whether the table should be managed by migrations. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            string? name,
            string? schema,
            bool excludedFromMigrations)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable(
                (OwnedNavigationBuilder)referenceOwnershipBuilder, name, schema, excludedFromMigrations);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToTable(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetTable(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetTableName(name, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToTable(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            string? schema,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetTable(name, fromDataAnnotation)
                || !entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetTableName(name, fromDataAnnotation);
            entityTypeBuilder.Metadata.SetSchema(schema, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the table name can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetTable(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.TableName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the schema of the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToSchema(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? schema,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetSchema(schema, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the schema of the table name can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetSchema(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? schema,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(schema, nameof(schema));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.Schema, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Mark the table that this entity type is mapped to as excluded from migrations.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="excludedFromMigrations"> A value indicating whether the table should be managed by migrations. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ExcludeTableFromMigrations(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            bool? excludedFromMigrations,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanExcludeTableFromMigrations(excludedFromMigrations, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetIsTableExcludedFromMigrations(excludedFromMigrations, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the table that this entity type is mapped to can be excluded from migrations
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="excludedFromMigrations"> A value indicating whether the table should be managed by migrations. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanExcludeTableFromMigrations(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            bool? excludedFromMigrations,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.CanSetAnnotation
                (RelationalAnnotationNames.IsTableExcludedFromMigrations, excludedFromMigrations, fromDataAnnotation);

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToView(
            this EntityTypeBuilder entityTypeBuilder,
            string? name)
            => entityTypeBuilder.ToView(name, null);

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToView<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToView(
            this EntityTypeBuilder entityTypeBuilder,
            string? name,
            string? schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.Metadata.SetViewName(name);
            entityTypeBuilder.Metadata.SetViewSchema(schema);
            entityTypeBuilder.Metadata.SetAnnotation(RelationalAnnotationNames.ViewDefinitionSql, null);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToView<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name,
            string? schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name, schema);

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToView(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetView(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetViewName(name, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToView(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            string? schema,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetView(name, fromDataAnnotation)
                || !entityTypeBuilder.CanSetViewSchema(schema, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetViewName(name, fromDataAnnotation);
            entityTypeBuilder.Metadata.SetViewSchema(schema, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the view name can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetView(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.ViewName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the schema of the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToViewSchema(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? schema,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetSchema(schema, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetViewSchema(schema, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the schema of the view can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the view. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetViewSchema(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? schema,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(schema, nameof(schema));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.ViewSchema, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures a SQL string used to provide data for the entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="query"> The SQL query that will provide the underlying data for the entity type. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToSqlQuery(
            this EntityTypeBuilder entityTypeBuilder,
            string query)
        {
            Check.NotNull(query, nameof(query));

            entityTypeBuilder.Metadata.SetSqlQuery(query);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a SQL string used to provide data for the entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="query"> The SQL query that will provide the underlying data for the entity type. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToSqlQuery<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string query)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToSqlQuery((EntityTypeBuilder)entityTypeBuilder, query);

        /// <summary>
        ///     Configures a SQL string used to provide data for the entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The SQL query that will provide the underlying data for the entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToSqlQuery(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetSqlQuery(name, fromDataAnnotation))
            {
                return null;
            }

            var entityType = entityTypeBuilder.Metadata;
            entityType.SetSqlQuery(name, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the query SQL string can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The SQL query that will provide the underlying data for the entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetSqlQuery(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.SqlQuery, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <returns> The function configuration builder. </returns>
        public static EntityTypeBuilder ToFunction(
            this EntityTypeBuilder entityTypeBuilder,
            string? name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            SetFunction(name, entityTypeBuilder.Metadata);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="configureFunction"> The function configuration action. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToFunction(
            this EntityTypeBuilder entityTypeBuilder,
            string name,
            Action<TableValuedFunctionBuilder> configureFunction)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NotNull(configureFunction, nameof(configureFunction));

            configureFunction(new TableValuedFunctionBuilder(SetFunction(name, entityTypeBuilder.Metadata)));

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <returns> The function configuration builder. </returns>
        public static EntityTypeBuilder<TEntity> ToFunction<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToFunction((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="configureFunction"> The function configuration action. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToFunction<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            Action<TableValuedFunctionBuilder> configureFunction)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToFunction((EntityTypeBuilder)entityTypeBuilder, name, configureFunction);

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="ownedNavigationBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <returns> The function configuration builder. </returns>
        public static OwnedNavigationBuilder ToFunction(
            this OwnedNavigationBuilder ownedNavigationBuilder,
            string? name)
        {
            Check.NotNull(ownedNavigationBuilder, nameof(ownedNavigationBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            SetFunction(name, ownedNavigationBuilder.OwnedEntityType);

            return ownedNavigationBuilder;
        }

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="ownedNavigationBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="configureFunction"> The function configuration action. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToFunction(
            this OwnedNavigationBuilder ownedNavigationBuilder,
            string name,
            Action<TableValuedFunctionBuilder> configureFunction)
        {
            Check.NotNull(ownedNavigationBuilder, nameof(ownedNavigationBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NotNull(configureFunction, nameof(configureFunction));

            configureFunction(new TableValuedFunctionBuilder(SetFunction(name, ownedNavigationBuilder.OwnedEntityType)));

            return ownedNavigationBuilder;
        }

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <returns> The function configuration builder. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToFunction<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            string? name)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToFunction((OwnedNavigationBuilder)referenceOwnershipBuilder, name);

        /// <summary>
        ///     Configures the function that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="configureFunction"> The function configuration action. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToFunction<TEntity, TRelatedEntity>(
            this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            string name,
            Action<TableValuedFunctionBuilder> configureFunction)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToFunction(
                (OwnedNavigationBuilder)referenceOwnershipBuilder, name, configureFunction);

        [return: NotNullIfNotNull("name")]
        private static IMutableDbFunction? SetFunction(string? name, IMutableEntityType entityType)
        {
            entityType.SetFunctionName(name);

            var model = entityType.Model;
            var function = name is not null
                ? model.FindDbFunction(name) ?? model.AddDbFunction(
                    name, typeof(IQueryable<>).MakeGenericType(entityType.ClrType))
                : null;

            return function;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToFunction(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetFunction(name, fromDataAnnotation))
            {
                return null;
            }

            var entityType = entityTypeBuilder.Metadata;
            entityType.SetFunctionName(name, fromDataAnnotation);

            if (name is null)
            {
                entityType.Model.Builder.HasDbFunction(name, typeof(IQueryable<>).MakeGenericType(entityType.ClrType), fromDataAnnotation);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the view or table name can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetFunction(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.FunctionName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder HasCheckConstraint(
            this EntityTypeBuilder entityTypeBuilder,
            string name,
            string? sql)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            InternalCheckConstraintBuilder.HasCheckConstraint(
                  (IConventionEntityType)entityTypeBuilder.Metadata,
                  name,
                  sql,
                  ConfigurationSource.Explicit);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="buildAction"> An action that performs configuration of the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder HasCheckConstraint(
            this EntityTypeBuilder entityTypeBuilder,
            string name,
            string sql,
            Action<CheckConstraintBuilder> buildAction)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(buildAction, nameof(buildAction));

            entityTypeBuilder.HasCheckConstraint(name, sql);

            buildAction(new CheckConstraintBuilder(entityTypeBuilder.Metadata.FindCheckConstraint(name)!));

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            string? sql)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasCheckConstraint((EntityTypeBuilder)entityTypeBuilder, name, sql);

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="buildAction"> An action that performs configuration of the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string name,
            string sql,
            Action<CheckConstraintBuilder> buildAction)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasCheckConstraint((EntityTypeBuilder)entityTypeBuilder, name, sql, buildAction);

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the check constraint was configured,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionCheckConstraintBuilder? HasCheckConstraint(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            string? sql,
            bool fromDataAnnotation = false)
            => InternalCheckConstraintBuilder.HasCheckConstraint(
                Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)).Metadata,
                name,
                sql,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                ?.Builder;

        /// <summary>
        ///     Returns a value indicating whether the check constraint can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        [Obsolete("Use CanHaveCheckConstraint")]
        public static bool CanSetCheckConstraint(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            string? sql,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.CanHaveCheckConstraint(name, sql, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the check constraint can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanHaveCheckConstraint(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            string? sql,
            bool fromDataAnnotation = false)
            => InternalCheckConstraintBuilder.CanHaveCheckConstraint(
                Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)).Metadata,
                name,
                sql,
                fromDataAnnotation? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Configures a comment to be applied to the table
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder HasComment(
            this EntityTypeBuilder entityTypeBuilder,
            string? comment)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Metadata.SetComment(comment);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a comment to be applied to the table
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder<TEntity> HasComment<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? comment)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasComment((EntityTypeBuilder)entityTypeBuilder, comment);

        /// <summary>
        ///     Configures a comment to be applied to the table
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? HasComment(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? comment,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            if (!entityTypeBuilder.CanSetComment(comment, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetComment(comment, fromDataAnnotation);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether a comment can be set for this entity type
        ///     using the specified configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetComment(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? comment,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.CanSetAnnotation(
                RelationalAnnotationNames.Comment,
                comment,
                fromDataAnnotation);
    }
}
