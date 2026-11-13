// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper output) : MinMaxByQueryTestBase(fixture, output)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Nullable_keys(bool async, bool maximum, bool stringKey, int scenario)
    {
        await base.Nullable_keys(async, maximum, stringKey, scenario);

        var key = stringKey ? "StringKey" : "Key";
        AssertSql(
            scenario == 4
                ? $$"""
@scenario='4'
@p2='2'
@p='1'

SELECT "i0"."Id", "i0"."Key", "i0"."Scenario", "i0"."StringKey"
FROM (
    SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
    FROM "Item" AS "i"
    WHERE "i"."Scenario" = @scenario
    ORDER BY "i"."Scenario", "i"."Id" DESC
    LIMIT @p2 OFFSET @p
) AS "i0"
ORDER BY "i0"."{{key}}" IS NULL, "i0"."{{key}}"{{(maximum ? " DESC" : "")}}, "i0"."Scenario", "i0"."Id" DESC
LIMIT 1
"""
                : $$"""
@scenario='{{scenario}}'

SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
FROM "Item" AS "i"
WHERE "i"."Scenario" = @scenario
ORDER BY "i"."{{key}}" IS NULL, "i"."{{key}}"{{(maximum ? " DESC" : "")}}, "i"."Id"
LIMIT 1
""");
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
