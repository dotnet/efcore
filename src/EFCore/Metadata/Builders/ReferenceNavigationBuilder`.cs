// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a relationship where configuration began on an end of the
    ///         relationship with a reference that points to an instance of another entity type.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
    /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
    public class ReferenceNavigationBuilder<TEntity, TRelatedEntity> : ReferenceNavigationBuilder
        where TEntity : class
        where TRelatedEntity : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder builder)
            : base(declaringEntityType, relatedEntityType, navigationName, builder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] PropertyInfo navigationProperty,
            [NotNull] InternalRelationshipBuilder builder)
            : base(declaringEntityType, relatedEntityType, navigationProperty, builder)
        {
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
        /// <param name="navigationName">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null or not specified, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public new virtual ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany([CanBeNull] string navigationName = null)
            => new ReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                RelatedEntityType,
                DeclaringEntityType,
                WithManyBuilder(Check.NullButNotEmpty(navigationName, nameof(navigationName))));

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
        ///     A lambda expression representing the collection navigation property on the other end of this
        ///     relationship (<c>blog => blog.Posts</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression)
            => new ReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                RelatedEntityType,
                DeclaringEntityType,
                WithManyBuilder(navigationExpression?.GetPropertyAccess()));

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
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null or not specified, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne([CanBeNull] string navigationName = null)
            => new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                DeclaringEntityType,
                RelatedEntityType,
                WithOneBuilder(Check.NullButNotEmpty(navigationName, nameof(navigationName))));

        /// <summary>
        ///     <para>
        ///         Configures this as a one-to-one relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        /// </summary>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on the other end of this
        ///     relationship (<c>blog => blog.BlogInfo</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            [CanBeNull] Expression<Func<TRelatedEntity, TEntity>> navigationExpression)
            => new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                DeclaringEntityType,
                RelatedEntityType,
                WithOneBuilder(navigationExpression?.GetPropertyAccess()));
    }
}
