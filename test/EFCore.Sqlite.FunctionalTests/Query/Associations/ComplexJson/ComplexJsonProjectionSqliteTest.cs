// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonProjectionSqliteTest(ComplexJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonProjectionRelationalTestBase<ComplexJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.SelectMany_related_collection(queryTrackingBehavior));

    public override Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior));

    public override Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior));

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
