// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="Guid" /> to and from a <see cref="string" /> using the
    ///     standard "8-4-4-4-12" format./>.
    /// </summary>
    public class GuidToStringConverter : StringGuidConverter<Guid, string>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public GuidToStringConverter()
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
        public GuidToStringConverter(ConverterMappingHints? mappingHints)
            : base(
                ToString(),
                ToGuid(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(Guid), typeof(string), i => new GuidToStringConverter(i.MappingHints), _defaultHints);
    }
}
