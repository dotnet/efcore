// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ManyToManyNoTrackingQuerySqliteTest(ManyToManyQuerySqliteFixture fixture)
    : ManyToManyNoTrackingQueryRelationalTestBase<ManyToManyQuerySqliteFixture>(fixture)
{

    // Sqlite does not support Apply operations

    public override async Task Skip_navigation_order_by_single_or_default(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Skip_navigation_order_by_single_or_default(async))).Message);

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async))).Message);

    public override async Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(
        bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(async)))
            .Message);
}
