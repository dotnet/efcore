// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Microsoft.EntityFrameworkCore;

public abstract class JsonTypesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : JsonTypesTestBase<TFixture>.JsonTypesFixtureBase, new()
{
    protected JsonTypesTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, """{"Prop":-128}""")]
    [InlineData(sbyte.MaxValue, """{"Prop":127}""")]
    [InlineData((sbyte)0, """{"Prop":0}""")]
    [InlineData((sbyte)1, """{"Prop":1}""")]
    public virtual void Can_read_write_sbyte_JSON_values(sbyte value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Int8)), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, """{"Prop":-32768}""")]
    [InlineData(short.MaxValue, """{"Prop":32767}""")]
    [InlineData((short)0, """{"Prop":0}""")]
    [InlineData((short)1, """{"Prop":1}""")]
    public virtual void Can_read_write_short_JSON_values(short value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Int16)), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    public virtual void Can_read_write_int_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Int32)), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, """{"Prop":-9223372036854775808}""")]
    [InlineData(long.MaxValue, """{"Prop":9223372036854775807}""")]
    [InlineData((long)0, """{"Prop":0}""")]
    [InlineData((long)1, """{"Prop":1}""")]
    public virtual void Can_read_write_long_JSON_values(long value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Int64)), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, """{"Prop":0}""")]
    [InlineData(byte.MaxValue, """{"Prop":255}""")]
    [InlineData((byte)1, """{"Prop":1}""")]
    public virtual void Can_read_write_byte_JSON_values(byte value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.UInt8)), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, """{"Prop":0}""")]
    [InlineData(ushort.MaxValue, """{"Prop":65535}""")]
    [InlineData((ushort)1, """{"Prop":1}""")]
    public virtual void Can_read_write_ushort_JSON_values(ushort value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.UInt16)), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, """{"Prop":0}""")]
    [InlineData(uint.MaxValue, """{"Prop":4294967295}""")]
    [InlineData((uint)1, """{"Prop":1}""")]
    public virtual void Can_read_write_uint_JSON_values(uint value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.UInt32)), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, """{"Prop":0}""")]
    [InlineData(ulong.MaxValue, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)1, """{"Prop":1}""")]
    public virtual void Can_read_write_ulong_JSON_values(ulong value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.UInt64)), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, """{"Prop":-3.4028235E+38}""")]
    [InlineData(float.MaxValue, """{"Prop":3.4028235E+38}""")]
    [InlineData((float)0.0, """{"Prop":0}""")]
    [InlineData((float)1.1, """{"Prop":1.1}""")]
    public virtual void Can_read_write_float_JSON_values(float value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Float)), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, """{"Prop":-1.7976931348623157E+308}""")]
    [InlineData(double.MaxValue, """{"Prop":1.7976931348623157E+308}""")]
    [InlineData(0.0, """{"Prop":0}""")]
    [InlineData(1.1, """{"Prop":1.1}""")]
    public virtual void Can_read_write_double_JSON_values(double value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Double)), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":-79228162514264337593543950335}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":79228162514264337593543950335}""")]
    [InlineData("0.0", """{"Prop":0.0}""")]
    [InlineData("1.1", """{"Prop":1.1}""")]
    public virtual void Can_read_write_decimal_JSON_values(decimal value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Decimal)), value, json);

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"0001-01-01"}""")]
    [InlineData("12/31/9999", """{"Prop":"9999-12-31"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    public virtual void Can_read_write_DateOnly_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.DateOnly)),
            DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", """{"Prop":"00:00:00.0000000"}""")]
    [InlineData("23:59:59.9999999", """{"Prop":"23:59:59.9999999"}""")]
    [InlineData("11:05:12.3456789", """{"Prop":"11:05:12.3456789"}""")]
    public virtual void Can_read_write_TimeOnly_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.TimeOnly)),
            TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01T00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31T23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29T10:52:47.2064353"}""")]
    public virtual void Can_read_write_DateTime_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.DateTime)),
            DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31T23:59:59.9999999+02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29T11:11:15.5672854+04:00"}""")]
    public virtual void Can_read_write_DateTimeOffset_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.DateTimeOffset)),
            DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", """{"Prop":"-10675199:2:48:05.4775808"}""")]
    [InlineData("10675199.02:48:05.4775807", """{"Prop":"10675199:2:48:05.4775807"}""")]
    [InlineData("00:00:00", """{"Prop":"0:00:00"}""")]
    [InlineData("12:23:23.8018854", """{"Prop":"12:23:23.8018854"}""")]
    public virtual void Can_read_write_TimeSpan_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.TimeSpan)), TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, """{"Prop":false}""")]
    [InlineData(true, """{"Prop":true}""")]
    public virtual void Can_read_write_bool_JSON_values(bool value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Boolean)), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, """{"Prop":"\u0000"}""")]
    [InlineData(char.MaxValue, """{"Prop":"\uFFFF"}""")]
    [InlineData(' ', """{"Prop":" "}""")]
    [InlineData("Z", """{"Prop":"Z"}""")]
    public virtual void Can_read_write_char_JSON_values(char value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Character)), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"ffffffff-ffff-ffff-ffff-ffffffffffff"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8c44242f-8e3f-4a20-8be8-98c7c1aadebd"}""")]
    public virtual void Can_read_write_GUID_JSON_values(Guid value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Guid)), value, json);

    [ConditionalTheory]
    [InlineData("MinValue", """{"Prop":"MinValue"}""")]
    [InlineData("MaxValue", """{"Prop":"MaxValue"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    public virtual void Can_read_write_string_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.String)), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"AAAAAQ=="}""")]
    [InlineData("255,255,255,255", """{"Prop":"/////w=="}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"AQIDBA=="}""")]
    public virtual void Can_read_write_binary_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.Bytes)),
            value == "" ? Array.Empty<byte>() : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        """{"Prop":"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName"}""")]
    [InlineData("file:///C:/test/path/file.txt", """{"Prop":"file:///C:/test/path/file.txt"}""")]
    public virtual void Can_read_write_URI_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.Uri)), new Uri(value), json);

    [ConditionalTheory]
    [InlineData("127.0.0.1", """{"Prop":"127.0.0.1"}""")]
    [InlineData("0.0.0.0", """{"Prop":"0.0.0.0"}""")]
    [InlineData("255.255.255.255", """{"Prop":"255.255.255.255"}""")]
    [InlineData("192.168.1.156", """{"Prop":"192.168.1.156"}""")]
    [InlineData("::1", """{"Prop":"::1"}""")]
    [InlineData("::", """{"Prop":"::"}""")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", """{"Prop":"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"}""")]
    public virtual void Can_read_write_IP_address_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.IpAddress)), IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", """{"Prop":"001122334455"}""")]
    [InlineData("00-11-22-33-44-55", """{"Prop":"001122334455"}""")]
    [InlineData("0011.2233.4455", """{"Prop":"001122334455"}""")]
    public virtual void Can_read_write_physical_address_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.PhysicalAddress)), PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, """{"Prop":-128}""")]
    [InlineData((sbyte)Enum8.Max, """{"Prop":127}""")]
    [InlineData((sbyte)Enum8.Default, """{"Prop":0}""")]
    [InlineData((sbyte)Enum8.One, """{"Prop":1}""")]
    public virtual void Can_read_write_sbyte_enum_JSON_values(Enum8 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Enum8)), value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, """{"Prop":-32768}""")]
    [InlineData((short)Enum16.Max, """{"Prop":32767}""")]
    [InlineData((short)Enum16.Default, """{"Prop":0}""")]
    [InlineData((short)Enum16.One, """{"Prop":1}""")]
    public virtual void Can_read_write_short_enum_JSON_values(Enum16 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Enum16)), value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, """{"Prop":-2147483648}""")]
    [InlineData((int)Enum32.Max, """{"Prop":2147483647}""")]
    [InlineData((int)Enum32.Default, """{"Prop":0}""")]
    [InlineData((int)Enum32.One, """{"Prop":1}""")]
    public virtual void Can_read_write_int_enum_JSON_values(Enum32 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Enum32)), value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, """{"Prop":-9223372036854775808}""")]
    [InlineData((long)Enum64.Max, """{"Prop":9223372036854775807}""")]
    [InlineData((long)Enum64.Default, """{"Prop":0}""")]
    [InlineData((long)Enum64.One, """{"Prop":1}""")]
    public virtual void Can_read_write_long_enum_JSON_values(Enum64 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.Enum64)), value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, """{"Prop":0}""")]
    [InlineData((byte)EnumU8.Max, """{"Prop":255}""")]
    [InlineData((byte)EnumU8.One, """{"Prop":1}""")]
    public virtual void Can_read_write_byte_enum_JSON_values(EnumU8 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.EnumU8)), value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, """{"Prop":0}""")]
    [InlineData((ushort)EnumU16.Max, """{"Prop":65535}""")]
    [InlineData((ushort)EnumU16.One, """{"Prop":1}""")]
    public virtual void Can_read_write_ushort_enum_JSON_values(EnumU16 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.EnumU16)), value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, """{"Prop":0}""")]
    [InlineData((uint)EnumU32.Max, """{"Prop":4294967295}""")]
    [InlineData((uint)EnumU32.One, """{"Prop":1}""")]
    public virtual void Can_read_write_uint_enum_JSON_values(EnumU32 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.EnumU32)), value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, """{"Prop":0}""")]
    [InlineData((ulong)EnumU64.Max, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)EnumU64.One, """{"Prop":1}""")]
    public virtual void Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<Types>().GetProperty(nameof(Types.EnumU64)), value, json);

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, """{"Prop":-128}""")]
    [InlineData(sbyte.MaxValue, """{"Prop":127}""")]
    [InlineData((sbyte)0, """{"Prop":0}""")]
    [InlineData((sbyte)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_sbyte_JSON_values(sbyte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Int8)), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, """{"Prop":-32768}""")]
    [InlineData(short.MaxValue, """{"Prop":32767}""")]
    [InlineData((short)0, """{"Prop":0}""")]
    [InlineData((short)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_short_JSON_values(short? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Int16)), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_int_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Int32)), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, """{"Prop":-9223372036854775808}""")]
    [InlineData(long.MaxValue, """{"Prop":9223372036854775807}""")]
    [InlineData((long)0, """{"Prop":0}""")]
    [InlineData((long)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_long_JSON_values(long? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Int64)), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, """{"Prop":0}""")]
    [InlineData(byte.MaxValue, """{"Prop":255}""")]
    [InlineData((byte)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_byte_JSON_values(byte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.UInt8)), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, """{"Prop":0}""")]
    [InlineData(ushort.MaxValue, """{"Prop":65535}""")]
    [InlineData((ushort)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ushort_JSON_values(ushort? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.UInt16)), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, """{"Prop":0}""")]
    [InlineData(uint.MaxValue, """{"Prop":4294967295}""")]
    [InlineData((uint)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_uint_JSON_values(uint? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.UInt32)), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, """{"Prop":0}""")]
    [InlineData(ulong.MaxValue, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ulong_JSON_values(ulong? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.UInt64)), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, """{"Prop":-3.4028235E+38}""")]
    [InlineData(float.MaxValue, """{"Prop":3.4028235E+38}""")]
    [InlineData((float)0.0, """{"Prop":0}""")]
    [InlineData((float)1.1, """{"Prop":1.1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_float_JSON_values(float? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Float)), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, """{"Prop":-1.7976931348623157E+308}""")]
    [InlineData(double.MaxValue, """{"Prop":1.7976931348623157E+308}""")]
    [InlineData(0.0, """{"Prop":0}""")]
    [InlineData(1.1, """{"Prop":1.1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_double_JSON_values(double? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Double)), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":-79228162514264337593543950335}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":79228162514264337593543950335}""")]
    [InlineData("0.0", """{"Prop":0.0}""")]
    [InlineData("1.1", """{"Prop":1.1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_decimal_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Decimal)),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"0001-01-01"}""")]
    [InlineData("12/31/9999", """{"Prop":"9999-12-31"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateOnly_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.DateOnly)),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", """{"Prop":"00:00:00.0000000"}""")]
    [InlineData("23:59:59.9999999", """{"Prop":"23:59:59.9999999"}""")]
    [InlineData("11:05:12.3456789", """{"Prop":"11:05:12.3456789"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_TimeOnly_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.TimeOnly)),
            value == null ? default(TimeOnly?) : TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01T00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31T23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29T10:52:47.2064353"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.DateTime)),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01T00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31T23:59:59.9999999+02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01T00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29T11:11:15.5672854+04:00"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.DateTimeOffset)),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", """{"Prop":"-10675199:2:48:05.4775808"}""")]
    [InlineData("10675199.02:48:05.4775807", """{"Prop":"10675199:2:48:05.4775807"}""")]
    [InlineData("00:00:00", """{"Prop":"0:00:00"}""")]
    [InlineData("12:23:23.8018854", """{"Prop":"12:23:23.8018854"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_TimeSpan_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.TimeSpan)),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, """{"Prop":false}""")]
    [InlineData(true, """{"Prop":true}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_bool_JSON_values(bool? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Boolean)), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, """{"Prop":"\u0000"}""")]
    [InlineData(char.MaxValue, """{"Prop":"\uFFFF"}""")]
    [InlineData(' ', """{"Prop":" "}""")]
    [InlineData('Z', """{"Prop":"Z"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_char_JSON_values(char? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Character)), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"ffffffff-ffff-ffff-ffff-ffffffffffff"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8c44242f-8e3f-4a20-8be8-98c7c1aadebd"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_GUID_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Guid)),
            value == null ? default(Guid?) : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("MinValue", """{"Prop":"MinValue"}""")]
    [InlineData("MaxValue", """{"Prop":"MaxValue"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.String)), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"AAAAAQ=="}""")]
    [InlineData("255,255,255,255", """{"Prop":"/////w=="}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"AQIDBA=="}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_binary_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Bytes)),
            value == null
                ? default
                : value == ""
                    ? Array.Empty<byte>()
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        """{"Prop":"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName"}""")]
    [InlineData("file:///C:/test/path/file.txt", """{"Prop":"file:///C:/test/path/file.txt"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_URI_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Uri)),
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
    public virtual void Can_read_write_nullable_IP_address_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.IpAddress)),
            value == null ? default : IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", """{"Prop":"001122334455"}""")]
    [InlineData("00-11-22-33-44-55", """{"Prop":"001122334455"}""")]
    [InlineData("0011.2233.4455", """{"Prop":"001122334455"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_physical_address_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.PhysicalAddress)),
            value == null ? default : PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, """{"Prop":-128}""")]
    [InlineData((sbyte)Enum8.Max, """{"Prop":127}""")]
    [InlineData((sbyte)Enum8.Default, """{"Prop":0}""")]
    [InlineData((sbyte)Enum8.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_sbyte_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Enum8)),
            value == null ? default(Enum8?) : (Enum8)value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, """{"Prop":-32768}""")]
    [InlineData((short)Enum16.Max, """{"Prop":32767}""")]
    [InlineData((short)Enum16.Default, """{"Prop":0}""")]
    [InlineData((short)Enum16.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_short_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Enum16)),
            value == null ? default(Enum16?) : (Enum16)value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, """{"Prop":-2147483648}""")]
    [InlineData((int)Enum32.Max, """{"Prop":2147483647}""")]
    [InlineData((int)Enum32.Default, """{"Prop":0}""")]
    [InlineData((int)Enum32.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_int_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Enum32)),
            value == null ? default(Enum32?) : (Enum32)value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, """{"Prop":-9223372036854775808}""")]
    [InlineData((long)Enum64.Max, """{"Prop":9223372036854775807}""")]
    [InlineData((long)Enum64.Default, """{"Prop":0}""")]
    [InlineData((long)Enum64.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_long_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.Enum64)),
            value == null ? default(Enum64?) : (Enum64)value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, """{"Prop":0}""")]
    [InlineData((byte)EnumU8.Max, """{"Prop":255}""")]
    [InlineData((byte)EnumU8.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_byte_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.EnumU8)),
            value == null ? default(EnumU8?) : (EnumU8)value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, """{"Prop":0}""")]
    [InlineData((ushort)EnumU16.Max, """{"Prop":65535}""")]
    [InlineData((ushort)EnumU16.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ushort_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.EnumU16)),
            value == null ? default(EnumU16?) : (EnumU16)value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, """{"Prop":0}""")]
    [InlineData((uint)EnumU32.Max, """{"Prop":4294967295}""")]
    [InlineData((uint)EnumU32.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_uint_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.EnumU32)),
            value == null ? default(EnumU32?) : (EnumU32)value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, """{"Prop":0}""")]
    [InlineData((ulong)EnumU64.Max, """{"Prop":18446744073709551615}""")]
    [InlineData((ulong)EnumU64.One, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.EnumU64)),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, """{"Prop":"-128"}""")]
    [InlineData(sbyte.MaxValue, """{"Prop":"127"}""")]
    [InlineData((sbyte)0, """{"Prop":"0"}""")]
    [InlineData((sbyte)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_sbyte_as_string_JSON_values(sbyte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Int8)), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, """{"Prop":"-32768"}""")]
    [InlineData(short.MaxValue, """{"Prop":"32767"}""")]
    [InlineData((short)0, """{"Prop":"0"}""")]
    [InlineData((short)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_short_as_string_JSON_values(short? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Int16)), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":"-2147483648"}""")]
    [InlineData(int.MaxValue, """{"Prop":"2147483647"}""")]
    [InlineData(0, """{"Prop":"0"}""")]
    [InlineData(1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_int_as_string_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Int32)), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, """{"Prop":"-9223372036854775808"}""")]
    [InlineData(long.MaxValue, """{"Prop":"9223372036854775807"}""")]
    [InlineData((long)0, """{"Prop":"0"}""")]
    [InlineData((long)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_long_as_string_JSON_values(long? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Int64)), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, """{"Prop":"0"}""")]
    [InlineData(byte.MaxValue, """{"Prop":"255"}""")]
    [InlineData((byte)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_byte_as_string_JSON_values(byte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.UInt8)), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, """{"Prop":"0"}""")]
    [InlineData(ushort.MaxValue, """{"Prop":"65535"}""")]
    [InlineData((ushort)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ushort_as_string_JSON_values(ushort? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.UInt16)), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, """{"Prop":"0"}""")]
    [InlineData(uint.MaxValue, """{"Prop":"4294967295"}""")]
    [InlineData((uint)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_uint_as_string_JSON_values(uint? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.UInt32)), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, """{"Prop":"0"}""")]
    [InlineData(ulong.MaxValue, """{"Prop":"18446744073709551615"}""")]
    [InlineData((ulong)1, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ulong_as_string_JSON_values(ulong? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.UInt64)), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, """{"Prop":"-3.4028235E\u002B38"}""")]
    [InlineData(float.MaxValue, """{"Prop":"3.4028235E\u002B38"}""")]
    [InlineData((float)0.0, """{"Prop":"0"}""")]
    [InlineData((float)1.1, """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_float_as_string_JSON_values(float? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Float)), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, """{"Prop":"-1.7976931348623157E\u002B308"}""")]
    [InlineData(double.MaxValue, """{"Prop":"1.7976931348623157E\u002B308"}""")]
    [InlineData(0.0, """{"Prop":"0"}""")]
    [InlineData(1.1, """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_double_as_string_JSON_values(double? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Double)), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":"-79228162514264337593543950335"}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":"79228162514264337593543950335"}""")]
    [InlineData("0.0", """{"Prop":"0.0"}""")]
    [InlineData("1.1", """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_decimal_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Decimal)),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("1/1/0001", """{"Prop":"0001-01-01"}""")]
    [InlineData("12/31/9999", """{"Prop":"9999-12-31"}""")]
    [InlineData("5/29/2023", """{"Prop":"2023-05-29"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateOnly_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.DateOnly)),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", """{"Prop":"00:00:00"}""")]
    [InlineData("23:59:59.9999999", """{"Prop":"23:59:59.9999999"}""")]
    [InlineData("11:05:12.3456789", """{"Prop":"11:05:12.3456789"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_TimeOnly_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.TimeOnly)),
            value == null ? default(TimeOnly?) : TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01 00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31 23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29 10:52:47.2064353"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTime_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.DateTime)),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01 00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31 23:59:59.9999999\u002B02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01 00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29 11:11:15.5672854\u002B04:00"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTimeOffset_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.DateTimeOffset)),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", """{"Prop":"-10675199.02:48:05.4775808"}""")]
    [InlineData("10675199.02:48:05.4775807", """{"Prop":"10675199.02:48:05.4775807"}""")]
    [InlineData("00:00:00", """{"Prop":"00:00:00"}""")]
    [InlineData("12:23:23.8018854", """{"Prop":"12:23:23.8018854"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_TimeSpan_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.TimeSpan)),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, """{"Prop":"0"}""")]
    [InlineData(true, """{"Prop":"1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_bool_as_string_JSON_values(bool? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Boolean)), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, """{"Prop":"\u0000"}""")]
    [InlineData(char.MaxValue, """{"Prop":"\uFFFF"}""")]
    [InlineData(' ', """{"Prop":" "}""")]
    [InlineData('Z', """{"Prop":"Z"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_char_as_string_JSON_values(char? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Character)), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"ffffffff-ffff-ffff-ffff-ffffffffffff"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8c44242f-8e3f-4a20-8be8-98c7c1aadebd"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_as_string_GUID_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Guid)),
            value == null ? default(Guid?) : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("MinValue", """{"Prop":"MinValue"}""")]
    [InlineData("MaxValue", """{"Prop":"MaxValue"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_string_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.String)), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"AAAAAQ=="}""")]
    [InlineData("255,255,255,255", """{"Prop":"/////w=="}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"AQIDBA=="}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_binary_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Bytes)),
            value == null
                ? default
                : value == ""
                    ? Array.Empty<byte>()
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        """{"Prop":"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName"}""")]
    [InlineData("file:///C:/test/path/file.txt", """{"Prop":"file:///C:/test/path/file.txt"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_URI_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Uri)),
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
    public virtual void Can_read_write_nullable_IP_address_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.IpAddress)),
            value == null ? default : IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", """{"Prop":"001122334455"}""")]
    [InlineData("00-11-22-33-44-55", """{"Prop":"001122334455"}""")]
    [InlineData("0011.2233.4455", """{"Prop":"001122334455"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_physical_address_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.PhysicalAddress)),
            value == null ? default : PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, """{"Prop":"Min"}""")]
    [InlineData((sbyte)Enum8.Max, """{"Prop":"Max"}""")]
    [InlineData((sbyte)Enum8.Default, """{"Prop":"Default"}""")]
    [InlineData((sbyte)Enum8.One, """{"Prop":"One"}""")]
    [InlineData((sbyte)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_sbyte_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Enum8)),
            value == null ? default(Enum8?) : (Enum8)value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, """{"Prop":"Min"}""")]
    [InlineData((short)Enum16.Max, """{"Prop":"Max"}""")]
    [InlineData((short)Enum16.Default, """{"Prop":"Default"}""")]
    [InlineData((short)Enum16.One, """{"Prop":"One"}""")]
    [InlineData((short)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_short_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Enum16)),
            value == null ? default(Enum16?) : (Enum16)value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, """{"Prop":"Min"}""")]
    [InlineData((int)Enum32.Max, """{"Prop":"Max"}""")]
    [InlineData((int)Enum32.Default, """{"Prop":"Default"}""")]
    [InlineData((int)Enum32.One, """{"Prop":"One"}""")]
    [InlineData(77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_int_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Enum32)),
            value == null ? default(Enum32?) : (Enum32)value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, """{"Prop":"Min"}""")]
    [InlineData((long)Enum64.Max, """{"Prop":"Max"}""")]
    [InlineData((long)Enum64.Default, """{"Prop":"Default"}""")]
    [InlineData((long)Enum64.One, """{"Prop":"One"}""")]
    [InlineData((long)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_long_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.Enum64)),
            value == null ? default(Enum64?) : (Enum64)value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, """{"Prop":"Min"}""")]
    [InlineData((byte)EnumU8.Max, """{"Prop":"Max"}""")]
    [InlineData((byte)EnumU8.One, """{"Prop":"One"}""")]
    [InlineData((byte)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_byte_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.EnumU8)),
            value == null ? default(EnumU8?) : (EnumU8)value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, """{"Prop":"Min"}""")]
    [InlineData((ushort)EnumU16.Max, """{"Prop":"Max"}""")]
    [InlineData((ushort)EnumU16.One, """{"Prop":"One"}""")]
    [InlineData((ushort)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ushort_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.EnumU16)),
            value == null ? default(EnumU16?) : (EnumU16)value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, """{"Prop":"Min"}""")]
    [InlineData((uint)EnumU32.Max, """{"Prop":"Max"}""")]
    [InlineData((uint)EnumU32.One, """{"Prop":"One"}""")]
    [InlineData((uint)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_uint_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.EnumU32)),
            value == null ? default(EnumU32?) : (EnumU32)value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, """{"Prop":"Min"}""")]
    [InlineData((ulong)EnumU64.Max, """{"Prop":"Max"}""")]
    [InlineData((ulong)EnumU64.One, """{"Prop":"One"}""")]
    [InlineData((ulong)77, """{"Prop":"77"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_ulong_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<TypesAsStrings>().GetProperty(nameof(TypesAsStrings.EnumU64)),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    [ConditionalFact]
    public virtual void Can_read_write_point()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.Point)),
            factory.CreatePoint(new Coordinate(2, 4)),
            """{"Prop":"POINT (2 4)"}""");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.PointZ)),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            """{"Prop":"POINT Z(2 4 6)"}""");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.PointM)),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            """{"Prop":"POINT (2 4)"}""");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.PointZM)),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            """{"Prop":"POINT Z(1 2 3)"}""");

        Can_read_and_write_JSON_value<Point?>(
            entityType.GetProperty(nameof(GeometryTypes.Point)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.LineString)),
            factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }),
            """{"Prop":"LINESTRING (0 0, 1 0)"}""");

        Can_read_and_write_JSON_value<LineString?>(
            entityType.GetProperty(nameof(GeometryTypes.LineString)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_multi_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.MultiLineString)),
            factory.CreateMultiLineString(
                new[]
                {
                    factory.CreateLineString(
                        new[] { new Coordinate(0, 0), new Coordinate(0, 1) }),
                    factory.CreateLineString(
                        new[] { new Coordinate(1, 0), new Coordinate(1, 1) })
                }),
            """{"Prop":"MULTILINESTRING ((0 0, 0 1), (1 0, 1 1))"}""");

        Can_read_and_write_JSON_value<MultiLineString?>(
            entityType.GetProperty(nameof(GeometryTypes.MultiLineString)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.Polygon)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            """{"Prop":"POLYGON ((0 0, 1 0, 0 1, 0 0))"}""");

        Can_read_and_write_JSON_value<Polygon?>(
            entityType.GetProperty(nameof(GeometryTypes.Polygon)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon_typed_as_geometry()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.Geometry)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            """{"Prop":"POLYGON ((0 0, 1 0, 0 1, 0 0))"}""");

        Can_read_and_write_JSON_value<Geometry?>(
            entityType.GetProperty(nameof(GeometryTypes.Geometry)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_point_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Point)),
            factory.CreatePoint(new Coordinate(2, 4)),
            """{"Prop":{"type":"Point","coordinates":[2.0,4.0]}}""");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.PointZ)),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            """{"Prop":{"type":"Point","coordinates":[2.0,4.0]}}""");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.PointM)),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            """{"Prop":{"type":"Point","coordinates":[2.0,4.0]}}""");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.PointZM)),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            """{"Prop":{"type":"Point","coordinates":[1.0,2.0]}}""");

        Can_read_and_write_JSON_value<Point?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Point)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_line_string_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.LineString)),
            factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }),
            """{"Prop":{"type":"LineString","coordinates":[[0.0,0.0],[1.0,0.0]]}}""");

        Can_read_and_write_JSON_value<LineString?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.LineString)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_multi_line_string_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.MultiLineString)),
            factory.CreateMultiLineString(
                new[]
                {
                    factory.CreateLineString(
                        new[] { new Coordinate(0, 0), new Coordinate(0, 1) }),
                    factory.CreateLineString(
                        new[] { new Coordinate(1, 0), new Coordinate(1, 1) })
                }),
            """{"Prop":{"type":"MultiLineString","coordinates":[[[0.0,0.0],[0.0,1.0]],[[1.0,0.0],[1.0,1.0]]]}}""");

        Can_read_and_write_JSON_value<MultiLineString?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.MultiLineString)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Polygon)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            """{"Prop":{"type":"Polygon","coordinates":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}}""");

        Can_read_and_write_JSON_value<Polygon?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Polygon)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon_typed_as_geometry_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Geometry)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            """{"Prop":{"type":"Polygon","coordinates":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}}""");

        Can_read_and_write_JSON_value<Geometry?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Geometry)),
            null,
            """{"Prop":null}""");
    }

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    public virtual void Can_read_write_converted_type_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<Types>().GetProperty(nameof(Types.DddId)), new DddId { Id = value }, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, """{"Prop":-2147483648}""")]
    [InlineData(int.MaxValue, """{"Prop":2147483647}""")]
    [InlineData(0, """{"Prop":0}""")]
    [InlineData(1, """{"Prop":1}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_converted_type_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullableTypes>().GetProperty(nameof(NullableTypes.DddId)),
            value == null ? default(DddId?) : new DddId { Id = value.Value }, json);

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_sbyte_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Int8)),
            new List<sbyte>
            {
                sbyte.MinValue,
                0,
                sbyte.MaxValue
            },
            """{"Prop":[-128,0,127]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_short_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Int16)),
            new List<short>
            {
                short.MinValue,
                0,
                short.MaxValue
            },
            """{"Prop":[-32768,0,32767]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_int_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Int32)),
            new List<int>
            {
                int.MinValue,
                0,
                int.MaxValue
            },
            """{"Prop":[-2147483648,0,2147483647]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_long_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Int64)),
            new List<long>
            {
                long.MinValue,
                0,
                long.MaxValue
            },
            """{"Prop":[-9223372036854775808,0,9223372036854775807]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_byte_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.UInt8)),
            new List<byte>
            {
                byte.MinValue,
                1,
                byte.MaxValue
            },
            """{"Prop":[0,1,255]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_ushort_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.UInt16)),
            new List<ushort>
            {
                ushort.MinValue,
                1,
                ushort.MaxValue
            },
            """{"Prop":[0,1,65535]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_uint_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.UInt32)),
            new List<uint>
            {
                uint.MinValue,
                1,
                uint.MaxValue
            },
            """{"Prop":[0,1,4294967295]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_ulong_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.UInt64)),
            new List<ulong>
            {
                ulong.MinValue,
                1,
                ulong.MaxValue
            },
            """{"Prop":[0,1,18446744073709551615]}""",
            new ObservableCollection<ulong>());

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_float_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Float)),
            new List<float>
            {
                float.MinValue,
                0,
                float.MaxValue
            },
            """{"Prop":[-3.4028235E+38,0,3.4028235E+38]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_double_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Double)),
            new List<double>
            {
                double.MinValue,
                0,
                double.MaxValue
            },
            """{"Prop":[-1.7976931348623157E+308,0,1.7976931348623157E+308]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_decimal_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Decimal)),
            new List<decimal>
            {
                decimal.MinValue,
                0,
                decimal.MaxValue
            },
            """{"Prop":[-79228162514264337593543950335,0,79228162514264337593543950335]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.DateOnly)),
            new List<DateOnly>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue
            },
            """{"Prop":["0001-01-01","2023-05-29","9999-12-31"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_TimeOnly_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.TimeOnly)),
            new List<TimeOnly>
            {
                TimeOnly.MinValue,
                new(11, 5, 2, 3, 4),
                TimeOnly.MaxValue
            },
            """{"Prop":["00:00:00.0000000","11:05:02.0030040","23:59:59.9999999"]}""",
            new List<TimeOnly>());

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_DateTime_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.DateTime)),
            new List<DateTime>
            {
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":["0001-01-01T00:00:00","2023-05-29T10:52:47","9999-12-31T23:59:59.9999999"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.DateTimeOffset)),
            new List<DateTimeOffset>
            {
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            },
            """{"Prop":["0001-01-01T00:00:00+00:00","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47+00:00","2023-05-29T10:52:47+02:00","9999-12-31T23:59:59.9999999+00:00"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_TimeSpan_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.TimeSpan)),
            new List<TimeSpan>
            {
                TimeSpan.MinValue,
                new(1, 2, 3, 4, 5),
                TimeSpan.MaxValue
            },
            """{"Prop":["-10675199:2:48:05.4775808","1:2:03:04.005","10675199:2:48:05.4775807"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_bool_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Boolean)),
            new List<bool> { false, true },
            """{"Prop":[false,true]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_char_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Character)),
            new List<char>
            {
                char.MinValue,
                'X',
                char.MaxValue
            },
            """{"Prop":["\u0000","X","\uFFFF"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_GUID_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Guid)),
            new List<Guid>
            {
                new(),
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            },
            """{"Prop":["00000000-0000-0000-0000-000000000000","8c44242f-8e3f-4a20-8be8-98c7c1aadebd","ffffffff-ffff-ffff-ffff-ffffffffffff"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_string_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.String)),
            new List<string>
            {
                "MinValue",
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            },
            """{"Prop":["MinValue","\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A","MaxValue"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_binary_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Bytes)),
            new List<byte[]>
            {
                new byte[] { 0, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":["AAAAAQ==","/////w==","","AQIDBA=="]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_URI_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Uri)),
            new List<Uri>
            {
                new("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName"),
                new("file:///C:/test/path/file.txt")
            },
            """{"Prop":["https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName","file:///C:/test/path/file.txt"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_IP_address_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.IpAddress)),
            new List<IPAddress>
            {
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("0.0.0.0"),
                IPAddress.Parse("255.255.255.255"),
                IPAddress.Parse("192.168.1.156"),
                IPAddress.Parse("::1"),
                IPAddress.Parse("::"),
                IPAddress.Parse("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"),
            },
            """{"Prop":["127.0.0.1","0.0.0.0","255.255.255.255","192.168.1.156","::1","::","2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_physical_address_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.PhysicalAddress)),
            new List<PhysicalAddress>
            {
                PhysicalAddress.None,
                PhysicalAddress.Parse("001122334455"),
                PhysicalAddress.Parse("00-11-22-33-44-55"),
                PhysicalAddress.Parse("0011.2233.4455")
            },
            """{"Prop":["","001122334455","001122334455","001122334455"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_sbyte_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Enum8)),
            new List<Enum8>
            {
                Enum8.Min,
                Enum8.Max,
                Enum8.Default,
                Enum8.One,
                (Enum8)(-8)
            },
            """{"Prop":[-128,127,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_short_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Enum16)),
            new List<Enum16>
            {
                Enum16.Min,
                Enum16.Max,
                Enum16.Default,
                Enum16.One,
                (Enum16)(-8)
            },
            """{"Prop":[-32768,32767,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_int_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Enum32)),
            new List<Enum32>
            {
                Enum32.Min,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            },
            """{"Prop":[-2147483648,2147483647,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_long_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Enum64)),
            new List<Enum64>
            {
                Enum64.Min,
                Enum64.Max,
                Enum64.Default,
                Enum64.One,
                (Enum64)(-8)
            },
            """{"Prop":[-9223372036854775808,9223372036854775807,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_byte_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.EnumU8)),
            new List<EnumU8>
            {
                EnumU8.Min,
                EnumU8.Max,
                EnumU8.Default,
                EnumU8.One,
                (EnumU8)8
            },
            """{"Prop":[0,255,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_ushort_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.EnumU16)),
            new List<EnumU16>
            {
                EnumU16.Min,
                EnumU16.Max,
                EnumU16.Default,
                EnumU16.One,
                (EnumU16)8
            },
            """{"Prop":[0,65535,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_uint_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.EnumU32)),
            new List<EnumU32>
            {
                EnumU32.Min,
                EnumU32.Max,
                EnumU32.Default,
                EnumU32.One,
                (EnumU32)8
            },
            """{"Prop":[0,4294967295,0,1,8]}""",
            new ObservableCollection<EnumU32>());

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.EnumU64)),
            new List<EnumU64>
            {
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":[0,18446744073709551615,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_sbyte_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Int8)),
            new List<sbyte?>
            {
                null,
                sbyte.MinValue,
                0,
                sbyte.MaxValue
            },
            """{"Prop":[null,-128,0,127]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_short_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Int16)),
            new List<short?>
            {
                short.MinValue,
                null,
                0,
                short.MaxValue
            },
            """{"Prop":[-32768,null,0,32767]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_int_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Int32)),
            new List<int?>
            {
                int.MinValue,
                0,
                null,
                int.MaxValue
            },
            """{"Prop":[-2147483648,0,null,2147483647]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_long_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Int64)),
            new List<long?>
            {
                long.MinValue,
                0,
                long.MaxValue,
                null
            },
            """{"Prop":[-9223372036854775808,0,9223372036854775807,null]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_byte_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.UInt8)),
            new List<byte?>
            {
                null,
                byte.MinValue,
                1,
                byte.MaxValue
            },
            """{"Prop":[null,0,1,255]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_ushort_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.UInt16)),
            new List<ushort?>
            {
                ushort.MinValue,
                null,
                1,
                ushort.MaxValue
            },
            """{"Prop":[0,null,1,65535]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_uint_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.UInt32)),
            new List<uint?>
            {
                uint.MinValue,
                1,
                null,
                uint.MaxValue
            },
            """{"Prop":[0,1,null,4294967295]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_ulong_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.UInt64)),
            new List<ulong?>
            {
                ulong.MinValue,
                1,
                ulong.MaxValue,
                null
            },
            """{"Prop":[0,1,18446744073709551615,null]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_float_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Float)),
            new List<float?>
            {
                null,
                float.MinValue,
                0,
                float.MaxValue
            },
            """{"Prop":[null,-3.4028235E+38,0,3.4028235E+38]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_double_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Double)),
            new List<double?>
            {
                double.MinValue,
                null,
                0,
                double.MaxValue
            },
            """{"Prop":[-1.7976931348623157E+308,null,0,1.7976931348623157E+308]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_decimal_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Decimal)),
            new List<decimal?>
            {
                decimal.MinValue,
                0,
                null,
                decimal.MaxValue
            },
            """{"Prop":[-79228162514264337593543950335,0,null,79228162514264337593543950335]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_DateOnly_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.DateOnly)),
            new List<DateOnly?>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue,
                null
            },
            """{"Prop":["0001-01-01","2023-05-29","9999-12-31",null]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_TimeOnly_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.TimeOnly)),
            new List<TimeOnly?>
            {
                null,
                TimeOnly.MinValue,
                new(11, 5, 2, 3, 4),
                TimeOnly.MaxValue
            },
            """{"Prop":[null,"00:00:00.0000000","11:05:02.0030040","23:59:59.9999999"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_DateTime_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.DateTime)),
            new List<DateTime?>
            {
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":["0001-01-01T00:00:00",null,"2023-05-29T10:52:47","9999-12-31T23:59:59.9999999"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.DateTimeOffset)),
            new List<DateTimeOffset?>
            {
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                null,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            },
            """{"Prop":["0001-01-01T00:00:00+00:00","2023-05-29T10:52:47-02:00","2023-05-29T10:52:47+00:00",null,"2023-05-29T10:52:47+02:00","9999-12-31T23:59:59.9999999+00:00"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_TimeSpan_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.TimeSpan)),
            new List<TimeSpan?>
            {
                TimeSpan.MinValue,
                new(1, 2, 3, 4, 5),
                TimeSpan.MaxValue,
                null
            },
            """{"Prop":["-10675199:2:48:05.4775808","1:2:03:04.005","10675199:2:48:05.4775807",null]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_bool_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Boolean)),
            new List<bool?>
            {
                false,
                null,
                true
            },
            """{"Prop":[false,null,true]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_char_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Character)),
            new List<char?>
            {
                char.MinValue,
                'X',
                char.MaxValue,
                null
            },
            """{"Prop":["\u0000","X","\uFFFF",null]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_GUID_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Guid)),
            new List<Guid?>
            {
                new(),
                null,
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            },
            """{"Prop":["00000000-0000-0000-0000-000000000000",null,"8c44242f-8e3f-4a20-8be8-98c7c1aadebd","ffffffff-ffff-ffff-ffff-ffffffffffff"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_string_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.String)),
            new List<string?>
            {
                "MinValue",
                null,
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            },
            """{"Prop":["MinValue",null,"\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A","MaxValue"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_binary_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Bytes)),
            new List<byte[]?>
            {
                new byte[] { 0, 0, 0, 1 },
                null,
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":["AAAAAQ==",null,"/////w==","","AQIDBA=="]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_URI_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Uri)),
            new List<Uri?>
            {
                new("https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName"),
                null,
                new("file:///C:/test/path/file.txt")
            },
            """{"Prop":["https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1\u0026q2=v2#FragmentName",null,"file:///C:/test/path/file.txt"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_IP_address_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.IpAddress)),
            new List<IPAddress?>
            {
                IPAddress.Parse("127.0.0.1"),
                null,
                IPAddress.Parse("0.0.0.0"),
                IPAddress.Parse("255.255.255.255"),
                IPAddress.Parse("192.168.1.156"),
                IPAddress.Parse("::1"),
                IPAddress.Parse("::"),
                IPAddress.Parse("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"),
            },
            """{"Prop":["127.0.0.1",null,"0.0.0.0","255.255.255.255","192.168.1.156","::1","::","2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_physical_address_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.PhysicalAddress)),
            new List<PhysicalAddress?>
            {
                PhysicalAddress.None,
                null,
                PhysicalAddress.Parse("001122334455"),
                PhysicalAddress.Parse("00-11-22-33-44-55"),
                PhysicalAddress.Parse("0011.2233.4455")
            },
            """{"Prop":["",null,"001122334455","001122334455","001122334455"]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_sbyte_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Enum8)),
            new List<Enum8?>
            {
                Enum8.Min,
                null,
                Enum8.Max,
                Enum8.Default,
                Enum8.One,
                (Enum8)(-8)
            },
            """{"Prop":[-128,null,127,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_short_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Enum16)),
            new List<Enum16?>
            {
                Enum16.Min,
                null,
                Enum16.Max,
                Enum16.Default,
                Enum16.One,
                (Enum16)(-8)
            },
            """{"Prop":[-32768,null,32767,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_int_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Enum32)),
            new List<Enum32?>
            {
                Enum32.Min,
                null,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            },
            """{"Prop":[-2147483648,null,2147483647,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_long_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Enum64)),
            new List<Enum64?>
            {
                Enum64.Min,
                null,
                Enum64.Max,
                Enum64.Default,
                Enum64.One,
                (Enum64)(-8)
            },
            """{"Prop":[-9223372036854775808,null,9223372036854775807,0,1,-8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_byte_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.EnumU8)),
            new List<EnumU8?>
            {
                EnumU8.Min,
                null,
                EnumU8.Max,
                EnumU8.Default,
                EnumU8.One,
                (EnumU8)8
            },
            """{"Prop":[0,null,255,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_ushort_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.EnumU16)),
            new List<EnumU16?>
            {
                EnumU16.Min,
                null,
                EnumU16.Max,
                EnumU16.Default,
                EnumU16.One,
                (EnumU16)8
            },
            """{"Prop":[0,null,65535,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_uint_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.EnumU32)),
            new List<EnumU32?>
            {
                EnumU32.Min,
                null,
                EnumU32.Max,
                EnumU32.Default,
                EnumU32.One,
                (EnumU32)8
            },
            """{"Prop":[0,null,4294967295,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.EnumU64)),
            new List<EnumU64?>
            {
                EnumU64.Min,
                null,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":[0,null,18446744073709551615,0,1,8]}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_sbyte_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Int8Converted)),
            new sbyte[] { sbyte.MinValue, 0, sbyte.MaxValue },
            """{"Prop":"[-128,0,127]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_int_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Int32Converted)),
            new List<int>
            {
                int.MinValue,
                0,
                int.MaxValue
            },
            """{"Prop":"[-2147483648,0,2147483647]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_ulong_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.UInt64Converted)),
            new ObservableCollection<ulong>
            {
                ulong.MinValue,
                1,
                ulong.MaxValue
            },
            """{"Prop":"[0,1,18446744073709551615]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_double_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.DoubleConverted)),
            new[] { double.MinValue, 0, double.MaxValue },
            """{"Prop":"[-1.7976931348623157E\u002B308,0,1.7976931348623157E\u002B308]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_DateOnly_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.DateOnlyConverted)),
            new List<DateOnly>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue
            },
            """{"Prop":"[\u00220001-01-01\u0022,\u00222023-05-29\u0022,\u00229999-12-31\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_DateTime_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.DateTimeConverted)),
            new List<DateTime>
            {
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":"[\u00220001-01-01T00:00:00\u0022,\u00222023-05-29T10:52:47\u0022,\u00229999-12-31T23:59:59.9999999\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_bool_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.BooleanConverted)),
            new List<bool> { false, true },
            """{"Prop":"[false,true]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_char_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.CharacterConverted)),
            new List<char>
            {
                char.MinValue,
                'X',
                char.MaxValue
            },
            """{"Prop":"[\u0022\\u0000\u0022,\u0022X\u0022,\u0022\\uFFFF\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_string_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.StringConverted)),
            new List<string>
            {
                "MinValue",
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            },
            """{"Prop":"[\u0022MinValue\u0022,\u0022\\u2764\\u2765\\uC6C3\\uC720\\u264B\\u262E\\u270C\\u260F\\u2622\\u2620\\u2714\\u2611\\u265A\\u25B2\\u266A\\u0E3F\\u0189\\u26CF\\u2665\\u2763\\u2642\\u2640\\u263F\\uD83D\\uDC4D\\u270D\\u2709\\u2623\\u2624\\u2718\\u2612\\u265B\\u25BC\\u266B\\u2318\\u231B\\u00A1\\u2661\\u10E6\\u30C4\\u263C\\u2601\\u2745\\u267E\\uFE0F\\u270E\\u00A9\\u00AE\\u2122\\u03A3\\u272A\\u272F\\u262D\\u27B3\\u24B6\\u271E\\u2103\\u2109\\u00B0\\u273F\\u26A1\\u2603\\u2602\\u2704\\u00A2\\u20AC\\u00A3\\u221E\\u272B\\u2605\\u00BD\\u262F\\u2721\\u262A\u0022,\u0022MaxValue\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_binary_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.BytesConverted)),
            new List<byte[]>
            {
                new byte[] { 0, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":"[\u0022AAAAAQ==\u0022,\u0022/////w==\u0022,\u0022\u0022,\u0022AQIDBA==\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_int_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.Enum32Converted)),
            new List<Enum32>
            {
                Enum32.Min,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            },
            """{"Prop":"[-2147483648,2147483647,0,1,-8]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_ulong_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypeCollections>().GetProperty(nameof(PrimitiveTypeCollections.EnumU64Converted)),
            new List<EnumU64>
            {
                EnumU64.Min,
                EnumU64.Max,
                EnumU64.Default,
                EnumU64.One,
                (EnumU64)8
            },
            """{"Prop":"[0,18446744073709551615,0,1,8]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_sbyte_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Int8Converted)),
            new sbyte?[] { null, sbyte.MinValue, 0, sbyte.MaxValue },
            """{"Prop":"[null,-128,0,127]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_int_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Int32Converted)),
            new List<int?>
            {
                int.MinValue,
                0,
                null,
                int.MaxValue
            },
            """{"Prop":"[-2147483648,0,null,2147483647]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_ulong_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.UInt64Converted)),
            new ObservableCollection<ulong?>
            {
                ulong.MinValue,
                1,
                ulong.MaxValue,
                null
            },
            """{"Prop":"[0,1,18446744073709551615,null]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_double_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.DoubleConverted)),
            new double?[] { double.MinValue, null, 0, double.MaxValue },
            """{"Prop":"[-1.7976931348623157E\u002B308,null,0,1.7976931348623157E\u002B308]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_DateOnly_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.DateOnlyConverted)),
            new List<DateOnly?>
            {
                DateOnly.MinValue,
                new(2023, 5, 29),
                DateOnly.MaxValue,
                null
            },
            """{"Prop":"[\u00220001-01-01\u0022,\u00222023-05-29\u0022,\u00229999-12-31\u0022,null]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_DateTime_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.DateTimeConverted)),
            new List<DateTime?>
            {
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":"[\u00220001-01-01T00:00:00\u0022,null,\u00222023-05-29T10:52:47\u0022,\u00229999-12-31T23:59:59.9999999\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_bool_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.BooleanConverted)),
            new List<bool?>
            {
                false,
                null,
                true
            },
            """{"Prop":"[false,null,true]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_char_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.CharacterConverted)),
            new List<char?>
            {
                char.MinValue,
                'X',
                char.MaxValue,
                null
            },
            """{"Prop":"[\u0022\\u0000\u0022,\u0022X\u0022,\u0022\\uFFFF\u0022,null]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_string_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.StringConverted)),
            new List<string?>
            {
                "MinValue",
                null,
                "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
                "MaxValue"
            },
            """{"Prop":"[\u0022MinValue\u0022,null,\u0022\\u2764\\u2765\\uC6C3\\uC720\\u264B\\u262E\\u270C\\u260F\\u2622\\u2620\\u2714\\u2611\\u265A\\u25B2\\u266A\\u0E3F\\u0189\\u26CF\\u2665\\u2763\\u2642\\u2640\\u263F\\uD83D\\uDC4D\\u270D\\u2709\\u2623\\u2624\\u2718\\u2612\\u265B\\u25BC\\u266B\\u2318\\u231B\\u00A1\\u2661\\u10E6\\u30C4\\u263C\\u2601\\u2745\\u267E\\uFE0F\\u270E\\u00A9\\u00AE\\u2122\\u03A3\\u272A\\u272F\\u262D\\u27B3\\u24B6\\u271E\\u2103\\u2109\\u00B0\\u273F\\u26A1\\u2603\\u2602\\u2704\\u00A2\\u20AC\\u00A3\\u221E\\u272B\\u2605\\u00BD\\u262F\\u2721\\u262A\u0022,\u0022MaxValue\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_binary_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.BytesConverted)),
            new List<byte[]?>
            {
                new byte[] { 0, 0, 0, 1 },
                null,
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":"[\u0022AAAAAQ==\u0022,null,\u0022/////w==\u0022,\u0022\u0022,\u0022AQIDBA==\u0022]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_int_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.Enum32Converted)),
            new List<Enum32?>
            {
                Enum32.Min,
                null,
                Enum32.Max,
                Enum32.Default,
                Enum32.One,
                (Enum32)(-8)
            },
            """{"Prop":"[-2147483648,null,2147483647,0,1,-8]"}""");

    [ConditionalFact]
    public virtual void Can_read_write_collection_of_nullable_ulong_enum_values_with_converter_as_JSON_string()
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypeCollections>().GetProperty(nameof(NullablePrimitiveTypeCollections.EnumU64Converted)),
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

    protected virtual void Can_read_and_write_JSON_value<TModel>(
        IProperty property,
        TModel value,
        string json,
        object? existingObject = null)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var jsonReaderWriter = property.GetJsonValueReaderWriter()
            ?? property.GetTypeMapping().JsonValueReaderWriter!;

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

        var actual = Encoding.UTF8.GetString(buffer);

        Assert.Equal(json, actual);

        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(buffer));
        readerManager.MoveNext();
        readerManager.MoveNext();
        readerManager.MoveNext();

        if (readerManager.CurrentReader.TokenType == JsonTokenType.Null)
        {
            Assert.Null(value);
        }
        else
        {
            var fromJson = jsonReaderWriter.FromJson(ref readerManager, existingObject);
            if (existingObject != null)
            {
                Assert.Same(fromJson, existingObject);
            }

            Assert.Equal(value, fromJson);
        }
    }

    protected class Types
    {
        public sbyte Int8 { get; set; }
        public short Int16 { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }

        public byte UInt8 { get; set; }
        public ushort UInt16 { get; set; }
        public uint UInt32 { get; set; }
        public ulong UInt64 { get; set; }

        public float Float { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }

        public DateTime DateTime { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }

        public Guid Guid { get; set; }
        public string String { get; set; } = null!;
        public byte[] Bytes { get; set; } = null!;

        public bool Boolean { get; set; }
        public char Character { get; set; }

        public Uri Uri { get; set; } = null!;
        public PhysicalAddress PhysicalAddress { get; set; } = null!;
        public IPAddress IpAddress { get; set; } = null!;

        public Enum8 Enum8 { get; set; }
        public Enum16 Enum16 { get; set; }
        public Enum32 Enum32 { get; set; }
        public Enum64 Enum64 { get; set; }

        public EnumU8 EnumU8 { get; set; }
        public EnumU16 EnumU16 { get; set; }
        public EnumU32 EnumU32 { get; set; }
        public EnumU64 EnumU64 { get; set; }

        public DddId DddId { get; set; }
    }

    protected class NullableTypes
    {
        public sbyte? Int8 { get; set; }
        public short? Int16 { get; set; }
        public int? Int32 { get; set; }
        public long? Int64 { get; set; }

        public byte? UInt8 { get; set; }
        public ushort? UInt16 { get; set; }
        public uint? UInt32 { get; set; }
        public ulong? UInt64 { get; set; }

        public float? Float { get; set; }
        public double? Double { get; set; }
        public decimal? Decimal { get; set; }

        public DateTime? DateTime { get; set; }
        public DateTimeOffset? DateTimeOffset { get; set; }
        public TimeSpan? TimeSpan { get; set; }
        public DateOnly? DateOnly { get; set; }
        public TimeOnly? TimeOnly { get; set; }

        public Guid? Guid { get; set; }
        public string? String { get; set; }
        public byte[]? Bytes { get; set; }

        public bool? Boolean { get; set; }
        public char? Character { get; set; }

        public Uri? Uri { get; set; }
        public PhysicalAddress? PhysicalAddress { get; set; }
        public IPAddress? IpAddress { get; set; }

        public Enum8? Enum8 { get; set; }
        public Enum16? Enum16 { get; set; }
        public Enum32? Enum32 { get; set; }
        public Enum64? Enum64 { get; set; }

        public EnumU8? EnumU8 { get; set; }
        public EnumU16? EnumU16 { get; set; }
        public EnumU32? EnumU32 { get; set; }
        public EnumU64? EnumU64 { get; set; }

        public DddId? DddId { get; set; }
    }

    protected class TypesAsStrings
    {
        public sbyte? Int8 { get; set; }
        public short? Int16 { get; set; }
        public int? Int32 { get; set; }
        public long? Int64 { get; set; }

        public byte? UInt8 { get; set; }
        public ushort? UInt16 { get; set; }
        public uint? UInt32 { get; set; }
        public ulong? UInt64 { get; set; }

        public float? Float { get; set; }
        public double? Double { get; set; }
        public decimal? Decimal { get; set; }

        public DateTime? DateTime { get; set; }
        public DateTimeOffset? DateTimeOffset { get; set; }
        public TimeSpan? TimeSpan { get; set; }
        public DateOnly? DateOnly { get; set; }
        public TimeOnly? TimeOnly { get; set; }

        public Guid? Guid { get; set; }
        public string? String { get; set; }
        public byte[]? Bytes { get; set; }

        public bool? Boolean { get; set; }
        public char? Character { get; set; }

        public Uri? Uri { get; set; }
        public PhysicalAddress? PhysicalAddress { get; set; }
        public IPAddress? IpAddress { get; set; }

        public Enum8? Enum8 { get; set; }
        public Enum16? Enum16 { get; set; }
        public Enum32? Enum32 { get; set; }
        public Enum64? Enum64 { get; set; }

        public EnumU8? EnumU8 { get; set; }
        public EnumU16? EnumU16 { get; set; }
        public EnumU32? EnumU32 { get; set; }
        public EnumU64? EnumU64 { get; set; }
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

    public class GeometryTypes
    {
        public int Id { get; set; }
        public Geometry? Geometry { get; set; }
        public Point? Point { get; set; }
        public Point PointZ { get; set; } = null!;
        public Point PointM { get; set; } = null!;
        public Point PointZM { get; set; } = null!;
        public Polygon? Polygon { get; set; }
        public MultiLineString? MultiLineString { get; set; }
        public LineString? LineString { get; set; }
    }

    public class GeometryTypesAsGeoJson
    {
        public int Id { get; set; }
        public Geometry? Geometry { get; set; }
        public Point? Point { get; set; }
        public Point PointZ { get; set; } = null!;
        public Point PointM { get; set; } = null!;
        public Point PointZM { get; set; } = null!;
        public Polygon? Polygon { get; set; }
        public MultiLineString? MultiLineString { get; set; }
        public LineString? LineString { get; set; }
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

    protected class PrimitiveTypeCollections
    {
        public sbyte[] Int8 { get; set; } = null!;
        public IList<short> Int16 { get; set; } = null!;
        public List<int> Int32 { get; set; } = null!;
        public IList<long> Int64 { get; set; } = null!;

        public List<byte> UInt8 { get; set; } = null!;
        public Collection<ushort> UInt16 { get; set; } = null!;
        public List<uint> UInt32 { get; set; } = null!;
        public ObservableCollection<ulong> UInt64 { get; set; } = null!;

        public float[] Float { get; set; } = null!;
        public double[] Double { get; set; } = null!;
        public decimal[] Decimal { get; set; } = null!;

        public IList<DateTime> DateTime { get; set; } = null!;
        public IList<DateTimeOffset> DateTimeOffset { get; set; } = null!;
        public IList<TimeSpan> TimeSpan { get; set; } = null!;
        public IList<DateOnly> DateOnly { get; set; } = null!;
        public IList<TimeOnly> TimeOnly { get; set; } = null!;

        public IList<Guid> Guid { get; set; } = null!;
        public IList<string> String { get; set; } = null!;
        public IList<byte[]> Bytes { get; set; } = null!;

        public IList<bool> Boolean { get; set; } = null!;
        public IList<char> Character { get; set; } = null!;

        public List<Uri> Uri { get; set; } = null!;
        public List<PhysicalAddress> PhysicalAddress { get; set; } = null!;
        public List<IPAddress> IpAddress { get; set; } = null!;

        public List<Enum8> Enum8 { get; set; } = null!;
        public List<Enum16> Enum16 { get; set; } = null!;
        public List<Enum32> Enum32 { get; set; } = null!;
        public List<Enum64> Enum64 { get; set; } = null!;

        public IList<EnumU8> EnumU8 { get; set; } = null!;
        public IList<EnumU16> EnumU16 { get; set; } = null!;
        public IList<EnumU32> EnumU32 { get; set; } = null!;
        public IList<EnumU64> EnumU64 { get; set; } = null!;

        public sbyte[] Int8Converted { get; set; } = null!;
        public List<int> Int32Converted { get; set; } = null!;
        public ObservableCollection<ulong> UInt64Converted { get; set; } = null!;
        public double[] DoubleConverted { get; set; } = null!;
        public IList<DateTime> DateTimeConverted { get; set; } = null!;
        public IList<DateOnly> DateOnlyConverted { get; set; } = null!;
        public IList<string> StringConverted { get; set; } = null!;
        public IList<byte[]> BytesConverted { get; set; } = null!;
        public IList<bool> BooleanConverted { get; set; } = null!;
        public IList<char> CharacterConverted { get; set; } = null!;
        public List<Enum32> Enum32Converted { get; set; } = null!;
        public IList<EnumU64> EnumU64Converted { get; set; } = null!;

        //public IList<DddId> DddId { get; set; } = null!; // TODO Custom collection element
    }

    protected class NullablePrimitiveTypeCollections
    {
        public sbyte?[] Int8 { get; set; } = null!;
        public IList<short?> Int16 { get; set; } = null!;
        public List<int?> Int32 { get; set; } = null!;
        public IList<long?> Int64 { get; set; } = null!;

        public List<byte?> UInt8 { get; set; } = null!;
        public Collection<ushort?> UInt16 { get; set; } = null!;
        public List<uint?> UInt32 { get; set; } = null!;
        public ObservableCollection<ulong?> UInt64 { get; set; } = null!;

        public float?[] Float { get; set; } = null!;
        public double?[] Double { get; set; } = null!;
        public decimal?[] Decimal { get; set; } = null!;

        public IList<DateTime?> DateTime { get; set; } = null!;
        public IList<DateTimeOffset?> DateTimeOffset { get; set; } = null!;
        public IList<TimeSpan?> TimeSpan { get; set; } = null!;
        public IList<DateOnly?> DateOnly { get; set; } = null!;
        public IList<TimeOnly?> TimeOnly { get; set; } = null!;

        public IList<Guid?> Guid { get; set; } = null!;
        public IList<string?> String { get; set; } = null!;
        public IList<byte[]?> Bytes { get; set; } = null!;

        public IList<bool?> Boolean { get; set; } = null!;
        public IList<char?> Character { get; set; } = null!;

        public List<Uri?> Uri { get; set; } = null!;
        public List<PhysicalAddress?> PhysicalAddress { get; set; } = null!;
        public List<IPAddress?> IpAddress { get; set; } = null!;

        public List<Enum8?> Enum8 { get; set; } = null!;
        public List<Enum16?> Enum16 { get; set; } = null!;
        public List<Enum32?> Enum32 { get; set; } = null!;
        public List<Enum64?> Enum64 { get; set; } = null!;

        public IList<EnumU8?> EnumU8 { get; set; } = null!;
        public IList<EnumU16?> EnumU16 { get; set; } = null!;
        public IList<EnumU32?> EnumU32 { get; set; } = null!;
        public IList<EnumU64?> EnumU64 { get; set; } = null!;

        public sbyte?[] Int8Converted { get; set; } = null!;
        public List<int?> Int32Converted { get; set; } = null!;
        public ObservableCollection<ulong?> UInt64Converted { get; set; } = null!;
        public double?[] DoubleConverted { get; set; } = null!;
        public IList<DateTime?> DateTimeConverted { get; set; } = null!;
        public IList<DateOnly?> DateOnlyConverted { get; set; } = null!;
        public IList<string?> StringConverted { get; set; } = null!;
        public IList<byte[]?> BytesConverted { get; set; } = null!;
        public IList<bool?> BooleanConverted { get; set; } = null!;
        public IList<char?> CharacterConverted { get; set; } = null!;
        public List<Enum32?> Enum32Converted { get; set; } = null!;
        public IList<EnumU64?> EnumU64Converted { get; set; } = null!;

        //public IList<DddId?> DddId { get; set; } = null!; // TODO Custom collection element
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
            var builder = new StringBuilder("{");
            var depth = 1;
            var comma = false;
            while (depth > 0)
            {
                manager.MoveNext();

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
            }

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

    public abstract class JsonTypesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        private DbContext? _staticContext;

        protected override string StoreName
            => "JsonTypes";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w => w.Ignore(
                    CoreEventId.MappedEntityTypeIgnoredWarning,
                    CoreEventId.MappedPropertyIgnoredWarning,
                    CoreEventId.MappedNavigationIgnoredWarning));

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configurationBuilder.Properties<DddId>().HaveConversion<DddIdConverter>();

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Types>(
                b =>
                {
                    b.HasNoKey();
                });

            modelBuilder.Entity<NullableTypes>(
                b =>
                {
                    b.HasNoKey();
                });

            modelBuilder.Entity<TypesAsStrings>(
                b =>
                {
                    b.HasNoKey();
                    b.Property(e => e.Int8).HasConversion<string?>();
                    b.Property(e => e.Int16).HasConversion<string?>();
                    b.Property(e => e.Int32).HasConversion<string?>();
                    b.Property(e => e.Int64).HasConversion<string?>();
                    b.Property(e => e.UInt8).HasConversion<string?>();
                    b.Property(e => e.UInt16).HasConversion<string?>();
                    b.Property(e => e.UInt32).HasConversion<string?>();
                    b.Property(e => e.UInt64).HasConversion<string?>();
                    b.Property(e => e.Float).HasConversion<string?>();
                    b.Property(e => e.Double).HasConversion<string?>();
                    b.Property(e => e.Decimal).HasConversion<string?>();
                    b.Property(e => e.DateTime).HasConversion<string?>();
                    b.Property(e => e.DateTimeOffset).HasConversion<string?>();
                    b.Property(e => e.TimeSpan).HasConversion<string?>();
                    b.Property(e => e.DateOnly).HasConversion<string?>();
                    b.Property(e => e.TimeOnly).HasConversion<string?>();
                    b.Property(e => e.Guid).HasConversion<string?>();
                    b.Property(e => e.String).HasConversion<string?>();
                    b.Property(e => e.Bytes).HasConversion<string?>();
                    b.Property(e => e.Boolean).HasConversion<string?>();
                    b.Property(e => e.Character).HasConversion<string?>();
                    b.Property(e => e.Uri).HasConversion<string?>();
                    b.Property(e => e.PhysicalAddress).HasConversion<string?>();
                    b.Property(e => e.IpAddress).HasConversion<string?>();
                    b.Property(e => e.Enum8).HasConversion<string?>();
                    b.Property(e => e.Enum16).HasConversion<string?>();
                    b.Property(e => e.Enum32).HasConversion<string?>();
                    b.Property(e => e.Enum64).HasConversion<string?>();
                    b.Property(e => e.EnumU8).HasConversion<string?>();
                    b.Property(e => e.EnumU16).HasConversion<string?>();
                    b.Property(e => e.EnumU32).HasConversion<string?>();
                    b.Property(e => e.EnumU64).HasConversion<string?>();
                });

            modelBuilder.Entity<GeometryTypes>().HasNoKey();

            modelBuilder.Entity<GeometryTypesAsGeoJson>(
                b =>
                {
                    b.Property(e => e.Point).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.PointZ).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.PointM).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.PointZM).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.Geometry).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.LineString).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.MultiLineString).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.Polygon).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                });

            modelBuilder.Entity<PrimitiveTypeCollections>(
                b =>
                {
                    b.HasNoKey();
                    b.Property(e => e.Int8Converted)
                        .HasConversion<CustomCollectionConverter<sbyte[], sbyte>, CustomCollectionComparer<sbyte[], sbyte>>();
                    b.Property(e => e.Int32Converted)
                        .HasConversion<CustomCollectionConverter<List<int>, int>, CustomCollectionComparer<List<int>, int>>();
                    b.Property(e => e.UInt64Converted)
                        .HasConversion<CustomCollectionConverter<ObservableCollection<ulong>, ulong>,
                            CustomCollectionComparer<ObservableCollection<ulong>, ulong>>();
                    b.Property(e => e.DoubleConverted)
                        .HasConversion<CustomCollectionConverter<double[], double>, CustomCollectionComparer<double[], double>>();
                    b.Property(e => e.DateTimeConverted)
                        .HasConversion<CustomCollectionConverter<IList<DateTime>, DateTime>,
                            CustomCollectionComparer<IList<DateTime>, DateTime>>();
                    b.Property(e => e.DateOnlyConverted)
                        .HasConversion<CustomCollectionConverter<IList<DateOnly>, DateOnly>,
                            CustomCollectionComparer<IList<DateOnly>, DateOnly>>();
                    b.Property(e => e.StringConverted)
                        .HasConversion<CustomCollectionConverter<IList<string>, string>, CustomCollectionComparer<IList<string>, string>>();
                    b.Property(e => e.BytesConverted)
                        .HasConversion<CustomCollectionConverter<IList<byte[]>, byte[]>, CustomCollectionComparer<IList<byte[]>, byte[]>>();
                    b.Property(e => e.BooleanConverted)
                        .HasConversion<CustomCollectionConverter<IList<bool>, bool>, CustomCollectionComparer<IList<bool>, bool>>();
                    b.Property(e => e.CharacterConverted)
                        .HasConversion<CustomCollectionConverter<IList<char>, char>, CustomCollectionComparer<IList<char>, char>>();
                    b.Property(e => e.Enum32Converted)
                        .HasConversion<CustomCollectionConverter<List<Enum32>, Enum32>, CustomCollectionComparer<List<Enum32>, Enum32>>();
                    b.Property(e => e.EnumU64Converted)
                        .HasConversion<CustomCollectionConverter<IList<EnumU64>, EnumU64>,
                            CustomCollectionComparer<IList<EnumU64>, EnumU64>>();
                });

            modelBuilder.Entity<NullablePrimitiveTypeCollections>(
                b =>
                {
                    b.HasNoKey();
                    b.Property(e => e.Int8Converted)
                        .HasConversion<CustomCollectionConverter<sbyte?[], sbyte?>, CustomCollectionComparer<sbyte?[], sbyte?>>();
                    b.Property(e => e.Int32Converted)
                        .HasConversion<CustomCollectionConverter<List<int?>, int?>, CustomCollectionComparer<List<int?>, int?>>();
                    b.Property(e => e.UInt64Converted)
                        .HasConversion<CustomCollectionConverter<ObservableCollection<ulong?>, ulong?>,
                            CustomCollectionComparer<ObservableCollection<ulong?>, ulong?>>();
                    b.Property(e => e.DoubleConverted)
                        .HasConversion<CustomCollectionConverter<double?[], double?>, CustomCollectionComparer<double?[], double?>>();
                    b.Property(e => e.DateTimeConverted)
                        .HasConversion<CustomCollectionConverter<IList<DateTime?>, DateTime?>,
                            CustomCollectionComparer<IList<DateTime?>, DateTime?>>();
                    b.Property(e => e.DateOnlyConverted)
                        .HasConversion<CustomCollectionConverter<IList<DateOnly?>, DateOnly?>,
                            CustomCollectionComparer<IList<DateOnly?>, DateOnly?>>();
                    b.Property(e => e.StringConverted)
                        .HasConversion<CustomCollectionConverter<IList<string?>, string?>,
                            CustomCollectionComparer<IList<string?>, string?>>();
                    b.Property(e => e.BytesConverted)
                        .HasConversion<CustomCollectionConverter<IList<byte[]?>, byte[]?>,
                            CustomCollectionComparer<IList<byte[]?>, byte[]?>>();
                    b.Property(e => e.BooleanConverted)
                        .HasConversion<CustomCollectionConverter<IList<bool?>, bool?>, CustomCollectionComparer<IList<bool?>, bool?>>();
                    b.Property(e => e.CharacterConverted)
                        .HasConversion<CustomCollectionConverter<IList<char?>, char?>, CustomCollectionComparer<IList<char?>, char?>>();
                    b.Property(e => e.Enum32Converted)
                        .HasConversion<CustomCollectionConverter<List<Enum32?>, Enum32?>,
                            CustomCollectionComparer<List<Enum32?>, Enum32?>>();
                    b.Property(e => e.EnumU64Converted)
                        .HasConversion<CustomCollectionConverter<IList<EnumU64?>, EnumU64?>,
                            CustomCollectionComparer<IList<EnumU64?>, EnumU64?>>();
                });
        }

        public DbContext StaticContext
            => _staticContext ??= CreateContext();

        public IEntityType EntityType<TEntity>()
            => StaticContext.Model.FindEntityType(typeof(TEntity))!;
    }
}
