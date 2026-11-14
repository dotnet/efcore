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

SELECT TOP(1) [u].[Id], [u].[Key], [u].[Scenario], [u].[StringKey]
FROM (
    SELECT [i5].[Id], [i5].[Key], [i5].[Scenario], [i5].[StringKey]
    FROM (
        SELECT TOP(1) [i0].[Id], [i0].[Key], [i0].[Scenario], [i0].[StringKey]
        FROM (
            SELECT [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
            FROM [Item] AS [i]
            WHERE [i].[Scenario] = @scenario
            ORDER BY [i].[Scenario], [i].[Id] DESC
            OFFSET @p ROWS FETCH NEXT @p2 ROWS ONLY
        ) AS [i0]
        WHERE [i0].[{{key}}] IS NOT NULL AND [i0].[{{key}}] = (
            SELECT TOP(1) [i2].[{{key}}]
            FROM (
                SELECT [i1].[{{key}}]
                FROM [Item] AS [i1]
                WHERE [i1].[Scenario] = @scenario
                ORDER BY [i1].[Scenario], [i1].[Id] DESC
                OFFSET @p ROWS FETCH NEXT @p2 ROWS ONLY
            ) AS [i2]
            WHERE [i2].[{{key}}] IS NOT NULL
            ORDER BY [i2].[{{key}}]{{(maximum ? " DESC" : "")}})
        ORDER BY [i0].[Scenario], [i0].[Id] DESC
    ) AS [i5]
    UNION ALL
    SELECT [i6].[Id], [i6].[Key], [i6].[Scenario], [i6].[StringKey]
    FROM (
        SELECT TOP(1) [i4].[Id], [i4].[Key], [i4].[Scenario], [i4].[StringKey]
        FROM (
            SELECT [i3].[Id], [i3].[Key], [i3].[Scenario], [i3].[StringKey]
            FROM [Item] AS [i3]
            WHERE [i3].[Scenario] = @scenario
            ORDER BY [i3].[Scenario], [i3].[Id] DESC
            OFFSET @p ROWS FETCH NEXT @p2 ROWS ONLY
        ) AS [i4]
        WHERE [i4].[{{key}}] IS NULL
        ORDER BY [i4].[Scenario], [i4].[Id] DESC
    ) AS [i6]
) AS [u]
ORDER BY CASE
    WHEN [u].[{{key}}] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
"""
                : $$"""
@scenario='{{scenario}}'

SELECT TOP(1) [u].[Id], [u].[Key], [u].[Scenario], [u].[StringKey]
FROM (
    SELECT [i2].[Id], [i2].[Key], [i2].[Scenario], [i2].[StringKey]
    FROM (
        SELECT TOP(1) [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
        FROM [Item] AS [i]
        WHERE [i].[Scenario] = @scenario AND [i].[{{key}}] IS NOT NULL AND [i].[{{key}}] = (
            SELECT TOP(1) [i0].[{{key}}]
            FROM [Item] AS [i0]
            WHERE [i0].[Scenario] = @scenario AND [i0].[{{key}}] IS NOT NULL
            ORDER BY [i0].[{{key}}]{{(maximum ? " DESC" : "")}})
        ORDER BY [i].[Id]{{(scenario == 5 ? " DESC" : "")}}
    ) AS [i2]
    UNION ALL
    SELECT [i3].[Id], [i3].[Key], [i3].[Scenario], [i3].[StringKey]
    FROM (
        SELECT TOP(1) [i1].[Id], [i1].[Key], [i1].[Scenario], [i1].[StringKey]
        FROM [Item] AS [i1]
        WHERE [i1].[Scenario] = @scenario AND [i1].[{{key}}] IS NULL
        ORDER BY [i1].[Id]{{(scenario == 5 ? " DESC" : "")}}
    ) AS [i3]
) AS [u]
ORDER BY CASE
    WHEN [u].[{{key}}] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
