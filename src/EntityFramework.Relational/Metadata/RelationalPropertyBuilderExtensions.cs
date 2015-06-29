// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalPropertyBuilderExtensions
    {
        public static PropertyBuilder HasColumnName(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.Relational().Column = name;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)HasColumnName((PropertyBuilder)propertyBuilder, name);

        public static PropertyBuilder HasColumnOrder(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] int? columnOrder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.Relational().ColumnOrder = columnOrder;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasColumnOrder<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] int? columnOrder)
            => (PropertyBuilder<TProperty>)HasColumnOrder((PropertyBuilder)propertyBuilder, columnOrder);

        public static PropertyBuilder HasColumnType(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string typeName)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(typeName, nameof(typeName));

            propertyBuilder.Metadata.Relational().ColumnType = typeName;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> HasColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string typeName)
            => (PropertyBuilder<TProperty>)HasColumnType((PropertyBuilder)propertyBuilder, typeName);

        public static PropertyBuilder DefaultValueSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            propertyBuilder.Metadata.Relational().DefaultValueSql = sql;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> DefaultValueSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)DefaultValueSql((PropertyBuilder)propertyBuilder, sql);

        public static PropertyBuilder DefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            propertyBuilder.Metadata.Relational().DefaultValue = value;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> DefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)DefaultValue((PropertyBuilder)propertyBuilder, value);
    }
}
