// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Converts arrays of bytes to and from strings.
    /// </summary>
    public class BytesToStringConverter : ValueConverter<byte[], string>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(sizeFunction: s =>
            {
                var expanded = (int)((s - 1)  * 1.4);
                return expanded + (4 - (expanded % 4));
            });

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BytesToStringConverter(
            ConverterMappingHints mappingHints = default)
            : base(
                v => v == null ? null : Convert.ToBase64String(v),
                v => v == null ? null : Convert.FromBase64String(v),
                mappingHints.With(_defaultHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(byte[]), typeof(string), i => new BytesToStringConverter(i.MappingHints), _defaultHints);
    }
}
