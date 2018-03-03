// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> to and from strings.
    /// </summary>
    public class DateTimeToStringConverter : ValueConverter<DateTime, string>
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
        public DateTimeToStringConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF"),
                v => v == null ? default : DateTime.Parse(v, CultureInfo.InvariantCulture),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(DateTime), typeof(string), i => new DateTimeToStringConverter(i.MappingHints), _defaultHints);
    }
}
