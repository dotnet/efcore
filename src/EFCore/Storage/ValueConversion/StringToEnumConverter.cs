// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from enum values.
    /// </summary>
    public class StringToEnumConverter<TEnum> : StringEnumConverter<string, TEnum, TEnum>
        where TEnum : struct
    {
        /// <summary>
        ///     Creates a new instance of this converter. This converter does not preserve order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToEnumConverter(ConverterMappingHints? mappingHints = null)
            : base(
                ToEnum(),
                ToString(),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(TEnum), i => new StringToEnumConverter<TEnum>(i.MappingHints));
    }
}
