// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public class PropertyBuilder<TProperty> : PropertyBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public PropertyBuilder(IMutableProperty property)
            : base(property)
        {
        }

        /// <summary>
        ///     Adds or updates an annotation on the property. If an annotation with the key specified in
        ///     <paramref name="annotation" /> already exists its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasAnnotation(string annotation, object? value)
            => (PropertyBuilder<TProperty>)base.HasAnnotation(annotation, value);

        /// <summary>
        ///     Configures whether this property must have a value assigned or whether null is a valid value.
        ///     A property can only be configured as non-required if it is based on a CLR type that can be
        ///     assigned <see langword="null" />.
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
        ///     Configures the precision and scale of the property.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasPrecision(int precision, int scale)
            => (PropertyBuilder<TProperty>)base.HasPrecision(precision, scale);

        /// <summary>
        ///     <para>
        ///         Configures the precision of the property.
        ///     </para>
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasPrecision(int precision)
            => (PropertyBuilder<TProperty>)base.HasPrecision(precision);

        /// <summary>
        ///     Configures the property as capable of persisting unicode characters.
        ///     Can only be set on <see cref="string" /> properties.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property can contain unicode characters. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> IsUnicode(bool unicode = true)
            => (PropertyBuilder<TProperty>)base.IsUnicode(unicode);

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
        public new virtual PropertyBuilder<TProperty> IsRowVersion()
            => (PropertyBuilder<TProperty>)base.IsRowVersion();

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
        public new virtual PropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator
            => (PropertyBuilder<TProperty>)base.HasValueGenerator<TGenerator>();

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
        ///         Setting null does not disable value generation for this property, it just clears any generator explicitly
        ///         configured for this property. The database provider may still have a value generator for the property type.
        ///     </para>
        /// </summary>
        /// <param name="valueGeneratorType"> A type that inherits from <see cref="ValueGenerator" />. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasValueGenerator(Type? valueGeneratorType)
            => (PropertyBuilder<TProperty>)base.HasValueGenerator(valueGeneratorType);

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
        public new virtual PropertyBuilder<TProperty> HasValueGenerator(Func<IProperty, IEntityType, ValueGenerator> factory)
            => (PropertyBuilder<TProperty>)base.HasValueGenerator(factory);

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
        public new virtual PropertyBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            where TFactory : ValueGeneratorFactory
            => (PropertyBuilder<TProperty>)base.HasValueGeneratorFactory<TFactory>();

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
        public new virtual PropertyBuilder<TProperty> HasValueGeneratorFactory(Type? valueGeneratorFactoryType)
            => (PropertyBuilder<TProperty>)base.HasValueGeneratorFactory(valueGeneratorFactoryType);

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
        ///     Configures a property to have a value generated when saving a new or existing entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnAddOrUpdate();

        /// <summary>
        ///     Configures a property to have a value generated when saving an existing entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnUpdate()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnUpdate();

        /// <summary>
        ///     Configures a property to have a value generated under certain conditions when saving an existing entity.
        /// </summary>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnUpdateSometimes()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnUpdateSometimes();

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
        public new virtual PropertyBuilder<TProperty> HasField(string fieldName)
            => (PropertyBuilder<TProperty>)base.HasField(fieldName);

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
        public new virtual PropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (PropertyBuilder<TProperty>)base.UsePropertyAccessMode(propertyAccessMode);

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <typeparam name="TProvider"> The type to convert to and from. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion<TProvider>()
            => (PropertyBuilder<TProperty>)base.HasConversion<TProvider>();

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="providerClrType"> The type to convert to and from. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion(Type? providerClrType)
            => (PropertyBuilder<TProperty>)base.HasConversion(providerClrType);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given conversion expressions.
        /// </summary>
        /// <typeparam name="TProvider"> The store type generated by the conversions. </typeparam>
        /// <param name="convertToProviderExpression"> An expression to convert objects when writing data to the store. </param>
        /// <param name="convertFromProviderExpression"> An expression to convert objects when reading data from the store. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
            => HasConversion(
                new ValueConverter<TProperty, TProvider>(
                    Check.NotNull(convertToProviderExpression, nameof(convertToProviderExpression)),
                    Check.NotNull(convertFromProviderExpression, nameof(convertFromProviderExpression))));

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter{TModel,TProvider}" />.
        /// </summary>
        /// <typeparam name="TProvider"> The store type generated by the converter. </typeparam>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider>? converter)
            => HasConversion((ValueConverter?)converter);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion(ValueConverter? converter)
            => (PropertyBuilder<TProperty>)base.HasConversion(converter);

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <typeparam name="TProvider"> The type to convert to and from. </typeparam>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion<TProvider>(ValueComparer? valueComparer)
            => (PropertyBuilder<TProperty>)base.HasConversion<TProvider>(valueComparer);

        /// <summary>
        ///     Configures the property so that the property value is converted to the given type before
        ///     writing to the database and converted back when reading from the database.
        /// </summary>
        /// <param name="providerClrType"> The type to convert to and from. </param>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion(
            Type providerClrType,
            ValueComparer? valueComparer)
            => (PropertyBuilder<TProperty>)base.HasConversion(providerClrType, valueComparer);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given conversion expressions.
        /// </summary>
        /// <typeparam name="TProvider"> The store type generated by the conversions. </typeparam>
        /// <param name="convertToProviderExpression"> An expression to convert objects when writing data to the store. </param>
        /// <param name="convertFromProviderExpression"> An expression to convert objects when reading data from the store. </param>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer)
            => HasConversion(
                new ValueConverter<TProperty, TProvider>(
                    Check.NotNull(convertToProviderExpression, nameof(convertToProviderExpression)),
                    Check.NotNull(convertFromProviderExpression, nameof(convertFromProviderExpression))),
                valueComparer);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter{TModel,TProvider}" />.
        /// </summary>
        /// <typeparam name="TProvider"> The store type generated by the converter. </typeparam>
        /// <param name="converter"> The converter to use. </param>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual PropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider>? converter,
            ValueComparer? valueComparer)
            => HasConversion((ValueConverter?)converter, valueComparer);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converter"> The converter to use. </param>
        /// <param name="valueComparer"> The comparer to use for values before conversion. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion(
            ValueConverter? converter,
            ValueComparer? valueComparer)
            => (PropertyBuilder<TProperty>)base.HasConversion(converter, valueComparer);

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <typeparam name="TConverter"> A type that derives from <see cref="ValueConverter"/>. </typeparam>
        /// <typeparam name="TComparer"> A type that derives from <see cref="ValueComparer"/>. </typeparam>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion<TConverter, TComparer>()
            where TConverter : ValueConverter
            where TComparer : ValueComparer
            => (PropertyBuilder<TProperty>)base.HasConversion<TConverter, TComparer>();

        /// <summary>
        ///     Configures the property so that the property value is converted to and from the database
        ///     using the given <see cref="ValueConverter" />.
        /// </summary>
        /// <param name="converterType"> A type that derives from <see cref="ValueConverter"/>. </param>
        /// <param name="comparerType"> A type that derives from <see cref="ValueComparer"/>. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public new virtual PropertyBuilder<TProperty> HasConversion(Type converterType, Type? comparerType)
            => (PropertyBuilder<TProperty>)base.HasConversion(converterType, comparerType);
    }
}
