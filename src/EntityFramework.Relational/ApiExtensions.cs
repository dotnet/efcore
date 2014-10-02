// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
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
            public const string ColumnMaxLength = "ColumnMaxLength";
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
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
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
            where TEntityBuilder : IEntityBuilder<TEntityBuilder>
        {
            Check.NotNull(entityBuilder, "entityBuilder");
            Check.NotEmpty(tableName, "tableName");
            Check.NotNull(schema, "schema");

            entityBuilder.Annotation(Annotations.TableName, tableName);
            entityBuilder.Annotation(Annotations.Schema, schema);

            return entityBuilder;
        }

        public static TPropertyBuilder ColumnName<TPropertyBuilder>(
            [NotNull] this TPropertyBuilder propertyBuilder,
            [NotNull] string columnName)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotEmpty(columnName, "columnName");

            propertyBuilder.Annotation(Annotations.ColumnName, columnName);

            return propertyBuilder;
        }

        public static TPropertyBuilder ColumnType<TPropertyBuilder>(
            [NotNull] this TPropertyBuilder propertyBuilder,
            [NotNull] string typeName)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(typeName, "typeName");

            propertyBuilder.Annotation(Annotations.StorageTypeName, typeName);

            return propertyBuilder;
        }

        public static TPropertyBuilder ColumnDefaultSql<TPropertyBuilder>(
            [NotNull] this TPropertyBuilder propertyBuilder,
            [NotNull] string columnDefaultSql)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(columnDefaultSql, "columnDefaultSql");

            propertyBuilder.Annotation(Annotations.ColumnDefaultSql, columnDefaultSql);

            return propertyBuilder;
        }

        public static TPropertyBuilder ColumnMaxLength<TPropertyBuilder>(
            [NotNull] this TPropertyBuilder propertyBuilder,
            [NotNull] int columnMaxLength)
            where TPropertyBuilder : IPropertyBuilder<TPropertyBuilder>
        {
            Check.NotNull(propertyBuilder, "propertyBuilder");
            Check.NotNull(columnMaxLength, "columnMaxLength");

            propertyBuilder.Annotation(Annotations.ColumnMaxLength, columnMaxLength.ToString(CultureInfo.InvariantCulture));

            return propertyBuilder;
        }

        public static TForeignKeyBuilder CascadeDelete<TForeignKeyBuilder>(
            [NotNull] this TForeignKeyBuilder foreignKeyBuilder,
            bool cascadeDelete)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(foreignKeyBuilder, "foreignKeyBuilder");

            foreignKeyBuilder.Annotation(Annotations.CascadeDelete, cascadeDelete.ToString());

            return foreignKeyBuilder;
        }

        public static TIndexBuilder IsClustered<TIndexBuilder>(
            [NotNull] this TIndexBuilder indexesBuilder,
            bool clustered)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(indexesBuilder, "indexesBuilder");

            indexesBuilder.Annotation(Annotations.IsClustered, clustered.ToString());

            return indexesBuilder;
        }

        public static TForeignKeyBuilder KeyName<TForeignKeyBuilder>(
            [NotNull] this IForeignKeyBuilder<TForeignKeyBuilder> builder,
            [NotNull] string keyName)
            where TForeignKeyBuilder : IForeignKeyBuilder<TForeignKeyBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(keyName, "keyName");

            builder.Annotation(Annotations.KeyName, keyName);

            return (TForeignKeyBuilder)builder;
        }

        public static TIndexBuilder IndexName<TIndexBuilder>(
            [NotNull] this TIndexBuilder builder,
            [NotNull] string indexName)
            where TIndexBuilder : IIndexBuilder<TIndexBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(indexName, "indexName");

            builder.Annotation(Annotations.IndexName, indexName);

            return builder;
        }

        public static TKeyBuilder KeyName<TKeyBuilder>(
            [NotNull] this IKeyBuilder<TKeyBuilder> builder,
            [NotNull] string keyName)
            where TKeyBuilder : IKeyBuilder<TKeyBuilder>
        {
            Check.NotNull(builder, "builder");
            Check.NotEmpty(keyName, "keyName");

            builder.Annotation(Annotations.KeyName, keyName);

            return (TKeyBuilder)builder;
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

        public static int? ColumnMaxLength([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            var maxLengthString = property[Annotations.ColumnMaxLength];

            int maxLength;
            if (maxLengthString == null
                || !int.TryParse(maxLengthString, NumberStyles.Integer, CultureInfo.InvariantCulture, out maxLength))
            {
                return null;
            }

            return maxLength;
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
        // Issue #764
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

            return entityType[Annotations.TableName] ?? entityType.SimpleName;
        }

        public static string Schema([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return entityType[Annotations.Schema];
        }

        public static SchemaQualifiedName SchemaQualifiedName([NotNull] this IEntityType entityType)
        {
            return new SchemaQualifiedName(entityType.TableName(), entityType.Schema());
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
    }
}
