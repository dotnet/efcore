// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlitePropertyBuilderExtensions
    {
        public static PropertyBuilder ForSqliteHasColumnName([NotNull] this PropertyBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().ColumnName = name;

            return builder;
        }

        public static PropertyBuilder<TEntity> ForSqliteHasColumnName<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).ForSqliteHasColumnName(name);

        public static PropertyBuilder ForSqliteHasColumnType([NotNull] this PropertyBuilder builder, [CanBeNull] string type)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(type, nameof(type));

            builder.Metadata.Sqlite().ColumnType = type;

            return builder;
        }

        public static PropertyBuilder<TEntity> ForSqliteHasColumnType<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string type)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).ForSqliteHasColumnType(type);

        public static PropertyBuilder ForSqliteHasDefaultValueSql(
            [NotNull] this PropertyBuilder builder,
            [CanBeNull] string sql)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(sql, nameof(sql));

            builder.ValueGeneratedOnAdd();
            builder.Metadata.Sqlite().GeneratedValueSql = sql;

            return builder;
        }

        public static PropertyBuilder<TEntity> ForSqliteHasDefaultValueSql<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string sql)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).ForSqliteHasDefaultValueSql(sql);
    }
}
