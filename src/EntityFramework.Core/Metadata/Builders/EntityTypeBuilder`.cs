// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring an <see cref="EntityType" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
    public class EntityTypeBuilder<TEntity> : EntityTypeBuilder
        where TEntity : class
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="EntityTypeBuilder{TEntity}" /> class to configure a given
        ///         entity type.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="builder"> Internal typeBuilder for the entity type being configured. </param>
        public EntityTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
            : base(builder)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same typeBuilder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> Annotation([NotNull] string annotation, [NotNull] object value)
        {
            base.Annotation(annotation, value);

            return this;
        }

        /// <summary>
        ///     Sets the properties that make up the primary key for this entity type.
        /// </summary>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the primary key property(s) (<c>t => t.Id1</c>).
        ///     </para>
        ///     <para>
        ///         If the primary key is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Id1, t.Id2 }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        public virtual KeyBuilder Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
        {
            Check.NotNull(keyExpression, nameof(keyExpression));

            return new KeyBuilder(Builder.PrimaryKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Creates a new unique constraint for this entity type if one does not already exist over the specified properties.
        /// </summary>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the unique constraint property(s) (<c>t => t.Id1</c>).
        ///     </para>
        ///     <para>
        ///         If the unique constraint is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Id1, t.Id2 }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the unique constraint. </returns>
        public virtual KeyBuilder AlternateKey([NotNull] Expression<Func<TEntity, object>> keyExpression)
        {
            Check.NotNull(keyExpression, nameof(keyExpression));

            return new KeyBuilder(Builder.Key(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Returns an object that can be used to configure a property of the entity type.
        ///     If the specified property is not already part of the model, it will be added.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be configured (
        ///     <c>t => t.Property1</c>).
        /// </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            var propertyInfo = propertyExpression.GetPropertyAccess();
            return new PropertyBuilder<TProperty>(Builder.Property(propertyInfo, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be ignored
        ///     (<c>t => t.Property1</c>).
        /// </param>
        public virtual EntityTypeBuilder<TEntity> Ignore([NotNull] Expression<Func<TEntity, object>> propertyExpression)
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            var propertyName = propertyExpression.GetPropertyAccess().Name;
            Builder.Ignore(propertyName, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the entity type. </param>
        public new virtual EntityTypeBuilder<TEntity> Ignore([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Builder.Ignore(propertyName, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures an index on the specified properties. If there is an existing index on the given
        ///     set of properties, then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="indexExpression">
        ///     <para>
        ///         A lambda expression representing the property(s) to be included in the index
        ///         (<c>t => t.Property1</c>).
        ///     </para>
        ///     <para>
        ///         If the index is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Property1, t.Property2 }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the index. </returns>
        public virtual IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
        {
            Check.NotNull(indexExpression, nameof(indexExpression));

            return new IndexBuilder(Builder.Index(indexExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see
        ///             cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.InverseCollection(Expression{Func{TRelatedEntity, IEnumerable{TEntity}}})" />
        ///         or
        ///         <see
        ///             cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.InverseReference(Expression{Func{TRelatedEntity, TEntity}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="reference">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>t => t.Reference1</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder<TEntity, TRelatedEntity> Reference<TRelatedEntity>(
            [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> reference = null)
            where TRelatedEntity : class
        {
            var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
            var navigationName = reference?.GetPropertyAccess().Name;

            return new ReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                relatedEntityType,
                navigationName,
                ReferenceBuilder(relatedEntityType, navigationName));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see
        ///             cref="CollectionNavigationBuilder{TEntity,TRelatedEntity}.InverseReference(Expression{Func{TRelatedEntity, TEntity}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="collection">
        ///     A lambda expression representing the collection navigation property on this entity type that represents
        ///     the relationship (<c>t => t.Collection1</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual CollectionNavigationBuilder<TEntity, TRelatedEntity> Collection<TRelatedEntity>(
            [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
            where TRelatedEntity : class
        {
            var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
            var navigationName = collection?.GetPropertyAccess().Name;

            return new CollectionNavigationBuilder<TEntity, TRelatedEntity>(
                CollectionBuilder(relatedEntityType, navigationName));
        }

        private InternalEntityTypeBuilder Builder => this.GetService<InternalEntityTypeBuilder>();
    }
}
