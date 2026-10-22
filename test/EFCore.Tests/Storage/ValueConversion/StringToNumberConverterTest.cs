// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class StringToNumberConverterTest
{
    private static readonly StringToNumberConverter<ulong> _naturalStringToUlong = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_ulongs()
    {
        var converter = _naturalStringToUlong.ConvertToProviderExpression.Compile();

        Assert.Equal(ulong.MaxValue, converter("18446744073709551615"));
        Assert.Equal((ulong)77, converter("77"));
        Assert.Equal((ulong)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_ulongs_object()
    {
        var converter = _naturalStringToUlong.ConvertToProvider;

        Assert.Equal(ulong.MaxValue, converter("18446744073709551615"));
        Assert.Equal((ulong)77, converter("77"));
        Assert.Equal((ulong)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_ulongs_to_natural_strings()
    {
        var converter = _naturalStringToUlong.ConvertFromProviderExpression.Compile();

        Assert.Equal("18446744073709551615", converter(ulong.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_ulongs_to_natural_strings_object()
    {
        var converter = _naturalStringToUlong.ConvertFromProvider;

        Assert.Equal("18446744073709551615", converter(ulong.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("77", converter((ulong?)77));
        Assert.Equal("0", converter((ulong)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<long> _naturalStringToLong = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_longs()
    {
        var converter = _naturalStringToLong.ConvertToProviderExpression.Compile();

        Assert.Equal(long.MaxValue, converter("9223372036854775807"));
        Assert.Equal(long.MinValue, converter("-9223372036854775808"));
        Assert.Equal(77, converter("77"));
        Assert.Equal(-77, converter("-77"));
        Assert.Equal(0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-9223372036854775809"));
        Assert.Throws<OverflowException>(() => converter("9223372036854775808"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_longs_object()
    {
        var converter = _naturalStringToLong.ConvertToProvider;

        Assert.Equal(long.MaxValue, converter("9223372036854775807"));
        Assert.Equal(long.MinValue, converter("-9223372036854775808"));
        Assert.Equal((long)77, converter("77"));
        Assert.Equal((long)-77, converter("-77"));
        Assert.Equal((long)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-9223372036854775809"));
        Assert.Throws<OverflowException>(() => converter("9223372036854775808"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_longs_to_natural_strings()
    {
        var converter = _naturalStringToLong.ConvertFromProviderExpression.Compile();

        Assert.Equal("9223372036854775807", converter(long.MaxValue));
        Assert.Equal("-9223372036854775808", converter(long.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_longs_to_natural_strings_object()
    {
        var converter = _naturalStringToLong.ConvertFromProvider;

        Assert.Equal("9223372036854775807", converter(long.MaxValue));
        Assert.Equal("-9223372036854775808", converter(long.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("77", converter((long?)77));
        Assert.Equal("0", converter((long)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<uint> _naturalStringToUint = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_uints()
    {
        var converter = _naturalStringToUint.ConvertToProviderExpression.Compile();

        Assert.Equal(uint.MaxValue, converter("4294967295"));
        Assert.Equal((uint)77, converter("77"));
        Assert.Equal((uint)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<OverflowException>(() => converter("4294967296"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_uints_object()
    {
        var converter = _naturalStringToUint.ConvertToProvider;

        Assert.Equal(uint.MaxValue, converter("4294967295"));
        Assert.Equal((uint)77, converter("77"));
        Assert.Equal((uint)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<OverflowException>(() => converter("4294967296"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_uints_to_natural_strings()
    {
        var converter = _naturalStringToUint.ConvertFromProviderExpression.Compile();

        Assert.Equal("4294967295", converter(uint.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_uints_to_natural_strings_object()
    {
        var converter = _naturalStringToUint.ConvertFromProvider;

        Assert.Equal("4294967295", converter(uint.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("77", converter((uint?)77));
        Assert.Equal("0", converter((uint)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<int> _naturalStringToInt = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_ints()
    {
        var converter = _naturalStringToInt.ConvertToProviderExpression.Compile();

        Assert.Equal(int.MaxValue, converter("2147483647"));
        Assert.Equal(int.MinValue, converter("-2147483648"));
        Assert.Equal(77, converter("77"));
        Assert.Equal(-77, converter("-77"));
        Assert.Equal(0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-2147483649"));
        Assert.Throws<OverflowException>(() => converter("2147483648"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_ints_object()
    {
        var converter = _naturalStringToInt.ConvertToProvider;

        Assert.Equal(int.MaxValue, converter("2147483647"));
        Assert.Equal(int.MinValue, converter("-2147483648"));
        Assert.Equal(77, converter("77"));
        Assert.Equal(-77, converter("-77"));

        Assert.Throws<OverflowException>(() => converter("-2147483649"));
        Assert.Throws<OverflowException>(() => converter("2147483648"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_ints_to_natural_strings()
    {
        var converter = _naturalStringToInt.ConvertFromProviderExpression.Compile();

        Assert.Equal("2147483647", converter(int.MaxValue));
        Assert.Equal("-2147483648", converter(int.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_ints_to_natural_strings_object()
    {
        var converter = _naturalStringToInt.ConvertFromProvider;

        Assert.Equal("2147483647", converter(int.MaxValue));
        Assert.Equal("-2147483648", converter(int.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("77", converter((int?)77));
        Assert.Equal("0", converter((int?)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<ushort> _naturalStringToUshort = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_ushorts()
    {
        var converter = _naturalStringToUshort.ConvertToProviderExpression.Compile();

        Assert.Equal(ushort.MaxValue, converter("65535"));
        Assert.Equal((ushort)77, converter("77"));
        Assert.Equal((ushort)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<OverflowException>(() => converter("65536"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_ushorts_object()
    {
        var converter = _naturalStringToUshort.ConvertToProvider;

        Assert.Equal(ushort.MaxValue, converter("65535"));
        Assert.Equal((ushort)77, converter("77"));
        Assert.Equal((ushort)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<OverflowException>(() => converter("65536"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_ushorts_to_natural_strings()
    {
        var converter = _naturalStringToUshort.ConvertFromProviderExpression.Compile();

        Assert.Equal("65535", converter(ushort.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_ushorts_to_natural_strings_object()
    {
        var converter = _naturalStringToUshort.ConvertFromProvider;

        Assert.Equal("65535", converter(ushort.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("77", converter((ushort?)77));
        Assert.Equal("0", converter((ushort)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<short> _naturalStringToShort = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_shorts()
    {
        var converter = _naturalStringToShort.ConvertToProviderExpression.Compile();

        Assert.Equal(short.MaxValue, converter("32767"));
        Assert.Equal(short.MinValue, converter("-32768"));
        Assert.Equal(77, converter("77"));
        Assert.Equal(-77, converter("-77"));
        Assert.Equal(0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-32769"));
        Assert.Throws<OverflowException>(() => converter("32768"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_shorts_object()
    {
        var converter = _naturalStringToShort.ConvertToProvider;

        Assert.Equal(short.MaxValue, converter("32767"));
        Assert.Equal(short.MinValue, converter("-32768"));
        Assert.Equal((short)77, converter("77"));
        Assert.Equal((short)-77, converter("-77"));
        Assert.Equal((short)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-32769"));
        Assert.Throws<OverflowException>(() => converter("32768"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_shorts_to_natural_strings()
    {
        var converter = _naturalStringToShort.ConvertFromProviderExpression.Compile();

        Assert.Equal("32767", converter(short.MaxValue));
        Assert.Equal("-32768", converter(short.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_shorts_to_natural_strings_object()
    {
        var converter = _naturalStringToShort.ConvertFromProvider;

        Assert.Equal("32767", converter(short.MaxValue));
        Assert.Equal("-32768", converter(short.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("77", converter((short?)77));
        Assert.Equal("0", converter((short)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<byte> _naturalStringToByte = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_bytes()
    {
        var converter = _naturalStringToByte.ConvertToProviderExpression.Compile();

        Assert.Equal(byte.MaxValue, converter("255"));
        Assert.Equal((byte)77, converter("77"));
        Assert.Equal((byte)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<OverflowException>(() => converter("256"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_bytes_object()
    {
        var converter = _naturalStringToByte.ConvertToProvider;

        Assert.Equal(byte.MaxValue, converter("255"));
        Assert.Equal((byte)77, converter("77"));
        Assert.Equal((byte)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-1"));
        Assert.Throws<OverflowException>(() => converter("256"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_natural_strings()
    {
        var converter = _naturalStringToByte.ConvertFromProviderExpression.Compile();

        Assert.Equal("255", converter(byte.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_bytes_to_natural_strings_object()
    {
        var converter = _naturalStringToByte.ConvertFromProvider;

        Assert.Equal("255", converter(byte.MaxValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("77", converter((byte?)77));
        Assert.Equal("0", converter((byte)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<sbyte> _naturalStringToSbyte = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_sbytes()
    {
        var converter = _naturalStringToSbyte.ConvertToProviderExpression.Compile();

        Assert.Equal(sbyte.MaxValue, converter("127"));
        Assert.Equal(sbyte.MinValue, converter("-128"));
        Assert.Equal(77, converter("77"));
        Assert.Equal(-77, converter("-77"));
        Assert.Equal(0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-129"));
        Assert.Throws<OverflowException>(() => converter("128"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_sbytes_object()
    {
        var converter = _naturalStringToSbyte.ConvertToProvider;

        Assert.Equal(sbyte.MaxValue, converter("127"));
        Assert.Equal(sbyte.MinValue, converter("-128"));
        Assert.Equal((sbyte)77, converter("77"));
        Assert.Equal((sbyte)-77, converter("-77"));
        Assert.Equal((sbyte)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-129"));
        Assert.Throws<OverflowException>(() => converter("128"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_sbytes_to_natural_strings()
    {
        var converter = _naturalStringToSbyte.ConvertFromProviderExpression.Compile();

        Assert.Equal("127", converter(sbyte.MaxValue));
        Assert.Equal("-128", converter(sbyte.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("0", converter(0));
    }

    [ConditionalFact]
    public void Can_convert_sbytes_to_natural_strings_object()
    {
        var converter = _naturalStringToSbyte.ConvertFromProvider;

        Assert.Equal("127", converter(sbyte.MaxValue));
        Assert.Equal("-128", converter(sbyte.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("77", converter((sbyte?)77));
        Assert.Equal("0", converter((sbyte)0));
        Assert.Null(converter(null));
    }

    private static readonly StringToNumberConverter<decimal> _naturalStringToDecimal = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_decimals()
    {
        var converter = _naturalStringToDecimal.ConvertToProviderExpression.Compile();

        Assert.Equal(decimal.MaxValue, converter("79228162514264337593543950335"));
        Assert.Equal(decimal.MinValue, converter("-79228162514264337593543950335"));
        Assert.Equal((decimal)-792264.3375935, converter("-792264.3375935"));
        Assert.Equal((decimal)0.000000001, converter("0.000000001"));
        Assert.Equal((decimal)0.00000000000000000001, converter("0.00000000000000000001"));
        Assert.Equal((decimal)-0.00000000000000000001, converter("-0.00000000000000000001"));

        Assert.Throws<OverflowException>(() => converter("-79228162514264337593543950336"));
        Assert.Throws<OverflowException>(() => converter("79228162514264337593543950336"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_decimals_to_natural_strings()
    {
        var converter = _naturalStringToDecimal.ConvertFromProviderExpression.Compile();

        Assert.Equal("79228162514264337593543950335", converter(decimal.MaxValue));
        Assert.Equal("-79228162514264337593543950335", converter(decimal.MinValue));
        Assert.Equal("-792264.3375935", converter((decimal)-792264.3375935));
        Assert.Equal("0.000000001", converter((decimal)0.000000001));
        Assert.Equal("0.00000000000000000001", converter((decimal)0.00000000000000000001));
        Assert.Equal("-0.00000000000000000001", converter((decimal)-0.00000000000000000001));
    }

    private static readonly StringToNumberConverter<double> _naturalStringToDouble = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_doubles()
    {
        var converter = _naturalStringToDouble.ConvertToProviderExpression.Compile();

        Assert.Equal(double.MaxValue, converter("1.7976931348623157E+308"));
        Assert.Equal(double.MinValue, converter("-1.7976931348623157E+308"));
        Assert.Equal(-792264.3375935, converter("-792264.3375935"));
        Assert.Equal(0.000000001, converter("1E-09"));
        Assert.Equal(0.00000000000000000001, converter("1E-20"));
        Assert.Equal(-0.00000000000000000001, converter("-1E-20"));

        Assert.Equal(double.PositiveInfinity, converter("1.7976931348623157E+309"));
        Assert.Equal(double.NegativeInfinity, converter("-1.7976931348623157E+309"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_doubles_to_natural_strings()
    {
        var converter = _naturalStringToDouble.ConvertFromProviderExpression.Compile();

        Assert.Equal("1.7976931348623157E+308", converter(double.MaxValue));
        Assert.Equal("-1.7976931348623157E+308", converter(double.MinValue));
        Assert.Equal("-792264.3375935", converter(-792264.3375935));
        Assert.Equal("1E-09", converter(0.000000001));
        Assert.Equal("1E-20", converter(0.00000000000000000001));
        Assert.Equal("-1E-20", converter(-0.00000000000000000001));
    }

    private static readonly StringToNumberConverter<float> _naturalStringToFloat = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_floats()
    {
        var converter = _naturalStringToFloat.ConvertToProviderExpression.Compile();

        Assert.Equal(float.MaxValue, converter("3.40282347E+38"));
        Assert.Equal(float.MinValue, converter("-3.40282347E+38"));
        Assert.Equal((float)-79.3335, converter("-79.3335"));
        Assert.Equal((float)0.000000001, converter("1E-09"));
        Assert.Equal((float)0.00000000000000000001, converter("1E-20"));
        Assert.Equal((float)-0.00000000000000000001, converter("-1E-20"));

        Assert.Equal(float.PositiveInfinity, converter("3.40282347E+39"));
        Assert.Equal(float.NegativeInfinity, converter("-3.40282347E+39"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Throws<ArgumentNullException>(() => converter(null));
    }

    [ConditionalFact]
    public void Can_convert_floats_to_natural_strings()
    {
        var converter = _naturalStringToFloat.ConvertFromProviderExpression.Compile();

        Assert.Equal("3.4028235E+38", converter(float.MaxValue));
        Assert.Equal("-3.4028235E+38", converter(float.MinValue));
        Assert.Equal("-79.3335", converter((float)-79.3335));
        Assert.Equal("1E-09", converter((float)0.000000001));
        Assert.Equal("1E-20", converter((float)0.00000000000000000001));
        Assert.Equal("-1E-20", converter((float)-0.00000000000000000001));
    }

    private static readonly StringToNumberConverter<sbyte?> _naturalStringToNullableSbyte = new();

    [ConditionalFact]
    public void Can_convert_natural_strings_to_nullable_sbytes()
    {
        var converter = _naturalStringToNullableSbyte.ConvertToProviderExpression.Compile();

        Assert.Equal(sbyte.MaxValue, converter("127"));
        Assert.Equal(sbyte.MinValue, converter("-128"));
        Assert.Equal((sbyte?)77, converter("77"));
        Assert.Equal((sbyte?)-77, converter("-77"));
        Assert.Equal((sbyte?)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-129"));
        Assert.Throws<OverflowException>(() => converter("128"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_natural_strings_to_nullable_sbytes_object()
    {
        var converter = _naturalStringToNullableSbyte.ConvertToProvider;

        Assert.Equal(sbyte.MaxValue, converter("127"));
        Assert.Equal(sbyte.MinValue, converter("-128"));
        Assert.Equal((sbyte)77, converter("77"));
        Assert.Equal((sbyte)-77, converter("-77"));
        Assert.Equal((sbyte?)0, converter("0"));

        Assert.Throws<OverflowException>(() => converter("-129"));
        Assert.Throws<OverflowException>(() => converter("128"));
        Assert.Throws<FormatException>(() => converter("Not a number"));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_nullable_sbytes_to_natural_strings()
    {
        var converter = _naturalStringToNullableSbyte.ConvertFromProviderExpression.Compile();

        Assert.Equal("127", converter(sbyte.MaxValue));
        Assert.Equal("-128", converter(sbyte.MinValue));
        Assert.Equal("77", converter(77));
        Assert.Equal("-77", converter(-77));
        Assert.Equal("0", converter(0));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void Can_convert_nullable_sbytes_to_natural_strings_object()
    {
        var converter = _naturalStringToNullableSbyte.ConvertFromProvider;

        Assert.Equal("127", converter((sbyte?)sbyte.MaxValue));
        Assert.Equal("-128", converter((sbyte?)sbyte.MinValue));
        Assert.Equal("77", converter((sbyte?)77));
        Assert.Equal("-77", converter((sbyte?)-77));
        Assert.Equal("0", converter((sbyte?)0));
        Assert.Null(converter(null));
    }

    [ConditionalFact]
    public void String_to_integer_converter_throws_for_bad_type()
        => Assert.Equal(
            CoreStrings.ConverterBadType(
                typeof(StringNumberConverter<string, Guid, Guid>).ShortDisplayName(),
                "Guid",
                "'int', 'long', 'short', 'byte', 'uint', 'ulong', 'ushort', 'sbyte', 'decimal', 'float', 'double'"),
            Assert.Throws<InvalidOperationException>(
                () => new StringToNumberConverter<Guid>()).Message);
}
