// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
        public PropertyBuilder([NotNull] IMutableProperty property)
            : base(property)
        {
        }

        /// <inheritdoc cref="PropertyBuilder.HasAnnotation" />
        public new virtual PropertyBuilder<TProperty> HasAnnotation([NotNull] string annotation, [NotNull] object value)
            => (PropertyBuilder<TProperty>)base.HasAnnotation(annotation, value);

        /// <inheritdoc cref="PropertyBuilder.IsRequired" />
        public new virtual PropertyBuilder<TProperty> IsRequired(bool required = true)
            => (PropertyBuilder<TProperty>)base.IsRequired(required);

        /// <inheritdoc cref="PropertyBuilder.HasMaxLength" />
        public new virtual PropertyBuilder<TProperty> HasMaxLength(int maxLength)
            => (PropertyBuilder<TProperty>)base.HasMaxLength(maxLength);

        /// <inheritdoc cref="PropertyBuilder.HasPrecision(int,int)" />
        public new virtual PropertyBuilder<TProperty> HasPrecision(int precision, int scale)
            => (PropertyBuilder<TProperty>)base.HasPrecision(precision, scale);

        /// <inheritdoc cref="PropertyBuilder.HasPrecision(int)" />
        public new virtual PropertyBuilder<TProperty> HasPrecision(int precision)
            => (PropertyBuilder<TProperty>)base.HasPrecision(precision);

        /// <inheritdoc cref="PropertyBuilder.IsUnicode" />
        public new virtual PropertyBuilder<TProperty> IsUnicode(bool unicode = true)
            => (PropertyBuilder<TProperty>)base.IsUnicode(unicode);

        /// <inheritdoc cref="PropertyBuilder.IsRowVersion" />
        public new virtual PropertyBuilder<TProperty> IsRowVersion()
            => (PropertyBuilder<TProperty>)base.IsRowVersion();

        /// <inheritdoc cref="PropertyBuilder.HasValueGenerator{TGenerator}" />
        public new virtual PropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator
            => (PropertyBuilder<TProperty>)base.HasValueGenerator<TGenerator>();

        /// <inheritdoc cref="PropertyBuilder.HasValueGenerator(Type)" />
        public new virtual PropertyBuilder<TProperty> HasValueGenerator([CanBeNull] Type valueGeneratorType)
            => (PropertyBuilder<TProperty>)base.HasValueGenerator(valueGeneratorType);

        /// <inheritdoc cref="PropertyBuilder.HasValueGenerator(Func{IProperty, IEntityType, ValueGenerator})" />
        public new virtual PropertyBuilder<TProperty> HasValueGenerator([NotNull] Func<IProperty, IEntityType, ValueGenerator> factory)
            => (PropertyBuilder<TProperty>)base.HasValueGenerator(factory);

        /// <inheritdoc cref="PropertyBuilder.IsConcurrencyToken" />
        public new virtual PropertyBuilder<TProperty> IsConcurrencyToken(bool concurrencyToken = true)
            => (PropertyBuilder<TProperty>)base.IsConcurrencyToken(concurrencyToken);

        /// <inheritdoc cref="PropertyBuilder.ValueGeneratedNever" />
        public new virtual PropertyBuilder<TProperty> ValueGeneratedNever()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedNever();

        /// <inheritdoc cref="PropertyBuilder.ValueGeneratedOnAdd" />
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnAdd()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnAdd();

        /// <inheritdoc cref="PropertyBuilder.ValueGeneratedOnAddOrUpdate" />
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnAddOrUpdate();

        /// <inheritdoc cref="PropertyBuilder.ValueGeneratedOnUpdate" />
        public new virtual PropertyBuilder<TProperty> ValueGeneratedOnUpdate()
            => (PropertyBuilder<TProperty>)base.ValueGeneratedOnUpdate();

        /// <inheritdoc cref="PropertyBuilder.ValueGeneratedOnUpdateSometimes" />
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
        public new virtual PropertyBuilder<TProperty> HasField([NotNull] string fieldName)
            => (PropertyBuilder<TProperty>)base.HasField(fieldName);

        /// <inheritdoc cref="PropertyBuilder.HasConversion{TProvider}()" />
        public new virtual PropertyBuilder<TProperty> HasConversion<TProvider>()
            => (PropertyBuilder<TProperty>)base.HasConversion<TProvider>();

        /// <inheritdoc cref="PropertyBuilder.HasConversion(Type)" />
        public new virtual PropertyBuilder<TProperty> HasConversion([CanBeNull] Type providerClrType)
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
            [NotNull] Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            [NotNull] Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
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
        public virtual PropertyBuilder<TProperty> HasConversion<TProvider>([CanBeNull] ValueConverter<TProperty, TProvider> converter)
            => HasConversion((ValueConverter)converter);

        /// <inheritdoc cref="PropertyBuilder.HasConversion(ValueConverter)" />
        public new virtual PropertyBuilder<TProperty> HasConversion([CanBeNull] ValueConverter converter)
            => (PropertyBuilder<TProperty>)base.HasConversion(converter);

        /// <inheritdoc cref="PropertyBuilder.HasConversion{TProvider}(ValueComparer)" />
        public new virtual PropertyBuilder<TProperty> HasConversion<TProvider>([CanBeNull] ValueComparer valueComparer)
            => (PropertyBuilder<TProperty>)base.HasConversion<TProvider>(valueComparer);

        /// <inheritdoc cref="PropertyBuilder.HasConversion(Type,ValueComparer)" />
        public new virtual PropertyBuilder<TProperty> HasConversion(
            [CanBeNull] Type providerClrType,
            [CanBeNull] ValueComparer valueComparer)
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
            [NotNull] Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            [NotNull] Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            [CanBeNull] ValueComparer valueComparer)
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
            [CanBeNull] ValueConverter<TProperty, TProvider> converter,
            [CanBeNull] ValueComparer valueComparer)
            => HasConversion((ValueConverter)converter, valueComparer);

        /// <inheritdoc cref="PropertyBuilder.HasConversion(ValueConverter,ValueComparer)" />
        public new virtual PropertyBuilder<TProperty> HasConversion(
            [CanBeNull] ValueConverter converter,
            [CanBeNull] ValueComparer valueComparer)
            => (PropertyBuilder<TProperty>)base.HasConversion(converter, valueComparer);

        /// <inheritdoc cref="PropertyBuilder.UsePropertyAccessMode" />
        public new virtual PropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (PropertyBuilder<TProperty>)base.UsePropertyAccessMode(propertyAccessMode);
    }
}
