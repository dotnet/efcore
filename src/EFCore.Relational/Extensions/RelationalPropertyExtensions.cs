// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="IProperty" /> for relational database metadata.
    /// </summary>
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     Returns the name of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name of the column to which the property is mapped. </returns>
        public static string GetColumnName([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ColumnName);
            return annotation != null ? (string)annotation.Value : property.GetDefaultColumnName();
        }

        /// <summary>
        ///     Returns the name of the column to which the property is mapped for a particular table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The name of the column to which the property is mapped. </returns>
        public static string GetColumnName(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(tableName, nameof(tableName));

            var overrides = RelationalPropertyOverrides.Find(property, tableName, schema);
            if (overrides?.GetColumnNameConfigurationSource() != null)
            {
                return overrides.ColumnName;
            }

            var annotation = property.FindAnnotation(RelationalAnnotationNames.ColumnName);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            return GetDefaultColumnName(property, tableName, schema);
        }

        /// <summary>
        ///     Returns the default column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The default column name to which the property would be mapped. </returns>
        public static string GetDefaultColumnName([NotNull] this IProperty property)
            => Uniquifier.Truncate(property.Name, property.DeclaringEntityType.Model.GetMaxIdentifierLength());

        /// <summary>
        ///     Returns the default column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The default column name to which the property would be mapped. </returns>
        public static string GetDefaultColumnName(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var sharedTablePrincipalPrimaryKeyProperty = FindSharedObjectRootPrimaryKeyProperty(
                property, tableName, schema, StoreObjectType.Table);
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.GetColumnName(tableName, schema);
            }

            var sharedTablePrincipalConcurrencyProperty = FindSharedObjectRootConcurrencyTokenProperty(
                property, tableName, schema, StoreObjectType.Table);
            if (sharedTablePrincipalConcurrencyProperty != null)
            {
                return sharedTablePrincipalConcurrencyProperty.GetColumnName(tableName, schema);
            }

            var entityType = property.DeclaringEntityType;
            StringBuilder builder = null;
            do
            {
                var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                if (ownership == null)
                {
                    entityType = null;
                }
                else
                {
                    var ownerType = ownership.PrincipalEntityType;
                    var table = tableName;
                    if (tableName == ownerType.GetTableName()
                        && schema == ownerType.GetSchema())
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder();
                        }

                        builder.Insert(0, "_");
                        builder.Insert(0, ownership.PrincipalToDependent.Name);
                        entityType = ownerType;
                    }
                    else
                    {
                        entityType = null;
                    }
                }
            }
            while (entityType != null);

            var baseName = property.GetDefaultColumnName();
            if (builder == null)
            {
                return baseName;
            }

            builder.Append(baseName);
            baseName = builder.ToString();

            return Uniquifier.Truncate(baseName, property.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetColumnName([NotNull] this IMutableProperty property, [CanBeNull] string name)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetColumnName(
            [NotNull] this IConventionProperty property, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Sets the column to which the property is mapped for a particular table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        public static void SetColumnName(
            [NotNull] this IMutableProperty property, [CanBeNull] string name, [NotNull] string tableName, [CanBeNull] string schema)
            => RelationalPropertyOverrides.GetOrCreate(property, tableName, schema)
                .SetColumnName(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the column to which the property is mapped for a particular table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetColumnName(
            [NotNull] this IConventionProperty property,
            [CanBeNull] string name,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
            => RelationalPropertyOverrides.GetOrCreate(
                Check.NotNull(property, nameof(property)), Check.NotNull(tableName, nameof(tableName)), schema)
                .SetColumnName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name. </returns>
        public static ConfigurationSource? GetColumnNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ColumnName)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name for a particular table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name for a particular table. </returns>
        public static ConfigurationSource? GetColumnNameConfigurationSource(
            [NotNull] this IConventionProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => RelationalPropertyOverrides.Find(property, tableName, schema)?.GetColumnNameConfigurationSource();

        /// <summary>
        ///     Returns the name of the column to which the property is mapped for a particular view.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name of the view column to which the property is mapped. </returns>
        public static string GetViewColumnName([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ViewColumnName);
            return annotation != null ? (string)annotation.Value : GetDefaultViewColumnName(property);
        }

        /// <summary>
        ///     Returns the name of the column to which the property is mapped for a particular view.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The name of the view column to which the property is mapped. </returns>
        public static string GetViewColumnName(
            [NotNull] this IProperty property,
            [NotNull] string viewName,
            [CanBeNull] string schema)
        {
            var overrides = RelationalPropertyOverrides.Find(property, viewName, schema);
            if (overrides?.GetViewColumnNameConfigurationSource() != null)
            {
                return overrides.ViewColumnName;
            }

            var annotation = property.FindAnnotation(RelationalAnnotationNames.ViewColumnName);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            return GetDefaultViewColumnName(property, viewName, schema);
        }

        /// <summary>
        ///     Returns the default view column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The default view column name to which the property would be mapped. </returns>
        public static string GetDefaultViewColumnName([NotNull] this IProperty property)
            => property.GetDefaultColumnName();

        /// <summary>
        ///     Returns the default view column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The default view column name to which the property would be mapped. </returns>
        public static string GetDefaultViewColumnName(
            [NotNull] this IProperty property,
            [NotNull] string viewName,
            [CanBeNull] string schema)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(viewName, nameof(viewName));

            var sharedTablePrincipalPrimaryKeyProperty = FindSharedObjectRootPrimaryKeyProperty(
                property, viewName, schema, StoreObjectType.View);
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.GetViewColumnName(viewName, schema);
            }

            var sharedTablePrincipalConcurrencyProperty = FindSharedObjectRootConcurrencyTokenProperty(
                property, viewName, schema, StoreObjectType.View);
            if (sharedTablePrincipalConcurrencyProperty != null)
            {
                return sharedTablePrincipalConcurrencyProperty.GetViewColumnName(viewName, schema);
            }

            var entityType = property.DeclaringEntityType;
            StringBuilder builder = null;
            do
            {
                var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                if (ownership == null)
                {
                    entityType = null;
                }
                else
                {
                    var ownerType = ownership.PrincipalEntityType;
                    if (viewName == ownerType.GetViewName()
                        && schema == ownerType.GetViewSchema())
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder();
                        }

                        builder.Insert(0, "_");
                        builder.Insert(0, ownership.PrincipalToDependent.Name);
                        entityType = ownerType;
                    }
                    else
                    {
                        entityType = null;
                    }
                }
            }
            while (entityType != null);

            if (builder == null)
            {
                return property.GetColumnName(viewName, schema);
            }

            builder.Append(property.Name);
            return Uniquifier.Truncate(builder.ToString(), property.DeclaringEntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Sets the view column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        public static void SetViewColumnName([NotNull] this IMutableProperty property, [CanBeNull] string name)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ViewColumnName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the view column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetViewColumnName(
            [NotNull] this IConventionProperty property, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ViewColumnName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Sets the column to which the property is mapped for a particular view.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        public static void SetViewColumnName(
            [NotNull] this IMutableProperty property, [CanBeNull] string name, [NotNull] string viewName, [CanBeNull] string schema)
            => RelationalPropertyOverrides.GetOrCreate(
                Check.NotNull(property, nameof(property)), Check.NotNull(viewName, nameof(viewName)), schema)
                .SetViewColumnName(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the column to which the property is mapped for a particular view.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetViewColumnName(
            [NotNull] this IConventionProperty property,
            [CanBeNull] string name,
            [NotNull] string viewName,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
            => RelationalPropertyOverrides.GetOrCreate(property, viewName, schema)
                .SetViewColumnName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the view column name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the view column name. </returns>
        public static ConfigurationSource? GetViewColumnNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ViewColumnName)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name for a particular view.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name for a particular view. </returns>
        public static ConfigurationSource? GetViewColumnNameConfigurationSource(
            [NotNull] this IConventionProperty property,
            [NotNull] string viewName,
            [CanBeNull] string schema)
            => RelationalPropertyOverrides.Find(property, viewName, schema)?.GetViewColumnNameConfigurationSource();

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The database type of the column to which the property is mapped. </returns>
        public static string GetColumnType([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ColumnType);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            return property.FindRelationalTypeMapping()?.StoreType;
        }

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The database type of the column to which the property is mapped. </returns>
        public static string GetColumnType(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ColumnType);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            return GetDefaultColumnType(property, tableName, schema);
        }

        private static string GetDefaultColumnType(IProperty property, string tableName, string schema)
        {
            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.GetColumnType(tableName, schema)
                : property.FindRelationalTypeMapping(tableName, schema)?.StoreType;
        }

        /// <summary>
        ///     Sets the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetColumnType([NotNull] this IMutableProperty property, [CanBeNull] string value)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetColumnType(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name. </returns>
        public static ConfigurationSource? GetColumnTypeConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ColumnType)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the table columns to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The table columns to which the property is mapped. </returns>
        public static IEnumerable<IColumnMapping> GetTableColumnMappings([NotNull] this IProperty property) =>
            (IEnumerable<IColumnMapping>)property[RelationalAnnotationNames.TableColumnMappings]
                ?? Enumerable.Empty<IColumnMapping>();

        /// <summary>
        ///     Returns the view columns to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The view columns to which the property is mapped. </returns>
        public static IEnumerable<IViewColumnMapping> GetViewColumnMappings([NotNull] this IProperty property) =>
            (IEnumerable<IViewColumnMapping>)property[RelationalAnnotationNames.ViewColumnMappings]
                ?? Enumerable.Empty<IViewColumnMapping>();

        /// <summary>
        ///     Returns the table column corresponding to this property if it's mapped to the given table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The target table name. </param>
        /// <param name="schema"> The target table schema. </param>
        /// <returns> The table column to which the property is mapped. </returns>
        public static IColumn FindTableColumn([NotNull] this IProperty property, [NotNull] string tableName, [CanBeNull] string schema)
            => property.GetTableColumnMappings().Select(m => m.Column)
                .FirstOrDefault(c => c.Table.Name == tableName && c.Table.Schema == schema);

        /// <summary>
        ///     Returns the view column corresponding to this property if it's mapped to the given table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="viewName"> The target table name. </param>
        /// <param name="schema"> The target table schema. </param>
        /// <returns> The table column to which the property is mapped. </returns>
        public static IViewColumn FindViewColumn([NotNull] this IProperty property, [NotNull] string viewName, [CanBeNull] string schema)
            => property.GetViewColumnMappings().Select(m => m.Column)
                .FirstOrDefault(c => c.View.Name == viewName && c.View.Schema == schema);

        /// <summary>
        ///     Returns the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SQL expression that is used as the default value for the column this property is mapped to. </returns>
        public static string GetDefaultValueSql([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql);
            return annotation != null ? (string)annotation.Value : null;
        }

        /// <summary>
        ///     Returns the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The SQL expression that is used as the default value for the column this property is mapped to. </returns>
        public static string GetDefaultValueSql(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return GetDefaultValueSql(sharedTableRootProperty, tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Sets the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDefaultValueSql([NotNull] this IMutableProperty property, [CanBeNull] string value)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetDefaultValueSql(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the default value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default value SQL expression. </returns>
        public static ConfigurationSource? GetDefaultValueSqlConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SQL expression that is used as the computed value for the column this property is mapped to. </returns>
        public static string GetComputedColumnSql([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ComputedColumnSql);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            return null;
        }

        /// <summary>
        ///     Returns the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The SQL expression that is used as the computed value for the column this property is mapped to. </returns>
        public static string GetComputedColumnSql(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ComputedColumnSql);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return GetComputedColumnSql(sharedTableRootProperty, tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Sets the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetComputedColumnSql([NotNull] this IMutableProperty property, [CanBeNull] string value)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     Sets the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetComputedColumnSql(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the computed value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the computed value SQL expression. </returns>
        public static ConfigurationSource? GetComputedColumnSqlConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ComputedColumnSql)?.GetConfigurationSource();

        /// <summary>
        ///     Gets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns>
        ///     Whether the value of the computed column this property is mapped to is stored in the database,
        ///     or calculated when it is read.
        /// </returns>
        public static bool? GetIsStored([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.IsStored);
            return annotation != null ? (bool?)annotation.Value : null;
        }

        /// <summary>
        ///     Gets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns>
        ///     Whether the value of the computed column this property is mapped to is stored in the database,
        ///     or calculated when it is read.
        /// </returns>
        public static bool? GetIsStored(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.IsStored);
            if (annotation != null)
            {
                return (bool?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return GetIsStored(sharedTableRootProperty, tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Sets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetIsStored([NotNull] this IMutableProperty property, bool? value)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.IsStored,
                value);

        /// <summary>
        ///     Sets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsStored(
            [NotNull] this IConventionProperty property, bool? value, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsStored, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the computed value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the computed value SQL expression. </returns>
        public static ConfigurationSource? GetIsStoredConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.IsStored)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The object that is used as the default value for the column this property is mapped to. </returns>
        public static object GetDefaultValue([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValue);
            return annotation != null ? annotation.Value : null;
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The object that is used as the default value for the column this property is mapped to. </returns>
        public static object GetDefaultValue(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValue);
            if (annotation != null)
            {
                return annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return GetDefaultValue(sharedTableRootProperty, tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Sets the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDefaultValue([NotNull] this IMutableProperty property, [CanBeNull] object value)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.DefaultValue, ConvertDefaultValue(property, value));

        /// <summary>
        ///     Sets the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static object SetDefaultValue(
            [NotNull] this IConventionProperty property, [CanBeNull] object value, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValue, ConvertDefaultValue(property, value), fromDataAnnotation);

            return value;
        }

        private static object ConvertDefaultValue([NotNull] IProperty property, [CanBeNull] object value)
        {
            if (value == null
                || value == DBNull.Value)
            {
                return value;
            }

            var valueType = value.GetType();
            if (property.ClrType.UnwrapNullableType() != valueType)
            {
                try
                {
                    return Convert.ChangeType(value, property.ClrType, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncorrectDefaultValueType(
                            value, valueType, property.Name, property.ClrType, property.DeclaringEntityType.DisplayName()));
                }
            }

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the default value.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default value. </returns>
        public static ConfigurationSource? GetDefaultValueConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.DefaultValue)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> A flag indicating if the property as capable of storing only fixed-length data, such as strings. </returns>
        public static bool? IsFixedLength([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.IsFixedLength);
            return annotation != null ? (bool?)annotation.Value : null;
        }

        /// <summary>
        ///     Returns a flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> A flag indicating if the property as capable of storing only fixed-length data, such as strings. </returns>
        public static bool? IsFixedLength(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.IsFixedLength);
            if (annotation != null)
            {
                return (bool?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return IsFixedLength(sharedTableRootProperty, tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Sets a flag indicating whether the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        public static void SetIsFixedLength([NotNull] this IMutableProperty property, bool? fixedLength)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength);

        /// <summary>
        ///     Sets a flag indicating whether the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsFixedLength([NotNull] this IConventionProperty property, bool? fixedLength, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength, fromDataAnnotation);

            return fixedLength;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IProperty)" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IProperty)" />. </returns>
        public static ConfigurationSource? GetIsFixedLengthConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.IsFixedLength)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Checks whether the column mapped to the given <see cref="IProperty" /> will be nullable
        ///         when created in the database.
        ///     </para>
        ///     <para>
        ///         This depends on the property itself and also how it is mapped. For example,
        ///         derived non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///         As well as properties on optional types sharing the same table.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <returns> <see langword="true" /> if the mapped column is nullable; <see langword="false" /> otherwise. </returns>
        public static bool IsColumnNullable([NotNull] this IProperty property)
            => !property.IsPrimaryKey()
                && (property.IsNullable
                    || (property.DeclaringEntityType.BaseType != null && property.DeclaringEntityType.GetDiscriminatorProperty() != null)
                    || property.DeclaringEntityType.FindTableRowInternalForeignKeys(
                        property.DeclaringEntityType.GetTableName(), property.DeclaringEntityType.GetSchema()).Any());

        /// <summary>
        ///     <para>
        ///         Checks whether the column mapped to the given <see cref="IProperty" /> will be nullable
        ///         when created in the database.
        ///     </para>
        ///     <para>
        ///         This depends on the property itself and also how it is mapped. For example,
        ///         derived non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///         As well as properties on optional types sharing the same table.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> <see langword="true" /> if the mapped column is nullable; <see langword="false" /> otherwise. </returns>
        public static bool IsColumnNullable(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            if (property.IsPrimaryKey())
            {
                return false;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.IsColumnNullable(tableName, schema);
            }

            return property.IsNullable
                    || (property.DeclaringEntityType.BaseType != null && property.DeclaringEntityType.GetDiscriminatorProperty() != null)
                    || property.DeclaringEntityType.FindTableRowInternalForeignKeys(tableName, schema).Any();
        }

        /// <summary>
        ///     <para>
        ///         Checks whether or not the column mapped to the given <see cref="IProperty" /> will be nullable
        ///         when created in the database.
        ///     </para>
        ///     <para>
        ///         This depends on the property itself and also how it is mapped. For example,
        ///         derived non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///         As well as properties on optional types sharing the same table.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <returns> <see langword="true" /> if the mapped column is nullable; <see langword="false" /> otherwise. </returns>
        public static bool IsViewColumnNullable([NotNull] this IProperty property)
            => !property.IsPrimaryKey()
                && (property.IsNullable
                    || (property.DeclaringEntityType.BaseType != null && property.DeclaringEntityType.GetDiscriminatorProperty() != null)
                    || property.DeclaringEntityType.FindViewRowInternalForeignKeys(
                        property.DeclaringEntityType.GetViewName(), property.DeclaringEntityType.GetViewSchema()).Any());

        /// <summary>
        ///     <para>
        ///         Checks whether or not the column mapped to the given <see cref="IProperty" /> will be nullable
        ///         when created in the database.
        ///     </para>
        ///     <para>
        ///         This depends on the property itself and also how it is mapped. For example,
        ///         derived non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///         As well as properties on optional types sharing the same table.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> <see langword="true" /> if the mapped column is nullable; <see langword="false" /> otherwise. </returns>
        public static bool IsViewColumnNullable(
            [NotNull] this IProperty property,
            [NotNull] string viewName,
            [CanBeNull] string schema)
        {
            if (property.IsPrimaryKey())
            {
                return false;
            }

            var sharedTableRootProperty = property.FindSharedViewRootProperty(viewName, schema);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.IsViewColumnNullable(viewName, schema);
            }

            return property.IsNullable
                    || (property.DeclaringEntityType.BaseType != null && property.DeclaringEntityType.GetDiscriminatorProperty() != null)
                    || property.DeclaringEntityType.FindViewRowInternalForeignKeys(viewName, schema).Any();
        }

        /// <summary>
        ///     Returns the comment for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comment for the column this property is mapped to. </returns>
        public static string GetComment([NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.Comment);
            return annotation != null ? (string)annotation.Value : null;
        }

        /// <summary>
        ///     Returns the comment for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The comment for the column this property is mapped to. </returns>
        public static string GetComment(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.Comment);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return GetComment(sharedTableRootProperty, tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Configures a comment to be applied to the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comment"> The comment for the column. </param>
        public static void SetComment([NotNull] this IMutableProperty property, [CanBeNull] string comment)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment);

        /// <summary>
        ///     Configures a comment to be applied to the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetComment(
            [NotNull] this IConventionProperty property, [CanBeNull] string comment, bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment, fromDataAnnotation);

            return comment;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column comment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column comment. </returns>
        public static ConfigurationSource? GetCommentConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.Comment)
                ?.GetConfigurationSource();

        /// <summary>
        ///     Returns the collation to be used for the column.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The collation for the column this property is mapped to. </returns>
        public static string GetCollation(
            [NotNull] this IProperty property)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.Collation);
            return annotation != null ? (string)annotation.Value : null;
        }

        /// <summary>
        ///     Returns the collation to be used for the column.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The collation for the column this property is mapped to. </returns>
        public static string GetCollation(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.Collation);
            if (annotation != null)
            {
                return (string)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedTableRootProperty(tableName, schema);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.GetCollation(tableName, schema);
            }

            return null;
        }

        /// <summary>
        ///     Configures a collation to be used for column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="collation"> The collation for the column. </param>
        public static void SetCollation([NotNull] this IMutableProperty property, [CanBeNull] string collation)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collation);

        /// <summary>
        ///     Configures a collation to be used for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="collation"> The collation for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string SetCollation(
            [NotNull] this IConventionProperty property,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collation, fromDataAnnotation);
            return collation;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column collation.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column collation. </returns>
        public static ConfigurationSource? GetCollationConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.Collation)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping. </returns>
        [DebuggerStepThrough]
        public static RelationalTypeMapping GetRelationalTypeMapping([NotNull] this IProperty property)
            => (RelationalTypeMapping)property.GetTypeMapping();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use FindRelationalTypeMapping")]
        public static RelationalTypeMapping FindRelationalMapping([NotNull] this IProperty property)
            => property.FindRelationalTypeMapping();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        [DebuggerStepThrough]
        public static RelationalTypeMapping FindRelationalTypeMapping([NotNull] this IProperty property)
            => (RelationalTypeMapping)property.FindTypeMapping();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public static RelationalTypeMapping FindRelationalTypeMapping(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => property.FindRelationalTypeMapping();

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IProperty FindSharedTableRootProperty(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => FindSharedObjectRootProperty(property, tableName, schema, StoreObjectType.Table);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IMutableProperty FindSharedTableRootProperty(
            [NotNull] this IMutableProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => (IMutableProperty)FindSharedObjectRootProperty(property, tableName, schema, StoreObjectType.Table);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IConventionProperty FindSharedTableRootProperty(
            [NotNull] this IConventionProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => (IConventionProperty)FindSharedObjectRootProperty(property, tableName, schema, StoreObjectType.Table);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IProperty FindSharedViewRootProperty(
            [NotNull] this IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => FindSharedObjectRootProperty(property, tableName, schema, StoreObjectType.View);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IMutableProperty FindSharedViewRootProperty(
            [NotNull] this IMutableProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => (IMutableProperty)FindSharedObjectRootProperty(property, tableName, schema, StoreObjectType.View);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IConventionProperty FindSharedViewRootProperty(
            [NotNull] this IConventionProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema)
            => (IConventionProperty)FindSharedObjectRootProperty(property, tableName, schema, StoreObjectType.View);

        private static IProperty FindSharedObjectRootProperty(
            [NotNull] IProperty property,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            StoreObjectType storeObjectType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(tableName, nameof(tableName));

            var column = storeObjectType == StoreObjectType.Table
                ? property.GetColumnName(tableName, schema)
                : property.GetViewColumnName(tableName, schema);

            if (column == null)
            {
                throw new InvalidOperationException(RelationalStrings.PropertyNotMappedToTable(
                    property.Name, property.DeclaringEntityType, schema == null ? tableName : schema + "." + tableName));
            }

            var rootProperty = property;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                var allProperties = rootProperty.DeclaringEntityType
                    .FindRowInternalForeignKeys(tableName, schema, storeObjectType)
                    .SelectMany(fk => fk.PrincipalEntityType.GetProperties());
                var linkedProperty = storeObjectType == StoreObjectType.Table
                        ? allProperties.FirstOrDefault(p => p.GetColumnName(tableName, schema) == column)
                        : allProperties.FirstOrDefault(p => p.GetViewColumnName(tableName, schema) == column);
                if (linkedProperty == null)
                {
                    break;
                }

                rootProperty = linkedProperty;
            }

            return rootProperty == property ? null : rootProperty;
        }

        private static IProperty FindSharedObjectRootPrimaryKeyProperty(
            [NotNull] IProperty property,
            [NotNull] string name,
            [CanBeNull] string schema,
            StoreObjectType storeObjectType)
        {
            if (!property.IsPrimaryKey())
            {
                return null;
            }

            var principalProperty = property;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                var linkingRelationship = principalProperty.DeclaringEntityType
                    .FindRowInternalForeignKeys(name, schema, storeObjectType).FirstOrDefault();
                if (linkingRelationship == null)
                {
                    break;
                }

                principalProperty = linkingRelationship.PrincipalKey.Properties[linkingRelationship.Properties.IndexOf(principalProperty)];
            }

            return principalProperty == property ? null : principalProperty;
        }

        private static IProperty FindSharedObjectRootConcurrencyTokenProperty(
            [NotNull] IProperty property,
            [NotNull] string name,
            [CanBeNull] string schema,
            StoreObjectType storeObjectType)
        {
            if (!property.IsConcurrencyToken)
            {
                return null;
            }

            var principalProperty = property;
            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                var linkingRelationship = principalProperty.DeclaringEntityType
                    .FindRowInternalForeignKeys(name, schema, storeObjectType).FirstOrDefault();
                if (linkingRelationship == null)
                {
                    break;
                }

                principalProperty = linkingRelationship.PrincipalEntityType.FindProperty(property.Name);
                if (principalProperty == null
                    || !principalProperty.IsConcurrencyToken)
                {
                    return null;
                }
            }

            return principalProperty == property ? null : principalProperty;
        }

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IAnnotatable FindOverrides(
            [NotNull] this IProperty property, [NotNull] string tableName, [CanBeNull] string schema)
            => RelationalPropertyOverrides.Find(property, tableName, schema);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IMutableAnnotatable GetOrCreateOverrides(
            [NotNull] this IMutableProperty property, [NotNull] string tableName, [CanBeNull] string schema)
            => RelationalPropertyOverrides.GetOrCreate(property, tableName, schema);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The schema. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IConventionAnnotatable GetOrCreateOverrides(
            [NotNull] this IConventionProperty property, [NotNull] string tableName, [CanBeNull] string schema)
            => RelationalPropertyOverrides.GetOrCreate(property, tableName, schema);
    }
}
