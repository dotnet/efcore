// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="IPAddress" /> to and from a <see cref="string" />.
    /// </summary>
    public class IPAddressToStringConverter : StringIPAddressConverter<IPAddress, string>
    {
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
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(IPAddress), typeof(string), i => new IPAddressToStringConverter(i.MappingHints));
    }
}
