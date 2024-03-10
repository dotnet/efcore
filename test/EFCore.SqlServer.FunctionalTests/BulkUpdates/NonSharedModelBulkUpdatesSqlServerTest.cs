// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class NonSharedModelBulkUpdatesSqlServerTest : NonSharedModelBulkUpdatesTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_aggregate_root_when_eager_loaded_owned_collection(bool async)
    {
        await base.Delete_aggregate_root_when_eager_loaded_owned_collection(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Owner] AS [o]
""");
    }

    public override async Task Delete_with_owned_collection_and_non_natively_translatable_query(bool async)
    {
        await base.Delete_with_owned_collection_and_non_natively_translatable_query(async);

        AssertSql(
            """
@__p_0='1'

DELETE FROM [o]
FROM [Owner] AS [o]
WHERE [o].[Id] IN (
    SELECT [o0].[Id]
    FROM [Owner] AS [o0]
    ORDER BY [o0].[Title]
    OFFSET @__p_0 ROWS
)
""");
    }

    public override async Task Delete_aggregate_root_when_table_sharing_with_owned(bool async)
    {
        await base.Delete_aggregate_root_when_table_sharing_with_owned(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Owner] AS [o]
""");
    }

    public override async Task Delete_aggregate_root_when_table_sharing_with_non_owned_throws(bool async)
    {
        await base.Delete_aggregate_root_when_table_sharing_with_non_owned_throws(async);

        AssertSql();
    }

    public override async Task Update_non_owned_property_on_entity_with_owned(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned(async);

        AssertSql(
            """
UPDATE [o]
SET [o].[Title] = N'SomeValue'
FROM [Owner] AS [o]
""");
    }

    public override async Task Update_non_owned_property_on_entity_with_owned2(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned2(async);

        AssertSql(
            """
UPDATE [o]
SET [o].[Title] = COALESCE([o].[Title], N'') + N'_Suffix'
FROM [Owner] AS [o]
""");
    }

    public override async Task Update_non_owned_property_on_entity_with_owned_in_join(bool async)
    {
        await base.Update_non_owned_property_on_entity_with_owned_in_join(async);

        AssertSql(
            """
UPDATE [o]
SET [o].[Title] = N'NewValue'
FROM [Owner] AS [o]
INNER JOIN [Owner] AS [o0] ON [o].[Id] = [o0].[Id]
""");
    }

    public override async Task Update_owned_and_non_owned_properties_with_table_sharing(bool async)
    {
        await base.Update_owned_and_non_owned_properties_with_table_sharing(async);

        AssertSql(
            """
UPDATE [o]
SET [o].[OwnedReference_Number] = CAST(LEN([o].[Title]) AS int),
    [o].[Title] = CONVERT(varchar(11), [o].[OwnedReference_Number])
FROM [Owner] AS [o]
""");
    }

    public override async Task Update_main_table_in_entity_with_entity_splitting(bool async)
    {
        await base.Update_main_table_in_entity_with_entity_splitting(async);

        AssertSql(
            """
UPDATE [b]
SET [b].[CreationTimestamp] = '2020-01-01T00:00:00.0000000'
FROM [Blogs] AS [b]
""");
    }

    public override async Task Update_non_main_table_in_entity_with_entity_splitting(bool async)
    {
        await base.Update_non_main_table_in_entity_with_entity_splitting(async);

        AssertSql(
            """
UPDATE [b0]
SET [b0].[Rating] = CAST(LEN([b0].[Title]) AS int),
    [b0].[Title] = CONVERT(varchar(11), [b0].[Rating])
FROM [Blogs] AS [b]
INNER JOIN [BlogsPart1] AS [b0] ON [b].[Id] = [b0].[Id]
""");
    }

    public override async Task Delete_entity_with_auto_include(bool async)
    {
        await base.Delete_entity_with_auto_include(async);

        AssertSql(
            """
DELETE FROM [c]
FROM [Context30572_Principal] AS [c]
LEFT JOIN [Context30572_Dependent] AS [c0] ON [c].[DependentId] = [c0].[Id]
""");
    }

    public override async Task Delete_predicate_based_on_optional_navigation(bool async)
    {
        await base.Delete_predicate_based_on_optional_navigation(async);

        AssertSql(
            """
DELETE FROM [p]
FROM [Posts] AS [p]
LEFT JOIN [Blogs] AS [b] ON [p].[BlogId] = [b].[Id]
WHERE [b].[Title] LIKE N'Arthur%'
""");
    }

    public override async Task Update_with_alias_uniquification_in_setter_subquery(bool async)
    {
        await base.Update_with_alias_uniquification_in_setter_subquery(async);

        AssertSql(
            """
UPDATE [o]
SET [o].[Total] = (
    SELECT COALESCE(SUM([o0].[Amount]), 0)
    FROM [OrderProduct] AS [o0]
    WHERE [o].[Id] = [o0].[OrderId])
FROM [Orders] AS [o]
WHERE [o].[Id] = 1
""");
    }

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
