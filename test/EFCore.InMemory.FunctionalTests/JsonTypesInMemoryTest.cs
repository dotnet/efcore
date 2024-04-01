// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class JsonTypesInMemoryTest : JsonTypesTestBase
{
    public override Task Can_read_write_point()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_point());

    public override Task Can_read_write_point_with_M()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_point_with_M());

    public override Task Can_read_write_point_with_Z()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_point_with_Z());

    public override Task Can_read_write_point_with_Z_and_M()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_point_with_Z_and_M());

    public override Task Can_read_write_line_string()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_line_string());

    public override Task Can_read_write_multi_line_string()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_multi_line_string());

    public override Task Can_read_write_polygon()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(() => base.Can_read_write_polygon());

    public override Task Can_read_write_polygon_typed_as_geometry()
        // No built-in JSON support for spatial types in the in-memory provider
        => Assert.ThrowsAsync<NullReferenceException>(base.Can_read_write_polygon_typed_as_geometry);

    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;
}
