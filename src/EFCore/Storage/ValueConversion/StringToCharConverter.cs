// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from <see cref="char" /> values.
    /// </summary>
    public class StringToCharConverter : StringCharConverter<string, char>
    {
        private static readonly ConverterMappingHints _defaultHints = new(size: 1);

        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToCharConverter(ConverterMappingHints? mappingHints = null)
            : base(
                ToChar(),
                ToString(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(char), i => new StringToCharConverter(i.MappingHints), _defaultHints);
    }
}
