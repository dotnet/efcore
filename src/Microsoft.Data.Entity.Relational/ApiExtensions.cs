// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public static class MetadataExtensions
    {
        public static class Annotations
        {
            public const string StorageTypeName = "StorageTypeName";
            public const string ColumnDefaultValue = "ColumnDefaultValue";
            public const string ColumnDefaultSql = "ColumnDefaultSql";
            public const string IsClustered = "IsClustered";
            public const string CascadeDelete = "CascadeDelete";
        }

        public static ModelBuilder.EntityBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this ModelBuilder.EntityBuilder<TEntity> entityBuilder,
            SchemaQualifiedName tableName)
            where TEntity : class
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder.StorageName(tableName);

            return entityBuilder;
        }

        public static ModelBuilder.EntityBuilder<TEntity>.PropertiesBuilder.PropertyBuilder ColumnName<TEntity>(
            [NotNull] this ModelBuilder.EntityBuilder<TEntity>.PropertiesBuilder.PropertyBuilder propertyBuilder,
            [NotNull] string columnName)
            where TEntity : class
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.StorageName(columnName);

            return propertyBuilder;
        }

        public static ModelBuilder.EntityBuilder<TEntity>.PropertiesBuilder.PropertyBuilder ColumnType<TEntity>(
            [NotNull] this ModelBuilder.EntityBuilder<TEntity>.PropertiesBuilder.PropertyBuilder propertyBuilder,
            [NotNull] string typeName)
            where TEntity : class
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.Annotation(Annotations.StorageTypeName, typeName);

            return propertyBuilder;
        }

        public static ModelBuilder.EntityBuilder<TEntity>.PropertiesBuilder.PropertyBuilder ColumnDefaultSql<TEntity>(
            [NotNull] this ModelBuilder.EntityBuilder<TEntity>.PropertiesBuilder.PropertyBuilder propertyBuilder,
            [NotNull] string columnDefaultSql)
            where TEntity : class
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.Annotation(Annotations.ColumnDefaultSql, columnDefaultSql);

            return propertyBuilder;
        }

        public static ModelBuilder.EntityBuilder<TEntity>.ForeignKeysBuilder.ForeignKeyBuilder CascadeDelete<TEntity>(
            [NotNull] this ModelBuilder.EntityBuilder<TEntity>.ForeignKeysBuilder.ForeignKeyBuilder foreignKeyBuilder,
            bool cascadeDelete)
            where TEntity : class
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            foreignKeyBuilder.Annotation(Annotations.CascadeDelete, cascadeDelete.ToString());

            return foreignKeyBuilder;
        }

        public static string ColumnType([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            return property[Annotations.StorageTypeName];
        }

        public static object ColumnDefaultValue([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            return property[Annotations.ColumnDefaultValue];
        }

        public static string ColumnDefaultSql([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            return property[Annotations.ColumnDefaultSql];
        }

        public static bool IsClustered([NotNull] this IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            var isClusteredString = primaryKey[Annotations.IsClustered];

            bool isClustered;
            if (isClusteredString == null
                || !bool.TryParse(isClusteredString, out isClustered))
            {
                isClustered = true;
            }

            return isClustered;
        }

        public static bool CascadeDelete([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var cascadeDeleteString = foreignKey[Annotations.CascadeDelete];

            bool cascadeDelete;
            if (cascadeDeleteString == null
                || !bool.TryParse(cascadeDeleteString, out cascadeDelete))
            {
                cascadeDelete = false;
            }

            return cascadeDelete;
        }

        public static IEnumerable<Column> GetStoreGeneratedColumns([NotNull] this Table table)
        {
            Check.NotNull(table, "table");

            return table.Columns.Where(
                c => c.ValueGenerationStrategy == StoreValueGenerationStrategy.Identity ||
                     c.ValueGenerationStrategy == StoreValueGenerationStrategy.Computed);
        }
    }
}
