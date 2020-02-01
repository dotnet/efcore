// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from numeric values.
    /// </summary>
    public class StringToNumberConverter<TNumber> : StringNumberConverter<string, TNumber, TNumber>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToNumberConverter(
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                ToNumber(),
                ToString(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(string), typeof(TNumber), i => new StringToNumberConverter<TNumber>(i.MappingHints), _defaultHints);
    }
}
