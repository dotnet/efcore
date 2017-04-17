// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="ReferenceOwnershipBuilder" />.
    /// </summary>
    public static class SqlServerReferenceOwnershipBuilderExtensions
    {
        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ForSqlServerToTable(
            [NotNull] this ReferenceOwnershipBuilder referenceOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceOwnershipBuilder.OwnedEntityType.SqlServer().TableName = name;

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder<TEntity, TRelatedEntity> ForSqlServerToTable<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)ForSqlServerToTable(
                (ReferenceOwnershipBuilder)referenceOwnershipBuilder, name);

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ForSqlServerToTable(
            [NotNull] this ReferenceOwnershipBuilder referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var relationalEntityTypeAnnotations = referenceOwnershipBuilder.OwnedEntityType.SqlServer();
            relationalEntityTypeAnnotations.TableName = name;
            relationalEntityTypeAnnotations.Schema = schema;

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the table. </param>
        /// <param name="schema"> The schema of the table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder<TEntity, TRelatedEntity> ForSqlServerToTable<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)ForSqlServerToTable(
                (ReferenceOwnershipBuilder)referenceOwnershipBuilder, name, schema);

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ForSqlServerIsMemoryOptimized(
            [NotNull] this ReferenceOwnershipBuilder referenceOwnershipBuilder, bool memoryOptimized = true)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));

            referenceOwnershipBuilder.OwnedEntityType.SqlServer().IsMemoryOptimized = memoryOptimized;

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the table that the entity maps to when targeting SQL Server as memory-optimized.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="memoryOptimized"> A value indicating whether the table is memory-optimized. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder<TEntity, TRelatedEntity> ForSqlServerIsMemoryOptimized<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder, bool memoryOptimized = true)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)ForSqlServerIsMemoryOptimized((ReferenceOwnershipBuilder)referenceOwnershipBuilder, memoryOptimized);

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting SQL Server.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ForSqlServerHasConstraintName(
            [NotNull] this ReferenceOwnershipBuilder referenceOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceOwnershipBuilder.Metadata.SqlServer().Name = name;

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting SQL Server.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type on the other end of the relationship. </typeparam>
        public static ReferenceOwnershipBuilder<TEntity, TRelatedEntity> ForSqlServerHasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)ForSqlServerHasConstraintName(
                (ReferenceOwnershipBuilder)referenceOwnershipBuilder, name);
    }
}
