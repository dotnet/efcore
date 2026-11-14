// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocPrecompiledQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : AdHocPrecompiledQueryRelationalTestBase(fixture, testOutputHelper)
{
    protected override bool AlwaysPrintGeneratedSources
        => false;

    protected override DbContextOptionsBuilder UseWindowFunctionAggregates(DbContextOptionsBuilder optionsBuilder)
    {
#pragma warning disable EF9001 // UseWindowFunctionAggregates is experimental
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseWindowFunctionAggregates();
#pragma warning restore EF9001
        return optionsBuilder;
    }

    public override async Task Coalesce_nullable_long_in_anonymous_projection()
    {
        await base.Coalesce_nullable_long_in_anonymous_projection();

        AssertSql(
            """
SELECT COALESCE("r"."SizeBytes", 0) AS "SizeBytes"
FROM "Records" AS "r"
ORDER BY "r"."Id"
""");
    }

    public override async Task GroupBy_element_selector_with_aggregates_through_navigation()
    {
        await base.GroupBy_element_selector_with_aggregates_through_navigation();

        AssertSql(
            """
SELECT "o"."EmployeeId" AS "Key", MAX("g"."Region") AS "Region", COALESCE(SUM("o"."Amount"), 0) AS "Total", MAX(CASE
    WHEN "g"."Region" IS NULL THEN 0
    ELSE "o"."EmployeeId"
END) AS "KeyWhereRegion"
FROM "Orders" AS "o"
INNER JOIN "GroupByNavigationCustomer" AS "g" ON "o"."CustomerId" = "g"."Id"
GROUP BY "o"."EmployeeId"
ORDER BY "o"."EmployeeId"
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers
        => SqlitePrecompiledQueryTestHelpers.Instance;
}
