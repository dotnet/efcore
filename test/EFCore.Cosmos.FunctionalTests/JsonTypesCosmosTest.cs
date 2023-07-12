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
