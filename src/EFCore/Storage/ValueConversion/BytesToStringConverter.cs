// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts arrays of bytes to and from strings.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
    /// </remarks>
    public class BytesToStringConverter : ValueConverter<byte[]?, string?>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information.
        /// </remarks>
        public BytesToStringConverter()
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
        ///</param>
        public BytesToStringConverter(ConverterMappingHints? mappingHints)
            : base(
                v => v == null ? null : Convert.ToBase64String(v),
                v => v == null ? null : Convert.FromBase64String(v),
                convertsNulls: true,
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(byte[]), typeof(string), i => new BytesToStringConverter(i.MappingHints));
    }
}
