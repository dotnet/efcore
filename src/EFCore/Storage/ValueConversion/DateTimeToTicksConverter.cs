// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> to and <see cref="DateTime.Ticks" />.
    /// </summary>
    public class DateTimeToTicksConverter : ValueConverter<DateTime, long>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public DateTimeToTicksConverter()
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
        public DateTimeToTicksConverter(ConverterMappingHints? mappingHints)
            : base(
                v => v.Ticks,
                v => new DateTime(v),
                mappingHints)
        {
        }
    }
}
