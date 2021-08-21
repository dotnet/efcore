// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from <see cref="DateTime" /> values.
    /// </summary>
    public class StringToDateTimeConverter : StringDateTimeConverter<string, DateTime>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public StringToDateTimeConverter()
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
        public StringToDateTimeConverter(ConverterMappingHints? mappingHints)
            : base(
                ToDateTime(),
                ToString(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(DateTime), i => new StringToDateTimeConverter(i.MappingHints), _defaultHints);
    }
}
