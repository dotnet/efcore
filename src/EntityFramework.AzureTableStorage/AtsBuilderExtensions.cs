// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Metadata;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class AtsBuilderExtensions
    {
        public static AtsPropertyBuilder ForAzureTableStorage<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            return new AtsPropertyBuilder(propertyBuilder.Metadata);
        }

        public static TPropertyBuilder ForAzureTableStorage<TPropertyBuilder>(
            [NotNull] this IPropertyBuilder<TPropertyBuilder> propertyBuilder,
            [NotNull] Action<AtsPropertyBuilder> relationalPropertyBuilder)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(relationalPropertyBuilder, "relationalPropertyBuilder");

            relationalPropertyBuilder(ForAzureTableStorage(propertyBuilder));

            return (TPropertyBuilder)propertyBuilder;
        }

        public static AtsEntityBuilder ForAzureTableStorage<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            return new AtsEntityBuilder(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForAzureTableStorage<TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntityBuilder> entityBuilder,
            [NotNull] Action<AtsEntityBuilder> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            relationalEntityBuilder(ForAzureTableStorage(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }

        public static AtsEntityBuilder<TEntity> ForAzureTableStorage<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            return new AtsEntityBuilder<TEntity>(entityBuilder.Metadata);
        }

        public static TEntityBuilder ForAzureTableStorage<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> entityBuilder,
            [NotNull] Action<AtsEntityBuilder<TEntity>> relationalEntityBuilder)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            relationalEntityBuilder(ForAzureTableStorage(entityBuilder));

            return (TEntityBuilder)entityBuilder;
        }
    }
}
