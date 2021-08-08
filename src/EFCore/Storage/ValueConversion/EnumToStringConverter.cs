// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts enum values to and from their string representation.
    /// </summary>
    public class EnumToStringConverter<TEnum> : StringEnumConverter<TEnum, string, TEnum>
        where TEnum : struct
    {
        /// <summary>
        ///     Creates a new instance of this converter. This converter does not preserve order.
        /// </summary>
        public EnumToStringConverter()
            : this(null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this converter. This converter does not preserve order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public EnumToStringConverter(ConverterMappingHints? mappingHints)
            : base(
                ToString(),
                ToEnum(),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(TEnum), typeof(string), i => new EnumToStringConverter<TEnum>(i.MappingHints));
    }
}
