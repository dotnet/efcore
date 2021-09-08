// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="Guid" /> to and from an array of <see cref="byte" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
    /// </remarks>
    public class GuidToBytesConverter : ValueConverter<Guid, byte[]>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new(size: 16, valueGeneratorFactory: (p, t) => new SequentialGuidValueGenerator());

        /// <summary>
        ///     <para>
        ///         Creates a new instance of this converter.
        ///     </para>
        ///     <para>
        ///         This converter does not preserve order because the ordering of bits in
        ///         the standard binary representation of a GUID does not match the ordering
        ///         in the standard string representation.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
        /// </remarks>
        public GuidToBytesConverter()
            : this(null)
        {
        }

        /// <summary>
        ///     <para>
        ///         Creates a new instance of this converter.
        ///     </para>
        ///     <para>
        ///         This converter does not preserve order because the ordering of bits in
        ///         the standard binary representation of a GUID does not match the ordering
        ///         in the standard string representation.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
        /// </remarks>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public GuidToBytesConverter(ConverterMappingHints? mappingHints)
            : base(
                v => v.ToByteArray(),
                v => new Guid(v),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(Guid), typeof(byte[]), i => new GuidToBytesConverter(i.MappingHints), _defaultHints);
    }
}
