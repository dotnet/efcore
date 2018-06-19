// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class StringNumberConverter<TModel, TProvider, TNumber> : ValueConverter<TModel, TProvider>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 64);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public StringNumberConverter(
            [NotNull] Expression<Func<TModel, TProvider>> convertToProviderExpression,
            [NotNull] Expression<Func<TProvider, TModel>> convertFromProviderExpression,
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected static Expression<Func<string, TNumber>> ToNumber()
        {
            var type = typeof(TNumber).UnwrapNullableType();

            CheckTypeSupported(
                type,
                typeof(StringNumberConverter<TModel, TProvider, TNumber>),
                typeof(int), typeof(long), typeof(short), typeof(byte),
                typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
                typeof(decimal), typeof(float), typeof(double));

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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new static Expression<Func<TNumber, string>> ToString()
        {
            var type = typeof(TNumber).UnwrapNullableType();

            CheckTypeSupported(
                type,
                typeof(StringNumberConverter<TModel, TProvider, TNumber>),
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
    }
}
