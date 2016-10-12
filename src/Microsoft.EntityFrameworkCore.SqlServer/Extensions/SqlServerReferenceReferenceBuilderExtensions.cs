// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="ReferenceReferenceBuilder" />.
    /// </summary>
    public static class SqlServerReferenceReferenceBuilderExtensions
    {
        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting SQL Server.
        /// </summary>
        /// <param name="referenceReferenceBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceReferenceBuilder ForSqlServerHasConstraintName(
            [NotNull] this ReferenceReferenceBuilder referenceReferenceBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceReferenceBuilder.Metadata.SqlServer().Name = name;

            return referenceReferenceBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting SQL Server.
        /// </summary>
        /// <param name="referenceReferenceBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type on the other end of the relationship. </typeparam>
        public static ReferenceReferenceBuilder<TEntity, TRelatedEntity> ForSqlServerHasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceReferenceBuilder<TEntity, TRelatedEntity>)ForSqlServerHasConstraintName(
                (ReferenceReferenceBuilder)referenceReferenceBuilder, name);
    }
}
