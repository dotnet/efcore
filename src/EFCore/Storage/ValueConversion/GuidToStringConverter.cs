// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="Guid" /> to and from a <see cref="string" /> using the
    ///     standard "8-4-4-4-12" format./>.
    /// </summary>
    public class GuidToStringConverter : ValueConverter<Guid, string>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 36);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource"/> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public GuidToStringConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v.ToString("D"),
                v => v == null ? Guid.Empty : new Guid(v),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(Guid), typeof(string), i => new GuidToStringConverter(i.MappingHints), _defaultHints);
    }
}
