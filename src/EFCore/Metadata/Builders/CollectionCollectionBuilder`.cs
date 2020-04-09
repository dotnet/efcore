// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a many-to-many relationship.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TLeftEntity"> One of the entity types in this relationship. </typeparam>
    /// <typeparam name="TRightEntity"> One of the entity types in this relationship. </typeparam>
    public class CollectionCollectionBuilder<TLeftEntity, TRightEntity> : CollectionCollectionBuilder
        where TLeftEntity : class
        where TRightEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CollectionCollectionBuilder(
            [NotNull] IMutableEntityType leftEntityType,
            [NotNull] IMutableEntityType rightEntityType,
            [NotNull] IMutableSkipNavigation leftNavigation,
            [NotNull] IMutableSkipNavigation rightNavigation)
            : base(leftEntityType, rightEntityType, leftNavigation, rightNavigation)
        {
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <typeparam name="TAssociationEntity"> The type of the association entity. </typeparam>
        /// <returns> The builder for the association type. </returns>
        public virtual EntityTypeBuilder<TAssociationEntity> UsingEntity<TAssociationEntity>(
            [NotNull] Func<EntityTypeBuilder<TAssociationEntity>, ReferenceCollectionBuilder<TLeftEntity, TAssociationEntity>> configureRight,
            [NotNull] Func<EntityTypeBuilder<TAssociationEntity>, ReferenceCollectionBuilder<TRightEntity, TAssociationEntity>> configureLeft)
            where TAssociationEntity : class
        {
            var entityTypeBuilder = new EntityTypeBuilder<TAssociationEntity>(
                ModelBuilder.Entity(typeof(TAssociationEntity), ConfigurationSource.Explicit).Metadata);

            var leftForeignKey = configureLeft(entityTypeBuilder).Metadata;
            var rightForeignKey = configureRight(entityTypeBuilder).Metadata;

            Using(rightForeignKey, leftForeignKey);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureAssociation"> The configuration of the association type. </param>
        /// <typeparam name="TAssociationEntity"> The type of the association entity. </typeparam>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder<TLeftEntity> UsingEntity<TAssociationEntity>(
            [NotNull] Func<EntityTypeBuilder<TAssociationEntity>, ReferenceCollectionBuilder<TLeftEntity, TAssociationEntity>> configureRight,
            [NotNull] Func<EntityTypeBuilder<TAssociationEntity>, ReferenceCollectionBuilder<TRightEntity, TAssociationEntity>> configureLeft,
            [NotNull] Action<EntityTypeBuilder<TAssociationEntity>> configureAssociation)
            where TAssociationEntity : class
        {
            var entityTypeBuilder = UsingEntity<TAssociationEntity>(configureRight, configureLeft);
            configureAssociation(entityTypeBuilder);

            return new EntityTypeBuilder<TLeftEntity>(LeftEntityType);
        }
    }
}
