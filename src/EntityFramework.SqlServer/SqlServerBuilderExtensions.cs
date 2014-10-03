// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerBuilderExtensions
    {
        public static SqlServerPropertyBuilder ForSqlServer<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            return new SqlServerPropertyBuilder(propertyBuilder.Metadata);
        }

        public static TPropertyBuilder ForSqlServer<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder,
            [NotNull] Action<SqlServerPropertyBuilder> sqlServerPropertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(sqlServerPropertyBuilder, "sqlServerPropertyBuilder");

            sqlServerPropertyBuilder(ForSqlServer(propertyBuilder));

            return (TPropertyBuilder)propertyBuilder;
        }

        public static SqlServerEntityBuilder ForSqlServer<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            return new SqlServerEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForSqlServer<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder,
            [NotNull] Action<SqlServerEntityBuilder> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            relationalEntityBuilder(ForSqlServer(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static SqlServerEntityBuilder ForSqlServer<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            return new SqlServerEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForSqlServer<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder,
            [NotNull] Action<SqlServerEntityBuilder> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            relationalEntityBuilder(ForSqlServer(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static SqlServerKeyBuilder ForSqlServer<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, "keyBuilder");

            return new SqlServerKeyBuilder(keyBuilder.Metadata);
        }

        public static TKeyBuilder ForSqlServer<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder,
            [NotNull] Action<SqlServerKeyBuilder> relationalKeyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, "keyBuilder");
            Check.NotNull(relationalKeyBuilder, "relationalKeyBuilder");

            relationalKeyBuilder(ForSqlServer(keyBuilder));

            return (TKeyBuilder)keyBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new SqlServerForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TForeignKeyBuilder ForSqlServer<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalForeignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalForeignKeyBuilder, "relationalForeignKeyBuilder");

            relationalForeignKeyBuilder(ForSqlServer(foreignKeyBuilder));

            return (TForeignKeyBuilder)foreignKeyBuilder;
        }

        public static SqlServerIndexBuilder ForSqlServer<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, "indexBuilder");

            return new SqlServerIndexBuilder(indexBuilder.Metadata);
        }

        public static TIndexBuilder ForSqlServer<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder,
            [NotNull] Action<SqlServerIndexBuilder> relationalIndexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, "indexBuilder");
            Check.NotNull(relationalIndexBuilder, "relationalIndexBuilder");

            relationalIndexBuilder(ForSqlServer(indexBuilder));

            return (TIndexBuilder)indexBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TOneToManyBuilder>(
            [NotNull] this IOneToManyBuilder<TOneToManyBuilder> foreignKeyBuilder)
            where TOneToManyBuilder : IOneToManyBuilder<TOneToManyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new SqlServerForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TOneToManyBuilder ForSqlServer<TOneToManyBuilder>(
            [NotNull] this IOneToManyBuilder<TOneToManyBuilder> foreignKeyBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalOneToManyBuilder)
            where TOneToManyBuilder : IOneToManyBuilder<TOneToManyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalOneToManyBuilder, "relationalOneToManyBuilder");

            relationalOneToManyBuilder(ForSqlServer(foreignKeyBuilder));

            return (TOneToManyBuilder)foreignKeyBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TManyToOneBuilder>(
            [NotNull] this IManyToOneBuilder<TManyToOneBuilder> foreignKeyBuilder)
            where TManyToOneBuilder : IManyToOneBuilder<TManyToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new SqlServerForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TManyToOneBuilder ForSqlServer<TManyToOneBuilder>(
            [NotNull] this IManyToOneBuilder<TManyToOneBuilder> foreignKeyBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalManyToOneBuilder)
            where TManyToOneBuilder : IManyToOneBuilder<TManyToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalManyToOneBuilder, "relationalManyToOneBuilder");

            relationalManyToOneBuilder(ForSqlServer(foreignKeyBuilder));

            return (TManyToOneBuilder)foreignKeyBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TOneToOneBuilder>(
            [NotNull] this IOneToOneBuilder<TOneToOneBuilder> foreignKeyBuilder)
            where TOneToOneBuilder : IOneToOneBuilder<TOneToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new SqlServerForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TOneToOneBuilder ForSqlServer<TOneToOneBuilder>(
            [NotNull] this IOneToOneBuilder<TOneToOneBuilder> foreignKeyBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalOneToOneBuilder)
            where TOneToOneBuilder : IOneToOneBuilder<TOneToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalOneToOneBuilder, "relationalOneToOneBuilder");

            relationalOneToOneBuilder(ForSqlServer(foreignKeyBuilder));

            return (TOneToOneBuilder)foreignKeyBuilder;
        }
    }
}
