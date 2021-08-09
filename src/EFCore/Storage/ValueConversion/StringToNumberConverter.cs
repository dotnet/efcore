// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from numeric values.
    /// </summary>
    public class StringToNumberConverter<TNumber> : StringNumberConverter<string, TNumber, TNumber>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public StringToNumberConverter()
            : this(null)
        {
        }
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToNumberConverter(ConverterMappingHints? mappingHints)
            : base(
                ToNumber(),
                ToString(),
                typeof(TNumber).IsNullableType(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(TNumber), i => new StringToNumberConverter<TNumber>(i.MappingHints), _defaultHints);
    }
}
