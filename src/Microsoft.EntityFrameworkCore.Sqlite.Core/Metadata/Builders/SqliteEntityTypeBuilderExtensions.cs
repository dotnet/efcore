// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class SqliteEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQLite.
        /// </summary>
        /// <param name="builder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ForSqliteToTable([NotNull] this EntityTypeBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().TableName = name;

            return builder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQLite.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="builder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ForSqliteToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)((EntityTypeBuilder)builder).ForSqliteToTable(name);
    }
}
