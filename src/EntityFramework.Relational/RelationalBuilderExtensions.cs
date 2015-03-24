// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalBuilderExtensions
    {
        public static RelationalPropertyBuilder ForRelational(
            [NotNull] this PropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return new RelationalPropertyBuilder(propertyBuilder.Metadata);
        }

        public static PropertyBuilder ForRelational(
            [NotNull] this PropertyBuilder propertyBuilder,
            [NotNull] Action<RelationalPropertyBuilder> builderAction)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(propertyBuilder));

            return propertyBuilder;
        }

        public static PropertyBuilder<TProperty> ForRelational<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [NotNull] Action<RelationalPropertyBuilder> builderAction)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(propertyBuilder));

            return propertyBuilder;
        }

        public static RelationalEntityTypeBuilder ForRelational(
            [NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return new RelationalEntityTypeBuilder(entityTypeBuilder.Metadata);
        }

        public static EntityTypeBuilder ForRelational(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] Action<RelationalEntityTypeBuilder> builderAction)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            builderAction(ForRelational(entityTypeBuilder));

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ForRelational<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Action<RelationalEntityTypeBuilder> builderAction)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            builderAction(ForRelational(entityTypeBuilder));

            return entityTypeBuilder;
        }

        public static RelationalKeyBuilder ForRelational(
            [NotNull] this KeyBuilder keyBuilder)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            return new RelationalKeyBuilder(keyBuilder.Metadata);
        }

        public static KeyBuilder ForRelational(
            [NotNull] this KeyBuilder keyBuilder,
            [NotNull] Action<RelationalKeyBuilder> builderAction)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(keyBuilder));

            return keyBuilder;
        }

        public static RelationalIndexBuilder ForRelational(
            [NotNull] this IndexBuilder indexBuilder)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return new RelationalIndexBuilder(indexBuilder.Metadata);
        }

        public static IndexBuilder ForRelational(
            [NotNull] this IndexBuilder indexBuilder,
            [NotNull] Action<RelationalIndexBuilder> builderAction)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(indexBuilder));

            return indexBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder)
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));

            return new RelationalForeignKeyBuilder(referenceCollectionBuilder.Metadata);
        }

        public static ReferenceCollectionBuilder ForRelational(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(referenceCollectionBuilder));

            return referenceCollectionBuilder;
        }

        public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> ForRelational<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
            where TEntity : class
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(referenceCollectionBuilder));

            return referenceCollectionBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational(
            [NotNull] this CollectionReferenceBuilder collectionReferenceBuilder)
        {
            Check.NotNull(collectionReferenceBuilder, nameof(collectionReferenceBuilder));

            return new RelationalForeignKeyBuilder(collectionReferenceBuilder.Metadata);
        }

        public static CollectionReferenceBuilder ForRelational(
            [NotNull] this CollectionReferenceBuilder collectionReferenceBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(collectionReferenceBuilder, nameof(collectionReferenceBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(collectionReferenceBuilder));

            return collectionReferenceBuilder;
        }

        public static CollectionReferenceBuilder<TEntity, TRelatedEntity> ForRelational<TEntity, TRelatedEntity>(
            [NotNull] this CollectionReferenceBuilder<TEntity, TRelatedEntity> collectionReferenceBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
            where TEntity : class
        {
            Check.NotNull(collectionReferenceBuilder, nameof(collectionReferenceBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(collectionReferenceBuilder));

            return collectionReferenceBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));

            return new RelationalForeignKeyBuilder(referenceReferenceBuilder.Metadata);
        }

        public static ReferenceReferenceBuilder ForRelational(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(referenceReferenceBuilder));

            return referenceReferenceBuilder;
        }

        public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> ForRelational<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(referenceReferenceBuilder));

            return referenceReferenceBuilder;
        }

        public static RelationalModelBuilder ForRelational(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            return new RelationalModelBuilder(modelBuilder.Model);
        }

        public static ModelBuilder ForRelational(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] Action<RelationalModelBuilder> builderAction)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(modelBuilder));

            return modelBuilder;
        }
    }
}
