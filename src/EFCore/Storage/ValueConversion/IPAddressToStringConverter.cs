// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Net;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="IPAddress" /> to and from a <see cref="string" />.
    /// </summary>
    public class IPAddressToStringConverter : ValueConverter<IPAddress, string>
    {
        // IPv4-mapped IPv6 addresses can go up to 45 bytes, e.g. 0000:0000:0000:0000:0000:ffff:192.168.1.1
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 45);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public IPAddressToStringConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                ToString(),
                ToIPAddress(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(IPAddress),
                typeof(string), i => new IPAddressToStringConverter(i.MappingHints),
                _defaultHints);

        private static new Expression<Func<IPAddress, string>> ToString()
            => v => v == null ? default : v.ToString();

        private static Expression<Func<string, IPAddress>> ToIPAddress()
            => v => v == null ? default : IPAddress.Parse(v);
    }
}
