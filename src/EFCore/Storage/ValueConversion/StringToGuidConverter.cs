// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from a <see cref="Guid" /> using the
    ///     standard "8-4-4-4-12" format./>.
    /// </summary>
    public class StringToGuidConverter : StringGuidConverter<string, Guid>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToGuidConverter(ConverterMappingHints? mappingHints = null)
            : base(
                ToGuid(),
                ToString(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(Guid), i => new StringToGuidConverter(i.MappingHints), _defaultHints);
    }
}
