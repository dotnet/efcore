// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a relationship where configuration began on
    ///         an end of the relationship with a collection that contains instances of another entity type.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
    /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
    public class CollectionNavigationBuilder<TEntity, TRelatedEntity> : CollectionNavigationBuilder
        where TEntity : class
        where TRelatedEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CollectionNavigationBuilder(
            IMutableEntityType declaringEntityType,
            IMutableEntityType relatedEntityType,
            MemberIdentity navigation,
            IMutableForeignKey? foreignKey,
            IMutableSkipNavigation? skipNavigation)
            : base(declaringEntityType, relatedEntityType, navigation, foreignKey, skipNavigation)
        {
        }

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public new virtual ReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
            string? navigationName = null)
        {
            return new(
                DeclaringEntityType,
                RelatedEntityType,
                WithOneBuilder(
                    Check.NullButNotEmpty(navigationName, nameof(navigationName))).Metadata);
        }

        /// <summary>
        ///     <para>
        ///         Configures this as a one-to-many relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        /// </summary>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on the other end of this
        ///     relationship (<c>post => post.Blog</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression)
            => new(
                DeclaringEntityType,
                RelatedEntityType,
                WithOneBuilder(navigationExpression?.GetMemberAccess()).Metadata);

        /// <summary>
        ///     Configures this as a many-to-many relationship.
        /// </summary>
        /// <param name="navigationName">
        ///     The name of the collection navigation property on the other end of this relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public new virtual CollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(string navigationName)
        {
            var leftName = Builder?.Metadata.PrincipalToDependent!.Name;
            var collectionCollectionBuilder =
                new CollectionCollectionBuilder<TRelatedEntity, TEntity>(
                    RelatedEntityType,
                    DeclaringEntityType,
                    WithLeftManyNavigation(navigationName),
                    WithRightManyNavigation(navigationName, leftName!));

            Configure(collectionCollectionBuilder);

            return collectionCollectionBuilder;
        }

        /// <summary>
        ///     <para>
        ///         Configures this as a many-to-many relationship.
        ///     </para>
        /// </summary>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on the other end of this
        ///     relationship (<c>post => post.Blog</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual CollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>> navigationExpression)
        {
            if (Builder != null
                && Builder.Metadata.PrincipalToDependent == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.MissingInverseManyToManyNavigation(
                        Builder.Metadata.PrincipalEntityType.DisplayName(),
                        Builder.Metadata.DeclaringEntityType.DisplayName()));
            }

            var leftName = Builder?.Metadata.PrincipalToDependent!.Name;
            var collectionCollectionBuilder =
                new CollectionCollectionBuilder<TRelatedEntity, TEntity>(
                    RelatedEntityType,
                    DeclaringEntityType,
                    WithLeftManyNavigation(navigationExpression.GetMemberAccess()),
                    WithRightManyNavigation(navigationExpression.GetMemberAccess(), leftName));

            Configure(collectionCollectionBuilder);

            return collectionCollectionBuilder;
        }
    }
}
