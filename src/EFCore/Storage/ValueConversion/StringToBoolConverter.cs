// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from <see cref="bool" /> values.
    /// </summary>
    public class StringToBoolConverter : ValueConverter<string, bool>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        public StringToBoolConverter()
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
        public StringToBoolConverter(ConverterMappingHints? mappingHints)
            : base(
                v => Convert.ToBoolean(v),
                v => Convert.ToString(v),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(string), typeof(bool), i => new StringToBoolConverter(i.MappingHints));
    }
}
