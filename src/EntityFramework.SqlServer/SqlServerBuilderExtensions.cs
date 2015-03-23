// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
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

        public static SqlServerEntityTypeBuilder ForSqlServer<TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntityTypeBuilder> entityTypeBuilder)
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return new SqlServerEntityTypeBuilder(entityTypeBuilder.Metadata);
        }

        public static TEntityTypeBuilder ForSqlServer<TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntityTypeBuilder> entityTypeBuilder,
            [NotNull] Action<SqlServerEntityTypeBuilder> relationalEntityTypeBuilder)
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            relationalEntityTypeBuilder(ForSqlServer(entityTypeBuilder));

            return (TEntityTypeBuilder)entityTypeBuilder;
        }

        public static SqlServerEntityTypeBuilder ForSqlServer<TEntity, TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntity, TEntityTypeBuilder> entityTypeBuilder)
            where TEntity : class
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntity, TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return new SqlServerEntityTypeBuilder(entityTypeBuilder.Metadata);
        }

        public static TEntityTypeBuilder ForSqlServer<TEntity, TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntity, TEntityTypeBuilder> entityTypeBuilder,
            [NotNull] Action<SqlServerEntityTypeBuilder> relationalEntityTypeBuilder)
            where TEntity : class
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntity, TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            relationalEntityTypeBuilder(ForSqlServer(entityTypeBuilder));

            return (TEntityTypeBuilder)entityTypeBuilder;
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
            [NotNull] this IReferenceCollectionBuilder<TOneToManyBuilder> referenceCollectionBuilder)
            where TOneToManyBuilder : IReferenceCollectionBuilder<TOneToManyBuilder>
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));

            return new SqlServerForeignKeyBuilder(referenceCollectionBuilder.Metadata);
        }

        public static TOneToManyBuilder ForSqlServer<TOneToManyBuilder>(
            [NotNull] this IReferenceCollectionBuilder<TOneToManyBuilder> referenceCollectionBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalOneToManyBuilder)
            where TOneToManyBuilder : IReferenceCollectionBuilder<TOneToManyBuilder>
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NotNull(relationalOneToManyBuilder, nameof(relationalOneToManyBuilder));

            relationalOneToManyBuilder(ForSqlServer(referenceCollectionBuilder));

            return (TOneToManyBuilder)referenceCollectionBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TManyToOneBuilder>(
            [NotNull] this ICollectionReferenceBuilder<TManyToOneBuilder> collectionReferenceBuilder)
            where TManyToOneBuilder : ICollectionReferenceBuilder<TManyToOneBuilder>
        {
            Check.NotNull(collectionReferenceBuilder, nameof(collectionReferenceBuilder));

            return new SqlServerForeignKeyBuilder(collectionReferenceBuilder.Metadata);
        }

        public static TManyToOneBuilder ForSqlServer<TManyToOneBuilder>(
            [NotNull] this ICollectionReferenceBuilder<TManyToOneBuilder> collectionReferenceBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalManyToOneBuilder)
            where TManyToOneBuilder : ICollectionReferenceBuilder<TManyToOneBuilder>
        {
            Check.NotNull(collectionReferenceBuilder, nameof(collectionReferenceBuilder));
            Check.NotNull(relationalManyToOneBuilder, nameof(relationalManyToOneBuilder));

            relationalManyToOneBuilder(ForSqlServer(collectionReferenceBuilder));

            return (TManyToOneBuilder)collectionReferenceBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer<TOneToOneBuilder>(
            [NotNull] this IReferenceReferenceBuilder<TOneToOneBuilder> referenceReferenceBuilder)
            where TOneToOneBuilder : IReferenceReferenceBuilder<TOneToOneBuilder>
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));

            return new SqlServerForeignKeyBuilder(referenceReferenceBuilder.Metadata);
        }

        public static TOneToOneBuilder ForSqlServer<TOneToOneBuilder>(
            [NotNull] this IReferenceReferenceBuilder<TOneToOneBuilder> referenceReferenceBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> relationalOneToOneBuilder)
            where TOneToOneBuilder : IReferenceReferenceBuilder<TOneToOneBuilder>
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NotNull(relationalOneToOneBuilder, nameof(relationalOneToOneBuilder));

            relationalOneToOneBuilder(ForSqlServer(referenceReferenceBuilder));

            return (TOneToOneBuilder)referenceReferenceBuilder;
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
