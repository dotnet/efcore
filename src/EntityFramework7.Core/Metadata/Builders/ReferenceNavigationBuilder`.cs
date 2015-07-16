// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.Metadata.Builders
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
        ///     <para>
        ///         Initializes a new instance of the <see cref="ReferenceNavigationBuilder{TEntity, TRelatedEntity}" /> class.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="relatedEntityType"> The entity type that the reference points to. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on the end of the relationship that configuration began
        ///     on. If null, there is no navigation property on this end of the relationship.
        /// </param>
        /// <param name="builder"> The internal builder being used to configure the relationship. </param>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string navigationName,
            [NotNull] InternalRelationshipBuilder builder)
            : base(relatedEntityType, navigationName, builder)
        {
        }

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="collection">
        ///     A lambda expression representing the collection navigation property on the other end of this
        ///     relationship (<c>t => t.Collection1</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceCollectionBuilder<TRelatedEntity, TEntity> InverseCollection(
            [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection)
            => new ReferenceCollectionBuilder<TRelatedEntity, TEntity>(InverseCollectionBuilder(collection?.GetPropertyAccess().Name));

        /// <summary>
        ///     Configures this as a one-to-one relationship.
        /// </summary>
        /// <param name="reference">
        ///     A lambda expression representing the reference navigation property on the other end of this
        ///     relationship (<c>t => t.Reference1</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> InverseReference([CanBeNull] Expression<Func<TRelatedEntity, TEntity>> reference)
            => new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(InverseReferenceBuilder(reference?.GetPropertyAccess().Name));

        /// <summary>
        ///     Configures this as a one-to-many relationship.
        /// </summary>
        /// <param name="collection">
        ///     The name of the collection navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public new virtual ReferenceCollectionBuilder<TRelatedEntity, TEntity> InverseCollection([CanBeNull] string collection = null)
            => new ReferenceCollectionBuilder<TRelatedEntity, TEntity>(InverseCollectionBuilder(collection));

        /// <summary>
        ///     Configures this as a one-to-one relationship.
        /// </summary>
        /// <param name="reference">
        ///     The name of the reference navigation property on the other end of this relationship.
        ///     If null, there is no navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public new virtual ReferenceReferenceBuilder<TEntity, TRelatedEntity> InverseReference([CanBeNull] string reference = null)
            => new ReferenceReferenceBuilder<TEntity, TRelatedEntity>(InverseReferenceBuilder(reference));
    }
}
