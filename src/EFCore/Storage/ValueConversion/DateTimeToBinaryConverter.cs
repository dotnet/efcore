// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> using <see cref="DateTime.ToBinary" />. This
    ///     will preserve the <see cref="DateTimeKind" />.
    /// </summary>
    public class DateTimeToBinaryConverter : ValueConverter<DateTime, long>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public DateTimeToBinaryConverter(ConverterMappingHints? mappingHints = null)
            : base(
                v => v.ToBinary(),
                v => DateTime.FromBinary(v),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(DateTime), typeof(long), i => new DateTimeToBinaryConverter(i.MappingHints));
    }
}
