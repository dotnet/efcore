// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IEntityType" /> for relational database metadata.
    /// </summary>
    public static class RelationalEntityTypeExtensions
    {
        /// <summary>
        ///     Returns the name of the table to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The name of the table to which the entity type is mapped. </returns>
        public static string GetTableName([NotNull] this IEntityType entityType) =>
            entityType.BaseType != null
                ? entityType.GetRootType().GetTableName()
                : (string)entityType[RelationalAnnotationNames.TableName] ?? GetDefaultTableName(entityType);

        /// <summary>
        ///     Returns the default table name that would be used for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The default name of the table to which the entity type would be mapped. </returns>
        public static string GetDefaultTableName([NotNull] this IEntityType entityType)
        {
            var ownership = entityType.FindOwnership();
            if (ownership != null
                && ownership.IsUnique)
            {
                return ownership.PrincipalEntityType.GetTableName();
            }

            return Uniquifier.Truncate(
                entityType.HasDefiningNavigation()
                    ? $"{entityType.DefiningEntityType.GetTableName()}_{entityType.DefiningNavigationName}"
                    : entityType.ShortName(),
                entityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the name of the table to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the table name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetTableName([NotNull] this IMutableEntityType entityType, [CanBeNull] string name)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.TableName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the name of the table to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the table name for. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetTableName(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] string name, bool fromDataAnnotation = false)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.TableName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the table name.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the table name. </returns>
        public static ConfigurationSource? GetTableNameConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.TableName)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the database schema that contains the mapped table.
        /// </summary>
        /// <param name="entityType"> The entity type to get the schema for. </param>
        /// <returns> The database schema that contains the mapped table. </returns>
        public static string GetSchema([NotNull] this IEntityType entityType) =>
            entityType.BaseType != null
                ? entityType.GetRootType().GetSchema()
                : (string)entityType[RelationalAnnotationNames.Schema] ?? GetDefaultSchema(entityType);

        /// <summary>
        ///     Returns the default database schema that would be used for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the table name for. </param>
        /// <returns> The default database schema to which the entity type would be mapped. </returns>
        public static string GetDefaultSchema([NotNull] this IEntityType entityType)
        {
            var ownership = entityType.FindOwnership();
            if (ownership != null
                && ownership.IsUnique)
            {
                return ownership.PrincipalEntityType.GetSchema();
            }

            return entityType.HasDefiningNavigation()
                ? entityType.DefiningEntityType.GetSchema()
                : entityType.Model.GetDefaultSchema();
        }

        /// <summary>
        ///     Sets the database schema that contains the mapped table.
        /// </summary>
        /// <param name="entityType"> The entity type to set the schema for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetSchema([NotNull] this IMutableEntityType entityType, [CanBeNull] string value)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Schema,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the database schema that contains the mapped table.
        /// </summary>
        /// <param name="entityType"> The entity type to set the schema for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetSchema(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] string value, bool fromDataAnnotation = false)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.Schema,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the database schema.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the database schema. </returns>
        public static ConfigurationSource? GetSchemaConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.Schema)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Finds an <see cref="ICheckConstraint" /> with the given name.
        /// </summary>
        /// <param name="entityType"> The entity type to find the check constraint for. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <returns>
        ///     The <see cref="ICheckConstraint" /> or <c>null</c> if no check constraint with the
        ///     given name in the given entity type was found.
        /// </returns>
        public static ICheckConstraint FindCheckConstraint(
            [NotNull] this IEntityType entityType, [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return CheckConstraint.FindCheckConstraint(entityType, name);
        }

        /// <summary>
        ///     Finds an <see cref="IConventionCheckConstraint" /> with the given name.
        /// </summary>
        /// <param name="entityType"> The entity type to find the check constraint for. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <returns>
        ///     The <see cref="IConventionCheckConstraint" /> or <c>null</c> if no check constraint with the
        ///     given name in the given entity type was found.
        /// </returns>
        public static IConventionCheckConstraint FindCheckConstraint(
            [NotNull] this IConventionEntityType entityType, [NotNull] string name)
            => (IConventionCheckConstraint)((IEntityType)entityType).FindCheckConstraint(name);

        /// <summary>
        ///     Creates a new check constraint with the given name on entity type. Throws an exception
        ///     if a check constraint with the same name exists on the same entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the check constraint to. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <returns> The new check constraint. </returns>
        public static ICheckConstraint AddCheckConstraint(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] string name,
            [NotNull] string sql)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(sql, nameof(sql));

            return new CheckConstraint(entityType, name, sql, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     Creates a new check constraint with the given name on entity type. Throws an exception
        ///     if a check constraint with the same name exists on the same entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to add the check constraint to. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <param name="sql"> The logical constraint sql used in the check constraint. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The new check constraint. </returns>
        public static IConventionCheckConstraint AddCheckConstraint(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] string name,
            [NotNull] string sql,
            bool fromDataAnnotation = false)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(sql, nameof(sql));

            return new CheckConstraint(
                (IMutableEntityType)entityType, name, sql,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
        }

        /// <summary>
        ///     Removes the <see cref="ICheckConstraint" /> with the given name.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the check constraint from. </param>
        /// <param name="name"> The check constraint name to be removed. </param>
        /// <returns>
        ///     True if the <see cref="ICheckConstraint" /> is successfully found and removed; otherwise, false.
        /// </returns>
        public static bool RemoveCheckConstraint(
            [NotNull] this IMutableEntityType entityType,
            [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return CheckConstraint.RemoveCheckConstraint(entityType, name);
        }

        /// <summary>
        ///     Removes the <see cref="IConventionCheckConstraint" /> with the given name.
        /// </summary>
        /// <param name="entityType"> The entity type to remove the check constraint from. </param>
        /// <param name="name"> The check constraint name. </param>
        /// <returns>
        ///     True if the <see cref="IConventionCheckConstraint" /> is successfully found and removed; otherwise, false.
        /// </returns>
        public static bool RemoveCheckConstraint(
            [NotNull] this IConventionEntityType entityType,
            [NotNull] string name)
            => RemoveCheckConstraint((IMutableEntityType)entityType, name);

        /// <summary>
        ///     Returns all <see cref="ICheckConstraint" /> contained in the entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to get the check constraints for. </param>
        public static IEnumerable<ICheckConstraint> GetCheckConstraints([NotNull] this IEntityType entityType)
            => CheckConstraint.GetCheckConstraints(entityType);

        /// <summary>
        ///     Returns the comment for the column this property is mapped to.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <returns> The comment for the column this property is mapped to. </returns>
        public static string GetComment([NotNull] this IEntityType entityType)
            => (string)entityType[RelationalAnnotationNames.Comment];

        /// <summary>
        ///     Configures a comment to be applied to the column this property is mapped to.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="comment"> The comment for the column. </param>
        public static void SetComment([NotNull] this IMutableEntityType entityType, [CanBeNull] string comment)
            => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment);

        /// <summary>
        ///     Configures a comment to be applied to the column this property is mapped to.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetComment(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] string comment, bool fromDataAnnotation = false)
            => entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment, fromDataAnnotation);

        /// <summary>
        ///     Gets a value indicating whether the entity type is ignored by Migrations.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <returns>A value indicating whether the entity type is ignored by Migrations.</returns>
        public static bool IsIgnoredByMigrations([NotNull] this IEntityType entityType)
        {
            if (entityType.BaseType != null)
            {
                return entityType.BaseType.IsIgnoredByMigrations();
            }

            if (entityType.GetDefiningQuery() != null)
            {
                return true;
            }

            var viewDefinition = entityType.FindAnnotation(RelationalAnnotationNames.ViewDefinition);
            if (viewDefinition == null)
            {
                var ownership = entityType.FindOwnership();
                if (ownership != null
                    && ownership.IsUnique
                    && entityType.FindAnnotation(RelationalAnnotationNames.TableName) == null)
                {
                    return ownership.PrincipalEntityType.IsIgnoredByMigrations();
                }

                return false;
            }

            return viewDefinition.Value == null;
        }
    }
}
