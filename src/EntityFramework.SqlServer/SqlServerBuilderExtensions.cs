// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerBuilderExtensions
    {
        public static SqlServerPropertyBuilder ForSqlServer(
            [NotNull] this PropertyBuilder propertyBuilder)
            => new SqlServerPropertyBuilder(Check.NotNull(propertyBuilder, nameof(propertyBuilder)).Metadata);

        public static PropertyBuilder ForSqlServer(
            [NotNull] this PropertyBuilder propertyBuilder,
            [NotNull] Action<SqlServerPropertyBuilder> builderAction)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(propertyBuilder));

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ForSqlServer<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [NotNull] Action<SqlServerPropertyBuilder> builderAction)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(propertyBuilder));

            return propertyBuilder;
        }

        public static SqlServerEntityTypeBuilder ForSqlServer(
            [NotNull] this EntityTypeBuilder entityTypeBuilder)
            => new SqlServerEntityTypeBuilder(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)).Metadata);

        public static EntityTypeBuilder ForSqlServer(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] Action<SqlServerEntityTypeBuilder> builderAction)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            builderAction(ForSqlServer(entityTypeBuilder));

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ForSqlServer<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Action<SqlServerEntityTypeBuilder> builderAction)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            builderAction(ForSqlServer(entityTypeBuilder));

            return entityTypeBuilder;
        }

        public static SqlServerKeyBuilder ForSqlServer(
            [NotNull] this KeyBuilder keyBuilder)
            => new SqlServerKeyBuilder(Check.NotNull(keyBuilder, nameof(keyBuilder)).Metadata);

        public static KeyBuilder ForSqlServer(
            [NotNull] this KeyBuilder keyBuilder,
            [NotNull] Action<SqlServerKeyBuilder> builderAction)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(keyBuilder));

            return keyBuilder;
        }

        public static SqlServerIndexBuilder ForSqlServer(
            [NotNull] this IndexBuilder indexBuilder)
            => new SqlServerIndexBuilder(Check.NotNull(indexBuilder, nameof(indexBuilder)).Metadata);

        public static IndexBuilder ForSqlServer(
            [NotNull] this IndexBuilder indexBuilder,
            [NotNull] Action<SqlServerIndexBuilder> builderAction)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(indexBuilder));

            return indexBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder)
            => new SqlServerForeignKeyBuilder(
                Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder)).Metadata);

        public static ReferenceCollectionBuilder ForSqlServer(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> builderAction)
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(referenceCollectionBuilder));

            return referenceCollectionBuilder;
        }

        public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> ForSqlServer<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> builderAction)
            where TEntity : class
            where TRelatedEntity : class
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(referenceCollectionBuilder));

            return referenceCollectionBuilder;
        }

        public static SqlServerForeignKeyBuilder ForSqlServer(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder)
            => new SqlServerForeignKeyBuilder(
                Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder)).Metadata);

        public static ReferenceReferenceBuilder ForSqlServer(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> builderAction)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(referenceReferenceBuilder));

            return referenceReferenceBuilder;
        }

        public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> ForSqlServer<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder,
            [NotNull] Action<SqlServerForeignKeyBuilder> builderAction)
            where TEntity : class
            where TRelatedEntity : class
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(referenceReferenceBuilder));

            return referenceReferenceBuilder;
        }

        public static SqlServerModelBuilder ForSqlServer(
            [NotNull] this ModelBuilder modelBuilder)
            => new SqlServerModelBuilder(Check.NotNull(modelBuilder, nameof(modelBuilder)).Model);

        public static ModelBuilder ForSqlServer(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Action<SqlServerModelBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForSqlServer(modelBuilder));

            return modelBuilder;
        }
    }
}
