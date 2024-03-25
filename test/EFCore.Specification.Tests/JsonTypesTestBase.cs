// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Microsoft.EntityFrameworkCore;

public abstract class JsonTypesTestBase : NonSharedModelTestBase
{
    [ConditionalTheory]
    [InlineData(sbyte.MinValue, """{"Prop":-128}""")]
    [InlineData(sbyte.MaxValue, """{"Prop":127}""")]
    [InlineData((sbyte)0, """{"Prop":0}""")]
    [InlineData((sbyte)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_sbyte_JSON_values(sbyte value, string json)
        => Can_read_and_write_JSON_value<Int8Type, sbyte>(nameof(Int8Type.Int8), value, json);

    protected class Int8Type
    {
        public sbyte Int8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(short.MinValue, """{"Prop":-32768}""")]
    [InlineData(short.MaxValue, """{"Prop":32767}""")]
    [InlineData((short)0, """{"Prop":0}""")]
    [InlineData((short)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_short_JSON_values(short value, string json)
        => Can_read_and_write_JSON_value<Int16Type, short>(nameof(Int16Type.Int16), value, json);

    protected class Int16Type
    {
        public short Int16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    public virtual Task Can_read_write_int_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value<Int32Type, int>(nameof(Int32Type.Int32), value, json);

    protected class Int32Type
    {
        public int Int32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(long.MinValue, """{"Prop":-9223372036854775808}""")]
    [InlineData(long.MaxValue, """{"Prop":9223372036854775807}""")]
    [InlineData((long)0, """{"Prop":0}""")]
    [InlineData((long)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_long_JSON_values(long value, string json)
        => Can_read_and_write_JSON_value<Int64Type, long>(nameof(Int64Type.Int64), value, json);

    protected class Int64Type
    {
        public long Int64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(byte.MinValue, """{"Prop":0}""")]
    [InlineData(byte.MaxValue, """{"Prop":255}""")]
    [InlineData((byte)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_byte_JSON_values(byte value, string json)
        => Can_read_and_write_JSON_value<UInt8Type, byte>(nameof(UInt8Type.UInt8), value, json);

    protected class UInt8Type
    {
        public byte UInt8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(ushort.MinValue, """{"Prop":0}""")]
    [InlineData(ushort.MaxValue, """{"Prop":65535}""")]
    [InlineData((ushort)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_ushort_JSON_values(ushort value, string json)
        => Can_read_and_write_JSON_value<UInt16Type, ushort>(nameof(UInt16Type.UInt16), value, json);

    protected class UInt16Type
    {
        public ushort UInt16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(uint.MinValue, """{"Prop":0}""")]
    [InlineData(uint.MaxValue, """{"Prop":4294967295}""")]
    [InlineData((uint)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_uint_JSON_values(uint value, string json)
        => Can_read_and_write_JSON_value<UInt32Type, uint>(nameof(UInt32Type.UInt32), value, json);

    protected class UInt32Type
    {
        public uint UInt32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(ulong.MinValue, """{"Prop":0}""")]
    [InlineData(ulong.MaxValue, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)1, """{"Prop":1}""")]
    public virtual Task Can_read_write_ulong_JSON_values(ulong value, string json)
        => Can_read_and_write_JSON_value<UInt64Type, ulong>(nameof(UInt64Type.UInt64), value, json);

    protected class UInt64Type
    {
        public ulong UInt64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(float.MinValue, """{"Prop":-3.4028235E+38}""")]
    [InlineData(float.MaxValue, """{"Prop":3.4028235E+38}""")]
    [InlineData((float)0.0, """{"Prop":0}""")]
    [InlineData((float)1.1, """{"Prop":1.1}""")]
    public virtual Task Can_read_write_float_JSON_values(float value, string json)
        => Can_read_and_write_JSON_value<FloatType, float>(nameof(FloatType.Float), value, json);

    protected class FloatType
    {
        public float Float { get; set; }
    }

    [ConditionalTheory]
    [InlineData(double.MinValue, """{"Prop":-1.7976931348623157E+308}""")]
    [InlineData(double.MaxValue, """{"Prop":1.7976931348623157E+308}""")]
    [InlineData(0.0, """{"Prop":0}""")]
    [InlineData(1.1, """{"Prop":1.1}""")]
    public virtual Task Can_read_write_double_JSON_values(double value, string json)
        => Can_read_and_write_JSON_value<DoubleType, double>(nameof(DoubleType.Double), value, json);

    protected class DoubleType
    {
        public double Double { get; set; }
    }

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":-79228162514264337593543950335}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":79228162514264337593543950335}""")]
    [InlineData("0.0", """{"Prop":0.0}""")]
    [InlineData("1.1", """{"Prop":1.1}""")]
    public virtual Task Can_read_write_decimal_JSON_values(decimal value, string json)
        => Can_read_and_write_JSON_value<DecimalType, decimal>(nameof(DecimalType.Decimal), value, json);

    protected class DecimalType
    {
        public decimal Decimal { get; set; }
    }

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"0001-01-01"}""")]
    [InlineData("12/31/9999", """{"Prop":"9999-12-31"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    public virtual Task Can_read_write_DateOnly_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<DateOnlyType, DateOnly>(
            nameof(DateOnlyType.DateOnly),
            DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    protected class DateOnlyType
    {
        public DateOnly DateOnly { get; set; }
    }

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", """{"Prop":"00:00:00.0000000"}""")]
    [InlineData("23:59:59.9999999", """{"Prop":"23:59:59.9999999"}""")]
    [InlineData("11:05:12.3456789", """{"Prop":"11:05:12.3456789"}""")]
    public virtual Task Can_read_write_TimeOnly_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<TimeOnlyType, TimeOnly>(
            nameof(TimeOnlyType.TimeOnly),
            TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    protected class TimeOnlyType
    {
        public TimeOnly TimeOnly { get; set; }
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01T00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31T23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29T10:52:47.2064353"}""")]
    public virtual Task Can_read_write_DateTime_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeType, DateTime>(
            nameof(DateTimeType.DateTime),
            DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    protected class DateTimeType
    {
        public DateTime DateTime { get; set; }
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31T23:59:59.9999999+02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29T11:11:15.5672854+04:00"}""")]
    public virtual Task Can_read_write_DateTimeOffset_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeOffsetType, DateTimeOffset>(
            nameof(DateTimeOffsetType.DateTimeOffset),
            DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    protected class DateTimeOffsetType
    {
        public DateTimeOffset DateTimeOffset { get; set; }
    }

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", """{"Prop":"-10675199:2:48:05.4775808"}""")]
    [InlineData("10675199.02:48:05.4775807", """{"Prop":"10675199:2:48:05.4775807"}""")]
    [InlineData("00:00:00", """{"Prop":"0:00:00"}""")]
    [InlineData("12:23:23.8018854", """{"Prop":"12:23:23.8018854"}""")]
    public virtual Task Can_read_write_TimeSpan_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<TimeSpanType, TimeSpan>(
            nameof(TimeSpanType.TimeSpan),
            TimeSpan.Parse(value), json);

    protected class TimeSpanType
    {
        public TimeSpan TimeSpan { get; set; }
    }

    [ConditionalTheory]
    [InlineData(false, """{"Prop":false}""")]
    [InlineData(true, """{"Prop":true}""")]
    public virtual Task Can_read_write_bool_JSON_values(bool value, string json)
        => Can_read_and_write_JSON_value<BooleanType, bool>(nameof(BooleanType.Boolean), value, json);

    protected class BooleanType
    {
        public bool Boolean { get; set; }
    }

    [ConditionalTheory]
    [InlineData(char.MinValue, """{"Prop":"\u0000"}""")]
    [InlineData(char.MaxValue, """{"Prop":"\uFFFF"}""")]
    [InlineData(' ', """{"Prop":" "}""")]
    [InlineData("Z", """{"Prop":"Z"}""")]
    public virtual Task Can_read_write_char_JSON_values(char value, string json)
        => Can_read_and_write_JSON_value<CharacterType, char>(nameof(CharacterType.Character), value, json);

    protected class CharacterType
    {
        public char Character { get; set; }
    }

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"ffffffff-ffff-ffff-ffff-ffffffffffff"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8c44242f-8e3f-4a20-8be8-98c7c1aadebd"}""")]
    public virtual Task Can_read_write_GUID_JSON_values(Guid value, string json)
        => Can_read_and_write_JSON_value<GuidType, Guid>(nameof(GuidType.Guid), value, json);

    protected class GuidType
    {
        public Guid Guid { get; set; }
    }

    [ConditionalTheory]
    [InlineData("MinValue", """{"Prop":"MinValue"}""")]
    [InlineData("MaxValue", """{"Prop":"MaxValue"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    public virtual Task Can_read_write_string_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<StringType, string>(nameof(StringType.String), value, json);

    protected class StringType
    {
        public string String { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"AAAAAQ=="}""")]
    [InlineData("255,255,255,255", """{"Prop":"/////w=="}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"AQIDBA=="}""")]
    public virtual Task Can_read_write_binary_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<BytesType, byte[]>(
            nameof(BytesType.Bytes),
            value == "" ? [] : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    protected class BytesType
    {
        public byte[] Bytes { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        """{"Prop":"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName"}""")]
    [InlineData("file:///C:/test/path/file.txt", """{"Prop":"file:///C:/test/path/file.txt"}""")]
    public virtual Task Can_read_write_URI_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<UriType, Uri>(nameof(UriType.Uri), new Uri(value), json);

    protected class UriType
    {
        public Uri Uri { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData("127.0.0.1", """{"Prop":"127.0.0.1"}""")]
    [InlineData("0.0.0.0", """{"Prop":"0.0.0.0"}""")]
    [InlineData("255.255.255.255", """{"Prop":"255.255.255.255"}""")]
    [InlineData("192.168.1.156", """{"Prop":"192.168.1.156"}""")]
    [InlineData("::1", """{"Prop":"::1"}""")]
    [InlineData("::", """{"Prop":"::"}""")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", """{"Prop":"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"}""")]
    public virtual Task Can_read_write_IP_address_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<IPAddressType, IPAddress>(nameof(IPAddressType.IpAddress), IPAddress.Parse(value), json);

    protected class IPAddressType
    {
        public IPAddress IpAddress { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData("001122334455", """{"Prop":"001122334455"}""")]
    [InlineData("00-11-22-33-44-55", """{"Prop":"001122334455"}""")]
    [InlineData("0011.2233.4455", """{"Prop":"001122334455"}""")]
    public virtual Task Can_read_write_physical_address_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<PhysicalAddressType, PhysicalAddress>(
            nameof(PhysicalAddressType.PhysicalAddress),
            PhysicalAddress.Parse(value), json);

    protected class PhysicalAddressType
    {
        public PhysicalAddress PhysicalAddress { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, """{"Prop":-128}""")]
    [InlineData((sbyte)Enum8.Max, """{"Prop":127}""")]
    [InlineData((sbyte)Enum8.Default, """{"Prop":0}""")]
    [InlineData((sbyte)Enum8.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_sbyte_enum_JSON_values(Enum8 value, string json)
        => Can_read_and_write_JSON_value<Enum8Type, Enum8>(nameof(Enum8Type.Enum8), value, json);

    protected class Enum8Type
    {
        public Enum8 Enum8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, """{"Prop":-32768}""")]
    [InlineData((short)Enum16.Max, """{"Prop":32767}""")]
    [InlineData((short)Enum16.Default, """{"Prop":0}""")]
    [InlineData((short)Enum16.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_short_enum_JSON_values(Enum16 value, string json)
        => Can_read_and_write_JSON_value<Enum16Type, Enum16>(nameof(Enum16Type.Enum16), value, json);

    protected class Enum16Type
    {
        public Enum16 Enum16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, """{"Prop":-2147483648}""")]
    [InlineData((int)Enum32.Max, """{"Prop":2147483647}""")]
    [InlineData((int)Enum32.Default, """{"Prop":0}""")]
    [InlineData((int)Enum32.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_int_enum_JSON_values(Enum32 value, string json)
        => Can_read_and_write_JSON_value<Enum32Type, Enum32>(nameof(Enum32Type.Enum32), value, json);

    protected class Enum32Type
    {
        public Enum32 Enum32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, """{"Prop":-9223372036854775808}""")]
    [InlineData((long)Enum64.Max, """{"Prop":9223372036854775807}""")]
    [InlineData((long)Enum64.Default, """{"Prop":0}""")]
    [InlineData((long)Enum64.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_long_enum_JSON_values(Enum64 value, string json)
        => Can_read_and_write_JSON_value<Enum64Type, Enum64>(nameof(Enum64Type.Enum64), value, json);

    protected class Enum64Type
    {
        public Enum64 Enum64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, """{"Prop":0}""")]
    [InlineData((byte)EnumU8.Max, """{"Prop":255}""")]
    [InlineData((byte)EnumU8.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_byte_enum_JSON_values(EnumU8 value, string json)
        => Can_read_and_write_JSON_value<EnumU8Type, EnumU8>(nameof(EnumU8Type.EnumU8), value, json);

    protected class EnumU8Type
    {
        public EnumU8 EnumU8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, """{"Prop":0}""")]
    [InlineData((ushort)EnumU16.Max, """{"Prop":65535}""")]
    [InlineData((ushort)EnumU16.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_ushort_enum_JSON_values(EnumU16 value, string json)
        => Can_read_and_write_JSON_value<EnumU16Type, EnumU16>(nameof(EnumU16Type.EnumU16), value, json);

    protected class EnumU16Type
    {
        public EnumU16 EnumU16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, """{"Prop":0}""")]
    [InlineData((uint)EnumU32.Max, """{"Prop":4294967295}""")]
    [InlineData((uint)EnumU32.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_uint_enum_JSON_values(EnumU32 value, string json)
        => Can_read_and_write_JSON_value<EnumU32Type, EnumU32>(nameof(EnumU32Type.EnumU32), value, json);

    protected class EnumU32Type
    {
        public EnumU32 EnumU32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, """{"Prop":0}""")]
    [InlineData((ulong)EnumU64.Max, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)EnumU64.One, """{"Prop":1}""")]
    public virtual Task Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
        => Can_read_and_write_JSON_value<EnumU64Type, EnumU64>(nameof(EnumU64Type.EnumU64), value, json);

    protected class EnumU64Type
    {
        public EnumU64 EnumU64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, """{"Prop":-128}""")]
    [InlineData(sbyte.MaxValue, """{"Prop":127}""")]
    [InlineData((sbyte)0, """{"Prop":0}""")]
    [InlineData((sbyte)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_sbyte_JSON_values(sbyte? value, string json)
        => Can_read_and_write_JSON_value<NullableInt8Type, sbyte?>(nameof(NullableInt8Type.Int8), value, json);

    protected class NullableInt8Type
    {
        public sbyte? Int8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(short.MinValue, """{"Prop":-32768}""")]
    [InlineData(short.MaxValue, """{"Prop":32767}""")]
    [InlineData((short)0, """{"Prop":0}""")]
    [InlineData((short)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_short_JSON_values(short? value, string json)
        => Can_read_and_write_JSON_value<NullableInt16Type, short?>(nameof(NullableInt16Type.Int16), value, json);

    protected class NullableInt16Type
    {
        public short? Int16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_int_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value<NullableInt32Type, int?>(nameof(NullableInt32Type.Int32), value, json);

    protected class NullableInt32Type
    {
        public int? Int32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(long.MinValue, """{"Prop":-9223372036854775808}""")]
    [InlineData(long.MaxValue, """{"Prop":9223372036854775807}""")]
    [InlineData((long)0, """{"Prop":0}""")]
    [InlineData((long)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_long_JSON_values(long? value, string json)
        => Can_read_and_write_JSON_value<NullableInt64Type, long?>(nameof(NullableInt64Type.Int64), value, json);

    protected class NullableInt64Type
    {
        public long? Int64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(byte.MinValue, """{"Prop":0}""")]
    [InlineData(byte.MaxValue, """{"Prop":255}""")]
    [InlineData((byte)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_byte_JSON_values(byte? value, string json)
        => Can_read_and_write_JSON_value<NullableUInt8Type, byte?>(nameof(NullableUInt8Type.UInt8), value, json);

    protected class NullableUInt8Type
    {
        public byte? UInt8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(ushort.MinValue, """{"Prop":0}""")]
    [InlineData(ushort.MaxValue, """{"Prop":65535}""")]
    [InlineData((ushort)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ushort_JSON_values(ushort? value, string json)
        => Can_read_and_write_JSON_value<NullableUInt16Type, ushort?>(nameof(NullableUInt16Type.UInt16), value, json);

    protected class NullableUInt16Type
    {
        public ushort? UInt16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(uint.MinValue, """{"Prop":0}""")]
    [InlineData(uint.MaxValue, """{"Prop":4294967295}""")]
    [InlineData((uint)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_uint_JSON_values(uint? value, string json)
        => Can_read_and_write_JSON_value<NullableUInt32Type, uint?>(nameof(NullableUInt32Type.UInt32), value, json);

    protected class NullableUInt32Type
    {
        public uint? UInt32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(ulong.MinValue, """{"Prop":0}""")]
    [InlineData(ulong.MaxValue, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ulong_JSON_values(ulong? value, string json)
        => Can_read_and_write_JSON_value<NullableUInt64Type, ulong?>(nameof(NullableUInt64Type.UInt64), value, json);

    protected class NullableUInt64Type
    {
        public ulong? UInt64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(float.MinValue, """{"Prop":-3.4028235E+38}""")]
    [InlineData(float.MaxValue, """{"Prop":3.4028235E+38}""")]
    [InlineData((float)0.0, """{"Prop":0}""")]
    [InlineData((float)1.1, """{"Prop":1.1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_float_JSON_values(float? value, string json)
        => Can_read_and_write_JSON_value<NullableFloatType, float?>(nameof(NullableFloatType.Float), value, json);

    protected class NullableFloatType
    {
        public float? Float { get; set; }
    }

    [ConditionalTheory]
    [InlineData(double.MinValue, """{"Prop":-1.7976931348623157E+308}""")]
    [InlineData(double.MaxValue, """{"Prop":1.7976931348623157E+308}""")]
    [InlineData(0.0, """{"Prop":0}""")]
    [InlineData(1.1, """{"Prop":1.1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_double_JSON_values(double? value, string json)
        => Can_read_and_write_JSON_value<NullableDoubleType, double?>(nameof(NullableDoubleType.Double), value, json);

    protected class NullableDoubleType
    {
        public double? Double { get; set; }
    }

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":-79228162514264337593543950335}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":79228162514264337593543950335}""")]
    [InlineData("0.0", """{"Prop":0.0}""")]
    [InlineData("1.1", """{"Prop":1.1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_decimal_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDecimalType, decimal?>(
            nameof(NullableDecimalType.Decimal),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    protected class NullableDecimalType
    {
        public decimal? Decimal { get; set; }
    }

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"0001-01-01"}""")]
    [InlineData("12/31/9999", """{"Prop":"9999-12-31"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_DateOnly_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateOnlyType, DateOnly?>(
            nameof(NullableDateOnlyType.DateOnly),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    protected class NullableDateOnlyType
    {
        public DateOnly? DateOnly { get; set; }
    }

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", """{"Prop":"00:00:00.0000000"}""")]
    [InlineData("23:59:59.9999999", """{"Prop":"23:59:59.9999999"}""")]
    [InlineData("11:05:12.3456789", """{"Prop":"11:05:12.3456789"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_TimeOnly_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableTimeOnlyType, TimeOnly?>(
            nameof(NullableTimeOnlyType.TimeOnly),
            value == null ? default(TimeOnly?) : TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    protected class NullableTimeOnlyType
    {
        public TimeOnly? TimeOnly { get; set; }
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01T00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31T23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29T10:52:47.2064353"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateTimeType, DateTime?>(
            nameof(NullableDateTimeType.DateTime),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    protected class NullableDateTimeType
    {
        public DateTime? DateTime { get; set; }
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31T23:59:59.9999999+02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29T11:11:15.5672854+04:00"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetType, DateTimeOffset?>(
            nameof(NullableDateTimeOffsetType.DateTimeOffset),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    protected class NullableDateTimeOffsetType
    {
        public DateTimeOffset? DateTimeOffset { get; set; }
    }

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", """{"Prop":"-10675199:2:48:05.4775808"}""")]
    [InlineData("10675199.02:48:05.4775807", """{"Prop":"10675199:2:48:05.4775807"}""")]
    [InlineData("00:00:00", """{"Prop":"0:00:00"}""")]
    [InlineData("12:23:23.8018854", """{"Prop":"12:23:23.8018854"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_TimeSpan_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableTimeSpanType, TimeSpan?>(
            nameof(NullableTimeSpanType.TimeSpan),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    protected class NullableTimeSpanType
    {
        public TimeSpan? TimeSpan { get; set; }
    }

    [ConditionalTheory]
    [InlineData(false, """{"Prop":false}""")]
    [InlineData(true, """{"Prop":true}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_bool_JSON_values(bool? value, string json)
        => Can_read_and_write_JSON_value<NullableBooleanType, bool?>(nameof(NullableBooleanType.Boolean), value, json);

    protected class NullableBooleanType
    {
        public bool? Boolean { get; set; }
    }

    [ConditionalTheory]
    [InlineData(char.MinValue, """{"Prop":"\u0000"}""")]
    [InlineData(char.MaxValue, """{"Prop":"\uFFFF"}""")]
    [InlineData(' ', """{"Prop":" "}""")]
    [InlineData('Z', """{"Prop":"Z"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_char_JSON_values(char? value, string json)
        => Can_read_and_write_JSON_value<NullableCharacterType, char?>(nameof(NullableCharacterType.Character), value, json);

    protected class NullableCharacterType
    {
        public char? Character { get; set; }
    }

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"ffffffff-ffff-ffff-ffff-ffffffffffff"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8c44242f-8e3f-4a20-8be8-98c7c1aadebd"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_GUID_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableGuidType, Guid?>(
            nameof(NullableGuidType.Guid),
            value == null ? null : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    protected class NullableGuidType
    {
        public Guid? Guid { get; set; }
    }

    [ConditionalTheory]
    [InlineData("MinValue", """{"Prop":"MinValue"}""")]
    [InlineData("MaxValue", """{"Prop":"MaxValue"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableStringType, string?>(nameof(NullableStringType.String), value, json);

    protected class NullableStringType
    {
        public string? String { get; set; }
    }

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"AAAAAQ=="}""")]
    [InlineData("255,255,255,255", """{"Prop":"/////w=="}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"AQIDBA=="}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_binary_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableBytesType, byte[]?>(
            nameof(NullableBytesType.Bytes),
            value == null
                ? default
                : value == ""
                    ? []
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    protected class NullableBytesType
    {
        public byte[]? Bytes { get; set; }
    }

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        """{"Prop":"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName"}""")]
    [InlineData("file:///C:/test/path/file.txt", """{"Prop":"file:///C:/test/path/file.txt"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_URI_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableUriType, Uri?>(
            nameof(NullableUriType.Uri),
            value == null ? default : new Uri(value), json);

    protected class NullableUriType
    {
        public Uri? Uri { get; set; }
    }

    [ConditionalTheory]
    [InlineData("127.0.0.1", """{"Prop":"127.0.0.1"}""")]
    [InlineData("0.0.0.0", """{"Prop":"0.0.0.0"}""")]
    [InlineData("255.255.255.255", """{"Prop":"255.255.255.255"}""")]
    [InlineData("192.168.1.156", """{"Prop":"192.168.1.156"}""")]
    [InlineData("::1", """{"Prop":"::1"}""")]
    [InlineData("::", """{"Prop":"::"}""")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", """{"Prop":"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_IP_address_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullableIPAddressType, IPAddress?>(
            nameof(NullableIPAddressType.IpAddress),
            value == null ? default : IPAddress.Parse(value), json);

    protected class NullableIPAddressType
    {
        public IPAddress? IpAddress { get; set; }
    }

    [ConditionalTheory]
    [InlineData("001122334455", """{"Prop":"001122334455"}""")]
    [InlineData("00-11-22-33-44-55", """{"Prop":"001122334455"}""")]
    [InlineData("0011.2233.4455", """{"Prop":"001122334455"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_physical_address_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value<NullablePhysicalAddressType, PhysicalAddress?>(
            nameof(NullablePhysicalAddressType.PhysicalAddress),
            value == null ? default : PhysicalAddress.Parse(value), json);

    protected class NullablePhysicalAddressType
    {
        public PhysicalAddress? PhysicalAddress { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, """{"Prop":-128}""")]
    [InlineData((sbyte)Enum8.Max, """{"Prop":127}""")]
    [InlineData((sbyte)Enum8.Default, """{"Prop":0}""")]
    [InlineData((sbyte)Enum8.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_sbyte_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnum8Type, Enum8?>(
            nameof(NullableEnum8Type.Enum8),
            value == null ? default(Enum8?) : (Enum8)value, json);

    protected class NullableEnum8Type
    {
        public Enum8? Enum8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, """{"Prop":-32768}""")]
    [InlineData((short)Enum16.Max, """{"Prop":32767}""")]
    [InlineData((short)Enum16.Default, """{"Prop":0}""")]
    [InlineData((short)Enum16.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_short_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnum16Type, Enum16?>(
            nameof(NullableEnum16Type.Enum16),
            value == null ? default(Enum16?) : (Enum16)value, json);

    protected class NullableEnum16Type
    {
        public Enum16? Enum16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, """{"Prop":-2147483648}""")]
    [InlineData((int)Enum32.Max, """{"Prop":2147483647}""")]
    [InlineData((int)Enum32.Default, """{"Prop":0}""")]
    [InlineData((int)Enum32.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_int_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnum32Type, Enum32?>(
            nameof(NullableEnum32Type.Enum32),
            value == null ? default(Enum32?) : (Enum32)value, json);

    protected class NullableEnum32Type
    {
        public Enum32? Enum32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, """{"Prop":-9223372036854775808}""")]
    [InlineData((long)Enum64.Max, """{"Prop":9223372036854775807}""")]
    [InlineData((long)Enum64.Default, """{"Prop":0}""")]
    [InlineData((long)Enum64.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_long_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnum64Type, Enum64?>(
            nameof(NullableEnum64Type.Enum64),
            value == null ? default(Enum64?) : (Enum64)value, json);

    protected class NullableEnum64Type
    {
        public Enum64? Enum64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, """{"Prop":0}""")]
    [InlineData((byte)EnumU8.Max, """{"Prop":255}""")]
    [InlineData((byte)EnumU8.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_byte_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnumU8Type, EnumU8?>(
            nameof(NullableEnumU8Type.EnumU8),
            value == null ? default(EnumU8?) : (EnumU8)value, json);

    protected class NullableEnumU8Type
    {
        public EnumU8? EnumU8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, """{"Prop":0}""")]
    [InlineData((ushort)EnumU16.Max, """{"Prop":65535}""")]
    [InlineData((ushort)EnumU16.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ushort_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnumU16Type, EnumU16?>(
            nameof(NullableEnumU16Type.EnumU16),
            value == null ? default(EnumU16?) : (EnumU16)value, json);

    protected class NullableEnumU16Type
    {
        public EnumU16? EnumU16 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, """{"Prop":0}""")]
    [InlineData((uint)EnumU32.Max, """{"Prop":4294967295}""")]
    [InlineData((uint)EnumU32.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_uint_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnumU32Type, EnumU32?>(
            nameof(NullableEnumU32Type.EnumU32),
            value == null ? default(EnumU32?) : (EnumU32)value, json);

    protected class NullableEnumU32Type
    {
        public EnumU32? EnumU32 { get; set; }
    }

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, """{"Prop":0}""")]
    [InlineData((ulong)EnumU64.Max, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)EnumU64.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnumU64Type, EnumU64?>(
            nameof(NullableEnumU64Type.EnumU64),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    protected class NullableEnumU64Type
    {
        public EnumU64? EnumU64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, """{"Prop":"-128"}""")]
    [InlineData(sbyte.MaxValue, """{"Prop":"127"}""")]
    [InlineData((sbyte)0, """{"Prop":"0"}""")]
    [InlineData((sbyte)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_sbyte_as_string_JSON_values(sbyte? value, string json)
        => Can_read_and_write_JSON_property_value<NullableInt8Type, sbyte?>(
            b => b.HasConversion<string>(),
            nameof(NullableInt8Type.Int8), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, """{"Prop":"-32768"}""")]
    [InlineData(short.MaxValue, """{"Prop":"32767"}""")]
    [InlineData((short)0, """{"Prop":"0"}""")]
    [InlineData((short)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_short_as_string_JSON_values(short? value, string json)
        => Can_read_and_write_JSON_property_value<NullableInt16Type, short?>(
            b => b.HasConversion<string>(),
            nameof(NullableInt16Type.Int16), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":"-2147483648"}""")]
    [InlineData(int.MaxValue, """{"Prop":"2147483647"}""")]
    [InlineData(0, """{"Prop":"0"}""")]
    [InlineData(1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_int_as_string_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_property_value<NullableInt32Type, int?>(
            b => b.HasConversion<string>(),
            nameof(NullableInt32Type.Int32), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, """{"Prop":"-9223372036854775808"}""")]
    [InlineData(long.MaxValue, """{"Prop":"9223372036854775807"}""")]
    [InlineData((long)0, """{"Prop":"0"}""")]
    [InlineData((long)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_long_as_string_JSON_values(long? value, string json)
        => Can_read_and_write_JSON_property_value<NullableInt64Type, long?>(
            b => b.HasConversion<string>(),
            nameof(NullableInt64Type.Int64), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, """{"Prop":"0"}""")]
    [InlineData(byte.MaxValue, """{"Prop":"255"}""")]
    [InlineData((byte)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_byte_as_string_JSON_values(byte? value, string json)
        => Can_read_and_write_JSON_property_value<NullableUInt8Type, byte?>(
            b => b.HasConversion<string>(),
            nameof(NullableUInt8Type.UInt8), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, """{"Prop":"0"}""")]
    [InlineData(ushort.MaxValue, """{"Prop":"65535"}""")]
    [InlineData((ushort)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ushort_as_string_JSON_values(ushort? value, string json)
        => Can_read_and_write_JSON_property_value<NullableUInt16Type, ushort?>(
            b => b.HasConversion<string>(),
            nameof(NullableUInt16Type.UInt16), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, """{"Prop":"0"}""")]
    [InlineData(uint.MaxValue, """{"Prop":"4294967295"}""")]
    [InlineData((uint)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_uint_as_string_JSON_values(uint? value, string json)
        => Can_read_and_write_JSON_property_value<NullableUInt32Type, uint?>(
            b => b.HasConversion<string>(),
            nameof(NullableUInt32Type.UInt32), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, """{"Prop":"0"}""")]
    [InlineData(ulong.MaxValue, """{"Prop":"18446744073709551615"}""")]
    [InlineData((ulong)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ulong_as_string_JSON_values(ulong? value, string json)
        => Can_read_and_write_JSON_property_value<NullableUInt64Type, ulong?>(
            b => b.HasConversion<string>(),
            nameof(NullableUInt64Type.UInt64), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, """{"Prop":"-3.4028235E\u002B38"}""")]
    [InlineData(float.MaxValue, """{"Prop":"3.4028235E\u002B38"}""")]
    [InlineData((float)0.0, """{"Prop":"0"}""")]
    [InlineData((float)1.1, """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_float_as_string_JSON_values(float? value, string json)
        => Can_read_and_write_JSON_property_value<NullableFloatType, float?>(
            b => b.HasConversion<string>(),
            nameof(NullableFloatType.Float), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, """{"Prop":"-1.7976931348623157E\u002B308"}""")]
    [InlineData(double.MaxValue, """{"Prop":"1.7976931348623157E\u002B308"}""")]
    [InlineData(0.0, """{"Prop":"0"}""")]
    [InlineData(1.1, """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_double_as_string_JSON_values(double? value, string json)
        => Can_read_and_write_JSON_property_value<NullableDoubleType, double?>(
            b => b.HasConversion<string>(),
            nameof(NullableDoubleType.Double), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":"-79228162514264337593543950335"}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":"79228162514264337593543950335"}""")]
    [InlineData("0.0", """{"Prop":"0.0"}""")]
    [InlineData("1.1", """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_decimal_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableDecimalType, decimal?>(
            b => b.HasConversion<string>(),
            nameof(NullableDecimalType.Decimal),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"0001-01-01"}""")]
    [InlineData("12/31/9999", """{"Prop":"9999-12-31"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_DateOnly_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableDateOnlyType, DateOnly?>(
            b => b.HasConversion<string>(),
            nameof(NullableDateOnlyType.DateOnly),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", """{"Prop":"00:00:00"}""")]
    [InlineData("23:59:59.9999999", """{"Prop":"23:59:59.9999999"}""")]
    [InlineData("11:05:12.3456789", """{"Prop":"11:05:12.3456789"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_TimeOnly_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableTimeOnlyType, TimeOnly?>(
            b => b.HasConversion<string>(),
            nameof(NullableTimeOnlyType.TimeOnly),
            value == null ? default(TimeOnly?) : TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01 00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31 23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29 10:52:47.2064353"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_DateTime_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableDateTimeType, DateTime?>(
            b => b.HasConversion<string>(),
            nameof(NullableDateTimeType.DateTime),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01 00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31 23:59:59.9999999\u002B02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01 00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29 11:11:15.5672854\u002B04:00"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_DateTimeOffset_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableDateTimeOffsetType, DateTimeOffset?>(
            b => b.HasConversion<string>(),
            nameof(NullableDateTimeOffsetType.DateTimeOffset),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", """{"Prop":"-10675199.02:48:05.4775808"}""")]
    [InlineData("10675199.02:48:05.4775807", """{"Prop":"10675199.02:48:05.4775807"}""")]
    [InlineData("00:00:00", """{"Prop":"00:00:00"}""")]
    [InlineData("12:23:23.8018854", """{"Prop":"12:23:23.8018854"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_TimeSpan_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableTimeSpanType, TimeSpan?>(
            b => b.HasConversion<string>(),
            nameof(NullableTimeSpanType.TimeSpan),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, """{"Prop":"0"}""")]
    [InlineData(true, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_bool_as_string_JSON_values(bool? value, string json)
        => Can_read_and_write_JSON_property_value<NullableBooleanType, bool?>(
            b => b.HasConversion<string>(),
            nameof(NullableBooleanType.Boolean), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, """{"Prop":"\u0000"}""")]
    [InlineData(char.MaxValue, """{"Prop":"\uFFFF"}""")]
    [InlineData(' ', """{"Prop":" "}""")]
    [InlineData('Z', """{"Prop":"Z"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_char_as_string_JSON_values(char? value, string json)
        => Can_read_and_write_JSON_property_value<NullableCharacterType, char?>(
            b => b.HasConversion<string>(),
            nameof(NullableCharacterType.Character), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"ffffffff-ffff-ffff-ffff-ffffffffffff"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8c44242f-8e3f-4a20-8be8-98c7c1aadebd"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_as_string_GUID_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableGuidType, Guid?>(
            b => b.HasConversion<string>(),
            nameof(NullableGuidType.Guid),
            value == null ? default(Guid?) : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("MinValue", """{"Prop":"MinValue"}""")]
    [InlineData("MaxValue", """{"Prop":"MaxValue"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_string_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableStringType, string?>(
            b => b.HasConversion<string>(),
            nameof(NullableStringType.String), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"AAAAAQ=="}""")]
    [InlineData("255,255,255,255", """{"Prop":"/////w=="}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"AQIDBA=="}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_binary_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableBytesType, byte[]?>(
            b => b.HasConversion<string>(),
            nameof(NullableBytesType.Bytes),
            value == null
                ? default
                : value == ""
                    ? []
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        """{"Prop":"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName"}""")]
    [InlineData("file:///C:/test/path/file.txt", """{"Prop":"file:///C:/test/path/file.txt"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_URI_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableUriType, Uri?>(
            b => b.HasConversion<string>(),
            nameof(NullableUriType.Uri),
            value == null ? default : new Uri(value), json);

    [ConditionalTheory]
    [InlineData("127.0.0.1", """{"Prop":"127.0.0.1"}""")]
    [InlineData("0.0.0.0", """{"Prop":"0.0.0.0"}""")]
    [InlineData("255.255.255.255", """{"Prop":"255.255.255.255"}""")]
    [InlineData("192.168.1.156", """{"Prop":"192.168.1.156"}""")]
    [InlineData("::1", """{"Prop":"::1"}""")]
    [InlineData("::", """{"Prop":"::"}""")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", """{"Prop":"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_IP_address_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullableIPAddressType, IPAddress?>(
            b => b.HasConversion<string>(),
            nameof(NullableIPAddressType.IpAddress),
            value == null ? default : IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", """{"Prop":"001122334455"}""")]
    [InlineData("00-11-22-33-44-55", """{"Prop":"001122334455"}""")]
    [InlineData("0011.2233.4455", """{"Prop":"001122334455"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_physical_address_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_property_value<NullablePhysicalAddressType, PhysicalAddress?>(
            b => b.HasConversion<string>(),
            nameof(NullablePhysicalAddressType.PhysicalAddress),
            value == null ? default : PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, """{"Prop":"Min"}""")]
    [InlineData((sbyte)Enum8.Max, """{"Prop":"Max"}""")]
    [InlineData((sbyte)Enum8.Default, """{"Prop":"Default"}""")]
    [InlineData((sbyte)Enum8.One, """{"Prop":"One"}""")]
    [InlineData((sbyte)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_sbyte_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnum8Type, Enum8?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnum8Type.Enum8),
            value == null ? default(Enum8?) : (Enum8)value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, """{"Prop":"Min"}""")]
    [InlineData((short)Enum16.Max, """{"Prop":"Max"}""")]
    [InlineData((short)Enum16.Default, """{"Prop":"Default"}""")]
    [InlineData((short)Enum16.One, """{"Prop":"One"}""")]
    [InlineData((short)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_short_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnum16Type, Enum16?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnum16Type.Enum16),
            value == null ? default(Enum16?) : (Enum16)value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, """{"Prop":"Min"}""")]
    [InlineData((int)Enum32.Max, """{"Prop":"Max"}""")]
    [InlineData((int)Enum32.Default, """{"Prop":"Default"}""")]
    [InlineData((int)Enum32.One, """{"Prop":"One"}""")]
    [InlineData(77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_int_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnum32Type, Enum32?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnum32Type.Enum32),
            value == null ? default(Enum32?) : (Enum32)value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, """{"Prop":"Min"}""")]
    [InlineData((long)Enum64.Max, """{"Prop":"Max"}""")]
    [InlineData((long)Enum64.Default, """{"Prop":"Default"}""")]
    [InlineData((long)Enum64.One, """{"Prop":"One"}""")]
    [InlineData((long)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_long_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnum64Type, Enum64?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnum64Type.Enum64),
            value == null ? default(Enum64?) : (Enum64)value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, """{"Prop":"Min"}""")]
    [InlineData((byte)EnumU8.Max, """{"Prop":"Max"}""")]
    [InlineData((byte)EnumU8.One, """{"Prop":"One"}""")]
    [InlineData((byte)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_byte_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnumU8Type, EnumU8?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnumU8Type.EnumU8),
            value == null ? default(EnumU8?) : (EnumU8)value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, """{"Prop":"Min"}""")]
    [InlineData((ushort)EnumU16.Max, """{"Prop":"Max"}""")]
    [InlineData((ushort)EnumU16.One, """{"Prop":"One"}""")]
    [InlineData((ushort)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ushort_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnumU16Type, EnumU16?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnumU16Type.EnumU16),
            value == null ? default(EnumU16?) : (EnumU16)value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, """{"Prop":"Min"}""")]
    [InlineData((uint)EnumU32.Max, """{"Prop":"Max"}""")]
    [InlineData((uint)EnumU32.One, """{"Prop":"One"}""")]
    [InlineData((uint)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_uint_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnumU32Type, EnumU32?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnumU32Type.EnumU32),
            value == null ? default(EnumU32?) : (EnumU32)value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, """{"Prop":"Min"}""")]
    [InlineData((ulong)EnumU64.Max, """{"Prop":"Max"}""")]
    [InlineData((ulong)EnumU64.One, """{"Prop":"One"}""")]
    [InlineData((ulong)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_ulong_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_property_value<NullableEnumU64Type, EnumU64?>(
            b => b.HasConversion<string>(),
            nameof(NullableEnumU64Type.EnumU64),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    [ConditionalFact]
    public virtual async Task Can_read_write_point()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointType, Point>(
            nameof(PointType.Point),
            factory.CreatePoint(new Coordinate(2, 4)),
            """{"Prop":"POINT (2 4)"}""");
    }

    public class PointType
    {
        public Point? Point { get; set; }
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_point()
        => Can_read_and_write_JSON_value<NullablePointType, Point?>(
            nameof(NullablePointType.Point),
            null,
            """{"Prop":null}""");

    public class NullablePointType
    {
        public Point? Point { get; set; }
    }

    [ConditionalFact]
    public async virtual Task Can_read_write_point_with_Z()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointZType, Point>(
            nameof(PointZType.PointZ),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            """{"Prop":"POINT Z(2 4 6)"}""");
    }

    public class PointZType
    {
        public Point PointZ { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_point_with_M()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointMType, Point>(
            nameof(PointMType.PointM),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            """{"Prop":"POINT (2 4)"}""");
    }

    public class PointMType
    {
        public Point PointM { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_point_with_Z_and_M()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PointZMType, Point>(
            nameof(PointZMType.PointZM),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            """{"Prop":"POINT Z(1 2 3)"}""");
    }

    public class PointZMType
    {
        public Point PointZM { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<LineStringType, LineString>(
            nameof(LineStringType.LineString),
            factory.CreateLineString([new Coordinate(0, 0), new Coordinate(1, 0)]),
            """{"Prop":"LINESTRING (0 0, 1 0)"}""");
    }

    public class LineStringType
    {
        public LineString? LineString { get; set; }
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_line_string()
        => Can_read_and_write_JSON_value<NullableLineStringType, LineString?>(
            nameof(NullableLineStringType.LineString),
            null,
            """{"Prop":null}""");

    public class NullableLineStringType
    {
        public LineString? LineString { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_multi_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<MultiLineStringType, MultiLineString>(
            nameof(MultiLineStringType.MultiLineString),
            factory.CreateMultiLineString(
            [
                factory.CreateLineString(
                    [new Coordinate(0, 0), new Coordinate(0, 1)]),
                factory.CreateLineString(
                    [new Coordinate(1, 0), new Coordinate(1, 1)])
            ]),
            """{"Prop":"MULTILINESTRING ((0 0, 0 1), (1 0, 1 1))"}""");
    }

    public class MultiLineStringType
    {
        public MultiLineString MultiLineString { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_multi_line_string()
        => Can_read_and_write_JSON_value<NullableMultiLineStringType, MultiLineString?>(
            nameof(NullableMultiLineStringType.MultiLineString),
            null,
            """{"Prop":null}""");

    public class NullableMultiLineStringType
    {
        public MultiLineString? MultiLineString { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_polygon()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<PolygonType, Polygon>(
            nameof(PolygonType.Polygon),
            factory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]),
            """{"Prop":"POLYGON ((0 0, 1 0, 0 1, 0 0))"}""");
    }

    public class PolygonType
    {
        public Polygon Polygon { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_polygon()
        => Can_read_and_write_JSON_value<NullablePolygonType, Polygon?>(
            nameof(NullablePolygonType.Polygon),
            null,
            """{"Prop":null}""");

    public class NullablePolygonType
    {
        public Polygon? Polygon { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_polygon_typed_as_geometry()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_value<GeometryType, Geometry>(
            nameof(GeometryType.Geometry),
            factory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]),
            """{"Prop":"POLYGON ((0 0, 1 0, 0 1, 0 0))"}""");
    }

    public class GeometryType
    {
        public Geometry Geometry { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_polygon_typed_as_nullable_geometry()
        => Can_read_and_write_JSON_value<NullableGeometryType, Geometry?>(
            nameof(NullableGeometryType.Geometry),
            null,
            """{"Prop":null}""");

    public class NullableGeometryType
    {
        public Geometry? Geometry { get; set; }
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_point_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<PointType, Point>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(PointType.Point),
            factory.CreatePoint(new Coordinate(2, 4)),
            """{"Prop":{"type":"Point","coordinates":[2.0,4.0]}}""");
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_point_as_GeoJson()
        => Can_read_and_write_JSON_property_value<NullablePointType, Point?>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(NullablePointType.Point),
            null,
            """{"Prop":null}""");

    [ConditionalFact]
    public virtual async Task Can_read_write_point_with_Z_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<PointZType, Point>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(PointZType.PointZ),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            """{"Prop":{"type":"Point","coordinates":[2.0,4.0]}}""");
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_point_with_M_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<PointMType, Point>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(PointMType.PointM),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            """{"Prop":{"type":"Point","coordinates":[2.0,4.0]}}""");
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_point_with_Z_and_M_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<PointZMType, Point>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(PointZMType.PointZM),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            """{"Prop":{"type":"Point","coordinates":[1.0,2.0]}}""");
    }

    [ConditionalFact]
    public virtual async Task Can_read_write_line_string_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<LineStringType, LineString>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(LineStringType.LineString),
            factory.CreateLineString([new Coordinate(0, 0), new Coordinate(1, 0)]),
            """{"Prop":{"type":"LineString","coordinates":[[0.0,0.0],[1.0,0.0]]}}""");
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_line_string_as_GeoJson()
        => Can_read_and_write_JSON_property_value<NullableLineStringType, LineString?>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(NullableLineStringType.LineString),
            null,
            """{"Prop":null}""");

    [ConditionalFact]
    public virtual async Task Can_read_write_multi_line_string_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<MultiLineStringType, MultiLineString>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(MultiLineStringType.MultiLineString),
            factory.CreateMultiLineString(
            [
                factory.CreateLineString(
                    [new Coordinate(0, 0), new Coordinate(0, 1)]),
                factory.CreateLineString(
                    [new Coordinate(1, 0), new Coordinate(1, 1)])
            ]),
            """{"Prop":{"type":"MultiLineString","coordinates":[[[0.0,0.0],[0.0,1.0]],[[1.0,0.0],[1.0,1.0]]]}}""");
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_multi_line_string_as_GeoJson()
        => Can_read_and_write_JSON_property_value<NullableMultiLineStringType, MultiLineString?>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(NullableMultiLineStringType.MultiLineString),
            null,
            """{"Prop":null}""");

    [ConditionalFact]
    public virtual async Task Can_read_write_polygon_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<PolygonType, Polygon>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(PolygonType.Polygon),
            factory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]),
            """{"Prop":{"type":"Polygon","coordinates":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}}""");
    }

    [ConditionalFact]
    public virtual Task Can_read_write_nullable_polygon_as_GeoJson()
        => Can_read_and_write_JSON_property_value<NullablePolygonType, Polygon?>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(NullablePolygonType.Polygon),
            null,
            """{"Prop":null}""");

    [ConditionalFact]
    public virtual async Task Can_read_write_polygon_typed_as_geometry_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        await Can_read_and_write_JSON_property_value<GeometryType, Geometry>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(GeometryType.Geometry),
            factory.CreatePolygon([new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)]),
            """{"Prop":{"type":"Polygon","coordinates":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}}""");
    }

    [ConditionalFact]
    public virtual Task Can_read_write_polygon_typed_as_nullable_geometry_as_GeoJson()
        => Can_read_and_write_JSON_property_value<NullableGeometryType, Geometry?>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter)),
            nameof(NullableGeometryType.Geometry),
            null,
            """{"Prop":null}""");

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    public virtual Task Can_read_write_converted_type_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value<DddIdType, DddId>(
            b => b.Entity<DddIdType>().HasNoKey().Property(e => e.DddId),
            b => b.Properties<DddId>().HaveConversion<DddIdConverter>(),
            nameof(DddIdType.DddId),
            new DddId { Id = value }, json);

    protected class DddIdType
    {
        public DddId DddId { get; set; }
    }

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual Task Can_read_write_nullable_converted_type_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value<NullableDddIdType, DddId?>(
            b => b.Entity<NullableDddIdType>().HasNoKey().Property(e => e.DddId),
            b => b.Properties<DddId>().HaveConversion<DddIdConverter>(),
            nameof(NullableDddIdType.DddId),
            value == null ? null : new DddId { Id = value.Value }, json);

    protected class NullableDddIdType
    {
        public DddId? DddId { get; set; }
    }

    [ConditionalTheory]
    [InlineData(EnumProperty.FieldA, """{"Prop":"A"}""")]
    [InlineData(EnumProperty.FieldB, """{"Prop":"B"}""")]
    public virtual Task Can_read_write_enum_char_converted_type_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value<EnumCharType, EnumProperty>(
            b => b.Entity<EnumCharType>().HasNoKey().Property(e => e.EnumProperty),
            b => b.Properties<EnumProperty>().HaveConversion<EnumValueConverter<EnumProperty>>(),
            nameof(EnumCharType.EnumProperty),
            (EnumProperty)value,
            json);

    protected class EnumValueConverter<T>() : ValueConverter<T, char>(
        p => p.ToChar(null), p => (T)Enum.Parse(typeof(T), Convert.ToInt32(p).ToString()))
        where T : Enum, IConvertible;

    protected class EnumCharType
    {
        public EnumProperty EnumProperty { get; set; }
    }

    protected enum EnumProperty
    {
        FieldA = 'A',
        FieldB = 'B',
        FieldC = 'C',
    }

    [ConditionalTheory]
    [InlineData("127.0.0.1", """{"Prop":"127.0.0.1"}""")]
    [InlineData("0.0.0.0", """{"Prop":"0.0.0.0"}""")]
    [InlineData("255.255.255.255", """{"Prop":"255.255.255.255"}""")]
    [InlineData("192.168.1.156", """{"Prop":"192.168.1.156"}""")]
    [InlineData("::1", """{"Prop":"::1"}""")]
    [InlineData("::", """{"Prop":"::"}""")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", """{"Prop":"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"}""")]
    public virtual Task Can_read_write_custom_converted_type_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value<IpAddressType, IpAddress>(
            b => b.Entity<IpAddressType>().HasNoKey().Property(e => e.Address),
            b => b.Properties<IpAddress>().HaveConversion<IpAddressConverter>(),
            nameof(IpAddressType.Address),
            new(IPAddress.Parse(value)),
            json);

    protected class IpAddressConverter() : ValueConverter<IpAddress, IPAddress>(
        v => v.Address,
        v => new IpAddress(v));

    protected class IpAddressType
    {
        public IpAddress? Address { get; set; }
    }

    protected class IpAddress(IPAddress address)
    {
        public IPAddress Address { get; } = address;

        protected bool Equals(IpAddress other)
            => Address.Equals(other.Address);

        public override bool Equals(object? obj)
            => !ReferenceEquals(null, obj) && (ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((IpAddress)obj));

        public override int GetHashCode()
            => Address.GetHashCode();
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_sbyte_JSON_values()
        => Can_read_and_write_JSON_value<Int8CollectionType, List<sbyte>>(
            nameof(Int8CollectionType.Int8),
            [
                sbyte.MinValue,
                0,
                sbyte.MaxValue
            ],
            """{"Prop":[-128,0,127]}""",
            mappedCollection: true);

    protected class Int8CollectionType
    {
        public sbyte[] Int8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_short_JSON_values()
        => Can_read_and_write_JSON_value<Int16CollectionType, List<short>>(
            nameof(Int16CollectionType.Int16),
            [
                short.MinValue,
                0,
                short.MaxValue
            ],
            """{"Prop":[-32768,0,32767]}""",
            mappedCollection: true);

    protected class Int16CollectionType
    {
        public IList<short> Int16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_int_JSON_values()
        => Can_read_and_write_JSON_value<Int32CollectionType, List<int>>(
            nameof(Int32CollectionType.Int32),
            [
                int.MinValue,
                0,
                int.MaxValue
            ],
            """{"Prop":[-2147483648,0,2147483647]}""",
            mappedCollection: true);

    protected class Int32CollectionType
    {
        public List<int> Int32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_long_JSON_values()
        => Can_read_and_write_JSON_value<Int64CollectionType, List<long>>(
            nameof(Int64CollectionType.Int64),
            [
                long.MinValue,
                0,
                long.MaxValue
            ],
            """{"Prop":[-9223372036854775808,0,9223372036854775807]}""",
            mappedCollection: true);

    protected class Int64CollectionType
    {
        public IList<long> Int64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_byte_JSON_values()
        => Can_read_and_write_JSON_value<UInt8CollectionType, List<byte>>(
            nameof(UInt8CollectionType.UInt8),
            [
                byte.MinValue,
                1,
                byte.MaxValue
            ],
            """{"Prop":[0,1,255]}""",
            mappedCollection: true);

    protected class UInt8CollectionType
    {
        public List<byte> UInt8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_ushort_JSON_values()
        => Can_read_and_write_JSON_value<UInt16CollectionType, List<ushort>>(
            nameof(UInt16CollectionType.UInt16),
            [
                ushort.MinValue,
                1,
                ushort.MaxValue
            ],
            """{"Prop":[0,1,65535]}""",
            mappedCollection: true);

    protected class UInt16CollectionType
    {
        public Collection<ushort> UInt16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_uint_JSON_values()
        => Can_read_and_write_JSON_value<UInt32CollectionType, List<uint>>(
            nameof(UInt32CollectionType.UInt32),
            [
                uint.MinValue,
                1,
                uint.MaxValue
            ],
            """{"Prop":[0,1,4294967295]}""",
            mappedCollection: true);

    protected class UInt32CollectionType
    {
        public List<uint> UInt32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_ulong_JSON_values()
        => Can_read_and_write_JSON_value<UInt64CollectionType, List<ulong>>(
            nameof(UInt64CollectionType.UInt64),
            [
                ulong.MinValue,
                1,
                ulong.MaxValue
            ],
            """{"Prop":[0,1,18446744073709551615]}""",
            mappedCollection: true,
            new ObservableCollection<ulong>());

    protected class UInt64CollectionType
    {
        public ObservableCollection<ulong> UInt64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_float_JSON_values()
        => Can_read_and_write_JSON_value<FloatCollectionType, List<float>>(
            nameof(FloatCollectionType.Float),
            [
                float.MinValue,
                0,
                float.MaxValue
            ],
            """{"Prop":[-3.4028235E+38,0,3.4028235E+38]}""",
            mappedCollection: true);

    protected class FloatCollectionType
    {
        public float[] Float { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_double_JSON_values()
        => Can_read_and_write_JSON_value<DoubleCollectionType, List<double>>(
            nameof(DoubleCollectionType.Double),
            [
                double.MinValue,
                0,
                double.MaxValue
            ],
            """{"Prop":[-1.7976931348623157E+308,0,1.7976931348623157E+308]}""",
            mappedCollection: true);

    protected class DoubleCollectionType
    {
        public double[] Double { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_decimal_JSON_values()
        => Can_read_and_write_JSON_value<DecimalCollectionType, List<decimal>>(
            nameof(DecimalCollectionType.Decimal),
            [
                decimal.MinValue,
                0,
                decimal.MaxValue
            ],
            """{"Prop":[-79228162514264337593543950335,0,79228162514264337593543950335]}""",
            mappedCollection: true);

    protected class DecimalCollectionType
    {
        public decimal[] Decimal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value<DateOnlyCollectionType, List<DateOnly>>(
            nameof(DateOnlyCollectionType.DateOnly),
            [
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue
            ],
            """{"Prop":["0001-01-01","2023-05-29","9999-12-31"]}""",
            mappedCollection: true);

    protected class DateOnlyCollectionType
    {
        public IList<DateOnly> DateOnly { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_TimeOnly_JSON_values()
        => Can_read_and_write_JSON_value<TimeOnlyCollectionType, List<TimeOnly>>(
            nameof(TimeOnlyCollectionType.TimeOnly),
            [
                TimeOnly.MinValue,
                new(11, 5, 2, 3, 4),
                TimeOnly.MaxValue
            ],
            """{"Prop":["00:00:00.0000000","11:05:02.0030040","23:59:59.9999999"]}""",
            mappedCollection: true,
            new List<TimeOnly>());

    protected class TimeOnlyCollectionType
    {
        public IList<TimeOnly> TimeOnly { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateTime_JSON_values()
        => Can_read_and_write_JSON_value<DateTimeCollectionType, List<DateTime>>(
            nameof(DateTimeCollectionType.DateTime),
            [
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            ],
            """{"Prop":["0001-01-01T00:00:00","2023-05-29T10:52:47","9999-12-31T23:59:59.9999999"]}""",
            mappedCollection: true);

    protected class DateTimeCollectionType
    {
        public IList<DateTime> DateTime { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value<DateTimeOffsetCollectionType, List<DateTimeOffset>>(
            nameof(DateTimeOffsetCollectionType.DateTimeOffset),
            [
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            ],
            """{"Prop":["0001-01-01T00:00:00+00:00","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47+00:00","2023-05-29T10:52:47+02:00","9999-12-31T23:59:59.9999999+00:00"]}""",
            mappedCollection: true);

    protected class DateTimeOffsetCollectionType
    {
        public IList<DateTimeOffset> DateTimeOffset { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_TimeSpan_JSON_values()
        => Can_read_and_write_JSON_value<TimeSpanCollectionType, List<TimeSpan>>(
            nameof(TimeSpanCollectionType.TimeSpan),
            [
                TimeSpan.MinValue,
                new(1, 2, 3, 4, 5),
                TimeSpan.MaxValue
            ],
            """{"Prop":["-10675199:2:48:05.4775808","1:2:03:04.005","10675199:2:48:05.4775807"]}""",
            mappedCollection: true);

    protected class TimeSpanCollectionType
    {
        public IList<TimeSpan> TimeSpan { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_bool_JSON_values()
        => Can_read_and_write_JSON_value<BooleanCollectionType, List<bool>>(
            nameof(BooleanCollectionType.Boolean),
            [false, true],
            """{"Prop":[false,true]}""",
            mappedCollection: true);

    protected class BooleanCollectionType
    {
        public IList<bool> Boolean { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_char_JSON_values()
        => Can_read_and_write_JSON_value<CharacterCollectionType, List<char>>(
            nameof(CharacterCollectionType.Character),
            [
                char.MinValue,
                'X',
                char.MaxValue
            ],
            """{"Prop":["\u0000","X","\uFFFF"]}""",
            mappedCollection: true);

    protected class CharacterCollectionType
    {
        public IList<char> Character { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_GUID_JSON_values()
        => Can_read_and_write_JSON_value<GuidCollectionType, List<Guid>>(
            nameof(GuidCollectionType.Guid),
            [
                new(),
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            ],
            """{"Prop":["00000000-0000-0000-0000-000000000000","8c44242f-8e3f-4a20-8be8-98c7c1aadebd","ffffffff-ffff-ffff-ffff-ffffffffffff"]}""",
            mappedCollection: true);

    protected class GuidCollectionType
    {
        public IList<Guid> Guid { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_string_JSON_values()
        => Can_read_and_write_JSON_value<StringCollectionType, List<string>>(
            nameof(StringCollectionType.String),
            [
                "MinValue",
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            ],
            """{"Prop":["MinValue","\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A","MaxValue"]}""",
            mappedCollection: true);

    protected class StringCollectionType
    {
        public IList<string> String { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_binary_JSON_values()
        => Can_read_and_write_JSON_value<BytesCollectionType, List<byte[]>>(
            nameof(BytesCollectionType.Bytes),
            [
                [0, 0, 0, 1],
                [255, 255, 255, 255],
                [],
                [1, 2, 3, 4]
            ],
            """{"Prop":["AAAAAQ==","/////w==","","AQIDBA=="]}""",
            mappedCollection: true);

    protected class BytesCollectionType
    {
        public IList<byte[]> Bytes { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_URI_JSON_values()
        => Can_read_and_write_JSON_value<UriCollectionType, List<Uri>>(
            nameof(UriCollectionType.Uri),
            [
                new("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName"),
                new("file:///C:/test/path/file.txt")
            ],
            """{"Prop":["https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName","file:///C:/test/path/file.txt"]}""",
            mappedCollection: true);

    protected class UriCollectionType
    {
        public List<Uri> Uri { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_IP_address_JSON_values()
        => Can_read_and_write_JSON_value<IpAddressCollectionType, List<IPAddress>>(
            nameof(IpAddressCollectionType.IpAddress),
            [
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("0.0.0.0"),
                IPAddress.Parse("255.255.255.255"),
                IPAddress.Parse("192.168.1.156"),
                IPAddress.Parse("::1"),
                IPAddress.Parse("::"),
                IPAddress.Parse("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577")
            ],
            """{"Prop":["127.0.0.1","0.0.0.0","255.255.255.255","192.168.1.156","::1","::","2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"]}""",
            mappedCollection: true);

    protected class IpAddressCollectionType
    {
        public List<IPAddress> IpAddress { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_physical_address_JSON_values()
        => Can_read_and_write_JSON_value<PhysicalAddressCollectionType, List<PhysicalAddress>>(
            nameof(PhysicalAddressCollectionType.PhysicalAddress),
            [
                PhysicalAddress.None,
                PhysicalAddress.Parse("001122334455"),
                PhysicalAddress.Parse("00-11-22-33-44-55"),
                PhysicalAddress.Parse("0011.2233.4455")
            ],
            """{"Prop":["","001122334455","001122334455","001122334455"]}""",
            mappedCollection: true);

    protected class PhysicalAddressCollectionType
    {
        public List<PhysicalAddress> PhysicalAddress { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_sbyte_enum_JSON_values()
        => Can_read_and_write_JSON_value<Enum8CollectionType, List<Enum8>>(
            nameof(Enum8CollectionType.Enum8),
            [
                Enum8.Min,
                Enum8.Max,
                Enum8.Default,
                Enum8.One,
                (Enum8)(-8)
            ],
            """{"Prop":[-128,127,0,1,-8]}""",
            mappedCollection: true);

    protected class Enum8CollectionType
    {
        public List<Enum8> Enum8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_short_enum_JSON_values()
        => Can_read_and_write_JSON_value<Enum16CollectionType, List<Enum16>>(
            nameof(Enum16CollectionType.Enum16),
            [
                Enum16.Min,
                Enum16.Max,
                Enum16.Default,
                Enum16.One,
                (Enum16)(-8)
            ],
            """{"Prop":[-32768,32767,0,1,-8]}""",
            mappedCollection: true);

    protected class Enum16CollectionType
    {
        public List<Enum16> Enum16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_int_enum_JSON_values()
        => Can_read_and_write_JSON_value<Enum32CollectionType, List<Enum32>>(
            nameof(Enum32CollectionType.Enum32),
            [
                Enum32.Min,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            ],
            """{"Prop":[-2147483648,2147483647,0,1,-8]}""",
            mappedCollection: true);

    protected class Enum32CollectionType
    {
        public List<Enum32> Enum32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_long_enum_JSON_values()
        => Can_read_and_write_JSON_value<Enum64CollectionType, List<Enum64>>(
            nameof(Enum64CollectionType.Enum64),
            [
                Enum64.Min,
                Enum64.Max,
                Enum64.Default,
                Enum64.One,
                (Enum64)(-8)
            ],
            """{"Prop":[-9223372036854775808,9223372036854775807,0,1,-8]}""",
            mappedCollection: true);

    protected class Enum64CollectionType
    {
        public List<Enum64> Enum64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_byte_enum_JSON_values()
        => Can_read_and_write_JSON_value<EnumU8CollectionType, List<EnumU8>>(
            nameof(EnumU8CollectionType.EnumU8),
            [
                EnumU8.Min,
                EnumU8.Max,
                EnumU8.Default,
                EnumU8.One,
                (EnumU8)8
            ],
            """{"Prop":[0,255,0,1,8]}""",
            mappedCollection: true);

    protected class EnumU8CollectionType
    {
        public IList<EnumU8> EnumU8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_ushort_enum_JSON_values()
        => Can_read_and_write_JSON_value<EnumU16CollectionType, List<EnumU16>>(
            nameof(EnumU16CollectionType.EnumU16),
            [
                EnumU16.Min,
                EnumU16.Max,
                EnumU16.Default,
                EnumU16.One,
                (EnumU16)8
            ],
            """{"Prop":[0,65535,0,1,8]}""",
            mappedCollection: true);

    protected class EnumU16CollectionType
    {
        public IList<EnumU16> EnumU16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_uint_enum_JSON_values()
        => Can_read_and_write_JSON_value<EnumU32CollectionType, List<EnumU32>>(
            nameof(EnumU32CollectionType.EnumU32),
            [
                EnumU32.Min,
                EnumU32.Max,
                EnumU32.Default,
                EnumU32.One,
                (EnumU32)8
            ],
            """{"Prop":[0,4294967295,0,1,8]}""",
            mappedCollection: true,
            new ObservableCollection<EnumU32>());

    protected class EnumU32CollectionType
    {
        public IList<EnumU32> EnumU32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<EnumU64CollectionType, List<EnumU64>>(
            nameof(EnumU64CollectionType.EnumU64),
            [
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            ],
            """{"Prop":[0,18446744073709551615,0,1,8]}""",
            mappedCollection: true);

    protected class EnumU64CollectionType
    {
        public IList<EnumU64> EnumU64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_sbyte_JSON_values()
        => Can_read_and_write_JSON_value<NullableInt8CollectionType, List<sbyte?>>(
            nameof(NullableInt8CollectionType.Int8),
            [
                null,
                sbyte.MinValue,
                0,
                sbyte.MaxValue
            ],
            """{"Prop":[null,-128,0,127]}""",
            mappedCollection: true);

    protected class NullableInt8CollectionType
    {
        public sbyte?[] Int8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_short_JSON_values()
        => Can_read_and_write_JSON_value<NullableInt16CollectionType, List<short?>>(
            nameof(NullableInt16CollectionType.Int16),
            [
                short.MinValue,
                null,
                0,
                short.MaxValue
            ],
            """{"Prop":[-32768,null,0,32767]}""",
            mappedCollection: true);

    protected class NullableInt16CollectionType
    {
        public IList<short?> Int16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_int_JSON_values()
        => Can_read_and_write_JSON_value<NullableInt32CollectionType, List<int?>>(
            nameof(NullableInt32CollectionType.Int32),
            [
                int.MinValue,
                0,
                null,
                int.MaxValue
            ],
            """{"Prop":[-2147483648,0,null,2147483647]}""",
            mappedCollection: true);

    protected class NullableInt32CollectionType
    {
        public List<int?> Int32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_long_JSON_values()
        => Can_read_and_write_JSON_value<NullableInt64CollectionType, List<long?>>(
            nameof(NullableInt64CollectionType.Int64),
            [
                long.MinValue,
                0,
                long.MaxValue,
                null
            ],
            """{"Prop":[-9223372036854775808,0,9223372036854775807,null]}""",
            mappedCollection: true);

    protected class NullableInt64CollectionType
    {
        public IList<long?> Int64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_byte_JSON_values()
        => Can_read_and_write_JSON_value<NullableUInt8CollectionType, List<byte?>>(
            nameof(NullableUInt8CollectionType.UInt8),
            [
                null,
                byte.MinValue,
                1,
                byte.MaxValue
            ],
            """{"Prop":[null,0,1,255]}""",
            mappedCollection: true);

    protected class NullableUInt8CollectionType
    {
        public List<byte?> UInt8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_ushort_JSON_values()
        => Can_read_and_write_JSON_value<NullableUInt16CollectionType, List<ushort?>>(
            nameof(NullableUInt16CollectionType.UInt16),
            [
                ushort.MinValue,
                null,
                1,
                ushort.MaxValue
            ],
            """{"Prop":[0,null,1,65535]}""",
            mappedCollection: true);

    protected class NullableUInt16CollectionType
    {
        public Collection<ushort?> UInt16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_uint_JSON_values()
        => Can_read_and_write_JSON_value<NullableUInt32CollectionType, List<uint?>>(
            nameof(NullableUInt32CollectionType.UInt32),
            [
                uint.MinValue,
                1,
                null,
                uint.MaxValue
            ],
            """{"Prop":[0,1,null,4294967295]}""",
            mappedCollection: true);

    protected class NullableUInt32CollectionType
    {
        public List<uint?> UInt32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_ulong_JSON_values()
        => Can_read_and_write_JSON_value<NullableUInt64CollectionType, List<ulong?>>(
            nameof(NullableUInt64CollectionType.UInt64),
            [
                ulong.MinValue,
                1,
                ulong.MaxValue,
                null
            ],
            """{"Prop":[0,1,18446744073709551615,null]}""",
            mappedCollection: true);

    protected class NullableUInt64CollectionType
    {
        public ObservableCollection<ulong?> UInt64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_float_JSON_values()
        => Can_read_and_write_JSON_value<NullableFloatCollectionType, List<float?>>(
            nameof(NullableFloatCollectionType.Float),
            [
                null,
                float.MinValue,
                0,
                float.MaxValue
            ],
            """{"Prop":[null,-3.4028235E+38,0,3.4028235E+38]}""",
            mappedCollection: true);

    protected class NullableFloatCollectionType
    {
        public float?[] Float { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_double_JSON_values()
        => Can_read_and_write_JSON_value<NullableDoubleCollectionType, List<double?>>(
            nameof(NullableDoubleCollectionType.Double),
            [
                double.MinValue,
                null,
                0,
                double.MaxValue
            ],
            """{"Prop":[-1.7976931348623157E+308,null,0,1.7976931348623157E+308]}""",
            mappedCollection: true);

    protected class NullableDoubleCollectionType
    {
        public double?[] Double { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_decimal_JSON_values()
        => Can_read_and_write_JSON_value<NullableDecimalCollectionType, List<decimal?>>(
            nameof(NullableDecimalCollectionType.Decimal),
            [
                decimal.MinValue,
                0,
                null,
                decimal.MaxValue
            ],
            """{"Prop":[-79228162514264337593543950335,0,null,79228162514264337593543950335]}""",
            mappedCollection: true);

    protected class NullableDecimalCollectionType
    {
        public decimal?[] Decimal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateOnlyCollectionType, List<DateOnly?>>(
            nameof(NullableDateOnlyCollectionType.DateOnly),
            [
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue,
                null
            ],
            """{"Prop":["0001-01-01","2023-05-29","9999-12-31",null]}""",
            mappedCollection: true);

    protected class NullableDateOnlyCollectionType
    {
        public IList<DateOnly?> DateOnly { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_TimeOnly_JSON_values()
        => Can_read_and_write_JSON_value<NullableTimeOnlyCollectionType, List<TimeOnly?>>(
            nameof(NullableTimeOnlyCollectionType.TimeOnly),
            [
                null,
                TimeOnly.MinValue,
                new(11, 5, 2, 3, 4),
                TimeOnly.MaxValue
            ],
            """{"Prop":[null,"00:00:00.0000000","11:05:02.0030040","23:59:59.9999999"]}""",
            mappedCollection: true);

    protected class NullableTimeOnlyCollectionType
    {
        public IList<TimeOnly?> TimeOnly { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_DateTime_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateTimeCollectionType, List<DateTime?>>(
            nameof(NullableDateTimeCollectionType.DateTime),
            [
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            ],
            """{"Prop":["0001-01-01T00:00:00",null,"2023-05-29T10:52:47","9999-12-31T23:59:59.9999999"]}""",
            mappedCollection: true);

    protected class NullableDateTimeCollectionType
    {
        public IList<DateTime?> DateTime { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetCollectionType, List<DateTimeOffset?>>(
            nameof(NullableDateTimeOffsetCollectionType.DateTimeOffset),
            [
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                null,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            ],
            """{"Prop":["0001-01-01T00:00:00+00:00","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47+00:00",null,"2023-05-29T10:52:47+02:00","9999-12-31T23:59:59.9999999+00:00"]}""",
            mappedCollection: true);

    protected class NullableDateTimeOffsetCollectionType
    {
        public IList<DateTimeOffset?> DateTimeOffset { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_TimeSpan_JSON_values()
        => Can_read_and_write_JSON_value<NullableTimeSpanCollectionType, List<TimeSpan?>>(
            nameof(NullableTimeSpanCollectionType.TimeSpan),
            [
                TimeSpan.MinValue,
                new(1, 2, 3, 4, 5),
                TimeSpan.MaxValue,
                null
            ],
            """{"Prop":["-10675199:2:48:05.4775808","1:2:03:04.005","10675199:2:48:05.4775807",null]}""",
            mappedCollection: true);

    protected class NullableTimeSpanCollectionType
    {
        public IList<TimeSpan?> TimeSpan { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_bool_JSON_values()
        => Can_read_and_write_JSON_value<NullableBooleanCollectionType, List<bool?>>(
            nameof(NullableBooleanCollectionType.Boolean),
            [
                false,
                null,
                true
            ],
            """{"Prop":[false,null,true]}""",
            mappedCollection: true);

    protected class NullableBooleanCollectionType
    {
        public IList<bool?> Boolean { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_char_JSON_values()
        => Can_read_and_write_JSON_value<NullableCharacterCollectionType, List<char?>>(
            nameof(NullableCharacterCollectionType.Character),
            [
                char.MinValue,
                'X',
                char.MaxValue,
                null
            ],
            """{"Prop":["\u0000","X","\uFFFF",null]}""",
            mappedCollection: true);

    protected class NullableCharacterCollectionType
    {
        public IList<char?> Character { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_GUID_JSON_values()
        => Can_read_and_write_JSON_value<NullableGuidCollectionType, List<Guid?>>(
            nameof(NullableGuidCollectionType.Guid),
            [
                new(),
                null,
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            ],
            """{"Prop":["00000000-0000-0000-0000-000000000000",null,"8c44242f-8e3f-4a20-8be8-98c7c1aadebd","ffffffff-ffff-ffff-ffff-ffffffffffff"]}""",
            mappedCollection: true);

    protected class NullableGuidCollectionType
    {
        public IList<Guid?> Guid { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_string_JSON_values()
        => Can_read_and_write_JSON_value<NullableStringCollectionType, List<string?>>(
            nameof(NullableStringCollectionType.String),
            [
                "MinValue",
                null,
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            ],
            """{"Prop":["MinValue",null,"\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A","MaxValue"]}""",
            mappedCollection: true);

    protected class NullableStringCollectionType
    {
        public IList<string> String { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_binary_JSON_values()
        => Can_read_and_write_JSON_value<NullableBytesCollectionType, List<byte[]?>>(
            nameof(NullableBytesCollectionType.Bytes),
            [
                [0, 0, 0, 1],
                null,
                [255, 255, 255, 255],
                [],
                [1, 2, 3, 4]
            ],
            """{"Prop":["AAAAAQ==",null,"/////w==","","AQIDBA=="]}""",
            mappedCollection: true);

    protected class NullableBytesCollectionType
    {
        public IList<byte[]?> Bytes { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_URI_JSON_values()
        => Can_read_and_write_JSON_value<NullableUriCollectionType, List<Uri?>>(
            nameof(NullableUriCollectionType.Uri),
            [
                new("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName"),
                null,
                new("file:///C:/test/path/file.txt")
            ],
            """{"Prop":["https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName",null,"file:///C:/test/path/file.txt"]}""",
            mappedCollection: true);

    protected class NullableUriCollectionType
    {
        public List<Uri?> Uri { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_IP_address_JSON_values()
        => Can_read_and_write_JSON_value<NullableIpAddressCollectionType, List<IPAddress?>>(
            nameof(NullableIpAddressCollectionType.IpAddress),
            [
                IPAddress.Parse("127.0.0.1"),
                null,
                IPAddress.Parse("0.0.0.0"),
                IPAddress.Parse("255.255.255.255"),
                IPAddress.Parse("192.168.1.156"),
                IPAddress.Parse("::1"),
                IPAddress.Parse("::"),
                IPAddress.Parse("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577")
            ],
            """{"Prop":["127.0.0.1",null,"0.0.0.0","255.255.255.255","192.168.1.156","::1","::","2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"]}""",
            mappedCollection: true);

    protected class NullableIpAddressCollectionType
    {
        public List<IPAddress?> IpAddress { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_physical_address_JSON_values()
        => Can_read_and_write_JSON_value<NullablePhysicalAddressCollectionType, List<PhysicalAddress?>>(
            nameof(NullablePhysicalAddressCollectionType.PhysicalAddress),
            [
                PhysicalAddress.None,
                null,
                PhysicalAddress.Parse("001122334455"),
                PhysicalAddress.Parse("00-11-22-33-44-55"),
                PhysicalAddress.Parse("0011.2233.4455")
            ],
            """{"Prop":["",null,"001122334455","001122334455","001122334455"]}""",
            mappedCollection: true);

    protected class NullablePhysicalAddressCollectionType
    {
        public List<PhysicalAddress?> PhysicalAddress { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_sbyte_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnum8CollectionType, List<Enum8?>>(
            nameof(NullableEnum8CollectionType.Enum8),
            [
                Enum8.Min,
                null,
                Enum8.Max,
                Enum8.Default,
                Enum8.One,
                (Enum8)(-8)
            ],
            """{"Prop":[-128,null,127,0,1,-8]}""",
            mappedCollection: true);

    protected class NullableEnum8CollectionType
    {
        public List<Enum8?> Enum8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_short_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnum16CollectionType, List<Enum16?>>(
            nameof(NullableEnum16CollectionType.Enum16),
            [
                Enum16.Min,
                null,
                Enum16.Max,
                Enum16.Default,
                Enum16.One,
                (Enum16)(-8)
            ],
            """{"Prop":[-32768,null,32767,0,1,-8]}""",
            mappedCollection: true);

    protected class NullableEnum16CollectionType
    {
        public List<Enum16?> Enum16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_int_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnum32CollectionType, List<Enum32?>>(
            nameof(NullableEnum32CollectionType.Enum32),
            [
                Enum32.Min,
                null,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            ],
            """{"Prop":[-2147483648,null,2147483647,0,1,-8]}""",
            mappedCollection: true);

    protected class NullableEnum32CollectionType
    {
        public List<Enum32?> Enum32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_long_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnum64CollectionType, List<Enum64?>>(
            nameof(NullableEnum64CollectionType.Enum64),
            [
                Enum64.Min,
                null,
                Enum64.Max,
                Enum64.Default,
                Enum64.One,
                (Enum64)(-8)
            ],
            """{"Prop":[-9223372036854775808,null,9223372036854775807,0,1,-8]}""",
            mappedCollection: true);

    protected class NullableEnum64CollectionType
    {
        public List<Enum64?> Enum64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_byte_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU8CollectionType, List<EnumU8?>>(
            nameof(NullableEnumU8CollectionType.EnumU8),
            [
                EnumU8.Min,
                null,
                EnumU8.Max,
                EnumU8.Default,
                EnumU8.One,
                (EnumU8?)8
            ],
            """{"Prop":[0,null,255,0,1,8]}""",
            mappedCollection: true);

    protected class NullableEnumU8CollectionType
    {
        public IList<EnumU8?> EnumU8 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_ushort_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU16CollectionType, List<EnumU16?>>(
            nameof(NullableEnumU16CollectionType.EnumU16),
            [
                EnumU16.Min,
                null,
                EnumU16.Max,
                EnumU16.Default,
                EnumU16.One,
                (EnumU16?)8
            ],
            """{"Prop":[0,null,65535,0,1,8]}""",
            mappedCollection: true);

    protected class NullableEnumU16CollectionType
    {
        public IList<EnumU16?> EnumU16 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_uint_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU32CollectionType, List<EnumU32?>>(
            nameof(NullableEnumU32CollectionType.EnumU32),
            [
                EnumU32.Min,
                null,
                EnumU32.Max,
                EnumU32.Default,
                EnumU32.One,
                (EnumU32?)8
            ],
            """{"Prop":[0,null,4294967295,0,1,8]}""",
            mappedCollection: true);

    protected class NullableEnumU32CollectionType
    {
        public IList<EnumU32?> EnumU32 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value<NullableEnumU64CollectionType, List<EnumU64?>>(
            nameof(NullableEnumU64CollectionType.EnumU64),
            [
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64?)8
            ],
            """{"Prop":[0,null,18446744073709551615,0,1,8]}""",
            mappedCollection: true);

    protected class NullableEnumU64CollectionType
    {
        public IList<EnumU64?> EnumU64 { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_sbyte_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<Int8ConvertedType, sbyte[]>(
            b => b.HasConversion<CustomCollectionConverter<sbyte[], sbyte>, CustomCollectionComparer<sbyte[], sbyte>>(),
            nameof(Int8ConvertedType.Int8Converted),
            [sbyte.MinValue, 0, sbyte.MaxValue],
            """{"Prop":"[-128,0,127]"}""");

    protected class Int8ConvertedType
    {
        public sbyte[] Int8Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_int_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<Int32ConvertedType, List<int>>(
            b => b.HasConversion<CustomCollectionConverter<List<int>, int>, CustomCollectionComparer<List<int>, int>>(),
            nameof(Int32ConvertedType.Int32Converted),
            [
                int.MinValue,
                0,
                int.MaxValue
            ],
            """{"Prop":"[-2147483648,0,2147483647]"}""");

    protected class Int32ConvertedType
    {
        public List<int> Int32Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_ulong_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<UInt64ConvertedType, ObservableCollection<ulong>>(
            b => b.HasConversion<CustomCollectionConverter<ObservableCollection<ulong>, ulong>,
                CustomCollectionComparer<ObservableCollection<ulong>, ulong>>(),
            nameof(UInt64ConvertedType.UInt64Converted),
            [
                ulong.MinValue,
                1,
                ulong.MaxValue
            ],
            """{"Prop":"[0,1,18446744073709551615]"}""");

    protected class UInt64ConvertedType
    {
        public ObservableCollection<ulong> UInt64Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_double_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<DoubleConvertedType, double[]>(
            b => b.HasConversion<CustomCollectionConverter<double[], double>, CustomCollectionComparer<double[], double>>(),
            nameof(DoubleConvertedType.DoubleConverted),
            [double.MinValue, 0, double.MaxValue],
            """{"Prop":"[-1.7976931348623157E\u002B308,0,1.7976931348623157E\u002B308]"}""");

    protected class DoubleConvertedType
    {
        public double[] DoubleConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateOnly_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<DateOnlyConvertedType, IList<DateOnly>>(
            b => b.HasConversion<CustomCollectionConverter<IList<DateOnly>, DateOnly>,
                CustomCollectionComparer<IList<DateOnly>, DateOnly>>(),
            nameof(DateOnlyConvertedType.DateOnlyConverted),
            new List<DateOnly>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue
            },
            """{"Prop":"[\u00220001-01-01\u0022,\u00222023-05-29\u0022,\u00229999-12-31\u0022]"}""");

    protected class DateOnlyConvertedType
    {
        public IList<DateOnly> DateOnlyConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_DateTime_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<DateTimeConvertedType, IList<DateTime>>(
            b => b
                .HasConversion<CustomCollectionConverter<IList<DateTime>, DateTime>, CustomCollectionComparer<IList<DateTime>, DateTime>>(),
            nameof(DateTimeConvertedType.DateTimeConverted),
            new List<DateTime>
            {
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":"[\u00220001-01-01T00:00:00\u0022,\u00222023-05-29T10:52:47\u0022,\u00229999-12-31T23:59:59.9999999\u0022]"}""");

    protected class DateTimeConvertedType
    {
        public IList<DateTime> DateTimeConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_bool_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<BooleanConvertedType, IList<bool>>(
            b => b.HasConversion<CustomCollectionConverter<IList<bool>, bool>, CustomCollectionComparer<IList<bool>, bool>>(),
            nameof(BooleanConvertedType.BooleanConverted),
            new List<bool> { false, true },
            """{"Prop":"[false,true]"}""");

    protected class BooleanConvertedType
    {
        public IList<bool> BooleanConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_char_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<CharacterConvertedType, IList<char>>(
            b => b.HasConversion<CustomCollectionConverter<IList<char>, char>, CustomCollectionComparer<IList<char>, char>>(),
            nameof(CharacterConvertedType.CharacterConverted),
            new List<char>
            {
                char.MinValue,
                'X',
                char.MaxValue
            },
            """{"Prop":"[\u0022\\u0000\u0022,\u0022X\u0022,\u0022\\uFFFF\u0022]"}""");

    protected class CharacterConvertedType
    {
        public IList<char> CharacterConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_string_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<StringConvertedType, IList<string>>(
            b => b.HasConversion<CustomCollectionConverter<IList<string>, string>, CustomCollectionComparer<IList<string>, string>>(),
            nameof(StringConvertedType.StringConverted),
            new List<string>
            {
                "MinValue",
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            },
            """{"Prop":"[\u0022MinValue\u0022,\u0022\\u2764\\u2765\\uC6C3\\uC720\\u264B\\u262E\\u270C\\u260F\\u2622\\u2620\\u2714\\u2611\\u265A\\u25B2\\u266A\\u0E3F\\u0189\\u26CF\\u2665\\u2763\\u2642\\u2640\\u263F\\uD83D\\uDC4D\\u270D\\u2709\\u2623\\u2624\\u2718\\u2612\\u265B\\u25BC\\u266B\\u2318\\u231B\\u00A1\\u2661\\u10E6\\u30C4\\u263C\\u2601\\u2745\\u267E\\uFE0F\\u270E\\u00A9\\u00AE\\u2122\\u03A3\\u272A\\u272F\\u262D\\u27B3\\u24B6\\u271E\\u2103\\u2109\\u00B0\\u273F\\u26A1\\u2603\\u2602\\u2704\\u00A2\\u20AC\\u00A3\\u221E\\u272B\\u2605\\u00BD\\u262F\\u2721\\u262A\u0022,\u0022MaxValue\u0022]"}""");

    protected class StringConvertedType
    {
        public IList<string> StringConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_binary_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<BytesConvertedType, IList<byte[]>>(
            b => b.HasConversion<CustomCollectionConverter<IList<byte[]>, byte[]>, CustomCollectionComparer<IList<byte[]>, byte[]>>(),
            nameof(BytesConvertedType.BytesConverted),
            new List<byte[]>
            {
                new byte[] { 0, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":"[\u0022AAAAAQ==\u0022,\u0022/////w==\u0022,\u0022\u0022,\u0022AQIDBA==\u0022]"}""");

    protected class BytesConvertedType
    {
        public IList<byte[]> BytesConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_int_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<Enum32ConvertedType, List<Enum32>>(
            b => b.HasConversion<CustomCollectionConverter<List<Enum32>, Enum32>, CustomCollectionComparer<List<Enum32>, Enum32>>(),
            nameof(Enum32ConvertedType.Enum32Converted),
            [
                Enum32.Min,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            ],
            """{"Prop":"[-2147483648,2147483647,0,1,-8]"}""");

    protected class Enum32ConvertedType
    {
        public List<Enum32> Enum32Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_ulong_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<EnumU64ConvertedType, IList<EnumU64>>(
            b => b.HasConversion<CustomCollectionConverter<IList<EnumU64>, EnumU64>, CustomCollectionComparer<IList<EnumU64>, EnumU64>>(),
            nameof(EnumU64ConvertedType.EnumU64Converted),
            new List<EnumU64>
            {
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":"[0,18446744073709551615,0,1,8]"}""");

    protected class EnumU64ConvertedType
    {
        public IList<EnumU64> EnumU64Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_sbyte_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableInt8ConvertedType, sbyte?[]>(
            b => b.HasConversion<CustomCollectionConverter<sbyte?[], sbyte?>, CustomCollectionComparer<sbyte?[], sbyte?>>(),
            nameof(NullableInt8ConvertedType.Int8Converted),
            [null, sbyte.MinValue, 0, sbyte.MaxValue],
            """{"Prop":"[null,-128,0,127]"}""");

    protected class NullableInt8ConvertedType
    {
        public sbyte?[] Int8Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_int_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableInt32ConvertedType, List<int?>>(
            b => b.HasConversion<CustomCollectionConverter<List<int?>, int?>, CustomCollectionComparer<List<int?>, int?>>(),
            nameof(NullableInt32ConvertedType.Int32Converted),
            [
                int.MinValue,
                0,
                null,
                int.MaxValue
            ],
            """{"Prop":"[-2147483648,0,null,2147483647]"}""");

    protected class NullableInt32ConvertedType
    {
        public List<int?> Int32Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_ulong_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableUInt64ConvertedType, ObservableCollection<ulong?>>(
            b => b.HasConversion<CustomCollectionConverter<ObservableCollection<ulong?>, ulong?>,
                CustomCollectionComparer<ObservableCollection<ulong?>, ulong?>>(),
            nameof(NullableUInt64ConvertedType.UInt64Converted),
            [
                ulong.MinValue,
                1,
                ulong.MaxValue,
                null
            ],
            """{"Prop":"[0,1,18446744073709551615,null]"}""");

    protected class NullableUInt64ConvertedType
    {
        public ObservableCollection<ulong?> UInt64Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_double_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableDoubleConvertedType, double?[]>(
            b => b.HasConversion<CustomCollectionConverter<double?[], double?>, CustomCollectionComparer<double?[], double?>>(),
            nameof(NullableDoubleConvertedType.DoubleConverted),
            [double.MinValue, null, 0, double.MaxValue],
            """{"Prop":"[-1.7976931348623157E\u002B308,null,0,1.7976931348623157E\u002B308]"}""");

    protected class NullableDoubleConvertedType
    {
        public double?[] DoubleConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_DateOnly_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableDateOnlyConvertedType, IList<DateOnly?>>(
            b => b.HasConversion<CustomCollectionConverter<IList<DateOnly?>, DateOnly?>,
                CustomCollectionComparer<IList<DateOnly?>, DateOnly?>>(),
            nameof(NullableDateOnlyConvertedType.DateOnlyConverted),
            new List<DateOnly?>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue,
                null
            },
            """{"Prop":"[\u00220001-01-01\u0022,\u00222023-05-29\u0022,\u00229999-12-31\u0022,null]"}""");

    protected class NullableDateOnlyConvertedType
    {
        public IList<DateOnly?> DateOnlyConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_DateTime_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableDateTimeConvertedType, IList<DateTime?>>(
            b => b
                .HasConversion<CustomCollectionConverter<IList<DateTime?>, DateTime?>,
                    CustomCollectionComparer<IList<DateTime?>, DateTime?>>(),
            nameof(NullableDateTimeConvertedType.DateTimeConverted),
            new List<DateTime?>
            {
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":"[\u00220001-01-01T00:00:00\u0022,null,\u00222023-05-29T10:52:47\u0022,\u00229999-12-31T23:59:59.9999999\u0022]"}""");

    protected class NullableDateTimeConvertedType
    {
        public IList<DateTime?> DateTimeConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_bool_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableBooleanConvertedType, IList<bool?>>(
            b => b.HasConversion<CustomCollectionConverter<IList<bool?>, bool?>, CustomCollectionComparer<IList<bool?>, bool?>>(),
            nameof(NullableBooleanConvertedType.BooleanConverted),
            new List<bool?>
            {
                false,
                null,
                true
            },
            """{"Prop":"[false,null,true]"}""");

    protected class NullableBooleanConvertedType
    {
        public IList<bool?> BooleanConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_char_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableCharacterConvertedType, IList<char?>>(
            b => b.HasConversion<CustomCollectionConverter<IList<char?>, char?>, CustomCollectionComparer<IList<char?>, char?>>(),
            nameof(NullableCharacterConvertedType.CharacterConverted),
            new List<char?>
            {
                char.MinValue,
                'X',
                char.MaxValue,
                null
            },
            """{"Prop":"[\u0022\\u0000\u0022,\u0022X\u0022,\u0022\\uFFFF\u0022,null]"}""");

    protected class NullableCharacterConvertedType
    {
        public IList<char?> CharacterConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_string_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableStringConvertedType, IList<string?>>(
            b => b.HasConversion<CustomCollectionConverter<IList<string?>, string?>, CustomCollectionComparer<IList<string?>, string?>>(),
            nameof(NullableStringConvertedType.StringConverted),
            new List<string?>
            {
                "MinValue",
                null,
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            },
            """{"Prop":"[\u0022MinValue\u0022,null,\u0022\\u2764\\u2765\\uC6C3\\uC720\\u264B\\u262E\\u270C\\u260F\\u2622\\u2620\\u2714\\u2611\\u265A\\u25B2\\u266A\\u0E3F\\u0189\\u26CF\\u2665\\u2763\\u2642\\u2640\\u263F\\uD83D\\uDC4D\\u270D\\u2709\\u2623\\u2624\\u2718\\u2612\\u265B\\u25BC\\u266B\\u2318\\u231B\\u00A1\\u2661\\u10E6\\u30C4\\u263C\\u2601\\u2745\\u267E\\uFE0F\\u270E\\u00A9\\u00AE\\u2122\\u03A3\\u272A\\u272F\\u262D\\u27B3\\u24B6\\u271E\\u2103\\u2109\\u00B0\\u273F\\u26A1\\u2603\\u2602\\u2704\\u00A2\\u20AC\\u00A3\\u221E\\u272B\\u2605\\u00BD\\u262F\\u2721\\u262A\u0022,\u0022MaxValue\u0022]"}""");

    protected class NullableStringConvertedType
    {
        public IList<string?> StringConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_binary_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableBytesConvertedType, IList<byte[]?>>(
            b => b.HasConversion<CustomCollectionConverter<IList<byte[]?>, byte[]?>, CustomCollectionComparer<IList<byte[]?>, byte[]?>>(),
            nameof(NullableBytesConvertedType.BytesConverted),
            new List<byte[]?>
            {
                new byte[] { 0, 0, 0, 1 },
                null,
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":"[\u0022AAAAAQ==\u0022,null,\u0022/////w==\u0022,\u0022\u0022,\u0022AQIDBA==\u0022]"}""");

    protected class NullableBytesConvertedType
    {
        public IList<byte[]?> BytesConverted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_int_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableEnum32ConvertedType, List<Enum32?>>(
            b => b.HasConversion<CustomCollectionConverter<List<Enum32?>, Enum32?>, CustomCollectionComparer<List<Enum32?>, Enum32?>>(),
            nameof(NullableEnum32ConvertedType.Enum32Converted),
            [
                Enum32.Min,
                null,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            ],
            """{"Prop":"[-2147483648,null,2147483647,0,1,-8]"}""");

    protected class NullableEnum32ConvertedType
    {
        public List<Enum32?> Enum32Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_ulong_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_property_value<NullableEnumU64ConvertedType, IList<EnumU64?>>(
            b => b
                .HasConversion<CustomCollectionConverter<IList<EnumU64?>, EnumU64?>, CustomCollectionComparer<IList<EnumU64?>, EnumU64?>>(),
            nameof(NullableEnumU64ConvertedType.EnumU64Converted),
            new List<EnumU64?>
            {
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":"[0,null,18446744073709551615,0,1,8]"}""");

    protected class NullableEnumU64ConvertedType
    {
        public IList<EnumU64?> EnumU64Converted { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_int_with_converter_JSON_values()
        => Can_read_and_write_JSON_collection_value<DddIdCollectionType, List<DddId>>(
            b => b.ElementType(
                b =>
                {
                    b.HasConversion<DddIdConverter>();
                    b.IsRequired();
                }),
            nameof(DddIdCollectionType.DddId),
            [
                new() { Id = int.MinValue },
                new() { Id = 0 },
                new() { Id = int.MaxValue }
            ],
            """{"Prop":[-2147483648,0,2147483647]}""",
            facets: new Dictionary<string, object?> { { CoreAnnotationNames.ValueConverter, typeof(DddIdConverter) } });

    protected class DddIdCollectionType
    {
        public IList<DddId> DddId { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_nullable_int_with_converter_JSON_values()
        => Can_read_and_write_JSON_collection_value<NullableDddIdCollectionType, List<DddId?>>(
            b => b.ElementType().HasConversion<DddIdConverter>(),
            nameof(NullableDddIdCollectionType.DddId),
            [
                null,
                new() { Id = int.MinValue },
                null,
                new() { Id = 0 },
                new() { Id = int.MaxValue }
            ],
            """{"Prop":[null,-2147483648,null,0,2147483647]}""",
            facets: new Dictionary<string, object?> { { CoreAnnotationNames.ValueConverter, typeof(DddIdConverter) } });

    protected class NullableDddIdCollectionType
    {
        public IList<DddId?> DddId { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_binary_as_collection()
        => Can_read_and_write_JSON_collection_value<BinaryAsJsonType, byte[]>(
            _ => { },
            nameof(BinaryAsJsonType.BinaryAsJson),
            [77, 78, 79, 80],
            """{"Prop":[77,78,79,80]}""");

    protected class BinaryAsJsonType
    {
        public byte[] BinaryAsJson { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_decimal_with_precision_and_scale_JSON_values()
        => Can_read_and_write_JSON_collection_value<DecimalCollectionType, List<decimal>>(
            b => b.ElementType().HasPrecision(12, 6),
            nameof(DecimalCollectionType.Decimal),
            [
                decimal.MinValue,
                0,
                decimal.MaxValue
            ],
            """{"Prop":[-79228162514264337593543950335,0,79228162514264337593543950335]}""",
            facets: new Dictionary<string, object?> { { CoreAnnotationNames.Precision, 12 }, { CoreAnnotationNames.Scale, 6 } });

    [ConditionalFact]
    public virtual Task Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values()
        => Can_read_and_write_JSON_collection_value<GuidCollectionType, List<Guid>>(
            b => b.ElementType().HasConversion<byte[]>(),
            nameof(GuidCollectionType.Guid),
            [
                new(),
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            ],
            """{"Prop":["AAAAAAAAAAAAAAAAAAAAAA==","LyREjD+OIEqL6JjHwarevQ==","/////////////////////w=="]}""",
            facets: new Dictionary<string, object?> { { CoreAnnotationNames.ProviderClrType, typeof(byte[]) } });

    protected virtual async Task Can_read_and_write_JSON_value<TEntity, TModel>(
        string propertyName,
        TModel value,
        string json,
        bool mappedCollection = false,
        object? existingObject = null,
        Dictionary<string, object?>? facets = null)
        where TEntity : class
    {
        if (mappedCollection)
        {
            await Can_read_and_write_JSON_value<TEntity, TModel>(
                b => b.Entity<TEntity>().HasNoKey().PrimitiveCollection(propertyName),
                null,
                propertyName,
                value,
                json,
                mappedCollection,
                existingObject,
                facets);
        }
        else
        {
            await Can_read_and_write_JSON_value<TEntity, TModel>(
                b => b.Entity<TEntity>().HasNoKey().Property(propertyName),
                null,
                propertyName,
                value,
                json,
                mappedCollection,
                existingObject,
                facets);
        }
    }

    protected virtual Task Can_read_and_write_JSON_property_value<TEntity, TModel>(
        Action<PropertyBuilder> buildProperty,
        string propertyName,
        TModel value,
        string json,
        object? existingObject = null,
        Dictionary<string, object?>? facets = null)
        where TEntity : class
        => Can_read_and_write_JSON_value<TEntity, TModel>(
            b => buildProperty(b.Entity<TEntity>().HasNoKey().Property(propertyName)),
            null,
            propertyName,
            value,
            json,
            mappedCollection: false,
            existingObject: existingObject,
            facets: facets);

    protected virtual Task Can_read_and_write_JSON_collection_value<TEntity, TModel>(
        Action<PrimitiveCollectionBuilder> buildCollection,
        string propertyName,
        TModel value,
        string json,
        object? existingObject = null,
        Dictionary<string, object?>? facets = null)
        where TEntity : class
        => Can_read_and_write_JSON_value<TEntity, TModel>(
            b => buildCollection(b.Entity<TEntity>().HasNoKey().PrimitiveCollection(propertyName)),
            null,
            propertyName,
            value,
            json,
            mappedCollection: true,
            existingObject: existingObject,
            facets: facets);

    protected virtual async Task Can_read_and_write_JSON_value<TEntity, TModel>(
        Action<ModelBuilder> buildModel,
        Action<ModelConfigurationBuilder>? configureConventions,
        string propertyName,
        TModel value,
        string json,
        bool mappedCollection = false,
        object? existingObject = null,
        Dictionary<string, object?>? facets = null)
        where TEntity : class
    {
        var contextFactory = await CreateContextFactory<DbContext>(
            buildModel,
            addServices: AddServices,
            configureConventions: configureConventions);
        using var context = contextFactory.CreateContext();

        var property = context.Model.FindEntityType(typeof(TEntity))!.GetProperty(propertyName);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var jsonReaderWriter = property.GetJsonValueReaderWriter()
            ?? property.GetTypeMapping().JsonValueReaderWriter!;

        var toString = value == null ? null : jsonReaderWriter.ToJsonString(value);
        var fromString = toString == null ? null : jsonReaderWriter.FromJsonString(toString, existingObject);
        Assert.Equal(value, fromString);

        var actual = ToJsonPropertyString(jsonReaderWriter, value);
        Assert.Equal(json, actual);

        var fromJson = FromJsonPropertyString(jsonReaderWriter, actual, existingObject);
        if (existingObject != null)
        {
            Assert.Same(fromJson, existingObject);
        }

        Assert.Equal(value, fromJson);

        var element = property.GetElementType();
        if (mappedCollection)
        {
            Assert.NotNull(element);

            Assert.Equal(typeof(TModel).GetSequenceType(), element.ClrType);
            Assert.Same(property, element.CollectionProperty);
            Assert.Null(element.FindTypeMapping()!.ElementTypeMapping);

            bool elementNullable;
            if (element.ClrType.IsValueType)
            {
                elementNullable = element.ClrType.IsNullableType();
            }
            else
            {
                var nullabilityInfo = property switch
                {
                    { PropertyInfo: PropertyInfo p } => _nullabilityInfoContext.Create(p),
                    { FieldInfo: FieldInfo f } => _nullabilityInfoContext.Create(f),
                    _ => throw new UnreachableException()
                };

                elementNullable = nullabilityInfo is not
                    { ElementType.ReadState: NullabilityState.NotNull } and not
                    { GenericTypeArguments: [{ ReadState: NullabilityState.NotNull }] };
            }

            Assert.Equal(elementNullable, element.IsNullable);

            var comparer = element.GetValueComparer()!;
            var elementReaderWriter = element.GetJsonValueReaderWriter()!;
            foreach (var item in (IEnumerable)value!)
            {
                Assert.True(comparer.Equals(item, comparer.Snapshot(item)));
                Assert.True(
                    comparer.Equals(
                        item, FromJsonPropertyString(
                            elementReaderWriter, ToJsonPropertyString(elementReaderWriter, item))));
            }

            AssertElementFacets(element, facets);
        }
        else
        {
            Assert.Null(element);
        }
    }

    protected override string StoreName
        => "JsonTypesTest";

    protected virtual IServiceCollection AddServices(IServiceCollection serviceCollection)
        => serviceCollection;

    protected virtual void AssertElementFacets(IElementType element, Dictionary<string, object?>? facets)
    {
        Assert.Equal(FacetValue(CoreAnnotationNames.Precision), element.GetPrecision());
        Assert.Equal(FacetValue(CoreAnnotationNames.Scale), element.GetScale());
        Assert.Equal(FacetValue(CoreAnnotationNames.MaxLength), element.GetMaxLength());
        Assert.Equal(FacetValue(CoreAnnotationNames.ProviderClrType), element.GetProviderClrType());
        Assert.Equal(FacetValue(CoreAnnotationNames.Unicode), element.IsUnicode());
        Assert.Equal(FacetValue(CoreAnnotationNames.ValueConverter), element.GetValueConverter()?.GetType());

        object? FacetValue(string facetName)
            => facets?.TryGetValue(facetName, out var facet) == true ? facet : null;
    }

    protected string ToJsonPropertyString(JsonValueReaderWriter jsonReaderWriter, object? value)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("Prop");
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            jsonReaderWriter.ToJson(writer, value);
        }

        writer.WriteEndObject();
        writer.Flush();

        var buffer = stream.ToArray();

        return Encoding.UTF8.GetString(buffer);
    }

    protected object? FromJsonPropertyString(JsonValueReaderWriter jsonReaderWriter, string value, object? existingValue = null)
    {
        var buffer = Encoding.UTF8.GetBytes(value);
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(buffer), null);
        readerManager.MoveNext();
        readerManager.MoveNext();
        readerManager.MoveNext();

        return readerManager.CurrentReader.TokenType == JsonTokenType.Null
            ? null
            : jsonReaderWriter.FromJson(ref readerManager, existingValue);
    }

    protected readonly struct DddId
    {
        public int Id { get; init; }
    }

    protected class DddIdConverter : ValueConverter<DddId, int>
    {
        public DddIdConverter()
            : base(v => v.Id, v => new DddId { Id = v })
        {
        }
    }

    public enum Enum8 : sbyte
    {
        Min = sbyte.MinValue,
        Default = 0,
        One = 1,
        Max = sbyte.MaxValue
    }

    public enum Enum16 : short
    {
        Min = short.MinValue,
        Default = 0,
        One = 1,
        Max = short.MaxValue
    }

    public enum Enum32
    {
        Min = int.MinValue,
        Default = 0,
        One = 1,
        Max = int.MaxValue
    }

    public enum Enum64 : long
    {
        Min = long.MinValue,
        Default = 0,
        One = 1,
        Max = long.MaxValue
    }

    public enum EnumU8 : byte
    {
        Min = byte.MinValue,
        Default = 0,
        One = 1,
        Max = byte.MaxValue
    }

    public enum EnumU16 : ushort
    {
        Min = ushort.MinValue,
        Default = 0,
        One = 1,
        Max = ushort.MaxValue
    }

    public enum EnumU32 : uint
    {
        Min = uint.MinValue,
        Default = 0,
        One = 1,
        Max = uint.MaxValue
    }

    public enum EnumU64 : ulong
    {
        Min = ulong.MinValue,
        Default = 0,
        One = 1,
        Max = ulong.MaxValue
    }

    public class CustomCollectionConverter<T, TElement> : ValueConverter<T, string>
        where T : class, IList<TElement>
    {
        public CustomCollectionConverter()
            : base(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<T>(v, (JsonSerializerOptions?)null)!)
        {
        }
    }

    public class CustomCollectionComparer<T, TElement> : ValueComparer<T>
        where T : class, IList<TElement>
    {
        public CustomCollectionComparer()
            : base(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
                c => (T)(object)c.ToList())
        {
        }
    }

    public sealed class JsonGeoJsonReaderWriter : JsonValueReaderWriter<Geometry>
    {
        public static JsonGeoJsonReaderWriter Instance { get; } = new();

        private JsonGeoJsonReaderWriter()
        {
        }

        public override Geometry FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        {
            var builder = new StringBuilder();
            var depth = 0;
            var comma = false;
            do
            {
                switch (manager.CurrentReader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        depth--;
                        builder.Append('}');
                        comma = true;
                        break;
                    case JsonTokenType.PropertyName:
                        builder.Append(comma ? ",\"" : "\"").Append(manager.CurrentReader.GetString()).Append("\":");
                        comma = false;
                        break;
                    case JsonTokenType.StartObject:
                        depth++;
                        builder.Append(comma ? ",{" : "{");
                        comma = false;
                        break;
                    case JsonTokenType.String:
                        builder.Append(comma ? ",\"" : "\"").Append(manager.CurrentReader.GetString()).Append('"');
                        comma = true;
                        break;
                    case JsonTokenType.Number:
                        builder.Append(comma ? "," : "").Append(manager.CurrentReader.GetDecimal());
                        comma = true;
                        break;
                    case JsonTokenType.True:
                        builder.Append(comma ? ",true" : "true");
                        comma = true;
                        break;
                    case JsonTokenType.False:
                        builder.Append(comma ? ",false" : "false");
                        comma = true;
                        break;
                    case JsonTokenType.Null:
                        builder.Append(comma ? ",null" : "null");
                        comma = true;
                        break;
                    case JsonTokenType.StartArray:
                        builder.Append(comma ? ",[" : "[");
                        comma = false;
                        break;
                    case JsonTokenType.EndArray:
                        builder.Append(']');
                        comma = true;
                        break;
                    case JsonTokenType.None:
                        break;
                    case JsonTokenType.Comment:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (depth > 0)
                {
                    manager.MoveNext();
                }
            }
            while (depth > 0);

            var serializer = GeoJsonSerializer.Create();
            using var stringReader = new StringReader(builder.ToString());
            using var jsonReader = new JsonTextReader(stringReader);
            return serializer.Deserialize<Geometry>(jsonReader)!;
        }

        public override void ToJsonTyped(Utf8JsonWriter writer, Geometry value)
        {
            var serializer = GeoJsonSerializer.Create();
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            serializer.Serialize(jsonWriter, value);
            writer.WriteRawValue(stringWriter.ToString());
        }
    }

    private readonly NullabilityInfoContext _nullabilityInfoContext = new();
}
