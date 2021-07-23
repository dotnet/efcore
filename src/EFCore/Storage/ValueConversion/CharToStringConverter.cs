// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="char" /> to and from a single-character <see cref="string" />.
    /// </summary>
    public class CharToStringConverter : StringCharConverter<char, string>
    {
        private static readonly ConverterMappingHints _defaultHints = new(size: 1);

        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public CharToStringConverter(ConverterMappingHints? mappingHints = null)
            : base(
                ToString(),
                ToChar(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(char), typeof(string), i => new CharToStringConverter(i.MappingHints), _defaultHints);
    }
}
