// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
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
            Check.NotNull(propertyBuilder, "propertyBuilder");

            return new RelationalPropertyBuilder(propertyBuilder.Metadata);
        }

        public static TPropertyBuilder ForRelational<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder,
            [NotNull] Action<RelationalPropertyBuilder> relationalPropertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(relationalPropertyBuilder, "relationalPropertyBuilder");

            relationalPropertyBuilder(ForRelational(propertyBuilder));

            return (TPropertyBuilder)propertyBuilder;
        }

        public static RelationalEntityBuilder ForRelational<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            return new RelationalEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForRelational<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder,
            [NotNull] Action<RelationalEntityBuilder> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            relationalEntityBuilder(ForRelational(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static RelationalEntityBuilder ForRelational<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            return new RelationalEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForRelational<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder,
            [NotNull] Action<RelationalEntityBuilder> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            relationalEntityBuilder(ForRelational(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static RelationalKeyBuilder ForRelational<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, "keyBuilder");

            return new RelationalKeyBuilder(keyBuilder.Metadata);
        }

        public static TKeyBuilder ForRelational<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> keyBuilder,
            [NotNull] Action<RelationalKeyBuilder> relationalKeyBuilder)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(keyBuilder, "keyBuilder");
            Check.NotNull(relationalKeyBuilder, "relationalKeyBuilder");

            relationalKeyBuilder(ForRelational(keyBuilder));

            return (TKeyBuilder)keyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TForeignKeyBuilder ForRelational<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalForeignKeyBuilder)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalForeignKeyBuilder, "relationalForeignKeyBuilder");

            relationalForeignKeyBuilder(ForRelational(foreignKeyBuilder));

            return (TForeignKeyBuilder)foreignKeyBuilder;
        }

        public static RelationalIndexBuilder ForRelational<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, "indexBuilder");

            return new RelationalIndexBuilder(indexBuilder.Metadata);
        }

        public static TIndexBuilder ForRelational<TIndexBuilder>(
            [NotNull] this IIndexBuilder<TIndexBuilder> indexBuilder,
            [NotNull] Action<RelationalIndexBuilder> relationalIndexBuilder)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexBuilder, "indexBuilder");
            Check.NotNull(relationalIndexBuilder, "relationalIndexBuilder");

            relationalIndexBuilder(ForRelational(indexBuilder));

            return (TIndexBuilder)indexBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TOneToManyBuilder>(
            [NotNull] this IOneToManyBuilder<TOneToManyBuilder> foreignKeyBuilder)
            where TOneToManyBuilder : IOneToManyBuilder<TOneToManyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TOneToManyBuilder ForRelational<TOneToManyBuilder>(
            [NotNull] this IOneToManyBuilder<TOneToManyBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalOneToManyBuilder)
            where TOneToManyBuilder : IOneToManyBuilder<TOneToManyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalOneToManyBuilder, "relationalOneToManyBuilder");

            relationalOneToManyBuilder(ForRelational(foreignKeyBuilder));

            return (TOneToManyBuilder)foreignKeyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TManyToOneBuilder>(
            [NotNull] this IManyToOneBuilder<TManyToOneBuilder> foreignKeyBuilder)
            where TManyToOneBuilder : IManyToOneBuilder<TManyToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TManyToOneBuilder ForRelational<TManyToOneBuilder>(
            [NotNull] this IManyToOneBuilder<TManyToOneBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalManyToOneBuilder)
            where TManyToOneBuilder : IManyToOneBuilder<TManyToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalManyToOneBuilder, "relationalManyToOneBuilder");

            relationalManyToOneBuilder(ForRelational(foreignKeyBuilder));

            return (TManyToOneBuilder)foreignKeyBuilder;
        }

        public static RelationalForeignKeyBuilder ForRelational<TOneToOneBuilder>(
            [NotNull] this IOneToOneBuilder<TOneToOneBuilder> foreignKeyBuilder)
            where TOneToOneBuilder : IOneToOneBuilder<TOneToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            return new RelationalForeignKeyBuilder(foreignKeyBuilder.Metadata);
        }

        public static TOneToOneBuilder ForRelational<TOneToOneBuilder>(
            [NotNull] this IOneToOneBuilder<TOneToOneBuilder> foreignKeyBuilder,
            [NotNull] Action<RelationalForeignKeyBuilder> relationalOneToOneBuilder)
            where TOneToOneBuilder : IOneToOneBuilder<TOneToOneBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");
            Check.NotNull(relationalOneToOneBuilder, "relationalOneToOneBuilder");

            relationalOneToOneBuilder(ForRelational(foreignKeyBuilder));

            return (TOneToOneBuilder)foreignKeyBuilder;
        }

        public static RelationalModelBuilder ForRelational<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, "modelBuilder");

            return new RelationalModelBuilder(modelBuilder.Metadata);
        }

        public static TModelBuilder ForRelational<TModelBuilder>(
            [NotNull] this IModelBuilder<TModelBuilder> modelBuilder,
            [NotNull] Action<RelationalModelBuilder> sqlServerModelBuilder)
            where TModelBuilder : IModelBuilder<TModelBuilder>
        {
            Check.NotNull(modelBuilder, "modelBuilder");
            Check.NotNull(sqlServerModelBuilder, "sqlServerModelBuilder");

            sqlServerModelBuilder(ForRelational(modelBuilder));

            return (TModelBuilder)modelBuilder;
        }
    }
}
