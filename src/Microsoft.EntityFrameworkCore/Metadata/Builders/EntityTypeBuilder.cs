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
    ///     <para>
    ///         Provides a simple API for configuring an <see cref="EntityType" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class EntityTypeBuilder : IInfrastructure<IMutableModel>, IInfrastructure<InternalEntityTypeBuilder>
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="EntityTypeBuilder" /> class to configure a given
        ///         entity type.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="builder"> Internal builder for the entity type being configured. </param>
        public EntityTypeBuilder([NotNull] InternalEntityTypeBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        /// <summary>
        ///     Creates a new builder based on the provided internal builder. This can be overridden by derived builders
        ///     so that logic inherited from this base class will create instances of the derived builder.
        /// </summary>
        /// <param name="builder"> The internal builder to create the new builder from. </param>
        /// <returns> The newly created builder. </returns>
        protected virtual EntityTypeBuilder New([NotNull] InternalEntityTypeBuilder builder)
            => new EntityTypeBuilder(builder);

        private InternalEntityTypeBuilder Builder { get; }

        /// <summary>
        ///     Gets the internal builder being used to configure the entity type.
        /// </summary>
        InternalEntityTypeBuilder IInfrastructure<InternalEntityTypeBuilder>.Instance => Builder;

        /// <summary>
        ///     The entity type being configured.
        /// </summary>
        public virtual IMutableEntityType Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that the entity type belongs to.
        /// </summary>
        IMutableModel IInfrastructure<IMutableModel>.Instance => Builder.ModelBuilder.Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <param name="name"> The name of the base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder HasBaseType([CanBeNull] string name)
            => New(Builder.HasBaseType(name, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the base type of this entity in an inheritance hierarchy.
        /// </summary>
        /// <param name="entityType"> The base type. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual EntityTypeBuilder HasBaseType([CanBeNull] Type entityType)
            => New(Builder.HasBaseType(entityType, ConfigurationSource.Explicit));

        /// <summary>
        ///     Sets the properties that make up the primary key for this entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the primary key. </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        public virtual KeyBuilder HasKey([NotNull] params string[] propertyNames)
            => new KeyBuilder(Builder.PrimaryKey(Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit));

        /// <summary>
        ///     Creates a new unique constraint for this entity type if one does not already exist over the specified
        ///     properties.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the unique constraint. </param>
        /// <returns> An object that can be used to configure the unique constraint. </returns>
        public virtual KeyBuilder HasAlternateKey([NotNull] params string[] propertyNames)
            => new KeyBuilder(Builder.HasKey(Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit));

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
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] string propertyName)
            => new PropertyBuilder<TProperty>(Builder.Property(
                Check.NotEmpty(propertyName, nameof(propertyName)),
                typeof(TProperty),
                ConfigurationSource.Explicit));

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
        /// <param name="propertyType"> The type of the property to be configured. </param>
        /// <param name="propertyName"> The name of the property to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertyBuilder Property([NotNull] Type propertyType, [NotNull] string propertyName)
            => new PropertyBuilder(Builder.Property(
                Check.NotEmpty(propertyName, nameof(propertyName)),
                Check.NotNull(propertyType, nameof(propertyType)),
                ConfigurationSource.Explicit));

        /// <summary>
        ///     Excludes the given property from the entity type. This method is typically used to remove properties
        ///     from the entity type that were added by convention.
        /// </summary>
        /// <param name="propertyName"> The name of then property to be removed from the entity type. </param>
        public virtual EntityTypeBuilder Ignore([NotNull] string propertyName)
        {
            Check.NotEmpty(propertyName, nameof(propertyName));

            Builder.Ignore(propertyName, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures an index on the specified properties. If there is an existing index on the given
        ///     set of properties, then the existing index will be returned for configuration.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the index. </param>
        /// <returns> An object that can be used to configure the index. </returns>
        public virtual IndexBuilder HasIndex([NotNull] params string[] propertyNames)
            => new IndexBuilder(Builder.HasIndex(Check.NotEmpty(propertyNames, nameof(propertyNames)), ConfigurationSource.Explicit));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
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

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

            return new ReferenceNavigationBuilder(
                Builder.Metadata,
                relatedEntityType,
                navigationName,
                HasOneBuilder(relatedEntityType, navigationName));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a reference that points
        ///         to a single instance of the other type in the relationship.
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
            [CanBeNull] string navigationName = null)
        {
            Check.NotEmpty(relatedTypeName, nameof(relatedTypeName));
            Check.NullButNotEmpty(navigationName, nameof(navigationName));

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit).Metadata;

            return new ReferenceNavigationBuilder(
                Builder.Metadata,
                relatedEntityType,
                navigationName,
                HasOneBuilder(relatedEntityType, navigationName));
        }

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="CollectionNavigationBuilder.WithOne" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="relatedType"> The entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the collection navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual CollectionNavigationBuilder HasMany(
            [NotNull] Type relatedType,
            [CanBeNull] string navigationName = null)
            => new CollectionNavigationBuilder(HasManyBuilder(
                Builder.ModelBuilder.Entity(Check.NotNull(relatedType, nameof(relatedType)), ConfigurationSource.Explicit).Metadata,
                Check.NullButNotEmpty(navigationName, nameof(navigationName))));

        /// <summary>
        ///     <para>
        ///         Configures a relationship where this entity type has a collection that contains
        ///         instances of the other type in the relationship.
        ///     </para>
        ///     <para>
        ///         After calling this method, you should chain a call to
        ///         <see cref="CollectionNavigationBuilder.WithOne" />
        ///         to fully configure the relationship. Calling just this method without the chained call will not
        ///         produce a valid relationship.
        ///     </para>
        /// </summary>
        /// <param name="relatedTypeName"> The name of the entity type that this relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the collection navigation property on this entity type that represents the relationship. If
        ///     no property is specified, the relationship will be configured without a navigation property on this
        ///     end.
        /// </param>
        /// <returns> An object that can be used to configure the relationship. </returns>
        public virtual CollectionNavigationBuilder HasMany(
            [NotNull] string relatedTypeName,
            [CanBeNull] string navigationName = null)
            => new CollectionNavigationBuilder(HasManyBuilder(
                Builder.ModelBuilder.Entity(Check.NotEmpty(relatedTypeName, nameof(relatedTypeName)), ConfigurationSource.Explicit).Metadata,
                Check.NullButNotEmpty(navigationName, nameof(navigationName))));

        /// <summary>
        ///     Creates a relationship builder for a relationship that has a reference navigation property on this entity.
        /// </summary>
        /// <param name="relatedEntityType"> The entity type that the relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the reference navigation property on this entity. If null is passed, then a relationship with no navigation
        ///     property is created.
        /// </param>
        /// <returns> The newly created builder. </returns>
        protected virtual InternalRelationshipBuilder HasOneBuilder(
            [NotNull] EntityType relatedEntityType, [CanBeNull] string navigationName)
            => HasOneBuilder(relatedEntityType, PropertyIdentity.Create(navigationName));

        /// <summary>
        ///     Creates a relationship builder for a relationship that has a reference navigation property on this entity.
        /// </summary>
        /// <param name="relatedEntityType"> The entity type that the relationship targets. </param>
        /// <param name="navigationProperty">
        ///     The reference navigation property on this entity. If null is passed, then a relationship with no navigation
        ///     property is created.
        /// </param>
        /// <returns> The newly created builder. </returns>
        protected virtual InternalRelationshipBuilder HasOneBuilder(
            [NotNull] EntityType relatedEntityType, [CanBeNull] PropertyInfo navigationProperty)
            => HasOneBuilder(relatedEntityType, PropertyIdentity.Create(navigationProperty));

        private InternalRelationshipBuilder HasOneBuilder(EntityType relatedEntityType, PropertyIdentity navigation)
        {
            var navigationProperty = navigation.Property;
            if (relatedEntityType == Metadata)
            {
                var relationship = Builder.Relationship(relatedEntityType.Builder, ConfigurationSource.Explicit)
                    .RelatedEntityTypes(relatedEntityType, Builder.Metadata, ConfigurationSource.Explicit);
                return navigationProperty != null
                    ? relationship.DependentToPrincipal(navigationProperty, ConfigurationSource.Explicit)
                    : relationship.DependentToPrincipal(navigation.Name, ConfigurationSource.Explicit);
            }
            else
            {
                return navigationProperty != null
                    ? Builder.Navigation(relatedEntityType.Builder, navigationProperty, ConfigurationSource.Explicit)
                    : Builder.Navigation(relatedEntityType.Builder, navigation.Name, ConfigurationSource.Explicit);
            }
        }

        /// <summary>
        ///     Creates a relationship builder for a relationship that has a collection navigation property on this entity.
        /// </summary>
        /// <param name="relatedEntityType"> The entity type that the relationship targets. </param>
        /// <param name="navigationName">
        ///     The name of the collection navigation property on this entity. If null is passed, then a relationship with no navigation
        ///     property is created.
        /// </param>
        /// <returns> The newly created builder. </returns>
        protected virtual InternalRelationshipBuilder HasManyBuilder(
            [NotNull] EntityType relatedEntityType, [CanBeNull] string navigationName)
            => relatedEntityType.Builder
                .Relationship(Builder, ConfigurationSource.Explicit)
                .IsUnique(false, ConfigurationSource.Explicit)
                .PrincipalToDependent(navigationName, ConfigurationSource.Explicit);

        /// <summary>
        ///     Creates a relationship builder for a relationship that has a collection navigation property on this entity.
        /// </summary>
        /// <param name="relatedEntityType"> The entity type that the relationship targets. </param>
        /// <param name="navigationProperty">
        ///     The collection navigation property on this entity. If null is passed, then a relationship with no navigation
        ///     property is created.
        /// </param>
        /// <returns> The newly created builder. </returns>
        protected virtual InternalRelationshipBuilder HasManyBuilder(
            [NotNull] EntityType relatedEntityType, [CanBeNull] PropertyInfo navigationProperty)
            => relatedEntityType.Builder
                .Relationship(Builder, ConfigurationSource.Explicit)
                .IsUnique(false, ConfigurationSource.Explicit)
                .PrincipalToDependent(navigationProperty, ConfigurationSource.Explicit);

        public virtual EntityTypeBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
        {
            Builder.Metadata.ChangeTrackingStrategy = changeTrackingStrategy;

            return this;
        }
    }
}
