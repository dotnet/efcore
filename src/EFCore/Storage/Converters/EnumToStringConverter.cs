// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Converts enum values to and from their string representation.
    /// </summary>
    public class EnumToStringConverter<TEnum> : ValueConverter<TEnum, string>
        where TEnum : struct
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 512);

        /// <summary>
        ///     Creates a new instance of this converter. This converter does not preserve order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public EnumToStringConverter(ConverterMappingHints mappingHints = default)
            : base(v => v.ToString(), ToEnum(), mappingHints.With(_defaultHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(TEnum), typeof(string), i => new EnumToStringConverter<TEnum>(i.MappingHints), _defaultHints);

        private static Expression<Func<string, TEnum>> ToEnum()
        {
            if (!typeof(TEnum).UnwrapNullableType().IsEnum)
            {
                throw new InvalidOperationException(
                    CoreStrings.ConverterBadType(
                        typeof(EnumToStringConverter<TEnum>).ShortDisplayName(),
                        typeof(TEnum).ShortDisplayName(),
                        "enum types"));
            }

            return v => ConvertToEnum(v);
        }

        private static TEnum ConvertToEnum(string value)
            => Enum.TryParse<TEnum>(value, out var result)
                ? result
                : Enum.TryParse(value, true, out result)
                    ? result
                    : ulong.TryParse(value, out var ulongValue)
                        ? (TEnum)(object)ulongValue
                        : long.TryParse(value, out var longValue)
                            ? (TEnum)(object)longValue
                            : default;
    }
}
