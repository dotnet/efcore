// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Defines conversions from an object of one type in a model to an object of the same or
    ///     different type in the store.
    /// </summary>
    public abstract class ValueConverter
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueConverter" /> class.
        /// </summary>
        /// <param name="convertToProviderExpression">
        ///     The expression to convert objects when writing data to the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="convertFromProviderExpression">
        ///     The expression to convert objects when reading data from the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        protected ValueConverter(
            LambdaExpression convertToProviderExpression,
            LambdaExpression convertFromProviderExpression,
            ConverterMappingHints? mappingHints = null)
            : this(convertToProviderExpression, convertFromProviderExpression, false, mappingHints)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueConverter" /> class.
        /// </summary>
        /// <param name="convertToProviderExpression">
        ///     The expression to convert objects when writing data to the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="convertFromProviderExpression">
        ///     The expression to convert objects when reading data from the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </param>
        /// <param name="convertsNulls">
        ///     If <see langword="true" />, then the nulls will be passed to the converter for conversion. Otherwise null
        ///     values always remain null.
        /// </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        protected ValueConverter(
            LambdaExpression convertToProviderExpression,
            LambdaExpression convertFromProviderExpression,
            bool convertsNulls,
            ConverterMappingHints? mappingHints = null)
        {
            Check.NotNull(convertToProviderExpression, nameof(convertToProviderExpression));
            Check.NotNull(convertFromProviderExpression, nameof(convertFromProviderExpression));

            ConvertToProviderExpression = convertToProviderExpression;
            ConvertFromProviderExpression = convertFromProviderExpression;
            ConvertsNulls = convertsNulls;
            MappingHints = mappingHints;
        }

        /// <summary>
        ///     Gets the function to convert objects when writing data to the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public abstract Func<object?, object?> ConvertToProvider { get; }

        /// <summary>
        ///     Gets the function to convert objects when reading data from the store,
        ///     setup to handle nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public abstract Func<object?, object?> ConvertFromProvider { get; }

        /// <summary>
        ///     Gets the expression to convert objects when writing data to the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public virtual LambdaExpression ConvertToProviderExpression { get; }

        /// <summary>
        ///     Gets the expression to convert objects when reading data from the store,
        ///     exactly as supplied and may not handle
        ///     nulls, boxing, and non-exact matches of simple types.
        /// </summary>
        public virtual LambdaExpression ConvertFromProviderExpression { get; }

        /// <summary>
        ///     The CLR type used in the EF model.
        /// </summary>
        public abstract Type ModelClrType { get; }

        /// <summary>
        ///     The CLR type used when reading and writing from the store.
        /// </summary>
        public abstract Type ProviderClrType { get; }

        /// <summary>
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </summary>
        public virtual ConverterMappingHints? MappingHints { get; }

        /// <summary>
        ///     <para>
        ///         If <see langword="true" />, then the nulls will be passed to the converter for conversion. Otherwise null
        ///         values always remain null.
        ///     </para>
        ///     <para>
        ///         By default, value converters do not handle nulls so that a value converter for a non-nullable property (such as
        ///         a primary key) can be used for correlated nullable properties, such as any corresponding foreign key properties.
        ///     </para>
        /// </summary>
        public virtual bool ConvertsNulls { get; }

        /// <summary>
        ///     Checks that the type used with a value converter is supported by that converter and throws if not.
        /// </summary>
        /// <param name="type"> The type to check. </param>
        /// <param name="converterType"> The value converter type. </param>
        /// <param name="supportedTypes"> The types that are supported. </param>
        /// <returns> The given type. </returns>
        protected static Type CheckTypeSupported(
            Type type,
            Type converterType,
            params Type[] supportedTypes)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(converterType, nameof(converterType));
            Check.NotEmpty(supportedTypes, nameof(supportedTypes));

            if (!supportedTypes.Contains(type))
            {
                throw new InvalidOperationException(
                    CoreStrings.ConverterBadType(
                        converterType.ShortDisplayName(),
                        type.ShortDisplayName(),
                        string.Join(", ", supportedTypes.Select(t => $"'{t.ShortDisplayName()}'"))));
            }

            return type;
        }

        /// <summary>
        ///     Composes another <see cref="ValueConverter" /> instance with this one such that
        ///     the result of the first conversion is used as the input to the second conversion.
        /// </summary>
        /// <param name="secondConverter"> The second converter. </param>
        /// <returns> The composed converter. </returns>
        public virtual ValueConverter ComposeWith(
            ValueConverter? secondConverter)
        {
            if (secondConverter == null)
            {
                return this;
            }

            if (ProviderClrType.UnwrapNullableType() != secondConverter.ModelClrType.UnwrapNullableType())
            {
                throw new ArgumentException(
                    CoreStrings.ConvertersCannotBeComposed(
                        ModelClrType.ShortDisplayName(),
                        ProviderClrType.ShortDisplayName(),
                        secondConverter.ModelClrType.ShortDisplayName(),
                        secondConverter.ProviderClrType.ShortDisplayName()));
            }

            var firstConverter
                = ProviderClrType.IsNullableType()
                && !secondConverter.ModelClrType.IsNullableType()
                    ? ComposeWith(
                        (ValueConverter)Activator.CreateInstance(
                            typeof(CastingConverter<,>).MakeGenericType(
                                ProviderClrType,
                                secondConverter.ModelClrType),
                            MappingHints)!)
                    : this;

            return (ValueConverter)Activator.CreateInstance(
                typeof(CompositeValueConverter<,,>).MakeGenericType(
                    firstConverter.ModelClrType,
                    firstConverter.ProviderClrType,
                    secondConverter.ProviderClrType),
                firstConverter,
                secondConverter,
                secondConverter.MappingHints == null
                    ? firstConverter.MappingHints
                    : secondConverter.MappingHints.With(firstConverter.MappingHints))!;
        }
    }
}
