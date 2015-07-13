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
        public static PropertyBuilder HasSqliteColumnName([NotNull] this PropertyBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().Column = name;

            return builder;
        }

        public static PropertyBuilder<TEntity> HasSqliteColumnName<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).HasSqliteColumnName(name);

        public static PropertyBuilder HasSqliteColumnType([NotNull] this PropertyBuilder builder, [CanBeNull] string type)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(type, nameof(type));

            builder.Metadata.Sqlite().ColumnType = type;

            return builder;
        }

        public static PropertyBuilder<TEntity> HasSqliteColumnType<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string type)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).HasSqliteColumnType(type);

        public static PropertyBuilder SqliteDefaultValueSql(
            [NotNull] this PropertyBuilder builder,
            [CanBeNull] string sql)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(sql, nameof(sql));

            builder.Metadata.Sqlite().DefaultValueSql = sql;

            return builder;
        }

        public static PropertyBuilder<TEntity> SqliteDefaultValueSql<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string sql)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).SqliteDefaultValueSql(sql);
    }
}
