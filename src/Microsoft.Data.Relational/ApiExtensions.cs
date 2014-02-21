// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public static class ApiExtensions
    {
        public static class Annotations
        {
            public const string StorageTypeName = "StorageTypeName";
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
    }
}
