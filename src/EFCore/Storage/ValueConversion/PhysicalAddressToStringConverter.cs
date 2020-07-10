// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts a <see cref="PhysicalAddress" /> to and from a <see cref="string" />.
    /// </summary>
    public class PhysicalAddressToStringConverter : ValueConverter<PhysicalAddress, string>
    {
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: 20);

        /// <summary>
        ///     Creates a new instance of this converter.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public PhysicalAddressToStringConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(
                ToString(),
                ToPhysicalAddress(),
                _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(PhysicalAddress),
                typeof(string),
                i => new PhysicalAddressToStringConverter(i.MappingHints),
                _defaultHints);

        private static new Expression<Func<PhysicalAddress, string>> ToString()
            => v => v == null ? default : v.ToString();

        private static Expression<Func<string, PhysicalAddress>> ToPhysicalAddress()
            => v => v == null ? default : PhysicalAddress.Parse(v);
    }
}
