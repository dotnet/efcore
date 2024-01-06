// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindKeylessEntitiesQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindKeylessEntitiesQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture)
{

    // mapping to view not supported on InMemory
    public override Task KeylessEntity_by_database_view(bool async)
        => Task.CompletedTask;

    public override Task Entity_mapped_to_view_on_right_side_of_join(bool async)
        => Task.CompletedTask;

    public override async Task KeylessEntity_with_included_nav(bool async)
        => await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.KeylessEntity_with_included_nav(async));

    public override async Task KeylessEntity_with_included_navs_multi_level(bool async)
        => await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.KeylessEntity_with_included_navs_multi_level(async));
}
