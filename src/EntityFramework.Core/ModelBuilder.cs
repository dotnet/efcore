// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring a <see cref="Model" /> that defines the shape of your
    ///         entities and how they map to the database.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="ModelBuilder" /> to construct a model for a context by overriding
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> or creating a <see cref="Model" />
    ///         externally
    ///         and setting is on a <see cref="DbContextOptions" /> instance that is passed to the context
    ///         constructor.
    ///     </para>
    /// </summary>
    public class ModelBuilder : IAccessor<InternalModelBuilder>
    {
        private readonly InternalModelBuilder _builder;

        // TODO: Configure property facets, foreign keys & navigation properties
        // Issue #213

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     apply a set of conventions.
        /// </summary>
        /// <param name="conventions"> The conventions to be applied to the model. </param>
        public ModelBuilder([NotNull] ConventionSet conventions)
        {
            Check.NotNull(conventions, nameof(conventions));

            _builder = new InternalModelBuilder(new Model(), conventions).Initialize();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelBuilder" /> class that will
        ///     configure an existing model and apply a set of conventions.
        /// </summary>
        /// <param name="conventions"> The conventions to be applied to the model. </param>
        /// <param name="model"> The model to be configured. </param>
        public ModelBuilder([NotNull] ConventionSet conventions, [NotNull] Model model)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(conventions, nameof(conventions));

            _builder = new InternalModelBuilder(model, conventions).Initialize();
        }

        public virtual ModelBuilder Validate()
        {
            Builder.Validate();

            return this;
        }

        /// <summary>
        ///     The model being configured.
        /// </summary>
        public virtual Model Model => Builder.Metadata;

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
        ///     The internal <see cref="ModelBuilder" /> being used to configure this model.
        /// </summary>
        InternalModelBuilder IAccessor<InternalModelBuilder>.Service => _builder;

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
        public virtual ModelBuilder Ignore<TEntity>() where TEntity : class
            => Ignore(typeof(TEntity));

        /// <summary>
        ///     Excludes the given entity type from the model. This method is typically used to remove types from
        ///     the model that were added by convention.
        /// </summary>
        /// <param name="type"> The entity type to be removed from the model. </param>
        public virtual ModelBuilder Ignore([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            Builder.Ignore(type, ConfigurationSource.Explicit);

            return this;
        }

        private InternalModelBuilder Builder => this.GetService();
    }
}
