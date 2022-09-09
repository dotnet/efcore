﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class FiltersInheritanceBulkUpdatesSqlServerTest : FiltersInheritanceBulkUpdatesTestBase<
    FiltersInheritanceBulkUpdatesSqlServerFixture>
{
    public FiltersInheritanceBulkUpdatesSqlServerTest(FiltersInheritanceBulkUpdatesSqlServerFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql(
            @"DELETE FROM [a]
FROM [Animals] AS [a]
WHERE [a].[CountryId] = 1 AND [a].[Name] = N'Great spotted kiwi'");
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql(
            @"DELETE FROM [a]
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi' AND [a].[CountryId] = 1 AND [a].[Name] = N'Great spotted kiwi'");
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
            @"DELETE FROM [c]
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Animals] AS [a]
    WHERE [a].[CountryId] = 1 AND [c].[Id] = [a].[CountryId] AND [a].[CountryId] > 0) > 0");
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);

        AssertSql(
            @"DELETE FROM [c]
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Animals] AS [a]
    WHERE [a].[CountryId] = 1 AND [c].[Id] = [a].[CountryId] AND [a].[Discriminator] = N'Kiwi' AND [a].[CountryId] > 0) > 0");
    }

    public override async Task Delete_GroupBy_Where_Select_First(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_2(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_2(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_First_3(bool async)
    {
        await base.Delete_GroupBy_Where_Select_First_3(async);

        AssertSql(
            @"DELETE FROM [a]
FROM [Animals] AS [a]
WHERE [a].[CountryId] = 1 AND EXISTS (
    SELECT 1
    FROM [Animals] AS [a0]
    WHERE [a0].[CountryId] = 1
    GROUP BY [a0].[CountryId]
    HAVING COUNT(*) < 3 AND (
        SELECT TOP(1) [a1].[Id]
        FROM [Animals] AS [a1]
        WHERE [a1].[CountryId] = 1 AND [a0].[CountryId] = [a1].[CountryId]) = [a].[Id])");
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='3'

DELETE FROM [a]
FROM [Animals] AS [a]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [a0].[Id], [a0].[CountryId], [a0].[Discriminator], [a0].[Name], [a0].[Species], [a0].[EagleId], [a0].[IsFlightless], [a0].[Group], [a0].[FoundOn]
        FROM [Animals] AS [a0]
        WHERE [a0].[CountryId] = 1 AND [a0].[Name] = N'Great spotted kiwi'
        ORDER BY [a0].[Name]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [t]
    WHERE [t].[Id] = [a].[Id])");
    }

    public override async Task Update_where_hierarchy(bool async)
    {
        await base.Update_where_hierarchy(async);

        AssertExecuteUpdateSql(
            @"UPDATE [a]
SET [a].[Name] = N'Animal'
FROM [Animals] AS [a]
WHERE [a].[CountryId] = 1 AND [a].[Name] = N'Great spotted kiwi'");
    }

    public override async Task Update_where_hierarchy_subquery(bool async)
    {
        await base.Update_where_hierarchy_subquery(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_hierarchy_derived(bool async)
    {
        await base.Update_where_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            @"UPDATE [a]
SET [a].[Name] = N'Kiwi'
FROM [Animals] AS [a]
WHERE [a].[Discriminator] = N'Kiwi' AND [a].[CountryId] = 1 AND [a].[Name] = N'Great spotted kiwi'");
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);

        AssertExecuteUpdateSql(
            @"UPDATE [c]
SET [c].[Name] = N'Monovia'
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Animals] AS [a]
    WHERE [a].[CountryId] = 1 AND [c].[Id] = [a].[CountryId] AND [a].[CountryId] > 0) > 0");
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            @"UPDATE [c]
SET [c].[Name] = N'Monovia'
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Animals] AS [a]
    WHERE [a].[CountryId] = 1 AND [c].[Id] = [a].[CountryId] AND [a].[Discriminator] = N'Kiwi' AND [a].[CountryId] > 0) > 0");
    }

    public override async Task Update_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Update_where_keyless_entity_mapped_to_sql_query(async);

        AssertExecuteUpdateSql();
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
