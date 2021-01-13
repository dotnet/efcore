// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a navigation to an owned entity type.
    /// </summary>
    public class OwnedNavigationBuilder<TEntity, TDependentEntity> : OwnedNavigationBuilder
        where TEntity : class
        where TDependentEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public OwnedNavigationBuilder([NotNull] IMutableForeignKey ownership)
            : base(ownership)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the owned entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> HasAnnotation(
            [NotNull] string annotation,
            [NotNull] object value)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.HasAnnotation(annotation, value);

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
        public virtual KeyBuilder<TDependentEntity> HasKey([NotNull] Expression<Func<TDependentEntity, object>> keyExpression)
            => new KeyBuilder<TDependentEntity>(
                DependentEntityType.Builder.PrimaryKey(
                    Check.NotNull(keyExpression, nameof(keyExpression)).GetMemberAccessList(), ConfigurationSource.Explicit).Metadata);

        /// <summary>
        ///     Sets the properties that make up the primary key for this owned entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the primary key. </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        public new virtual KeyBuilder<TDependentEntity> HasKey([NotNull] params string[] propertyNames)
            => new KeyBuilder<TDependentEntity>(
                DependentEntityType.Builder.PrimaryKey(
                    Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit).Metadata);

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
        public virtual PropertyBuilder<TProperty> Property<TProperty>(
            [NotNull] Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => UpdateBuilder(
                () => new PropertyBuilder<TProperty>(
                    DependentEntityType.Builder.Property(
                        Check.NotNull(propertyExpression, nameof(propertyExpression)).GetMemberAccess(),
                        ConfigurationSource.Explicit).Metadata));

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure an existing navigation property
        ///         from the owned type to its owner. It is an error for the navigation property
        ///         not to exist.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNavigation"> The target entity type. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the navigation property to be configured (
        ///     <c>blog => blog.Posts</c>).
        /// </param>
        /// <returns> An object that can be used to configure the navigation property. </returns>
        public virtual NavigationBuilder<TDependentEntity, TNavigation> Navigation<TNavigation>(
            [NotNull] Expression<Func<TDependentEntity, TNavigation>> navigationExpression)
            where TNavigation : class
            => new NavigationBuilder<TDependentEntity, TNavigation>(
                DependentEntityType.Builder.Navigation(
                    Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure an existing navigation property
        ///         from the owned type to its owner. It is an error for the navigation property
        ///         not to exist.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNavigation"> The target entity type. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the navigation property to be configured (
        ///     <c>blog => blog.Posts</c>).
        /// </param>
        /// <returns> An object that can be used to configure the navigation property. </returns>
        public virtual NavigationBuilder<TDependentEntity, TNavigation> Navigation<TNavigation>(
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNavigation>>> navigationExpression)
            where TNavigation : class
            => new NavigationBuilder<TDependentEntity, TNavigation>(
                DependentEntityType.Builder.Navigation(
                    Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     or navigations from the owned entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of the property to be removed from the entity type. </param>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> Ignore([NotNull] string propertyName)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.Ignore(propertyName);

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     or navigations from the owned entity type that were added by convention.
        /// </summary>
        /// <param name="propertyExpression">
        ///     A lambda expression representing the property to be ignored
        ///     (<c>blog => blog.Url</c>).
        /// </param>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(
            [NotNull] Expression<Func<TDependentEntity, object>> propertyExpression)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)
                base.Ignore(
                    Check.NotNull(propertyExpression, nameof(propertyExpression))
                        .GetMemberAccess().GetSimpleMemberName());

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
        public virtual IndexBuilder<TEntity> HasIndex([NotNull] Expression<Func<TDependentEntity, object>> indexExpression)
            => new IndexBuilder<TEntity>(
                DependentEntityType.Builder.HasIndex(
                        Check.NotNull(indexExpression, nameof(indexExpression)).GetMemberAccessList(), ConfigurationSource.Explicit)
                    .Metadata);

        /// <summary>
        ///     Configures an index on the specified properties. If there is an existing index on the given
        ///     set of properties, then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the index. </param>
        /// <returns> An object that can be used to configure the index. </returns>
        public new virtual IndexBuilder<TEntity> HasIndex([NotNull] params string[] propertyNames)
            => new IndexBuilder<TEntity>(
                DependentEntityType.Builder.HasIndex(
                    Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit).Metadata);

        /// <summary>
        ///     <para>
        ///         Configures the relationship to the owner.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        /// </summary>
        /// <param name="ownerReference">
        ///     The name of the reference navigation property pointing to the owner.
        ///     If null or not specified, there is no navigation property pointing to the owner.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public new virtual OwnershipBuilder<TEntity, TDependentEntity> WithOwner(
            [CanBeNull] string ownerReference = null)
        {
            Check.NullButNotEmpty(ownerReference, nameof(ownerReference));

            return new OwnershipBuilder<TEntity, TDependentEntity>(
                PrincipalEntityType,
                DependentEntityType,
                Builder.HasNavigation(
                    ownerReference,
                    pointsToPrincipal: true,
                    ConfigurationSource.Explicit).Metadata);
        }

        /// <summary>
        ///     <para>
        ///         Configures the relationship to the owner.
        ///     </para>
        ///     <para>
        ///         Note that calling this method with no parameters will explicitly configure this side
        ///         of the relationship to use no navigation property, even if such a property exists on the
        ///         entity type. If the navigation property is to be used, then it must be specified.
        ///     </para>
        /// </summary>
        /// <param name="referenceExpression">
        ///     A lambda expression representing the reference navigation property pointing to the owner
        ///     (<c>blog => blog.BlogInfo</c>). If no property is specified, the relationship will be
        ///     configured without a navigation property pointing to the owner.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual OwnershipBuilder<TEntity, TDependentEntity> WithOwner(
            [CanBeNull] Expression<Func<TDependentEntity, TEntity>> referenceExpression)
            => new OwnershipBuilder<TEntity, TDependentEntity>(
                PrincipalEntityType,
                DependentEntityType,
                Builder.HasNavigation(
                    referenceExpression?.GetMemberAccess(),
                    pointsToPrincipal: true,
                    ConfigurationSource.Explicit).Metadata);

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] string navigationName)
            where TNewDependentEntity : class
            => OwnsOneBuilder<TNewDependentEntity>(
                new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName)
            where TNewDependentEntity : class
            => OwnsOneBuilder<TNewDependentEntity>(
                new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), typeof(TNewDependentEntity)),
                new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
            where TNewDependentEntity : class
            => OwnsOneBuilder<TNewDependentEntity>(
                new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
            where TNewDependentEntity : class
            => OwnsOneBuilder<TNewDependentEntity>(
                new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), typeof(TNewDependentEntity)),
                new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(
                OwnsOneBuilder<TNewDependentEntity>(
                    new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                    new MemberIdentity(navigationName)));
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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.OwnsOne(ownedTypeName, navigationName, buildAction);

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="ownedType"> The CLR type of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne(
            [NotNull] string ownedTypeName,
            [NotNull] Type ownedType,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.OwnsOne(ownedTypeName, ownedType, navigationName, buildAction);

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne(
            [NotNull] Type ownedType,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.OwnsOne(ownedType, navigationName, buildAction);

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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(
                OwnsOneBuilder<TNewDependentEntity>(
                    new TypeIdentity(ownedTypeName, typeof(TNewDependentEntity)), new MemberIdentity(navigationName)));
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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(
                OwnsOneBuilder<TNewDependentEntity>(
                    new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                    new MemberIdentity(navigationExpression.GetMemberAccess())));
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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(
                OwnsOneBuilder<TNewDependentEntity>(
                    new TypeIdentity(ownedTypeName, typeof(TNewDependentEntity)),
                    new MemberIdentity(navigationExpression.GetMemberAccess())));
            return this;
        }

        private OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOneBuilder<TNewDependentEntity>(
            TypeIdentity ownedType,
            MemberIdentity navigation)
            where TNewDependentEntity : class
        {
            InternalForeignKeyBuilder relationship;
            using (var batch = DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                relationship = DependentEntityType.Builder.HasOwnership(ownedType, navigation, ConfigurationSource.Explicit);
                relationship.IsUnique(true, ConfigurationSource.Explicit);
                relationship = (InternalForeignKeyBuilder)batch.Run(relationship.Metadata).Builder;
            }

            return new OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(relationship.Metadata);
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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string navigationName)
            where TNewDependentEntity : class
            => OwnsManyBuilder<TNewDependentEntity>(
                new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName)
            where TNewDependentEntity : class
            => OwnsManyBuilder<TNewDependentEntity>(
                new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), typeof(TNewDependentEntity)),
                new MemberIdentity(Check.NotEmpty(navigationName, nameof(navigationName))));

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
            where TNewDependentEntity : class
            => OwnsManyBuilder<TNewDependentEntity>(
                new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
            where TNewDependentEntity : class
            => OwnsManyBuilder<TNewDependentEntity>(
                new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName)), typeof(TNewDependentEntity)),
                new MemberIdentity(Check.NotNull(navigationExpression, nameof(navigationExpression)).GetMemberAccess()));

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction(
                    OwnsManyBuilder<TNewDependentEntity>(
                        new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                        new MemberIdentity(navigationName)));
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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.OwnsMany(ownedTypeName, navigationName, buildAction);

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany(
            [NotNull] Type ownedType,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.OwnsMany(ownedType, navigationName, buildAction);

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="ownedType"> The CLR type of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany(
            [NotNull] string ownedTypeName,
            [NotNull] Type ownedType,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.OwnsMany(ownedTypeName, ownedType, navigationName, buildAction);

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
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction(
                    OwnsManyBuilder<TNewDependentEntity>(
                        new TypeIdentity(ownedTypeName, typeof(TNewDependentEntity)), new MemberIdentity(navigationName)));
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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction(
                    OwnsManyBuilder<TNewDependentEntity>(
                        new TypeIdentity(typeof(TNewDependentEntity), (Model)Metadata.DeclaringEntityType.Model),
                        new MemberIdentity(navigationExpression.GetMemberAccess())));
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
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="M:WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <typeparam name="TNewDependentEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationExpression">
        ///     A lambda expression representing the reference navigation property on this entity type that represents
        ///     the relationship (<c>customer => customer.Address</c>).
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            [NotNull] string ownedTypeName,
            [NotNull] Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
            [NotNull] Action<OwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
        {
            Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
            Check.NotNull(navigationExpression, nameof(navigationExpression));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction(
                    OwnsManyBuilder<TNewDependentEntity>(
                        new TypeIdentity(ownedTypeName, typeof(TNewDependentEntity)),
                        new MemberIdentity(navigationExpression.GetMemberAccess())));
                return this;
            }
        }

        private OwnedNavigationBuilder<TDependentEntity, TNewRelatedEntity> OwnsManyBuilder<TNewRelatedEntity>(
            TypeIdentity ownedType,
            MemberIdentity navigation)
            where TNewRelatedEntity : class
        {
            InternalForeignKeyBuilder relationship;
            using (var batch = DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                relationship = DependentEntityType.Builder.HasOwnership(ownedType, navigation, ConfigurationSource.Explicit);
                relationship.IsUnique(false, ConfigurationSource.Explicit);
                relationship = (InternalForeignKeyBuilder)batch.Run(relationship.Metadata).Builder;
            }

            return new OwnedNavigationBuilder<TDependentEntity, TNewRelatedEntity>(relationship.Metadata);
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
                DependentEntityType.Builder.HasRelationship(
                    relatedEntityType, navigationName, ConfigurationSource.Explicit,
                    targetIsPrincipal: DependentEntityType == relatedEntityType ? true : (bool?)null).Metadata);
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
            var navigation = navigationExpression?.GetMemberAccess();
            var relatedEntityType = FindRelatedEntityType(typeof(TNewRelatedEntity), navigation?.GetSimpleMemberName());

            return new ReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity>(
                DependentEntityType,
                relatedEntityType,
                navigation,
                DependentEntityType.Builder.HasRelationship(
                    relatedEntityType, navigation, ConfigurationSource.Explicit,
                    targetIsPrincipal: DependentEntityType == relatedEntityType ? true : (bool?)null).Metadata);
        }

        /// <summary>
        ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
        ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> HasChangeTrackingStrategy(
            ChangeTrackingStrategy changeTrackingStrategy)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.HasChangeTrackingStrategy(changeTrackingStrategy);

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
        public new virtual OwnedNavigationBuilder<TEntity, TDependentEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (OwnedNavigationBuilder<TEntity, TDependentEntity>)base.UsePropertyAccessMode(propertyAccessMode);

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
        ///     A collection of seed data.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public virtual DataBuilder<TDependentEntity> HasData([NotNull] IEnumerable<TDependentEntity> data)
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

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     A collection of seed data represented by anonymous types.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public new virtual DataBuilder<TDependentEntity> HasData([NotNull] IEnumerable<object> data)
        {
            base.HasData(data);

            return new DataBuilder<TDependentEntity>();
        }
    }
}
