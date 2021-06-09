// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for setting defaults and configuring conventions before they run.
    ///     </para>
    ///     <para>
    ///         You can use <see cref="ModelConfigurationBuilder" /> to configure the conventions for a context by overriding
    ///         <see cref="DbContext.ConfigureConventions(ModelConfigurationBuilder)" /> on your derived context.
    ///         Alternatively you can create the model externally and set it on a <see cref="DbContextOptions" /> instance
    ///         that is passed to the context constructor.
    ///     </para>
    /// </summary>
    public class ModelConfigurationBuilder
    {
        private readonly ModelConfiguration _modelConfiguration = new();
        private readonly ConventionSet _conventions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelConfigurationBuilder" />.
        /// </summary>
        /// <param name="conventions"> The conventions to be applied during model building. </param>
        public ModelConfigurationBuilder(ConventionSet conventions)
        {
            Check.NotNull(conventions, nameof(conventions));

            _conventions = conventions;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual ModelConfiguration ModelConfiguration => _modelConfiguration;

        /// <summary>
        ///     Prevents the conventions from the given type from discovering properties of the given or derived types.
        /// </summary>
        /// <typeparam name="TEntity"> The type to be ignored. </typeparam>
        /// <returns>
        ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelConfigurationBuilder IgnoreAny<TEntity>()
            => IgnoreAny(typeof(TEntity));

        /// <summary>
        ///     Prevents the conventions from the given type from discovering properties of the given or derived types.
        /// </summary>
        /// <param name="type"> The type to be ignored. </param>
        /// <returns>
        ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelConfigurationBuilder IgnoreAny(Type type)
        {
            Check.NotNull(type, nameof(type));

            _modelConfiguration.AddIgnored(type);

            return this;
        }

        /// <summary>
        ///     Marks the given and derived types as corresponding to entity type properties.
        /// </summary>
        /// <typeparam name="TProperty"> The property type to be configured. </typeparam>
        /// <returns> An object that can be used to provide the configuration defaults for the properties. </returns>
        public virtual PropertiesConfigurationBuilder<TProperty> Properties<TProperty>()
        {
            var property = _modelConfiguration.GetOrAddProperty(typeof(TProperty));

            return new PropertiesConfigurationBuilder<TProperty>(property);
        }

        /// <summary>
        ///     Marks the given and derived types as corresponding to entity type properties.
        /// </summary>
        /// <typeparam name="TProperty"> The property type to be configured. </typeparam>
        /// <param name="buildAction"> An action that performs configuration of the property. </param>
        /// <returns>
        ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelConfigurationBuilder Properties<TProperty>(
            Action<PropertiesConfigurationBuilder<TProperty>> buildAction)
        {
            Check.NotNull(buildAction, nameof(buildAction));

            var propertyBuilder = Properties<TProperty>();
            buildAction(propertyBuilder);

            return this;
        }

        /// <summary>
        ///     Marks the given and derived types as corresponding to entity type properties.
        /// </summary>
        /// <param name="propertyType"> The property type to be configured. </param>
        /// <returns> An object that can be used to configure the property. </returns>
        public virtual PropertiesConfigurationBuilder Properties(Type propertyType)
        {
            Check.NotNull(propertyType, nameof(propertyType));

            var property = _modelConfiguration.GetOrAddProperty(propertyType);

            return new PropertiesConfigurationBuilder(property);
        }

        /// <summary>
        ///     Marks the given and derived types as corresponding to entity type properties.
        /// </summary>
        /// <param name="propertyType"> The property type to be configured. </param>
        /// <param name="buildAction"> An action that performs configuration of the property. </param>
        /// <returns>
        ///     The same <see cref="ModelConfigurationBuilder" /> instance so that additional configuration calls can be chained.
        /// </returns>
        public virtual ModelConfigurationBuilder Properties(
            Type propertyType,
            Action<PropertiesConfigurationBuilder> buildAction)
        {
            Check.NotNull(propertyType, nameof(propertyType));
            Check.NotNull(buildAction, nameof(buildAction));

            var propertyBuilder = Properties(propertyType);
            buildAction(propertyBuilder);

            return this;
        }

        /// <summary>
        ///     Creates the configured <see cref="ModelBuilder" /> used to create the model. This is done automatically when using
        ///     <see cref="DbContext.OnModelCreating" />; this method allows it to be run
        ///     explicitly in cases where the automatic execution is not possible.
        /// </summary>
        /// <param name="modelDependencies"> The dependencies object used during model building. </param>
        /// <returns> The configured <see cref="ModelBuilder" />. </returns>
        public virtual ModelBuilder CreateModelBuilder(ModelDependencies? modelDependencies)
            => new(_conventions, modelDependencies, _modelConfiguration.IsEmpty() ? null : _modelConfiguration);

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string? ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
