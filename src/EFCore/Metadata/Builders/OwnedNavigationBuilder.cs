// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class OwnedNavigationBuilder : IInfrastructure<InternalEntityTypeBuilder>
    {
        private InternalRelationshipBuilder _builder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public OwnedNavigationBuilder(
            [NotNull] EntityType principalEntityType,
            [NotNull] EntityType dependentEntityType,
            [NotNull] InternalRelationshipBuilder builder)
        {
            PrincipalEntityType = principalEntityType;
            DependentEntityType = dependentEntityType;
            _builder = builder;
        }

        /// <summary>
        ///     Gets the principal entity type used to configure this relationship.
        /// </summary>
        protected virtual EntityType PrincipalEntityType { get; }

        /// <summary>
        ///     Gets the dependent entity type used to configure this relationship.
        /// </summary>
        protected virtual EntityType DependentEntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalRelationshipBuilder Builder
        {
            get
            {
                if (_builder.Metadata.Builder == null)
                {
                    _builder = PrincipalEntityType.FindNavigation(_builder.Metadata.PrincipalToDependent.Name)?.ForeignKey.Builder;
                }

                return _builder;
            }

            private set => _builder = value;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual T UpdateBuilder<T>([NotNull] Func<T> configure)
        {
            IConventionForeignKey foreignKey = _builder.Metadata;
            var result = DependentEntityType.Model.ConventionDispatcher.Run(configure, ref foreignKey);
            if (foreignKey != null)
            {
                _builder = ((ForeignKey)foreignKey).Builder;
            }

            return result;
        }

        /// <summary>
        ///     Gets the internal builder being used to configure the owned entity type.
        /// </summary>
        InternalEntityTypeBuilder IInfrastructure<InternalEntityTypeBuilder>.Instance => DependentEntityType.Builder;

        /// <summary>
        ///     The foreign key that represents this ownership.
        /// </summary>
        public virtual IMutableForeignKey Metadata => Builder.Metadata;

        /// <summary>
        ///     The owned entity type being configured.
        /// </summary>
        public virtual IMutableEntityType OwnedEntityType => DependentEntityType;

        /// <summary>
        ///     Adds or updates an annotation on the owned entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual OwnedNavigationBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            DependentEntityType.Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the properties that make up the primary key for this owned entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the primary key. </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        public virtual KeyBuilder HasKey([NotNull] params string[] propertyNames)
            => new KeyBuilder(
                DependentEntityType.Builder.PrimaryKey(
                    Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit).Metadata);

        /// <summary>
        ///     <para>
        ///         Returns an object that can be used to configure a property of the owned entity type.
        ///         If no property with the given name exists, then a new property will be added.
        ///     </para>
        ///     <para>
        ///         When adding a new property with this overload the property name must match the
        ///         name of a CLR property or field on the entity type. This overload cannot be used to
        ///         add a new shadow state property.
        ///     </para>
        /// </summary>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder Property([NotNull] string propertyName)
            => UpdateBuilder(
                () => new PropertyBuilder(
                    DependentEntityType.Builder.Property(
                        Check.NotEmpty(propertyName, nameof(propertyName)),
                        ConfigurationSource.Explicit).Metadata));

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
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] string propertyName)
            => UpdateBuilder(
                () => new PropertyBuilder<TProperty>(
                    DependentEntityType.Builder.Property(
                        typeof(TProperty),
                        Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit).Metadata));

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
        /// <param name="propertyType"> The type of the property to be configured. </param>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string propertyName)
            => new PropertyBuilder(
                DependentEntityType.Builder.Property(
                    Check.NotNull(propertyType, nameof(propertyType)),
                    Check.NotEmpty(propertyName, nameof(propertyName)), ConfigurationSource.Explicit).Metadata);

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     or navigations from the owned entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of the property to be removed from the entity type. </param>
        public virtual OwnedNavigationBuilder Ignore([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            DependentEntityType.Builder.Ignore(propertyName, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures an index on the specified properties. If there is an existing index on the given
        ///     set of properties, then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the index. </param>
        /// <returns> An object that can be used to configure the index. </returns>
        public virtual IndexBuilder HasIndex([NotNull] params string[] propertyNames)
            => new IndexBuilder(
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
        public virtual OwnershipBuilder WithOwner(
            [CanBeNull] string ownerReference = null)
        {
            Check.NullButNotEmpty(ownerReference, nameof(ownerReference));

            return new OwnershipBuilder(
                PrincipalEntityType,
                DependentEntityType,
                Builder.HasNavigation(
                    ownerReference,
                    pointsToPrincipal: true,
                    ConfigurationSource.Explicit).Metadata);
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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual OwnedNavigationBuilder OwnsOne(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName)
            => OwnsOneBuilder(
                new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName))),
                Check.NotEmpty(navigationName, nameof(navigationName)));

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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual OwnedNavigationBuilder OwnsOne(
            [NotNull] Type ownedType,
            [NotNull] string navigationName)
            => OwnsOneBuilder(
                new TypeIdentity(Check.NotNull(ownedType, nameof(ownedType)), (Model)OwnedEntityType.Model),
                Check.NotEmpty(navigationName, nameof(navigationName)));

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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder OwnsOne(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
        {
            Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction.Invoke(OwnsOneBuilder(new TypeIdentity(ownedTypeName), navigationName));
                return this;
            }
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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder OwnsOne(
            [NotNull] Type ownedType,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
        {
            Check.NotNull(ownedType, nameof(ownedType));
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction.Invoke(OwnsOneBuilder(new TypeIdentity(ownedType, (Model)OwnedEntityType.Model), navigationName));
                return this;
            }
        }

        private OwnedNavigationBuilder OwnsOneBuilder(in TypeIdentity ownedType, string navigationName)
        {
            InternalRelationshipBuilder relationship;
            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                relationship = ownedType.Type == null
                    ? DependentEntityType.Builder.HasOwnership(ownedType.Name, navigationName, ConfigurationSource.Explicit)
                    : DependentEntityType.Builder.HasOwnership(ownedType.Type, navigationName, ConfigurationSource.Explicit);
                relationship.IsUnique(true, ConfigurationSource.Explicit);
            }

            return new OwnedNavigationBuilder(
                DependentEntityType,
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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder OwnsMany(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName)
            => OwnsManyBuilder(
                new TypeIdentity(Check.NotEmpty(ownedTypeName, nameof(ownedTypeName))),
                Check.NotEmpty(navigationName, nameof(navigationName)));

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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the owned type and the relationship. </returns>
        public virtual OwnedNavigationBuilder OwnsMany(
            [NotNull] Type ownedType,
            [NotNull] string navigationName)
            => OwnsManyBuilder(
                new TypeIdentity(Check.NotNull(ownedType, nameof(ownedType)), DependentEntityType.Model),
                Check.NotEmpty(navigationName, nameof(navigationName)));

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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder OwnsMany(
            [NotNull] string ownedTypeName,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
        {
            Check.NotEmpty(ownedTypeName, nameof(ownedTypeName));
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction.Invoke(OwnsManyBuilder(new TypeIdentity(ownedTypeName), navigationName));
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
        ///         <see cref="WithOwner" /> to fully configure the relationship.
        ///     </para>
        /// </summary>
        /// <param name="ownedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <param name="buildAction"> An action that performs configuration of the owned type and the relationship. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual OwnedNavigationBuilder OwnsMany(
            [NotNull] Type ownedType,
            [NotNull] string navigationName,
            [NotNull] Action<OwnedNavigationBuilder> buildAction)
        {
            Check.NotNull(ownedType, nameof(ownedType));
            Check.NotEmpty(navigationName, nameof(navigationName));
            Check.NotNull(buildAction, nameof(buildAction));

            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                buildAction.Invoke(OwnsManyBuilder(new TypeIdentity(ownedType, DependentEntityType.Model), navigationName));
                return this;
            }
        }

        private OwnedNavigationBuilder OwnsManyBuilder(in TypeIdentity ownedType, string navigationName)
        {
            InternalRelationshipBuilder relationship;
            using (DependentEntityType.Model.ConventionDispatcher.DelayConventions())
            {
                relationship = ownedType.Type == null
                    ? DependentEntityType.Builder.HasOwnership(ownedType.Name, navigationName, ConfigurationSource.Explicit)
                    : DependentEntityType.Builder.HasOwnership(ownedType.Type, navigationName, ConfigurationSource.Explicit);
                relationship.IsUnique(false, ConfigurationSource.Explicit);
            }

            return new OwnedNavigationBuilder(
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
        ///         <see cref="ReferenceNavigationBuilder.WithMany" />
        ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
        ///         the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="relatedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder HasOne(
            [NotNull] string relatedTypeName,
            [CanBeNull] string navigationName)
        {
            Check.NotEmpty(relatedTypeName, nameof(relatedTypeName));
            Check.NullButNotEmpty(navigationName, nameof(navigationName));

            var relatedEntityType = FindRelatedEntityType(relatedTypeName, navigationName);

            return new ReferenceNavigationBuilder(
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
        ///         <see cref="ReferenceNavigationBuilder.WithMany" />
        ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
        ///         the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder HasOne(
            [NotNull] string navigationName)
        {
            Check.NotEmpty(navigationName, nameof(navigationName));

            return OwnedEntityType.ClrType == null
                ? HasOne(navigationName, null) // Path only used by pre 3.0 snapshots
                : HasOne(OwnedEntityType.GetNavigationMemberInfo(navigationName).GetMemberType(), navigationName);
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
        ///         <see cref="ReferenceNavigationBuilder.WithMany" />
        ///         or <see cref="ReferenceNavigationBuilder.WithOne" /> to fully configure
        ///         the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="relatedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual ReferenceNavigationBuilder HasOne(
            [NotNull] Type relatedType,
            [CanBeNull] string navigationName = null)
        {
            Check.NotNull(relatedType, nameof(relatedType));
            Check.NullButNotEmpty(navigationName, nameof(navigationName));

            var relatedEntityType = FindRelatedEntityType(relatedType, navigationName);

            return new ReferenceNavigationBuilder(
                DependentEntityType,
                relatedEntityType,
                navigationName,
                DependentEntityType.Builder.HasRelationship(
                    relatedEntityType, navigationName, ConfigurationSource.Explicit,
                    targetIsPrincipal: DependentEntityType == relatedEntityType ? true : (bool?)null).Metadata);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual EntityType FindRelatedEntityType([NotNull] string relatedTypeName, [CanBeNull] string navigationName)
        {
            var relatedEntityType = DependentEntityType.FindInDefinitionPath(relatedTypeName);
            if (relatedEntityType != null)
            {
                return relatedEntityType;
            }

            var model = DependentEntityType.Model;
            if (navigationName != null)
            {
                relatedEntityType = model.FindEntityType(relatedTypeName, navigationName, DependentEntityType);
            }

            if (relatedEntityType == null
                && model.GetProductVersion()?.StartsWith("2.", StringComparison.Ordinal) == true)
            {
                var owner = DependentEntityType.FindOwnership().PrincipalEntityType;
                if (owner.Name == relatedTypeName)
                {
                    relatedEntityType = owner;
                }
            }

            return relatedEntityType ?? Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit).Metadata;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual EntityType FindRelatedEntityType([NotNull] Type relatedType, [CanBeNull] string navigationName)
        {
            var relatedEntityType = DependentEntityType.FindInDefinitionPath(relatedType);
            if (relatedEntityType != null)
            {
                return relatedEntityType;
            }

            if (navigationName != null)
            {
                relatedEntityType = Builder.ModelBuilder.Metadata.FindEntityType(relatedType, navigationName, DependentEntityType);
            }

            return relatedEntityType ?? DependentEntityType.Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;
        }

        /// <summary>
        ///     Configures the <see cref="ChangeTrackingStrategy" /> to be used for this entity type.
        ///     This strategy indicates how the context detects changes to properties for an instance of the entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual OwnedNavigationBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
        {
            DependentEntityType.Builder.HasChangeTrackingStrategy(changeTrackingStrategy, ConfigurationSource.Explicit);

            return this;
        }

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
        public virtual OwnedNavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        {
            DependentEntityType.Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures this entity to have seed data. It is used to generate data motion migrations.
        /// </summary>
        /// <param name="data">
        ///     An array of seed data represented by anonymous types.
        /// </param>
        /// <returns> An object that can be used to configure the model data. </returns>
        public virtual DataBuilder HasData([NotNull] params object[] data)
        {
            Check.NotNull(data, nameof(data));

            OwnedEntityType.AddData(data);

            return new DataBuilder();
        }
    }
}
