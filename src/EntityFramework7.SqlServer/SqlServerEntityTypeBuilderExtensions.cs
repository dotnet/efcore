// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public static class SqlServerEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder ToSqlServerTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SqlServer().TableName = name;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ToSqlServerTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToSqlServerTable((EntityTypeBuilder)entityTypeBuilder, name);

        public static EntityTypeBuilder ToSqlServerTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            entityTypeBuilder.Metadata.SqlServer().TableName = name;
            entityTypeBuilder.Metadata.SqlServer().Schema = schema;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ToSqlServerTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToSqlServerTable((EntityTypeBuilder)entityTypeBuilder, name, schema);
    }
}
