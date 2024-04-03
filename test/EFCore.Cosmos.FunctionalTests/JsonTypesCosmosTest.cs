// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesCosmosTest : JsonTypesTestBase
{
    public override Task Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values(string expected)
        => base.Can_read_write_collection_of_Guid_converted_to_bytes_JSON_values("""{"Prop":["AAAAAAAAAAAAAAAAAAAAAA==","LyREjD\u002BOIEqL6JjHwarevQ==","/////////////////////w=="]}""");

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
