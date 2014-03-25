// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;
using System;

namespace Microsoft.Data.Relational
{
    public static class ApiExtensions
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
            [NotNull] bool cascadeDelete)
            where TEntity : class
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            foreignKeyBuilder.Annotation(Annotations.CascadeDelete, cascadeDelete.ToString());

            return foreignKeyBuilder;
        }

        public static string ColumnType([NotNull] this IProperty property)
        {
            return property[ApiExtensions.Annotations.StorageTypeName];
        }

        public static object ColumnDefaultValue([NotNull] this IProperty property)
        {
            return property[ApiExtensions.Annotations.ColumnDefaultValue];
        }

        public static string ColumnDefaultSql([NotNull] this IProperty property)
        {
            return property[ApiExtensions.Annotations.ColumnDefaultSql];
        }

        public static bool IsClustered([NotNull] this IKey primaryKey)
        {
            var isClusteredString = primaryKey[ApiExtensions.Annotations.IsClustered];

            bool isClustered;
            if (isClusteredString == null
                || !bool.TryParse(isClusteredString, out isClustered))
            {
                isClustered = false;
            }

            return isClustered;
        }

        public static bool CascadeDelete([NotNull] this IForeignKey foreignKey)
        {
            var cascadeDeleteString = foreignKey[ApiExtensions.Annotations.CascadeDelete];

            bool cascadeDelete;
            if (cascadeDeleteString == null
                || !bool.TryParse(cascadeDeleteString, out cascadeDelete))
            {
                cascadeDelete = false;
            }

            return cascadeDelete;
        }
    }
}
