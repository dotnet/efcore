// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion
{
    /// <summary>
    ///     Converts numeric values to and from arrays of bytes.
    /// </summary>
    public class NumberToBytesConverter<TNumber> : ValueConverter<TNumber, byte[]>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConverterMappingHints _defaultHints
            = new ConverterMappingHints(size: GetByteCount());

        /// <summary>
        ///     <para>
        ///         Creates a new instance of this converter.
        ///     </para>
        ///     <para>
        ///         This converter supports <see cref="double" />, <see cref="float" />, <see cref="decimal" />,
        ///         <see cref="int" />, <see cref="long" />, <see cref="short" />, <see cref="byte" />,
        ///         <see cref="uint" />, <see cref="ulong" />, <see cref="ushort" />, <see cref="sbyte" />,
        ///         and <see cref="char" />.
        ///     </para>
        /// </summary>
        /// <param name="mappingHints">
        ///     Hints that can be used by the <see cref="ITypeMappingSource" /> to create data types with appropriate
        ///     facets for the converted data.
        /// </param>
        public NumberToBytesConverter([CanBeNull] ConverterMappingHints mappingHints = null)
            : base(ToBytes(), ToNumber(), _defaultHints.With(mappingHints))
        {
        }

        /// <summary>
        ///     A <see cref="ValueConverterInfo" /> for the default use of this converter.
        /// </summary>
        public static ValueConverterInfo DefaultInfo { get; }
            = new ValueConverterInfo(
                typeof(TNumber), typeof(byte[]), i => new NumberToBytesConverter<TNumber>(i.MappingHints), _defaultHints);

        private static Expression<Func<TNumber, byte[]>> ToBytes()
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
                            _toBytesMethod,
                            input)
                        : EnsureEndian(
                            Expression.Call(
                                typeof(BitConverter).GetMethod(
                                    nameof(BitConverter.GetBytes),
                                    new[] { type }),
                                input));

            if (typeof(TNumber).IsNullableType())
            {
                output = Expression.Condition(
                    Expression.Property(
                        param,
                        typeof(TNumber).GetProperty(nameof(Nullable<int>.HasValue))),
                    output,
                    Expression.Constant(null, typeof(byte[])));
            }

            return Expression.Lambda<Func<TNumber, byte[]>>(output, param);
        }

        private static Expression<Func<byte[], TNumber>> ToNumber()
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
                            _toDecimalMethod,
                            param)
                        : (Expression)Expression.Call(
                            typeof(BitConverter).GetMethod(
                                "To" + type.Name,
                                new[] { typeof(byte[]), typeof(int) }),
                            EnsureEndian(param),
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

        private static Expression EnsureEndian(Expression expression)
        {
            if (!BitConverter.IsLittleEndian)
            {
                return expression;
            }

            switch (GetByteCount())
            {
                case 8:
                    return Expression.Call(_reverseLongMethod, expression);
                case 4:
                    return Expression.Call(_reverseIntMethod, expression);
                case 2:
                    return Expression.Call(_reverseShortMethod, expression);
                default:
                    return expression;
            }
        }

        private static readonly MethodInfo _reverseLongMethod
            = typeof(NumberToBytesConverter<TNumber>).GetMethod(
                nameof(ReverseLong),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo _reverseIntMethod
            = typeof(NumberToBytesConverter<TNumber>).GetMethod(
                nameof(ReverseInt),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo _reverseShortMethod
            = typeof(NumberToBytesConverter<TNumber>).GetMethod(
                nameof(ReverseShort),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static byte[] ReverseLong(byte[] bytes)
            => new[] { bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0] };

        private static byte[] ReverseInt(byte[] bytes)
            => new[] { bytes[3], bytes[2], bytes[1], bytes[0] };

        private static byte[] ReverseShort(byte[] bytes)
            => new[] { bytes[1], bytes[0] };

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

        private static readonly MethodInfo _toBytesMethod
            = typeof(NumberToBytesConverter<TNumber>).GetMethod(
                nameof(DecimalToBytes),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static byte[] DecimalToBytes(decimal value)
        {
            var bits = decimal.GetBits(value);

            var bytes = new byte[16];
            Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[0])), 0, bytes, 12, 4);
            Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[1])), 0, bytes, 8, 4);
            Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[2])), 0, bytes, 4, 4);
            Array.Copy(EnsureEndianInt(BitConverter.GetBytes(bits[3])), 0, bytes, 0, 4);

            return bytes;
        }

        private static readonly MethodInfo _toDecimalMethod
            = typeof(NumberToBytesConverter<TNumber>).GetMethod(
                nameof(BytesToDecimal),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static decimal BytesToDecimal(byte[] bytes)
        {
            var gotBytes = bytes;
            if (BitConverter.IsLittleEndian)
            {
                gotBytes = new byte[16];
                Array.Copy(bytes, gotBytes, 16);
                Array.Reverse(gotBytes, 0, 4);
                Array.Reverse(gotBytes, 4, 4);
                Array.Reverse(gotBytes, 8, 4);
                Array.Reverse(gotBytes, 12, 4);
            }

            var specialBits = BitConverter.ToUInt32(gotBytes, 0);

            return new decimal(
                BitConverter.ToInt32(gotBytes, 12),
                BitConverter.ToInt32(gotBytes, 8),
                BitConverter.ToInt32(gotBytes, 4),
                (specialBits & 0x80000000) != 0,
                (byte)((specialBits & 0x00FF0000) >> 16));
        }
    }
}
