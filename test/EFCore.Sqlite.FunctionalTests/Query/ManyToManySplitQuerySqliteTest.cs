// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ManyToManySplitQuerySqliteTest(ManyToManySplitQuerySqliteFixture fixture) : ManyToManyQueryRelationalTestBase<ManyToManySplitQuerySqliteFixture>(fixture)
{
    public override async Task Skip_navigation_order_by_single_or_default(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Skip_navigation_order_by_single_or_default(async))).Message);
}
