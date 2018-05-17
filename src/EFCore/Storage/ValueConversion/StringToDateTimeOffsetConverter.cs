// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from <see cref="DateTime" /> values.
    /// </summary>
    public class StringToDateTimeOffsetConverter : ValueConverter<string, DateTimeOffset>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 48);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource"/> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToDateTimeOffsetConverter(
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v == null ? default : DateTimeOffset.Parse(v, CultureInfo.InvariantCulture),
                v => v.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz"),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(string), typeof(DateTimeOffset), i => new StringToDateTimeOffsetConverter(i.MappingHints), _defaultHints);
    }
}
