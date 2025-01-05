// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class TPCFiltersInheritanceBulkUpdatesSqlServerTest(
    TPCFiltersInheritanceBulkUpdatesSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : TPCFiltersInheritanceBulkUpdatesTestBase<TPCFiltersInheritanceBulkUpdatesSqlServerFixture>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_where_hierarchy(bool async)
    {
        await base.Delete_where_hierarchy(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_derived(bool async)
    {
        await base.Delete_where_hierarchy_derived(async);

        AssertSql(
            """
DELETE FROM [k]
FROM [Kiwi] AS [k]
WHERE [k].[CountryId] = 1 AND [k].[Name] = N'Great spotted kiwi'
""");
    }

    public override async Task Delete_where_using_hierarchy(bool async)
    {
        await base.Delete_where_using_hierarchy(async);

        AssertSql(
            """
DELETE FROM [c]
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [e].[CountryId]
        FROM [Eagle] AS [e]
        UNION ALL
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [u].[CountryId] = 1 AND [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
    }

    public override async Task Delete_where_using_hierarchy_derived(bool async)
    {
        await base.Delete_where_using_hierarchy_derived(async);

        AssertSql(
            """
DELETE FROM [c]
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [u].[CountryId] = 1 AND [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
    }

    public override async Task Delete_where_keyless_entity_mapped_to_sql_query(bool async)
    {
        await base.Delete_where_keyless_entity_mapped_to_sql_query(async);

        AssertSql();
    }

    public override async Task Delete_where_hierarchy_subquery(bool async)
    {
        await base.Delete_where_hierarchy_subquery(async);

        AssertSql();
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

        AssertSql();
    }

    public override async Task Update_base_type(bool async)
    {
        await base.Update_base_type(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_base_type_with_OfType(bool async)
    {
        await base.Update_base_type_with_OfType(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_hierarchy_subquery(bool async)
    {
        await base.Update_where_hierarchy_subquery(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_base_property_on_derived_type(bool async)
    {
        await base.Update_base_property_on_derived_type(async);

        AssertExecuteUpdateSql(
            """
UPDATE [k]
SET [k].[Name] = N'SomeOtherKiwi'
FROM [Kiwi] AS [k]
WHERE [k].[CountryId] = 1
""");
    }

    public override async Task Update_derived_property_on_derived_type(bool async)
    {
        await base.Update_derived_property_on_derived_type(async);

        AssertExecuteUpdateSql(
            """
UPDATE [k]
SET [k].[FoundOn] = CAST(0 AS tinyint)
FROM [Kiwi] AS [k]
WHERE [k].[CountryId] = 1
""");
    }

    public override async Task Update_where_using_hierarchy(bool async)
    {
        await base.Update_where_using_hierarchy(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[Name] = N'Monovia'
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [e].[CountryId]
        FROM [Eagle] AS [e]
        UNION ALL
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [u].[CountryId] = 1 AND [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
    }

    public override async Task Update_base_and_derived_types(bool async)
    {
        await base.Update_base_and_derived_types(async);

        AssertExecuteUpdateSql(
            """
UPDATE [k]
SET [k].[FoundOn] = CAST(0 AS tinyint),
    [k].[Name] = N'Kiwi'
FROM [Kiwi] AS [k]
WHERE [k].[CountryId] = 1
""");
    }

    public override async Task Update_where_using_hierarchy_derived(bool async)
    {
        await base.Update_where_using_hierarchy_derived(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[Name] = N'Monovia'
FROM [Countries] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [k].[CountryId]
        FROM [Kiwi] AS [k]
    ) AS [u]
    WHERE [u].[CountryId] = 1 AND [c].[Id] = [u].[CountryId] AND [u].[CountryId] > 0) > 0
""");
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
