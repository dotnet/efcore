// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
///     Converts numeric values to and from arrays of bytes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
/// </remarks>
public class NumberToBytesConverter<TNumber> : ValueConverter<TNumber, byte[]>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConverterMappingHints DefaultHints = new(size: GetByteCount());

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This converter supports <see cref="double" />, <see cref="float" />, <see cref="decimal" />,
    ///         <see cref="int" />, <see cref="long" />, <see cref="short" />, <see cref="byte" />,
    ///         <see cref="uint" />, <see cref="ulong" />, <see cref="ushort" />, <see cref="sbyte" />,
    ///         and <see cref="char" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public NumberToBytesConverter()
        : this(null)
    {
    }

    /// <summary>
    ///     Creates a new instance of this converter.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This converter supports <see cref="double" />, <see cref="float" />, <see cref="decimal" />,
    ///         <see cref="int" />, <see cref="long" />, <see cref="short" />, <see cref="byte" />,
    ///         <see cref="uint" />, <see cref="ulong" />, <see cref="ushort" />, <see cref="sbyte" />,
    ///         and <see cref="char" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-value-converters">EF Core value converters</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="mappingHints">
    ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
    ///     facets for the converted data.
    /// </param>
    public NumberToBytesConverter(ConverterMappingHints? mappingHints)
        : base(ToBytes(), ToNumber(), DefaultHints.With(mappingHints))
    {
    }

    /// <summary>
    ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
    /// </summary>
    public static ValueConverterInfo DefaultInfo { get; }
        = new(typeof(TNumber), typeof(byte[]), i => new NumberToBytesConverter<TNumber>(i.MappingHints), DefaultHints);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static Expression<Func<TNumber, byte[]>> ToBytes()
    {
        var type = typeof(TNumber).UnwrapNullableType();

        CheckTypeSupported(
            type,
            typeof(NumberToBytesConverter<TNumber>),
            typeof(double), typeof(float), typeof(decimal), typeof(char),
            typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(uint), typeof(ulong), typeof(ushort), typeof(sbyte));

        var param = Expression.Parameter(typeof(TNumber), "v");

        var input = typeof(TNumber).IsNullableType()
            ? Expression.Convert(param, type)
            : (Expression)param;

        var output = type == typeof(byte)
            ? Expression.NewArrayInit(typeof(byte), input)
            : type == typeof(sbyte)
                ? Expression.NewArrayInit(
                    typeof(byte),
                    Expression.Convert(input, typeof(byte)))
                : type == typeof(decimal)
                    ? Expression.Call(
                        ToBytesMethod,
                        input)
                    : EnsureEndian(
                        Expression.Call(
                            typeof(BitConverter).GetMethod(
                                nameof(BitConverter.GetBytes),
                                [type])!,
                            input));

        if (typeof(TNumber).IsNullableType())
        {
            output = Expression.Condition(
                Expression.Property(
                    param,
                    typeof(TNumber).GetProperty(nameof(Nullable<int>.HasValue))!),
                output,
                Expression.Constant(null, typeof(byte[])));
        }

        return Expression.Lambda<Func<TNumber, byte[]>>(output, param);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static Expression<Func<byte[], TNumber>> ToNumber()
    {
        var type = typeof(TNumber).UnwrapNullableType();
        var param = Expression.Parameter(typeof(byte[]), "v");

        var output = type == typeof(byte)
            ? Expression.ArrayAccess(param, Expression.Constant(0))
            : type == typeof(sbyte)
                ? Expression.Convert(
                    Expression.ArrayAccess(
                        param,
                        Expression.Constant(0)),
                    typeof(sbyte))
                : type == typeof(decimal)
                    ? Expression.Call(
                        ToDecimalMethod,
                        param)
                    : (Expression)Expression.Call(
                        typeof(BitConverter).GetMethod(
                            "To" + type.Name,
                            [typeof(byte[]), typeof(int)])!,
                        EnsureEndian(HandleEmptyArray(param)),
                        Expression.Constant(0));

        if (typeof(TNumber).IsNullableType())
        {
            output = Expression.Convert(output, typeof(TNumber));
        }

        return Expression.Lambda<Func<byte[], TNumber>>(
            Expression.Condition(
                Expression.ReferenceEqual(param, Expression.Constant(null)),
                Expression.Constant(default(TNumber), typeof(TNumber)),
                output),
            param);
    }

    private static Expression HandleEmptyArray(Expression expression)
    {
        if (!typeof(TNumber).IsInteger())
        {
            return expression;
        }

        return Expression.Condition(
            Expression.Equal(Expression.ArrayLength(expression), Expression.Constant(0)),
            Expression.NewArrayBounds(typeof(byte), Expression.Constant(GetByteCount())),
            expression);
    }

    private static Expression EnsureEndian(Expression expression)
    {
        if (!BitConverter.IsLittleEndian)
        {
            return expression;
        }

        return GetByteCount() switch
        {
            8 => Expression.Call(ReverseLongMethod, expression),
            4 => Expression.Call(ReverseIntMethod, expression),
            2 => Expression.Call(ReverseShortMethod, expression),
            _ => expression
        };
    }

    private static readonly MethodInfo ReverseLongMethod
        = typeof(NumberToBytesConverter<TNumber>).GetMethod(
            nameof(ReverseLong),
            BindingFlags.Static | BindingFlags.Public)!;

    private static readonly MethodInfo ReverseIntMethod
        = typeof(NumberToBytesConverter<TNumber>).GetMethod(
            nameof(ReverseInt),
            BindingFlags.Static | BindingFlags.Public)!;

    private static readonly MethodInfo ReverseShortMethod
        = typeof(NumberToBytesConverter<TNumber>).GetMethod(
            nameof(ReverseShort),
            BindingFlags.Static | BindingFlags.Public)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static byte[] ReverseLong(byte[] bytes)
        => [bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0]];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static byte[] ReverseInt(byte[] bytes)
        => [bytes[3], bytes[2], bytes[1], bytes[0]];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static byte[] ReverseShort(byte[] bytes)
        => [bytes[1], bytes[0]];

    private static int GetByteCount()
    {
        var type = typeof(TNumber).UnwrapNullableType();

        return type == typeof(decimal)
            ? 16
            : (type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(double)
                    ? 8
                    : (type == typeof(int)
                        || type == typeof(uint)
                        || type == typeof(float)
                            ? 4
                            : (type == typeof(short)
                                || type == typeof(ushort)
                                || type == typeof(char)
                                    ? 2
                                    : 1)));
    }

    private static byte[] EnsureEndianInt(byte[] bytes)
        => BitConverter.IsLittleEndian
            ? ReverseInt(bytes)
            : bytes;

    private static readonly MethodInfo ToBytesMethod
        = typeof(NumberToBytesConverter<TNumber>).GetMethod(
            nameof(DecimalToBytes),
            BindingFlags.Static | BindingFlags.Public)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static byte[] DecimalToBytes(decimal value)
    {
        var bits = decimal.GetBits(value);

        var bytes = new byte[16];
        Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[0])), 0, bytes, 12, 4);
        Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[1])), 0, bytes, 8, 4);
        Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[2])), 0, bytes, 4, 4);
        Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[3])), 0, bytes, 0, 4);

        return bytes;
    }

    private static readonly MethodInfo ToDecimalMethod
        = typeof(NumberToBytesConverter<TNumber>).GetMethod(
            nameof(BytesToDecimal),
            BindingFlags.Static | BindingFlags.Public)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static decimal BytesToDecimal(byte[] bytes)
    {
        Span<byte> gotBytes = BitConverter.IsLittleEndian ? stackalloc byte[16] : bytes;
        if (BitConverter.IsLittleEndian)
        {
            bytes.CopyTo(gotBytes);
            gotBytes.Slice(0, 4).Reverse();
            gotBytes.Slice(4, 4).Reverse();
            gotBytes.Slice(8, 4).Reverse();
            gotBytes.Slice(12, 4).Reverse();
        }

        var specialBits = BitConverter.ToUInt32(gotBytes);

        return new decimal(
            BitConverter.ToInt32(gotBytes.Slice(12)),
            BitConverter.ToInt32(gotBytes.Slice(8)),
            BitConverter.ToInt32(gotBytes.Slice(4)),
            (specialBits & 0x80000000) != 0,
            (byte)((specialBits & 0x00FF0000) >> 16));
    }
}
