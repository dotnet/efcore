// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts numeric values to and from their string representation.
    /// </summary>
    public class NumberToStringConverter<TNumber> : StringNumberConverter<TNumber, string, TNumber>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public NumberToStringConverter(
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                ToString(),
                ToNumber(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(TNumber), typeof(string), i => new NumberToStringConverter<TNumber>(i.MappingHints), _defaultHints);
    }
}
