// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesSqliteTest(NonSharedFixture fixture) : JsonTypesRelationalTestBase(fixture)
{
    public override Task Can_read_write_array_of_list_of_GUID_JSON_values(string expected)
        => base.Can_read_write_array_of_list_of_GUID_JSON_values(
            """{"Prop":[["00000000-0000-0000-0000-000000000000","8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"],[],["FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]]}""");

    public override Task Can_read_write_array_of_list_of_binary_JSON_values(string expected)
        => base.Can_read_write_array_of_list_of_binary_JSON_values("""{"Prop":[["000102","01","4D"],[],["4E"]]}""");

    public override Task Can_read_write_list_of_array_of_binary_JSON_values(string expected)
        => base.Can_read_write_list_of_array_of_binary_JSON_values("""{"Prop":[["000102","01","4D"],[],["4E"]]}""");

    public override Task Can_read_write_list_of_array_of_GUID_JSON_values(string expected)
        => base.Can_read_write_list_of_array_of_GUID_JSON_values(
            """{"Prop":[["00000000-0000-0000-0000-000000000000","8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"],[],["FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]]}""");

    public override Task Can_read_write_list_of_array_of_list_of_array_of_binary_JSON_values(string expected)
        => base.Can_read_write_list_of_array_of_list_of_array_of_binary_JSON_values(
            """{"Prop":[[[["000102","01","4D"]],[],[[],[]]],[],[[[]],[["000102","01","4D"]]]]}""");

    public override Task Can_read_write_list_of_array_of_nullable_GUID_JSON_values(string expected)
        => base.Can_read_write_list_of_array_of_nullable_GUID_JSON_values(
            """{"Prop":[["00000000-0000-0000-0000-000000000000",null,"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"],[],["FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]]}""");

    public override Task Can_read_write_binary_JSON_values(string value, string json)
        => base.Can_read_write_binary_JSON_values(
            value, value switch
            {
                "" => json,
                "0,0,0,1" => """{"Prop":"00000001"}""",
                "1,2,3,4" => """{"Prop":"01020304"}""",
                "255,255,255,255" => """{"Prop":"FFFFFFFF"}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_collection_of_decimal_JSON_values(string expected)
        => base.Can_read_write_collection_of_decimal_JSON_values(
            """{"Prop":["-79228162514264337593543950335.0","0.0","79228162514264337593543950335.0"]}""");

    public override Task Can_read_write_collection_of_DateTime_JSON_values(string expected)
        => base.Can_read_write_collection_of_DateTime_JSON_values(
            """{"Prop":["0001-01-01 00:00:00","2023-05-29 10:52:47","9999-12-31 23:59:59.9999999"]}""");

    public override Task Can_read_write_collection_of_DateTimeOffset_JSON_values(string expected)
        => base.Can_read_write_collection_of_DateTimeOffset_JSON_values(
            """{"Prop":["0001-01-01 00:00:00+00:00","2023-05-29 10:52:47-02:00","2023-05-29 10:52:47+00:00","2023-05-29 10:52:47+02:00","9999-12-31 23:59:59.9999999+00:00"]}""");

    public override Task Can_read_write_collection_of_GUID_JSON_values(string expected)
        => base.Can_read_write_collection_of_GUID_JSON_values(
            """{"Prop":["00000000-0000-0000-0000-000000000000","8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD","FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]}""");

    public override Task Can_read_write_collection_of_binary_JSON_values(string expected)
        => base.Can_read_write_collection_of_binary_JSON_values("""{"Prop":["00000001","FFFFFFFF","","01020304"]}""");

    public override Task Can_read_write_collection_of_decimal_with_precision_and_scale_JSON_values(string expected)
        => base.Can_read_write_collection_of_decimal_with_precision_and_scale_JSON_values(
            """{"Prop":["-79228162514264337593543950335.0","0.0","79228162514264337593543950335.0"]}""");

    public override Task Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values(string expected)
        => base.Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values(
            """{"Prop":["00000000000000000000000000000000","2F24448C3F8E204A8BE898C7C1AADEBD","FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"]}""");

    public override Task Can_read_write_DateTime_JSON_values(string value, string json)
        => base.Can_read_write_DateTime_JSON_values(
            value, value switch
            {
                "0001-01-01T00:00:00.0000000" => """{"Prop":"0001-01-01 00:00:00"}""",
                "9999-12-31T23:59:59.9999999" => """{"Prop":"9999-12-31 23:59:59.9999999"}""",
                "2023-05-29T10:52:47.2064353" => """{"Prop":"2023-05-29 10:52:47.2064353"}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_DateTimeOffset_JSON_values(string value, string json)
        => base.Can_read_write_DateTimeOffset_JSON_values(
            value, value switch
            {
                "0001-01-01T00:00:00.0000000-01:00" => """{"Prop":"0001-01-01 00:00:00-01:00"}""",
                "9999-12-31T23:59:59.9999999+02:00" => """{"Prop":"9999-12-31 23:59:59.9999999+02:00"}""",
                "0001-01-01T00:00:00.0000000-03:00" => """{"Prop":"0001-01-01 00:00:00-03:00"}""",
                "2023-05-29T11:11:15.5672854+04:00" => """{"Prop":"2023-05-29 11:11:15.5672854+04:00"}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_decimal_JSON_values(decimal value, string json)
        => base.Can_read_write_decimal_JSON_values(
            value, value switch
            {
                -79228162514264337593543950335m => """{"Prop":"-79228162514264337593543950335.0"}""",
                79228162514264337593543950335m => """{"Prop":"79228162514264337593543950335.0"}""",
                0.0m => """{"Prop":"0.0"}""",
                1.1m => """{"Prop":"1.1"}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_GUID_JSON_values(string value, string json)
        => base.Can_read_write_GUID_JSON_values(
            value, value switch
            {
                "00000000-0000-0000-0000-000000000000" => """{"Prop":"00000000-0000-0000-0000-000000000000"}""",
                "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" => """{"Prop":"FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"}""",
                "8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD" => """{"Prop":"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_nullable_binary_JSON_values(string? value, string json)
        => base.Can_read_write_nullable_binary_JSON_values(
            value, value switch
            {
                "0,0,0,1" => """{"Prop":"00000001"}""",
                "255,255,255,255" => """{"Prop":"FFFFFFFF"}""",
                "" => """{"Prop":""}""",
                "1,2,3,4" => """{"Prop":"01020304"}""",
                null => """{"Prop":null}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_nullable_DateTime_JSON_values(string? value, string json)
        => base.Can_read_write_nullable_DateTime_JSON_values(
            value, value switch
            {
                "0001-01-01T00:00:00.0000000" => """{"Prop":"0001-01-01 00:00:00"}""",
                "9999-12-31T23:59:59.9999999" => """{"Prop":"9999-12-31 23:59:59.9999999"}""",
                "2023-05-29T10:52:47.2064353" => """{"Prop":"2023-05-29 10:52:47.2064353"}""",
                null => """{"Prop":null}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_nullable_DateTimeOffset_JSON_values(string? value, string json)
        => base.Can_read_write_nullable_DateTimeOffset_JSON_values(
            value, value switch
            {
                "0001-01-01T00:00:00.0000000-01:00" => """{"Prop":"0001-01-01 00:00:00-01:00"}""",
                "9999-12-31T23:59:59.9999999+02:00" => """{"Prop":"9999-12-31 23:59:59.9999999+02:00"}""",
                "0001-01-01T00:00:00.0000000-03:00" => """{"Prop":"0001-01-01 00:00:00-03:00"}""",
                "2023-05-29T11:11:15.5672854+04:00" => """{"Prop":"2023-05-29 11:11:15.5672854+04:00"}""",
                null => """{"Prop":null}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_nullable_decimal_JSON_values(string? value, string json)
        => base.Can_read_write_nullable_decimal_JSON_values(
            value, value switch
            {
                "-79228162514264337593543950335" => """{"Prop":"-79228162514264337593543950335.0"}""",
                "79228162514264337593543950335" => """{"Prop":"79228162514264337593543950335.0"}""",
                "0.0" => """{"Prop":"0.0"}""",
                "1.1" => """{"Prop":"1.1"}""",
                null => """{"Prop":null}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_nullable_GUID_JSON_values(string? value, string json)
        => base.Can_read_write_nullable_GUID_JSON_values(
            value, value switch
            {
                "00000000-0000-0000-0000-000000000000" => """{"Prop":"00000000-0000-0000-0000-000000000000"}""",
                "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" => """{"Prop":"FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"}""",
                "8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD" => """{"Prop":"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD"}""",
                null => """{"Prop":null}""",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            });

    public override Task Can_read_write_collection_of_nullable_binary_JSON_values(string expected)
        => base.Can_read_write_collection_of_nullable_binary_JSON_values("""{"Prop":["00000001",null,"FFFFFFFF","","01020304"]}""");

    public override Task Can_read_write_collection_of_nullable_DateTime_JSON_values(string expected)
        => base.Can_read_write_collection_of_nullable_DateTime_JSON_values(
            """{"Prop":["0001-01-01 00:00:00",null,"2023-05-29 10:52:47","9999-12-31 23:59:59.9999999"]}""");

    public override Task Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values(string expected)
        => base.Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values(
            """{"Prop":["0001-01-01 00:00:00+00:00","2023-05-29 10:52:47-02:00","2023-05-29 10:52:47+00:00",null,"2023-05-29 10:52:47+02:00","9999-12-31 23:59:59.9999999+00:00"]}""");

    public override Task Can_read_write_collection_of_nullable_decimal_JSON_values(string expected)
        => base.Can_read_write_collection_of_nullable_decimal_JSON_values(
            """{"Prop":["-79228162514264337593543950335.0","0.0",null,"79228162514264337593543950335.0"]}""");

    public override Task Can_read_write_collection_of_nullable_GUID_JSON_values(string expected)
        => base.Can_read_write_collection_of_nullable_GUID_JSON_values(
            """{"Prop":["00000000-0000-0000-0000-000000000000",null,"8C44242F-8E3F-4A20-8BE8-98C7C1AADEBD","FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF"]}""");

    public override Task Can_read_write_ulong_enum_JSON_values(EnumU64 value, string json)
        => Can_read_and_write_JSON_value<EnumU64Type, EnumU64>(nameof(EnumU64Type.EnumU64), value, json);

    public override Task Can_read_write_nullable_ulong_enum_JSON_values(object? value, string json)
        => Can_read_and_write_JSON_value<NullableEnumU64Type, EnumU64?>(
            nameof(NullableEnumU64Type.EnumU64),
            value == null ? default(EnumU64?) : (EnumU64)value, json);

    public override Task Can_read_write_collection_of_ulong_enum_JSON_values()
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

    public override Task Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
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

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder)
            .ConfigureWarnings(
                w => w
                    .Ignore(SqliteEventId.SchemaConfiguredWarning)
                    .Ignore(SqliteEventId.CompositeKeyWithValueGeneration));
        new SqliteDbContextOptionsBuilder(builder).UseNetTopologySuite();
        return builder;
    }
}
