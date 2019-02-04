// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="OwnedNavigationBuilder" />.
    /// </summary>
    public static class RelationalOwnershipBuilderExtensions
    {
        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="ownershipBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnershipBuilder HasConstraintName(
            [NotNull] this OwnershipBuilder ownershipBuilder,
            [CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            ownershipBuilder.GetInfrastructure<InternalRelationshipBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .HasConstraintName(name);

            return ownershipBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="ownershipBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TDependentEntity"> The entity type on the other end of the relationship. </typeparam>
        public static OwnershipBuilder<TEntity, TDependentEntity> HasConstraintName<TEntity, TDependentEntity>(
            [NotNull] this OwnershipBuilder<TEntity, TDependentEntity> ownershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TDependentEntity : class
            => (OwnershipBuilder<TEntity, TDependentEntity>)HasConstraintName(
                (OwnershipBuilder)ownershipBuilder, name);
    }
}
