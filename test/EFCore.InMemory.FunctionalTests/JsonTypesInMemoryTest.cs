// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesInMemoryTest : JsonTypesTestBase<JsonTypesInMemoryTest.JsonTypesInMemoryFixture>
{
    public JsonTypesInMemoryTest(JsonTypesInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
    }

    public override void Can_read_write_point()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_point());

    public override void Can_read_write_line_string()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_line_string());

    public override void Can_read_write_multi_line_string()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_multi_line_string());

    public override void Can_read_write_polygon()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_polygon());

    public override void Can_read_write_polygon_typed_as_geometry()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.Throws<NullReferenceException>(() => base.Can_read_write_polygon_typed_as_geometry());

    public class JsonTypesInMemoryFixture : JsonTypesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;
    }
}
