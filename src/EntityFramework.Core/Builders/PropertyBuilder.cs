// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Builders
{
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
}
