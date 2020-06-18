// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class RelationalPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.SetColumnName(name);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)HasColumnName((PropertyBuilder)propertyBuilder, name);

        /// <summary>
        ///     Configures the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetColumnName(name, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetColumnName(name, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to in a particular table when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The table schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetColumnName(name, tableName, schema, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetColumnName(name, tableName, schema, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given column can be set for the property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the property can be mapped to the given column. </returns>
        public static bool CanSetColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.ColumnName, name, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the given column for a particular table can be set for the property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="tableName"> The table name. </param>
        /// <param name="schema"> The table schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the property can be mapped to the given column. </returns>
        public static bool CanSetColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [NotNull] string tableName,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            var overrides = RelationalPropertyOverrides.Find(propertyBuilder.Metadata, tableName, schema);
            return overrides == null
                || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                    .Overrides(overrides.GetColumnNameConfigurationSource())
                               || overrides.ColumnName == name;
        }

        /// <summary>
        ///     Configures the column that the property maps to in a view in a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasViewColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.SetViewColumnName(name);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to in a view in a relational database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasViewColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)HasViewColumnName((PropertyBuilder)propertyBuilder, name);

        /// <summary>
        ///     Configures the column that the property maps to in a view in a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasViewColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetViewColumnName(name, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetViewColumnName(name, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the column that the property maps to in a particular view in a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The view schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasViewColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [NotNull] string viewName,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetViewColumnName(name, viewName, schema, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetViewColumnName(name, viewName, schema, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given view column can be set for the property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the property can be mapped to the given column. </returns>
        public static bool CanSetViewColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.ViewColumnName, name, fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the given column in a particular view can be set for the property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the column. </param>
        /// <param name="viewName"> The view name. </param>
        /// <param name="schema"> The view schema. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the property can be mapped to the given column. </returns>
        public static bool CanSetViewColumnName(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string name,
            [NotNull] string viewName,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            var overrides = RelationalPropertyOverrides.Find(propertyBuilder.Metadata, viewName, schema);
            return overrides == null
                || (fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention)
                    .Overrides(overrides.GetViewColumnNameConfigurationSource())
                               || overrides.ViewColumnName == name;
        }

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting a relational database.
        ///     This should be the complete type name, including precision, scale, length, etc.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string typeName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(typeName, nameof(typeName));

            propertyBuilder.Metadata.SetColumnType(typeName);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting a relational database.
        ///     This should be the complete type name, including precision, scale, length, etc.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string typeName)
            => (PropertyBuilder<TProperty>)HasColumnType((PropertyBuilder)propertyBuilder, typeName);

        /// <summary>
        ///     Configures the data type of the column that the property maps to when targeting a relational database.
        ///     This should be the complete type name, including precision, scale, length, etc.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasColumnType(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string typeName,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetColumnType(typeName, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetColumnType(typeName, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given data type can be set for the property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given data type can be set for the property. </returns>
        public static bool CanSetColumnType(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string typeName,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.ColumnType, typeName, fromDataAnnotation);

        /// <summary>
        ///     Configures the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public static PropertyBuilder IsFixedLength(
            [NotNull] this PropertyBuilder propertyBuilder,
            bool fixedLength = true)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.SetIsFixedLength(fixedLength);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public static PropertyBuilder<TProperty> IsFixedLength<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            bool fixedLength = true)
            => (PropertyBuilder<TProperty>)IsFixedLength((PropertyBuilder)propertyBuilder, fixedLength);

        /// <summary>
        ///     Configures the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder IsFixedLength(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            bool? fixedLength,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetIsFixedLength(fixedLength, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetIsFixedLength(fixedLength, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the property can be configured as being fixed length or not.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the property can be configured as being fixed length or not. </returns>
        public static bool CanSetIsFixedLength(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            bool? fixedLength,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength, fromDataAnnotation);

        /// <summary>
        ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression for the default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasDefaultValueSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            propertyBuilder.Metadata.SetDefaultValueSql(sql);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression for the default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)HasDefaultValueSql((PropertyBuilder)propertyBuilder, sql);

        /// <summary>
        ///     Configures the default value expression for the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression for the default value of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasDefaultValueSql(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetDefaultValueSql(sql, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetDefaultValueSql(sql, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given default value expression can be set for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression for the default value of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given default value expression can be set for the column. </returns>
        public static bool CanSetDefaultValueSql(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
                Check.NullButNotEmpty(sql, nameof(sql)),
                fromDataAnnotation);

        /// <summary>
        ///     Configures the property to map to a computed column when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <param name="stored">
        ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
        ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
        ///     <see langword="null" /> selects the database provider default.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasComputedColumnSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql,
            bool? stored = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            propertyBuilder.Metadata.SetComputedColumnSql(sql);

            if (stored != null)
            {
                propertyBuilder.Metadata.SetIsStored(stored);
            }

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to map to a computed column when targeting a relational database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <param name="stored">
        ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
        ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
        ///     <see langword="null" /> selects the database provider default.
        /// </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql,
            bool? stored = null)
            => (PropertyBuilder<TProperty>)HasComputedColumnSql((PropertyBuilder)propertyBuilder, sql, stored);

        /// <summary>
        ///     Configures the property to map to a computed column when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasComputedColumnSql(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetComputedColumnSql(sql, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetComputedColumnSql(sql, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to map to a computed column of the given type when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="stored">
        ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
        ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
        ///     <see langword="null" /> selects the database provider default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder IsStoredComputedColumn(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            bool? stored,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetIsStoredComputedColumn(stored, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetIsStored(stored, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given computed value SQL expression can be set for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given computed value SQL expression can be set for the column. </returns>
        public static bool CanSetComputedColumnSql(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string sql,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
                Check.NullButNotEmpty(sql, nameof(sql)),
                fromDataAnnotation);

        /// <summary>
        ///     Returns a value indicating whether the given computed column type can be set for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="stored">
        ///     If <see langword="true" />, the computed value is calculated on row modification and stored in the database like a regular column.
        ///     If <see langword="false" />, the value is computed when the value is read, and does not occupy any actual storage.
        ///     <see langword="null" /> selects the database provider default.
        /// </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given computed column type can be set for the column. </returns>
        public static bool CanSetIsStoredComputedColumn(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            bool? stored,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(
                RelationalAnnotationNames.IsStored,
                stored,
                fromDataAnnotation);

        /// <summary>
        ///     <para>
        ///         Configures the default value for the column that the property maps
        ///         to when targeting a relational database.
        ///     </para>
        ///     <para>
        ///         When called with no argument, this method tells EF that a column has a default
        ///         value constraint of some sort without needing to specify exactly what it is.
        ///         This can be useful when mapping EF to an existing database.
        ///     </para>
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasDefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value = null)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.SetDefaultValue(value ?? DBNull.Value);

            return propertyBuilder;
        }

        /// <summary>
        ///     <para>
        ///         Configures the default value for the column that the property maps
        ///         to when targeting a relational database.
        ///     </para>
        ///     <para>
        ///         When called with no argument, this method tells EF that a column has a default
        ///         value constraint of some sort without needing to specify exactly what it is.
        ///         This can be useful when mapping EF to an existing database.
        ///     </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasDefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value = null)
            => (PropertyBuilder<TProperty>)HasDefaultValue((PropertyBuilder)propertyBuilder, value);

        /// <summary>
        ///     Configures the default value for the column that the property maps to when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasDefaultValue(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] object value,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetDefaultValue(value, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetDefaultValue(value, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as default for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="value"> The default value of the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given value can be set as default for the column. </returns>
        public static bool CanSetDefaultValue(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] object value,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(
                RelationalAnnotationNames.DefaultValue,
                value,
                fromDataAnnotation);

        /// <summary>
        ///     Configures a comment to be applied to the column
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasComment(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string comment)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.SetComment(comment);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures a comment to be applied to the column
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasComment<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string comment)
            => (PropertyBuilder<TProperty>)HasComment((PropertyBuilder)propertyBuilder, comment);

        /// <summary>
        ///     Configures a comment to be applied to the column
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder HasComment(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string comment,
            bool fromDataAnnotation = false)
        {
            if (!propertyBuilder.CanSetComment(comment, fromDataAnnotation))
            {
                return null;
            }

            propertyBuilder.Metadata.SetComment(comment, fromDataAnnotation);
            return propertyBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as comment for the column.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="comment"> The comment for the column. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given value can be set as default for the column. </returns>
        public static bool CanSetComment(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string comment,
            bool fromDataAnnotation = false)
            => propertyBuilder.CanSetAnnotation(
                RelationalAnnotationNames.Comment,
                comment,
                fromDataAnnotation);

        /// <summary>
        ///     Configures the property to use the given collation. The database column will be be created with the given
        ///     collation, and it will be used implicitly in all collation-sensitive operations.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="collation"> The collation for the column. </param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder UseCollation([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string collation)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(collation, nameof(collation));

            propertyBuilder.Metadata.SetCollation(collation);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to use the given collation. The database column will be be created with the given
        ///     collation, and it will be used implicitly in all collation-sensitive operations.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="collation"> The collation for the column. </param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static PropertyBuilder<TProperty> UseCollation<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder, [CanBeNull] string collation)
            => (PropertyBuilder<TProperty>)UseCollation((PropertyBuilder)propertyBuilder, collation);

        /// <summary>
        ///     Configures the property to use the given collation. The database column will be be created with the given
        ///     collation, and it will be used implicitly in all collation-sensitive operations.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="collation"> The collation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionPropertyBuilder UseCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            if (propertyBuilder.CanSetCollation(collation, fromDataAnnotation))
            {
                propertyBuilder.Metadata.SetCollation(collation, fromDataAnnotation);

                return propertyBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given value can be set as the collation.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="collation"> The collation. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given value can be set as default for the column. </returns>
        public static bool CanSetCollation(
            [NotNull] this IConventionPropertyBuilder propertyBuilder,
            [CanBeNull] string collation,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return propertyBuilder.CanSetAnnotation(RelationalAnnotationNames.Collation, collation, fromDataAnnotation);
        }
    }
}
