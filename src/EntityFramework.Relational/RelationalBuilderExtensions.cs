// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalBuilderExtensions
    {
        public static RelationalPropertyBuilder ForRelational<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            return new RelationalPropertyBuilder(propertyBuilder.Metadata);
        }

        public static TPropertyBuilder ForRelational<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder,
            [NotNull] Action<RelationalPropertyBuilder> relationalPropertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(relationalPropertyBuilder, nameof(relationalPropertyBuilder));

            relationalPropertyBuilder(ForRelational(propertyBuilder));

            return (TPropertyBuilder)propertyBuilder;
        }

        public static RelationalEntityTypeBuilder ForRelational<TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntityTypeBuilder> entityTypeBuilder)
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return new RelationalEntityTypeBuilder(entityTypeBuilder.Metadata);
        }

        public static TEntityTypeBuilder ForRelational<TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntityTypeBuilder> entityTypeBuilder,
            [NotNull] Action<RelationalEntityTypeBuilder> relationalEntityTypeBuilder)
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            relationalEntityTypeBuilder(ForRelational(entityTypeBuilder));

            return (TEntityTypeBuilder)entityTypeBuilder;
        }

        public static RelationalEntityTypeBuilder ForRelational<TEntity, TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntity, TEntityTypeBuilder> entityTypeBuilder)
            where TEntity : class
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntity, TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return new RelationalEntityTypeBuilder(entityTypeBuilder.Metadata);
        }

        public static TEntityTypeBuilder ForRelational<TEntity, TEntityTypeBuilder>(
            [NotNull] this IEntityTypeBuilder<TEntity, TEntityTypeBuilder> entityTypeBuilder,
            [NotNull] Action<RelationalEntityTypeBuilder> relationalEntityTypeBuilder)
            where TEntity : class
            where TEntityTypeBuilder : IEntityTypeBuilder<TEntity, TEntityTypeBuilder>
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            relationalEntityTypeBuilder(ForRelational(entityTypeBuilder));

            return (TEntityTypeBuilder)entityTypeBuilder;
        }

        public static RelationalKeyBuilder ForRelational<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            return new RelationalKeyBuilder(keyBuilder.Metadata);
        }

        public static TKeyBuilder ForRelational<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder,
            [NotNull] Action<RelationalKeyBuilder> relationalKeyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NotNull(relationalKeyBuilder, nameof(relationalKeyBuilder));

            relationalKeyBuilder(ForRelational(keyBuilder));

            return (TKeyBuilder)keyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TForeignKeyBuilder ForRelational<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalForeignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(relationalForeignKeyBuilder, nameof(relationalForeignKeyBuilder));

            relationalForeignKeyBuilder(ForRelational(foreignKeyBuilder));

            return (TForeignKeyBuilder)foreignKeyBuilder;
        }

        public static RelationalIndexBuilder ForRelational<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            return new RelationalIndexBuilder(indexBuilder.Metadata);
        }

        public static TIndexBuilder ForRelational<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder,
            [NotNull] Action<RelationalIndexBuilder> relationalIndexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));
            Check.NotNull(relationalIndexBuilder, nameof(relationalIndexBuilder));

            relationalIndexBuilder(ForRelational(indexBuilder));

            return (TIndexBuilder)indexBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TOneToManyBuilder>(
            [NotNull] this IReferenceCollectionBuilder<TOneToManyBuilder> foreignKeyBuilder)
            where TOneToManyBuilder : IReferenceCollectionBuilder<TOneToManyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TOneToManyBuilder ForRelational<TOneToManyBuilder>(
            [NotNull] this IReferenceCollectionBuilder<TOneToManyBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalOneToManyBuilder)
            where TOneToManyBuilder : IReferenceCollectionBuilder<TOneToManyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(relationalOneToManyBuilder, nameof(relationalOneToManyBuilder));

            relationalOneToManyBuilder(ForRelational(foreignKeyBuilder));

            return (TOneToManyBuilder)foreignKeyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TManyToOneBuilder>(
            [NotNull] this ICollectionReferenceBuilder<TManyToOneBuilder> foreignKeyBuilder)
            where TManyToOneBuilder : ICollectionReferenceBuilder<TManyToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TManyToOneBuilder ForRelational<TManyToOneBuilder>(
            [NotNull] this ICollectionReferenceBuilder<TManyToOneBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalManyToOneBuilder)
            where TManyToOneBuilder : ICollectionReferenceBuilder<TManyToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(relationalManyToOneBuilder, nameof(relationalManyToOneBuilder));

            relationalManyToOneBuilder(ForRelational(foreignKeyBuilder));

            return (TManyToOneBuilder)foreignKeyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TOneToOneBuilder>(
            [NotNull] this IReferenceReferenceBuilder<TOneToOneBuilder> foreignKeyBuilder)
            where TOneToOneBuilder : IReferenceReferenceBuilder<TOneToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TOneToOneBuilder ForRelational<TOneToOneBuilder>(
            [NotNull] this IReferenceReferenceBuilder<TOneToOneBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalOneToOneBuilder)
            where TOneToOneBuilder : IReferenceReferenceBuilder<TOneToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, nameof(foreignKeyBuilder));
            Check.NotNull(relationalOneToOneBuilder, nameof(relationalOneToOneBuilder));

            relationalOneToOneBuilder(ForRelational(foreignKeyBuilder));

            return (TOneToOneBuilder)foreignKeyBuilder;
        }

        public static RelationalModelBuilder ForRelational<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            return new RelationalModelBuilder(modelBuilder.Metadata);
        }

        public static TModelBuilder ForRelational<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder,
            [NotNull] Action<RelationalModelBuilder> sqlServerModelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(sqlServerModelBuilder, nameof(sqlServerModelBuilder));

            sqlServerModelBuilder(ForRelational(modelBuilder));

            return (TModelBuilder)modelBuilder;
        }
    }
}
