// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts enum values to and from their underlying numeric representation.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class EnumToNumberConverter<TEnum, TNumber> : ValueConverter<TEnum, TNumber>
    where TEnum : struct, Enum
    where TNumber : struct
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConverterMappingHints? DefaultHints = CreateDefaultHints();

    private static ConverterMappingHints? CreateDefaultHints()
    {
        var underlyingModelType = typeof(TEnum).UnwrapEnumType();

        return (underlyingModelType == typeof(long) || underlyingModelType == typeof(ulong))
            && typeof(TNumber) == typeof(decimal)
                ? new ConverterMappingHints(precision: 20, scale: 0)
                : default;
    }

    /// <summary>
    ///     Creates a new instance of this converter. This converter preserves order.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    /// </remarks>
    public EnumToNumberConverter()
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
    public EnumToNumberConverter(ConverterMappingHints? mappingHints)
        : base(
            ToNumber(),
            ToEnum(),
            DefaultHints?.With(mappingHints) ?? mappingHints)
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(TEnum), typeof(TNumber), i => new EnumToNumberConverter<TEnum, TNumber>(i.MappingHints), DefaultHints);

    private static Expression<Func<TEnum, TNumber>> ToNumber()
    {
        if (!typeof(TEnum).UnwrapNullableType().IsEnum)
        {
            throw new InvalidOperationException(
                CoreStrings.ConverterBadType(
                    typeof(EnumToNumberConverter<TEnum, TNumber>).ShortDisplayName(),
                    typeof(TEnum).ShortDisplayName(),
                    "enum types"));
        }

        CheckTypeSupported(
            typeof(TNumber).UnwrapNullableType(),
            typeof(EnumToNumberConverter<TEnum, TNumber>),
            typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte),
            typeof(double), typeof(float), typeof(decimal));

        var param = Expression.Parameter(typeof(TEnum), "value");

        return Expression.Lambda<Func<TEnum, TNumber>>(
            Expression.Convert(
                typeof(TNumber) == typeof(decimal)
                    ? Expression.Convert(param, typeof(long))
                    : param,
                typeof(TNumber)), param);
    }

    private static Expression<Func<TNumber, TEnum>> ToEnum()
    {
        var param = Expression.Parameter(typeof(TNumber), "value");
        return Expression.Lambda<Func<TNumber, TEnum>>(
            Expression.Convert(
                typeof(TNumber) == typeof(decimal)
                    ? Expression.Convert(param, typeof(long))
                    : param,
                typeof(TEnum)), param);
    }
}
