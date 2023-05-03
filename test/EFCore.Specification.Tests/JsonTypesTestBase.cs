// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

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
    [InlineData(sbyte.MinValue, "{\"Prop\":-128}")]
    [InlineData(sbyte.MaxValue, "{\"Prop\":127}")]
    [InlineData((sbyte)0, "{\"Prop\":0}")]
    [InlineData((sbyte)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_sbyte_JSON_values(sbyte value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Int8)), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, "{\"Prop\":-32768}")]
    [InlineData(short.MaxValue, "{\"Prop\":32767}")]
    [InlineData((short)0, "{\"Prop\":0}")]
    [InlineData((short)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_short_JSON_values(short value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Int16)), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, "{\"Prop\":-2147483648}")]
    [InlineData(int.MaxValue, "{\"Prop\":2147483647}")]
    [InlineData(0, "{\"Prop\":0}")]
    [InlineData(1, "{\"Prop\":1}")]
    public virtual void Can_read_write_int_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Int32)), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, "{\"Prop\":-9223372036854775808}")]
    [InlineData(long.MaxValue, "{\"Prop\":9223372036854775807}")]
    [InlineData((long)0, "{\"Prop\":0}")]
    [InlineData((long)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_long_JSON_values(long value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Int64)), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, "{\"Prop\":0}")]
    [InlineData(byte.MaxValue, "{\"Prop\":255}")]
    //[InlineData((byte)0, "{\"Prop\":0}")]
    [InlineData((byte)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_byte_JSON_values(byte value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.UInt8)), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, "{\"Prop\":0}")]
    [InlineData(ushort.MaxValue, "{\"Prop\":65535}")]
    //[InlineData((ushort)0, "{\"Prop\":0}")]
    [InlineData((ushort)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_ushort_JSON_values(ushort value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.UInt16)), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, "{\"Prop\":0}")]
    [InlineData(uint.MaxValue, "{\"Prop\":4294967295}")]
    //[InlineData((uint)0, "{\"Prop\":0}")]
    [InlineData((uint)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_uint_JSON_values(uint value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.UInt32)), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, "{\"Prop\":0}")]
    [InlineData(ulong.MaxValue, "{\"Prop\":18446744073709551615}")]
    //[InlineData((ulong)0, "{\"Prop\":0}")]
    [InlineData((ulong)1, "{\"Prop\":1}")]
    public virtual void Can_read_write_ulong_JSON_values(ulong value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.UInt64)), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, "{\"Prop\":-3.4028235E+38}")]
    [InlineData(float.MaxValue, "{\"Prop\":3.4028235E+38}")]
    [InlineData((float)0.0, "{\"Prop\":0}")]
    [InlineData((float)1.1, "{\"Prop\":1.1}")]
    public virtual void Can_read_write_float_JSON_values(float value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Float)), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, "{\"Prop\":-1.7976931348623157E+308}")]
    [InlineData(double.MaxValue, "{\"Prop\":1.7976931348623157E+308}")]
    [InlineData(0.0, "{\"Prop\":0}")]
    [InlineData(1.1, "{\"Prop\":1.1}")]
    public virtual void Can_read_write_double_JSON_values(double value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Double)), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", "{\"Prop\":-79228162514264337593543950335}")]
    [InlineData("79228162514264337593543950335", "{\"Prop\":79228162514264337593543950335}")]
    [InlineData("0.0", "{\"Prop\":0.0}")]
    [InlineData("1.1", "{\"Prop\":1.1}")]
    public virtual void Can_read_write_decimal_JSON_values(decimal value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Decimal)), value, json);

    [ConditionalTheory]
    [InlineData("1/1/0001", "{\"Prop\":\"0001-01-01\"}")]
    [InlineData("12/31/9999", "{\"Prop\":\"9999-12-31\"}")]
    //[InlineData("1/1/0001", "{\"Prop\":\"0001-01-01\"}")]
    [InlineData("5/29/2023", "{\"Prop\":\"2023-05-29\"}")]
    public virtual void Can_read_write_DateOnly_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.DateOnly)),
            DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", "{\"Prop\":\"00:00:00.0000000\"}")]
    [InlineData("23:59:59.9999999", "{\"Prop\":\"23:59:59.9999999\"}")]
    //[InlineData("00:00:00.0000000", "{\"Prop\":\"00:00:00.0000000\"}")]
    [InlineData("11:05:12.3456789", "{\"Prop\":\"11:05:12.3456789\"}")]
    public virtual void Can_read_write_TimeOnly_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.TimeOnly)),
            TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", "{\"Prop\":\"0001-01-01T00:00:00\"}")]
    [InlineData("9999-12-31T23:59:59.9999999", "{\"Prop\":\"9999-12-31T23:59:59.9999999\"}")]
    //[InlineData("0001-01-01T00:00:00.0000000", "{\"Prop\":\"0001-01-01T00:00:00\"}")]
    [InlineData("2023-05-29T10:52:47.2064353", "{\"Prop\":\"2023-05-29T10:52:47.2064353\"}")]
    public virtual void Can_read_write_DateTime_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.DateTime)),
            DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", "{\"Prop\":\"0001-01-01T00:00:00-01:00\"}")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", "{\"Prop\":\"9999-12-31T23:59:59.9999999+02:00\"}")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", "{\"Prop\":\"0001-01-01T00:00:00-03:00\"}")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", "{\"Prop\":\"2023-05-29T11:11:15.5672854+04:00\"}")]
    public virtual void Can_read_write_DateTimeOffset_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.DateTimeOffset)),
            DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", "{\"Prop\":\"-10675199:2:48:05.4775808\"}")]
    [InlineData("10675199.02:48:05.4775807", "{\"Prop\":\"10675199:2:48:05.4775807\"}")]
    [InlineData("00:00:00", "{\"Prop\":\"0:00:00\"}")]
    [InlineData("12:23:23.8018854", "{\"Prop\":\"12:23:23.8018854\"}")]
    public virtual void Can_read_write_TimeSpan_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.TimeSpan)), TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, "{\"Prop\":false}")]
    [InlineData(true, "{\"Prop\":true}")]
    public virtual void Can_read_write_bool_JSON_values(bool value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Boolean)), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, "{\"Prop\":\"\\u0000\"}")]
    [InlineData(char.MaxValue, "{\"Prop\":\"\\uFFFF\"}")]
    [InlineData(' ', "{\"Prop\":\" \"}")]
    [InlineData("Z", "{\"Prop\":\"Z\"}")]
    public virtual void Can_read_write_char_JSON_values(char value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Character)), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", "{\"Prop\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", "{\"Prop\":\"ffffffff-ffff-ffff-ffff-ffffffffffff\"}")]
    //[InlineData("00000000-0000-0000-0000-000000000000", "{\"Prop\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", "{\"Prop\":\"8c44242f-8e3f-4a20-8be8-98c7c1aadebd\"}")]
    public virtual void Can_read_write_GUID_JSON_values(Guid value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Guid)), value, json);

    [ConditionalTheory]
    [InlineData("MinValue", "{\"Prop\":\"MinValue\"}")]
    [InlineData("MaxValue", "{\"Prop\":\"MaxValue\"}")]
    [InlineData("", "{\"Prop\":\"\"}")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    public virtual void Can_read_write_string_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.String)), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", "{\"Prop\":\"AAAAAQ==\"}")]
    [InlineData("255,255,255,255", "{\"Prop\":\"/////w==\"}")]
    [InlineData("", "{\"Prop\":\"\"}")]
    [InlineData("1,2,3,4", "{\"Prop\":\"AQIDBA==\"}")]
    public virtual void Can_read_write_binary_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Bytes)),
            value == "" ? Array.Empty<byte>() : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        "{\"Prop\":\"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName\"}")]
    [InlineData("file:///C:/test/path/file.txt", "{\"Prop\":\"file:///C:/test/path/file.txt\"}")]
    public virtual void Can_read_write_URI_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Uri)), new Uri(value), json);

    [ConditionalTheory]
    [InlineData("127.0.0.1", "{\"Prop\":\"127.0.0.1\"}")]
    [InlineData("0.0.0.0", "{\"Prop\":\"0.0.0.0\"}")]
    [InlineData("255.255.255.255", "{\"Prop\":\"255.255.255.255\"}")]
    [InlineData("192.168.1.156", "{\"Prop\":\"192.168.1.156\"}")]
    [InlineData("::1", "{\"Prop\":\"::1\"}")]
    [InlineData("::", "{\"Prop\":\"::\"}")]
    //[InlineData("::", "{\"Prop\":\"::\"}")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", "{\"Prop\":\"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577\"}")]
    public virtual void Can_read_write_IP_address_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.IpAddress)), IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", "{\"Prop\":\"001122334455\"}")]
    [InlineData("00-11-22-33-44-55", "{\"Prop\":\"001122334455\"}")]
    [InlineData("0011.2233.4455", "{\"Prop\":\"001122334455\"}")]
    public virtual void Can_read_write_physical_address_JSON_values(string value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.PhysicalAddress)), PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, "{\"Prop\":-128}")]
    [InlineData((sbyte)Enum8.Max, "{\"Prop\":127}")]
    [InlineData((sbyte)Enum8.Default, "{\"Prop\":0}")]
    [InlineData((sbyte)Enum8.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_sbyte_enum_JSON_values(Enum8 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Enum8)), value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, "{\"Prop\":-32768}")]
    [InlineData((short)Enum16.Max, "{\"Prop\":32767}")]
    [InlineData((short)Enum16.Default, "{\"Prop\":0}")]
    [InlineData((short)Enum16.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_short_enum_JSON_values(Enum16 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Enum16)), value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, "{\"Prop\":-2147483648}")]
    [InlineData((int)Enum32.Max, "{\"Prop\":2147483647}")]
    [InlineData((int)Enum32.Default, "{\"Prop\":0}")]
    [InlineData((int)Enum32.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_int_enum_JSON_values(Enum32 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Enum32)), value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, "{\"Prop\":-9223372036854775808}")]
    [InlineData((long)Enum64.Max, "{\"Prop\":9223372036854775807}")]
    [InlineData((long)Enum64.Default, "{\"Prop\":0}")]
    [InlineData((long)Enum64.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_long_enum_JSON_values(Enum64 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.Enum64)), value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, "{\"Prop\":0}")]
    [InlineData((byte)EnumU8.Max, "{\"Prop\":255}")]
    //[InlineData((byte)EnumU8.Default, "{\"Prop\":0}")]
    [InlineData((byte)EnumU8.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_byte_enum_JSON_values(EnumU8 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.EnumU8)), value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, "{\"Prop\":0}")]
    [InlineData((ushort)EnumU16.Max, "{\"Prop\":65535}")]
    //[InlineData((ushort)EnumU16.Default, "{\"Prop\":0}")]
    [InlineData((ushort)EnumU16.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_ushort_enum_JSON_values(EnumU16 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.EnumU16)), value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, "{\"Prop\":0}")]
    [InlineData((uint)EnumU32.Max, "{\"Prop\":4294967295}")]
    //[InlineData((uint)EnumU32.Default, "{\"Prop\":0}")]
    [InlineData((uint)EnumU32.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_uint_enum_JSON_values(EnumU32 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.EnumU32)), value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, "{\"Prop\":0}")]
    [InlineData((ulong)EnumU64.Max, "{\"Prop\":18446744073709551615}")]
    //[InlineData((ulong)EnumU64.Default, "{\"Prop\":0}")]
    [InlineData((ulong)EnumU64.One, "{\"Prop\":1}")]
    public virtual void Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
        => Can_read_and_write_JSON_value(Fixture.EntityType<PrimitiveTypes>().GetProperty(nameof(PrimitiveTypes.EnumU64)), value, json);

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, "{\"Prop\":-128}")]
    [InlineData(sbyte.MaxValue, "{\"Prop\":127}")]
    [InlineData((sbyte)0, "{\"Prop\":0}")]
    [InlineData((sbyte)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_sbyte_JSON_values(sbyte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Int8)), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, "{\"Prop\":-32768}")]
    [InlineData(short.MaxValue, "{\"Prop\":32767}")]
    [InlineData((short)0, "{\"Prop\":0}")]
    [InlineData((short)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_short_JSON_values(short? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Int16)), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, "{\"Prop\":-2147483648}")]
    [InlineData(int.MaxValue, "{\"Prop\":2147483647}")]
    [InlineData(0, "{\"Prop\":0}")]
    [InlineData(1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_int_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Int32)), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, "{\"Prop\":-9223372036854775808}")]
    [InlineData(long.MaxValue, "{\"Prop\":9223372036854775807}")]
    [InlineData((long)0, "{\"Prop\":0}")]
    [InlineData((long)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_long_JSON_values(long? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Int64)), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, "{\"Prop\":0}")]
    [InlineData(byte.MaxValue, "{\"Prop\":255}")]
    //[InlineData((byte)0, "{\"Prop\":0}")]
    [InlineData((byte)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_byte_JSON_values(byte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.UInt8)), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, "{\"Prop\":0}")]
    [InlineData(ushort.MaxValue, "{\"Prop\":65535}")]
    //[InlineData((ushort)0, "{\"Prop\":0}")]
    [InlineData((ushort)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ushort_JSON_values(ushort? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.UInt16)), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, "{\"Prop\":0}")]
    [InlineData(uint.MaxValue, "{\"Prop\":4294967295}")]
    //[InlineData((uint)0, "{\"Prop\":0}")]
    [InlineData((uint)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_uint_JSON_values(uint? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.UInt32)), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, "{\"Prop\":0}")]
    [InlineData(ulong.MaxValue, "{\"Prop\":18446744073709551615}")]
    //[InlineData((ulong)0, "{\"Prop\":0}")]
    [InlineData((ulong)1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ulong_JSON_values(ulong? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.UInt64)), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, "{\"Prop\":-3.4028235E+38}")]
    [InlineData(float.MaxValue, "{\"Prop\":3.4028235E+38}")]
    [InlineData((float)0.0, "{\"Prop\":0}")]
    [InlineData((float)1.1, "{\"Prop\":1.1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_float_JSON_values(float? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Float)), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, "{\"Prop\":-1.7976931348623157E+308}")]
    [InlineData(double.MaxValue, "{\"Prop\":1.7976931348623157E+308}")]
    [InlineData(0.0, "{\"Prop\":0}")]
    [InlineData(1.1, "{\"Prop\":1.1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_double_JSON_values(double? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Double)), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", "{\"Prop\":-79228162514264337593543950335}")]
    [InlineData("79228162514264337593543950335", "{\"Prop\":79228162514264337593543950335}")]
    [InlineData("0.0", "{\"Prop\":0.0}")]
    [InlineData("1.1", "{\"Prop\":1.1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_decimal_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Decimal)),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("1/1/0001", "{\"Prop\":\"0001-01-01\"}")]
    [InlineData("12/31/9999", "{\"Prop\":\"9999-12-31\"}")]
    //[InlineData("1/1/0001", "{\"Prop\":\"0001-01-01\"}")]
    [InlineData("5/29/2023", "{\"Prop\":\"2023-05-29\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_DateOnly_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.DateOnly)),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", "{\"Prop\":\"00:00:00.0000000\"}")]
    [InlineData("23:59:59.9999999", "{\"Prop\":\"23:59:59.9999999\"}")]
    //[InlineData("00:00:00.0000000", "{\"Prop\":\"00:00:00.0000000\"}")]
    [InlineData("11:05:12.3456789", "{\"Prop\":\"11:05:12.3456789\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_TimeOnly_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.TimeOnly)),
            value == null ? default(TimeOnly?) : TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", "{\"Prop\":\"0001-01-01T00:00:00\"}")]
    [InlineData("9999-12-31T23:59:59.9999999", "{\"Prop\":\"9999-12-31T23:59:59.9999999\"}")]
    //[InlineData("0001-01-01T00:00:00.0000000", "{\"Prop\":\"0001-01-01T00:00:00\"}")]
    [InlineData("2023-05-29T10:52:47.2064353", "{\"Prop\":\"2023-05-29T10:52:47.2064353\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.DateTime)),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", "{\"Prop\":\"0001-01-01T00:00:00-01:00\"}")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", "{\"Prop\":\"9999-12-31T23:59:59.9999999+02:00\"}")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", "{\"Prop\":\"0001-01-01T00:00:00-03:00\"}")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", "{\"Prop\":\"2023-05-29T11:11:15.5672854+04:00\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.DateTimeOffset)),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", "{\"Prop\":\"-10675199:2:48:05.4775808\"}")]
    [InlineData("10675199.02:48:05.4775807", "{\"Prop\":\"10675199:2:48:05.4775807\"}")]
    [InlineData("00:00:00", "{\"Prop\":\"0:00:00\"}")]
    [InlineData("12:23:23.8018854", "{\"Prop\":\"12:23:23.8018854\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_TimeSpan_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.TimeSpan)),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, "{\"Prop\":false}")]
    [InlineData(true, "{\"Prop\":true}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_bool_JSON_values(bool? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Boolean)), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, "{\"Prop\":\"\\u0000\"}")]
    [InlineData(char.MaxValue, "{\"Prop\":\"\\uFFFF\"}")]
    [InlineData(' ', "{\"Prop\":\" \"}")]
    [InlineData('Z', "{\"Prop\":\"Z\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_char_JSON_values(char? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Character)), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", "{\"Prop\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", "{\"Prop\":\"ffffffff-ffff-ffff-ffff-ffffffffffff\"}")]
    //[InlineData("00000000-0000-0000-0000-000000000000", "{\"Prop\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", "{\"Prop\":\"8c44242f-8e3f-4a20-8be8-98c7c1aadebd\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_GUID_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Guid)),
            value == null ? default(Guid?) : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("MinValue", "{\"Prop\":\"MinValue\"}")]
    [InlineData("MaxValue", "{\"Prop\":\"MaxValue\"}")]
    [InlineData("", "{\"Prop\":\"\"}")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.String)), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", "{\"Prop\":\"AAAAAQ==\"}")]
    [InlineData("255,255,255,255", "{\"Prop\":\"/////w==\"}")]
    [InlineData("", "{\"Prop\":\"\"}")]
    [InlineData("1,2,3,4", "{\"Prop\":\"AQIDBA==\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_binary_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Bytes)),
            value == null
                ? default
                : value == ""
                    ? Array.Empty<byte>()
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        "{\"Prop\":\"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName\"}")]
    [InlineData("file:///C:/test/path/file.txt", "{\"Prop\":\"file:///C:/test/path/file.txt\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_URI_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Uri)),
            value == null ? default : new Uri(value), json);

    [ConditionalTheory]
    [InlineData("127.0.0.1", "{\"Prop\":\"127.0.0.1\"}")]
    [InlineData("0.0.0.0", "{\"Prop\":\"0.0.0.0\"}")]
    [InlineData("255.255.255.255", "{\"Prop\":\"255.255.255.255\"}")]
    [InlineData("192.168.1.156", "{\"Prop\":\"192.168.1.156\"}")]
    [InlineData("::1", "{\"Prop\":\"::1\"}")]
    [InlineData("::", "{\"Prop\":\"::\"}")]
    //[InlineData("::", "{\"Prop\":\"::\"}")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", "{\"Prop\":\"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_IP_address_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.IpAddress)),
            value == null ? default : IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", "{\"Prop\":\"001122334455\"}")]
    [InlineData("00-11-22-33-44-55", "{\"Prop\":\"001122334455\"}")]
    [InlineData("0011.2233.4455", "{\"Prop\":\"001122334455\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_physical_address_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.PhysicalAddress)),
            value == null ? default : PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, "{\"Prop\":-128}")]
    [InlineData((sbyte)Enum8.Max, "{\"Prop\":127}")]
    [InlineData((sbyte)Enum8.Default, "{\"Prop\":0}")]
    [InlineData((sbyte)Enum8.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_sbyte_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Enum8)),
            value == null ? default(Enum8?) : (Enum8)value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, "{\"Prop\":-32768}")]
    [InlineData((short)Enum16.Max, "{\"Prop\":32767}")]
    [InlineData((short)Enum16.Default, "{\"Prop\":0}")]
    [InlineData((short)Enum16.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_short_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Enum16)),
            value == null ? default(Enum16?) : (Enum16)value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, "{\"Prop\":-2147483648}")]
    [InlineData((int)Enum32.Max, "{\"Prop\":2147483647}")]
    [InlineData((int)Enum32.Default, "{\"Prop\":0}")]
    [InlineData((int)Enum32.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_int_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Enum32)),
            value == null ? default(Enum32?) : (Enum32)value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, "{\"Prop\":-9223372036854775808}")]
    [InlineData((long)Enum64.Max, "{\"Prop\":9223372036854775807}")]
    [InlineData((long)Enum64.Default, "{\"Prop\":0}")]
    [InlineData((long)Enum64.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_long_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.Enum64)),
            value == null ? default(Enum64?) : (Enum64)value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, "{\"Prop\":0}")]
    [InlineData((byte)EnumU8.Max, "{\"Prop\":255}")]
    //[InlineData((byte)EnumU8.Default, "{\"Prop\":0}")]
    [InlineData((byte)EnumU8.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_byte_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.EnumU8)),
            value == null ? default(EnumU8?) : (EnumU8)value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, "{\"Prop\":0}")]
    [InlineData((ushort)EnumU16.Max, "{\"Prop\":65535}")]
    //[InlineData((ushort)EnumU16.Default, "{\"Prop\":0}")]
    [InlineData((ushort)EnumU16.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ushort_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.EnumU16)),
            value == null ? default(EnumU16?) : (EnumU16)value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, "{\"Prop\":0}")]
    [InlineData((uint)EnumU32.Max, "{\"Prop\":4294967295}")]
    //[InlineData((uint)EnumU32.Default, "{\"Prop\":0}")]
    [InlineData((uint)EnumU32.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_uint_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.EnumU32)),
            value == null ? default(EnumU32?) : (EnumU32)value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, "{\"Prop\":0}")]
    [InlineData((ulong)EnumU64.Max, "{\"Prop\":18446744073709551615}")]
    //[InlineData((ulong)EnumU64.Default, "{\"Prop\":0}")]
    [InlineData((ulong)EnumU64.One, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<NullablePrimitiveTypes>().GetProperty(nameof(NullablePrimitiveTypes.EnumU64)),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    [ConditionalTheory]
    [InlineData(sbyte.MinValue, "{\"Prop\":\"-128\"}")]
    [InlineData(sbyte.MaxValue, "{\"Prop\":\"127\"}")]
    [InlineData((sbyte)0, "{\"Prop\":\"0\"}")]
    [InlineData((sbyte)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_sbyte_as_string_JSON_values(sbyte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Int8)), value, json);

    [ConditionalTheory]
    [InlineData(short.MinValue, "{\"Prop\":\"-32768\"}")]
    [InlineData(short.MaxValue, "{\"Prop\":\"32767\"}")]
    [InlineData((short)0, "{\"Prop\":\"0\"}")]
    [InlineData((short)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_short_as_string_JSON_values(short? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Int16)), value, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, "{\"Prop\":\"-2147483648\"}")]
    [InlineData(int.MaxValue, "{\"Prop\":\"2147483647\"}")]
    [InlineData(0, "{\"Prop\":\"0\"}")]
    [InlineData(1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_int_as_string_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Int32)), value, json);

    [ConditionalTheory]
    [InlineData(long.MinValue, "{\"Prop\":\"-9223372036854775808\"}")]
    [InlineData(long.MaxValue, "{\"Prop\":\"9223372036854775807\"}")]
    [InlineData((long)0, "{\"Prop\":\"0\"}")]
    [InlineData((long)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_long_as_string_JSON_values(long? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Int64)), value, json);

    [ConditionalTheory]
    [InlineData(byte.MinValue, "{\"Prop\":\"0\"}")]
    [InlineData(byte.MaxValue, "{\"Prop\":\"255\"}")]
    //[InlineData((byte)0, "{\"Prop\":\"0\"}")]
    [InlineData((byte)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_byte_as_string_JSON_values(byte? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.UInt8)), value, json);

    [ConditionalTheory]
    [InlineData(ushort.MinValue, "{\"Prop\":\"0\"}")]
    [InlineData(ushort.MaxValue, "{\"Prop\":\"65535\"}")]
    //[InlineData((ushort)0, "{\"Prop\":\"0\"}")]
    [InlineData((ushort)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ushort_as_string_JSON_values(ushort? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.UInt16)), value, json);

    [ConditionalTheory]
    [InlineData(uint.MinValue, "{\"Prop\":\"0\"}")]
    [InlineData(uint.MaxValue, "{\"Prop\":\"4294967295\"}")]
    //[InlineData((uint)0, "{\"Prop\":\"0\"}")]
    [InlineData((uint)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_uint_as_string_JSON_values(uint? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.UInt32)), value, json);

    [ConditionalTheory]
    [InlineData(ulong.MinValue, "{\"Prop\":\"0\"}")]
    [InlineData(ulong.MaxValue, "{\"Prop\":\"18446744073709551615\"}")]
    //[InlineData((ulong)0, "{\"Prop\":\"0\"}")]
    [InlineData((ulong)1, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ulong_as_string_JSON_values(ulong? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.UInt64)), value, json);

    [ConditionalTheory]
    [InlineData(float.MinValue, "{\"Prop\":\"-3.4028235E+38\"}")]
    [InlineData(float.MaxValue, "{\"Prop\":\"3.4028235E+38\"}")]
    [InlineData((float)0.0, "{\"Prop\":\"0\"}")]
    [InlineData((float)1.1, "{\"Prop\":\"1.1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_float_as_string_JSON_values(float? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Float)), value, json);

    [ConditionalTheory]
    [InlineData(double.MinValue, "{\"Prop\":\"-1.7976931348623157E+308\"}")]
    [InlineData(double.MaxValue, "{\"Prop\":\"1.7976931348623157E+308\"}")]
    [InlineData(0.0, "{\"Prop\":\"0\"}")]
    [InlineData(1.1, "{\"Prop\":\"1.1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_double_as_string_JSON_values(double? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Double)), value, json);

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", "{\"Prop\":\"-79228162514264337593543950335\"}")]
    [InlineData("79228162514264337593543950335", "{\"Prop\":\"79228162514264337593543950335\"}")]
    [InlineData("0.0", "{\"Prop\":\"0.0\"}")]
    [InlineData("1.1", "{\"Prop\":\"1.1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_decimal_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Decimal)),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("1/1/0001", "{\"Prop\":\"0001-01-01\"}")]
    [InlineData("12/31/9999", "{\"Prop\":\"9999-12-31\"}")]
    //[InlineData("1/1/0001", "{\"Prop\":\"0001-01-01\"}")]
    [InlineData("5/29/2023", "{\"Prop\":\"2023-05-29\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_DateOnly_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.DateOnly)),
            value == null ? default(DateOnly?) : DateOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("00:00:00.0000000", "{\"Prop\":\"00:00:00\"}")]
    [InlineData("23:59:59.9999999", "{\"Prop\":\"23:59:59.9999999\"}")]
    //[InlineData("00:00:00.0000000", "{\"Prop\":\"00:00:00\"}")]
    [InlineData("11:05:12.3456789", "{\"Prop\":\"11:05:12.3456789\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_TimeOnly_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.TimeOnly)),
            value == null ? default(TimeOnly?) : TimeOnly.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", "{\"Prop\":\"0001-01-01 00:00:00\"}")]
    [InlineData("9999-12-31T23:59:59.9999999", "{\"Prop\":\"9999-12-31 23:59:59.9999999\"}")]
    //[InlineData("0001-01-01T00:00:00.0000000", "{\"Prop\":\"0001-01-01 00:00:00\"}")]
    [InlineData("2023-05-29T10:52:47.2064353", "{\"Prop\":\"2023-05-29 10:52:47.2064353\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_DateTime_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.DateTime)),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", "{\"Prop\":\"0001-01-01 00:00:00-01:00\"}")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", "{\"Prop\":\"9999-12-31 23:59:59.9999999+02:00\"}")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", "{\"Prop\":\"0001-01-01 00:00:00-03:00\"}")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", "{\"Prop\":\"2023-05-29 11:11:15.5672854+04:00\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_DateTimeOffset_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.DateTimeOffset)),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("-10675199.02:48:05.4775808", "{\"Prop\":\"-10675199.02:48:05.4775808\"}")]
    [InlineData("10675199.02:48:05.4775807", "{\"Prop\":\"10675199.02:48:05.4775807\"}")]
    [InlineData("00:00:00", "{\"Prop\":\"00:00:00\"}")]
    [InlineData("12:23:23.8018854", "{\"Prop\":\"12:23:23.8018854\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_TimeSpan_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.TimeSpan)),
            value == null ? default(TimeSpan?) : TimeSpan.Parse(value), json);

    [ConditionalTheory]
    [InlineData(false, "{\"Prop\":\"0\"}")]
    [InlineData(true, "{\"Prop\":\"1\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_bool_as_string_JSON_values(bool? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Boolean)), value, json);

    [ConditionalTheory]
    [InlineData(char.MinValue, "{\"Prop\":\"\\u0000\"}")]
    [InlineData(char.MaxValue, "{\"Prop\":\"\\uFFFF\"}")]
    [InlineData(' ', "{\"Prop\":\" \"}")]
    [InlineData('Z', "{\"Prop\":\"Z\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_char_as_string_JSON_values(char? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Character)), value, json);

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", "{\"Prop\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", "{\"Prop\":\"ffffffff-ffff-ffff-ffff-ffffffffffff\"}")]
    //[InlineData("00000000-0000-0000-0000-000000000000", "{\"Prop\":\"00000000-0000-0000-0000-000000000000\"}")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", "{\"Prop\":\"8c44242f-8e3f-4a20-8be8-98c7c1aadebd\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_as_string_GUID_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Guid)),
            value == null ? default(Guid?) : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalTheory]
    [InlineData("MinValue", "{\"Prop\":\"MinValue\"}")]
    [InlineData("MaxValue", "{\"Prop\":\"MaxValue\"}")]
    [InlineData("", "{\"Prop\":\"\"}")]
    [InlineData(
        "❤❥웃유♋☮✌☏☢☠✔☑♚▲♪฿Ɖ⛏♥❣♂♀☿👍✍✉☣☤✘☒♛▼♫⌘⌛¡♡ღツ☼☁❅♾️✎©®™Σ✪✯☭➳Ⓐ✞℃℉°✿⚡☃☂✄¢€£∞✫★½☯✡☪",
        @"{""Prop"":""\u2764\u2765\uC6C3\uC720\u264B\u262E\u270C\u260F\u2622\u2620\u2714\u2611\u265A\u25B2\u266A\u0E3F\u0189\u26CF\u2665\u2763\u2642\u2640\u263F\uD83D\uDC4D\u270D\u2709\u2623\u2624\u2718\u2612\u265B\u25BC\u266B\u2318\u231B\u00A1\u2661\u10E6\u30C4\u263C\u2601\u2745\u267E\uFE0F\u270E\u00A9\u00AE\u2122\u03A3\u272A\u272F\u262D\u27B3\u24B6\u271E\u2103\u2109\u00B0\u273F\u26A1\u2603\u2602\u2704\u00A2\u20AC\u00A3\u221E\u272B\u2605\u00BD\u262F\u2721\u262A""}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_string_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.String)), value, json);

    [ConditionalTheory]
    [InlineData("0,0,0,1", "{\"Prop\":\"AAAAAQ==\"}")]
    [InlineData("255,255,255,255", "{\"Prop\":\"/////w==\"}")]
    [InlineData("", "{\"Prop\":\"\"}")]
    [InlineData("1,2,3,4", "{\"Prop\":\"AQIDBA==\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_binary_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Bytes)),
            value == null
                ? default
                : value == ""
                    ? Array.Empty<byte>()
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    [ConditionalTheory]
    [InlineData(
        "https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName",
        "{\"Prop\":\"https://user:password@www.contoso.com:80/Home/Index.htm?q1=v1&q2=v2#FragmentName\"}")]
    [InlineData("file:///C:/test/path/file.txt", "{\"Prop\":\"file:///C:/test/path/file.txt\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_URI_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Uri)),
            value == null ? default : new Uri(value), json);

    [ConditionalTheory]
    [InlineData("127.0.0.1", "{\"Prop\":\"127.0.0.1\"}")]
    [InlineData("0.0.0.0", "{\"Prop\":\"0.0.0.0\"}")]
    [InlineData("255.255.255.255", "{\"Prop\":\"255.255.255.255\"}")]
    [InlineData("192.168.1.156", "{\"Prop\":\"192.168.1.156\"}")]
    [InlineData("::1", "{\"Prop\":\"::1\"}")]
    [InlineData("::", "{\"Prop\":\"::\"}")]
    //[InlineData("::", "{\"Prop\":\"::\"}")]
    [InlineData("2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577", "{\"Prop\":\"2a00:23c7:c60f:4f01:ba43:6d5a:e648:7577\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_IP_address_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.IpAddress)),
            value == null ? default : IPAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData("001122334455", "{\"Prop\":\"001122334455\"}")]
    [InlineData("00-11-22-33-44-55", "{\"Prop\":\"001122334455\"}")]
    [InlineData("0011.2233.4455", "{\"Prop\":\"001122334455\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_physical_address_as_string_JSON_values(string? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.PhysicalAddress)),
            value == null ? default : PhysicalAddress.Parse(value), json);

    [ConditionalTheory]
    [InlineData((sbyte)Enum8.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((sbyte)Enum8.Max, "{\"Prop\":\"Max\"}")]
    [InlineData((sbyte)Enum8.Default, "{\"Prop\":\"Default\"}")]
    [InlineData((sbyte)Enum8.One, "{\"Prop\":\"One\"}")]
    [InlineData((sbyte)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_sbyte_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Enum8)),
            value == null ? default(Enum8?) : (Enum8)value, json);

    [ConditionalTheory]
    [InlineData((short)Enum16.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((short)Enum16.Max, "{\"Prop\":\"Max\"}")]
    [InlineData((short)Enum16.Default, "{\"Prop\":\"Default\"}")]
    [InlineData((short)Enum16.One, "{\"Prop\":\"One\"}")]
    [InlineData((short)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_short_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Enum16)),
            value == null ? default(Enum16?) : (Enum16)value, json);

    [ConditionalTheory]
    [InlineData((int)Enum32.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((int)Enum32.Max, "{\"Prop\":\"Max\"}")]
    [InlineData((int)Enum32.Default, "{\"Prop\":\"Default\"}")]
    [InlineData((int)Enum32.One, "{\"Prop\":\"One\"}")]
    [InlineData(77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_int_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Enum32)),
            value == null ? default(Enum32?) : (Enum32)value, json);

    [ConditionalTheory]
    [InlineData((long)Enum64.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((long)Enum64.Max, "{\"Prop\":\"Max\"}")]
    [InlineData((long)Enum64.Default, "{\"Prop\":\"Default\"}")]
    [InlineData((long)Enum64.One, "{\"Prop\":\"One\"}")]
    [InlineData((long)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_long_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.Enum64)),
            value == null ? default(Enum64?) : (Enum64)value, json);

    [ConditionalTheory]
    [InlineData((byte)EnumU8.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((byte)EnumU8.Max, "{\"Prop\":\"Max\"}")]
    //[InlineData((byte)EnumU8.Default, "{\"Prop\":\"Min\"}")]
    [InlineData((byte)EnumU8.One, "{\"Prop\":\"One\"}")]
    [InlineData((byte)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_byte_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.EnumU8)),
            value == null ? default(EnumU8?) : (EnumU8)value, json);

    [ConditionalTheory]
    [InlineData((ushort)EnumU16.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((ushort)EnumU16.Max, "{\"Prop\":\"Max\"}")]
    //[InlineData((ushort)EnumU16.Default, "{\"Prop\":\"Min\"}")]
    [InlineData((ushort)EnumU16.One, "{\"Prop\":\"One\"}")]
    [InlineData((ushort)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ushort_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.EnumU16)),
            value == null ? default(EnumU16?) : (EnumU16)value, json);

    [ConditionalTheory]
    [InlineData((uint)EnumU32.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((uint)EnumU32.Max, "{\"Prop\":\"Max\"}")]
    //[InlineData((uint)EnumU32.Default, "{\"Prop\":\"Min\"}")]
    [InlineData((uint)EnumU32.One, "{\"Prop\":\"One\"}")]
    [InlineData((uint)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_uint_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.EnumU32)),
            value == null ? default(EnumU32?) : (EnumU32)value, json);

    [ConditionalTheory]
    [InlineData((ulong)EnumU64.Min, "{\"Prop\":\"Min\"}")]
    [InlineData((ulong)EnumU64.Max, "{\"Prop\":\"Max\"}")]
    //[InlineData((ulong)EnumU64.Default, "{\"Prop\":\"Min\"}")]
    [InlineData((ulong)EnumU64.One, "{\"Prop\":\"One\"}")]
    [InlineData((ulong)77, "{\"Prop\":\"77\"}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_ulong_enum_as_string_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<PrimitiveTypesAsStrings>().GetProperty(nameof(PrimitiveTypesAsStrings.EnumU64)),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    [ConditionalFact]
    public virtual void Can_read_write_point()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.Point)),
            factory.CreatePoint(new Coordinate(2, 4)),
            "{\"Prop\":\"POINT (2 4)\"}");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.PointZ)),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            "{\"Prop\":\"POINT Z(2 4 6)\"}");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.PointM)),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            "{\"Prop\":\"POINT (2 4)\"}");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.PointZM)),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            "{\"Prop\":\"POINT Z(1 2 3)\"}");

        Can_read_and_write_JSON_value<Point?>(
            entityType.GetProperty(nameof(GeometryTypes.Point)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_line_string()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.LineString)),
            factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }),
            "{\"Prop\":\"LINESTRING (0 0, 1 0)\"}");

        Can_read_and_write_JSON_value<LineString?>(
            entityType.GetProperty(nameof(GeometryTypes.LineString)),
            null,
            "{\"Prop\":null}");
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
            "{\"Prop\":\"MULTILINESTRING ((0 0, 0 1), (1 0, 1 1))\"}");

        Can_read_and_write_JSON_value<MultiLineString?>(
            entityType.GetProperty(nameof(GeometryTypes.MultiLineString)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.Polygon)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            "{\"Prop\":\"POLYGON ((0 0, 1 0, 0 1, 0 0))\"}");

        Can_read_and_write_JSON_value<Polygon?>(
            entityType.GetProperty(nameof(GeometryTypes.Polygon)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon_typed_as_geometry()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypes>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypes.Geometry)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            "{\"Prop\":\"POLYGON ((0 0, 1 0, 0 1, 0 0))\"}");

        Can_read_and_write_JSON_value<Geometry?>(
            entityType.GetProperty(nameof(GeometryTypes.Geometry)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_point_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Point)),
            factory.CreatePoint(new Coordinate(2, 4)),
            "{\"Prop\":{\"type\":\"Point\",\"coordinates\":[2.0,4.0]}}");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.PointZ)),
            factory.CreatePoint(new CoordinateZ(2, 4, 6)),
            "{\"Prop\":{\"type\":\"Point\",\"coordinates\":[2.0,4.0]}}");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.PointM)),
            factory.CreatePoint(new CoordinateM(2, 4, 6)),
            "{\"Prop\":{\"type\":\"Point\",\"coordinates\":[2.0,4.0]}}");

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.PointZM)),
            factory.CreatePoint(new CoordinateZM(1, 2, 3, 4)),
            "{\"Prop\":{\"type\":\"Point\",\"coordinates\":[1.0,2.0]}}");

        Can_read_and_write_JSON_value<Point?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Point)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_line_string_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.LineString)),
            factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) }),
            "{\"Prop\":{\"type\":\"LineString\",\"coordinates\":[[0.0,0.0],[1.0,0.0]]}}");

        Can_read_and_write_JSON_value<LineString?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.LineString)),
            null,
            "{\"Prop\":null}");
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
            "{\"Prop\":{\"type\":\"MultiLineString\",\"coordinates\":[[[0.0,0.0],[0.0,1.0]],[[1.0,0.0],[1.0,1.0]]]}}");

        Can_read_and_write_JSON_value<MultiLineString?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.MultiLineString)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Polygon)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            "{\"Prop\":{\"type\":\"Polygon\",\"coordinates\":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}}");

        Can_read_and_write_JSON_value<Polygon?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Polygon)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalFact]
    public virtual void Can_read_write_polygon_typed_as_geometry_as_GeoJson()
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        var entityType = Fixture.EntityType<GeometryTypesAsGeoJson>();

        Can_read_and_write_JSON_value(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Geometry)),
            factory.CreatePolygon(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0) }),
            "{\"Prop\":{\"type\":\"Polygon\",\"coordinates\":[[[0.0,0.0],[1.0,0.0],[0.0,1.0],[0.0,0.0]]]}}");

        Can_read_and_write_JSON_value<Geometry?>(
            entityType.GetProperty(nameof(GeometryTypesAsGeoJson.Geometry)),
            null,
            "{\"Prop\":null}");
    }

    [ConditionalTheory]
    [InlineData(int.MinValue, "{\"Prop\":-2147483648}")]
    [InlineData(int.MaxValue, "{\"Prop\":2147483647}")]
    [InlineData(0, "{\"Prop\":0}")]
    [InlineData(1, "{\"Prop\":1}")]
    public virtual void Can_read_write_converted_type_JSON_values(int value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<ConvertedTypes>().GetProperty(nameof(ConvertedTypes.Id)), new DddId { Id = value }, json);

    [ConditionalTheory]
    [InlineData(int.MinValue, "{\"Prop\":-2147483648}")]
    [InlineData(int.MaxValue, "{\"Prop\":2147483647}")]
    [InlineData(0, "{\"Prop\":0}")]
    [InlineData(1, "{\"Prop\":1}")]
    [InlineData(null, "{\"Prop\":null}")]
    public virtual void Can_read_write_nullable_converted_type_JSON_values(int? value, string json)
        => Can_read_and_write_JSON_value(
            Fixture.EntityType<ConvertedTypes>().GetProperty(nameof(ConvertedTypes.NullableId)),
            value == null ? default(DddId?) : new DddId { Id = value.Value }, json);

    protected virtual void Can_read_and_write_JSON_value<TModel>(IProperty property, TModel value, string json)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var jsonReaderWriter = property.GetJsonValueReaderWriter()!;
        var valueConverter = property.GetTypeMapping().Converter;

        writer.WriteStartObject();
        writer.WritePropertyName("Prop");
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            if (valueConverter == null)
            {
                jsonReaderWriter.ToJson(writer, value);
            }
            else
            {
                jsonReaderWriter.ToJson(writer, valueConverter.ConvertToProvider(value)!);
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        var buffer = stream.ToArray();

        var actual = Encoding.UTF8.GetString(buffer);
        actual = actual.Replace("\\u002B", "+"); // Why does GetString not do this?
        actual = actual.Replace("\\u0026", "&");

        Assert.Equal(json, actual);

        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(buffer));
        readerManager.MoveNext();
        readerManager.MoveNext();
        readerManager.MoveNext();

        Assert.Equal(
            value,
            property.IsNullable
            && readerManager.CurrentReader.TokenType == JsonTokenType.Null
                ? default!
                : valueConverter == null
                    ? jsonReaderWriter.FromJson(ref readerManager)
                    : valueConverter.ConvertFromProvider(jsonReaderWriter.FromJson(ref readerManager))!);
    }

    protected class PrimitiveTypes
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
    }

    protected class NullablePrimitiveTypes
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

    protected class PrimitiveTypesAsStrings
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

    protected class ConvertedTypes
    {
        public DddId Id { get; set; }
        public DddId? NullableId { get; set; }
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

    public sealed class JsonGeoJsonReaderWriter : JsonValueReaderWriter<Geometry>
    {
        public static JsonGeoJsonReaderWriter Instance { get; } = new();

        private JsonGeoJsonReaderWriter()
        {
        }

        public override Geometry FromJsonTyped(ref Utf8JsonReaderManager manager)
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

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<PrimitiveTypes>().HasNoKey();
            modelBuilder.Entity<NullablePrimitiveTypes>().HasNoKey();

            modelBuilder.Entity<PrimitiveTypesAsStrings>(
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

            modelBuilder.Entity<ConvertedTypes>(
                b =>
                {
                    b.HasNoKey();
                    b.Property(e => e.Id).HasConversion<DddIdConverter>();
                    b.Property(e => e.NullableId).HasConversion<DddIdConverter>();
                });

            modelBuilder.Entity<GeometryTypes>().HasNoKey();

            modelBuilder.Entity<GeometryTypesAsGeoJson>(
                b =>
                {
                    b.HasNoKey();
                    b.Property(e => e.Point).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.PointZ).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.PointM).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.PointZM).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.Geometry).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.LineString).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.MultiLineString).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                    b.Property(e => e.Polygon).Metadata.SetJsonValueReaderWriterType(typeof(JsonGeoJsonReaderWriter));
                });
        }

        public DbContext StaticContext
            => _staticContext ??= CreateContext();

        public IEntityType EntityType<TEntity>()
            => StaticContext.Model.FindEntityType(typeof(TEntity))!;
    }
}
