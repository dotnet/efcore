// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from <see cref="bool" /> values.
    /// </summary>
    public class StringToBoolConverter : ValueConverter<string, bool>
    {
        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToBoolConverter(
            [CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => Convert.ToBoolean(v),
                v => Convert.ToString(v),
                mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(string), typeof(bool), i => new StringToBoolConverter(i.MappingHints));
    }
}
