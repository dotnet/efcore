// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="bool" /> values to and from two string values.
    /// </summary>
    public class BoolToStringConverter : BoolToTwoValuesConverter<string>
    {
        /// <summary>
        ///     Creates a new instance of this converter. A case-insensitive first character test is used
        ///     when converting from the store.
        /// </summary>
        /// <param name="falseValue"> The string to use for <see langword="false" />. </param>
        /// <param name="trueValue"> The string to use for <see langword="true" />. </param>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BoolToStringConverter(
            [NotNull] string falseValue,
            [NotNull] string trueValue,
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                Check.NotNull(falseValue, nameof(falseValue)),
                Check.NotNull(trueValue, nameof(trueValue)),
                FromProvider(trueValue),
                new ConverterMappingHints(size: Math.Max(falseValue.Length, trueValue.Length)).With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(bool),
                typeof(string),
                i => new BoolToStringConverter("0", "1", i.MappingHints),
                new ConverterMappingHints(size: 1));

        private static Expression<Func<string, bool>> FromProvider(string trueValue)
        {
            var testChar = trueValue.ToUpperInvariant()[0];

            return v => !string.IsNullOrEmpty(v)
                && v.ToUpperInvariant()[0] == testChar;
        }
    }
}
