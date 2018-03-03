// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts numeric values to and from their string representation.
    /// </summary>
    public class NumberToStringConverter<TNumber> : ValueConverter<TNumber, string>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 64);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource"/> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public NumberToStringConverter(
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                  ToStringExpression(),
                  ToIntegerExpression(),
                  _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(TNumber), typeof(string), i => new NumberToStringConverter<TNumber>(i.MappingHints), _defaultHints);

        private static Expression<Func<TNumber, string>> ToStringExpression()
        {
            var type = typeof(TNumber).UnwrapNullableType();

            CheckTypeSupported(
                type,
                typeof(NumberToStringConverter<TNumber>),
                typeof(int), typeof(long), typeof(short), typeof(byte),
                typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
                typeof(decimal), typeof(float), typeof(double));

            return v => v == null
                ? null
                : string.Format(
                    CultureInfo.InvariantCulture,
                    type == typeof(float) || type == typeof(double) ? "{0:R}" : "{0}",
                    v);
        }

        private static Expression<Func<string, TNumber>> ToIntegerExpression()
        {
            var type = typeof(TNumber).UnwrapNullableType();

            var tryParseMethod = type.GetMethod(
                nameof(int.TryParse),
                new[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider), type.MakeByRefType() });

            var parsedVariable = Expression.Variable(type, "parsed");
            var param = Expression.Parameter(typeof(string), "v");

            return Expression.Lambda<Func<string, TNumber>>(
                Expression.Block(
                    typeof(TNumber),
                    new[] { parsedVariable },
                    Expression.Condition(
                        Expression.Call(
                            tryParseMethod,
                            param,
                            Expression.Constant(NumberStyles.Any),
                            Expression.Constant(CultureInfo.InvariantCulture, typeof(IFormatProvider)),
                            parsedVariable),
                        typeof(TNumber).IsNullableType()
                            ? (Expression)Expression.Convert(parsedVariable, typeof(TNumber))
                            : parsedVariable,
                        Expression.Constant(default(TNumber), typeof(TNumber)))),
                param);
        }
    }
}
