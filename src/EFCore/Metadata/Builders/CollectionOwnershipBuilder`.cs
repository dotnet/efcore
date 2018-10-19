// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a one-to-one ownership.
    ///     </para>
    /// </summary>
    public class CollectionOwnershipBuilder<TEntity, TDependentEntity> : CollectionOwnershipBuilder
        where TEntity : class
        where TDependentEntity : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionOwnershipBuilder(
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
        protected CollectionOwnershipBuilder(
            InternalRelationshipBuilder builder,
            CollectionOwnershipBuilder<TEntity, TDependentEntity> oldBuilder,
            bool foreignKeySet = false,
            bool principalKeySet = false,
            bool requiredSet = false)
            : base(builder, oldBuilder, foreignKeySet, principalKeySet, requiredSet)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the foreign key. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasForeignKeyAnnotation(
            [NotNull] string annotation, [NotNull] object value)
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)base.HasForeignKeyAnnotation(annotation, value);

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
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
            [NotNull] params string[] foreignKeyPropertyNames)
        {
            Builder = Builder.HasForeignKey(
                Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames)),
                DependentEntityType,
                ConfigurationSource.Explicit);
            return new CollectionOwnershipBuilder<TEntity, TDependentEntity>(
                Builder,
                this,
                foreignKeySet: foreignKeyPropertyNames.Length > 0);
        }

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
        ///         If <see cref="HasPrincipalKey(Expression{Func{TEntity, object}})" /> is not specified, then an
        ///         attempt will be made to match the data type and order of foreign key properties against the primary
        ///         key of the principal entity type. If they do not match, new shadow state properties that form a
        ///         unique index will be added to the principal entity type to serve as the reference key.
        ///     </para>
        /// </summary>
        /// <param name="foreignKeyExpression">
        ///     <para>
        ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
        ///     </para>
        ///     <para>
        ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
        ///         corresponding properties in <see cref="HasPrincipalKey(Expression{Func{TEntity, object}})" />.
        ///     </para>
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
            [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
        {
            Builder = Builder.HasForeignKey(
                Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression)).GetPropertyAccessList(),
                DependentEntityType,
                ConfigurationSource.Explicit);
            return new CollectionOwnershipBuilder<TEntity, TDependentEntity>(
                Builder,
                this,
                foreignKeySet: true);
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
            [NotNull] params string[] keyPropertyNames)
        {
            Builder = Builder.HasPrincipalKey(
                Check.NotNull(keyPropertyNames, nameof(keyPropertyNames)),
                ConfigurationSource.Explicit);
            return new CollectionOwnershipBuilder<TEntity, TDependentEntity>(
                Builder,
                this,
                principalKeySet: keyPropertyNames.Length > 0);
        }

        /// <summary>
        ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
        ///     method if you want to use a property(s) other than the primary key as the principal property(s). If
        ///     the specified property(s) is not already a unique constraint (or the primary key) then a new unique
        ///     constraint will be introduced.
        /// </summary>
        /// <param name="keyExpression">
        ///     <para>
        ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
        ///     </para>
        ///     <para>
        ///         If the principal key is made up of multiple properties then specify an anonymous type including the
        ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
        ///         corresponding properties in <see cref="HasForeignKey(Expression{Func{TDependentEntity, object}})" />.
        ///     </para>
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
            [NotNull] Expression<Func<TEntity, object>> keyExpression)
        {
            Builder = Builder.HasPrincipalKey(
                Check.NotNull(keyExpression, nameof(keyExpression)).GetPropertyAccessList(),
                ConfigurationSource.Explicit);
            return new CollectionOwnershipBuilder<TEntity, TDependentEntity>(
                Builder,
                this,
                principalKeySet: true);
        }

        /// <summary>
        ///     Configures how a delete operation is applied to dependent entities in the relationship when the
        ///     principal is deleted or the relationship is severed.
        /// </summary>
        /// <param name="deleteBehavior"> The action to perform. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> OnDelete(DeleteBehavior deleteBehavior)
        {
            Builder = Builder.DeleteBehavior(deleteBehavior, ConfigurationSource.Explicit);
            return this;
        }

        /// <summary>
        ///     Adds or updates an annotation on the owned entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasEntityTypeAnnotation(
            [NotNull] string annotation, [NotNull] object value)
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)base.HasEntityTypeAnnotation(annotation, value);

        /// <summary>
        ///     Sets the properties that make up the primary key for this owned entity type.
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
        public virtual KeyBuilder HasKey([NotNull] Expression<Func<TDependentEntity, object>> keyExpression)
            => new KeyBuilder(DependentEntityType.Builder.PrimaryKey(
                Check.NotNull(keyExpression, nameof(keyExpression)).GetPropertyAccessList(), ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure a property of the owned entity type.
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
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new PropertyBuilder<TProperty>(
                DependentEntityType.Builder.Property(
                    Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess(),
                    ConfigurationSource.Explicit));

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the owned entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the entity type. </param>
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> Ignore([NotNull] string propertyName)
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)base.Ignore(propertyName);

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the owned entity type that were added by convention.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be ignored
        ///     (<c>blog => blog.Url</c>).
        /// </param>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> Ignore(
            [NotNull] Expression<Func<TDependentEntity, object>> propertyExpression)
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)
                base.Ignore(Check.NotNull(propertyExpression, nameof(propertyExpression)).GetPropertyAccess().GetSimpleMemberName());

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
        public virtual IndexBuilder HasIndex([NotNull] Expression<Func<TDependentEntity, object>> indexExpression)
            => new IndexBuilder(DependentEntityType.Builder.HasIndex(
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
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual ReferenceOwnershipBuilder<TDependentEntity, TNewRelatedEntity> OwnsOne<TNewRelatedEntity>(
            [NotNull] string navigationName)
            where TNewRelatedEntity : class
            => OwnsOneBuilder<TNewRelatedEntity>(new PropertyIdentity(Check.NotNull(navigationName, nameof(navigationName))));

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
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual ReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            [NotNull] Expression<Func<TDependentEntity, TRelatedEntity>> navigationExpression)
            where TRelatedEntity : class
            => OwnsOneBuilder<TRelatedEntity>(new PropertyIdentity(
                Check.NotNull(navigationExpression, nameof(navigationExpression)).GetPropertyAccess()));

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
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> OwnsOne<TRelatedEntity>(
            [NotNull] string navigationName,
            [NotNull] Action<ReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity>> buildAction)
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
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> OwnsOne<TRelatedEntity>(
            [NotNull] Expression<Func<TDependentEntity, TRelatedEntity>> navigationExpression,
            [NotNull] Action<ReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction.Invoke(OwnsOneBuilder<TRelatedEntity>(new PropertyIdentity(navigationExpression.GetPropertyAccess())));
            return this;
        }

        private ReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity> OwnsOneBuilder<TRelatedEntity>(PropertyIdentity navigation)
            where TRelatedEntity : class
        {
            InternalRelationshipBuilder relationship;
            using (var batch = DependentEntityType.Model.ConventionDispatcher.StartBatch())
            {
                relationship = navigation.Property == null
                    ? DependentEntityType.Builder.Owns(typeof(TRelatedEntity), navigation.Name, ConfigurationSource.Explicit)
                    : DependentEntityType.Builder.Owns(typeof(TRelatedEntity), (PropertyInfo)navigation.Property, ConfigurationSource.Explicit);
                relationship.IsUnique(true, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship.Metadata).Builder;
            }

            return new ReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity>(
                relationship.Metadata.PrincipalEntityType,
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
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual CollectionOwnershipBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string navigationName)
            where TNewDependentEntity : class
            => OwnsManyBuilder<TNewDependentEntity>(new PropertyIdentity(Check.NotNull(navigationName, nameof(navigationName))));

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
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual CollectionOwnershipBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
            where TNewDependentEntity : class
            => OwnsManyBuilder<TNewDependentEntity>(
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
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string navigationName,
            [NotNull] Action<CollectionOwnershipBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotNull(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.StartBatch())
            {
                buildAction.Invoke(OwnsManyBuilder<TNewDependentEntity>(new PropertyIdentity(navigationName)));
                return this;
            }
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
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
            [NotNull] Action<CollectionOwnershipBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.StartBatch())
            {
                buildAction.Invoke(OwnsManyBuilder<TNewDependentEntity>(new PropertyIdentity(navigationExpression.GetPropertyAccess())));
                return this;
            }
        }

        private CollectionOwnershipBuilder<TDependentEntity, TNewRelatedEntity> OwnsManyBuilder<TNewRelatedEntity>(PropertyIdentity navigation)
            where TNewRelatedEntity : class
        {
            InternalRelationshipBuilder relationship;
            using (var batch = DependentEntityType.Model.ConventionDispatcher.StartBatch())
            {
                relationship = navigation.Property == null
                    ? DependentEntityType.Builder.Owns(typeof(TNewRelatedEntity), navigation.Name, ConfigurationSource.Explicit)
                    : DependentEntityType.Builder.Owns(typeof(TNewRelatedEntity), (PropertyInfo)navigation.Property, ConfigurationSource.Explicit);
                relationship.IsUnique(false, ConfigurationSource.Explicit);
                relationship = batch.Run(relationship.Metadata).Builder;
            }

            return new CollectionOwnershipBuilder<TDependentEntity, TNewRelatedEntity>(
                DependentEntityType,
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
        /// <typeparam name="TNewRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
            [CanBeNull] string navigationName)
            where TNewRelatedEntity : class
        {
            var relatedEntityType = FindRelatedEntityType(typeof(TNewRelatedEntity), navigationName);

            return new ReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity>(
                DependentEntityType,
                relatedEntityType,
                navigationName,
                DependentEntityType.Builder.Navigation(
                    relatedEntityType.Builder, navigationName, ConfigurationSource.Explicit,
                    setTargetAsPrincipal: DependentEntityType == relatedEntityType));
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
        public virtual ReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
            [CanBeNull] Expression<Func<TDependentEntity, TNewRelatedEntity>> navigationExpression = null)
            where TNewRelatedEntity : class
        {
            var navigation = navigationExpression?.GetPropertyAccess();
            var relatedEntityType = FindRelatedEntityType(typeof(TNewRelatedEntity), navigation?.GetSimpleMemberName());

            return new ReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity>(
                DependentEntityType,
                relatedEntityType,
                navigation,
                DependentEntityType.Builder.Navigation(
                    relatedEntityType.Builder, navigation, ConfigurationSource.Explicit,
                    setTargetAsPrincipal: DependentEntityType == relatedEntityType));
        }

        /// <summary>
        ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
        ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> HasChangeTrackingStrategy(
            ChangeTrackingStrategy changeTrackingStrategy)
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

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
        public new virtual CollectionOwnershipBuilder<TEntity, TDependentEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (CollectionOwnershipBuilder<TEntity, TDependentEntity>)base.UsePropertyAccessMode(propertyAccessMode);

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public virtual DataBuilder<TDependentEntity> HasData([NotNull] params TDependentEntity[] data)
        {
            Check.NotNull(data, nameof(data));

            OwnedEntityType.AddData(data);

            return new DataBuilder<TDependentEntity>();
        }

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data represented by anonymous types.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public new virtual DataBuilder<TDependentEntity> HasData([NotNull] params object[] data)
        {
            base.HasData(data);

            return new DataBuilder<TDependentEntity>();
        }
    }
}
