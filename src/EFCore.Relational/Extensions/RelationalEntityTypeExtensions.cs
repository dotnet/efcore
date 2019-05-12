// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
                ? entityType.RootType().GetTableName()
                : (string)entityType[RelationalAnnotationNames.TableName]
                  ?? GetDefaultTableName(entityType);

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

            return IdentifierHelpers.Truncate(
                entityType.HasDefiningNavigation()
                    ? $"{entityType.DefiningEntityType.GetTableName()}_{entityType.DefiningNavigationName}"
                    : entityType.ShortName(),
                entityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Returns the name of the view to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to get the view name for. </param>
        /// <returns> The name of the view to which the entity type is mapped. </returns>
        public static string GetViewName([NotNull] this IEntityType entityType) =>
            entityType.BaseType != null
                ? entityType.RootType().GetViewName()
                : (string)entityType[RelationalAnnotationNames.ViewName]
                  ?? GetTableName(entityType);

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
        ///     Sets the name of the view to which the entity type is mapped.
        /// </summary>
        /// <param name="entityType"> The entity type to set the view name for. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetViewName([NotNull] this IMutableEntityType entityType, [CanBeNull] string name)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ViewName,
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
                ? entityType.RootType().GetSchema()
                : (string)entityType[RelationalAnnotationNames.Schema]
                  ?? GetDefaultSchema(entityType);

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
        ///     Returns the database schema that contains the mapped view.
        /// </summary>
        /// <param name="entityType"> The entity type to get the schema for. </param>
        /// <returns> The database schema that contains the mapped view. </returns>
        public static string GetViewSchemaName([NotNull] this IEntityType entityType) =>
            entityType.BaseType != null
                ? entityType.RootType().GetViewSchemaName()
                : (string)entityType[RelationalAnnotationNames.ViewSchemaName]
                  ?? GetSchema(entityType);

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
        ///     Sets the database schema that contains the mapped view.
        /// </summary>
        /// <param name="entityType"> The entity type to set the schema for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetViewSchemaName([NotNull] this IMutableEntityType entityType, [CanBeNull] string value)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ViewSchemaName,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the database schema that contains the mapped view.
        /// </summary>
        /// <param name="entityType"> The entity type to set the schema for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetViewSchemaName(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] string value, bool fromDataAnnotation = false)
            => entityType.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ViewSchemaName,
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
        ///     Returns the <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to get the discriminator property for. </param>
        public static IProperty GetDiscriminatorProperty([NotNull] this IEntityType entityType)
        {
            if (entityType.BaseType != null)
            {
                return entityType.RootType().GetDiscriminatorProperty();
            }

            var propertyName = (string)entityType[RelationalAnnotationNames.DiscriminatorProperty];

            return propertyName == null ? null : entityType.FindProperty(propertyName);
        }

        /// <summary>
        ///     Sets the <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator property for. </param>
        /// <param name="property"> The property to set. </param>
        public static void SetDiscriminatorProperty([NotNull] this IMutableEntityType entityType, [CanBeNull] IProperty property)
        {
            CheckDiscriminatorProperty(entityType, property);

            var oldDiscriminatorType = entityType.GetDiscriminatorProperty()?.ClrType;
            if (property == null
                || property.ClrType != oldDiscriminatorType)
            {
                foreach (var derivedType in entityType.GetDerivedTypesInclusive())
                {
                    derivedType.RemoveDiscriminatorValue();
                }
            }

            entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.DiscriminatorProperty, property?.Name);
        }

        /// <summary>
        ///     Sets the <see cref="IProperty" /> that will be used for storing a discriminator value.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator property for. </param>
        /// <param name="property"> The property to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDiscriminatorProperty(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] IProperty property, bool fromDataAnnotation = false)
        {
            CheckDiscriminatorProperty(entityType, property);

            if (property != null
                && !property.ClrType.IsInstanceOfType(entityType.GetDiscriminatorValue()))
            {
                foreach (var derivedType in entityType.GetDerivedTypesInclusive())
                {
                    derivedType.RemoveDiscriminatorValue();
                }
            }

            entityType.SetOrRemoveAnnotation(RelationalAnnotationNames.DiscriminatorProperty, property?.Name, fromDataAnnotation);
        }

        private static void CheckDiscriminatorProperty(IEntityType entityType, IProperty property)
        {
            if (property != null)
            {
                if (entityType != entityType.RootType())
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DiscriminatorPropertyMustBeOnRoot(entityType.DisplayName()));
                }

                if (property.DeclaringEntityType != entityType)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.DiscriminatorPropertyNotFound(property.Name, entityType.DisplayName()));
                }
            }
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the discriminator property.
        /// </summary>
        /// <param name="entityType"> The entity type to find configuration source for. </param>
        /// <returns> The <see cref="ConfigurationSource" /> or <c>null</c> if no discriminator property has been set. </returns>
        public static ConfigurationSource? GetDiscriminatorPropertyConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.DiscriminatorProperty)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to find the discriminator value for. </param>
        /// <returns> The discriminator value for this entity type. </returns>
        public static object GetDiscriminatorValue([NotNull] this IEntityType entityType)
            => entityType[RelationalAnnotationNames.DiscriminatorValue];

        /// <summary>
        ///     Sets the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator value for. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDiscriminatorValue([NotNull] this IMutableEntityType entityType, [CanBeNull] object value)
        {
            CheckDiscriminatorValue(entityType, value);

            entityType.SetAnnotation(RelationalAnnotationNames.DiscriminatorValue, value);
        }

        /// <summary>
        ///     Sets the discriminator value for this entity type.
        /// </summary>
        /// <param name="entityType"> The entity type to set the discriminator value for. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetDiscriminatorValue(
            [NotNull] this IConventionEntityType entityType, [CanBeNull] object value, bool fromDataAnnotation = false)
        {
            CheckDiscriminatorValue(entityType, value);

            entityType.SetAnnotation(RelationalAnnotationNames.DiscriminatorValue, value, fromDataAnnotation);
        }

        private static void CheckDiscriminatorValue(IEntityType entityType, object value)
        {
            if (value != null
                && entityType.GetDiscriminatorProperty() == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.NoDiscriminatorForValue(entityType.DisplayName(), entityType.RootType().DisplayName()));
            }

            if (value != null
                && !entityType.GetDiscriminatorProperty().ClrType.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    RelationalStrings.DiscriminatorValueIncompatible(
                        value, entityType.GetDiscriminatorProperty().Name, entityType.GetDiscriminatorProperty().ClrType));
            }
        }

        /// <summary>
        ///     Removes the discriminator value for this entity type.
        /// </summary>
        public static void RemoveDiscriminatorValue([NotNull] this IMutableEntityType entityType)
            => entityType.RemoveAnnotation(RelationalAnnotationNames.DiscriminatorValue);

        /// <summary>
        ///     Removes the discriminator value for this entity type.
        /// </summary>
        public static void RemoveDiscriminatorValue([NotNull] this IConventionEntityType entityType)
            => entityType.RemoveAnnotation(RelationalAnnotationNames.DiscriminatorValue);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the discriminator value.
        /// </summary>
        /// <returns> The <see cref="ConfigurationSource" /> or <c>null</c> if no discriminator value has been set. </returns>
        public static ConfigurationSource? GetDiscriminatorValueConfigurationSource([NotNull] this IConventionEntityType entityType)
            => entityType.FindAnnotation(RelationalAnnotationNames.DiscriminatorValue)
                ?.GetConfigurationSource();
    }
}
