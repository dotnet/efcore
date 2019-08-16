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
using Microsoft.EntityFrameworkCore.Internal;
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
            => (string)property[RelationalAnnotationNames.ColumnName]
               ?? GetDefaultColumnName(property);

        /// <summary>
        ///     Returns the default column name to which the property would be mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The default column name to which the property would be mapped. </returns>
        public static string GetDefaultColumnName([NotNull] this IProperty property)
        {
            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return sharedTablePrincipalPrimaryKeyProperty.GetColumnName();
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
                    if (entityType.GetTableName() == ownerType.GetTableName()
                        && entityType.GetSchema() == ownerType.GetSchema())
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

            var baseName = property.Name;
            if (builder != null)
            {
                builder.Append(property.Name);
                baseName = builder.ToString();
            }

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
        public static void SetColumnName(
            [NotNull] this IConventionProperty property, [CanBeNull] string name, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name. </returns>
        public static ConfigurationSource? GetColumnNameConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ColumnName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the database type of the column to which the property is mapped.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The database type of the column to which the property is mapped. </returns>
        public static string GetColumnType([NotNull] this IProperty property)
        {
            var columnType = (string)property[RelationalAnnotationNames.ColumnType];
            if (columnType != null)
            {
                return columnType;
            }

            return GetDefaultColumnType(property);
        }

        private static string GetDefaultColumnType(IProperty property)
        {
            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            return sharedTablePrincipalPrimaryKeyProperty != null
                ? sharedTablePrincipalPrimaryKeyProperty.GetColumnType()
                : property.FindRelationalMapping()?.StoreType;
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
        public static void SetColumnType(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the column name.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the column name. </returns>
        public static ConfigurationSource? GetColumnTypeConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ColumnType)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the SQL expression that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The SQL expression that is used as the default value for the column this property is mapped to. </returns>
        public static string GetDefaultValueSql([NotNull] this IProperty property)
        {
            var sql = (string)property[RelationalAnnotationNames.DefaultValueSql];
            if (sql != null)
            {
                return sql;
            }

            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return GetDefaultValueSql(sharedTablePrincipalPrimaryKeyProperty);
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
        public static void SetDefaultValueSql(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

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
            var sql = (string)property[RelationalAnnotationNames.ComputedColumnSql];
            if (sql != null)
            {
                return sql;
            }

            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return GetComputedColumnSql(sharedTablePrincipalPrimaryKeyProperty);
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
        public static void SetComputedColumnSql(
            [NotNull] this IConventionProperty property, [CanBeNull] string value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value)),
                fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for the computed value SQL expression.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the computed value SQL expression. </returns>
        public static ConfigurationSource? GetComputedColumnSqlConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.ComputedColumnSql)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the object that is used as the default value for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The object that is used as the default value for the column this property is mapped to. </returns>
        public static object GetDefaultValue([NotNull] this IProperty property)
        {
            var value = property[RelationalAnnotationNames.DefaultValue];
            if (value != null)
            {
                return value;
            }

            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return GetDefaultValue(sharedTablePrincipalPrimaryKeyProperty);
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
        public static void SetDefaultValue(
            [NotNull] this IConventionProperty property, [CanBeNull] object value, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.DefaultValue, ConvertDefaultValue(property, value), fromDataAnnotation);

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
        public static bool IsFixedLength([NotNull] this IProperty property)
            => (bool?)property[RelationalAnnotationNames.IsFixedLength] ?? GetDefaultIsFixedLength(property);

        private static bool GetDefaultIsFixedLength(IProperty property)
        {
            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            return sharedTablePrincipalPrimaryKeyProperty != null && IsFixedLength(sharedTablePrincipalPrimaryKeyProperty);
        }

        /// <summary>
        ///     Sets a flag indicating whether the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        public static void SetIsFixedLength([NotNull] this IMutableProperty property, bool? fixedLength)
            => property.SetOrRemoveAnnotation(
                RelationalAnnotationNames.IsFixedLength,
                fixedLength);

        /// <summary>
        ///     Sets a flag indicating whether the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetIsFixedLength([NotNull] this IConventionProperty property, bool? fixedLength, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength, fromDataAnnotation);

        /// <summary>
        ///     Gets the <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IProperty)" />.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for <see cref="IsFixedLength(IProperty)" />. </returns>
        public static ConfigurationSource? GetIsFixedLengthConfigurationSource([NotNull] this IConventionProperty property)
            => property.FindAnnotation(RelationalAnnotationNames.IsFixedLength)?.GetConfigurationSource();

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
        /// <returns> The type mapping, or null if none was found. </returns>
        [DebuggerStepThrough]
        public static RelationalTypeMapping FindRelationalMapping([NotNull] this IProperty property)
            => (RelationalTypeMapping)property.FindTypeMapping();

        /// <summary>
        ///     <para>
        ///         Checks whether or not the column mapped to the given <see cref="IProperty" /> will be nullable
        ///         or not when created in the database.
        ///     </para>
        ///     <para>
        ///         This can depend not just on the property itself, but also how it is mapped. For example,
        ///         non-nullable properties in a TPH type hierarchy will be mapped to nullable columns.
        ///     </para>
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" />. </param>
        /// <returns> <c>True</c> if the mapped column is nullable; <c>false</c> otherwise. </returns>
        public static bool IsColumnNullable([NotNull] this IProperty property)
            => !property.IsPrimaryKey()
               && (property.DeclaringEntityType.BaseType != null
                   || property.IsNullable
                   || IsTableSplitting(property.DeclaringEntityType));

        private static bool IsTableSplitting(IEntityType entityType)
            => entityType.FindPrimaryKey()?.Properties[0].FindSharedTableLink() != null;

        /// <summary>
        ///     <para>
        ///         Finds the <see cref="IProperty" /> that represents the same primary key property
        ///         as the given property, but potentially in a shared root table.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The property found, or <code>null</code> if none was found.</returns>
        public static IProperty FindSharedTableRootPrimaryKeyProperty([NotNull] this IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var principalProperty = property;
            HashSet<IEntityType> visitedTypes = null;
            while (true)
            {
                var linkingRelationship = principalProperty.FindSharedTableLink();
                if (linkingRelationship == null)
                {
                    break;
                }

                if (visitedTypes == null)
                {
                    visitedTypes = new HashSet<IEntityType>
                    {
                        linkingRelationship.DeclaringEntityType
                    };
                }

                if (!visitedTypes.Add(linkingRelationship.PrincipalEntityType))
                {
                    return null;
                }

                principalProperty = linkingRelationship.PrincipalKey.Properties[linkingRelationship.Properties.IndexOf(principalProperty)];
            }

            return principalProperty == property ? null : principalProperty;
        }

        /// <summary>
        ///     Returns the comment for the column this property is mapped to.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The comment for the column this property is mapped to. </returns>
        public static string GetComment([NotNull] this IProperty property)
        {
            var value = (string)property[RelationalAnnotationNames.Comment];
            if (value != null)
            {
                return value;
            }

            var sharedTablePrincipalPrimaryKeyProperty = property.FindSharedTableRootPrimaryKeyProperty();
            if (sharedTablePrincipalPrimaryKeyProperty != null)
            {
                return GetComment(sharedTablePrincipalPrimaryKeyProperty);
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
        public static void SetComment(
            [NotNull] this IConventionProperty property, [CanBeNull] string comment, bool fromDataAnnotation = false)
            => property.SetOrRemoveAnnotation(RelationalAnnotationNames.Comment, comment, fromDataAnnotation);

    }
}
