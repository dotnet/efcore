// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsCollectionSqliteTest(NavigationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsCollectionRelationalTestBase<NavigationsSqliteFixture>(fixture, testOutputHelper)
{
    public override Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Distinct_projected(queryTrackingBehavior));

    private static async Task AssertApplyNotSupported(Func<Task> query)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
