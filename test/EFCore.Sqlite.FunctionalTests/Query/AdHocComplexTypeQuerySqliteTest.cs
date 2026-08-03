// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocComplexTypeQuerySqliteTest(NonSharedFixture fixture) : AdHocComplexTypeQueryRelationalTestBase(fixture)
{
    public override async Task Complex_property_on_split_entity()
    {
        await base.Complex_property_on_split_entity();

        AssertSql(
            """
SELECT "h"."Id", "h"."CreatedOn", "h0"."IsTestHook", "h0"."Weight", "h"."Number_Parsed", "h"."Number_Raw"
FROM "Hook" AS "h"
INNER JOIN "HookMetadata" AS "h0" ON "h"."Id" = "h0"."HookId"
""");
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
