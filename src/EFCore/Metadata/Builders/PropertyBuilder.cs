// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="IMutableProperty" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class PropertyBuilder : IInfrastructure<IConventionPropertyBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PropertyBuilder(IMutableProperty property)
        {
            Check.NotNull(property, nameof(property));

            Builder = ((Property)property).Builder;
        }

        /// <summary>
        ///     The internal builder being used to configure the property.
        /// </summary>
        IConventionPropertyBuilder IInfrastructure<IConventionPropertyBuilder>.Instance
            => Builder;

        private InternalPropertyBuilder Builder { get; }

        /// <summary>
        ///     The property being configured.
        /// </summary>
        public virtual IMutableProperty Metadata
            => Builder.Metadata;

        /// <summary>
        ///     Adds or updates an annotation on the property. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasAnnotation(string annotation, object? value)
        {
            Check.NotEmpty(annotation, nameof(annotation));

            Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures whether this property must have a value assigned or <see langword="null" /> is a valid value.
        ///     A property can only be configured as non-required if it is based on a CLR type that can be
        ///     assigned <see langword="null" />.
        /// </summary>
        /// <param name="required"> A value indicating whether the property is required. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder IsRequired(bool required = true)
        {
            Builder.IsRequired(required, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the maximum length of data that can be stored in this property.
        ///     Maximum length can only be set on array properties (including <see cref="string" /> properties).
        /// </summary>
        /// <param name="maxLength"> The maximum length of data allowed in the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasMaxLength(int maxLength)
        {
            Builder.HasMaxLength(maxLength, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the precision and scale of the property.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasPrecision(int precision, int scale)
        {
            Builder.HasPrecision(precision, ConfigurationSource.Explicit);
            Builder.HasScale(scale, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures the precision of the property.
        ///     </para>
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasPrecision(int precision)
        {
            Builder.HasPrecision(precision, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures whether the property as capable of persisting unicode characters.
        ///     Can only be set on <see cref="string" /> properties.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property can contain unicode characters. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder IsUnicode(bool unicode = true)
        {
            Builder.IsUnicode(unicode, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures the property as <see cref="ValueGeneratedOnAddOrUpdate" /> and
        ///         <see cref="IsConcurrencyToken" />.
        ///     </para>
        ///     <para>
        ///         Database providers can choose to interpret this in different way, but it is commonly used
        ///         to indicate some form of automatic row-versioning as used for optimistic concurrency detection.
        ///     </para>
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder IsRowVersion()
        {
            Builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Explicit);
            Builder.IsConcurrencyToken(true, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures the <see cref="ValueGenerator" /> that will generate values for this property.
        ///     </para>
        ///     <para>
        ///         Values are generated when the entity is added to the context using, for example,
        ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
        ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
        ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
        ///     </para>
        ///     <para>
        ///         A single instance of this type will be created and used to generate values for this property in all
        ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
        ///     </para>
        ///     <para>
        ///         This method is intended for use with custom value generation. Value generation for common cases is
        ///         usually handled automatically by the database provider.
        ///     </para>
        /// </summary>
        /// <typeparam name="TGenerator"> A type that inherits from <see cref="ValueGenerator" />. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator
        {
            Builder.HasValueGenerator(typeof(TGenerator), ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures the <see cref="ValueGenerator" /> that will generate values for this property.
        ///     </para>
        ///     <para>
        ///         Values are generated when the entity is added to the context using, for example,
        ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
        ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
        ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
        ///     </para>
        ///     <para>
        ///         A single instance of this type will be created and used to generate values for this property in all
        ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
        ///     </para>
        ///     <para>
        ///         This method is intended for use with custom value generation. Value generation for common cases is
        ///         usually handled automatically by the database provider.
        ///     </para>
        ///     <para>
        ///         Setting <see langword="null"/> does not disable value generation for this property, it just clears any generator explicitly
        ///         configured for this property. The database provider may still have a value generator for the property type.
        ///     </para>
        /// </summary>
        /// <param name="valueGeneratorType"> A type that inherits from <see cref="ValueGenerator" />. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasValueGenerator(Type? valueGeneratorType)
        {
            Builder.HasValueGenerator(valueGeneratorType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures a factory for creating a <see cref="ValueGenerator" /> to use to generate values
        ///         for this property.
        ///     </para>
        ///     <para>
        ///         Values are generated when the entity is added to the context using, for example,
        ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
        ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
        ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
        ///     </para>
        ///     <para>
        ///         This factory will be invoked once to create a single instance of the value generator, and
        ///         this will be used to generate values for this property in all instances of the entity type.
        ///     </para>
        ///     <para>
        ///         This method is intended for use with custom value generation. Value generation for common cases is
        ///         usually handled automatically by the database provider.
        ///     </para>
        /// </summary>
        /// <param name="factory"> A delegate that will be used to create value generator instances. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasValueGenerator(Func<IProperty, IEntityType, ValueGenerator> factory)
        {
            Check.NotNull(factory, nameof(factory));

            Builder.HasValueGenerator(factory, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Configures the <see cref="ValueGeneratorFactory" /> for creating a <see cref="ValueGenerator" />
        ///         to use to generate values for this property.
        ///     </para>
        ///     <para>
        ///         Values are generated when the entity is added to the context using, for example,
        ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
        ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
        ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
        ///     </para>
        ///     <para>
        ///         A single instance of this type will be created and used to generate values for this property in all
        ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
        ///     </para>
        ///     <para>
        ///         This method is intended for use with custom value generation. Value generation for common cases is
        ///         usually handled automatically by the database provider.
        ///     </para>
        ///     <para>
        ///         Setting <see langword="null"/> does not disable value generation for this property, it just clears any generator explicitly
        ///         configured for this property. The database provider may still have a value generator for the property type.
        ///     </para>
        /// </summary>
        /// <typeparam name="TFactory"> A type that inherits from <see cref="ValueGeneratorFactory" />. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasValueGeneratorFactory<TFactory>()
            where TFactory : ValueGeneratorFactory
            => HasValueGeneratorFactory(typeof(TFactory));

        /// <summary>
        ///     <para>
        ///         Configures the <see cref="ValueGeneratorFactory" /> for creating a <see cref="ValueGenerator" />
        ///         to use to generate values for this property.
        ///     </para>
        ///     <para>
        ///         Values are generated when the entity is added to the context using, for example,
        ///         <see cref="DbContext.Add{TEntity}" />. Values are generated only when the property is assigned
        ///         the CLR default value (<see langword="null" /> for <c>string</c>, <c>0</c> for <c>int</c>,
        ///         <c>Guid.Empty</c> for <c>Guid</c>, etc.).
        ///     </para>
        ///     <para>
        ///         A single instance of this type will be created and used to generate values for this property in all
        ///         instances of the entity type. The type must be instantiable and have a parameterless constructor.
        ///     </para>
        ///     <para>
        ///         This method is intended for use with custom value generation. Value generation for common cases is
        ///         usually handled automatically by the database provider.
        ///     </para>
        ///     <para>
        ///         Setting <see langword="null"/> does not disable value generation for this property, it just clears any generator explicitly
        ///         configured for this property. The database provider may still have a value generator for the property type.
        ///     </para>
        /// </summary>
        /// <param name="valueGeneratorFactoryType"> A type that inherits from <see cref="ValueGeneratorFactory" />. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasValueGeneratorFactory(Type? valueGeneratorFactoryType)
        {
            Builder.HasValueGeneratorFactory(valueGeneratorFactoryType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures whether this property should be used as a concurrency token. When a property is configured
        ///     as a concurrency token the value in the database will be checked when an instance of this entity type
        ///     is updated or deleted during <see cref="DbContext.SaveChanges()" /> to ensure it has not changed since
        ///     the instance was retrieved from the database. If it has changed, an exception will be thrown and the
        ///     changes will not be applied to the database.
        /// </summary>
        /// <param name="concurrencyToken"> A value indicating whether this property is a concurrency token. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder IsConcurrencyToken(bool concurrencyToken = true)
        {
            Builder.IsConcurrencyToken(concurrencyToken, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures a property to never have a value generated by the database when an instance of this
        ///     entity type is saved.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        /// <remarks>
        ///     Note that values may still be generated by a client-side value generator, if one is set explicitly or by a convention.
        /// </remarks>
        public virtual PropertyBuilder ValueGeneratedNever()
        {
            Builder.ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures a property to have a value generated only when saving a new entity, unless a non-null,
        ///     non-temporary value has been set, in which case the set value will be saved instead. The value
        ///     may be generated by a client-side value generator or may be generated by the database as part
        ///     of saving the entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder ValueGeneratedOnAdd()
        {
            Builder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures a property to have a value generated when saving a new or existing entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder ValueGeneratedOnAddOrUpdate()
        {
            Builder.ValueGenerated(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures a property to have a value generated when saving an existing entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder ValueGeneratedOnUpdate()
        {
            Builder.ValueGenerated(ValueGenerated.OnUpdate, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures a property to have a value generated under certain conditions when saving an existing entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder ValueGeneratedOnUpdateSometimes()
        {
            Builder.ValueGenerated(ValueGenerated.OnUpdateSometimes, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets the backing field to use for this property.
        ///     </para>
        ///     <para>
        ///         Backing fields are normally found by convention as described
        ///         here: http://go.microsoft.com/fwlink/?LinkId=723277.
        ///         This method is useful for setting backing fields explicitly in cases where the
        ///         correct field is not found by convention.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="UsePropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasField(string fieldName)
        {
            Check.NotEmpty(fieldName, nameof(fieldName));

            Builder.HasField(fieldName, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for this property.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses.  Calling this method will change that behavior
        ///         for this property as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        ///     <para>
        ///         Calling this method overrides for this property any access mode that was set on the
        ///         entity type or model.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        {
            Builder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <typeparam name="TProvider"> The type to convert to and from. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasConversion<TProvider>()
            => HasConversion(typeof(TProvider));

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="providerClrType"> The type to convert to and from. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasConversion(Type? providerClrType)
        {
            Builder.HasConversion(providerClrType, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasConversion(ValueConverter? converter)
        {
            Builder.HasConversion(converter, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <typeparam name="TProvider"> The type to convert to and from. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasConversion<TProvider>(ValueComparer? valueComparer)
            => HasConversion(typeof(TProvider), valueComparer);

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="providerClrType"> The type to convert to and from. </param>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasConversion(Type? providerClrType, ValueComparer? valueComparer)
        {
            Builder.HasConversion(providerClrType, ConfigurationSource.Explicit);
            Builder.HasValueComparer(valueComparer, ConfigurationSource.Explicit);

            return this;
        }

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder HasConversion(ValueConverter? converter, ValueComparer? valueComparer)
        {
            Builder.HasConversion(converter, ConfigurationSource.Explicit);
            Builder.HasValueComparer(valueComparer, ConfigurationSource.Explicit);

            return this;
        }

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
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
