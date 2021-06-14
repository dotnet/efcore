// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
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
    ///     Property extension methods for relational database metadata.
    /// </summary>
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     Returns the name of the table column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name of the table column to which the property is mapped. </returns>
        [Obsolete("Use the overload that takes a StoreObjectIdentifier")]
        public static string GetColumnName(this IProperty property)
            => (string?)property.FindAnnotation(RelationalAnnotationNames.ColumnName)?.Value ?? property.GetDefaultColumnName();

        /// <summary>
        ///     Returns the base name of the column to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The base name of the column to which the property would be mapped. </returns>
        public static string GetColumnBaseName(this IReadOnlyProperty property)
            => (string?)property.FindAnnotation(RelationalAnnotationNames.ColumnName)?.Value ?? property.GetDefaultColumnBaseName();

        /// <summary>
        ///     Returns the name of the column to which the property is mapped for a particular table.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The name of the column to which the property is mapped. </returns>
        public static string? GetColumnName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(property, nameof(property));

            var overrides = RelationalPropertyOverrides.Find(property, storeObject);
            if (overrides?.ColumnNameOverriden == true)
            {
                return overrides.ColumnName;
            }

            if (storeObject.StoreObjectType != StoreObjectType.Function
                && storeObject.StoreObjectType != StoreObjectType.SqlQuery)
            {
                if (property.IsPrimaryKey())
                {
                    var tableFound = false;
                    foreach (var containingType in property.DeclaringEntityType.GetDerivedTypesInclusive())
                    {
                        if (StoreObjectIdentifier.Create(containingType, storeObject.StoreObjectType) == storeObject)
                        {
                            tableFound = true;
                            break;
                        }
                    }

                    if (!tableFound)
                    {
                        return null;
                    }
                }
                else if (StoreObjectIdentifier.Create(property.DeclaringEntityType, storeObject.StoreObjectType) != storeObject)
                {
                    return null;
                }
            }

            var columnAnnotation = property.FindAnnotation(RelationalAnnotationNames.ColumnName);
            if (columnAnnotation != null)
            {
                return (string?)columnAnnotation.Value;
            }

            return GetDefaultColumnName(property, storeObject);
        }

        /// <summary>
        ///     Returns the default table column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The default table column name to which the property would be mapped. </returns>
        [Obsolete("Use the overload that takes a StoreObjectIdentifier")]
        public static string GetDefaultColumnName(this IProperty property)
        {
            var table = StoreObjectIdentifier.Create(property.DeclaringEntityType, StoreObjectType.Table);
            return table == null ? property.GetDefaultColumnBaseName() : property.GetDefaultColumnName(table.Value);
        }

        /// <summary>
        ///     Returns the default base name of the column to which the property would be mapped
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The default base column name to which the property would be mapped. </returns>
        public static string GetDefaultColumnBaseName(this IReadOnlyProperty property)
            => Uniquifier.Truncate(property.Name, property.DeclaringEntityType.Model.GetMaxIdentifierLength());

        /// <summary>
        ///     Returns the default column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The default column name to which the property would be mapped. </returns>
        public static string GetDefaultColumnName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var sharedTablePrincipalPrimaryKeyProperty = FindSharedObjectRootPrimaryKeyProperty(property, storeObject);
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.GetColumnName(storeObject)!;
            }

            var sharedTablePrincipalConcurrencyProperty = FindSharedObjectRootConcurrencyTokenProperty(property, storeObject);
            if (sharedTablePrincipalConcurrencyProperty != null)
            {
                return sharedTablePrincipalConcurrencyProperty.GetColumnName(storeObject)!;
            }

            var entityType = property.DeclaringEntityType;
            StringBuilder? builder = null;
            while (true)
            {
                var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                if (ownership == null)
                {
                    break;
                }

                var name = storeObject.Name;
                var schema = storeObject.Schema;
                var ownerType = ownership.PrincipalEntityType;
                switch (storeObject.StoreObjectType)
                {
                    case StoreObjectType.Table:
                        if (name != ownerType.GetTableName()
                            || schema != ownerType.GetSchema())
                        {
                            entityType = null;
                        }

                        break;
                    case StoreObjectType.View:
                        if (name != ownerType.GetViewName()
                            || schema != ownerType.GetViewSchema())
                        {
                            entityType = null;
                        }

                        break;
                    case StoreObjectType.Function:
                        if (name != ownerType.GetFunctionName())
                        {
                            entityType = null;
                        }

                        break;
                    default:
                        throw new NotSupportedException(storeObject.StoreObjectType.ToString());
                }

                if (entityType == null)
                {
                    break;
                }

                if (builder == null)
                {
                    builder = new StringBuilder();
                }

                builder.Insert(0, "_");
                builder.Insert(0, ownership.PrincipalToDependent!.Name);
                entityType = ownerType;
            }

            var baseName = property.GetDefaultColumnBaseName();
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
        public static void SetColumnName(this IMutableProperty property, string? name)
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
        public static string? SetColumnName(
            this IConventionProperty property,
            string? name,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Sets the column to which the property is mapped for a particular table-like store object.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        public static void SetColumnName(
            this IMutableProperty property,
            string? name,
            in StoreObjectIdentifier storeObject)
            => RelationalPropertyOverrides.GetOrCreate(property, storeObject)
                .SetColumnName(name, ConfigurationSource.Explicit);

        /// <summary>
        ///     Sets the column to which the property is mapped for a particular table-like store object.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="name"> The name to set. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetColumnName(
            this IConventionProperty property,
            string? name,
            in StoreObjectIdentifier storeObject,
            bool fromDataAnnotation = false)
            => RelationalPropertyOverrides.GetOrCreate(property, storeObject)
                .SetColumnName(name, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name. </returns>
        public static ConfigurationSource? GetColumnNameConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ColumnName)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name for a particular table-like store object.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name for a particular table-like store object. </returns>
        public static ConfigurationSource? GetColumnNameConfigurationSource(
            this IConventionProperty property,
            in StoreObjectIdentifier storeObject)
            => ((RelationalPropertyOverrides?)RelationalPropertyOverrides.Find(property, storeObject))?.GetColumnNameConfigurationSource();

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped, or <see langword="null" /> if the database type
        ///     could not be found.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns>
        ///     The database type of the column to which the property is mapped, or <see langword="null" /> if the database type could not
        ///     be found.
        /// </returns>
        public static string? GetColumnType(this IReadOnlyProperty property)
        {
            Check.NotNull(property, nameof(property));

            return (string?)(property.FindRelationalTypeMapping()?.StoreType
                ?? property.FindAnnotation(RelationalAnnotationNames.ColumnType)?.Value);
        }

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The database type of the column to which the property is mapped. </returns>
        public static string GetColumnType(this IProperty property)
            => ((IReadOnlyProperty)property).GetColumnType()!;

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped, or <see langword="null" /> if the database type
        ///     could not be found.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns>
        ///     The database type of the column to which the property is mapped, or <see langword="null" /> if the database type could not
        ///     be found.
        /// </returns>
        public static string? GetColumnType(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ColumnType);
            if (annotation != null)
            {
                return property.FindRelationalTypeMapping()?.StoreType ?? (string?)annotation.Value;
            }

            return GetDefaultColumnType(property, storeObject);
        }

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The database type of the column to which the property is mapped. </returns>
        public static string GetColumnType(this IProperty property, in StoreObjectIdentifier storeObject)
            => ((IReadOnlyProperty)property).GetColumnType(storeObject)!;

        private static string? GetDefaultColumnType(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.GetColumnType(storeObject)
                : property.FindRelationalTypeMapping(storeObject)?.StoreType;
        }

        /// <summary>
        ///     Sets the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetColumnType(this IMutableProperty property, string? value)
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
        public static string? SetColumnType(
            this IConventionProperty property,
            string? value,
            bool fromDataAnnotation = false)
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
        public static ConfigurationSource? GetColumnTypeConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ColumnType)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the default columns to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The default columns to which the property would be mapped. </returns>
        public static IEnumerable<IColumnMappingBase> GetDefaultColumnMappings(this IProperty property)
            => (IEnumerable<IColumnMappingBase>?)property.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.DefaultColumnMappings)
                ?? Enumerable.Empty<IColumnMappingBase>();

        /// <summary>
        ///     Returns the table columns to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The table columns to which the property is mapped. </returns>
        public static IEnumerable<IColumnMapping> GetTableColumnMappings(this IProperty property)
            => (IEnumerable<IColumnMapping>?)property.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.TableColumnMappings)
                ?? Enumerable.Empty<IColumnMapping>();

        /// <summary>
        ///     Returns the view columns to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The view columns to which the property is mapped. </returns>
        public static IEnumerable<IViewColumnMapping> GetViewColumnMappings(this IProperty property)
            => (IEnumerable<IViewColumnMapping>?)property.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.ViewColumnMappings)
                ?? Enumerable.Empty<IViewColumnMapping>();

        /// <summary>
        ///     Returns the SQL query columns to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SQL query columns to which the property is mapped. </returns>
        public static IEnumerable<ISqlQueryColumnMapping> GetSqlQueryColumnMappings(this IProperty property)
            => (IEnumerable<ISqlQueryColumnMapping>?)property.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.SqlQueryColumnMappings)
                ?? Enumerable.Empty<ISqlQueryColumnMapping>();

        /// <summary>
        ///     Returns the function columns to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The function columns to which the property is mapped. </returns>
        public static IEnumerable<IFunctionColumnMapping> GetFunctionColumnMappings(this IProperty property)
            => (IEnumerable<IFunctionColumnMapping>?)property.FindRuntimeAnnotationValue(
                    RelationalAnnotationNames.FunctionColumnMappings)
                ?? Enumerable.Empty<IFunctionColumnMapping>();

        /// <summary>
        ///     Returns the column corresponding to this property if it's mapped to the given table-like store object.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The column to which the property is mapped. </returns>
        public static IColumnBase? FindColumn(this IProperty property, in StoreObjectIdentifier storeObject)
        {
            switch (storeObject.StoreObjectType)
            {
                case StoreObjectType.Table:
                    foreach (var mapping in property.GetTableColumnMappings())
                    {
                        if (mapping.TableMapping.Table.Name == storeObject.Name && mapping.TableMapping.Table.Schema == storeObject.Schema)
                        {
                            return mapping.Column;
                        }
                    }

                    return null;
                case StoreObjectType.View:
                    foreach (var mapping in property.GetViewColumnMappings())
                    {
                        if (mapping.TableMapping.Table.Name == storeObject.Name && mapping.TableMapping.Table.Schema == storeObject.Schema)
                        {
                            return mapping.Column;
                        }
                    }

                    return null;
                case StoreObjectType.SqlQuery:
                    foreach (var mapping in property.GetSqlQueryColumnMappings())
                    {
                        if (mapping.TableMapping.Table.Name == storeObject.Name)
                        {
                            return mapping.Column;
                        }
                    }

                    return null;
                case StoreObjectType.Function:
                    foreach (var mapping in property.GetFunctionColumnMappings())
                    {
                        if (mapping.TableMapping.Table.Name == storeObject.Name)
                        {
                            return mapping.Column;
                        }
                    }

                    return null;
                default:
                    throw new NotSupportedException(storeObject.StoreObjectType.ToString());
            }
        }

        /// <summary>
        ///     Returns the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SQL expression that is used as the default value for the column this property is mapped to. </returns>
        public static string? GetDefaultValueSql(this IReadOnlyProperty property)
            => (string?)property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql)?.Value;

        /// <summary>
        ///     Returns the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The SQL expression that is used as the default value for the column this property is mapped to. </returns>
        public static string? GetDefaultValueSql(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return GetDefaultValueSql(sharedTableRootProperty, storeObject);
            }

            return null;
        }

        /// <summary>
        ///     Sets the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDefaultValueSql(this IMutableProperty property, string? value)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
                value);

        /// <summary>
        ///     Sets the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetDefaultValueSql(
            this IConventionProperty property,
            string? value,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
                value,
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the default value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default value SQL expression. </returns>
        public static ConfigurationSource? GetDefaultValueSqlConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.DefaultValueSql)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SQL expression that is used as the computed value for the column this property is mapped to. </returns>
        public static string? GetComputedColumnSql(this IReadOnlyProperty property)
            => (string?)property.FindAnnotation(RelationalAnnotationNames.ComputedColumnSql)?.Value;

        /// <summary>
        ///     Returns the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The SQL expression that is used as the computed value for the column this property is mapped to. </returns>
        public static string? GetComputedColumnSql(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.ComputedColumnSql);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return GetComputedColumnSql(sharedTableRootProperty, storeObject);
            }

            return null;
        }

        /// <summary>
        ///     Sets the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetComputedColumnSql(this IMutableProperty property, string? value)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
                value);

        /// <summary>
        ///     Sets the SQL expression that is used as the computed value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetComputedColumnSql(
            this IConventionProperty property,
            string? value,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
                value,
                fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the computed value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the computed value SQL expression. </returns>
        public static ConfigurationSource? GetComputedColumnSqlConfigurationSource(this IConventionProperty property)
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
        public static bool? GetIsStored(this IReadOnlyProperty property)
            => (bool?)property.FindAnnotation(RelationalAnnotationNames.IsStored)?.Value;

        /// <summary>
        ///     Gets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns>
        ///     Whether the value of the computed column this property is mapped to is stored in the database,
        ///     or calculated when it is read.
        /// </returns>
        public static bool? GetIsStored(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.IsStored);
            if (annotation != null)
            {
                return (bool?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return GetIsStored(sharedTableRootProperty, storeObject);
            }

            return null;
        }

        /// <summary>
        ///     Sets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetIsStored(this IMutableProperty property, bool? value)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsStored, value);

        /// <summary>
        ///     Sets whether the value of the computed column this property is mapped to is stored in the database, or calculated when
        ///     it is read.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsStored(
            this IConventionProperty property,
            bool? value,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsStored, value, fromDataAnnotation);

            return value;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the computed value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the computed value SQL expression. </returns>
        public static ConfigurationSource? GetIsStoredConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.IsStored)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The object that is used as the default value for the column this property is mapped to. </returns>
        public static object? GetDefaultValue(this IReadOnlyProperty property)
        {
            property.TryGetDefaultValue(out var defaultValue);
            return defaultValue;
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="defaultValue"> The default value, or the CLR default if no explicit default has been set. </param>
        /// <returns> <see langword="true" /> if a default value has been explicitly set; <see langword="false" /> otherwise. </returns>
        public static bool TryGetDefaultValue(this IReadOnlyProperty property, out object? defaultValue)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValue);

            if (annotation != null)
            {
                defaultValue = annotation.Value;
                return defaultValue != null;
            }

            defaultValue = property.ClrType.GetDefaultValue();
            return false;
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The object that is used as the default value for the column this property is mapped to. </returns>
        public static object? GetDefaultValue(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            property.TryGetDefaultValue(storeObject, out var defaultValue);
            return defaultValue;
        }

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <param name="defaultValue"> The default value, or the CLR default if no explicit default has been set. </param>
        /// <returns> <see langword="true" /> if a default value has been explicitly set; <see langword="false" /> otherwise. </returns>
        public static bool TryGetDefaultValue(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            out object? defaultValue)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.DefaultValue);
            if (annotation != null)
            {
                defaultValue = annotation.Value;
                return defaultValue != null;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return TryGetDefaultValue(sharedTableRootProperty, storeObject, out defaultValue);
            }

            defaultValue = property.ClrType.GetDefaultValue();
            return false;
        }

        /// <summary>
        ///     Sets the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetDefaultValue(this IMutableProperty property, object? value)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.DefaultValue, ConvertDefaultValue(property, value));

        /// <summary>
        ///     Sets the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static object? SetDefaultValue(
            this IConventionProperty property,
            object? value,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValue, ConvertDefaultValue(property, value), fromDataAnnotation);

            return value;
        }

        private static object? ConvertDefaultValue(IReadOnlyProperty property, object? value)
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
        public static ConfigurationSource? GetDefaultValueConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.DefaultValue)?.GetConfigurationSource();

        /// <summary>
        ///     Gets the maximum length of data that is allowed in this property. For example, if the property is a <see cref="string" />
        ///     then this is the maximum number of characters.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The maximum length, or <see langword="null" /> if none if defined. </returns>
        public static int? GetMaxLength(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(property, nameof(property));

            var maxLength = property.GetMaxLength();
            if (maxLength != null)
            {
                return maxLength.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null ? GetMaxLength(sharedTableRootProperty, storeObject) : null;
        }

        /// <summary>
        ///     Gets the precision of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of digits.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The precision, or <see langword="null" /> if none is defined. </returns>
        public static int? GetPrecision(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(property, nameof(property));

            var precision = property.GetPrecision();
            if (precision != null)
            {
                return precision;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null ? GetPrecision(sharedTableRootProperty, storeObject) : null;
        }

        /// <summary>
        ///     Gets the scale of data that is allowed in this property.
        ///     For example, if the property is a <see cref="decimal" /> then this is the maximum number of decimal places.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The scale, or <see langword="null" /> if none is defined. </returns>
        public static int? GetScale(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(property, nameof(property));

            var scale = property.GetScale();
            if (scale != null)
            {
                return scale.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null ? GetScale(sharedTableRootProperty, storeObject) : null;
        }

        /// <summary>
        ///     Gets a value indicating whether or not the property can persist Unicode characters.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The Unicode setting, or <see langword="null" /> if none is defined. </returns>
        public static bool? IsUnicode(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(property, nameof(property));

            var unicode = property.IsUnicode();
            if (unicode != null)
            {
                return unicode.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null ? IsUnicode(sharedTableRootProperty, storeObject) : null;
        }

        /// <summary>
        ///     Returns a flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> A flag indicating if the property as capable of storing only fixed-length data, such as strings. </returns>
        public static bool? IsFixedLength(this IReadOnlyProperty property)
            => (bool?)property.FindAnnotation(RelationalAnnotationNames.IsFixedLength)?.Value;

        /// <summary>
        ///     Returns a flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> A flag indicating if the property as capable of storing only fixed-length data, such as strings. </returns>
        public static bool? IsFixedLength(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(RelationalAnnotationNames.IsFixedLength);
            if (annotation != null)
            {
                return (bool?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return IsFixedLength(sharedTableRootProperty, storeObject);
            }

            return null;
        }

        /// <summary>
        ///     Sets a flag indicating whether the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        public static void SetIsFixedLength(this IMutableProperty property, bool? fixedLength)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength);

        /// <summary>
        ///     Sets a flag indicating whether the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static bool? SetIsFixedLength(
            this IConventionProperty property,
            bool? fixedLength,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength, fromDataAnnotation);

            return fixedLength;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IReadOnlyProperty)" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IReadOnlyProperty)" />. </returns>
        public static ConfigurationSource? GetIsFixedLengthConfigurationSource(this IConventionProperty property)
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
        /// <param name="property"> The <see cref="IReadOnlyProperty" />. </param>
        /// <returns> <see langword="true" /> if the mapped column is nullable; <see langword="false" /> otherwise. </returns>
        public static bool IsColumnNullable(this IReadOnlyProperty property)
            => property.IsNullable
                || (property.DeclaringEntityType.BaseType != null && property.DeclaringEntityType.FindDiscriminatorProperty() != null);

        /// <summary>
        ///     <para>
        ///         Checks whether the column mapped to the given property will be nullable
        ///         when created in the database.
        ///     </para>
        ///     <para>
        ///         This depends on the property itself and also how it is mapped. For example,
        ///         derived non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///         As well as properties on optional types sharing the same table.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> <see langword="true" /> if the mapped column is nullable; <see langword="false" /> otherwise. </returns>
        public static bool IsColumnNullable(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property.IsPrimaryKey())
            {
                return false;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.IsColumnNullable(storeObject);
            }

            return property.IsNullable
                || (property.DeclaringEntityType.BaseType != null && property.DeclaringEntityType.FindDiscriminatorProperty() != null)
                || IsOptionalSharingDependent(property.DeclaringEntityType, storeObject, 0);
        }

        private static bool IsOptionalSharingDependent(
            IReadOnlyEntityType entityType,
            in StoreObjectIdentifier storeObject,
            int recursionDepth)
        {
            if (recursionDepth++ == Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable)
            {
                return true;
            }

            bool? optional = null;
            foreach (var linkingForeignKey in entityType.FindRowInternalForeignKeys(storeObject))
            {
                optional = (optional ?? true)
                    && (!linkingForeignKey.IsRequiredDependent
                        || IsOptionalSharingDependent(linkingForeignKey.PrincipalEntityType, storeObject, recursionDepth));
            }

            return optional ?? (entityType.BaseType != null && entityType.FindDiscriminatorProperty() != null);
        }

        /// <summary>
        ///     Returns the comment for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comment for the column this property is mapped to. </returns>
        public static string? GetComment(this IReadOnlyProperty property)
            => property is RuntimeProperty
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)property.FindAnnotation(RelationalAnnotationNames.Comment)?.Value;

        /// <summary>
        ///     Returns the comment for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The comment for the column this property is mapped to. </returns>
        public static string? GetComment(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(RelationalAnnotationNames.Comment);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return GetComment(sharedTableRootProperty, storeObject);
            }

            return null;
        }

        /// <summary>
        ///     Configures a comment to be applied to the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comment"> The comment for the column. </param>
        public static void SetComment(this IMutableProperty property, string? comment)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment);

        /// <summary>
        ///     Configures a comment to be applied to the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetComment(
            this IConventionProperty property,
            string? comment,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment, fromDataAnnotation);

            return comment;
        }

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column comment.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column comment. </returns>
        public static ConfigurationSource? GetCommentConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.Comment)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the collation to be used for the column.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The collation for the column this property is mapped to. </returns>
        public static string? GetCollation(this IReadOnlyProperty property)
            => property is RuntimeProperty
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string?)property.FindAnnotation(RelationalAnnotationNames.Collation)?.Value;

        /// <summary>
        ///     Returns the collation to be used for the column.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The collation for the column this property is mapped to. </returns>
        public static string? GetCollation(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(RelationalAnnotationNames.Collation);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.GetCollation(storeObject);
            }

            return null;
        }

        /// <summary>
        ///     Configures a collation to be used for column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="collation"> The collation for the column. </param>
        public static void SetCollation(this IMutableProperty property, string? collation)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.Collation, collation);

        /// <summary>
        ///     Configures a collation to be used for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="collation"> The collation for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The configured value. </returns>
        public static string? SetCollation(
            this IConventionProperty property,
            string? collation,
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
        public static ConfigurationSource? GetCollationConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.Collation)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping. </returns>
        [DebuggerStepThrough]
        public static RelationalTypeMapping GetRelationalTypeMapping(this IReadOnlyProperty property)
            => (RelationalTypeMapping)property.GetTypeMapping();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        [DebuggerStepThrough]
        [Obsolete("Use FindRelationalTypeMapping")]
        public static RelationalTypeMapping? FindRelationalMapping(this IProperty property)
            => property.FindRelationalTypeMapping();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        [DebuggerStepThrough]
        public static RelationalTypeMapping? FindRelationalTypeMapping(this IReadOnlyProperty property)
            => (RelationalTypeMapping?)property.FindTypeMapping();

        /// <summary>
        ///     Returns the <see cref="RelationalTypeMapping" /> for the given property on a finalized model.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public static RelationalTypeMapping? FindRelationalTypeMapping(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
            => property.FindRelationalTypeMapping();

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IReadOnlyProperty? FindSharedStoreObjectRootProperty(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
            => FindSharedObjectRootProperty(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IMutableProperty? FindSharedStoreObjectRootProperty(
            this IMutableProperty property,
            in StoreObjectIdentifier storeObject)
            => (IMutableProperty?)FindSharedObjectRootProperty(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IConventionProperty? FindSharedStoreObjectRootProperty(
            this IConventionProperty property,
            in StoreObjectIdentifier storeObject)
            => (IConventionProperty?)FindSharedObjectRootProperty(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Finds the first <see cref="IProperty" /> that is mapped to the same column in a shared table-like object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> The property found, or <see langword="null" /> if none was found.</returns>
        public static IProperty? FindSharedStoreObjectRootProperty(
            this IProperty property,
            in StoreObjectIdentifier storeObject)
            => (IProperty?)FindSharedObjectRootProperty(property, storeObject);

        private static IReadOnlyProperty? FindSharedObjectRootProperty(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            Check.NotNull(property, nameof(property));

            var column = property.GetColumnName(storeObject);

            if (column == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.PropertyNotMappedToTable(
                        property.Name, property.DeclaringEntityType.DisplayName(), storeObject.DisplayName()));
            }

            var rootProperty = property;

            // Limit traversal to avoid getting stuck in a cycle (validation will throw for these later)
            // Using a hashset is detrimental to the perf when there are no cycles
            for (var i = 0; i < Metadata.Internal.RelationalEntityTypeExtensions.MaxEntityTypesSharingTable; i++)
            {
                IReadOnlyProperty? linkedProperty = null;
                foreach (var p in rootProperty.DeclaringEntityType
                    .FindRowInternalForeignKeys(storeObject)
                    .SelectMany(fk => fk.PrincipalEntityType.GetProperties()))
                {
                    if (p.GetColumnName(storeObject) == column)
                    {
                        linkedProperty = p;
                        break;
                    }
                }

                if (linkedProperty == null)
                {
                    break;
                }

                rootProperty = linkedProperty;
            }

            return rootProperty == property ? null : rootProperty;
        }

        private static IReadOnlyProperty? FindSharedObjectRootPrimaryKeyProperty(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
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
                    .FindRowInternalForeignKeys(storeObject).FirstOrDefault();
                if (linkingRelationship == null)
                {
                    break;
                }

                principalProperty = linkingRelationship.PrincipalKey.Properties[linkingRelationship.Properties.IndexOf(principalProperty)];
            }

            return principalProperty == property ? null : principalProperty;
        }

        private static IReadOnlyProperty? FindSharedObjectRootConcurrencyTokenProperty(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
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
                    .FindRowInternalForeignKeys(storeObject).FirstOrDefault();
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
        ///         Returns the property facet overrides for a particular table-like store object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IReadOnlyAnnotatable? FindOverrides(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
            => RelationalPropertyOverrides.Find(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table-like store object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IMutableAnnotatable? FindOverrides(this IMutableProperty property, in StoreObjectIdentifier storeObject)
            => (IMutableAnnotatable?)RelationalPropertyOverrides.Find(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table-like store object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IConventionAnnotatable? FindOverrides(this IConventionProperty property, in StoreObjectIdentifier storeObject)
            => (IConventionAnnotatable?)RelationalPropertyOverrides.Find(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table-like store object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IAnnotatable? FindOverrides(this IProperty property, in StoreObjectIdentifier storeObject)
            => RelationalPropertyOverrides.Find(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table-like store object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IMutableAnnotatable GetOrCreateOverrides(
            this IMutableProperty property,
            in StoreObjectIdentifier storeObject)
            => RelationalPropertyOverrides.GetOrCreate(property, storeObject);

        /// <summary>
        ///     <para>
        ///         Returns the property facet overrides for a particular table-like store object.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="storeObject"> The identifier of the table-like store object containing the column. </param>
        /// <returns> An object that stores property facet overrides. </returns>
        public static IConventionAnnotatable GetOrCreateOverrides(
            this IConventionProperty property,
            in StoreObjectIdentifier storeObject)
            => RelationalPropertyOverrides.GetOrCreate(property, storeObject);
    }
}
