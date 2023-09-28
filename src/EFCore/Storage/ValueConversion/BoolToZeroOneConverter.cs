// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts <see cref="bool" /> values to and from <c>0</c> and <c>1</c>.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class BoolToZeroOneConverter<TProvider> : BoolToTwoValuesConverter<TProvider>
{
    /// <summary>
    ///     Creates a new instance of this converter. This converter preserves order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public BoolToZeroOneConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter. This converter preserves order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public BoolToZeroOneConverter(ConverterMappingHints? mappingHints)
        : base(Zero(), One(), null, mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(bool), typeof(TProvider), i => new BoolToZeroOneConverter<TProvider>(i.MappingHints));

    private static TProvider Zero()
    {
        var type = typeof(TProvider).UnwrapNullableType();

        CheckTypeSupported(
            type,
            typeof(BoolToZeroOneConverter<TProvider>),
            typeof(int), typeof(short), typeof(long), typeof(sbyte),
            typeof(uint), typeof(ushort), typeof(ulong), typeof(byte),
            typeof(decimal), typeof(double), typeof(float));

        return (TProvider)(type == typeof(int)
            ? 0
            : type == typeof(short)
                ? (short)0
                : type == typeof(long)
                    ? (long)0
                    : type == typeof(sbyte)
                        ? (sbyte)0
                        : type == typeof(uint)
                            ? (uint)0
                            : type == typeof(ushort)
                                ? (ushort)0
                                : type == typeof(ulong)
                                    ? (ulong)0
                                    : type == typeof(byte)
                                        ? (byte)0
                                        : type == typeof(decimal)
                                            ? (decimal)0
                                            : type == typeof(double)
                                                ? (double)0
                                                : type == typeof(float)
                                                    ? (float)0
                                                    : (object)0);
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
