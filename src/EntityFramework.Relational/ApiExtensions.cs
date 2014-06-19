// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Utilities;

// ReSharper disable once CheckNamespace

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

        public static TEntityBuilder ToTable<TEntityBuilder>(
            [NotNull] this TEntityBuilder entityBuilder,
            SchemaQualifiedName tableName)
            where TEntityBuilder : ModelBuilder.EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder.StorageName(tableName);

            return entityBuilder;
        }

        public static ModelBuilder.EntityBuilderBase<TEntityBuilder>.PropertiesBuilder.PropertyBuilder ColumnName<TEntityBuilder>(
            [NotNull] this ModelBuilder.EntityBuilderBase<TEntityBuilder>.PropertiesBuilder.PropertyBuilder propertyBuilder,
            [NotNull] string columnName)
            where TEntityBuilder : ModelBuilder.EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.StorageName(columnName);

            return propertyBuilder;
        }

        public static ModelBuilder.EntityBuilderBase<TEntityBuilder>.PropertiesBuilder.PropertyBuilder ColumnType<TEntityBuilder>(
            [NotNull] this ModelBuilder.EntityBuilderBase<TEntityBuilder>.PropertiesBuilder.PropertyBuilder propertyBuilder,
            [NotNull] string typeName)
            where TEntityBuilder : ModelBuilder.EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.Annotation(Annotations.StorageTypeName, typeName);

            return propertyBuilder;
        }

        public static ModelBuilder.EntityBuilderBase<TEntityBuilder>.PropertiesBuilder.PropertyBuilder ColumnDefaultSql<TEntityBuilder>(
            [NotNull] this ModelBuilder.EntityBuilderBase<TEntityBuilder>.PropertiesBuilder.PropertyBuilder propertyBuilder,
            [NotNull] string columnDefaultSql)
            where TEntityBuilder : ModelBuilder.EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");

            propertyBuilder.Annotation(Annotations.ColumnDefaultSql, columnDefaultSql);

            return propertyBuilder;
        }

        public static ModelBuilder.EntityBuilderBase<TEntityBuilder>.ForeignKeysBuilder.ForeignKeyBuilder CascadeDelete<TEntityBuilder>(
            [NotNull] this ModelBuilder.EntityBuilderBase<TEntityBuilder>.ForeignKeysBuilder.ForeignKeyBuilder foreignKeyBuilder,
            bool cascadeDelete)
            where TEntityBuilder : ModelBuilder.EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            foreignKeyBuilder.Annotation(Annotations.CascadeDelete, cascadeDelete.ToString());

            return foreignKeyBuilder;
        }

        public static ModelBuilder.EntityBuilderBase<TEntityBuilder>.IndexesBuilder.IndexBuilder IsClustered<TEntityBuilder>(
            [NotNull] this ModelBuilder.EntityBuilderBase<TEntityBuilder>.IndexesBuilder.IndexBuilder indexesBuilder,
            bool clustered)
            where TEntityBuilder : ModelBuilder.EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(indexesBuilder, "indexesBuilder");

            indexesBuilder.Annotation(Annotations.IsClustered, clustered.ToString());

            return indexesBuilder;
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

        // TODO: Move this to Microsoft.Data.Entity.SqlServer
        public static bool IsClustered([NotNull] this IIndex index)
        {
            Check.NotNull(index, "index");

            var isClusteredString = index[Annotations.IsClustered];

            bool isClustered;
            if (isClusteredString == null
                || !bool.TryParse(isClusteredString, out isClustered))
            {
                isClustered = true;
            }

            return isClustered;
        }
    }
}
