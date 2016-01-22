// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Internal;

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
        public new virtual PropertyBuilder<TProperty> HasAnnotation([NotNull] string annotation, [NotNull] object value)
            => (PropertyBuilder<TProperty>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     Configures whether this property must have a value assigned or whether null is a valid value.
        ///     A property can only be configured as non-required if it is based on a CLR type that can be
        ///     assigned null.
        /// </summary>
        /// <param name="required"> A value indicating whether the property is required. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> IsRequired(bool required = true)
            => (PropertyBuilder<TProperty>)base.IsRequired(required);

        /// <summary>
        ///     Configures the maximum length of data that can be stored in this property.
        ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
        /// </summary>
        /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasMaxLength(int maxLength)
            => (PropertyBuilder<TProperty>)base.HasMaxLength(maxLength);

        /// <summary>
        ///     Configures whether this property should be used as a concurrency token. When a property is configured
        ///     as a concurrency token the value in the database will be checked when an instance of this entity type
        ///     is updated or deleted during <see cref="DbContext.SaveChanges()" /> to ensure it has not changed since
        ///     the instance was retrieved from the database. If it has changed, an exception will be thrown and the
        ///     changes will not be applied to the database.
        /// </summary>
        /// <param name="concurrencyToken"> A value indicating whether this property is a concurrency token. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> IsConcurrencyToken(bool concurrencyToken = true)
            => (PropertyBuilder<TProperty>)base.IsConcurrencyToken(concurrencyToken);

        /// <summary>
        ///     Configures a property to never have a value generated when an instance of this
        ///     entity type is saved.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        /// <remarks>
        ///     Note that temporary values may still be generated for use internally before a
        ///     new entity is saved.
        /// </remarks>
        public new virtual PropertyBuilder<TProperty> ValueGeneratedNever()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedNever();

        /// <summary>
        ///     Configures a property to have a value generated only when saving a new entity, unless a non-null,
        ///     non-temporary value has been set, in which case the set value will be saved instead. The value
        ///     may be generated by a client-side value generator or may be generated by the database as part
        ///     of saving the entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnAdd()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnAdd();

        /// <summary>
        ///     Configures a property to have a value generated only when saving a new or existing entity, unless
        ///     a non-null, non-temporary value has been set for a new entity, or the existing property value has
        ///     been modified for an existing entity, in which case the set value will be saved instead.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnAddOrUpdate();
    }
}
