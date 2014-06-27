// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity.Metadata
{
    public static class EntityBuilderExtensions
    {
        /// <summary>
        ///     Sets the partition key, row key, and creates a composite entity key
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="builder"></param>
        /// <param name="partitionKeyExpression"></param>
        /// <param name="rowKeyExpression"></param>
        /// <returns></returns>
        public static ModelBuilder.EntityBuilder<TEntity> PartitionAndRowKey<TEntity>([NotNull] this ModelBuilder.EntityBuilder<TEntity> builder, [NotNull] Expression<Func<TEntity, object>> partitionKeyExpression, [NotNull] Expression<Func<TEntity, object>> rowKeyExpression)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(partitionKeyExpression, "partitionKeyExpression");
            Check.NotNull(rowKeyExpression, "rowKeyExpression");
            builder.Properties(pb =>
                {
                    pb.Property(partitionKeyExpression).StorageName("PartitionKey");
                    pb.Property(rowKeyExpression).StorageName("RowKey");
                });

            var properties = partitionKeyExpression.GetPropertyAccessList().ToList();
            properties.Add(rowKeyExpression.GetPropertyAccess());
            builder.Key(properties.Select(s => s.Name).ToArray());
            return builder;
        }

        public static ModelBuilder.EntityBuilder<TEntity> Timestamp<TEntity>([NotNull] this ModelBuilder.EntityBuilder<TEntity> builder, [NotNull] Expression<Func<TEntity, object>> expression)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(expression, "expression");
            return builder.Properties(pb => pb.Property(expression).StorageName("Timestamp"));
        }

        public static ModelBuilder.EntityBuilder<TEntity> Timestamp<TEntity>([NotNull] this ModelBuilder.EntityBuilder<TEntity> builder, [NotNull] string name, bool shadowProperty = false)
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(name, "name");
            return builder.Properties(pb => pb.Property<TEntity>(name, shadowProperty).StorageName("Timestamp"));
        }
    }
}
