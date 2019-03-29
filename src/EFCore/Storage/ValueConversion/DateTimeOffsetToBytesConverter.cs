// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> to and from arrays of bytes.
    /// </summary>
    public class DateTimeOffsetToBytesConverter : ValueConverter<DateTimeOffset, byte[]>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 12);

        private static readonly NumberToBytesConverter<long> _longToBytes
            = new NumberToBytesConverter<long>();

        private static readonly NumberToBytesConverter<short> _shortToBytes
            = new NumberToBytesConverter<short>();

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public DateTimeOffsetToBytesConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => ToBytes(v),
                v => v == null ? default : FromBytes(v),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(DateTimeOffset), typeof(byte[]), i => new DateTimeOffsetToBytesConverter(i.MappingHints), _defaultHints);

        private static byte[] ToBytes(DateTimeOffset value)
        {
            var timeBytes = (byte[])_longToBytes.ConvertToProvider(value.DateTime.ToBinary())!;
            var offsetBytes = (byte[])_shortToBytes.ConvertToProvider(value.Offset.TotalMinutes)!;
            return timeBytes.Concat(offsetBytes).ToArray();
        }

        private static DateTimeOffset FromBytes(byte[] bytes)
        {
            var timeBinary = (long)_longToBytes.ConvertFromProvider(bytes);
            var offsetMins = (short)_shortToBytes.ConvertFromProvider(bytes.Skip(8).ToArray());
            return new DateTimeOffset(DateTime.FromBinary(timeBinary), new TimeSpan(0, offsetMins, 0));
        }
    }
}
