// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class NumberToBytesConverterTest
{
    private static readonly NumberToBytesConverter<byte> _byteToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_byte_to_bytes()
    {
        var converter = _byteToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 7 }, converter(7));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_byte()
    {
        var converter = _byteToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(7, converter([7]));
        Assert.Equal(0, converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_byte_to_bytes()
        => ValueConverterTest.OrderingTest(_byteToBytesConverter, (byte)0, (byte)7, (byte)77, (byte)255);

    private static readonly NumberToBytesConverter<byte?> _nullableByteToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_nullable_byte_to_bytes()
    {
        var converter = _nullableByteToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 7 }, converter(7));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_nullable_byte()
    {
        var converter = _nullableByteToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal((byte?)7, converter([7]));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_nullable_byte_to_bytes()
        => ValueConverterTest.OrderingTest(_nullableByteToBytesConverter, (byte?)0, (byte?)7, (byte?)77, (byte?)255);

    private static readonly NumberToBytesConverter<short> _shortToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_short_to_bytes()
    {
        var converter = _shortToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 30, 97 }, converter(7777));
        Assert.Equal(new byte[] { 225, 159 }, converter(-7777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_short()
    {
        var converter = _shortToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(7777, converter([30, 97]));
        Assert.Equal(-7777, converter([225, 159]));
        Assert.Equal(0, converter(null));
    }

    private static readonly NumberToBytesConverter<int> _intToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_int_to_bytes()
    {
        var converter = _intToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 4, 162, 203, 113 }, converter(77777777));
        Assert.Equal(new byte[] { 251, 93, 52, 143 }, converter(-77777777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_int()
    {
        var converter = _intToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(77777777, converter([4, 162, 203, 113]));
        Assert.Equal(-77777777, converter([251, 93, 52, 143]));
        Assert.Equal(0, converter(null));
    }

    private static readonly NumberToBytesConverter<int?> _nullableIntToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_nullable_int_to_bytes()
    {
        var converter = _nullableIntToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 4, 162, 203, 113 }, converter(77777777));
        Assert.Equal(new byte[] { 251, 93, 52, 143 }, converter(-77777777));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_nullable_int()
    {
        var converter = _nullableIntToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(77777777, converter([4, 162, 203, 113]));
        Assert.Equal(-77777777, converter([251, 93, 52, 143]));
        Assert.Null(converter(null));
    }

    private static readonly NumberToBytesConverter<long> _longToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_long_to_bytes()
    {
        var converter = _longToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 0, 0, 0, 181, 23, 43, 12, 113 }, converter(777777777777));
        Assert.Equal(new byte[] { 255, 255, 255, 74, 232, 212, 243, 143 }, converter(-777777777777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_long()
    {
        var converter = _longToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(777777777777, converter([0, 0, 0, 181, 23, 43, 12, 113]));
        Assert.Equal(-777777777777, converter([255, 255, 255, 74, 232, 212, 243, 143]));
        Assert.Equal(0, converter(null));
    }

    private static readonly NumberToBytesConverter<sbyte> _sbyteToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_sbyte_to_bytes()
    {
        var converter = _sbyteToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 7 }, converter(7));
        Assert.Equal(new byte[] { 249 }, converter(-7));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_sbyte()
    {
        var converter = _sbyteToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(7, converter([7]));
        Assert.Equal(-7, converter([249]));
        Assert.Equal(0, converter(null));
    }

    private static readonly NumberToBytesConverter<sbyte?> _nullableSbyteToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_nullable_sbyte_to_bytes()
    {
        var converter = _nullableSbyteToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 7 }, converter(7));
        Assert.Equal(new byte[] { 249 }, converter(-7));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_nullable_sbyte()
    {
        var converter = _nullableSbyteToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal((sbyte?)7, converter([7]));
        Assert.Equal((sbyte?)-7, converter([249]));
        Assert.Null(converter(null));
    }

    private static readonly NumberToBytesConverter<ushort> _ushortToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_ushort_to_bytes()
    {
        var converter = _ushortToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 30, 97 }, converter(7777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_ushort()
    {
        var converter = _ushortToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(7777, converter([30, 97]));
        Assert.Equal(0, converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_ushort_to_bytes()
        => ValueConverterTest.OrderingTest(_ushortToBytesConverter, (ushort)0, (ushort)7, (ushort)777, (ushort)7777);

    private static readonly NumberToBytesConverter<uint> _uintToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_uint_to_bytes()
    {
        var converter = _uintToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 4, 162, 203, 113 }, converter(77777777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_uint()
    {
        var converter = _uintToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal((uint)77777777, converter([4, 162, 203, 113]));
        Assert.Equal((uint)0, converter(null));
    }

    private static readonly NumberToBytesConverter<uint?> _nullableUintToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_nullable_uint_to_bytes()
    {
        var converter = _nullableUintToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 4, 162, 203, 113 }, converter(77777777));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_nullable_uint()
    {
        var converter = _nullableUintToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal((uint?)77777777, converter([4, 162, 203, 113]));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_uint_to_bytes()
        => ValueConverterTest.OrderingTest(_uintToBytesConverter, (uint)0, (uint)7, (uint)777777, (uint)77777777);

    private static readonly NumberToBytesConverter<ulong> _ulongToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_ulong_to_bytes()
    {
        var converter = _ulongToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 0, 0, 0, 181, 23, 43, 12, 113 }, converter(777777777777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_ulong()
    {
        var converter = _ulongToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal((ulong)777777777777, converter([0, 0, 0, 181, 23, 43, 12, 113]));
        Assert.Equal((ulong)0, converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_ulong_to_bytes()
        => ValueConverterTest.OrderingTest(_ulongToBytesConverter, (ulong)0, (ulong)777, (ulong)77777777, (ulong)7777777777777777);

    private static readonly NumberToBytesConverter<char> _charToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_char_to_bytes()
    {
        var converter = _charToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 0, 65 }, converter('A'));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_char()
    {
        var converter = _charToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal('A', converter([0, 65]));
        Assert.Equal(0, converter(null));
    }

    [ConditionalFact]
    public void Ordering_preserved_for_char_to_bytes()
        => ValueConverterTest.OrderingTest(_charToBytesConverter, '\u0000', 'A', 'Z', '\u7777');

    private static readonly NumberToBytesConverter<decimal> _decimalToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_decimal_to_bytes()
    {
        var converter = _decimalToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            converter(decimal.MaxValue));
        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            converter(long.MaxValue));
        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF },
            converter(int.MaxValue));
        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF },
            converter(short.MaxValue));
        Assert.Equal(
            new byte[] { 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            converter(decimal.MinValue));
        Assert.Equal(
            new byte[] { 0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
            converter((decimal)0.000000001));
        Assert.Equal(
            new byte[] { 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
            converter((decimal)0.00000000000000000001));
        Assert.Equal(
            new byte[] { 0x80, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
            converter((decimal)-0.00000000000000000001));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_decimal()
    {
        var converter = _decimalToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            decimal.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            long.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            int.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            short.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF]));
        Assert.Equal(
            decimal.MinValue,
            converter([0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            (decimal)0.000000001,
            converter([0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]));
        Assert.Equal(
            (decimal)0.00000000000000000001,
            converter([0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]));
        Assert.Equal(
            (decimal)-0.00000000000000000001,
            converter([0x80, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]));
    }

    private static readonly NumberToBytesConverter<decimal?> _nullableDecimalToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_nullable_decimal_to_bytes()
    {
        var converter = _nullableDecimalToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            converter(decimal.MaxValue));
        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            converter(long.MaxValue));
        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF },
            converter(int.MaxValue));
        Assert.Equal(
            new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF },
            converter(short.MaxValue));
        Assert.Equal(
            new byte[] { 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF },
            converter(decimal.MinValue));
        Assert.Equal(
            new byte[] { 0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
            converter((decimal?)0.000000001));
        Assert.Equal(
            new byte[] { 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
            converter((decimal?)0.00000000000000000001));
        Assert.Equal(
            new byte[] { 0x80, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 },
            converter((decimal?)-0.00000000000000000001));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_nullable_decimal()
    {
        var converter = _nullableDecimalToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(
            decimal.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            long.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            int.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            short.MaxValue,
            converter([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF]));
        Assert.Equal(
            decimal.MinValue,
            converter([0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]));
        Assert.Equal(
            (decimal?)0.000000001,
            converter([0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]));
        Assert.Equal(
            (decimal?)0.00000000000000000001,
            converter([0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]));
        Assert.Equal(
            (decimal?)-0.00000000000000000001,
            converter([0x80, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01]));
        Assert.Null(converter(null));
    }

    private static readonly NumberToBytesConverter<float> _floatToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_float_to_bytes()
    {
        var converter = _floatToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 68, 66, 113, 72 }, converter((float)777.77));
        Assert.Equal(new byte[] { 196, 66, 113, 72 }, converter((float)-777.77));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_float()
    {
        var converter = _floatToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal((float)777.77, converter([68, 66, 113, 72]));
        Assert.Equal((float)-777.77, converter([196, 66, 113, 72]));
        Assert.Equal(0, converter(null));
    }

    private static readonly NumberToBytesConverter<double> _doubleToBytesConverter = new();

    [ConditionalFact]
    public void Can_convert_double_to_bytes()
    {
        var converter = _doubleToBytesConverter.ConvertToProviderExpression.Compile();

        Assert.Equal(new byte[] { 65, 93, 171, 124, 113, 198, 251, 210 }, converter(7777777.77777));
        Assert.Equal(new byte[] { 193, 93, 171, 124, 113, 198, 251, 210 }, converter(-7777777.77777));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_double()
    {
        var converter = _doubleToBytesConverter.ConvertFromProviderExpression.Compile();

        Assert.Equal(7777777.77777, converter([65, 93, 171, 124, 113, 198, 251, 210]));
        Assert.Equal(-7777777.77777, converter([193, 93, 171, 124, 113, 198, 251, 210]));
        Assert.Equal(0, converter(null));
    }

    [ConditionalFact]
    public void Enum_to_integer_converter_throws_for_bad_types()
        => Assert.Equal(
            CoreStrings.ConverterBadType(
                "NumberToBytesConverter<Guid>",
                "Guid",
                "'double', 'float', 'decimal', 'char', 'int', 'long', 'short', 'byte', 'uint', 'ulong', 'ushort', 'sbyte'"),
            Assert.Throws<InvalidOperationException>(
                () => new NumberToBytesConverter<Guid>()).Message);
}
