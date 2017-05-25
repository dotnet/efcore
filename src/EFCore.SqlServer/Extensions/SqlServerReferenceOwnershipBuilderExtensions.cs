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
    }
}
