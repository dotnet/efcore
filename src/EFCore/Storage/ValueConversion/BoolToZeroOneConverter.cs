// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts <see cref="bool" /> values to and from <c>0</c> and <c>1</c>.
    /// </summary>
    public class BoolToZeroOneConverter<TProvider> : BoolToTwoValuesConverter<TProvider>
    {
        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BoolToZeroOneConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(Zero(), One(), null, mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(bool), typeof(TProvider), i => new BoolToZeroOneConverter<TProvider>(i.MappingHints));

        private static TProvider Zero()
        {
            CheckTypeSupported(
                typeof(TProvider).UnwrapNullableType(),
                typeof(BoolToZeroOneConverter<TProvider>),
                typeof(int), typeof(short), typeof(long), typeof(sbyte),
                typeof(uint), typeof(ushort), typeof(ulong), typeof(byte),
                typeof(decimal), typeof(double), typeof(float));

            return Activator.CreateInstance<TProvider>();
        }

        private static TProvider One()
        {
            var type = typeof(TProvider).UnwrapNullableType();

            return (TProvider)(type == typeof(int)
                ? 1
                : type == typeof(short)
                    ? (short)1
                    : type == typeof(long)
                        ? (long)1
                        : type == typeof(sbyte)
                            ? (sbyte)1
                            : type == typeof(uint)
                                ? (uint)1
                                : type == typeof(ushort)
                                    ? (ushort)1
                                    : type == typeof(ulong)
                                        ? (ulong)1
                                        : type == typeof(byte)
                                            ? (byte)1
                                            : type == typeof(decimal)
                                                ? (decimal)1
                                                : type == typeof(double)
                                                    ? (double)1
                                                    : type == typeof(float)
                                                        ? (float)1
                                                        : (object)1);
        }
    }
}
