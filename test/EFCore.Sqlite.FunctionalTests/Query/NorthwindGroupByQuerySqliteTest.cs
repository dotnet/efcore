// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindGroupByQuerySqliteTest : NorthwindGroupByQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindGroupByQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // SQLite evaluates random() per row, where SQL Server's RAND() is a runtime constant - so this is the provider that
    // can catch the grouping element's filter being emitted more than once. Every order has OrderID > 0, so All holds for
    // every group whichever rows the filter samples, which is what the expected query asserts directly; EF.Functions.Random
    // has no client translation, so the two queries cannot be the same one.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_All_over_volatile_filtered_grouping_element(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .Select(g => new { g.Key, All = g.Where(o => EF.Functions.Random() > 0.5).All(o => o.OrderID > 0) }),
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .Select(g => new { g.Key, All = true }),
            elementSorter: e => e.Key);

        AssertSql(
            """
SELECT "o"."CustomerID" AS "Key", COUNT(CASE
    WHEN abs(random() / 9.2233720368547799E+18) > 0.5 THEN CASE
        WHEN "o"."OrderID" > 0 THEN NULL
        ELSE 1
    END
END) = 0 AS "All"
FROM "Orders" AS "o"
GROUP BY "o"."CustomerID"
""");
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
        => AssertApplyNotSupported(()
            => base.Select_correlated_collection_after_GroupBy_aggregate_when_identifier_changes_to_complex(async));

    public override Task GroupBy_aggregate_from_multiple_query_in_same_projection_3(bool async)
        => Assert.ThrowsAsync<SqliteException>(() => base.GroupBy_aggregate_from_multiple_query_in_same_projection_3(async));

    public override async Task Odata_groupby_empty_key(bool async)
    {
        await base.Odata_groupby_empty_key(async);

        AssertSql(
            """
SELECT 'TotalAmount' AS "Name", COALESCE(ef_sum(CAST("o0"."OrderID" AS TEXT)), '0.0') AS "Value"
FROM (
    SELECT "o"."OrderID", 1 AS "Key"
    FROM "Orders" AS "o"
) AS "o0"
GROUP BY "o0"."Key"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static async Task AssertApplyNotSupported(Func<Task> query)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
