// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Converts <c>TModel</c> to and from <c>TStore</c> using simple casts from one type
    ///     to the other.
    /// </summary>
    public class CastingConverter<TModel, TStore> : ValueConverter<TModel, TStore>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConverterMappingHints _defaultHints = CreateDefaultHints();

        private static ConverterMappingHints CreateDefaultHints()
        {
            if (typeof(TStore).UnwrapNullableType() == typeof(decimal))
            {
                var underlyingModelType = typeof(TModel).UnwrapNullableType().UnwrapEnumType();

                if (underlyingModelType == typeof(long)
                    || underlyingModelType == typeof(ulong))
                {
                    return new ConverterMappingHints(precision: 20, scale: 0);
                }

                if (underlyingModelType == typeof(float)
                    || underlyingModelType == typeof(double))
                {
                    return new ConverterMappingHints(precision: 38, scale: 17);
                }
            }

            return default;
        }

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public CastingConverter(ConverterMappingHints mappingHints = default)
            : base(Convert<TModel, TStore>(), Convert<TStore, TModel>(), mappingHints.With(_defaultHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(TModel), typeof(TStore), i => new CastingConverter<TModel, TStore>(i.MappingHints), _defaultHints);

        private static Expression<Func<TIn, TOut>> Convert<TIn, TOut>()
        {
            var param = Expression.Parameter(typeof(TIn), "v");
            return Expression.Lambda<Func<TIn, TOut>>(
                Expression.Convert(param, typeof(TOut)),
                param);
        }
    }
}
