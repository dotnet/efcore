// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.RemoveAnnotation(RelationalAnnotationNames.ViewDefinition);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);
            entityTypeBuilder.Metadata.RemoveAnnotation(RelationalAnnotationNames.ViewDefinition);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name, schema);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            [NotNull] this OwnedNavigationBuilder referenceOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable((OwnedNavigationBuilder)referenceOwnershipBuilder, name);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ToTable(
            [NotNull] this OwnedNavigationBuilder referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            referenceOwnershipBuilder.OwnedEntityType.SetTableName(name);
            referenceOwnershipBuilder.OwnedEntityType.SetSchema(schema);

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
            [NotNull] this OwnedNavigationBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            where TRelatedEntity : class
            => (OwnedNavigationBuilder<TEntity, TRelatedEntity>)ToTable((OwnedNavigationBuilder)referenceOwnershipBuilder, name, schema);

        /// <summary>
        ///     Configures the table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ToTable(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
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
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ToTable(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name, [CanBeNull] string schema,
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
        ///     Returns a value indicating whether the view or table name can be set for this entity type
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetTable(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.TableName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the schema of the view or table that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ToSchema(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string schema,
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
        ///     Returns a value indicating whether the schema of the view or table name can be set for this entity type
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetSchema(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(schema, nameof(schema));

            return entityTypeBuilder.CanSetAnnotation(RelationalAnnotationNames.Schema, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ToView(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetAnnotation(RelationalAnnotationNames.ViewDefinition, null);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the view that the entity type maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToView<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
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
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.Metadata.SetTableName(name);
            entityTypeBuilder.Metadata.SetSchema(schema);
            entityTypeBuilder.Metadata.SetAnnotation(RelationalAnnotationNames.ViewDefinition, null);

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
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToView((EntityTypeBuilder)entityTypeBuilder, name, schema);

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder HasCheckConstraint(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var entityType = entityTypeBuilder.Metadata;

            var constraint = entityType.FindCheckConstraint(name);
            if (constraint != null)
            {
                if (constraint.Sql == sql)
                {
                    return entityTypeBuilder;
                }

                entityType.RemoveCheckConstraint(name);
            }

            if (sql != null)
            {
                entityType.AddCheckConstraint(name, sql);
            }

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
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)HasCheckConstraint((EntityTypeBuilder)entityTypeBuilder, name, sql);

        /// <summary>
        ///     Configures a database check constraint when targeting a relational database.
        /// </summary>
        /// <param name="entityTypeBuilder"> The entity type builder. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the check constraint was configured,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder HasCheckConstraint(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var entityType = entityTypeBuilder.Metadata;

            var constraint = entityType.FindCheckConstraint(name);
            if (constraint != null)
            {
                if (constraint.Sql == sql)
                {
                    ((CheckConstraint)constraint).UpdateConfigurationSource(
                         fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
                    return entityTypeBuilder;
                }

                if (!(fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                    .Overrides(constraint.GetConfigurationSource()))
                {
                    return null;
                }

                entityType.RemoveCheckConstraint(name);
            }

            if (sql != null)
            {
                entityType.AddCheckConstraint(name, sql, fromDataAnnotation);
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the check constraint can be configured.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the check constraint. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetCheckConstraint(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(sql, nameof(sql));

            var constraint = entityTypeBuilder.Metadata.FindCheckConstraint(name);

            return constraint == null
                   || constraint.Sql == sql
                   || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                   .Overrides(constraint.GetConfigurationSource());
        }

        /// <summary>
        ///     Configures a comment to be applied to the table
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <returns> A builder to further configure the entity type. </returns>
        public static EntityTypeBuilder HasComment(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string comment)
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
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string comment)
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
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder HasComment(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string comment,
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
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="comment"> The comment for the table. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetComment(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string comment,
            bool fromDataAnnotation = false)
            => entityTypeBuilder.CanSetAnnotation(
                RelationalAnnotationNames.Comment,
                comment,
                fromDataAnnotation);

    }
}
