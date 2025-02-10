// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindAggregateOperatorsQuerySqliteTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindAggregateOperatorsQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Sum_with_division_on_decimal(bool async)
    {
        await base.Sum_with_division_on_decimal(async);

        AssertSql(
            """
SELECT COALESCE(ef_sum(ef_divide(CAST("o"."Quantity" AS TEXT), '2.09')), '0.0')
FROM "Order Details" AS "o"
""");
    }

    public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
    {
        await base.Sum_with_division_on_decimal_no_significant_digits(async);

        AssertSql(
            """
SELECT COALESCE(ef_sum(ef_divide(CAST("o"."Quantity" AS TEXT), '2.0')), '0.0')
FROM "Order Details" AS "o"
""");
    }

    public override async Task Average_with_division_on_decimal(bool async)
    {
        await base.Average_with_division_on_decimal(async);

        AssertSql(
            """
SELECT ef_avg(ef_divide(CAST("o"."Quantity" AS TEXT), '2.09'))
FROM "Order Details" AS "o"
""");
    }

    public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
    {
        await base.Average_with_division_on_decimal_no_significant_digits(async);

        AssertSql(
            """
SELECT ef_avg(ef_divide(CAST("o"."Quantity" AS TEXT), '2.0'))
FROM "Order Details" AS "o"
""");
    }

    public override async Task Average_over_max_subquery(bool async)
    {
        await base.Average_over_max_subquery(async);

        AssertSql(
            """
@p='3'

SELECT ef_avg(CAST((
    SELECT AVG(CAST(5 + (
        SELECT MAX("o0"."ProductID")
        FROM "Order Details" AS "o0"
        WHERE "o"."OrderID" = "o0"."OrderID") AS REAL))
    FROM "Orders" AS "o"
    WHERE "c0"."CustomerID" = "o"."CustomerID") AS TEXT))
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    ORDER BY "c"."CustomerID"
    LIMIT @p
) AS "c0"
""");
    }

    public override async Task Average_over_nested_subquery(bool async)
    {
        await base.Average_over_nested_subquery(async);

        AssertSql(
            """
@p='3'

SELECT ef_avg(CAST((
    SELECT AVG(5.0 + (
        SELECT AVG(CAST("o0"."ProductID" AS REAL))
        FROM "Order Details" AS "o0"
        WHERE "o"."OrderID" = "o0"."OrderID"))
    FROM "Orders" AS "o"
    WHERE "c0"."CustomerID" = "o"."CustomerID") AS TEXT))
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    ORDER BY "c"."CustomerID"
    LIMIT @p
) AS "c0"
""");
    }

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Multiple_collection_navigation_with_FirstOrDefault_chained(async))).Message);

    // TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
    // optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
    //
    // public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
    //     => await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));
    //
    // public override async Task Contains_with_local_tuple_array_closure(bool async)
    //     => await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));

    public override async Task Contains_inside_aggregate_function_with_GroupBy(bool async)
    {
        await base.Contains_inside_aggregate_function_with_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT COUNT(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@cities) AS "c0"
    ) THEN 1
END)
FROM "Customers" AS "c"
GROUP BY "c"."Country"
""");
    }

    public override async Task Contains_inside_Average_without_GroupBy(bool async)
    {
        await base.Contains_inside_Average_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT AVG(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@cities) AS "c0"
    ) THEN 1.0
    ELSE 0.0
END)
FROM "Customers" AS "c"
""");
    }

    public override async Task Contains_inside_Sum_without_GroupBy(bool async)
    {
        await base.Contains_inside_Sum_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT COALESCE(SUM(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@cities) AS "c0"
    ) THEN 1
    ELSE 0
END), 0)
FROM "Customers" AS "c"
""");
    }

    public override async Task Contains_inside_Count_without_GroupBy(bool async)
    {
        await base.Contains_inside_Count_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."City" IN (
    SELECT "c0"."value"
    FROM json_each(@cities) AS "c0"
)
""");
    }

    public override async Task Contains_inside_LongCount_without_GroupBy(bool async)
    {
        await base.Contains_inside_LongCount_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."City" IN (
    SELECT "c0"."value"
    FROM json_each(@cities) AS "c0"
)
""");
    }

    public override async Task Contains_inside_Max_without_GroupBy(bool async)
    {
        await base.Contains_inside_Max_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT MAX(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@cities) AS "c0"
    ) THEN 1
    ELSE 0
END)
FROM "Customers" AS "c"
""");
    }

    public override async Task Contains_inside_Min_without_GroupBy(bool async)
    {
        await base.Contains_inside_Min_without_GroupBy(async);

        AssertSql(
            """
@cities='["London","Berlin"]' (Size = 19)

SELECT MIN(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@cities) AS "c0"
    ) THEN 1
    ELSE 0
END)
FROM "Customers" AS "c"
""");
    }

    public override async Task Type_casting_inside_sum(bool async)
    {
        await base.Type_casting_inside_sum(async);

        AssertSql(
            """
SELECT COALESCE(ef_sum(CAST("o"."Discount" AS TEXT)), '0.0')
FROM "Order Details" AS "o"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
