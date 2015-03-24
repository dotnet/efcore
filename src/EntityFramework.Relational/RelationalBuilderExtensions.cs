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
            [NotNull] this ReferenceCollectionBuilder foreignKeyBuilder)
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static ReferenceCollectionBuilder ForRelational(
            [NotNull] this ReferenceCollectionBuilder foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(foreignKeyBuilder));

            return foreignKeyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational(
            [NotNull] this CollectionReferenceBuilder foreignKeyBuilder)
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static CollectionReferenceBuilder ForRelational(
            [NotNull] this CollectionReferenceBuilder foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(foreignKeyBuilder));

            return foreignKeyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational(
            [NotNull] this ReferenceReferenceBuilder foreignKeyBuilder)
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static ReferenceReferenceBuilder ForRelational(
            [NotNull] this ReferenceReferenceBuilder foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> builderAction)
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(builderAction, nameof(builderAction));

            builderAction(ForRelational(foreignKeyBuilder));

            return foreignKeyBuilder;
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
