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
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Sum", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Sum_with_division_on_decimal(async)))
            .Message);

    public override async Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Sum", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Sum_with_division_on_decimal_no_significant_digits(async)))
            .Message);

    public override async Task Average_with_division_on_decimal(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_with_division_on_decimal(async)))
            .Message);

    public override async Task Average_with_division_on_decimal_no_significant_digits(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_with_division_on_decimal_no_significant_digits(async)))
            .Message);

    public override async Task Average_over_max_subquery_is_client_eval(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_over_max_subquery_is_client_eval(async)))
            .Message);

    public override async Task Average_over_nested_subquery_is_client_eval(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Average", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Average_over_nested_subquery_is_client_eval(async)))
            .Message);

    public override async Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Multiple_collection_navigation_with_FirstOrDefault_chained(async))).Message);

    public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
        // Aggregates. Issue #15937.
        => await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));

    public override async Task Contains_with_local_tuple_array_closure(bool async)
        => await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));

    public override async Task Contains_inside_aggregate_function_with_GroupBy(bool async)
    {
        await base.Contains_inside_aggregate_function_with_GroupBy(async);

        AssertSql(
            """
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT COUNT(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@__cities_0) AS "c0"
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
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT AVG(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@__cities_0) AS "c0"
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
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT COALESCE(SUM(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@__cities_0) AS "c0"
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
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."City" IN (
    SELECT "c0"."value"
    FROM json_each(@__cities_0) AS "c0"
)
""");
    }

    public override async Task Contains_inside_LongCount_without_GroupBy(bool async)
    {
        await base.Contains_inside_LongCount_without_GroupBy(async);

        AssertSql(
            """
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT COUNT(*)
FROM "Customers" AS "c"
WHERE "c"."City" IN (
    SELECT "c0"."value"
    FROM json_each(@__cities_0) AS "c0"
)
""");
    }

    public override async Task Contains_inside_Max_without_GroupBy(bool async)
    {
        await base.Contains_inside_Max_without_GroupBy(async);

        AssertSql(
            """
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT MAX(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@__cities_0) AS "c0"
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
@__cities_0='["London","Berlin"]' (Size = 19)

SELECT MIN(CASE
    WHEN "c"."City" IN (
        SELECT "c0"."value"
        FROM json_each(@__cities_0) AS "c0"
    ) THEN 1
    ELSE 0
END)
FROM "Customers" AS "c"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public override async Task Type_casting_inside_sum(bool async)
        => Assert.Equal(
            SqliteStrings.AggregateOperationNotSupported("Sum", "decimal"),
            (await Assert.ThrowsAsync<NotSupportedException>(
                async () => await base.Type_casting_inside_sum(async)))
            .Message);
}
