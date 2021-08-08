// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="IPAddress" /> to and from a <see cref="byte" />.
    /// </summary>
    public class IPAddressToBytesConverter : ValueConverter<IPAddress?, byte[]?>
    {
        private static readonly ConverterMappingHints _defaultHints = new(size: 16);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public IPAddressToBytesConverter()
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
        public IPAddressToBytesConverter(ConverterMappingHints? mappingHints)
            : base(
                v => v == null ? default : v.GetAddressBytes(),
                v => v == null ? default : new IPAddress(v),
                convertsNulls: true,
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(IPAddress), typeof(byte[]), i => new IPAddressToBytesConverter(i.MappingHints), _defaultHints);
    }
}
