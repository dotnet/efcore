// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="IPAddress" /> to and from a <see cref="byte" />.
    /// </summary>
    public class IPAddressToBytesConverter : ValueConverter<IPAddress, byte[]>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 16);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public IPAddressToBytesConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v == null ? default : v.GetAddressBytes(),
                v => v == null ? default : new IPAddress(v),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(IPAddress),
                typeof(byte[]),
                i => new IPAddressToBytesConverter(i.MappingHints),
                _defaultHints);
    }
}
