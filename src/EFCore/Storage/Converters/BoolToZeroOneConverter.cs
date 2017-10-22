// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Storage.Converters
{
    /// <summary>
    ///     Converts <see cref="bool" /> values to and from <c>0</c> and <c>1</c>.
    /// </summary>
    public class BoolToZeroOneConverter<TStore> : BoolToTwoValuesConverter<TStore>
    {
        /// <summary>
        ///     Creates a new instance of this converter. This converter preserves order.
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the type mapper to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public BoolToZeroOneConverter(ConverterMappingHints mappingHints = default)
            : base(Zero(), One(), null, mappingHints)
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(typeof(bool), typeof(TStore), i => new BoolToZeroOneConverter<TStore>(i.MappingHints));

        private static TStore Zero()
        {
            CheckTypeSupported(
                typeof(TStore).UnwrapNullableType(),
                typeof(BoolToZeroOneConverter<TStore>),
                typeof(int), typeof(short), typeof(long), typeof(sbyte),
                typeof(uint), typeof(ushort), typeof(ulong), typeof(byte),
                typeof(decimal), typeof(double), typeof(float));

            return Activator.CreateInstance<TStore>();
        }

        private static TStore One()
        {
            var type = typeof(TStore).UnwrapNullableType();

            return (TStore)(type == typeof(int)
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
