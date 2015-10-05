// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
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
    public class EntityTypeBuilder : IAccessor<Model>, IAccessor<InternalEntityTypeBuilder>
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

        protected virtual EntityTypeBuilder New([NotNull] InternalEntityTypeBuilder builder)
            => new EntityTypeBuilder(builder);

        private InternalEntityTypeBuilder Builder { get; }

        /// <summary>
        ///     The internal builder being used to configure the entity type.
        /// </summary>
        InternalEntityTypeBuilder IAccessor<InternalEntityTypeBuilder>.Service => Builder;

        /// <summary>
        ///     The entity type being configured.
        /// </summary>
        public virtual EntityType Metadata => Builder.Metadata;

        /// <summary>
        ///     The model that the entity type belongs to.
        /// </summary>
        Model IAccessor<Model>.Service => Builder.ModelBuilder.Metadata;

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

        public virtual EntityTypeBuilder HasBaseType([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return New(Builder.HasBaseType(name, ConfigurationSource.Explicit));
        }

        public virtual EntityTypeBuilder HasBaseType([NotNull] Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return New(Builder.HasBaseType(entityType, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Sets the properties that make up the primary key for this entity type.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the primary key. </param>
        /// <returns> An object that can be used to configure the primary key. </returns>
        public virtual KeyBuilder HasKey([NotNull] params string[] propertyNames)
        {
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            return new KeyBuilder(Builder.PrimaryKey(propertyNames, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Creates a new unique constraint for this entity type if one does not already exist over the specified
        ///     properties.
        /// </summary>
        /// <param name="propertyNames"> The names of the properties that make up the unique constraint. </param>
        /// <returns> An object that can be used to configure the unique constraint. </returns>
        public virtual KeyBuilder HasAlternateKey([NotNull] params string[] propertyNames)
        {
            Check.NotNull(propertyNames, nameof(propertyNames));

            return new KeyBuilder(Builder.HasKey(propertyNames, ConfigurationSource.Explicit));
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
        public virtual PropertyBuilder<TProperty> Property<TProperty>([NotNull] string propertyName)
            => new PropertyBuilder<TProperty>(PropertyBuilder(typeof(TProperty), propertyName));

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
            => new PropertyBuilder(PropertyBuilder(propertyType, propertyName));

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
        {
            Check.NotEmpty(propertyNames, nameof(propertyNames));

            return new IndexBuilder(Builder.HasIndex(propertyNames, ConfigurationSource.Explicit));
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

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

            return new ReferenceNavigationBuilder(
                relatedEntityType,
                navigationName,
                ReferenceBuilder(relatedEntityType, navigationName));
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

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit).Metadata;

            return new ReferenceNavigationBuilder(
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
        {
            Check.NotNull(relatedType, nameof(relatedType));

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedType, ConfigurationSource.Explicit).Metadata;

            return new CollectionNavigationBuilder(CollectionBuilder(relatedEntityType, navigationName));
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
        {
            Check.NotEmpty(relatedTypeName, nameof(relatedTypeName));

            var relatedEntityType = Builder.ModelBuilder.Entity(relatedTypeName, ConfigurationSource.Explicit).Metadata;

            return new CollectionNavigationBuilder(CollectionBuilder(relatedEntityType, navigationName));
        }

        protected virtual InternalRelationshipBuilder ReferenceBuilder(
            [NotNull] EntityType relatedEntityType, [CanBeNull] string navigationName)
            => Builder.Relationship(
                relatedEntityType,
                Metadata,
                navigationToPrincipalName: navigationName ?? "",
                navigationToDependentName: null,
                configurationSource: ConfigurationSource.Explicit,
                strictPrincipal: relatedEntityType == Metadata);

        protected virtual InternalRelationshipBuilder CollectionBuilder(
            [NotNull] EntityType relatedEntityType, [CanBeNull] string navigationName)
            => Builder.Relationship(
                Metadata,
                relatedEntityType,
                navigationToPrincipalName: null,
                navigationToDependentName: navigationName ?? "",
                configurationSource: ConfigurationSource.Explicit,
                isUnique: false);
        
        protected virtual InternalPropertyBuilder PropertyBuilder(
            [NotNull] Type propertyType, [CanBeNull] string propertyName)
        {
            Check.NotNull(propertyType, nameof(propertyType));
            Check.NotEmpty(propertyName, nameof(propertyName));

            var builder = Builder.Property(propertyName, ConfigurationSource.Explicit);
            var clrTypeSet = builder.ClrType(propertyType, ConfigurationSource.Explicit);
            Debug.Assert(clrTypeSet);
            return builder;
        }
    }
}
