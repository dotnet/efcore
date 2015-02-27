// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring a <see cref="Model" /> that defines the shape of your
    ///         entities and how they map to the data store.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="ModelBuilder" /> to construct a model for a context by overriding
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> or creating a <see cref="Model" />
    ///         externally
    ///         and setting is on a <see cref="DbContextOptions" /> instance that is passed to the context
    ///         constructor.
    ///     </para>
    /// </summary>
    public class ModelBuilder : IModelBuilder<ModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        // TODO: Configure property facets, foreign keys & navigation properties
        // Issue #213

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class with an empty model.
        ///     The builder will not use any conventions and the entire model must be explicitly configured.
        ///     To specify conventions to be used, use a constructor that accepts a <see cref="ConventionSet" />.
        /// </summary>
        public ModelBuilder()
            : this(new Model())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model. The builder will not use any conventions and the
        ///     entire model must be explicitly configured. To specify conventions to be used,
        ///     use a constructor that accepts a <see cref="ConventionSet" />.
        /// </summary>
        /// <param name="model"> The model to be configured. </param>
        public ModelBuilder([NotNull] Model model)
            : this(model, new ConventionSet())
        {
            Check.NotNull(model, nameof(model));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model and apply a set of conventions.
        /// </summary>
        /// <param name="model"> The model to be configured. </param>
        /// <param name="conventions"> The conventions to be applied to the model. </param>
        public ModelBuilder([NotNull] Model model, [NotNull] ConventionSet conventions)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(conventions, nameof(conventions));

            _builder = new InternalModelBuilder(model, conventions);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model. The builder will not use any conventions and the
        ///     entire model must be explicitly configured. To specify conventions to be used,
        ///     use a constructor that accepts a <see cref="ConventionSet" />.
        /// </summary>
        /// <param name="internalBuilder"> The internal builder being used to configure the model. </param>
        protected internal ModelBuilder([NotNull] InternalModelBuilder internalBuilder)
        {
            Check.NotNull(internalBuilder, nameof(internalBuilder));

            _builder = internalBuilder;
        }

        /// <summary>
        ///     The model being configured.
        /// </summary>
        public virtual Model Metadata => Builder.Metadata;

        /// <summary>
        ///     The model being configured.
        /// </summary>
        public virtual Model Model => Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the model. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual ModelBuilder Annotation(string annotation, string value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotEmpty(value, nameof(value));

            _builder.Annotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     The internal builder being used to configure this model.
        /// </summary>
        protected virtual InternalModelBuilder Builder => _builder;

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If the entity type is not already part of the model, it will be added to the model.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityBuilder<TEntity> Entity<TEntity>() where TEntity : class
        {
            return new EntityBuilder<TEntity>(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If the entity type is not already part of the model, it will be added to the model.
        /// </summary>
        /// <param name="entityType"> The entity type to be configured. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityBuilder Entity([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return new EntityBuilder(Builder.Entity(entityType, ConfigurationSource.Explicit));
        }

        // TODO Remove this constructor as part of #748
        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return new EntityBuilder(Builder.Entity(name, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     <para>
        ///         Performs configuration of a given entity type in the model. If the entity type is not already part
        ///         of the model, it will be added to the model.
        ///     </para>
        ///     <para>
        ///         This overload allows configuration of the entity type to be done in line in the method call rather
        ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
        ///         configuration at the model level to be chained after configuration for the entity type.
        ///     </para>
        /// </summary>
        /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
        /// <param name="entityBuilder"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same builder instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity<TEntity>([NotNull] Action<EntityBuilder<TEntity>> entityBuilder) where TEntity : class
        {
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity<TEntity>());

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Performs configuration of a given entity type in the model. If the entity type is not already part
        ///         of the model, it will be added to the model.
        ///     </para>
        ///     <para>
        ///         This overload allows configuration of the entity type to be done in line in the method call rather
        ///         than being chained after a call to <see cref="Entity{TEntity}()" />. This allows additional
        ///         configuration at the model level to be chained after configuration for the entity type.
        ///     </para>
        /// </summary>
        /// <param name="entityType"> The entity type to be configured. </param>
        /// <param name="entityBuilder"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same builder instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity([NotNull] Type entityType, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(entityType));

            return this;
        }

        // TODO Remove this constructor as part of #748
        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(entityBuilder, nameof(entityBuilder));

            entityBuilder(Entity(name));

            return this;
        }

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <typeparam name="TEntity"> The  entity type to be removed from the model. </typeparam>
        public virtual void Ignore<TEntity>() where TEntity : class
        {
            Ignore(typeof(TEntity));
        }

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <param name="entityType"> The entity type to be removed from the model. </param>
        public virtual void Ignore([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            Builder.Ignore(entityType, ConfigurationSource.Explicit);
        }

        // TODO Remove this constructor as part of #748
        public virtual void Ignore([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Builder.Ignore(name, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     <para>
        ///         Provides a simple API for configuring an <see cref="EntityType" />.
        ///     </para>
        ///     <para>
        ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
        ///         and it is not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        public class EntityBuilder : IEntityBuilder<EntityBuilder>
        {
            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref="EntityBuilder" /> class to configure a given entity
            ///         type.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            /// <param name="builder"> Internal builder for the entity type being configured. </param>
            public EntityBuilder([NotNull] InternalEntityBuilder builder)
            {
                Check.NotNull(builder, nameof(builder));

                Builder = builder;
            }

            /// <summary>
            ///     The internal builder being used to configure the entity type.
            /// </summary>
            protected virtual InternalEntityBuilder Builder { get; }

            /// <summary>
            ///     The entity type being configured.
            /// </summary>
            public virtual EntityType Metadata => Builder.Metadata;

            /// <summary>
            ///     The model that the entity type belongs to.
            /// </summary>
            Model IMetadataBuilder<EntityType, EntityBuilder>.Model => Builder.ModelBuilder.Metadata;

            /// <summary>
            ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
            ///     <paramref name="annotation" /> already exists it's value will be updated.
            /// </summary>
            /// <param name="annotation"> The key of the annotation to be added or updated. </param>
            /// <param name="value"> The value to be stored in the annotation. </param>
            /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
            public virtual EntityBuilder Annotation(string annotation, string value)
            {
                Check.NotEmpty(annotation, nameof(annotation));
                Check.NotEmpty(value, nameof(value));

                Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                return this;
            }

            /// <summary>
            ///     Sets the properties that make up the primary key for this entity type.
            /// </summary>
            /// <param name="propertyNames"> The names of the properties that make up the primary key. </param>
            /// <returns> An object that can be used to configure the primary key. </returns>
            public virtual KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new KeyBuilder(Builder.PrimaryKey(propertyNames, ConfigurationSource.Explicit));
            }

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
            public virtual PropertyBuilder Property<TProperty>([NotNull] string propertyName)
            {
                Check.NotEmpty(propertyName, nameof(propertyName));

                return Property(typeof(TProperty), propertyName);
            }

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
            {
                Check.NotNull(propertyType, nameof(propertyType));
                Check.NotEmpty(propertyName, nameof(propertyName));

                return new PropertyBuilder(Builder.Property(propertyType, propertyName, ConfigurationSource.Explicit));
            }

            /// <summary>
            ///     Excludes the given property from the entity type. This method is typically used to remove properties
            ///     from the entity type that were added by convention.
            /// </summary>
            /// <param name="propertyName"> The name of then property to be removed from the entity type. </param>
            public virtual void Ignore([NotNull] string propertyName)
            {
                Check.NotEmpty(propertyName, nameof(propertyName));

                Builder.Ignore(propertyName, ConfigurationSource.Explicit);
            }

            /// <summary>
            ///     Configures an index on the specified properties. If there is an existing index on the given
            ///     set of properties, then the existing index will be returned for configuration.
            /// </summary>
            /// <param name="propertyNames"> The names of the properties that make up the index. </param>
            /// <returns> An object that can be used to configure the index. </returns>
            public virtual IndexBuilder Index([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, nameof(propertyNames));

                return new IndexBuilder(Builder.Index(propertyNames, ConfigurationSource.Explicit));
            }

            /// <summary>
            ///     <para>
            ///         Configures a relationship where this entity type has a reference that points
            ///         to a single instance of the other type in the relationship.
            ///     </para>
            ///     <para>
            ///         After calling this method, you should chain a call to
            ///         <see cref="ReferenceNavigationBuilder.WithMany(string)" />
            ///         or <see cref="ReferenceNavigationBuilder.WithOne(string)" /> to fully configure
            ///         the relationship. Calling just this method without the chained call will not
            ///         produce a valid relationship.
            ///     </para>
            /// </summary>
            /// <param name="relatedType"> The entity type that this relationship targets. </param>
            /// <param name="referenceName">
            ///     The name of the reference navigation property on this entity type that represents the relationship. If
            ///     no property is specified, the relationship will be configured without a navigation property on this
            ///     end.
            /// </param>
            /// <returns> An object that can be used to configure the relationship. </returns>
            public virtual ReferenceNavigationBuilder HasOne(
                [NotNull] Type relatedType,
                [CanBeNull] string referenceName = null)
            {
                Check.NotNull(relatedType, nameof(relatedType));

                var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

                return new ReferenceNavigationBuilder(
                    relatedEntityType,
                    referenceName,
                    HasOneBuilder(relatedEntityType, referenceName));
            }

            /// <summary>
            ///     <para>
            ///         Configures a relationship where this entity type has a collection that contains
            ///         instances of the other type in the relationship.
            ///     </para>
            ///     <para>
            ///         After calling this method, you should chain a call to
            ///         <see cref="CollectionNavigationBuilder.WithOne(string)" />
            ///         to fully configure the relationship. Calling just this method without the chained call will not
            ///         produce a valid relationship.
            ///     </para>
            /// </summary>
            /// <param name="relatedType"> The entity type that this relationship targets. </param>
            /// <param name="collectionName">
            ///     The name of the collection navigation property on this entity type that represents the relationship. If
            ///     no property is specified, the relationship will be configured without a navigation property on this
            ///     end.
            /// </param>
            /// <returns> An object that can be used to configure the relationship. </returns>
            public virtual CollectionNavigationBuilder HasMany(
                [NotNull] Type relatedType,
                [CanBeNull] string collectionName = null)
            {
                Check.NotNull(relatedType, nameof(relatedType));

                var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

                return new CollectionNavigationBuilder(HasManyBuilder(relatedEntityType, collectionName));
            }

            /// <summary>
            ///     <para>
            ///         Configures a relationship where this entity type has a reference that points
            ///         to a single instance of the other type in the relationship.
            ///     </para>
            ///     <para>
            ///         After calling this method, you should chain a call to
            ///         <see cref="ReferenceNavigationBuilder.WithMany(string)" />
            ///         or <see cref="ReferenceNavigationBuilder.WithOne(string)" /> to fully configure
            ///         the relationship. Calling just this method without the chained call will not
            ///         produce a valid relationship.
            ///     </para>
            /// </summary>
            /// <param name="relatedEntityTypeName"> The name of the entity type that this relationship targets. </param>
            /// <param name="referenceName">
            ///     The name of the reference navigation property on this entity type that represents the relationship. If
            ///     no  property is specified, the relationship will be configured without a navigation property on this
            ///     end.
            /// </param>
            /// <returns> An object that can be used to configure the relationship. </returns>
            public virtual ReferenceNavigationBuilder HasOne(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string referenceName = null)
            {
                Check.NotEmpty(relatedEntityTypeName, nameof(relatedEntityTypeName));

                var relatedEntityType = Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName);

                return new ReferenceNavigationBuilder(
                    relatedEntityType,
                    referenceName,
                    HasOneBuilder(relatedEntityType, referenceName));
            }

            /// <summary>
            ///     <para>
            ///         Configures a relationship where this entity type has a collection that contains
            ///         instances of the other type in the relationship.
            ///     </para>
            ///     <para>
            ///         After calling this method, you should chain a call to
            ///         <see cref="CollectionNavigationBuilder.WithOne(string)" />
            ///         to fully configure the relationship. Calling just this method without the chained call will not
            ///         produce a valid relationship.
            ///     </para>
            /// </summary>
            /// <param name="relatedEntityTypeName"> The name of the entity type that this relationship targets. </param>
            /// <param name="collectionName">
            ///     The name of the collection navigation property on this entity type that represents the relationship. If
            ///     no property is specified, the relationship will be configured without a navigation property on this
            ///     end.
            /// </param>
            /// <returns> An object that can be used to configure the relationship. </returns>
            public virtual CollectionNavigationBuilder HasMany(
                [NotNull] string relatedEntityTypeName,
                [CanBeNull] string collectionName = null)
            {
                Check.NotEmpty(relatedEntityTypeName, nameof(relatedEntityTypeName));

                var relatedEntityType = Builder.ModelBuilder.Metadata.GetEntityType(relatedEntityTypeName);

                return new CollectionNavigationBuilder(HasManyBuilder(relatedEntityType, collectionName));
            }

            protected InternalRelationshipBuilder HasOneBuilder(EntityType relatedEntityType, string referenceName)
            {
                return Builder.Relationship(
                    relatedEntityType,
                    Metadata,
                    navigationToPrincipalName: referenceName ?? "",
                    navigationToDependentName: null,
                    configurationSource: ConfigurationSource.Explicit,
                    strictPrincipal: relatedEntityType == Metadata);
            }

            protected InternalRelationshipBuilder HasManyBuilder(EntityType relatedEntityType, string collectionName)
            {
                return Builder.Relationship(
                    Metadata,
                    relatedEntityType,
                    navigationToPrincipalName: null,
                    navigationToDependentName: collectionName ?? "",
                    configurationSource: ConfigurationSource.Explicit,
                    isUnique: false);
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a <see cref="Key" />.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class KeyBuilder : IKeyBuilder<KeyBuilder>
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="KeyBuilder" /> class to configure a given key.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> Internal builder for the key being configured. </param>
                public KeyBuilder([NotNull] InternalKeyBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     The internal builder being used to configure the key.
                /// </summary>
                protected virtual InternalKeyBuilder Builder { get; }

                /// <summary>
                ///     The key being configured.
                /// </summary>
                public virtual Key Metadata => Builder.Metadata;

                /// <summary>
                ///     The model that the key belongs to.
                /// </summary>
                Model IMetadataBuilder<Key, KeyBuilder>.Model => Builder.ModelBuilder.Metadata;

                /// <summary>
                ///     Adds or updates an annotation on the key. If an annotation with the key specified in
                ///     <paramref name="annotation" />
                ///     already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual KeyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a <see cref="Property" />.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class PropertyBuilder : IPropertyBuilder<PropertyBuilder>
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="PropertyBuilder" /> class to configure a given
                ///         property.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> Internal builder for the property being configured. </param>
                public PropertyBuilder([NotNull] InternalPropertyBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     The internal builder being used to configure the property.
                /// </summary>
                protected virtual InternalPropertyBuilder Builder { get; }

                /// <summary>
                ///     The property being configured.
                /// </summary>
                public virtual Property Metadata => Builder.Metadata;

                /// <summary>
                ///     The model that the property belongs to.
                /// </summary>
                Model IMetadataBuilder<Property, PropertyBuilder>.Model => Builder.ModelBuilder.Metadata;

                /// <summary>
                ///     Adds or updates an annotation on the property. If an annotation with the key specified in
                ///     <paramref name="annotation" /> already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether this property must have a value assigned or whether null is a valid value.
                ///     A property can only be configured as non-required if it is based on a CLR type that can be
                ///     assigned null.
                /// </summary>
                /// <param name="isRequired"> A value indicating whether the property is required. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder Required(bool isRequired = true)
                {
                    Builder.Required(isRequired, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures the maximum length of data that can be stored in this property.
                ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
                /// </summary>
                /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder MaxLength(int maxLength)
                {
                    Builder.MaxLength(maxLength, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether this property should be used as a concurrency token. When a property is configured
                ///     as a concurrency token the value in the data store will be checked when an instance of this entity type
                ///     is updated or deleted during <see cref="DbContext.SaveChanges" /> to ensure it has not changed since
                ///     the instance was retrieved from the data store. If it has changed, an exception will be thrown and the
                ///     changes will not be applied to the data store.
                /// </summary>
                /// <param name="isConcurrencyToken"> A value indicating whether this property is a concurrency token. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder ConcurrencyToken(bool isConcurrencyToken = true)
                {
                    Builder.ConcurrencyToken(isConcurrencyToken, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether this is a shadow state property. A shadow state property is one that does not have a
                ///     corresponding property in the entity class. The current value for the property is stored in
                ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
                /// </summary>
                /// <param name="isShadowProperty"> A value indicating whether this is a shadow state property. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder Shadow(bool isShadowProperty = true)
                {
                    Builder.Shadow(isShadowProperty, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether a value is generated for this property when a new instance of the entity type
                ///     is added to a context. Data stores will typically register an appropriate
                ///     <see cref="ValueGenerator" /> to handle generating values. This functionality is typically
                ///     used for key values and is switched on by convention.
                /// </summary>
                /// <param name="generateValue"> A value indicating whether a value should be generated. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder GenerateValueOnAdd(bool generateValue = true)
                {
                    Builder.GenerateValueOnAdd(generateValue, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether a value is generated for this property by the data store every time an
                ///     instance of this entity type is saved (initial add and any subsequent updates).
                /// </summary>
                /// <param name="computed"> A value indicating whether a value is generated by the data store. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder StoreComputed(bool computed = true)
                {
                    Builder.StoreComputed(computed, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether a default value is generated for this property by the store when an instance
                ///     of this entity type is saved and no value has been set.
                /// </summary>
                /// <param name="useDefault">
                ///     A value indicating whether a default value is generated by the data store when no value is set.
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual PropertyBuilder UseStoreDefault(bool useDefault = true)
                {
                    Builder.UseStoreDefault(useDefault, ConfigurationSource.Explicit);

                    return this;
                }
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring an <see cref="Index" />.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class IndexBuilder : IIndexBuilder<IndexBuilder>
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="IndexBuilder" /> class to configure a given index.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> Internal builder for the index being configured. </param>
                public IndexBuilder([NotNull] InternalIndexBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     The internal builder being used to configure the index.
                /// </summary>
                protected virtual InternalIndexBuilder Builder { get; }

                /// <summary>
                ///     The index being configured.
                /// </summary>
                public virtual Index Metadata => Builder.Metadata;

                /// <summary>
                ///     The model that the index belongs to.
                /// </summary>
                Model IMetadataBuilder<Index, IndexBuilder>.Model => Builder.ModelBuilder.Metadata;

                /// <summary>
                ///     Adds or updates an annotation on the index. If an annotation with the key specified in
                ///     <paramref name="annotation" />
                ///     already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual IndexBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     Configures whether this index is unique (i.e. the value(s) for each instance must be unique).
                /// </summary>
                /// <param name="isUnique"> A value indicating whether this index is unique. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual IndexBuilder IsUnique(bool isUnique = true)
                {
                    Builder.IsUnique(isUnique, ConfigurationSource.Explicit);

                    return this;
                }
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a relationship where configuration began on
            ///         an end of the relationship with a reference that points to an instance of another entity type.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class ReferenceNavigationBuilder
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="ReferenceNavigationBuilder" /> class.
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
                {
                    Check.NotNull(relatedEntityType, nameof(relatedEntityType));
                    Check.NotNull(builder, nameof(builder));

                    RelatedEntityType = relatedEntityType;
                    ReferenceName = referenceName;
                    Builder = builder;
                }

                /// <summary>
                ///     Gets or sets the name of the reference navigation property on the end of the relationship that
                ///     configuration began on. If null, there is no navigation property on this end of the relationship.
                /// </summary>
                protected string ReferenceName { get; set; }

                /// <summary>
                ///     Gets or sets the entity type that the reference points to.
                /// </summary>
                protected EntityType RelatedEntityType { get; set; }

                /// <summary>
                ///     The foreign key that represents this relationship.
                /// </summary>
                public virtual ForeignKey Metadata => Builder.Metadata;

                /// <summary>
                ///     Gets the internal builder being used to configure the relationship.
                /// </summary>
                protected virtual InternalRelationshipBuilder Builder { get; }

                /// <summary>
                ///     Configures this as a one-to-many relationship.
                /// </summary>
                /// <param name="collection">
                ///     The name of the collection navigation property on the other end of this relationship.
                ///     If null, there is no navigation property on the other end of the relationship.
                /// </param>
                /// <returns> An object to further configure the relationship. </returns>
                public virtual ManyToOneBuilder WithMany([CanBeNull] string collection = null) => new ManyToOneBuilder(WithManyBuilder(collection));

                /// <summary>
                ///     Returns the internal builder to be used when <see cref="WithMany" /> is called.
                /// </summary>
                /// <param name="collection">
                ///     The name of the collection navigation property on the other end of this relationship.
                ///     If null, there is no navigation property on the other end of the relationship.
                /// </param>
                /// <returns> The internal builder to further configure the relationship. </returns>
                protected InternalRelationshipBuilder WithManyBuilder(string collection)
                {
                    var needToInvert = Metadata.ReferencedEntityType != RelatedEntityType;
                    Debug.Assert((needToInvert && Metadata.EntityType == RelatedEntityType)
                                 || Metadata.ReferencedEntityType == RelatedEntityType);

                    var builder = Builder;
                    if (needToInvert)
                    {
                        builder = builder.Invert(ConfigurationSource.Explicit);
                    }

                    if (((IForeignKey)Metadata).IsUnique)
                    {
                        builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
                    }

                    builder = builder.Unique(false, ConfigurationSource.Explicit);

                    return builder.NavigationToDependent(collection, ConfigurationSource.Explicit, strictPreferExisting: true);
                }

                /// <summary>
                ///     Configures this as a one-to-one relationship.
                /// </summary>
                /// <param name="inverseReference">
                ///     The name of the reference navigation property on the other end of this relationship.
                ///     If null, there is no navigation property on the other end of the relationship.
                /// </param>
                /// <returns> An object to further configure the relationship. </returns>
                public virtual OneToOneBuilder WithOne([CanBeNull] string inverseReference = null) => new OneToOneBuilder(WithOneBuilder(inverseReference));

                /// <summary>
                ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
                /// </summary>
                /// <param name="inverseReferenceName">
                ///     The name of the reference navigation property on the other end of this relationship.
                ///     If null, there is no navigation property on the other end of the relationship.
                /// </param>
                /// <returns> The internal builder to further configure the relationship. </returns>
                protected InternalRelationshipBuilder WithOneBuilder(string inverseReferenceName)
                {
                    var isSelfReferencing = Metadata.EntityType == Metadata.ReferencedEntityType;
                    var inverseToPrincipal = !isSelfReferencing
                                             && Metadata.EntityType == RelatedEntityType
                                             && (string.IsNullOrEmpty(ReferenceName) || Metadata.GetNavigationToDependent()?.Name == ReferenceName);

                    Debug.Assert(inverseToPrincipal
                                 || (Metadata.ReferencedEntityType == RelatedEntityType
                                     && (string.IsNullOrEmpty(ReferenceName) || Metadata.GetNavigationToPrincipal()?.Name == ReferenceName)));

                    var builder = Builder;
                    if (!((IForeignKey)Metadata).IsUnique)
                    {
                        Debug.Assert(!inverseToPrincipal);

                        builder = builder.NavigationToDependent(null, ConfigurationSource.Explicit);
                    }

                    builder = builder.Unique(true, ConfigurationSource.Explicit);

                    builder = inverseToPrincipal
                        ? builder.NavigationToPrincipal(inverseReferenceName, ConfigurationSource.Explicit, strictPreferExisting: false)
                        : builder.NavigationToDependent(inverseReferenceName, ConfigurationSource.Explicit, strictPreferExisting: false);

                    return builder;
                }
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a relationship where configuration began on
            ///         an end of the relationship with a collection that contains instances of another entity type.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class CollectionNavigationBuilder
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="CollectionNavigationBuilder" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> The internal builder being used to configure the relationship. </param>
                public CollectionNavigationBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     The foreign key that represents this relationship.
                /// </summary>
                public virtual ForeignKey Metadata => Builder.Metadata;

                /// <summary>
                ///     Gets the internal builder being used to configure the relationship.
                /// </summary>
                protected virtual InternalRelationshipBuilder Builder { get; }

                /// <summary>
                ///     Configures this as a one-to-many relationship.
                /// </summary>
                /// <param name="reference">
                ///     The name of the reference navigation property on the other end of this relationship.
                ///     If null, there is no navigation property on the other end of the relationship.
                /// </param>
                /// <returns> An object to further configure the relationship. </returns>
                public virtual OneToManyBuilder WithOne([CanBeNull] string reference = null) => new OneToManyBuilder(WithOneBuilder(reference));

                /// <summary>
                ///     Returns the internal builder to be used when <see cref="WithOne" /> is called.
                /// </summary>
                /// <param name="reference">
                ///     The name of the reference navigation property on the other end of this relationship.
                ///     If null, there is no navigation property on the other end of the relationship.
                /// </param>
                /// <returns> The internal builder to further configure the relationship. </returns>
                protected InternalRelationshipBuilder WithOneBuilder(string reference) => Builder.NavigationToPrincipal(
                    reference,
                    ConfigurationSource.Explicit,
                    strictPreferExisting: true);
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a one-to-many relationship.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class OneToManyBuilder : IOneToManyBuilder<OneToManyBuilder>
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="OneToManyBuilder" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> The internal builder being used to configure this relationship. </param>
                public OneToManyBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     The foreign key that represents this relationship.
                /// </summary>
                public virtual ForeignKey Metadata => Builder.Metadata;

                /// <summary>
                ///     The model that this relationship belongs to.
                /// </summary>
                Model IMetadataBuilder<ForeignKey, OneToManyBuilder>.Model => Builder.ModelBuilder.Metadata;

                /// <summary>
                ///     Gets the internal builder being used to configure this relationship.
                /// </summary>
                protected virtual InternalRelationshipBuilder Builder { get; }

                /// <summary>
                ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
                ///     <paramref name="annotation" />
                ///     already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToManyBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
                }

                /// <summary>
                ///     <para>
                ///         Configures the property(s) to use as the foreign key for this relationship.
                ///     </para>
                ///     <para>
                ///         If the specified property name(s) do not exist on the entity type then a new shadow state
                ///         property(s) will be added to serve as the foreign key. A shadow state property is one that does not
                ///         have a corresponding property in the entity class. The current value for the  property is stored in
                ///         the <see cref="ChangeTracker" /> rather than being stored in instances
                ///         of the entity class.
                ///     </para>
                ///     <para>
                ///         If <see cref="ReferencedKey" /> is not specified, then an attempt will be made to match
                ///         the data type and order of foreign key properties against the primary key of the principal
                ///         entity type. If they do not match, new shadow state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///     </para>
                /// </summary>
                /// <param name="foreignKeyPropertyNames">
                ///     The name(s) of the foreign key property(s).
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToManyBuilder ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToManyBuilder(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToManyBuilder ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToManyBuilder(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
                ///     be assigned null).
                /// </summary>
                /// <param name="required"> A value indicating whether this is a required relationship. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToManyBuilder Required(bool required = true)
                {
                    return new OneToManyBuilder(Builder.Required(required, ConfigurationSource.Explicit));
                }
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a one-to-many relationship.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            public class ManyToOneBuilder : IManyToOneBuilder<ManyToOneBuilder>
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="ManyToOneBuilder" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> The internal builder being used to configure this relationship. </param>
                public ManyToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     Gets the internal builder being used to configure this relationship.
                /// </summary>
                protected virtual InternalRelationshipBuilder Builder { get; }

                /// <summary>
                ///     The foreign key that represents this relationship.
                /// </summary>
                public virtual ForeignKey Metadata => Builder.Metadata;

                /// <summary>
                ///     The model that this relationship belongs to.
                /// </summary>
                Model IMetadataBuilder<ForeignKey, ManyToOneBuilder>.Model => Builder.ModelBuilder.Metadata;

                /// <summary>
                ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
                ///     <paramref name="annotation" /> already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual ManyToOneBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
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
                ///         If <see cref="ReferencedKey" /> is not specified, then an attempt will be made to match
                ///         the data type and order of foreign key properties against the primary key of the principal
                ///         entity type. If they do not match, new shadow state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///     </para>
                /// </summary>
                /// <param name="foreignKeyPropertyNames">
                ///     The name(s) of the foreign key property(s).
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual ManyToOneBuilder ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new ManyToOneBuilder(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual ManyToOneBuilder ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new ManyToOneBuilder(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
                ///     be assigned null).
                /// </summary>
                /// <param name="required"> A value indicating whether this is a required relationship. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual ManyToOneBuilder Required(bool required = true)
                    => new ManyToOneBuilder(Builder.Required(required, ConfigurationSource.Explicit));
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a one-to-one relationship.
            ///     </para>
            ///     <para>
            ///         If multiple reference key properties are specified, the order of reference key properties should
            ///         match the order that the primary key or unique index properties were configured on the principal
            ///         entity type.
            ///     </para>
            /// </summary>
            public class OneToOneBuilder : IOneToOneBuilder<OneToOneBuilder>
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="OneToOneBuilder" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> The internal builder being used to configure this relationship. </param>
                public OneToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                {
                    Check.NotNull(builder, nameof(builder));

                    Builder = builder;
                }

                /// <summary>
                ///     Gets the internal builder being used to configure this relationship.
                /// </summary>
                protected virtual InternalRelationshipBuilder Builder { get; }

                /// <summary>
                ///     The foreign key that represents this relationship.
                /// </summary>
                public virtual ForeignKey Metadata => Builder.Metadata;

                /// <summary>
                ///     The model that this relationship belongs to.
                /// </summary>
                Model IMetadataBuilder<ForeignKey, OneToOneBuilder>.Model => Builder.ModelBuilder.Metadata;

                /// <summary>
                ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
                ///     <paramref name="annotation" /> already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder Annotation(string annotation, string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    Builder.Annotation(annotation, value, ConfigurationSource.Explicit);

                    return this;
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
                ///         If <see cref="ReferencedKey(Type, string[])" /> is not specified, then an attempt will be made to
                ///         match the data type and order of foreign key properties against the primary key of the principal
                ///         entity type. If they do not match, new shadow state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///     </para>
                /// </summary>
                /// <param name="dependentEntityType">
                ///     The entity type that is the dependent in this relationship (the type that has the foreign key
                ///     properties).
                /// </param>
                /// <param name="foreignKeyPropertyNames">
                ///     The name(s) of the foreign key property(s).
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] Type dependentEntityType,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityType, nameof(dependentEntityType));
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToOneBuilder(Builder.ForeignKey(dependentEntityType, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="principalEntityType">
                ///     The entity type that is the principal in this relationship (the type
                ///     that has the reference key properties).
                /// </param>
                /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] Type principalEntityType,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityType, nameof(principalEntityType));
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToOneBuilder(Builder.ReferencedKey(principalEntityType, keyPropertyNames, ConfigurationSource.Explicit));
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
                ///         If <see cref="ReferencedKey(Type, string[])" /> is not specified, then an attempt will be made to
                ///         match the data type and order of foreign key properties against the primary key of the principal
                ///         entity type. If they do not match, new shadow state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///     </para>
                /// </summary>
                /// <param name="dependentEntityTypeName">
                ///     The name of the entity type that is the dependent in this relationship (the type that has the foreign
                ///     key properties).
                /// </param>
                /// <param name="foreignKeyPropertyNames">
                ///     The name(s) of the foreign key property(s).
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder ForeignKey(
                    [NotNull] string dependentEntityTypeName,
                    [NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(dependentEntityTypeName, nameof(dependentEntityTypeName));
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToOneBuilder(Builder.ForeignKey(dependentEntityTypeName, foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="principalEntityTypeName">
                ///     The name of the entity type that is the principal in this relationship (the type
                ///     that has the reference key properties).
                /// </param>
                /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder ReferencedKey(
                    [NotNull] string principalEntityTypeName,
                    [NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(principalEntityTypeName, nameof(principalEntityTypeName));
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToOneBuilder(Builder.ReferencedKey(principalEntityTypeName, keyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     <para>
                ///         Configures the property(s) to use as the foreign key for this relationship.
                ///     </para>
                ///     <para>
                ///         If <see cref="ReferencedKey{TPrincipalEntity}" />
                ///         is not specified, then an attempt will be made to match the data type and order of foreign key
                ///         properties against the primary key of the principal entity type. If they do not match, new shadow
                ///         state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///         A shadow state property is one that does not have a corresponding property in the entity class. The
                ///         current value for the property is stored in the <see cref="ChangeTracker" /> rather than being
                ///         stored in instances of the entity class.
                ///     </para>
                /// </summary>
                /// <typeparam name="TDependentEntity">
                ///     The entity type that is the dependent in this relationship. That is, the type
                ///     that has the foreign key properties.
                /// </typeparam>
                /// <param name="foreignKeyExpression">
                ///     <para>
                ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
                ///     </para>
                ///     <para>
                ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
                ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
                ///         corresponding keys in <see cref="ReferencedKey{TPrincipalEntity}" />.
                ///     </para>
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder ForeignKey<TDependentEntity>(
                    [NotNull] Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                    return new OneToOneBuilder(
                        Builder.ForeignKey(typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <typeparam name="TPrincipalEntity">
                ///     The entity type that is the principal in this relationship. That is, the type
                ///     that has the reference key properties.
                /// </typeparam>
                /// <param name="keyExpression">
                ///     <para>
                ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
                ///     </para>
                ///     <para>
                ///         If the referenced key is made up of multiple properties then specify an anonymous type including
                ///         the properties (<c>t => new { t.Id1, t.Id2 }</c>).
                ///     </para>
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder ReferencedKey<TPrincipalEntity>(
                    [NotNull] Expression<Func<TPrincipalEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, nameof(keyExpression));

                    return new OneToOneBuilder(Builder.ReferencedKey(typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
                ///     be assigned null).
                /// </summary>
                /// <param name="required"> A value indicating whether this is a required relationship. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToOneBuilder Required(bool required = true)
                {
                    return new OneToOneBuilder(Builder.Required(required, ConfigurationSource.Explicit));
                }
            }
        }

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
        public class EntityBuilder<TEntity> : EntityBuilder, IEntityBuilder<TEntity, EntityBuilder<TEntity>>
            where TEntity : class
        {
            /// <summary>
            ///     <para>
            ///         Initializes a new instance of the <see cref="EntityBuilder{TEntity}" /> class to configure a given
            ///         entity type.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            /// <param name="builder"> Internal builder for the entity type being configured. </param>
            public EntityBuilder([NotNull] InternalEntityBuilder builder)
                : base(builder)
            {
            }

            /// <summary>
            ///     Adds or updates an annotation on the entity type. If an annotation with the key specified in
            ///     <paramref name="annotation" /> already exists it's value will be updated.
            /// </summary>
            /// <param name="annotation"> The key of the annotation to be added or updated. </param>
            /// <param name="value"> The value to be stored in the annotation. </param>
            /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
            public new virtual EntityBuilder<TEntity> Annotation(string annotation, string value)
            {
                base.Annotation(annotation, value);

                return this;
            }

            /// <summary>
            ///     The model that the entity type belongs to.
            /// </summary>
            Model IMetadataBuilder<EntityType, EntityBuilder<TEntity>>.Model => Builder.ModelBuilder.Metadata;

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
            ///     Returns an object that can be used to configure a property of the entity type.
            ///     If the specified property is not already part of the model, it will be added.
            /// </summary>
            /// <param name="propertyExpression">
            ///     A lambda expression representing the property to be configured (
            ///     <c>t => t.Property1</c>).
            /// </param>
            /// <returns> An object that can be used to configure the property. </returns>
            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, nameof(propertyExpression));

                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new PropertyBuilder(Builder.Property(propertyInfo, ConfigurationSource.Explicit));
            }

            /// <summary>
            ///     Excludes the given property from the entity type. This method is typically used to remove properties
            ///     from the entity type that were added by convention.
            /// </summary>
            /// <param name="propertyExpression">
            ///     A lambda expression representing the property to be ignored
            ///     (<c>t => t.Property1</c>).
            /// </param>
            public virtual void Ignore([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                Check.NotNull(propertyExpression, nameof(propertyExpression));

                var propertyName = propertyExpression.GetPropertyAccess().Name;
                Builder.Ignore(propertyName, ConfigurationSource.Explicit);
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
            ///             cref="ReferenceNavigationBuilder{TRelatedEntity}.WithMany(Expression{Func{TRelatedEntity, IEnumerable{TEntity}}})" />
            ///         or
            ///         <see
            ///             cref="ReferenceNavigationBuilder{TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity, TEntity}})" />
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
            public virtual ReferenceNavigationBuilder<TRelatedEntity> HasOne<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, TRelatedEntity>> reference = null)
            {
                var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
                var referenceName = reference?.GetPropertyAccess().Name;

                return new ReferenceNavigationBuilder<TRelatedEntity>(
                    relatedEntityType,
                    referenceName,
                    HasOneBuilder(relatedEntityType, referenceName));
            }

            /// <summary>
            ///     <para>
            ///         Configures a relationship where this entity type has a collection that contains
            ///         instances of the other type in the relationship.
            ///     </para>
            ///     <para>
            ///         After calling this method, you should chain a call to
            ///         <see
            ///             cref="CollectionNavigationBuilder{TRelatedEntity}.WithOne(Expression{Func{TRelatedEntity, TEntity}})" />
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
            public virtual CollectionNavigationBuilder<TRelatedEntity> HasMany<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
            {
                var relatedEntityType = Builder.ModelBuilder.Entity(typeof(TRelatedEntity), ConfigurationSource.Explicit).Metadata;
                var collectionName = collection?.GetPropertyAccess().Name;

                return new CollectionNavigationBuilder<TRelatedEntity>(
                    collectionName,
                    HasManyBuilder(relatedEntityType, collectionName));
            }

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
            /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
            public class ReferenceNavigationBuilder<TRelatedEntity> : ReferenceNavigationBuilder
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="ReferenceNavigationBuilder{TRelatedEntity}" /> class.
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
                public virtual ManyToOneBuilder<TRelatedEntity> WithMany(
                    [CanBeNull] Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
                    => new ManyToOneBuilder<TRelatedEntity>(WithManyBuilder(collection?.GetPropertyAccess().Name));

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

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a relationship where configuration began on
            ///         an end of the relationship with a collection that contains instances of another entity type.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
            public class CollectionNavigationBuilder<TRelatedEntity> : CollectionNavigationBuilder
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="CollectionNavigationBuilder{TRelatedEntity}" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="collection">
                ///     The name of the collection navigation property on the end of the relationship that configuration began
                ///     on. If null, there is no navigation property on this end of the relationship.
                /// </param>
                /// <param name="builder"> The internal builder being used to configure the relationship. </param>
                public CollectionNavigationBuilder(
                    [CanBeNull] string collection,
                    [NotNull] InternalRelationshipBuilder builder)
                    : base(builder)
                {
                }

                /// <summary>
                ///     Configures this as a one-to-many relationship.
                /// </summary>
                /// <param name="reference">
                ///     A lambda expression representing the reference navigation property on the other end of this
                ///     relationship (<c>t => t.Reference1</c>). If no property is specified, the relationship will be
                ///     configured without a navigation property on the other end of the relationship.
                /// </param>
                /// <returns> An object to further configure the relationship. </returns>
                public virtual OneToManyBuilder<TRelatedEntity> WithOne([CanBeNull] Expression<Func<TRelatedEntity, TEntity>> reference = null)
                    => new OneToManyBuilder<TRelatedEntity>(WithOneBuilder(reference?.GetPropertyAccess().Name));
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a one-to-many relationship.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
            public class OneToManyBuilder<TRelatedEntity> : OneToManyBuilder
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="OneToManyBuilder{TRelatedEntity}" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> The internal builder being used to configure this relationship. </param>
                public OneToManyBuilder([NotNull] InternalRelationshipBuilder builder)
                    : base(builder)
                {
                }

                /// <summary>
                ///     <para>
                ///         Configures the property(s) to use as the foreign key for this relationship.
                ///     </para>
                ///     <para>
                ///         If <see cref="ReferencedKey(Expression{Func{TEntity, object}})" /> is not specified, then an
                ///         attempt will be made to match the data type and order of foreign key properties against the primary
                ///         key of
                ///         the principal entity type. If they do not match, new shadow state properties that form a unique
                ///         index
                ///         will be added to the principal entity type to serve as the reference key.
                ///         A shadow state property is one that does not have a corresponding property in the entity class. The
                ///         current value for the property is stored in the <see cref="ChangeTracker" /> rather than being
                ///         stored in instances of the entity class.
                ///     </para>
                /// </summary>
                /// <param name="foreignKeyExpression">
                ///     <para>
                ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
                ///     </para>
                ///     <para>
                ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
                ///         properties (<c>t => new { t.Id1, t.Id2 }</c>). The order specified should match the order of
                ///         corresponding keys in <see cref="ReferencedKey(Expression{Func{TEntity, object}})" />.
                ///     </para>
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToManyBuilder<TRelatedEntity> ForeignKey(
                    [NotNull] Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="keyExpression">
                ///     <para>
                ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
                ///     </para>
                ///     <para>
                ///         If the referenced key is made up of multiple properties then specify an anonymous type including
                ///         the properties (<c>t => new { t.Id1, t.Id2 }</c>).
                ///     </para>
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual OneToManyBuilder<TRelatedEntity> ReferencedKey(
                    [NotNull] Expression<Func<TEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, nameof(keyExpression));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ReferencedKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
                ///     <paramref name="annotation" /> already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual OneToManyBuilder<TRelatedEntity> Annotation([NotNull] string annotation, [NotNull] string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    return (OneToManyBuilder<TRelatedEntity>)base.Annotation(annotation, value);
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
                ///         If <see cref="ReferencedKey(string[])" /> is not specified, then an attempt will be made to match
                ///         the data type and order of foreign key properties against the primary key of the principal
                ///         entity type. If they do not match, new shadow state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///     </para>
                /// </summary>
                /// <param name="foreignKeyPropertyNames">
                ///     The name(s) of the foreign key property(s).
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual OneToManyBuilder<TRelatedEntity> ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual OneToManyBuilder<TRelatedEntity> ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new OneToManyBuilder<TRelatedEntity>(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
                ///     be assigned null).
                /// </summary>
                /// <param name="required"> A value indicating whether this is a required relationship. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual OneToManyBuilder<TRelatedEntity> Required(bool required = true)
                    => new OneToManyBuilder<TRelatedEntity>(Builder.Required(required, ConfigurationSource.Explicit));
            }

            /// <summary>
            ///     <para>
            ///         Provides a simple API for configuring a one-to-many relationship.
            ///     </para>
            ///     <para>
            ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
            ///         and it is not designed to be directly constructed in your application code.
            ///     </para>
            /// </summary>
            /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
            public class ManyToOneBuilder<TRelatedEntity> : ManyToOneBuilder
            {
                /// <summary>
                ///     <para>
                ///         Initializes a new instance of the <see cref="ManyToOneBuilder{TRelatedEntity}" /> class.
                ///     </para>
                ///     <para>
                ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
                ///         and it is not designed to be directly constructed in your application code.
                ///     </para>
                /// </summary>
                /// <param name="builder"> The internal builder being used to configure this relationship. </param>
                public ManyToOneBuilder([NotNull] InternalRelationshipBuilder builder)
                    : base(builder)
                {
                }

                /// <summary>
                ///     <para>
                ///         Configures the property(s) to use as the foreign key for this relationship.
                ///     </para>
                ///     <para>
                ///         If <see cref="ReferencedKey(Expression{Func{TRelatedEntity, object}})" /> is not specified, then an
                ///         attempt will be made to match the data type and order of foreign key properties against the primary
                ///         key of the principal entity type. If they do not match, new shadow state properties that form a
                ///         unique
                ///         index will be added to the principal entity type to serve as the reference key.
                ///         A shadow state property is one that does not have a corresponding property in the entity class. The
                ///         current value for the property is stored in the <see cref="ChangeTracker" /> rather than being
                ///         stored in instances of the entity class.
                ///     </para>
                /// </summary>
                /// <param name="foreignKeyExpression">
                ///     <para>
                ///         A lambda expression representing the foreign key property(s) (<c>t => t.Id1</c>).
                ///     </para>
                ///     <para>
                ///         If the foreign key is made up of multiple properties then specify an anonymous type including the
                ///         properties (<c>t => new { t.Id1, t.Id2 }</c>).
                ///         The order specified should match the order of corresponding keys in
                ///         <see cref="ReferencedKey(Expression{Func{TRelatedEntity, object}})" />.
                ///     </para>
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual ManyToOneBuilder<TRelatedEntity> ForeignKey(
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression)
                {
                    Check.NotNull(foreignKeyExpression, nameof(foreignKeyExpression));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="keyExpression">
                ///     <para>
                ///         A lambda expression representing the reference key property(s) (<c>t => t.Id</c>).
                ///     </para>
                ///     <para>
                ///         If the referenced key is made up of multiple properties then specify an anonymous type including
                ///         the properties (<c>t => new { t.Id1, t.Id2 }</c>).
                ///     </para>
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public virtual ManyToOneBuilder<TRelatedEntity> ReferencedKey(
                    [NotNull] Expression<Func<TRelatedEntity, object>> keyExpression)
                {
                    Check.NotNull(keyExpression, nameof(keyExpression));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ReferencedKey(keyExpression.GetPropertyAccessList(), ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Adds or updates an annotation on the relationship. If an annotation with the key specified in
                ///     <paramref name="annotation" /> already exists it's value will be updated.
                /// </summary>
                /// <param name="annotation"> The key of the annotation to be added or updated. </param>
                /// <param name="value"> The value to be stored in the annotation. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual ManyToOneBuilder<TRelatedEntity> Annotation([NotNull] string annotation, [NotNull] string value)
                {
                    Check.NotEmpty(annotation, nameof(annotation));
                    Check.NotEmpty(value, nameof(value));

                    return (ManyToOneBuilder<TRelatedEntity>)base.Annotation(annotation, value);
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
                ///         If <see cref="ReferencedKey(string[])" /> is not specified, then an attempt will be made to match
                ///         the data type and order of foreign key properties against the primary key of the principal
                ///         entity type. If they do not match, new shadow state properties that form a unique index will be
                ///         added to the principal entity type to serve as the reference key.
                ///     </para>
                /// </summary>
                /// <param name="foreignKeyPropertyNames">
                ///     The name(s) of the foreign key property(s).
                /// </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual ManyToOneBuilder<TRelatedEntity> ForeignKey([NotNull] params string[] foreignKeyPropertyNames)
                {
                    Check.NotNull(foreignKeyPropertyNames, nameof(foreignKeyPropertyNames));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ForeignKey(foreignKeyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures the unique property(s) that this relationship targets. Typically you would only call this
                ///     method if you want to use a property(s) other than the primary key as the referenced property(s). If
                ///     the specified property(s) is not already a unique index (or the primary key) then a new unique index
                ///     will be introduced.
                /// </summary>
                /// <param name="keyPropertyNames"> The name(s) of the reference key property(s). </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual ManyToOneBuilder<TRelatedEntity> ReferencedKey([NotNull] params string[] keyPropertyNames)
                {
                    Check.NotNull(keyPropertyNames, nameof(keyPropertyNames));

                    return new ManyToOneBuilder<TRelatedEntity>(Builder.ReferencedKey(keyPropertyNames, ConfigurationSource.Explicit));
                }

                /// <summary>
                ///     Configures whether this is a required relationship (i.e. whether the foreign key property(s) can
                ///     be assigned null).
                /// </summary>
                /// <param name="required"> A value indicating whether this is a required relationship. </param>
                /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
                public new virtual ManyToOneBuilder<TRelatedEntity> Required(bool required = true)
                    => new ManyToOneBuilder<TRelatedEntity>(Builder.Required(required, ConfigurationSource.Explicit));
            }
        }
    }
}
