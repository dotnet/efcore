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
    public static class SqlitePropertyBuilderExtensions
    {
        public static PropertyBuilder ForSqliteHasColumnName([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string name)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            propertyBuilder.Metadata.Sqlite().ColumnName = name;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ForSqliteHasColumnName<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)((PropertyBuilder)propertyBuilder).ForSqliteHasColumnName(name);

        public static PropertyBuilder ForSqliteHasColumnType([NotNull] this PropertyBuilder propertyBuilder, [CanBeNull] string type)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(type, nameof(type));

            propertyBuilder.Metadata.Sqlite().ColumnType = type;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ForSqliteHasColumnType<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string type)
            => (PropertyBuilder<TProperty>)((PropertyBuilder)propertyBuilder).ForSqliteHasColumnType(type);

        public static PropertyBuilder ForSqliteHasDefaultValueSql(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string sql)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NullButNotEmpty(sql, nameof(sql));

            var property = (Property)propertyBuilder.Metadata;
            if (ConfigurationSource.Convention.Overrides(property.GetValueGeneratedConfigurationSource()))
            {
                property.SetValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            }

            propertyBuilder.Metadata.Sqlite().DefaultValueSql = sql;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ForSqliteHasDefaultValueSql<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string sql)
            => (PropertyBuilder<TProperty>)((PropertyBuilder)propertyBuilder).ForSqliteHasDefaultValueSql(sql);

        public static PropertyBuilder ForSqliteHasDefaultValue(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] object value)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            var property = (Property)propertyBuilder.Metadata;
            if (ConfigurationSource.Convention.Overrides(property.GetValueGeneratedConfigurationSource()))
            {
                property.SetValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
            }

            propertyBuilder.Metadata.Sqlite().DefaultValue = value;

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ForSqliteHasDefaultValue<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] object value)
            => (PropertyBuilder<TProperty>)((PropertyBuilder)propertyBuilder).ForSqliteHasDefaultValue(value);
    }
}
