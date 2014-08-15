// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.AzureTableStorage.Metadata
{
    public static class MetadataExtensions
    {
        public static class Annotations
        {
            public const string TableName = "TableName";
            public const string ColumnName = "ColumnName";
        }

        public static void ToTable([NotNull] this EntityType entityType, [NotNull] string tableName)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(tableName, "tableName");

            entityType[Annotations.TableName] = tableName;
        }

        public static void SetColumnName([NotNull] this Property property, [NotNull] string columnName)
        {
            Check.NotNull(property, "property");
            Check.NotEmpty(columnName, "columnName");

            property[Annotations.ColumnName] = columnName;
        }

        public static string TableName([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType[Annotations.TableName] ?? entityType.Name;
        }

        public static string ColumnName([NotNull] this IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return property[Annotations.ColumnName] ?? property.Name;
        }

        public static BasicModelBuilder.EntityBuilderBase<TMetadataBuilder> TableName<TMetadataBuilder>(
            [NotNull] this BasicModelBuilder.EntityBuilderBase<TMetadataBuilder> builder,
            [NotNull] string tableName)
            where TMetadataBuilder : BasicModelBuilder.MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(tableName, "tableName");

            builder.Annotation(Annotations.TableName, tableName);

            return builder;
        }

        public static BasicModelBuilder.EntityBuilderBase<TMetadataBuilder>.PropertyBuilder ColumnName<TMetadataBuilder>(
            [NotNull] this BasicModelBuilder.EntityBuilderBase<TMetadataBuilder>.PropertyBuilder builder,
            [NotNull] string columnName)
            where TMetadataBuilder : BasicModelBuilder.MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(columnName, "columnName");

            builder.Annotation(Annotations.ColumnName, columnName);

            return builder;
        }

        /// <summary>
        ///     Sets the partition key, row key, and creates a composite entity key
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="builder"></param>
        /// <param name="partitionKeyExpression"></param>
        /// <param name="rowKeyExpression"></param>
        /// <returns></returns>
        public static BasicModelBuilder.EntityBuilder<TEntity> PartitionAndRowKey<TEntity>([NotNull] this BasicModelBuilder.EntityBuilder<TEntity> builder, [NotNull] Expression<Func<TEntity, object>> partitionKeyExpression, [NotNull] Expression<Func<TEntity, object>> rowKeyExpression)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(partitionKeyExpression, "partitionKeyExpression");
            Check.NotNull(rowKeyExpression, "rowKeyExpression");

            builder.Property(partitionKeyExpression).ColumnName("PartitionKey");
            builder.Property(rowKeyExpression).ColumnName("RowKey");

            var properties = partitionKeyExpression.GetPropertyAccessList().ToList();
            properties.Add(rowKeyExpression.GetPropertyAccess());
            builder.Key(properties.Select(s => s.Name).ToArray());
            return builder;
        }

        public static BasicModelBuilder.EntityBuilder<TEntity> Timestamp<TEntity>([NotNull] this BasicModelBuilder.EntityBuilder<TEntity> builder, [NotNull] Expression<Func<TEntity, object>> expression)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(expression, "expression");

            builder.Property(expression).ColumnName("Timestamp");

            return builder;
        }

        public static BasicModelBuilder.EntityBuilder<TEntity> Timestamp<TEntity>([NotNull] this BasicModelBuilder.EntityBuilder<TEntity> builder, [NotNull] string name, bool shadowProperty = false)
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(name, "name");

            builder.Property<TEntity>(name, shadowProperty).ColumnName("Timestamp");

            return builder;
        }
    }
}
