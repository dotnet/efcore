// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="DateTime" /> to and <see cref="DateTime.Ticks" />.
    /// </summary>
    public class DateTimeToTicksConverter : ValueConverter<DateTime, long>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public DateTimeToTicksConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v.Ticks,
                v => new DateTime(v),
                mappingHints)
        {
        }
    }
}
