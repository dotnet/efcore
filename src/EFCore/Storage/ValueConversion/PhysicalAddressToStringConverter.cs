// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Net.NetworkInformation;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="PhysicalAddress" /> to and from a <see cref="string" />.
    /// </summary>
    public class PhysicalAddressToStringConverter : ValueConverter<PhysicalAddress?, string?>
    {
        private static readonly ConverterMappingHints _defaultHints = new(size: 20);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public PhysicalAddressToStringConverter(ConverterMappingHints? mappingHints = null)
            : base(
                ToString(),
                ToPhysicalAddress(),
                convertsNulls: true,
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(PhysicalAddress), typeof(string), i => new PhysicalAddressToStringConverter(i.MappingHints), _defaultHints);

        private static new Expression<Func<PhysicalAddress?, string?>> ToString()
            => v => v == null ? default! : v.ToString();

        private static Expression<Func<string?, PhysicalAddress?>> ToPhysicalAddress()
            => v => v == null ? default! : PhysicalAddress.Parse(v);
    }
}
