// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

            return entityType[Annotations.TableName] ?? entityType.SimpleName;
        }

        public static string ColumnName([NotNull] this IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return property[Annotations.ColumnName] ?? property.Name;
        }

        public static TEntityBuilder TableName<TEntityBuilder>(
            [NotNull] this TEntityBuilder builder,
            [NotNull] string tableName)
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(tableName, "tableName");

            builder.Annotation(Annotations.TableName, tableName);

            return builder;
        }

        public static TPropertyBuilder ColumnName<TPropertyBuilder>(
            [NotNull] this TPropertyBuilder builder,
            [NotNull] string columnName)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
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
        /// <typeparam name="TEntityBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="partitionKeyExpression"></param>
        /// <param name="rowKeyExpression"></param>
        /// <returns></returns>
        public static TEntityBuilder PartitionAndRowKey<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> builder,
            [NotNull] Expression<Func<TEntity, object>> partitionKeyExpression,
            [NotNull] Expression<Func<TEntity, object>> rowKeyExpression)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(partitionKeyExpression, "partitionKeyExpression");
            Check.NotNull(rowKeyExpression, "rowKeyExpression");

            var entityType = builder.Metadata;

            var partitionKeyInfo = partitionKeyExpression.GetPropertyAccess();
            var rowKeyInfo = rowKeyExpression.GetPropertyAccess();

            var partitionKey = entityType.TryGetProperty(partitionKeyInfo.Name) 
                ?? entityType.AddProperty(partitionKeyInfo);
            
            var rowKey = entityType.TryGetProperty(rowKeyInfo.Name) 
                ?? entityType.AddProperty(rowKeyInfo);

            partitionKey.SetColumnName("PartitionKey");
            rowKey.SetColumnName("RowKey");

            entityType.SetKey(new[] { partitionKey, rowKey });

            return (TEntityBuilder)builder;
        }

        public static TEntityBuilder Timestamp<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> builder, 
            [NotNull] Expression<Func<TEntity, object>> expression)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(expression, "expression");

            var entityType = builder.Metadata;

            var propertyInfo = expression.GetPropertyAccess();
            (entityType.TryGetProperty(propertyInfo.Name) 
                ?? entityType.AddProperty(propertyInfo)).SetColumnName("Timestamp");

            return (TEntityBuilder)builder;
        }

        public static TEntityBuilder Timestamp<TEntity, TEntityBuilder>(
            [NotNull] this IEntityBuilder<TEntity, TEntityBuilder> builder, 
            [NotNull] string name,
            bool shadowProperty = false)
            where TEntityBuilder : IEntityBuilder<TEntity, TEntityBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(name, "name");

            var entityType = builder.Metadata;

            // TODO: Consider forcing property to shadow state if not cuurently in shadow state
            (entityType.TryGetProperty(name) 
                ?? entityType.AddProperty(name, typeof(DateTimeOffset), shadowProperty, concurrencyToken: false)).SetColumnName("Timestamp");

            return (TEntityBuilder)builder;
        }
    }
}
