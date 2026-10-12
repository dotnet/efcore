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
    WHEN EXISTS (
        SELECT 1
        FROM "Orders" AS "o"
        WHERE "c"."CustomerID" = "o"."CustomerID") THEN (
        SELECT "o0"."OrderID"
        FROM "Orders" AS "o0"
        WHERE "c"."CustomerID" = "o0"."CustomerID"
        ORDER BY "o0"."OrderDate" DESC, "o0"."OrderID" DESC
        LIMIT 1)
    ELSE 0
END AS "LatestOrderID", (
    SELECT "o1"."OrderDate"
    FROM "Orders" AS "o1"
    WHERE "c"."CustomerID" = "o1"."CustomerID"
    ORDER BY "o1"."OrderDate" DESC, "o1"."OrderID" DESC
    LIMIT 1) AS "LatestOrderDate"
FROM "Customers" AS "c"
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
    WHEN NOT EXISTS (
        SELECT 1
        FROM "Orders" AS "o0"
        WHERE "c0"."CustomerID" = "o0"."CustomerID") THEN 0
    ELSE (
        SELECT "o1"."OrderID"
        FROM "Orders" AS "o1"
        WHERE "c0"."CustomerID" = "o1"."CustomerID"
        ORDER BY "o1"."OrderDate" DESC, "o1"."OrderID" DESC
        LIMIT 1)
END AS "LatestOrderID", (
    SELECT "o2"."OrderDate"
    FROM "Orders" AS "o2"
    WHERE "c0"."CustomerID" = "o2"."CustomerID"
    ORDER BY "o2"."OrderDate" DESC, "o2"."OrderID" DESC
    LIMIT 1) AS "LatestOrderDate"
FROM "Customers" AS "c0"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
