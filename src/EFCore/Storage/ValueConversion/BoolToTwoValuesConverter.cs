// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="bool" /> values to and from two different values.
    /// </summary>
    public class BoolToTwoValuesConverter<TProvider> : ValueConverter<bool, TProvider>
    {
        /// <summary>
        ///     <para>
        ///         Creates a new instance of this converter that will convert a <see langword="false" /> false
        ///         to one value and a <see langword="true" /> to another.
        ///     </para>
        ///     <para>
        ///         Use <see cref="BoolToZeroOneConverter{TProvider}" /> for converting a <see cref="bool" /> to zero/one.
        ///     </para>
        /// </summary>
        /// <param name="falseValue"> The value to convert to for <see langword="false" />. </param>
        /// <param name="trueValue"> The value to convert to for <see langword="true" />. </param>
        /// <param name="fromProvider"> Optional custom translator from store. </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BoolToTwoValuesConverter(
            [CanBeNull] TProvider falseValue,
            [CanBeNull] TProvider trueValue,
            [CanBeNull] Expression<Func<TProvider, bool>> fromProvider = null,
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(ToProvider(falseValue, trueValue), fromProvider ?? ToBool(trueValue), mappingHints)
        {
        }

        private static Expression<Func<bool, TProvider>> ToProvider(TProvider falseValue, TProvider trueValue)
        {
            var param = Expression.Parameter(typeof(bool), "v");
            return Expression.Lambda<Func<bool, TProvider>>(
                Expression.Condition(
                    param,
                    Expression.Constant(trueValue, typeof(TProvider)),
                    Expression.Constant(falseValue, typeof(TProvider))),
                param);
        }

        private static Expression<Func<TProvider, bool>> ToBool(TProvider trueValue)
        {
            var param = Expression.Parameter(typeof(TProvider), "v");
            return Expression.Lambda<Func<TProvider, bool>>(
                Expression.Equal(
                    param,
                    Expression.Constant(trueValue, typeof(TProvider))),
                param);
        }
    }
}
