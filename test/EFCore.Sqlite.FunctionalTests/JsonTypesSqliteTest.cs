// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Globalization;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

[SpatialiteRequired]
public class JsonTypesSqliteTest : JsonTypesRelationalTestBase
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => base.OnConfiguring(optionsBuilder.UseSqlite(b => b.UseNetTopologySuite()));

    public override void Can_read_write_binary_JSON_values(string value, string json)
        => base.Can_read_write_binary_JSON_values(value, value switch
        {
            "" => json,
            "0,0,0,1" => """{"Prop":"00000001"}""",
            "1,2,3,4" => """{"Prop":"01020304"}""",
            "255,255,255,255" => """{"Prop":"FFFFFFFF"}""",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        });

    [ConditionalFact]
    public override void Can_read_write_collection_of_decimal_JSON_values()
        => Can_read_and_write_JSON_value<DecimalCollectionType, List<decimal>>(nameof(DecimalCollectionType.Decimal),
            new List<decimal>
            {
                decimal.MinValue,
                0,
                decimal.MaxValue
            },
            """{"Prop":["-79228162514264337593543950335.0","0.0","79228162514264337593543950335.0"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_DateTime_JSON_values()
        => Can_read_and_write_JSON_value<DateTimeCollectionType, List<DateTime>>(nameof(DateTimeCollectionType.DateTime),
            new List<DateTime>
            {
                DateTime.MinValue,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":["0001-01-01 00:00:00","2023-05-29 10:52:47","9999-12-31 23:59:59.9999999"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value<DateTimeOffsetCollectionType, List<DateTimeOffset>>(
            nameof(DateTimeOffsetCollectionType.DateTimeOffset),
            new List<DateTimeOffset>
            {
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            },
            """{"Prop":["0001-01-01 00:00:00+00:00","2023-05-29 10:52:47-02:00","2023-05-29 10:52:47+00:00","2023-05-29 10:52:47+02:00","9999-12-31 23:59:59.9999999+00:00"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_GUID_JSON_values()
        => Can_read_and_write_JSON_value<GuidCollectionType, List<Guid>>(nameof(GuidCollectionType.Guid),
            new List<Guid>
            {
                new(),
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            },
            """{"Prop":["00000000-0000-0000-0000-000000000000","8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD","FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_binary_JSON_values()
        => Can_read_and_write_JSON_value<BytesCollectionType, List<byte[]>>(nameof(BytesCollectionType.Bytes),
            new List<byte[]>
            {
                new byte[] { 0, 0, 0, 1 },
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":["00000001","FFFFFFFF","","01020304"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_decimal_with_precision_and_scale_JSON_values()
        => Can_read_and_write_JSON_collection_value<DecimalCollectionType, List<decimal>>(
            b => b.ElementType().HasPrecision(12, 6),
            nameof(DecimalCollectionType.Decimal),
            new List<decimal>
            {
                decimal.MinValue,
                0,
                decimal.MaxValue
            },
            """{"Prop":["-79228162514264337593543950335.0","0.0","79228162514264337593543950335.0"]}""",
            facets: new Dictionary<string, object?> { { CoreAnnotationNames.Precision, 12 }, { CoreAnnotationNames.Scale, 6 } });

    [ConditionalFact]
    public override void Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values()
        => Can_read_and_write_JSON_collection_value<GuidCollectionType, List<Guid>>(
            b => b.ElementType().HasConversion<byte[]>(),
            nameof(GuidCollectionType.Guid),
            new List<Guid>
            {
                new(),
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            },
            """{"Prop":["00000000000000000000000000000000","2F24448C3F8E204A8BE898C7C1AADEBD","FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"]}""",
            facets: new Dictionary<string, object?> { { CoreAnnotationNames.ProviderClrType, typeof(byte[]) } });

    public override void Can_read_write_DateTime_JSON_values(string value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_DateTime_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01 00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31 23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29 10:52:47.2064353"}""")]
    public virtual void Can_read_write_DateTime_JSON_values_sqlite(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeType, DateTime>(nameof(DateTimeType.DateTime),
            DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_DateTimeOffset_JSON_values(string value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_DateTimeOffset_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01 00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31 23:59:59.9999999+02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01 00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29 11:11:15.5672854+04:00"}""")]
    public virtual void Can_read_write_DateTimeOffset_JSON_values_sqlite(string value, string json)
        => Can_read_and_write_JSON_value<DateTimeOffsetType, DateTimeOffset>(nameof(DateTimeOffsetType.DateTimeOffset),
            DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_decimal_JSON_values(decimal value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_decimal_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":"-79228162514264337593543950335.0"}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":"79228162514264337593543950335.0"}""")]
    [InlineData("0.0", """{"Prop":"0.0"}""")]
    [InlineData("1.1", """{"Prop":"1.1"}""")]
    public virtual void Can_read_write_decimal_JSON_values_sqlite(decimal value, string json)
        => Can_read_and_write_JSON_value<DecimalType, decimal>(nameof(DecimalType.Decimal), value, json);

    public override void Can_read_write_GUID_JSON_values(Guid value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_GUID_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"}""")]
    public virtual void Can_read_write_GUID_JSON_values_sqlite(Guid value, string json)
        => Can_read_and_write_JSON_value<GuidType, Guid>(nameof(GuidType.Guid), value, json);

    public override void Can_read_write_nullable_binary_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_nullable_binary_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0,0,0,1", """{"Prop":"00000001"}""")]
    [InlineData("255,255,255,255", """{"Prop":"FFFFFFFF"}""")]
    [InlineData("", """{"Prop":""}""")]
    [InlineData("1,2,3,4", """{"Prop":"01020304"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_binary_JSON_values_sqlite(string? value, string json)
        => Can_read_and_write_JSON_value<NullableBytesType, byte[]?>(nameof(NullableBytesType.Bytes),
            value == null
                ? default
                : value == ""
                    ? Array.Empty<byte>()
                    : value.Split(',').Select(e => byte.Parse(e)).ToArray(), json);

    public override void Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_nullable_DateTime_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000", """{"Prop":"0001-01-01 00:00:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999", """{"Prop":"9999-12-31 23:59:59.9999999"}""")]
    [InlineData("2023-05-29T10:52:47.2064353", """{"Prop":"2023-05-29 10:52:47.2064353"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTime_JSON_values_sqlite(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateTimeType, DateTime?>(nameof(NullableDateTimeType.DateTime),
            value == null ? default(DateTime?) : DateTime.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_nullable_DateTimeOffset_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("0001-01-01T00:00:00.0000000-01:00", """{"Prop":"0001-01-01 00:00:00-01:00"}""")]
    [InlineData("9999-12-31T23:59:59.9999999+02:00", """{"Prop":"9999-12-31 23:59:59.9999999+02:00"}""")]
    [InlineData("0001-01-01T00:00:00.0000000-03:00", """{"Prop":"0001-01-01 00:00:00-03:00"}""")]
    [InlineData("2023-05-29T11:11:15.5672854+04:00", """{"Prop":"2023-05-29 11:11:15.5672854+04:00"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_DateTimeOffset_JSON_values_sqlite(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetType, DateTimeOffset?>(nameof(NullableDateTimeOffsetType.DateTimeOffset),
            value == null ? default(DateTimeOffset?) : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_nullable_decimal_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_nullable_decimal_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("-79228162514264337593543950335", """{"Prop":"-79228162514264337593543950335.0"}""")]
    [InlineData("79228162514264337593543950335", """{"Prop":"79228162514264337593543950335.0"}""")]
    [InlineData("0.0", """{"Prop":"0.0"}""")]
    [InlineData("1.1", """{"Prop":"1.1"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_decimal_JSON_values_sqlite(string? value, string json)
        => Can_read_and_write_JSON_value<NullableDecimalType, decimal?>(nameof(NullableDecimalType.Decimal),
            value == null ? default(decimal?) : decimal.Parse(value, CultureInfo.InvariantCulture), json);

    public override void Can_read_write_nullable_GUID_JSON_values(string? value, string json)
    {
        // Cannot override since the base test contains [InlineData] attributes which still apply, and which contain data we need
        // to override. See Can_read_write_nullable_GUID_JSON_values_sqlite instead.
    }

    [ConditionalTheory]
    [InlineData("00000000-0000-0000-0000-000000000000", """{"Prop":"00000000-0000-0000-0000-000000000000"}""")]
    [InlineData("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", """{"Prop":"FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"}""")]
    [InlineData("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD", """{"Prop":"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"}""")]
    [InlineData(null, """{"Prop":null}""")]
    public virtual void Can_read_write_nullable_GUID_JSON_values_sqlite(string? value, string json)
        => Can_read_and_write_JSON_value<NullableGuidType, Guid?>(nameof(NullableGuidType.Guid),
            value == null ? null : Guid.Parse(value, CultureInfo.InvariantCulture), json);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_binary_JSON_values()
        => Can_read_and_write_JSON_value<NullableBytesCollectionType, List<byte[]?>>(nameof(NullableBytesCollectionType.Bytes),
            new List<byte[]?>
            {
                new byte[] { 0, 0, 0, 1 },
                null,
                new byte[] { 255, 255, 255, 255 },
                Array.Empty<byte>(),
                new byte[] { 1, 2, 3, 4 }
            },
            """{"Prop":["00000001",null,"FFFFFFFF","","01020304"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_DateTime_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateTimeCollectionType, List<DateTime?>>(nameof(NullableDateTimeCollectionType.DateTime),
            new List<DateTime?>
            {
                DateTime.MinValue,
                null,
                new(2023, 5, 29, 10, 52, 47),
                DateTime.MaxValue
            },
            """{"Prop":["0001-01-01 00:00:00",null,"2023-05-29 10:52:47","9999-12-31 23:59:59.9999999"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values()
        => Can_read_and_write_JSON_value<NullableDateTimeOffsetCollectionType, List<DateTimeOffset?>>(
            nameof(NullableDateTimeOffsetCollectionType.DateTimeOffset),
            new List<DateTimeOffset?>
            {
                DateTimeOffset.MinValue,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(-2, 0, 0)),
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(0, 0, 0)),
                null,
                new(new DateTime(2023, 5, 29, 10, 52, 47), new TimeSpan(2, 0, 0)),
                DateTimeOffset.MaxValue
            },
            """{"Prop":["0001-01-01 00:00:00+00:00","2023-05-29 10:52:47-02:00","2023-05-29 10:52:47+00:00",null,"2023-05-29 10:52:47+02:00","9999-12-31 23:59:59.9999999+00:00"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_decimal_JSON_values()
        => Can_read_and_write_JSON_value<NullableDecimalCollectionType, List<decimal?>>(nameof(NullableDecimalCollectionType.Decimal),
            new List<decimal?>
            {
                decimal.MinValue,
                0,
                null,
                decimal.MaxValue
            },
            """{"Prop":["-79228162514264337593543950335.0","0.0",null,"79228162514264337593543950335.0"]}""",
            mappedCollection: true);

    [ConditionalFact]
    public override void Can_read_write_collection_of_nullable_GUID_JSON_values()
        => Can_read_and_write_JSON_value<NullableGuidCollectionType, List<Guid?>>(nameof(NullableGuidCollectionType.Guid),
            new List<Guid?>
            {
                new(),
                null,
                new("8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"),
                Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
            },
            """{"Prop":["00000000-0000-0000-0000-000000000000",null,"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD","FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]}""",
            mappedCollection: true);

    [ConditionalTheory]
    public override void Can_read_write_Int128_JSON_values(Int128 value, string json)
        => Can_read_and_write_JSON_value<Int128Type, Int128>(nameof(Int128Type.SomeInt128), value, json);

    [ConditionalTheory]
    public override void Can_read_write_UInt128_JSON_values(UInt128 value, string json)
        => Can_read_and_write_JSON_value<UInt128Type, UInt128>(nameof(UInt128Type.SomeUInt128), value, json);

    [ConditionalTheory]
    public override void Can_read_write_BigInteger_JSON_values(BigInteger value, string json)
        => Can_read_and_write_JSON_value<BigIntegerType, BigInteger>(nameof(BigIntegerType.SomeBigInteger), value, json);
}
