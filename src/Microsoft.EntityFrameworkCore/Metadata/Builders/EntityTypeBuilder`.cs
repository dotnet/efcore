// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
            : base(builder)
        {
        }

        /// <summary>
        ///     Creates a new builder based on the provided internal builder. This overridden implementation creates
        ///     <see cref="EntityTypeBuilder{TEntity}" /> instances so that logic inherited from the base class will
        ///     use those instead of <see cref="EntityTypeBuilder" />.
        /// </summary>
        /// <param name="builder"> The internal builder to create the new builder from. </param>
        /// <returns> The newly created builder. </returns>
        protected override EntityTypeBuilder New(InternalEntityTypeBuilder builder)
            => new EntityTypeBuilder<TEntity>(builder);

        /// <summary>
        ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same typeBuilder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> HasAnnotation([NotNull] string annotation, [NotNull] object value)
            => (EntityTypeBuilder<TEntity>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <param name="name"> The name of the base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> HasBaseType([CanBeNull] string name)
            => (EntityTypeBuilder<TEntity>)base.HasBaseType(name);

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <param name="entityType"> The base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> HasBaseType([CanBeNull] Type entityType)
            => (EntityTypeBuilder<TEntity>)base.HasBaseType(entityType);

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <typeparam name="TBaseType"> The base type. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder<TEntity> HasBaseType<TBaseType>()
            => (EntityTypeBuilder<TEntity>)base.HasBaseType(typeof(TBaseType));

        /// <summary>
        ///     Sets the properties that make up the primary key for this entity type.
        /// </summary>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the primary key property(s) (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If the primary key is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        public virtual KeyBuilder HasKey([NotNull] Expression<Func<TEntity, object>> keyExpression)
            => new KeyBuilder(Builder.PrimaryKey(
                Check.NotNull(keyExpression, nameof(keyExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     Creates a new unique constraint for this entity type if one does not already exist over the specified
        ///     properties.
        /// </summary>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the unique constraint property(s) (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If the unique constraint is made up of multiple properties then specify an anonymous type including
        ///         the properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the unique constraint. </returns>
        public virtual KeyBuilder HasAlternateKey([NotNull] Expression<Func<TEntity, object>> keyExpression)
            => new KeyBuilder(Builder.HasKey(
                Check.NotNull(keyExpression, nameof(keyExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     Returns an object that can be used to configure a property of the entity type.
        ///     If the specified property is not already part of the model, it will be added.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be configured (
        ///     <c>blog => blog.Url</c>).
        /// </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
            => new PropertyBuilder<TProperty>(Builder.Property(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess(), ConfigurationSource.Explicit));

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be ignored
        ///     (<c>blog => blog.Url</c>).
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
        ///         (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If the index is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the index. </returns>
        public virtual IndexBuilder HasIndex([NotNull] Expression<Func<TEntity, object>> indexExpression)
            => new IndexBuilder(Builder.HasIndex(
                Check.NotNull(indexExpression, nameof(indexExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see
        ///             cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithMany(Expression{Func{TRelatedEntity,IEnumerable{TEntity}}})" />
        ///         or
        ///         <see
        ///             cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>post => post.Blog</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
            where TRelatedEntity : class
        {
            var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
            var navigation = navigationExpression?.GetPropertyAccess();

            return new ReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relatedEntityType,
                navigation,
                HasOneBuilder(relatedEntityType, navigation));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see
        ///             cref="CollectionNavigationBuilder{TEntity,TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the collection navigation property on this entity type that represents
        ///     the relationship (<c>blog => blog.Posts</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual CollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
            where TRelatedEntity : class
        {
            var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
            var navigation = navigationExpression?.GetPropertyAccess();

            return new CollectionNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relatedEntityType,
                navigation,
                HasManyBuilder(relatedEntityType, navigation));
        }

        /// <summary>
        ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
        ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
            => (EntityTypeBuilder<TEntity>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses.  Calling this method witll change that behavior
        ///         for all properties of this entity type as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        ///     <para>
        ///         Calling this method overrrides for all properties of this entity type any access mode that was
        ///         set on the model.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for properties of this entity type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (EntityTypeBuilder<TEntity>)base.UsePropertyAccessMode(propertyAccessMode);

        private InternalEntityTypeBuilder Builder => this.GetInfrastructure<InternalEntityTypeBuilder>();
    }
}
