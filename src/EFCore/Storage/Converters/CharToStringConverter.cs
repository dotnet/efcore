// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Converts a <see cref="char" /> to and from a single-character <see cref="string" />.
    /// </summary>
    public class CharToStringConverter : ValueConverter<char, string>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 1);

        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public CharToStringConverter(ConverterMappingHints mappingHints = default)
            : base(
                v => string.Format(CultureInfo.InvariantCulture, "{0}", v),
                v => v != null && v.Length >= 1 ? v[0] : (char)0,
                mappingHints.With(_defaultHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(char), typeof(string), i => new CharToStringConverter(i.MappingHints), _defaultHints);
    }
}
