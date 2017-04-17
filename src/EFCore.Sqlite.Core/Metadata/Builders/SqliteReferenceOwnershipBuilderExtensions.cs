// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQLite specific extension methods for <see cref="ReferenceReferenceBuilder" />.
    /// </summary>
    public static class SqliteReferenceOwnershipBuilderExtensions
    {
        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQLite.
        /// </summary>
        /// <param name="builder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ForSqliteToTable([NotNull] this ReferenceOwnershipBuilder builder, [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.OwnedEntityType.Sqlite().TableName = name;

            return builder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQLite.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="builder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder<TEntity, TRelatedEntity> ForSqliteToTable<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TRelatedEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)((ReferenceOwnershipBuilder)builder).ForSqliteToTable(name);

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting SQLite.
        /// </summary>
        /// <param name="builder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ForSqliteHasConstraintName(
            [NotNull] this ReferenceOwnershipBuilder builder,
            [CanBeNull] string name)
        {
            Check.NotNull(builder, nameof(builder));
            Check.NullButNotEmpty(name, nameof(name));

            builder.Metadata.Sqlite().Name = name;

            return builder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting SQLite.
        /// </summary>
        /// <param name="builder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TReferencedEntity"> The entity type on the other end of the relationship. </typeparam>
        public static ReferenceOwnershipBuilder<TEntity, TReferencedEntity> ForSqliteHasConstraintName<TEntity, TReferencedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TReferencedEntity> builder,
            [CanBeNull] string name)
            where TEntity : class
            where TReferencedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TReferencedEntity>)((ReferenceOwnershipBuilder)builder).ForSqliteHasConstraintName(name);
    }
}
