// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
        /// <param name="referenceName">
        ///     The name of the reference navigation property on the end of the relationship that configuration began
        ///     on. If null, there is no navigation property on this end of the relationship.
        /// </param>
        /// <param name="builder"> The internal builder being used to configure the relationship. </param>
        public ReferenceNavigationBuilder(
            [NotNull] EntityType relatedEntityType,
            [CanBeNull] string referenceName,
            [NotNull] InternalRelationshipBuilder builder)
            : base(relatedEntityType, referenceName, builder)
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
        public virtual ManyToOneBuilder<TEntity, TRelatedEntity> WithMany(
            [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
            => new ManyToOneBuilder<TEntity, TRelatedEntity>(WithManyBuilder(collection?.GetPropertyAccess().Name));

        /// <summary>
        ///     Configures this as a one-to-one relationship.
        /// </summary>
        /// <param name="inverseReference">
        ///     A lambda expression representing the reference navigation property on the other end of this
        ///     relationship (<c>t => t.Reference1</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on the other end of the relationship.
        /// </param>
        /// <returns> An object to further configure the relationship. </returns>
        public virtual OneToOneBuilder WithOne([CanBeNull] Expression<Func<TRelatedEntity, TEntity>> inverseReference = null)
            => new OneToOneBuilder(WithOneBuilder(inverseReference?.GetPropertyAccess().Name));
    }
}
