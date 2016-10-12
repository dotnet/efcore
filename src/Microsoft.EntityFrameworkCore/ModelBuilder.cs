// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring a <see cref="IMutableModel" /> that defines the shape of your
    ///         entities, the relationships between them, and how they map to the database.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="ModelBuilder" /> to construct a model for a context by overriding
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> on your derived context. Alternatively you can create the
    ///         model externally and set it on a <see cref="DbContextOptions" /> instance that is passed to the context constructor.
    ///     </para>
    /// </summary>
    public class ModelBuilder : IInfrastructure<InternalModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     apply a set of conventions.
        /// </summary>
        /// <param name="conventions"> The conventions to be applied to the model. </param>
        public ModelBuilder([NotNull] ConventionSet conventions)
        {
            Check.NotNull(conventions, nameof(conventions));

            _builder = new InternalModelBuilder(new Model(conventions));
        }

        /// <summary>
        ///     The model being configured.
        /// </summary>
        public virtual IMutableModel Model => Builder.Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the model. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same <see cref="ModelBuilder" /> instance so that multiple configuration calls can be chained. </returns>
        public virtual ModelBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         The internal <see cref="ModelBuilder" /> being used to configure this model.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods to configure the model. It is not intended to be used in
        ///         application code.
        ///     </para>
        /// </summary>
        InternalModelBuilder IInfrastructure<InternalModelBuilder>.Instance => _builder;

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If the entity type is not already part of the model, it will be added to the model.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type to be configured. </typeparam>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class
            => new EntityTypeBuilder<TEntity>(Builder.Entity(typeof(TEntity), ConfigurationSource.Explicit));

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If the entity type is not already part of the model, it will be added to the model.
        /// </summary>
        /// <param name="type"> The entity type to be configured. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder Entity([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return new EntityTypeBuilder(Builder.Entity(type, ConfigurationSource.Explicit));
        }

        /// <summary>
        ///     Returns an object that can be used to configure a given entity type in the model.
        ///     If an entity type with the provided name is not already part of the model,
        ///     a new entity type that does not have a corresponding CLR type will be added to the model.
        /// </summary>
        /// <param name="name"> The name of the entity type to be configured. </param>
        /// <returns> An object that can be used to configure the entity type. </returns>
        public virtual EntityTypeBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            return new EntityTypeBuilder(Builder.Entity(name, ConfigurationSource.Explicit));
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
        /// <param name="buildAction"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity<TEntity>([NotNull] Action<EntityTypeBuilder<TEntity>> buildAction) where TEntity : class
        {
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(Entity<TEntity>());

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
        /// <param name="type"> The entity type to be configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity([NotNull] Type type, [NotNull] Action<EntityTypeBuilder> buildAction)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(Entity(type));

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Performs configuration of a given entity type in the model.
        ///         If an entity type with the provided name is not already part of the model,
        ///         a new entity type that does not have a corresponding CLR type will be added to the model.
        ///     </para>
        ///     <para>
        ///         This overload allows configuration of the entity type to be done in line in the method call rather
        ///         than being chained after a call to <see cref="Entity(string)" />. This allows additional
        ///         configuration at the model level to be chained after configuration for the entity type.
        ///     </para>
        /// </summary>
        /// <param name="name"> The name of the entity type to be configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the entity type. </param>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityTypeBuilder> buildAction)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(buildAction, nameof(buildAction));

            buildAction(Entity(name));

            return this;
        }

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <typeparam name="TEntity"> The  entity type to be removed from the model. </typeparam>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Ignore<TEntity>() where TEntity : class
            => Ignore(typeof(TEntity));

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <param name="type"> The entity type to be removed from the model. </param>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder Ignore([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            Builder.Ignore(type, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the default <see cref="ChangeTrackingStrategy" /> to be used for this model.
        ///     This strategy indicates how the context detects changes to properties for an instance of an entity type.
        /// </summary>
        /// <param name="changeTrackingStrategy"> The change tracking strategy to be used. </param>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
        {
            Builder.Metadata.ChangeTrackingStrategy = changeTrackingStrategy;

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for all properties of this entity type.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses.  Calling this method witll change that behavior
        ///         for all properties in the model as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for properties of this model. </param>
        /// <returns>
        ///     The same <see cref="ModelBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        {
            Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

            return this;
        }

        private InternalModelBuilder Builder => this.GetInfrastructure();
    }
}
