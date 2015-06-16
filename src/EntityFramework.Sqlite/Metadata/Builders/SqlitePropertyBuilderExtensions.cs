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
        public static PropertyBuilder SqliteColumn([NotNull] this PropertyBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().Column = name;

            return builder;
        }

        public static PropertyBuilder<TEntity> SqliteColumn<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).SqliteColumn(name);

        public static PropertyBuilder SqliteColumnType([NotNull] this PropertyBuilder builder, [CanBeNull] string type)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(type, nameof(type));

            builder.Metadata.Sqlite().ColumnType = type;

            return builder;
        }

        public static PropertyBuilder<TEntity> SqliteColumnType<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string type)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).SqliteColumnType(type);

        public static PropertyBuilder SqliteDefaultExpression(
            [NotNull] this PropertyBuilder builder,
            [CanBeNull] string expression)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(expression, nameof(expression));

            builder.Metadata.Sqlite().DefaultExpression = expression;

            return builder;
        }

        public static PropertyBuilder<TEntity> SqliteDefaultExpression<TEntity>(
            [NotNull] this PropertyBuilder<TEntity> builder,
            [CanBeNull] string expression)
            where TEntity : class
            => (PropertyBuilder<TEntity>)((PropertyBuilder)builder).SqliteDefaultExpression(expression);
    }
}
