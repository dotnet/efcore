// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQueryFiltersQuerySqliteTest(NonSharedFixture fixture) : AdHocQueryFiltersQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task GroupBy_aggregate_over_required_navigation_with_query_filter(bool async)
    {
        await base.GroupBy_aggregate_over_required_navigation_with_query_filter(async);

        AssertSql(
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", (
    SELECT MAX("p"."Value")
    FROM "Dependents" AS "d0"
    INNER JOIN "Principals" AS "p" ON "d0"."PrincipalId" = "p"."Id" AND NOT ("p"."Filtered")
    WHERE "d"."GroupId" = "d0"."GroupId") AS "MaxValue"
FROM "Dependents" AS "d"
GROUP BY "d"."GroupId"
""",
            //
            """
SELECT "d"."GroupId" AS "Key", COUNT(*) AS "Count", MAX("p"."Value") AS "MaxValue"
FROM "Dependents" AS "d"
INNER JOIN "Principals" AS "p" ON "d"."PrincipalId" = "p"."Id"
GROUP BY "d"."GroupId"
""");
    }
}
