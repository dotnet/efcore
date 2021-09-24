// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> to and from binary representation in a long.
    ///     The DateTime is truncated beyond 0.1 millisecond precision.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
    /// </remarks>
    public class DateTimeOffsetToBinaryConverter : ValueConverter<DateTimeOffset, long>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
        /// </remarks>
        public DateTimeOffsetToBinaryConverter()
            : this(null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
        /// </remarks>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        ///</param>
        public DateTimeOffsetToBinaryConverter(ConverterMappingHints? mappingHints)
            : base(
                v => ((v.Ticks / 1000) << 11) | ((long)v.Offset.TotalMinutes & 0x7FF),
                v => new DateTimeOffset(
                    new DateTime((v >> 11) * 1000),
                    new TimeSpan(0, (int)((v << 53) >> 53), 0)),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(DateTimeOffset), typeof(long), i => new DateTimeOffsetToBinaryConverter(i.MappingHints));
    }
}
