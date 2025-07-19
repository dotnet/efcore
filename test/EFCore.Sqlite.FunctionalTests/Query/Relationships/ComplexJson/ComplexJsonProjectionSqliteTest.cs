// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexJson;

public class ComplexJsonProjectionSqliteTest(ComplexJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonProjectionRelationalTestBase<ComplexJsonSqliteFixture>(fixture, testOutputHelper)
{
    public override Task SelectMany_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.SelectMany_related_collection(async, queryTrackingBehavior));

    public override Task SelectMany_nested_collection_on_required_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.SelectMany_nested_collection_on_required_related(async, queryTrackingBehavior));

    public override Task SelectMany_nested_collection_on_optional_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.SelectMany_nested_collection_on_optional_related(async, queryTrackingBehavior));

    public override Task Select_subquery_required_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior));

    public override Task Select_subquery_optional_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertApplyNotSupported(() => base.Select_subquery_optional_related_FirstOrDefault(async, queryTrackingBehavior));

    private static async Task AssertApplyNotSupported(Func<Task> query)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
