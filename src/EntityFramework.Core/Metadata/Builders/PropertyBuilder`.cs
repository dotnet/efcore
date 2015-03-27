// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Metadata.Builders
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
    public class PropertyBuilder<TProperty> : PropertyBuilder
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
            : base(builder)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the property. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists it's value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> Annotation([NotNull] string annotation, [NotNull] object value)
            => (PropertyBuilder<TProperty>)base.Annotation(annotation, value);

        /// <summary>
        ///     Configures whether this property must have a value assigned or whether null is a valid value.
        ///     A property can only be configured as non-required if it is based on a CLR type that can be
        ///     assigned null.
        /// </summary>
        /// <param name="isRequired"> A value indicating whether the property is required. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> Required(bool isRequired = true)
            => (PropertyBuilder<TProperty>)base.Required(isRequired);

        /// <summary>
        ///     Configures the maximum length of data that can be stored in this property.
        ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
        /// </summary>
        /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> MaxLength(int maxLength)
            => (PropertyBuilder<TProperty>)base.MaxLength(maxLength);

        /// <summary>
        ///     Configures whether this property should be used as a concurrency token. When a property is configured
        ///     as a concurrency token the value in the data store will be checked when an instance of this entity type
        ///     is updated or deleted during <see cref="DbContext.SaveChanges()" /> to ensure it has not changed since
        ///     the instance was retrieved from the data store. If it has changed, an exception will be thrown and the
        ///     changes will not be applied to the data store.
        /// </summary>
        /// <param name="isConcurrencyToken"> A value indicating whether this property is a concurrency token. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> ConcurrencyToken(bool isConcurrencyToken = true)
            => (PropertyBuilder<TProperty>)base.ConcurrencyToken(isConcurrencyToken);

        /// <summary>
        ///     Configures whether a value is generated for this property when a new instance of the entity type
        ///     is added to a context. Data stores will typically register an appropriate
        ///     <see cref="ValueGenerator" /> to handle generating values. This functionality is typically
        ///     used for key values and is switched on by convention.
        /// </summary>
        /// <param name="generateValue"> A value indicating whether a value should be generated. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> GenerateValueOnAdd(bool generateValue = true)
            => (PropertyBuilder<TProperty>)base.GenerateValueOnAdd(generateValue);

        /// <summary>
        ///     Configures whether a value is generated for this property by the data store every time an
        ///     instance of this entity type is saved (initial add and any subsequent updates).
        /// </summary>
        /// <param name="computed"> A value indicating whether a value is generated by the data store. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> StoreComputed(bool computed = true)
            => (PropertyBuilder<TProperty>)base.StoreComputed(computed);

        /// <summary>
        ///     Configures whether a default value is generated for this property by the store when an instance
        ///     of this entity type is saved and no value has been set.
        /// </summary>
        /// <param name="useDefault">
        ///     A value indicating whether a default value is generated by the data store when no value is set.
        /// </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> UseStoreDefault(bool useDefault = true)
            => (PropertyBuilder<TProperty>)base.UseStoreDefault(useDefault);
    }
}
