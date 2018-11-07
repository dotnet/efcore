// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

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
        public DateTimeToBinaryConverter([CanBeNull] ConverterMappingHints mappingHints = null)
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
            = new ValueConverterInfo(typeof(DateTime), typeof(long), i => new DateTimeToBinaryConverter(i.MappingHints));
    }
}
