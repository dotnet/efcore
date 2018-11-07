// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="Guid" /> to and from an array of <see cref="byte" />.
    /// </summary>
    public class GuidToBytesConverter : ValueConverter<Guid, byte[]>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(
                size: 16,
                valueGeneratorFactory: (p, t) => new SequentialGuidValueGenerator());

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
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public GuidToBytesConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v.ToByteArray(),
                v => v == null ? Guid.Empty : new Guid(v),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(Guid), typeof(byte[]), i => new GuidToBytesConverter(i.MappingHints), _defaultHints);
    }
}
