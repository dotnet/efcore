// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.NetworkInformation;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="PhysicalAddress" /> to and from a <see cref="byte" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
    /// </remarks>
    public class PhysicalAddressToBytesConverter : ValueConverter<PhysicalAddress?, byte[]?>
    {
        private static readonly ConverterMappingHints _defaultHints = new(size: 8);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
        /// </remarks>
        public PhysicalAddressToBytesConverter()
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
        public PhysicalAddressToBytesConverter(ConverterMappingHints? mappingHints)
            : base(
                v => v == null ? default : v.GetAddressBytes(),
                v => v == null ? default : new PhysicalAddress(v),
                convertsNulls: true,
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(PhysicalAddress), typeof(byte[]), i => new PhysicalAddressToBytesConverter(i.MappingHints), _defaultHints);
    }
}
