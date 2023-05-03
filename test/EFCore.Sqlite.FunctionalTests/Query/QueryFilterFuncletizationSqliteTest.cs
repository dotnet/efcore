// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class QueryFilterFuncletizationSqliteTest : QueryFilterFuncletizationTestBase<
    QueryFilterFuncletizationSqliteTest.QueryFilterFuncletizationSqliteFixture>
{
    public QueryFilterFuncletizationSqliteTest(
        QueryFilterFuncletizationSqliteFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override void Using_multiple_entities_with_filters_reuses_parameters()
    {
        base.Using_multiple_entities_with_filters_reuses_parameters();

        AssertSql(
"""
@__ef_filter__Tenant_0='1'

SELECT "d"."Id", "d"."Tenant", "t"."Id", "t"."DeDupeFilter1Id", "t"."TenantX", "t0"."Id", "t0"."DeDupeFilter1Id", "t0"."Tenant"
FROM "DeDupeFilter1" AS "d"
LEFT JOIN (
    SELECT "d0"."Id", "d0"."DeDupeFilter1Id", "d0"."TenantX"
    FROM "DeDupeFilter2" AS "d0"
    WHERE "d0"."TenantX" = @__ef_filter__Tenant_0
) AS "t" ON "d"."Id" = "t"."DeDupeFilter1Id"
LEFT JOIN (
    SELECT "d1"."Id", "d1"."DeDupeFilter1Id", "d1"."Tenant"
    FROM "DeDupeFilter3" AS "d1"
    WHERE "d1"."Tenant" = @__ef_filter__Tenant_0
) AS "t0" ON "d"."Id" = "t0"."DeDupeFilter1Id"
WHERE "d"."Tenant" = @__ef_filter__Tenant_0
ORDER BY "d"."Id", "t"."Id"
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class QueryFilterFuncletizationSqliteFixture : QueryFilterFuncletizationRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
