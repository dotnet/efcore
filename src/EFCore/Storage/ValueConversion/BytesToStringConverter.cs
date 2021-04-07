// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts arrays of bytes to and from strings.
    /// </summary>
    public class BytesToStringConverter : ValueConverter<byte[], string>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BytesToStringConverter(
            ConverterMappingHints? mappingHints = null)
            : base(
                // TODO-NULLABLE: Null is already sanitized externally, clean up as part of #13850
                v => v == null ? null! : Convert.ToBase64String(v),
                v => v == null ? null! : Convert.FromBase64String(v),
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
