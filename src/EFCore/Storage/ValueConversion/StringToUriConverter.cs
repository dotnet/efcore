// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from <see cref="Uri" /> values.
    /// </summary>
    public class StringToUriConverter : StringUriConverter<string?, Uri?>
    {
        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        public StringToUriConverter()
            : this(null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToUriConverter(ConverterMappingHints? mappingHints)
            : base(
                ToUri(),
                ToString(),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(Uri), i => new StringToUriConverter(i.MappingHints));
    }
}
