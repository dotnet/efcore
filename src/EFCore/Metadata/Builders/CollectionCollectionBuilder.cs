// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-many relationship.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class CollectionCollectionBuilder
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
        {
            Check.DebugAssert(((IConventionEntityType)leftEntityType).Builder != null, "Builder is null");
            Check.DebugAssert(((IConventionEntityType)rightEntityType).Builder != null, "Builder is null");
            Check.DebugAssert(((IConventionSkipNavigation)leftNavigation).Builder != null, "Builder is null");
            Check.DebugAssert(((IConventionSkipNavigation)rightNavigation).Builder != null, "Builder is null");

            LeftEntityType = leftEntityType;
            RightEntityType = rightEntityType;
            LeftNavigation = leftNavigation;
            RightNavigation = rightNavigation;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableEntityType LeftEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual IMutableEntityType RightEntityType { get; }

        /// <summary>
        ///     One of the navigations involved in the relationship.
        /// </summary>
        public virtual IMutableSkipNavigation LeftNavigation { get; private set; }

        /// <summary>
        ///     One of the navigations involved in the relationship.
        /// </summary>
        public virtual IMutableSkipNavigation RightNavigation { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalModelBuilder ModelBuilder
            => LeftEntityType.AsEntityType().Model.Builder;

        /// <summary>
        ///     Configures the join entity type implementing the many-to-many relationship.
        /// </summary>
        /// <param name="configureJoinEntityType"> The configuration of the join entity type. </param>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder UsingEntity(
            [NotNull] Action<EntityTypeBuilder> configureJoinEntityType)
        {
            Check.DebugAssert(LeftNavigation.JoinEntityType != null, "LeftNavigation.JoinEntityType is null");
            Check.DebugAssert(RightNavigation.JoinEntityType != null, "RightNavigation.JoinEntityType is null");
            Check.DebugAssert(
                LeftNavigation.JoinEntityType == RightNavigation.JoinEntityType,
                "LeftNavigation.JoinEntityType != RightNavigation.JoinEntityType");

            var joinEntityTypeBuilder = new EntityTypeBuilder(LeftNavigation.JoinEntityType);
            configureJoinEntityType(joinEntityTypeBuilder);

            return new EntityTypeBuilder(RightEntityType);
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="joinEntityType"> The CLR type of the join entity. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <returns> The builder for the join entity type. </returns>
        public virtual EntityTypeBuilder UsingEntity(
            [NotNull] Type joinEntityType,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft)
        {
            Check.NotNull(joinEntityType, nameof(joinEntityType));
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));

            var existingJoinEntityType = (EntityType)
                (LeftNavigation.ForeignKey?.DeclaringEntityType
                    ?? RightNavigation.ForeignKey?.DeclaringEntityType);
            EntityType newJoinEntityType = null;
            if (existingJoinEntityType != null)
            {
                if (existingJoinEntityType.ClrType == joinEntityType
                    && !existingJoinEntityType.HasSharedClrType)
                {
                    newJoinEntityType = existingJoinEntityType;
                }
                else
                {
                    ModelBuilder.RemoveImplicitJoinEntity(existingJoinEntityType);
                }
            }

            if (newJoinEntityType == null)
            {
                newJoinEntityType = ModelBuilder.Entity(joinEntityType, ConfigurationSource.Explicit).Metadata;
            }

            var entityTypeBuilder = new EntityTypeBuilder(newJoinEntityType);

            var leftForeignKey = configureLeft(entityTypeBuilder).Metadata;
            var rightForeignKey = configureRight(entityTypeBuilder).Metadata;

            Using(rightForeignKey, leftForeignKey);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="joinEntityName"> The name of the join entity. </param>
        /// <param name="joinEntityType"> The CLR type of the join entity. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <returns> The builder for the join entity type. </returns>
        public virtual EntityTypeBuilder UsingEntity(
            [NotNull] string joinEntityName,
            [NotNull] Type joinEntityType,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft)
        {
            Check.NotEmpty(joinEntityName, nameof(joinEntityName));
            Check.NotNull(joinEntityType, nameof(joinEntityType));
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));

            var existingJoinEntityType = (EntityType)
                (LeftNavigation.ForeignKey?.DeclaringEntityType
                    ?? RightNavigation.ForeignKey?.DeclaringEntityType);
            EntityType newJoinEntityType = null;
            if (existingJoinEntityType != null)
            {
                if (existingJoinEntityType.ClrType == joinEntityType
                    && string.Equals(existingJoinEntityType.Name, joinEntityName, StringComparison.Ordinal))
                {
                    newJoinEntityType = existingJoinEntityType;
                }
                else
                {
                    ModelBuilder.RemoveImplicitJoinEntity(existingJoinEntityType);
                }
            }

            if (newJoinEntityType == null)
            {
                var existingEntityType = ModelBuilder.Metadata.FindEntityType(joinEntityName);
                if (existingEntityType?.ClrType == joinEntityType)
                {
                    newJoinEntityType = existingEntityType;
                }
                else
                {
                    if (!ModelBuilder.Metadata.IsShared(joinEntityType))
                    {
                        throw new InvalidOperationException(CoreStrings.TypeNotMarkedAsShared(joinEntityType.DisplayName()));
                    }

                    newJoinEntityType = ModelBuilder.SharedTypeEntity(joinEntityName, joinEntityType, ConfigurationSource.Explicit)
                        .Metadata;
                }
            }

            var entityTypeBuilder = new EntityTypeBuilder(newJoinEntityType);

            var leftForeignKey = configureLeft(entityTypeBuilder).Metadata;
            var rightForeignKey = configureRight(entityTypeBuilder).Metadata;

            Using(rightForeignKey, leftForeignKey);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="joinEntityType"> The CLR type of the join entity. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <param name="configureJoinEntityType"> The configuration of the join entity type. </param>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder UsingEntity(
            [NotNull] Type joinEntityType,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
            [NotNull] Action<EntityTypeBuilder> configureJoinEntityType)
        {
            Check.NotNull(joinEntityType, nameof(joinEntityType));
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));
            Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

            var entityTypeBuilder = UsingEntity(joinEntityType, configureRight, configureLeft);
            configureJoinEntityType(entityTypeBuilder);

            return new EntityTypeBuilder(RightEntityType);
        }

        /// <summary>
        ///     Configures the relationships to the entity types participating in the many-to-many relationship.
        /// </summary>
        /// <param name="joinEntityName"> The name of the join entity. </param>
        /// <param name="joinEntityType"> The CLR type of the join entity. </param>
        /// <param name="configureRight"> The configuration for the relationship to the right entity type. </param>
        /// <param name="configureLeft"> The configuration for the relationship to the left entity type. </param>
        /// <param name="configureJoinEntityType"> The configuration of the join entity type. </param>
        /// <returns> The builder for the originating entity type so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder UsingEntity(
            [NotNull] string joinEntityName,
            [NotNull] Type joinEntityType,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureRight,
            [NotNull] Func<EntityTypeBuilder, ReferenceCollectionBuilder> configureLeft,
            [NotNull] Action<EntityTypeBuilder> configureJoinEntityType)
        {
            Check.NotEmpty(joinEntityName, nameof(joinEntityName));
            Check.NotNull(joinEntityType, nameof(joinEntityType));
            Check.NotNull(configureRight, nameof(configureRight));
            Check.NotNull(configureLeft, nameof(configureLeft));
            Check.NotNull(configureJoinEntityType, nameof(configureJoinEntityType));

            var entityTypeBuilder = UsingEntity(joinEntityName, joinEntityType, configureRight, configureLeft);
            configureJoinEntityType(entityTypeBuilder);

            return new EntityTypeBuilder(RightEntityType);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual void Using([NotNull] IMutableForeignKey rightForeignKey, [NotNull] IMutableForeignKey leftForeignKey)
        {
            var leftBuilder = ((SkipNavigation)LeftNavigation).Builder;
            var rightBuilder = ((SkipNavigation)RightNavigation).Builder;

            leftBuilder = leftBuilder.HasInverse(rightBuilder.Metadata, ConfigurationSource.Explicit);

            leftBuilder = leftBuilder.HasForeignKey((ForeignKey)leftForeignKey, ConfigurationSource.Explicit);
            rightBuilder = rightBuilder.HasForeignKey((ForeignKey)rightForeignKey, ConfigurationSource.Explicit);

            LeftNavigation = leftBuilder.Metadata;
            RightNavigation = rightBuilder.Metadata;
        }

        #region Hidden System.Object members

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
