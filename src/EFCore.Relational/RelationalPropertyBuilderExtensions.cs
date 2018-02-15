// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

            propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Relational(ConfigurationSource.Explicit).HasColumnName(name);

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

            propertyBuilder.GetInfrastructure<InternalPropertyBuilder>().Relational(ConfigurationSource.Explicit).HasColumnType(typeName);

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

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.Relational(ConfigurationSource.Explicit).HasDefaultValueSql(sql);

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
        ///     Configures the property to map to a computed column when targeting a relational database.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder HasComputedColumnSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.Relational(ConfigurationSource.Explicit).HasComputedColumnSql(sql);

            return propertyBuilder;
        }

        /// <summary>
        ///     Configures the property to map to a computed column when targeting a relational database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="sql"> The SQL expression that computes values for the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)HasComputedColumnSql((PropertyBuilder)propertyBuilder, sql);

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

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.Relational(ConfigurationSource.Explicit).HasDefaultValue(value ?? DBNull.Value);

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

            var internalPropertyBuilder = propertyBuilder.GetInfrastructure<InternalPropertyBuilder>();
            internalPropertyBuilder.Relational(ConfigurationSource.Explicit).IsFixedLength(fixedLength);

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
    }
}
