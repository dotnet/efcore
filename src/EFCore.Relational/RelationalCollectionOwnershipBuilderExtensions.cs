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
    ///     Relational database specific extension methods for <see cref="CollectionOwnershipBuilder" />.
    /// </summary>
    public static class RelationalCollectionOwnershipBuilderExtensions
    {
        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <param name="collectionOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static CollectionOwnershipBuilder ToTable(
            [NotNull] this CollectionOwnershipBuilder collectionOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(collectionOwnershipBuilder, nameof(collectionOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            collectionOwnershipBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name);

            return collectionOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="collectionOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static CollectionOwnershipBuilder<TEntity, TDependentEntity> ToTable<TEntity, TDependentEntity>(
            [NotNull] this CollectionOwnershipBuilder<TEntity, TDependentEntity> collectionOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TDependentEntity : class
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)ToTable((CollectionOwnershipBuilder)collectionOwnershipBuilder, name);

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <param name="collectionOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static CollectionOwnershipBuilder ToTable(
            [NotNull] this CollectionOwnershipBuilder collectionOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(collectionOwnershipBuilder, nameof(collectionOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            collectionOwnershipBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name, schema);

            return collectionOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the view or table that the entity maps to when targeting a relational database.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="collectionOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the view or table. </param>
        /// <param name="schema"> The schema of the view or table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static CollectionOwnershipBuilder<TEntity, TDependentEntity> ToTable<TEntity, TDependentEntity>(
            [NotNull] this CollectionOwnershipBuilder<TEntity, TDependentEntity> collectionOwnershipBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            where TDependentEntity : class
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)ToTable((CollectionOwnershipBuilder)collectionOwnershipBuilder, name, schema);

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="referenceReferenceBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static CollectionOwnershipBuilder HasConstraintName(
            [NotNull] this CollectionOwnershipBuilder referenceReferenceBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceReferenceBuilder, nameof(referenceReferenceBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceReferenceBuilder.GetInfrastructure<InternalRelationshipBuilder>()
                .Relational(ConfigurationSource.Explicit)
                .HasConstraintName(name);

            return referenceReferenceBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting a relational database.
        /// </summary>
        /// <param name="referenceReferenceBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The entity type on one end of the relationship. </typeparam>
        /// <typeparam name="TDependentEntity"> The entity type on the other end of the relationship. </typeparam>
        public static CollectionOwnershipBuilder<TEntity, TDependentEntity> HasConstraintName<TEntity, TDependentEntity>(
            [NotNull] this CollectionOwnershipBuilder<TEntity, TDependentEntity> referenceReferenceBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TDependentEntity : class
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)HasConstraintName(
                (CollectionOwnershipBuilder)referenceReferenceBuilder, name);
    }
}
