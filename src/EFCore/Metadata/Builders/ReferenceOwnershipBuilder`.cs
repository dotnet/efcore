// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-one ownership.
    ///     </para>
    /// </summary>
    public class ReferenceOwnershipBuilder<TEntity, TRelatedEntity> : ReferenceOwnershipBuilder
        where TEntity : class
        where TRelatedEntity : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ReferenceOwnershipBuilder(
            [NotNull] EntityType declaringEntityType,
            [NotNull] EntityType relatedEntityType,
            [NotNull] InternalRelationshipBuilder builder)
            : base(declaringEntityType, relatedEntityType, builder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ReferenceOwnershipBuilder(
            InternalRelationshipBuilder builder,
            ReferenceOwnershipBuilder oldBuilder,
            bool inverted = false,
            bool foreignKeySet = false,
            bool principalKeySet = false,
            bool requiredSet = false)
            : base(builder, oldBuilder, inverted, foreignKeySet, principalKeySet, requiredSet)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the foreign key. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKeyAnnotation(
            [NotNull] string annotation, [NotNull] object value)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)base.HasForeignKeyAnnotation(annotation, value);

        /// <summary>
        ///     <para>
        ///         Configures the property(s) to use as the foreign key for this relationship.
        ///     </para>
        ///     <para>
        ///         If the specified property name(s) do not exist on the entity type then a new shadow state
        ///         property(s) will be added to serve as the foreign key. A shadow state property is one
        ///         that does not have a corresponding property in the entity class. The current value for the
        ///         property is stored in the <see cref="ChangeTracker" /> rather than being stored in instances
        ///         of the entity class.
        ///     </para>
        ///     <para>
        ///         If <see cref="HasPrincipalKey(string[])" /> is not specified, then an attempt will be made to
        ///         match the data type and order of foreign key properties against the primary key of the principal
        ///         entity type. If they do not match, new shadow state properties that form a unique index will be
        ///         added to the principal entity type to serve as the reference key.
        ///     </para>
        /// </summary>
        /// <param name="foreignKeyPropertyNames">
        ///     The name(s) of the foreign key property(s).
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
            [NotNull] params string[] foreignKeyPropertyNames)
            => new ReferenceOwnershipBuilder<TEntity, TRelatedEntity>(
                Builder.HasForeignKey(
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames)),
                    RelatedEntityType,
                    ConfigurationSource.Explicit),
                this,
                foreignKeySet: true);

        /// <summary>
        ///     <para>
        ///         Configures the property(s) to use as the foreign key for this relationship.
        ///     </para>
        ///     <para>
        ///         If the specified property name(s) do not exist on the entity type then a new shadow state
        ///         property(s) will be added to serve as the foreign key. A shadow state property is one
        ///         that does not have a corresponding property in the entity class. The current value for the
        ///         property is stored in the <see cref="ChangeTracker" /> rather than being stored in instances
        ///         of the entity class.
        ///     </para>
        ///     <para>
        ///         If <see cref="HasPrincipalKey(string[])" /> is not specified, then an attempt will be made to
        ///         match the data type and order of foreign key properties against the primary key of the principal
        ///         entity type. If they do not match, new shadow state properties that form a unique index will be
        ///         added to the principal entity type to serve as the reference key.
        ///     </para>
        /// </summary>
        /// <param name="foreignKeyExpression">
        ///     <para>
        ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
        ///     </para>
        ///     <para>
        ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
        ///         corresponding keys in <see cref="HasPrincipalKey(string[])" />.
        ///     </para>
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
            [NotNull] Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
            => new ReferenceOwnershipBuilder<TEntity, TRelatedEntity>(
                Builder.HasForeignKey(
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression)).GetPropertyAccessList(),
                    RelatedEntityType,
                    ConfigurationSource.Explicit),
                this,
                foreignKeySet: true);

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If multiple principal key properties are specified, the order of principal key properties should
        ///         match the order that the primary key or unique constraint properties were configured on the principal
        ///         entity type.
        ///     </para>
        /// </remarks>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            [NotNull] params string[] keyPropertyNames)
            => new ReferenceOwnershipBuilder<TEntity, TRelatedEntity>(
                Builder.HasPrincipalKey(
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames)),
                    ConfigurationSource.Explicit),
                this,
                principalKeySet: true);

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If multiple principal key properties are specified, the order of principal key properties should
        ///         match the order that the primary key or unique constraint properties were configured on the principal
        ///         entity type.
        ///     </para>
        /// </remarks>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
        ///     </para>
        ///     <para>
        ///         If the principal key is made up of multiple properties then specify an anonymous type including
        ///         the properties (<c>t => new { t.Id1, t.Id2 }</c>).
        ///     </para>
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            [NotNull] Expression<Func<TEntity, object>> keyExpression)
            => new ReferenceOwnershipBuilder<TEntity, TRelatedEntity>(
                Builder.HasPrincipalKey(
                    Check.NotNull(keyExpression, nameof(keyExpression)).GetPropertyAccessList(),
                    ConfigurationSource.Explicit),
                this,
                principalKeySet: true);

        /// <summary>
        ///     Configures how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        /// <param name="deleteBehavior"> The action to perform. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
            => new ReferenceOwnershipBuilder<TEntity, TRelatedEntity>(
                Builder.DeleteBehavior(deleteBehavior, ConfigurationSource.Explicit), this);

        /// <summary>
        ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasEntityTypeAnnotation(
            [NotNull] string annotation, [NotNull] object value)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)base.HasEntityTypeAnnotation(annotation, value);

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure a property of the entity type.
        ///         If no property with the given name exists, then a new property will be added.
        ///     </para>
        ///     <para>
        ///         When adding a new property, if a property with the same name exists in the entity class
        ///         then it will be added to the model. If no property exists in the entity class, then
        ///         a new shadow state property will be added. A shadow state property is one that does not have a
        ///         corresponding property in the entity class. The current value for the property is stored in
        ///         the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
        ///     </para>
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property to be configured. </typeparam>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be configured (
        ///     <c>blog => blog.Url</c>).
        /// </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] Expression<Func<TRelatedEntity, TProperty>> propertyExpression)
            => new PropertyBuilder<TProperty>(
                RelatedEntityType.Builder.Property(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess(),
                    ConfigurationSource.Explicit));

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the entity type. </param>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> Ignore([NotNull] string propertyName)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)base.Ignore(propertyName);

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be ignored
        ///     (<c>blog => blog.Url</c>).
        /// </param>
        public virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> Ignore(
            [NotNull] Expression<Func<TRelatedEntity, object>> propertyExpression)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)
                base.Ignore(Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess().Name);

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
        public virtual IndexBuilder HasIndex([NotNull] Expression<Func<TRelatedEntity, object>> indexExpression)
            => new IndexBuilder(
                RelatedEntityType.Builder.HasIndex(
                    Check.NotNull(indexExpression, nameof(indexExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///         The target entity key value is always propagated from the entity it belongs to.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual ReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity> OwnsOne<TNewRelatedEntity>(
            [NotNull] Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression)
            where TNewRelatedEntity : class
            => OwnsOneBuilder<TNewRelatedEntity>(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetPropertyAccess());

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///         The target entity key value is always propagated from the entity it belongs to.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TNewRelatedEntity>(
            [NotNull] Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression,
            [NotNull] Action<ReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity>> buildAction)
            where TNewRelatedEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DeclaringEntityType.Model.ConventionDispatcher.StartBatch())
            {
                buildAction.Invoke(OwnsOneBuilder<TNewRelatedEntity>(navigationExpression.GetPropertyAccess()));
                return this;
            }
        }

        private ReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity> OwnsOneBuilder<TNewRelatedEntity>(PropertyInfo navigation)
            where TNewRelatedEntity : class
        {
            InternalRelationshipBuilder relationship;
            using (var batch = RelatedEntityType.Model.ConventionDispatcher.StartBatch())
            {
                relationship = RelatedEntityType.Builder.Owns(typeof(TNewRelatedEntity), navigation, ConfigurationSource.Explicit);
                relationship.IsUnique(true, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship.Metadata).Builder;
            }

            return new ReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity>(
                relationship.Metadata.PrincipalEntityType,
                relationship.Metadata.DeclaringEntityType,
                relationship);
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}
        /// .WithMany(Expression{Func{TRelatedEntity,IEnumerable{TEntity}}})" />
        ///         or
        ///         <see cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}
        /// .WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>post => post.Blog</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
            [CanBeNull] Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression = null)
            where TNewRelatedEntity : class
        {
            var relatedEntityType = RelatedEntityType.FindInDefinitionPath(typeof(TNewRelatedEntity)) ??
                                    Builder.ModelBuilder.Entity(typeof(TNewRelatedEntity), ConfigurationSource.Explicit).Metadata;
            var navigation = navigationExpression?.GetPropertyAccess();

            return new ReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                RelatedEntityType,
                relatedEntityType,
                navigation,
                RelatedEntityType.Builder.Navigation(
                    relatedEntityType.Builder, navigation, ConfigurationSource.Explicit,
                    setTargetAsPrincipal: RelatedEntityType == relatedEntityType));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="CollectionNavigationBuilder{TEntity,TRelatedEntity}
        /// .WithOne(Expression{Func{TRelatedEntity,TEntity}})" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the collection navigation property on this entity type that represents
        ///     the relationship (<c>blog => blog.Posts</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property on this end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual CollectionNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasMany<TNewRelatedEntity>(
            [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TNewRelatedEntity>>> navigationExpression = null)
            where TNewRelatedEntity : class
        {
            var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TNewRelatedEntity), ConfigurationSource.Explicit).Metadata;
            var navigation = navigationExpression?.GetPropertyAccess();

            InternalRelationshipBuilder relationship;
            using (var batch = RelatedEntityType.Model.ConventionDispatcher.StartBatch())
            {
                relationship = relatedEntityType.Builder
                    .Relationship(RelatedEntityType.Builder, ConfigurationSource.Explicit)
                    .IsUnique(false, ConfigurationSource.Explicit)
                    .RelatedEntityTypes(RelatedEntityType, relatedEntityType, ConfigurationSource.Explicit)
                    .PrincipalToDependent(navigation, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship);
            }

            return new CollectionNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                RelatedEntityType,
                relatedEntityType,
                navigation,
                relationship);
        }

        /// <summary>
        ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
        ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasChangeTrackingStrategy(
            ChangeTrackingStrategy changeTrackingStrategy)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses.  Calling this method will change that behavior
        ///         for all properties of this entity type as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        ///     <para>
        ///         Calling this method overrides for all properties of this entity type any access mode that was
        ///         set on the model.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for properties of this entity type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)base.UsePropertyAccessMode(propertyAccessMode);

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data.
        /// </param>
        public virtual ReferenceOwnershipBuilder<TEntity, TRelatedEntity> SeedData([NotNull] params TRelatedEntity[] data)
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)base.SeedData(data);
    }
}
