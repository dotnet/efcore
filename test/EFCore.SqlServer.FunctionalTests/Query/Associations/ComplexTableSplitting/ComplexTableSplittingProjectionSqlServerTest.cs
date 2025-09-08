// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingProjectionSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingProjectionRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_property_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_property_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_value_type_property_on_null_related_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_related_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_Int]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_Int]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[RequiredRelated_Id], [r0].[RequiredRelated_Int], [r0].[RequiredRelated_Name], [r0].[RequiredRelated_String], [r0].[RequiredRelated_OptionalNested_Id], [r0].[RequiredRelated_OptionalNested_Int], [r0].[RequiredRelated_OptionalNested_Name], [r0].[RequiredRelated_OptionalNested_String], [r0].[RequiredRelated_RequiredNested_Id], [r0].[RequiredRelated_RequiredNested_Int], [r0].[RequiredRelated_RequiredNested_Name], [r0].[RequiredRelated_RequiredNested_String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_related(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[RequiredRelated_RequiredNested_Id], [r1].[RequiredRelated_RequiredNested_Int], [r1].[RequiredRelated_RequiredNested_Name], [r1].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[RequiredRelated_RequiredNested_Id], [r0].[RequiredRelated_RequiredNested_Int], [r0].[RequiredRelated_RequiredNested_Name], [r0].[RequiredRelated_RequiredNested_String]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[OptionalRelated_RequiredNested_Id], [r1].[OptionalRelated_RequiredNested_Int], [r1].[OptionalRelated_RequiredNested_Name], [r1].[OptionalRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[OptionalRelated_RequiredNested_Id], [r0].[OptionalRelated_RequiredNested_Int], [r0].[OptionalRelated_RequiredNested_Name], [r0].[OptionalRelated_RequiredNested_String]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    #region Value types

    public override async Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_with_value_types(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String], [v].[RequiredRelated_Id], [v].[RequiredRelated_Int], [v].[RequiredRelated_Name], [v].[RequiredRelated_String], [v].[RequiredRelated_OptionalNested_Id], [v].[RequiredRelated_OptionalNested_Int], [v].[RequiredRelated_OptionalNested_Name], [v].[RequiredRelated_OptionalNested_String], [v].[RequiredRelated_RequiredNested_Id], [v].[RequiredRelated_RequiredNested_Int], [v].[RequiredRelated_RequiredNested_Name], [v].[RequiredRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
""");
    }

    public override async Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_non_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[RequiredRelated_Id], [v].[RequiredRelated_Int], [v].[RequiredRelated_Name], [v].[RequiredRelated_String], [v].[RequiredRelated_OptionalNested_Id], [v].[RequiredRelated_OptionalNested_Int], [v].[RequiredRelated_OptionalNested_Name], [v].[RequiredRelated_OptionalNested_String], [v].[RequiredRelated_RequiredNested_Id], [v].[RequiredRelated_RequiredNested_Int], [v].[RequiredRelated_RequiredNested_Name], [v].[RequiredRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_with_Value(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
