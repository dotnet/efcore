// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class WindowFunctionQuerySqliteTest(NonSharedFixture fixture) : WindowFunctionQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder UseWindowFunctionAggregates(DbContextOptionsBuilder optionsBuilder)
    {
#pragma warning disable EF9001 // UseWindowFunctionAggregates is experimental
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseWindowFunctionAggregates();
#pragma warning restore EF9001

        return optionsBuilder;
    }

    public override async Task Elements_and_aggregate()
    {
        await base.Elements_and_aggregate();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Id", "e"."Bonus", "e"."Name", "e"."Salary", AVG(CAST("e"."Salary" AS REAL)) OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Elements_and_multiple_aggregates()
    {
        await base.Elements_and_multiple_aggregates();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Id", "e"."Bonus", "e"."Name", "e"."Salary", COUNT(*) OVER(PARTITION BY "e"."DepartmentName"), COALESCE(SUM("e"."Salary") OVER(PARTITION BY "e"."DepartmentName"), 0), MAX("e"."Salary") OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Elements_and_long_count()
    {
        await base.Elements_and_long_count();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Name", COUNT(*) OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Elements_and_aggregate_with_keys_differing_in_case()
    {
        await base.Elements_and_aggregate_with_keys_differing_in_case();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Name", COUNT(*) OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Elements_and_float_sum()
    {
        await base.Elements_and_float_sum();

        AssertSql(
            """
SELECT "e1"."DepartmentName", "e0"."Name", "e0"."Id", "e1"."c"
FROM (
    SELECT "e"."DepartmentName", COALESCE(SUM(CAST("e"."Salary" AS REAL)), 0.0) AS "c"
    FROM "Employees" AS "e"
    GROUP BY "e"."DepartmentName"
) AS "e1"
LEFT JOIN "Employees" AS "e0" ON "e1"."DepartmentName" = "e0"."DepartmentName"
ORDER BY "e1"."DepartmentName"
""");
    }

    public override async Task Projected_elements_and_aggregate()
    {
        await base.Projected_elements_and_aggregate();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Name", COALESCE(SUM("e"."Salary") OVER(PARTITION BY "e"."DepartmentName"), 0)
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Nullable_aggregate_over_partition_without_values()
    {
        await base.Nullable_aggregate_over_partition_without_values();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Id", "e"."Bonus", "e"."Name", "e"."Salary", AVG(CAST("e"."Bonus" AS REAL)) OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Sum_over_partition_without_values()
    {
        await base.Sum_over_partition_without_values();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Name", COALESCE(SUM("e"."Bonus") OVER(PARTITION BY "e"."DepartmentName"), 0)
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Decimal_aggregates()
    {
        await base.Decimal_aggregates();

        AssertSql(
            """
SELECT "e1"."DepartmentName", "e0"."Name", "e0"."Id", "e1"."c", "e1"."c0", "e1"."c1"
FROM (
    SELECT "e"."DepartmentName", COALESCE(ef_sum(CAST("e"."Salary" AS TEXT)), '0.0') AS "c", ef_avg(CAST("e"."Salary" AS TEXT)) AS "c0", ef_max(CAST("e"."Salary" AS TEXT)) AS "c1"
    FROM "Employees" AS "e"
    GROUP BY "e"."DepartmentName"
) AS "e1"
LEFT JOIN "Employees" AS "e0" ON "e1"."DepartmentName" = "e0"."DepartmentName"
ORDER BY "e1"."DepartmentName"
""");
    }

    public override async Task Repeated_aggregate()
    {
        await base.Repeated_aggregate();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Name", COALESCE(SUM("e"."Salary") OVER(PARTITION BY "e"."DepartmentName"), 0), COUNT(*) OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Constant_key()
    {
        await base.Constant_key();

        AssertSql(
            """
SELECT "e0"."Key", DENSE_RANK() OVER(ORDER BY "e0"."Key"), "e0"."Id", "e0"."Bonus", "e0"."DepartmentName", "e0"."Name", "e0"."Salary", AVG(CAST("e0"."Salary" AS REAL)) OVER(PARTITION BY "e0"."Key")
FROM (
    SELECT "e"."Id", "e"."Bonus", "e"."DepartmentName", "e"."Name", "e"."Salary", 1 AS "Key"
    FROM "Employees" AS "e"
) AS "e0"
ORDER BY "e0"."Key"
""");
    }

    public override async Task Filter_before_grouping()
    {
        await base.Filter_before_grouping();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Id", "e"."Bonus", "e"."Name", "e"."Salary", AVG(CAST("e"."Salary" AS REAL)) OVER(PARTITION BY "e"."DepartmentName")
FROM "Employees" AS "e"
WHERE "e"."Salary" > 50
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Elements_and_aggregate_without_opt_in()
    {
        await base.Elements_and_aggregate_without_opt_in();

        AssertSql(
            """
SELECT "e1"."DepartmentName", "e0"."Id", "e0"."Bonus", "e0"."DepartmentName", "e0"."Name", "e0"."Salary", "e1"."c"
FROM (
    SELECT AVG(CAST("e"."Salary" AS REAL)) AS "c", "e"."DepartmentName"
    FROM "Employees" AS "e"
    GROUP BY "e"."DepartmentName"
) AS "e1"
LEFT JOIN "Employees" AS "e0" ON "e1"."DepartmentName" = "e0"."DepartmentName"
ORDER BY "e1"."DepartmentName"
""");
    }

    public override async Task Aggregate_without_elements()
    {
        await base.Aggregate_without_elements();

        AssertSql(
            """
SELECT "e"."DepartmentName" AS "Key", AVG(CAST("e"."Salary" AS REAL)) AS "AverageSalary"
FROM "Employees" AS "e"
GROUP BY "e"."DepartmentName"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Elements_without_aggregate()
    {
        await base.Elements_without_aggregate();

        AssertSql(
            """
SELECT "e"."DepartmentName", DENSE_RANK() OVER(ORDER BY "e"."DepartmentName"), "e"."Id", "e"."Bonus", "e"."Name", "e"."Salary"
FROM "Employees" AS "e"
ORDER BY "e"."DepartmentName"
""");
    }

    public override async Task Distinct_aggregate_is_not_lifted()
    {
        await base.Distinct_aggregate_is_not_lifted();

        AssertSql(
            """
SELECT "e1"."DepartmentName", "e0"."Id", "e0"."Bonus", "e0"."DepartmentName", "e0"."Name", "e0"."Salary", "e1"."c"
FROM (
    SELECT "e"."DepartmentName", COUNT(DISTINCT ("e"."Salary")) AS "c"
    FROM "Employees" AS "e"
    GROUP BY "e"."DepartmentName"
) AS "e1"
LEFT JOIN "Employees" AS "e0" ON "e1"."DepartmentName" = "e0"."DepartmentName"
ORDER BY "e1"."DepartmentName"
""");
    }

    public override async Task Filtered_aggregate_is_not_lifted()
    {
        await base.Filtered_aggregate_is_not_lifted();

        AssertSql(
            """
SELECT "e1"."DepartmentName", "e0"."Id", "e0"."Bonus", "e0"."DepartmentName", "e0"."Name", "e0"."Salary", "e1"."c"
FROM (
    SELECT "e"."DepartmentName", COUNT(CASE
        WHEN "e"."Salary" > 100 THEN 1
    END) AS "c"
    FROM "Employees" AS "e"
    GROUP BY "e"."DepartmentName"
) AS "e1"
LEFT JOIN "Employees" AS "e0" ON "e1"."DepartmentName" = "e0"."DepartmentName"
ORDER BY "e1"."DepartmentName"
""");
    }
}
