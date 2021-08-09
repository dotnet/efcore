// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTimeOffset" /> to and from strings.
    /// </summary>
    public class DateTimeOffsetToStringConverter : StringDateTimeOffsetConverter<DateTimeOffset, string>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public DateTimeOffsetToStringConverter()
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
        public DateTimeOffsetToStringConverter(ConverterMappingHints? mappingHints)
            : base(
                ToString(),
                ToDateTimeOffset(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(DateTimeOffset), typeof(string), i => new DateTimeOffsetToStringConverter(i.MappingHints), _defaultHints);
    }
}
