// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
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
        ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
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
            => new EntityTypeBuilder<TEntity>(Builder.HasBaseType(name, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <param name="entityType"> The base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual EntityTypeBuilder<TEntity> HasBaseType([CanBeNull] Type entityType)
            => new EntityTypeBuilder<TEntity>(Builder.HasBaseType(entityType, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <typeparam name="TBaseType"> The base type. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder<TEntity> HasBaseType<TBaseType>()
            => HasBaseType(typeof(TBaseType));

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
            => new KeyBuilder(
                Builder.PrimaryKey(
                    Check.NotNull(keyExpression, nameof(keyExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     Creates an alternate key in the model for this entity type if one does not already exist over the specified
        ///     properties. This will force the properties to be read-only. Use <see cref="HasIndex" /> to specify uniqueness
        ///     in the model that does not force properties to be read-only.
        /// </summary>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the key property(s) (<c>blog => blog.Url</c>).
        ///     </para>
        ///     <para>
        ///         If the key is made up of multiple properties then specify an anonymous type including
        ///         the properties (<c>post => new { post.Title, post.BlogId }</c>).
        ///     </para>
        /// </param>
        /// <returns> An object that can be used to configure the key. </returns>
        public virtual KeyBuilder HasAlternateKey([NotNull] Expression<Func<TEntity, object>> keyExpression)
            => new KeyBuilder(
                Builder.HasKey(
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
            => new PropertyBuilder<TProperty>(
                Builder.Property(
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
            => (EntityTypeBuilder<TEntity>)base.Ignore(
                Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess().GetSimpleMemberName());

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the entity type. </param>
        public new virtual EntityTypeBuilder<TEntity> Ignore([NotNull] string propertyName)
            => (EntityTypeBuilder<TEntity>)base.Ignore(propertyName);

        /// <summary>
        ///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
        ///     this entity type.
        /// </summary>
        /// <param name="filter">The LINQ predicate expression.</param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder<TEntity> HasQueryFilter([CanBeNull] Expression<Func<TEntity, bool>> filter)
            => (EntityTypeBuilder<TEntity>)base.HasQueryFilter(filter);

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
            => new IndexBuilder(
                Builder.HasIndex(
                    Check.NotNull(indexExpression, nameof(indexExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            [NotNull] string navigationName)
            where TRelatedEntity : class
            => OwnsOneBuilder<TRelatedEntity>(new PropertyIdentity(Check.NotNull(navigationName, nameof(navigationName))));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            [NotNull] Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
            where TRelatedEntity : class
            => OwnsOneBuilder<TRelatedEntity>(
                new PropertyIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetPropertyAccess()));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
        {
            Check.NotNull(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction.Invoke(OwnsOneBuilder<TRelatedEntity>(new PropertyIdentity(navigationName)));
            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            [NotNull] Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
            [NotNull] Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction.Invoke(OwnsOneBuilder<TRelatedEntity>(new PropertyIdentity(navigationExpression.GetPropertyAccess())));
            return this;
        }

        private OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOneBuilder<TRelatedEntity>(PropertyIdentity navigation)
            where TRelatedEntity : class
        {
            InternalRelationshipBuilder relationship;
            using (var batch = Builder.Metadata.Model.ConventionDispatcher.StartBatch())
            {
                relationship = navigation.Property == null
                    ? Builder.Owns(typeof(TRelatedEntity), navigation.Name, ConfigurationSource.Explicit)
                    : Builder.Owns(typeof(TRelatedEntity), (PropertyInfo)navigation.Property, ConfigurationSource.Explicit);
                relationship.IsUnique(true, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship.Metadata).Builder;
            }

            return new OwnedNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relationship.Metadata.DeclaringEntityType,
                relationship);
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            [NotNull] string navigationName)
            where TRelatedEntity : class
            => OwnsManyBuilder<TRelatedEntity>(new PropertyIdentity(Check.NotNull(navigationName, nameof(navigationName))));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            [NotNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
            where TRelatedEntity : class
            => OwnsManyBuilder<TRelatedEntity>(
                new PropertyIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetPropertyAccess()));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
        {
            Check.NotNull(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction.Invoke(OwnsManyBuilder<TRelatedEntity>(new PropertyIdentity(navigationName)));
            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where the target entity is owned by (or part of) this entity.
        ///     </para>
        ///     <para>
        ///         The target entity type for each ownership relationship is treated as a different entity type
        ///         even if the navigation is of the same type. Configuration of the target entity type
        ///         isn't applied to the target entity type of other ownership relationships.
        ///     </para>
        ///     <para>
        ///         Most operations on an owned entity require accessing it through the owner entity using the corresponding navigation.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="OwnedNavigationBuilder{TEntity,TRelatedEntity}.WithOwner(string)" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            [NotNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
            [NotNull] Action<OwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction.Invoke(OwnsManyBuilder<TRelatedEntity>(new PropertyIdentity(navigationExpression.GetPropertyAccess())));
            return this;
        }

        private OwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsManyBuilder<TRelatedEntity>(PropertyIdentity navigation)
            where TRelatedEntity : class
        {
            InternalRelationshipBuilder relationship;
            using (var batch = Builder.Metadata.Model.ConventionDispatcher.StartBatch())
            {
                relationship = navigation.Property == null
                    ? Builder.Owns(typeof(TRelatedEntity), navigation.Name, ConfigurationSource.Explicit)
                    : Builder.Owns(typeof(TRelatedEntity), (PropertyInfo)navigation.Property, ConfigurationSource.Explicit);
                relationship.IsUnique(false, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship.Metadata).Builder;
            }

            return new OwnedNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relationship.Metadata.DeclaringEntityType,
                relationship);
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithMany(string)" />
        ///         or
        ///         <see cref="ReferenceNavigationBuilder{TEntity,TRelatedEntity}.WithOne(string)" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            [CanBeNull] string navigationName)
            where TRelatedEntity : class
        {
            var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigationName);

            return new ReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relatedEntityType,
                navigationName,
                Builder.Navigation(
                    relatedEntityType.Builder, navigationName, ConfigurationSource.Explicit,
                    setTargetAsPrincipal: Builder.Metadata == relatedEntityType));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
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
            var navigation = navigationExpression?.GetPropertyAccess();
            var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigation?.GetSimpleMemberName());

            return new ReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relatedEntityType,
                navigation,
                Builder.Navigation(
                    relatedEntityType.Builder, navigation, ConfigurationSource.Explicit,
                    setTargetAsPrincipal: Builder.Metadata == relatedEntityType));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
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
        /// <param name="navigationName">
        ///     The name of the collection navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual CollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            [CanBeNull] string navigationName)
            where TRelatedEntity : class
        {
            Check.NullButNotEmpty(navigationName, nameof(navigationName));

            var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigationName);

            InternalRelationshipBuilder relationship;
            using (var batch = Builder.Metadata.Model.ConventionDispatcher.StartBatch())
            {
                relationship = relatedEntityType.Builder
                    .Relationship(Builder, ConfigurationSource.Explicit)
                    .IsUnique(false, ConfigurationSource.Explicit)
                    .RelatedEntityTypes(Builder.Metadata, relatedEntityType, ConfigurationSource.Explicit)
                    .PrincipalToDependent(navigationName, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship);
            }

            return new CollectionNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
                relatedEntityType,
                navigationName,
                relationship);
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
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
            var navigation = navigationExpression?.GetPropertyAccess();
            var relatedEntityType = FindRelatedEntityType(typeof(TRelatedEntity), navigation?.GetSimpleMemberName());

            InternalRelationshipBuilder relationship;
            using (var batch = Builder.Metadata.Model.ConventionDispatcher.StartBatch())
            {
                relationship = relatedEntityType.Builder
                    .Relationship(Builder, ConfigurationSource.Explicit)
                    .IsUnique(false, ConfigurationSource.Explicit)
                    .RelatedEntityTypes(Builder.Metadata, relatedEntityType, ConfigurationSource.Explicit)
                    .PrincipalToDependent(navigation, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship);
            }

            return new CollectionNavigationBuilder<TEntity, TRelatedEntity>(
                Builder.Metadata,
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
        public new virtual EntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
            => (EntityTypeBuilder<TEntity>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

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
        public new virtual EntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (EntityTypeBuilder<TEntity>)base.UsePropertyAccessMode(propertyAccessMode);

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data of the same type as the entity.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public virtual DataBuilder<TEntity> HasData([NotNull] params TEntity[] data)
            => HasData((IEnumerable<object>)data);

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data of the same type as the entity.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public virtual DataBuilder<TEntity> HasData([NotNull] IEnumerable<TEntity> data)
            => HasData((IEnumerable<object>)data);

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data represented by anonymous types.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public new virtual DataBuilder<TEntity> HasData([NotNull] params object[] data)
            => HasData((IEnumerable<object>)data);

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data represented by anonymous types.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public new virtual DataBuilder<TEntity> HasData([NotNull] IEnumerable<object> data)
        {
            base.HasData(data);

            return new DataBuilder<TEntity>();
        }

        private InternalEntityTypeBuilder Builder => this.GetInfrastructure<InternalEntityTypeBuilder>();
    }
}
