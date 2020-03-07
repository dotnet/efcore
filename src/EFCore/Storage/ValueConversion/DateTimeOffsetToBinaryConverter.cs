// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> to and from binary representation in a long.
    ///     The DateTime is truncated beyond 0.1 millisecond precision.
    /// </summary>
    public class DateTimeOffsetToBinaryConverter : ValueConverter<DateTimeOffset, long>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public DateTimeOffsetToBinaryConverter([CanBeNull] ConverterMappingHints mappingHints = null)
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
            = new ValueConverterInfo(typeof(DateTimeOffset), typeof(long), i => new DateTimeOffsetToBinaryConverter(i.MappingHints));
    }
}
