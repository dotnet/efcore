// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Net;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="IPAddress" /> to and from a <see cref="string" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
    /// </remarks>
    public class IPAddressToStringConverter : ValueConverter<IPAddress?, string?>
    {
        // IPv4-mapped IPv6 addresses can go up to 45 bytes, e.g. 0000:0000:0000:0000:0000:ffff:192.168.1.1
        private static readonly ConverterMappingHints _defaultHints = new(size: 45);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public IPAddressToStringConverter()
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
        /// </param>
        public IPAddressToStringConverter(ConverterMappingHints? mappingHints)
            : base(
                ToString(),
                ToIPAddress(),
                convertsNulls: true,
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(IPAddress), typeof(string), i => new IPAddressToStringConverter(i.MappingHints), _defaultHints);

        private new static Expression<Func<IPAddress?, string?>> ToString()
            => v => v == null ? default : v.ToString();

        private static Expression<Func<string?, IPAddress?>> ToIPAddress()
            => v => v == null ? default : IPAddress.Parse(v);
    }
}
