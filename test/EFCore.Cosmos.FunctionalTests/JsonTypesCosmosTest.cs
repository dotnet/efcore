// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class JsonTypesCosmosTest : JsonTypesTestBase<JsonTypesCosmosTest.JsonTypesCosmosFixture>
{
    public JsonTypesCosmosTest(JsonTypesCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }

    public override void Can_read_write_point()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_point());

    public override void Can_read_write_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_line_string());

    public override void Can_read_write_multi_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_multi_line_string());

    public override void Can_read_write_polygon()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_polygon());

    public override void Can_read_write_polygon_typed_as_geometry()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_polygon_typed_as_geometry());

    public override void Can_read_write_point_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_point_as_GeoJson());

    public override void Can_read_write_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_line_string_as_GeoJson());

    public override void Can_read_write_multi_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_multi_line_string_as_GeoJson());

    public override void Can_read_write_polygon_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_polygon_as_GeoJson());

    public override void Can_read_write_polygon_typed_as_geometry_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_polygon_typed_as_geometry_as_GeoJson());

    public override void Can_read_write_collection_of_sbyte_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_sbyte_JSON_values());

    public override void Can_read_write_collection_of_short_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_short_JSON_values());

    public override void Can_read_write_collection_of_int_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_int_JSON_values());

    public override void Can_read_write_collection_of_long_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_long_JSON_values());

    public override void Can_read_write_collection_of_byte_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_byte_JSON_values());

    public override void Can_read_write_collection_of_uint_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_uint_JSON_values());

    public override void Can_read_write_collection_of_float_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_float_JSON_values());

    public override void Can_read_write_collection_of_double_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_double_JSON_values());

    public override void Can_read_write_collection_of_decimal_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_decimal_JSON_values());

    public override void Can_read_write_collection_of_DateOnly_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_DateOnly_JSON_values());

    public override void Can_read_write_collection_of_TimeOnly_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_TimeOnly_JSON_values());

    public override void Can_read_write_collection_of_DateTime_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_DateTime_JSON_values());

    public override void Can_read_write_collection_of_DateTimeOffset_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_DateTimeOffset_JSON_values());

    public override void Can_read_write_collection_of_TimeSpan_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_TimeSpan_JSON_values());

    public override void Can_read_write_collection_of_bool_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_bool_JSON_values());

    public override void Can_read_write_collection_of_char_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_char_JSON_values());

    public override void Can_read_write_collection_of_string_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_string_JSON_values());

    public override void Can_read_write_collection_of_binary_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_binary_JSON_values());

    public override void Can_read_write_collection_of_nullable_sbyte_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_sbyte_JSON_values());

    public override void Can_read_write_collection_of_nullable_short_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_short_JSON_values());

    public override void Can_read_write_collection_of_nullable_int_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_int_JSON_values());

    public override void Can_read_write_collection_of_nullable_long_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_long_JSON_values());

    public override void Can_read_write_collection_of_nullable_byte_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_byte_JSON_values());

    public override void Can_read_write_collection_of_nullable_uint_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_uint_JSON_values());

    public override void Can_read_write_collection_of_nullable_float_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_float_JSON_values());

    public override void Can_read_write_collection_of_nullable_double_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_double_JSON_values());

    public override void Can_read_write_collection_of_nullable_decimal_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_decimal_JSON_values());

    public override void Can_read_write_collection_of_nullable_DateOnly_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_DateOnly_JSON_values());

    public override void Can_read_write_collection_of_nullable_TimeOnly_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_TimeOnly_JSON_values());

    public override void Can_read_write_collection_of_nullable_DateTime_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_DateTime_JSON_values());

    public override void Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_DateTimeOffset_JSON_values());

    public override void Can_read_write_collection_of_nullable_TimeSpan_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_TimeSpan_JSON_values());

    public override void Can_read_write_collection_of_nullable_bool_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_bool_JSON_values());

    public override void Can_read_write_collection_of_nullable_char_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_char_JSON_values());

    public override void Can_read_write_collection_of_nullable_GUID_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_string_JSON_values());

    public override void Can_read_write_collection_of_nullable_string_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_string_JSON_values());

    public override void Can_read_write_collection_of_nullable_binary_JSON_values()
        // Cosmos currently uses a different mechanism for primitive collections
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_collection_of_nullable_binary_JSON_values());

    public class JsonTypesCosmosFixture : JsonTypesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Ignore<GeometryTypes>();
            modelBuilder.Ignore<GeometryTypesAsGeoJson>();
        }
    }
}
