// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class JsonTypesCosmosTest : JsonTypesTestBase
{
    // #25765 - the Cosmos type mapping source doesn't support primitive collections, so we end up with a Property
    // that has no ElementType; that causes the assertion on the element nullability to fail.
    public override void Can_read_write_collection_of_string_JSON_values()
        => Assert.Throws<EqualException>(() => base.Can_read_write_collection_of_string_JSON_values());

    // #25765 - the Cosmos type mapping source doesn't support primitive collections, so we end up with a Property
    // that has no ElementType; that causes the assertion on the element nullability to fail.
    public override void Can_read_write_collection_of_binary_JSON_values()
        => Assert.Throws<EqualException>(() => base.Can_read_write_collection_of_binary_JSON_values());

    // #25765 - the Cosmos type mapping source doesn't support primitive collections, so we end up with a Property
    // that has no ElementType; that causes the assertion on the element nullability to fail.
    public override void Can_read_write_collection_of_nullable_string_JSON_values()
        => Assert.Throws<EqualException>(() => base.Can_read_write_collection_of_nullable_string_JSON_values());

    public override void Can_read_write_point()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point());

    public override void Can_read_write_point_with_Z()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_with_Z());

    public override void Can_read_write_point_with_M()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_with_M());

    public override void Can_read_write_point_with_Z_and_M()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_with_Z_and_M());

    public override void Can_read_write_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_line_string());

    public override void Can_read_write_multi_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_multi_line_string());

    public override void Can_read_write_polygon()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon());

    public override void Can_read_write_polygon_typed_as_geometry()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon_typed_as_geometry());

    public override void Can_read_write_point_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_as_GeoJson());

    public override void Can_read_write_point_with_Z_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_with_Z_as_GeoJson());

    public override void Can_read_write_point_with_M_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_with_M_as_GeoJson());

    public override void Can_read_write_point_with_Z_and_M_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_with_Z_and_M_as_GeoJson());

    public override void Can_read_write_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_line_string_as_GeoJson());

    public override void Can_read_write_multi_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_multi_line_string_as_GeoJson());

    public override void Can_read_write_polygon_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon_as_GeoJson());

    public override void Can_read_write_polygon_typed_as_geometry_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon_typed_as_geometry_as_GeoJson());

    public override void Can_read_write_nullable_point()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point());

    public override void Can_read_write_nullable_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_line_string());

    public override void Can_read_write_nullable_multi_line_string()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_multi_line_string());

    public override void Can_read_write_nullable_polygon()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon());

    public override void Can_read_write_nullable_point_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_point_as_GeoJson());

    public override void Can_read_write_nullable_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_line_string_as_GeoJson());

    public override void Can_read_write_nullable_multi_line_string_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_multi_line_string_as_GeoJson());

    public override void Can_read_write_nullable_polygon_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon_as_GeoJson());

    public override void Can_read_write_polygon_typed_as_nullable_geometry()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon_typed_as_nullable_geometry());

    public override void Can_read_write_polygon_typed_as_nullable_geometry_as_GeoJson()
        // No built-in JSON support for spatial types in the Cosmos provider
        => Assert.Throws<InvalidOperationException>(() => base.Can_read_write_polygon_typed_as_nullable_geometry_as_GeoJson());

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var store = CosmosTestStore.GetOrCreate(nameof(JsonTypesCosmosTest));
        base.OnConfiguring(optionsBuilder.UseCosmos(store.ConnectionUri, store.AuthToken, store.Name));
    }
}
