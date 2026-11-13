// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQuerySqlServerTest(NonSharedFixture fixture, ITestOutputHelper output) : MinMaxByQueryTestBase(fixture, output)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Nullable_keys(bool async, bool maximum, bool stringKey, int scenario)
    {
        await base.Nullable_keys(async, maximum, stringKey, scenario);

        var key = stringKey ? "StringKey" : "Key";
        AssertSql(
            scenario == 4
                ? $$"""
@scenario='4'
@p='1'
@p2='2'

SELECT TOP(1) [i0].[Id], [i0].[Key], [i0].[Scenario], [i0].[StringKey]
FROM (
    SELECT [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
    FROM [Item] AS [i]
    WHERE [i].[Scenario] = @scenario
    ORDER BY [i].[Scenario], [i].[Id] DESC
    OFFSET @p ROWS FETCH NEXT @p2 ROWS ONLY
) AS [i0]
ORDER BY CASE
    WHEN [i0].[{{key}}] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [i0].[{{key}}]{{(maximum ? " DESC" : "")}}, [i0].[Scenario], [i0].[Id] DESC
"""
                : $$"""
@scenario='{{scenario}}'

SELECT TOP(1) [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
FROM [Item] AS [i]
WHERE [i].[Scenario] = @scenario
ORDER BY CASE
    WHEN [i].[{{key}}] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [i].[{{key}}]{{(maximum ? " DESC" : "")}}, [i].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
