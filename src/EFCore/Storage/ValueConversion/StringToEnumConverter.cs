// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
        public StringToEnumConverter([CanBeNull] ConverterMappingHints mappingHints = null)
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
            = new ValueConverterInfo(typeof(string), typeof(TEnum), i => new StringToEnumConverter<TEnum>(i.MappingHints));
    }
}
