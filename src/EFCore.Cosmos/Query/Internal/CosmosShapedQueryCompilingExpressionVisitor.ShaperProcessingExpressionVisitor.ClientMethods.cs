// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor
    {
        private static readonly byte[] WhiteSpaceBytes = Encoding.UTF8.GetBytes(" \t");
        private static readonly byte EndArrayByte = Encoding.UTF8.GetBytes("]")[0];
        private static readonly byte NextItemByte = Encoding.UTF8.GetBytes(",")[0];

        public static bool TryMaterializeNextJsonCollectionItem<T>(
            QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Shaper<T> shaper,
            int ordinal,
            out int bytesConsumed,
            [NotNullWhen(true)] out T? result)
        {
            var startLength = data.Length;

            // Don't use Utf8JsonReader, because it will read the whole next token, which could be a big string.
            data = data.TrimStart(WhiteSpaceBytes);
            if (data.Span[0] == EndArrayByte)
            {
                bytesConsumed = startLength - data.Length + 1;
                result = default;
                return false;
            }

            bytesConsumed = startLength - data.Length;

            result = shaper(queryContext, data, ordinal, out var shaperBytesConsumed)!;

            // The shaper might be a constant expression, in which case it won't consume any bytes. In that case, we need to skip the next token.
            if (shaperBytesConsumed == 0)
            {
                var reader = new Utf8JsonReader(data.Span, isFinalBlock: true, default);
                reader.Read();
                reader.Skip();
                shaperBytesConsumed = (int)reader.BytesConsumed;
            }

            data = data.Slice(shaperBytesConsumed);
            bytesConsumed += shaperBytesConsumed;

            SliceNextItemToken(data, out var bytesConsumedNextItem);
            bytesConsumed += bytesConsumedNextItem;

            return true;
        }

        private static readonly MethodInfo ShapeCollectionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(ShapeCollection), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new UnreachableException();

        private static TCollection ShapeCollection<TElement, TCollection>(
            QueryContext queryContext,
            ReadOnlyMemory<byte> data,
            Func<object> creator,
            Shaper<TElement> shaper,
            out int bytesConsumed)
        {
            // TODO: throw a better exception for non ICollection navigations
            var collection = (ICollection<TElement>)creator();

            var reader = new Utf8JsonReader(data.Span);
            reader.Read();
            data = data.Slice((int)reader.BytesConsumed);
            bytesConsumed = (int)reader.BytesConsumed;

            if (reader.TokenType == JsonTokenType.Null)
            {
                // #35916
                return (TCollection)collection;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(reader.TokenType));
            }

            int itemBytesConsumed;
            while (TryMaterializeNextJsonCollectionItem(queryContext, data, shaper, collection.Count, out itemBytesConsumed, out var item))
            {
                collection.Add(item);
                data = data.Slice(itemBytesConsumed);
                bytesConsumed += itemBytesConsumed;
            }
            bytesConsumed += itemBytesConsumed; // ']'

            return (TCollection)collection;
        }

        private static readonly MethodInfo SliceNextItemTokenMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(SliceNextItemToken), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new UnreachableException();

        private static ReadOnlyMemory<byte> SliceNextItemToken(ReadOnlyMemory<byte> data, out int bytesConsumed)
        {
            var startLength = data.Length;

            data = data.TrimStart(WhiteSpaceBytes);
            if (data.Span[0] == NextItemByte)
            {
                bytesConsumed = startLength - data.Length + 1;
                return data.Slice(1);
            }

            bytesConsumed = startLength - data.Length;
            return data;
        }

        private static readonly MethodInfo NewJsonReaderInvalidTokenTypeExceptionMethodInfo
            = typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(NewJsonReaderInvalidTokenTypeException), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new UnreachableException();

        private static InvalidOperationException NewJsonReaderInvalidTokenTypeException(JsonTokenType jsonTokenType)
            => new(CoreStrings.JsonReaderInvalidTokenType(jsonTokenType));

        private static class OrdinalConverters
        {
            private static readonly MethodInfo ToBoolMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToBool), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToByteMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToByte), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToSByteMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToSByte), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToShortMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToShort), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToUShortMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToUShort), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToCharMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToChar), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToUIntMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToUInt), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToLongMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToLong), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToULongMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToULong), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToFloatMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToFloat), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToDoubleMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToDouble), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToDecimalMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToDecimal), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToStringMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToString), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToGuidMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToGuid), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToDateTimeMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToDateTime), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToDateTimeOffsetMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToDateTimeOffset), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToTimeSpanMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToTimeSpan), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToDateOnlyMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToDateOnly), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToTimeOnlyMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToTimeOnly), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            private static readonly MethodInfo ToBytesMethod =
                typeof(OrdinalConverters).GetMethod(nameof(ToBytes), BindingFlags.Public | BindingFlags.Static) ?? throw new UnreachableException();

            public static readonly FrozenDictionary<Type, MethodInfo?> ConvertMethods =
                new Dictionary<Type, MethodInfo?>
                {
                    [typeof(bool)] = ToBoolMethod,

                    [typeof(byte)] = ToByteMethod,
                    [typeof(sbyte)] = ToSByteMethod,

                    [typeof(short)] = ToShortMethod,
                    [typeof(ushort)] = ToUShortMethod,

                    [typeof(char)] = ToCharMethod,

                    [typeof(int)] = null,
                    [typeof(uint)] = ToUIntMethod,

                    [typeof(long)] = ToLongMethod,
                    [typeof(ulong)] = ToULongMethod,

                    [typeof(float)] = ToFloatMethod,
                    [typeof(double)] = ToDoubleMethod,
                    [typeof(decimal)] = ToDecimalMethod,

                    [typeof(string)] = ToStringMethod,
                    [typeof(Guid)] = ToGuidMethod,

                    [typeof(DateTime)] = ToDateTimeMethod,
                    [typeof(DateTimeOffset)] = ToDateTimeOffsetMethod,
                    [typeof(TimeSpan)] = ToTimeSpanMethod,

                    [typeof(DateOnly)] = ToDateOnlyMethod,
                    [typeof(TimeOnly)] = ToTimeOnlyMethod,

                    [typeof(byte[])] = ToBytesMethod,
                }.ToFrozenDictionary();

            public static bool ToBool(int value) => value switch
            {
                0 => false,
                1 => true,
                _ => ThrowTooFewValues<bool>(value)
            };

            public static byte ToByte(int value)
                => value < byte.MinValue || value > byte.MaxValue ? ThrowTooFewValues<byte>(value) : (byte)value;

            public static sbyte ToSByte(int value) => value < sbyte.MinValue || value > sbyte.MaxValue ? ThrowTooFewValues<sbyte>(value) : (sbyte)value;

            public static short ToShort(int value) => value < short.MinValue || value > short.MaxValue ? ThrowTooFewValues<short>(value) : (short)value;

            public static ushort ToUShort(int value) => value < ushort.MinValue || value > ushort.MaxValue ? ThrowTooFewValues<ushort>(value) : (ushort)value;

            public static char ToChar(int value) => value < char.MinValue || value > char.MaxValue ? ThrowTooFewValues<char>(value) : (char)value;

            public static uint ToUInt(int value) => unchecked((uint)value);

            public static long ToLong(int value) => value;

            public static ulong ToULong(int value) => unchecked((uint)value);

            public static float ToFloat(int value)
            {
                var index = IntDomainIndex(value);

                if (index >= UsableFloatCount)
                {
                    return ThrowTooFewValues<float>(value);
                }

                var bits = index < PositiveFiniteFloatCount ? (uint)index : (uint)(0x80000001L + index - PositiveFiniteFloatCount);
                return BitConverter.Int32BitsToSingle(unchecked((int)bits));
            }

            public static double ToDouble(int value) => value;

            public static decimal ToDecimal(int value) => value;

            public static string ToString(int value) => value.ToString(CultureInfo.InvariantCulture);

            public static Guid ToGuid(int value) => new(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            public static DateTime ToDateTime(int value) => new(IntDomainIndex(value), DateTimeKind.Utc);

            public static DateTimeOffset ToDateTimeOffset(int value) => new(ToDateTime(value), TimeSpan.Zero);

            public static TimeSpan ToTimeSpan(int value) => TimeSpan.FromTicks(IntDomainIndex(value));

            public static DateOnly ToDateOnly(int value) => value < DateOnly.MinValue.DayNumber ||
                    value > DateOnly.MaxValue.DayNumber
                    ? ThrowTooFewValues<DateOnly>(value)
                    : DateOnly.FromDayNumber(value);

            public static TimeOnly ToTimeOnly(int value) => TimeOnly.FromTimeSpan(
                    TimeSpan.FromTicks(IntDomainIndex(value)));

            public static byte[] ToBytes(int value) =>
            [
                unchecked((byte)(value >> 24)),
                unchecked((byte)(value >> 16)),
                unchecked((byte)(value >> 8)),
                unchecked((byte)value)
            ];

            private static long IntDomainIndex(int value) => (long)value - int.MinValue;

            private static T ThrowTooFewValues<T>(int value) => throw new UnreachableException(
                    $"{typeof(T).Name} does not have enough usable values to uniquely represent {value}.");

            private const long PositiveFiniteFloatCount = 0x7F800000L;
            private const long NegativeFiniteFloatCountExcludingNegativeZero = 0x7F7FFFFFL;

            private const long UsableFloatCount =
                PositiveFiniteFloatCount + NegativeFiniteFloatCountExcludingNegativeZero;
        }
    }
}
