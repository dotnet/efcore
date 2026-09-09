// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class ValueConverterSelectorTest
{
    private readonly IValueConverterSelector _selector
        = new ValueConverterSelector(new ValueConverterSelectorDependencies());

    [ConditionalFact]
    public void Can_get_converters_for_int_enums()
        => AssertConverters(
            _selector.Select(typeof(Queen)).ToList(),
            (typeof(EnumToNumberConverter<Queen, int>), default),
            (typeof(EnumToNumberConverter<Queen, long>), default),
            (typeof(EnumToNumberConverter<Queen, decimal>), default),
            (typeof(EnumToStringConverter<Queen>), default),
            (typeof(CompositeValueConverter<Queen, int, byte[]>), new ConverterMappingHints(size: 4)),
            (typeof(EnumToNumberConverter<Queen, short>), default),
            (typeof(EnumToNumberConverter<Queen, byte>), default),
            (typeof(EnumToNumberConverter<Queen, ulong>), default),
            (typeof(EnumToNumberConverter<Queen, uint>), default),
            (typeof(EnumToNumberConverter<Queen, ushort>), default),
            (typeof(EnumToNumberConverter<Queen, sbyte>), default),
            (typeof(EnumToNumberConverter<Queen, double>), default),
            (typeof(EnumToNumberConverter<Queen, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_ulong_enums()
        => AssertConverters(
            _selector.Select(typeof(Gnr)).ToList(),
            (typeof(EnumToNumberConverter<Gnr, ulong>), default),
            (typeof(EnumToNumberConverter<Gnr, decimal>), new ConverterMappingHints(precision: 20, scale: 0)),
            (typeof(EnumToStringConverter<Gnr>), default),
            (typeof(CompositeValueConverter<Gnr, ulong, byte[]>), new ConverterMappingHints(size: 8)),
            (typeof(EnumToNumberConverter<Gnr, int>), default),
            (typeof(EnumToNumberConverter<Gnr, long>), default),
            (typeof(EnumToNumberConverter<Gnr, short>), default),
            (typeof(EnumToNumberConverter<Gnr, byte>), default),
            (typeof(EnumToNumberConverter<Gnr, uint>), default),
            (typeof(EnumToNumberConverter<Gnr, ushort>), default),
            (typeof(EnumToNumberConverter<Gnr, sbyte>), default),
            (typeof(EnumToNumberConverter<Gnr, double>), default),
            (typeof(EnumToNumberConverter<Gnr, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_long_enums()
        => AssertConverters(
            _selector.Select(typeof(Velvets)).ToList(),
            (typeof(EnumToNumberConverter<Velvets, long>), default),
            (typeof(EnumToNumberConverter<Velvets, decimal>), new ConverterMappingHints(precision: 20, scale: 0)),
            (typeof(EnumToStringConverter<Velvets>), default),
            (typeof(CompositeValueConverter<Velvets, long, byte[]>), new ConverterMappingHints(size: 8)),
            (typeof(EnumToNumberConverter<Velvets, int>), default),
            (typeof(EnumToNumberConverter<Velvets, short>), default),
            (typeof(EnumToNumberConverter<Velvets, byte>), default),
            (typeof(EnumToNumberConverter<Velvets, ulong>), default),
            (typeof(EnumToNumberConverter<Velvets, uint>), default),
            (typeof(EnumToNumberConverter<Velvets, ushort>), default),
            (typeof(EnumToNumberConverter<Velvets, sbyte>), default),
            (typeof(EnumToNumberConverter<Velvets, double>), default),
            (typeof(EnumToNumberConverter<Velvets, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_byte_enums()
        => AssertConverters(
            _selector.Select(typeof(Nwa)).ToList(),
            (typeof(EnumToNumberConverter<Nwa, byte>), default),
            (typeof(EnumToNumberConverter<Nwa, short>), default),
            (typeof(EnumToNumberConverter<Nwa, ushort>), default),
            (typeof(EnumToNumberConverter<Nwa, int>), default),
            (typeof(EnumToNumberConverter<Nwa, uint>), default),
            (typeof(EnumToNumberConverter<Nwa, long>), default),
            (typeof(EnumToNumberConverter<Nwa, ulong>), default),
            (typeof(EnumToNumberConverter<Nwa, decimal>), default),
            (typeof(EnumToStringConverter<Nwa>), default),
            (typeof(CompositeValueConverter<Nwa, byte, byte[]>), new ConverterMappingHints(size: 1)),
            (typeof(EnumToNumberConverter<Nwa, sbyte>), default),
            (typeof(EnumToNumberConverter<Nwa, double>), default),
            (typeof(EnumToNumberConverter<Nwa, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_enum_to_string()
        => AssertConverters(
            _selector.Select(typeof(Queen), typeof(string)).ToList(),
            (typeof(EnumToStringConverter<Queen>), default));

    [ConditionalFact]
    public void Can_get_converters_for_enum_to_underlying_enum_type()
        => AssertConverters(
            _selector.Select(typeof(Queen), typeof(int)).ToList(),
            (typeof(EnumToNumberConverter<Queen, int>), default));

    [ConditionalFact]
    public void Can_get_converters_for_enum_to_other_integer_type()
        => AssertConverters(
            _selector.Select(typeof(Queen), typeof(sbyte)).ToList(),
            (typeof(EnumToNumberConverter<Queen, sbyte>), default));

    [ConditionalFact]
    public void Can_get_converters_for_enum_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(Queen), typeof(byte[])).ToList(),
            (typeof(CompositeValueConverter<Queen, int, byte[]>), new ConverterMappingHints(size: 4)));

    [ConditionalFact]
    public void Can_get_converters_for_int()
        => AssertConverters(
            _selector.Select(typeof(int)).ToList(),
            (typeof(CastingConverter<int, long>), default),
            (typeof(CastingConverter<int, decimal>), default),
            (typeof(NumberToStringConverter<int>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<int>), new ConverterMappingHints(size: 4)),
            (typeof(CastingConverter<int, short>), default),
            (typeof(CastingConverter<int, byte>), default),
            (typeof(CastingConverter<int, ulong>), default),
            (typeof(CastingConverter<int, uint>), default),
            (typeof(CastingConverter<int, ushort>), default),
            (typeof(CastingConverter<int, sbyte>), default),
            (typeof(CastingConverter<int, double>), default),
            (typeof(CastingConverter<int, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_uint()
        => AssertConverters(
            _selector.Select(typeof(uint)).ToList(),
            (typeof(CastingConverter<uint, long>), default),
            (typeof(CastingConverter<uint, ulong>), default),
            (typeof(CastingConverter<uint, decimal>), default),
            (typeof(NumberToStringConverter<uint>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<uint>), new ConverterMappingHints(size: 4)),
            (typeof(CastingConverter<uint, int>), default),
            (typeof(CastingConverter<uint, short>), default),
            (typeof(CastingConverter<uint, byte>), default),
            (typeof(CastingConverter<uint, ushort>), default),
            (typeof(CastingConverter<uint, sbyte>), default),
            (typeof(CastingConverter<uint, double>), default),
            (typeof(CastingConverter<uint, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_sbyte()
        => AssertConverters(
            _selector.Select(typeof(sbyte)).ToList(),
            (typeof(CastingConverter<sbyte, short>), default),
            (typeof(CastingConverter<sbyte, int>), default),
            (typeof(CastingConverter<sbyte, long>), default),
            (typeof(CastingConverter<sbyte, decimal>), default),
            (typeof(NumberToStringConverter<sbyte>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<sbyte>), new ConverterMappingHints(size: 1)),
            (typeof(CastingConverter<sbyte, byte>), default),
            (typeof(CastingConverter<sbyte, ulong>), default),
            (typeof(CastingConverter<sbyte, uint>), default),
            (typeof(CastingConverter<sbyte, ushort>), default),
            (typeof(CastingConverter<sbyte, double>), default),
            (typeof(CastingConverter<sbyte, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_byte()
        => AssertConverters(
            _selector.Select(typeof(byte)).ToList(),
            (typeof(CastingConverter<byte, short>), default),
            (typeof(CastingConverter<byte, ushort>), default),
            (typeof(CastingConverter<byte, int>), default),
            (typeof(CastingConverter<byte, uint>), default),
            (typeof(CastingConverter<byte, long>), default),
            (typeof(CastingConverter<byte, ulong>), default),
            (typeof(CastingConverter<byte, decimal>), default),
            (typeof(NumberToStringConverter<byte>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<byte>), new ConverterMappingHints(size: 1)),
            (typeof(CastingConverter<byte, sbyte>), default),
            (typeof(CastingConverter<byte, double>), default),
            (typeof(CastingConverter<byte, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_double()
        => AssertConverters(
            _selector.Select(typeof(double)).ToList(),
            (typeof(CastingConverter<double, decimal>), new ConverterMappingHints(precision: 38, scale: 17)),
            (typeof(NumberToStringConverter<double>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<double>), new ConverterMappingHints(size: 8)),
            (typeof(CastingConverter<double, int>), default),
            (typeof(CastingConverter<double, long>), default),
            (typeof(CastingConverter<double, short>), default),
            (typeof(CastingConverter<double, byte>), default),
            (typeof(CastingConverter<double, ulong>), default),
            (typeof(CastingConverter<double, uint>), default),
            (typeof(CastingConverter<double, ushort>), default),
            (typeof(CastingConverter<double, sbyte>), default),
            (typeof(CastingConverter<double, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_float()
        => AssertConverters(
            _selector.Select(typeof(float)).ToList(),
            (typeof(CastingConverter<float, double>), default),
            (typeof(CastingConverter<float, decimal>), new ConverterMappingHints(precision: 38, scale: 17)),
            (typeof(NumberToStringConverter<float>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<float>), new ConverterMappingHints(size: 4)),
            (typeof(CastingConverter<float, int>), default),
            (typeof(CastingConverter<float, long>), default),
            (typeof(CastingConverter<float, short>), default),
            (typeof(CastingConverter<float, byte>), default),
            (typeof(CastingConverter<float, ulong>), default),
            (typeof(CastingConverter<float, uint>), default),
            (typeof(CastingConverter<float, ushort>), default),
            (typeof(CastingConverter<float, sbyte>), default));

    [ConditionalFact]
    public void Can_get_converters_for_decimal()
        => AssertConverters(
            _selector.Select(typeof(decimal)).ToList(),
            (typeof(NumberToStringConverter<decimal>), new ConverterMappingHints(size: 64)),
            (typeof(NumberToBytesConverter<decimal>), new ConverterMappingHints(size: 16)),
            (typeof(CastingConverter<decimal, int>), default),
            (typeof(CastingConverter<decimal, long>), default),
            (typeof(CastingConverter<decimal, short>), default),
            (typeof(CastingConverter<decimal, byte>), default),
            (typeof(CastingConverter<decimal, ulong>), default),
            (typeof(CastingConverter<decimal, uint>), default),
            (typeof(CastingConverter<decimal, ushort>), default),
            (typeof(CastingConverter<decimal, sbyte>), default),
            (typeof(CastingConverter<decimal, double>), default),
            (typeof(CastingConverter<decimal, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_double_to_float()
        => AssertConverters(
            _selector.Select(typeof(double), typeof(float)).ToList(),
            (typeof(CastingConverter<double, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_float_to_double()
        => AssertConverters(
            _selector.Select(typeof(float), typeof(double)).ToList(),
            (typeof(CastingConverter<float, double>), default));

    [ConditionalFact]
    public void Can_get_explicit_converters_for_numeric_types()
    {
        var types = new[]
        {
            typeof(int),
            typeof(short),
            typeof(long),
            typeof(sbyte),
            typeof(uint),
            typeof(ushort),
            typeof(ulong),
            typeof(byte),
            typeof(decimal),
            typeof(double),
            typeof(float)
        };

        foreach (var fromType in types)
        {
            foreach (var toType in types)
            {
                var converterInfos = _selector.Select(fromType, toType).ToList();

                if (fromType == toType)
                {
                    Assert.Empty(converterInfos);
                }
                else
                {
                    Assert.Equal(
                        typeof(CastingConverter<,>).MakeGenericType(fromType, toType),
                        converterInfos.Single().Create().GetType());
                }
            }
        }
    }

    [ConditionalFact]
    public void Can_get_converters_for_char()
        => AssertConverters(
            _selector.Select(typeof(char)).ToList(),
            (typeof(CharToStringConverter), new ConverterMappingHints(size: 1)),
            (typeof(CastingConverter<char, int>), default),
            (typeof(CastingConverter<char, ushort>), default),
            (typeof(CastingConverter<char, uint>), default),
            (typeof(CastingConverter<char, long>), default),
            (typeof(CastingConverter<char, ulong>), default),
            (typeof(CastingConverter<char, decimal>), default),
            (typeof(NumberToBytesConverter<char>), new ConverterMappingHints(size: 2)),
            (typeof(CastingConverter<char, short>), default),
            (typeof(CastingConverter<char, byte>), default),
            (typeof(CastingConverter<char, sbyte>), default),
            (typeof(CastingConverter<char, double>), default),
            (typeof(CastingConverter<char, float>), default));

    [ConditionalFact]
    public void Can_get_converters_for_char_to_string()
        => AssertConverters(
            _selector.Select(typeof(char), typeof(string)).ToList(),
            (typeof(CharToStringConverter), new ConverterMappingHints(size: 1)));

    [ConditionalFact]
    public void Can_get_converters_for_char_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(char), typeof(byte[])).ToList(),
            (typeof(NumberToBytesConverter<char>), new ConverterMappingHints(size: 2)));

    [ConditionalFact]
    public void Can_get_converters_for_char_to_specific_numeric()
        => AssertConverters(
            _selector.Select(typeof(char), typeof(ushort)).ToList(),
            (typeof(CastingConverter<char, ushort>), default));

    [ConditionalFact]
    public void Can_get_converters_for_bool()
        => AssertConverters(
            _selector.Select(typeof(bool)).ToList(),
            (typeof(BoolToZeroOneConverter<int>), default),
            (typeof(BoolToZeroOneConverter<long>), default),
            (typeof(BoolToZeroOneConverter<short>), default),
            (typeof(BoolToZeroOneConverter<byte>), default),
            (typeof(BoolToZeroOneConverter<ulong>), default),
            (typeof(BoolToZeroOneConverter<uint>), default),
            (typeof(BoolToZeroOneConverter<ushort>), default),
            (typeof(BoolToZeroOneConverter<sbyte>), default),
            (typeof(BoolToZeroOneConverter<decimal>), default),
            (typeof(BoolToZeroOneConverter<double>), default),
            (typeof(BoolToZeroOneConverter<float>), default),
            (typeof(BoolToStringConverter), new ConverterMappingHints(size: 1)),
            (typeof(CompositeValueConverter<bool, byte, byte[]>), new ConverterMappingHints(size: 1)));

    [ConditionalFact]
    public void Can_get_converters_for_GUID()
        => AssertConverters(
            _selector.Select(typeof(Guid)).ToList(),
            (typeof(GuidToStringConverter), new ConverterMappingHints(size: 36)),
            (typeof(GuidToBytesConverter), new ConverterMappingHints(size: 16)));

    [ConditionalFact]
    public void Can_get_converters_for_GUID_to_string()
        => AssertConverters(
            _selector.Select(typeof(Guid), typeof(string)).ToList(),
            (typeof(GuidToStringConverter), new ConverterMappingHints(size: 36)));

    [ConditionalFact]
    public void Can_get_converters_for_GUID_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(Guid), typeof(byte[])).ToList(),
            (typeof(GuidToBytesConverter), new ConverterMappingHints(size: 16)));

    [ConditionalFact]
    public void Can_get_converters_for_strings()
        => AssertConverters(
            _selector.Select(typeof(string)).ToList(),
            (typeof(StringToBytesConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_int()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(int)).ToList(),
            (typeof(StringToNumberConverter<int>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_long()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(long)).ToList(),
            (typeof(StringToNumberConverter<long>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_short()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(short)).ToList(),
            (typeof(StringToNumberConverter<short>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_byte()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(byte)).ToList(),
            (typeof(StringToNumberConverter<byte>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_ulong()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(ulong)).ToList(),
            (typeof(StringToNumberConverter<ulong>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_uint()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(uint)).ToList(),
            (typeof(StringToNumberConverter<uint>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_ushort()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(ushort)).ToList(),
            (typeof(StringToNumberConverter<ushort>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_sbyte()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(sbyte)).ToList(),
            (typeof(StringToNumberConverter<sbyte>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_decimal()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(decimal)).ToList(),
            (typeof(StringToNumberConverter<decimal>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_double()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(double)).ToList(),
            (typeof(StringToNumberConverter<double>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_float()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(float)).ToList(),
            (typeof(StringToNumberConverter<float>), new ConverterMappingHints(size: 64)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_enum()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(Queen)).ToList(),
            (typeof(StringToEnumConverter<Queen>), default));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_DateTime()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(DateTime)).ToList(),
            (typeof(StringToDateTimeConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_DateTimeOffset()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(DateTimeOffset)).ToList(),
            (typeof(StringToDateTimeOffsetConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_TimeOnly()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(TimeOnly)).ToList(),
            (typeof(StringToTimeOnlyConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_TimeSpan()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(TimeSpan)).ToList(),
            (typeof(StringToTimeSpanConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_Guid()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(Guid)).ToList(),
            (typeof(StringToGuidConverter), new ConverterMappingHints(size: 36)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_Uri()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(Uri)).ToList(),
            (typeof(StringToUriConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_bool()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(bool)).ToList(),
            (typeof(StringToBoolConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_char()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(char)).ToList(),
            (typeof(StringToCharConverter), new ConverterMappingHints(size: 1)));

    [ConditionalFact]
    public void Can_get_converters_for_string_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(string), typeof(byte[])).ToList(),
            (typeof(StringToBytesConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_bytes()
        => AssertConverters(
            _selector.Select(typeof(byte[])).ToList(),
            (typeof(BytesToStringConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_bytes_to_strings()
        => AssertConverters(
            _selector.Select(typeof(byte[]), typeof(string)).ToList(),
            (typeof(BytesToStringConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_DateTime()
        => AssertConverters(
            _selector.Select(typeof(DateTime)).ToList(),
            (typeof(DateTimeToStringConverter), new ConverterMappingHints(size: 48)),
            (typeof(DateTimeToBinaryConverter), default),
            (typeof(CompositeValueConverter<DateTime, long, byte[]>), new ConverterMappingHints(size: 8)));

    [ConditionalFact]
    public void Can_get_converters_for_DateTime_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(DateTime), typeof(byte[])).ToList(),
            (typeof(CompositeValueConverter<DateTime, long, byte[]>), new ConverterMappingHints(size: 8)));

    [ConditionalFact]
    public void Can_get_converters_for_DateTime_to_string()
        => AssertConverters(
            _selector.Select(typeof(DateTime), typeof(string)).ToList(),
            (typeof(DateTimeToStringConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_DateTime_to_long()
        => AssertConverters(
            _selector.Select(typeof(DateTime), typeof(long)).ToList(),
            (typeof(DateTimeToBinaryConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_DateTimeOffset()
        => AssertConverters(
            _selector.Select(typeof(DateTimeOffset)).ToList(),
            (typeof(DateTimeOffsetToStringConverter), new ConverterMappingHints(size: 48)),
            (typeof(DateTimeOffsetToBinaryConverter), default),
            (typeof(DateTimeOffsetToBytesConverter), new ConverterMappingHints(size: 12)));

    [ConditionalFact]
    public void Can_get_converters_for_DateTimeOffset_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(DateTimeOffset), typeof(byte[])).ToList(),
            (typeof(DateTimeOffsetToBytesConverter), new ConverterMappingHints(size: 12)));

    [ConditionalFact]
    public void Can_get_converters_for_DateTimeOffset_to_string()
        => AssertConverters(
            _selector.Select(typeof(DateTimeOffset), typeof(string)).ToList(),
            (typeof(DateTimeOffsetToStringConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_DateTimeOffset_to_long()
        => AssertConverters(
            _selector.Select(typeof(DateTimeOffset), typeof(long)).ToList(),
            (typeof(DateTimeOffsetToBinaryConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_TimeOnly()
        => AssertConverters(
            _selector.Select(typeof(TimeOnly)).ToList(),
            (typeof(TimeOnlyToStringConverter), new ConverterMappingHints(size: 48)),
            (typeof(TimeOnlyToTicksConverter), default),
            (typeof(CompositeValueConverter<TimeOnly, long, byte[]>), new ConverterMappingHints(size: 8)));

    [ConditionalFact]
    public void Can_get_converters_for_TimeOnly_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(TimeOnly), typeof(byte[])).ToList(),
            (typeof(CompositeValueConverter<TimeOnly, long, byte[]>), new ConverterMappingHints(size: 8)));

    [ConditionalFact]
    public void Can_get_converters_for_TimeOnly_to_string()
        => AssertConverters(
            _selector.Select(typeof(TimeOnly), typeof(string)).ToList(),
            (typeof(TimeOnlyToStringConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_TimeOnly_to_long()
        => AssertConverters(
            _selector.Select(typeof(TimeOnly), typeof(long)).ToList(),
            (typeof(TimeOnlyToTicksConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_TimeSpan()
        => AssertConverters(
            _selector.Select(typeof(TimeSpan)).ToList(),
            (typeof(TimeSpanToStringConverter), new ConverterMappingHints(size: 48)),
            (typeof(TimeSpanToTicksConverter), default),
            (typeof(CompositeValueConverter<TimeSpan, long, byte[]>), new ConverterMappingHints(size: 8)));

    [ConditionalFact]
    public void Can_get_converters_for_TimeSpan_to_bytes()
        => AssertConverters(
            _selector.Select(typeof(TimeSpan), typeof(byte[])).ToList(),
            (typeof(CompositeValueConverter<TimeSpan, long, byte[]>), new ConverterMappingHints(size: 8)));

    [ConditionalFact]
    public void Can_get_converters_for_TimeSpan_to_string()
        => AssertConverters(
            _selector.Select(typeof(TimeSpan), typeof(string)).ToList(),
            (typeof(TimeSpanToStringConverter), new ConverterMappingHints(size: 48)));

    [ConditionalFact]
    public void Can_get_converters_for_TimeSpan_to_long()
        => AssertConverters(
            _selector.Select(typeof(TimeSpan), typeof(long)).ToList(),
            (typeof(TimeSpanToTicksConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_Uri_to_string()
        => AssertConverters(
            _selector.Select(typeof(Uri), typeof(string)).ToList(),
            (typeof(UriToStringConverter), default));

    [ConditionalFact]
    public void Can_get_converters_for_IPAddress_to_string()
    {
        AssertConverters(
            _selector.Select(typeof(IPAddress), typeof(string)).ToList(),
            (typeof(IPAddressToStringConverter), new ConverterMappingHints(size: 45)));

        AssertConverters(
            _selector.Select(IPAddress.Loopback.GetType(), typeof(string)).ToList(),
            (typeof(IPAddressToStringConverter), new ConverterMappingHints(size: 45)));
    }

    [ConditionalFact]
    public void Can_get_converters_for_IPAddress_to_bytes()
    {
        AssertConverters(
            _selector.Select(typeof(IPAddress), typeof(byte[])).ToList(),
            (typeof(IPAddressToBytesConverter), new ConverterMappingHints(size: 16)));

        AssertConverters(
            _selector.Select(IPAddress.Loopback.GetType(), typeof(byte[])).ToList(),
            (typeof(IPAddressToBytesConverter), new ConverterMappingHints(size: 16)));
    }

    private static void AssertConverters(
        IList<ValueConverterInfo> converterInfos,
        params (Type InfoType, ConverterMappingHints Hints)[] converterTypes)
    {
        Assert.Equal(converterTypes.Length, converterInfos.Count);

        for (var i = 0; i < converterTypes.Length; i++)
        {
            var converter = converterInfos[i].Create();
            Assert.Equal(converterTypes[i].InfoType, converter.GetType());
            AssertHints(converterTypes[i].Hints, converterInfos[i].MappingHints);
            AssertHints(converterTypes[i].Hints, converter.MappingHints);
        }
    }

    private static void AssertHints(ConverterMappingHints expected, ConverterMappingHints actual)
    {
        Assert.Equal(actual?.IsUnicode, expected?.IsUnicode);
        Assert.Equal(actual?.Precision, expected?.Precision);
        Assert.Equal(actual?.Scale, expected?.Scale);
        Assert.Equal(actual?.Size, expected?.Size);
    }

    private enum Queen
    {
        Freddie,
        Brian,
        Rodger,
        John
    }

    private enum Nwa : byte
    {
        Yella,
        Dre,
        Eazy,
        Cube,
        Ren
    }

    private enum Gnr : ulong
    {
        Axl,
        Duff,
        Slash,
        Izzy,
        Stephen
    }

    private enum Velvets : long
    {
        Lou,
        John,
        Sterling,
        Maureen
    }
}
