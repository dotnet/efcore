// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindGroupByQuerySqliteTest : NorthwindGroupByQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindGroupByQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Select_uncorrelated_collection_with_groupby_multiple_collections_work(bool async)
        => AssertApplyNotSupported(() => base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async));

    public override Task Select_uncorrelated_collection_with_groupby_works(bool async)
        => AssertApplyNotSupported(() => base.Select_uncorrelated_collection_with_groupby_multiple_collections_work(async));

    public override Task Select_uncorrelated_collection_with_groupby_when_outer_is_distinct(bool async)
        => AssertApplyNotSupported(() => base.Select_uncorrelated_collection_with_groupby_works(async));

    public override Task AsEnumerable_in_subquery_for_GroupBy(bool async)
        => AssertApplyNotSupported(() => base.AsEnumerable_in_subquery_for_GroupBy(async));

    public override Task Complex_query_with_groupBy_in_subquery1(bool async)
        => AssertApplyNotSupported(() => base.Complex_query_with_groupBy_in_subquery1(async));

    public override Task Complex_query_with_groupBy_in_subquery2(bool async)
        => AssertApplyNotSupported(() => base.Complex_query_with_groupBy_in_subquery2(async));

    public override Task Complex_query_with_groupBy_in_subquery3(bool async)
        => AssertApplyNotSupported(() => base.Complex_query_with_groupBy_in_subquery3(async));

    public override Task Complex_query_with_groupBy_in_subquery4(bool async)
        => AssertApplyNotSupported(() => base.Complex_query_with_groupBy_in_subquery4(async));

    public override Task Select_nested_collection_with_groupby(bool async)
        => AssertApplyNotSupported(() => base.Select_nested_collection_with_groupby(async));

    public override Task Complex_query_with_group_by_in_subquery5(bool async)
        => AssertApplyNotSupported(() => base.Complex_query_with_group_by_in_subquery5(async));

    public override Task GroupBy_aggregate_from_multiple_query_in_same_projection(bool async)
        => AssertApplyNotSupported(() => base.GroupBy_aggregate_from_multiple_query_in_same_projection(async));

    public override Task Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(bool async)
        => AssertApplyNotSupported(
            () => base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(async));

    public override Task GroupBy_aggregate_from_multiple_query_in_same_projection_3(bool async)
        => Assert.ThrowsAsync<SqliteException>(
            () => base.GroupBy_aggregate_from_multiple_query_in_same_projection_3(async));

    public override async Task Odata_groupby_empty_key(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Sum", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                () => base.Odata_groupby_empty_key(async))).Message);

    private static async Task AssertApplyNotSupported(Func<Task> query)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
