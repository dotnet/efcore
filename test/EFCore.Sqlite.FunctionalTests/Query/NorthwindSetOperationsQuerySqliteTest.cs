// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSetOperationsQuerySqliteTest : NorthwindSetOperationsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindSetOperationsQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Client_eval_Union_FirstOrDefault(bool async)
        // Client evaluation in projection. Issue #16243.
        => Assert.Equal(
            RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Client_eval_Union_FirstOrDefault(async))).Message);

    public override async Task Set_operations_over_null_checked_to_one_non_entity_subquery(bool async)
    {
        await base.Set_operations_over_null_checked_to_one_non_entity_subquery(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."City", CASE
    WHEN "o1"."marker" IS NOT NULL THEN "o1"."OrderID"
    ELSE 0
END AS "LatestOrderID", "o1"."OrderDate" AS "LatestOrderDate"
FROM "Customers" AS "c"
LEFT JOIN (
    SELECT "o0"."OrderID", "o0"."OrderDate", "o0"."marker", "o0"."CustomerID"
    FROM (
        SELECT "o"."OrderID", "o"."OrderDate", 1 AS "marker", "o"."CustomerID", ROW_NUMBER() OVER(PARTITION BY "o"."CustomerID" ORDER BY "o"."OrderDate" DESC, "o"."OrderID" DESC) AS "row"
        FROM "Orders" AS "o"
    ) AS "o0"
    WHERE "o0"."row" <= 1
) AS "o1" ON "c"."CustomerID" = "o1"."CustomerID"
UNION ALL
SELECT "c0"."CustomerID", "c0"."City", 0 AS "LatestOrderID", NULL AS "LatestOrderDate"
FROM "Customers" AS "c0"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Orders" AS "o2"
    WHERE "c0"."CustomerID" = "o2"."CustomerID")
""",
            //
            """
SELECT "c"."CustomerID", "c"."City", 0 AS "LatestOrderID", NULL AS "LatestOrderDate"
FROM "Customers" AS "c"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Orders" AS "o"
    WHERE "c"."CustomerID" = "o"."CustomerID")
UNION
SELECT "c0"."CustomerID", "c0"."City", CASE
    WHEN "o2"."marker" IS NULL THEN 0
    ELSE "o2"."OrderID"
END AS "LatestOrderID", "o2"."OrderDate" AS "LatestOrderDate"
FROM "Customers" AS "c0"
LEFT JOIN (
    SELECT "o1"."OrderID", "o1"."OrderDate", "o1"."marker", "o1"."CustomerID"
    FROM (
        SELECT "o0"."OrderID", "o0"."OrderDate", 1 AS "marker", "o0"."CustomerID", ROW_NUMBER() OVER(PARTITION BY "o0"."CustomerID" ORDER BY "o0"."OrderDate" DESC, "o0"."OrderID" DESC) AS "row"
        FROM "Orders" AS "o0"
    ) AS "o1"
    WHERE "o1"."row" <= 1
) AS "o2" ON "c0"."CustomerID" = "o2"."CustomerID"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
