// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
        ///     Configures the join entity type implementing the many-to-many relationship.
        /// </summary>
        /// <param name="configureJoinEntityType"> The configuration of the join entity type. </param>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TRightEntity> UsingEntity(
            [NotNull] Action<EntityTypeBuilder> configureJoinEntityType)
        {
            Check.DebugAssert(LeftNavigation.JoinEntityType != null, "LeftNavigation.JoinEntityType is null");
            Check.DebugAssert(RightNavigation.JoinEntityType != null, "RightNavigation.JoinEntityType is null");
            Check.DebugAssert(
                LeftNavigation.JoinEntityType == RightNavigation.JoinEntityType,
                "LeftNavigation.JoinEntityType != RightNavigation.JoinEntityType");

            var joinEntityTypeBuilder = new EntityTypeBuilder(LeftNavigation.JoinEntityType);
            configureJoinEntityType(joinEntityTypeBuilder);

            return new EntityTypeBuilder<TRightEntity>(RightEntityType);
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <typeparam name="TJoinEntity"> The CLR type of the join entity. </typeparam>
        /// <returns> The builder for the join type. </returns>
        public virtual EntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            where TJoinEntity : class
        {
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));

            var existingJoinEntityType = (EntityType)
                (LeftNavigation.ForeignKey?.DeclaringEntityType
                    ?? RightNavigation.ForeignKey?.DeclaringEntityType);
            EntityType joinEntityType = null;
            if (existingJoinEntityType != null)
            {
                if (existingJoinEntityType.ClrType == typeof(TJoinEntity)
                    && !existingJoinEntityType.HasSharedClrType)
                {
                    joinEntityType = existingJoinEntityType;
                }
                else
                {
                    ModelBuilder.RemoveImplicitJoinEntity(existingJoinEntityType);
                }
            }

            if (joinEntityType == null)
            {
                joinEntityType = ModelBuilder.Entity(typeof(TJoinEntity), ConfigurationSource.Explicit).Metadata;
            }

            var entityTypeBuilder = new EntityTypeBuilder<TJoinEntity>(joinEntityType);

            var leftForeignKey = configureLeft(entityTypeBuilder).Metadata;
            var rightForeignKey = configureRight(entityTypeBuilder).Metadata;

            Using(rightForeignKey, leftForeignKey);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="joinEntityName"> The name of the join entity. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <typeparam name="TJoinEntity"> The CLR type of the join entity. </typeparam>
        /// <returns> The builder for the join entity type. </returns>
        public virtual EntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            [NotNull] string joinEntityName,
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            where TJoinEntity : class
        {
            Check.NotEmpty(joinEntityName, nameof(joinEntityName));
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));

            var existingJoinEntityType = (EntityType)
                (LeftNavigation.ForeignKey?.DeclaringEntityType
                    ?? RightNavigation.ForeignKey?.DeclaringEntityType);
            EntityType joinEntityType = null;
            if (existingJoinEntityType != null)
            {
                if (existingJoinEntityType.ClrType == typeof(TJoinEntity)
                    && string.Equals(existingJoinEntityType.Name, joinEntityName, StringComparison.Ordinal))
                {
                    joinEntityType = existingJoinEntityType;
                }
                else
                {
                    ModelBuilder.RemoveImplicitJoinEntity(existingJoinEntityType);
                }
            }

            if (joinEntityType == null)
            {
                var existingEntityType = ModelBuilder.Metadata.FindEntityType(joinEntityName);
                if (existingEntityType?.ClrType == typeof(TJoinEntity))
                {
                    joinEntityType = existingEntityType;
                }
                else
                {
                    if (!ModelBuilder.Metadata.IsShared(typeof(TJoinEntity)))
                    {
                        throw new InvalidOperationException(CoreStrings.TypeNotMarkedAsShared(typeof(TJoinEntity).DisplayName()));
                    }

                    joinEntityType = ModelBuilder.SharedTypeEntity(joinEntityName, typeof(TJoinEntity), ConfigurationSource.Explicit)
                        .Metadata;
                }
            }

            var entityTypeBuilder = new EntityTypeBuilder<TJoinEntity>(joinEntityType);

            var leftForeignKey = configureLeft(entityTypeBuilder).Metadata;
            var rightForeignKey = configureRight(entityTypeBuilder).Metadata;

            Using(rightForeignKey, leftForeignKey);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <param name="configureJoinEntityType"> The configuration of the join entity type. </param>
        /// <typeparam name="TJoinEntity"> The CLR type of the join entity. </typeparam>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            [NotNull] Action<EntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
        {
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));

            var entityTypeBuilder = UsingEntity(configureRight, configureLeft);
            configureJoinEntityType(entityTypeBuilder);

            return new EntityTypeBuilder<TRightEntity>(RightEntityType);
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="joinEntityName"> The name of the join entity. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <param name="configureJoinEntityType"> The configuration of the join entity type. </param>
        /// <typeparam name="TJoinEntity"> The CLR type of the join entity. </typeparam>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            [NotNull] string joinEntityName,
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            [NotNull] Func<EntityTypeBuilder<TJoinEntity>, ReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            [NotNull] Action<EntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
        {
            Check.NotEmpty(joinEntityName, nameof(joinEntityName));
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));
            Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

            var entityTypeBuilder = UsingEntity(joinEntityName, configureRight, configureLeft);
            configureJoinEntityType(entityTypeBuilder);

            return new EntityTypeBuilder<TRightEntity>(RightEntityType);
        }
    }
}
