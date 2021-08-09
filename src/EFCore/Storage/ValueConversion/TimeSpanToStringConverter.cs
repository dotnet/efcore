// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="TimeSpan" /> to and from strings.
    /// </summary>
    public class TimeSpanToStringConverter : StringTimeSpanConverter<TimeSpan, string>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public TimeSpanToStringConverter()
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
        public TimeSpanToStringConverter(ConverterMappingHints? mappingHints)
            : base(
                ToString(),
                ToTimeSpan(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(TimeSpan), typeof(string), i => new TimeSpanToStringConverter(i.MappingHints), _defaultHints);
    }
}
