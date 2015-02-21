// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerBuilderExtensions
    {
        public static SqlServerPropertyBuilder ForSqlServer<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return new SqlServerPropertyBuilder(propertyBuilder.Metadata);
        }

        public static TPropertyBuilder ForSqlServer<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder,
            [NotNull] Action<SqlServerPropertyBuilder> sqlServerPropertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(sqlServerPropertyBuilder, nameof(sqlServerPropertyBuilder));

            sqlServerPropertyBuilder(ForSqlServer(propertyBuilder));

            return (TPropertyBuilder)propertyBuilder;
        }

        public static SqlServerEntityBuilder ForSqlServer<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            return new SqlServerEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForSqlServer<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder,
            [NotNull] Action<SqlServerEntityBuilder> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            relationalEntityBuilder(ForSqlServer(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static SqlServerEntityBuilder ForSqlServer<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder)
            where TEntity : class
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            return new SqlServerEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForSqlServer<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder,
            [NotNull] Action<SqlServerEntityBuilder> relationalEntityBuilder)
            where TEntity : class
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            relationalEntityBuilder(ForSqlServer(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static SqlServerKeyBuilder ForSqlServer<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            return new SqlServerKeyBuilder(keyBuilder.Metadata);
        }

        public static TKeyBuilder ForSqlServer<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder,
            [NotNull] Action<SqlServerKeyBuilder> relationalKeyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NotNull(relationalKeyBuilder, nameof(relationalKeyBuilder));

            relationalKeyBuilder(ForSqlServer(keyBuilder));

            return (TKeyBuilder)keyBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new SqlServerForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TForeignKeyBuilder ForSqlServer<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalForeignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(relationalForeignKeyBuilder, nameof(relationalForeignKeyBuilder));

            relationalForeignKeyBuilder(ForSqlServer(foreignKeyBuilder));

            return (TForeignKeyBuilder)foreignKeyBuilder;
        }

        public static SqlServerIndexBuilder ForSqlServer<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return new SqlServerIndexBuilder(indexBuilder.Metadata);
        }

        public static TIndexBuilder ForSqlServer<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder,
            [NotNull] Action<SqlServerIndexBuilder> relationalIndexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(relationalIndexBuilder, nameof(relationalIndexBuilder));

            relationalIndexBuilder(ForSqlServer(indexBuilder));

            return (TIndexBuilder)indexBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TOneToManyBuilder>(
            [NotNull] this IOneToManyBuilder<TOneToManyBuilder> oneToManyBuilder)
            where TOneToManyBuilder : IOneToManyBuilder<TOneToManyBuilder>
        {
            Check.NotNull(oneToManyBuilder, nameof(oneToManyBuilder));

            return new SqlServerForeignKeyBuilder(oneToManyBuilder.Metadata);
        }

        public static TOneToManyBuilder ForSqlServer<TOneToManyBuilder>(
            [NotNull] this IOneToManyBuilder<TOneToManyBuilder> oneToManyBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalOneToManyBuilder)
            where TOneToManyBuilder : IOneToManyBuilder<TOneToManyBuilder>
        {
            Check.NotNull(oneToManyBuilder, nameof(oneToManyBuilder));
            Check.NotNull(relationalOneToManyBuilder, nameof(relationalOneToManyBuilder));

            relationalOneToManyBuilder(ForSqlServer(oneToManyBuilder));

            return (TOneToManyBuilder)oneToManyBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TManyToOneBuilder>(
            [NotNull] this IManyToOneBuilder<TManyToOneBuilder> manyToOneBuilder)
            where TManyToOneBuilder : IManyToOneBuilder<TManyToOneBuilder>
        {
            Check.NotNull(manyToOneBuilder, nameof(manyToOneBuilder));

            return new SqlServerForeignKeyBuilder(manyToOneBuilder.Metadata);
        }

        public static TManyToOneBuilder ForSqlServer<TManyToOneBuilder>(
            [NotNull] this IManyToOneBuilder<TManyToOneBuilder> manyToOneBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalManyToOneBuilder)
            where TManyToOneBuilder : IManyToOneBuilder<TManyToOneBuilder>
        {
            Check.NotNull(manyToOneBuilder, nameof(manyToOneBuilder));
            Check.NotNull(relationalManyToOneBuilder, nameof(relationalManyToOneBuilder));

            relationalManyToOneBuilder(ForSqlServer(manyToOneBuilder));

            return (TManyToOneBuilder)manyToOneBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TOneToOneBuilder>(
            [NotNull] this IOneToOneBuilder<TOneToOneBuilder> oneToOneBuilder)
            where TOneToOneBuilder : IOneToOneBuilder<TOneToOneBuilder>
        {
            Check.NotNull(oneToOneBuilder, nameof(oneToOneBuilder));

            return new SqlServerForeignKeyBuilder(oneToOneBuilder.Metadata);
        }

        public static TOneToOneBuilder ForSqlServer<TOneToOneBuilder>(
            [NotNull] this IOneToOneBuilder<TOneToOneBuilder> oneToOneBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalOneToOneBuilder)
            where TOneToOneBuilder : IOneToOneBuilder<TOneToOneBuilder>
        {
            Check.NotNull(oneToOneBuilder, nameof(oneToOneBuilder));
            Check.NotNull(relationalOneToOneBuilder, nameof(relationalOneToOneBuilder));

            relationalOneToOneBuilder(ForSqlServer(oneToOneBuilder));

            return (TOneToOneBuilder)oneToOneBuilder;
        }

        public static SqlServerModelBuilder ForSqlServer<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            return new SqlServerModelBuilder(modelBuilder.Metadata);
        }

        public static TModelBuilder ForSqlServer<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder,
            [NotNull] Action<SqlServerModelBuilder> sqlServerModelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(sqlServerModelBuilder, nameof(sqlServerModelBuilder));

            sqlServerModelBuilder(ForSqlServer(modelBuilder));

            return (TModelBuilder)modelBuilder;
        }
    }
}
