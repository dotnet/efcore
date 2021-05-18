// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="TimeSpan" /> to and <see cref="TimeSpan.TotalHours" />.
    /// </summary>
    public class TimeSpanToHoursConverter : ValueConverter<TimeSpan, double>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public TimeSpanToHoursConverter(ConverterMappingHints? mappingHints = null)
            : base(v => v.TotalHours, v => TimeSpan.FromHours(v), mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new(typeof(TimeSpan), typeof(double), i => new TimeSpanToHoursConverter(i.MappingHints));
    }
}
