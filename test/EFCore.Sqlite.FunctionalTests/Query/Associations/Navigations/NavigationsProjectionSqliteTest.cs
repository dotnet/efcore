// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsProjectionSqliteTest(NavigationsSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsProjectionRelationalTestBase<NavigationsSqliteFixture>(fixture, testOutputHelper)
{
    public override Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior));

    public override Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior));

    private static async Task AssertApplyNotSupported(Func<Task> query)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
