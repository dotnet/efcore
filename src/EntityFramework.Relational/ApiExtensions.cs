// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
            public const string TableName = "TableName";
            public const string Schema = "Schema";
            public const string ColumnName = "ColumnName";
            public const string KeyName = "KeyName";
            public const string IndexName = "IndexName";
        }

        public static TEntityBuilder ToTable<TEntityBuilder>(
            [NotNull] this TEntityBuilder entityBuilder,
            [NotNull] string tableName)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotEmpty(tableName, "tableName");

            entityBuilder.Annotation(Annotations.TableName, tableName);

            return entityBuilder;
        }

        public static TEntityBuilder ToTable<TEntityBuilder>(
            [NotNull] this TEntityBuilder entityBuilder,
            [NotNull] string tableName,
            [NotNull] string schema)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(schema, "schema");

            entityBuilder.Annotation(Annotations.TableName, tableName);
            entityBuilder.Annotation(Annotations.Schema, schema);

            return entityBuilder;
        }

        public static EntityBuilderBase<TEntityBuilder>.PropertyBuilder ColumnName<TEntityBuilder>(
            [NotNull] this EntityBuilderBase<TEntityBuilder>.PropertyBuilder propertyBuilder,
            [NotNull] string columnName)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotEmpty(columnName, "columnName");

            propertyBuilder.Annotation(Annotations.ColumnName, columnName);

            return propertyBuilder;
        }

        public static EntityBuilderBase<TEntityBuilder>.PropertyBuilder ColumnType<TEntityBuilder>(
            [NotNull] this EntityBuilderBase<TEntityBuilder>.PropertyBuilder propertyBuilder,
            [NotNull] string typeName)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(typeName, "typeName");

            propertyBuilder.Annotation(Annotations.StorageTypeName, typeName);

            return propertyBuilder;
        }

        public static EntityBuilderBase<TEntityBuilder>.PropertyBuilder ColumnDefaultSql<TEntityBuilder>(
            [NotNull] this EntityBuilderBase<TEntityBuilder>.PropertyBuilder propertyBuilder,
            [NotNull] string columnDefaultSql)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(columnDefaultSql, "columnDefaultSql");

            propertyBuilder.Annotation(Annotations.ColumnDefaultSql, columnDefaultSql);

            return propertyBuilder;
        }

        public static EntityBuilderBase<TEntityBuilder>.ForeignKeysBuilder.ForeignKeyBuilder CascadeDelete<TEntityBuilder>(
            [NotNull] this EntityBuilderBase<TEntityBuilder>.ForeignKeysBuilder.ForeignKeyBuilder foreignKeyBuilder,
            bool cascadeDelete)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            foreignKeyBuilder.Annotation(Annotations.CascadeDelete, cascadeDelete.ToString());

            return foreignKeyBuilder;
        }

        public static EntityBuilderBase<TEntityBuilder>.IndexesBuilder.IndexBuilder IsClustered<TEntityBuilder>(
            [NotNull] this EntityBuilderBase<TEntityBuilder>.IndexesBuilder.IndexBuilder indexesBuilder,
            bool clustered)
            where TEntityBuilder : EntityBuilderBase<TEntityBuilder>
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

        public static string TableName([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType[Annotations.TableName] ?? entityType.Name;
        }

        public static string Schema([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType[Annotations.Schema];
        }

        public static string ColumnName([NotNull] this IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return property[Annotations.ColumnName] ?? property.Name;
        }

        public static string KeyName([NotNull] this IKey key)
        {
            Check.NotNull(key, "key");

            return key[Annotations.KeyName];
        }

        public static string KeyName([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey[Annotations.KeyName];
        }

        public static string IndexName([NotNull] this IIndex index)
        {
            Check.NotNull(index, "index");

            return index[Annotations.IndexName];
        }

        public static void SetTableName([NotNull] this EntityType entityType, [NotNull] string tableName)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(tableName, "tableName");

            entityType[Annotations.TableName] = tableName;
        }

        public static void SetSchema([NotNull] this EntityType entityType, [NotNull] string schema)
        {
            Check.NotNull(entityType, "entityType");
            Check.NotEmpty(schema, "schema");

            entityType[Annotations.Schema] = schema;
        }

        public static void SetColumnName([NotNull] this Property property, [NotNull] string columnName)
        {
            Check.NotNull(property, "property");
            Check.NotEmpty(columnName, "columnName");

            property[Annotations.ColumnName] = columnName;
        }

        public static void SetKeyName([NotNull] this Key key, [NotNull] string keyName)
        {
            Check.NotNull(key, "key");
            Check.NotEmpty(keyName, "keyName");

            key[Annotations.KeyName] = keyName;
        }

        public static void SetIndexName([NotNull] this Index index, [NotNull] string indexName)
        {
            Check.NotNull(index, "index");
            Check.NotEmpty(indexName, "indexName");

            index[Annotations.IndexName] = indexName;
        }

        public static EntityBuilderBase<TMetadataBuilder> ToTable<TMetadataBuilder>(
            [NotNull] this EntityBuilderBase<TMetadataBuilder> builder,
            [NotNull] string tableName)
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(tableName, "tableName");

            builder.Annotation(Annotations.TableName, tableName);

            return builder;
        }

        public static EntityBuilderBase<TMetadataBuilder> ToTable<TMetadataBuilder>(
            [NotNull] this EntityBuilderBase<TMetadataBuilder> builder,
            [NotNull] string tableName,
            [NotNull] string schema)
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(schema, "schema");

            builder.Annotation(Annotations.TableName, tableName);
            builder.Annotation(Annotations.Schema, schema);

            return builder;
        }

        public static EntityBuilderBase<TMetadataBuilder>.ForeignKeysBuilder.ForeignKeyBuilder KeyName<TMetadataBuilder>(
            [NotNull] this EntityBuilderBase<TMetadataBuilder>.ForeignKeysBuilder.ForeignKeyBuilder builder,
            [NotNull] string keyName)
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(keyName, "keyName");

            builder.Annotation(Annotations.KeyName, keyName);

            return builder;
        }

        public static EntityBuilderBase<TMetadataBuilder>.IndexesBuilder.IndexBuilder IndexName<TMetadataBuilder>(
            [NotNull] this EntityBuilderBase<TMetadataBuilder>.IndexesBuilder.IndexBuilder builder,
            [NotNull] string indexName)
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(indexName, "indexName");

            builder.Annotation(Annotations.IndexName, indexName);

            return builder;
        }

        public static EntityBuilderBase<TMetadataBuilder>.KeyMetadataBuilder KeyName<TMetadataBuilder>(
            [NotNull] this EntityBuilderBase<TMetadataBuilder>.KeyMetadataBuilder builder,
            [NotNull] string keyName)
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(keyName, "keyName");

            builder.Annotation(Annotations.KeyName, keyName);

            return builder;
        }
    }
}
