// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="TimeSpan" /> to and <see cref="TimeSpan.Ticks" />.
    /// </summary>
    public class TimeSpanToTicksConverter : ValueConverter<TimeSpan, long>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public TimeSpanToTicksConverter()
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
        public TimeSpanToTicksConverter(ConverterMappingHints? mappingHints)
            : base(v => v.Ticks, v => new TimeSpan(v), mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(TimeSpan), typeof(long), i => new TimeSpanToTicksConverter(i.MappingHints));
    }
}
