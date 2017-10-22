// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Converts <see cref="bool" /> values to and from two different values.
    /// </summary>
    public class BoolToTwoValuesConverter<TStore> : ValueConverter<bool, TStore>
    {
        /// <summary>
        ///     <para>
        ///         Creates a new instance of this converter that will convert a <c>false</c> false
        ///         to one value and a <c>true</c> to another.
        ///     </para>
        ///     <para>
        ///         Use <see cref="BoolToZeroOneConverter{TStore}" /> for converting a <see cref="bool" /> to zero/one.
        ///     </para>
        /// </summary>
        /// <param name="falseValue"> The value to convert to for <c>false</c>. </param>
        /// <param name="trueValue"> The value to convert to for <c>true</c>. </param>
        /// <param name="fromStore"> Optional custom translator from store. </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BoolToTwoValuesConverter(
            [CanBeNull] TStore falseValue,
            [CanBeNull] TStore trueValue,
            [CanBeNull] Expression<Func<TStore, bool>> fromStore = null,
            ConverterMappingHints mappingHints = default)
            : base(ToStore(falseValue, trueValue), fromStore ?? ToBool(trueValue), mappingHints)
        {
        }

        private static Expression<Func<bool, TStore>> ToStore(TStore falseValue, TStore trueValue)
        {
            var param = Expression.Parameter(typeof(bool), "v");
            return Expression.Lambda<Func<bool, TStore>>(
                Expression.Condition(
                    param,
                    Expression.Constant(trueValue, typeof(TStore)),
                    Expression.Constant(falseValue, typeof(TStore))),
                param);
        }

        private static Expression<Func<TStore, bool>> ToBool(TStore trueValue)
        {
            var param = Expression.Parameter(typeof(TStore), "v");
            return Expression.Lambda<Func<TStore, bool>>(
                Expression.Equal(
                    param,
                    Expression.Constant(trueValue, typeof(TStore))),
                param);
        }
    }
}
