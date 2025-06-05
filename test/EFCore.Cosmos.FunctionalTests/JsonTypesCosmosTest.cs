// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesCosmosTest(NonSharedFixture fixture) : JsonTypesTestBase(fixture)
{
    public override Task Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values(
                """{"Prop":["AAAAAAAAAAAAAAAAAAAAAA==","LyREjD\u002BOIEqL6JjHwarevQ==","/////////////////////w=="]}"""));

    public override Task Can_read_write_array_of_list_of_binary_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_array_of_list_of_binary_JSON_values(expected));

    public override Task Can_read_write_array_of_list_of_GUID_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_array_of_list_of_GUID_JSON_values(expected));

    public override Task Can_read_write_collection_of_GUID_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_collection_of_GUID_JSON_values(expected));

    public override Task Can_read_write_collection_of_binary_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_collection_of_binary_JSON_values(expected));

    public override Task Can_read_write_collection_of_nullable_binary_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_collection_of_nullable_binary_JSON_values(expected));

    public override Task Can_read_write_collection_of_nullable_GUID_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_collection_of_nullable_GUID_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_binary_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_list_of_array_of_binary_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_GUID_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_list_of_array_of_GUID_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_list_of_array_of_binary_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Can_read_write_list_of_array_of_list_of_array_of_binary_JSON_values(expected));

    public override Task Can_read_write_list_of_array_of_nullable_GUID_JSON_values(string expected)
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Can_read_write_list_of_array_of_nullable_GUID_JSON_values(expected));

    public override Task Can_read_write_array_of_list_of_array_of_IPAddress_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_array_of_list_of_array_of_IPAddress_JSON_values);

    public override Task Can_read_write_array_of_list_of_IPAddress_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_array_of_list_of_IPAddress_JSON_values);

    public override Task Can_read_write_collection_of_byte_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_byte_enum_JSON_values);

    public override Task Can_read_write_collection_of_int_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_int_enum_JSON_values);

    public override Task Can_read_write_collection_of_int_with_converter_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_int_with_converter_JSON_values);

    public override Task Can_read_write_collection_of_IP_address_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_IP_address_JSON_values);

    public override Task Can_read_write_collection_of_long_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_long_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_int_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_int_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_int_with_converter_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_int_with_converter_JSON_values);

    public override Task Can_read_write_collection_of_nullable_IP_address_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_IP_address_JSON_values);

    public override Task Can_read_write_collection_of_nullable_long_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_long_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_physical_address_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_physical_address_JSON_values);

    public override Task Can_read_write_collection_of_nullable_sbyte_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_sbyte_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_short_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_short_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_uint_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_uint_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_ulong_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_ulong_enum_JSON_values);

    public override Task Can_read_write_collection_of_nullable_URI_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_URI_JSON_values);

    public override Task Can_read_write_collection_of_nullable_ushort_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_ushort_enum_JSON_values);

    public override Task Can_read_write_collection_of_physical_address_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_physical_address_JSON_values);

    public override Task Can_read_write_collection_of_sbyte_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_sbyte_enum_JSON_values);

    public override Task Can_read_write_collection_of_short_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_short_enum_JSON_values);

    public override Task Can_read_write_collection_of_uint_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_uint_enum_JSON_values);

    public override Task Can_read_write_collection_of_ulong_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_ulong_enum_JSON_values);

    public override Task Can_read_write_collection_of_URI_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_URI_JSON_values);

    public override Task Can_read_write_collection_of_ushort_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_ushort_enum_JSON_values);

    public override Task Can_read_write_list_of_array_of_IPAddress_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_list_of_array_of_IPAddress_JSON_values);

    public override Task Can_read_write_list_of_array_of_list_of_IPAddress_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_list_of_array_of_list_of_IPAddress_JSON_values);

    public override Task Can_read_write_collection_of_nullable_byte_enum_JSON_values()
        // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_collection_of_nullable_byte_enum_JSON_values);

    public override Task Can_read_write_point()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point);

    public override Task Can_read_write_point_with_Z()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_with_Z);

    public override Task Can_read_write_point_with_M()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_with_M);

    public override Task Can_read_write_point_with_Z_and_M()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_with_Z_and_M);

    public override Task Can_read_write_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_line_string);

    public override Task Can_read_write_multi_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_multi_line_string);

    public override Task Can_read_write_polygon()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon);

    public override Task Can_read_write_polygon_typed_as_geometry()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon_typed_as_geometry);

    public override Task Can_read_write_point_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_as_GeoJson);

    public override Task Can_read_write_point_with_Z_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_with_Z_as_GeoJson);

    public override Task Can_read_write_point_with_M_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_with_M_as_GeoJson);

    public override Task Can_read_write_point_with_Z_and_M_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_with_Z_and_M_as_GeoJson);

    public override Task Can_read_write_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_line_string_as_GeoJson);

    public override Task Can_read_write_multi_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_multi_line_string_as_GeoJson);

    public override Task Can_read_write_polygon_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon_as_GeoJson);

    public override Task Can_read_write_polygon_typed_as_geometry_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon_typed_as_geometry_as_GeoJson);

    public override Task Can_read_write_nullable_point()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point);

    public override Task Can_read_write_nullable_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_line_string);

    public override Task Can_read_write_nullable_multi_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_multi_line_string);

    public override Task Can_read_write_nullable_polygon()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon);

    public override Task Can_read_write_nullable_point_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_point_as_GeoJson);

    public override Task Can_read_write_nullable_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_line_string_as_GeoJson);

    public override Task Can_read_write_nullable_multi_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_multi_line_string_as_GeoJson);

    public override Task Can_read_write_nullable_polygon_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon_as_GeoJson);

    public override Task Can_read_write_polygon_typed_as_nullable_geometry()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon_typed_as_nullable_geometry);

    public override Task Can_read_write_polygon_typed_as_nullable_geometry_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.ThrowsAsync<InvalidOperationException>(base.Can_read_write_polygon_typed_as_nullable_geometry_as_GeoJson);

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
