// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts strings to and from a <see cref="Guid" /> using the
    ///     standard "8-4-4-4-12" format./>.
    /// </summary>
    public class StringToGuidConverter : ValueConverter<string, Guid>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(
                size: 36,
                valueGeneratorFactory: (p, t) => new SequentialGuidValueGenerator());

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource"/> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public StringToGuidConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                v => v == null ? default : new Guid(v),
                v => v.ToString("D"),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(string), typeof(Guid), i => new StringToGuidConverter(i.MappingHints), _defaultHints);
    }
}
