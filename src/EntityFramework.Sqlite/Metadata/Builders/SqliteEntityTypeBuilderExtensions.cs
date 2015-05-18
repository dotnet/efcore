// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity
{
    public static class SqliteEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder SqliteTable([NotNull] this EntityTypeBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().Table = name;

            return builder;
        }

        public static EntityTypeBuilder<TEntity> SqliteTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)builder).SqliteTable(name);
    }
}
