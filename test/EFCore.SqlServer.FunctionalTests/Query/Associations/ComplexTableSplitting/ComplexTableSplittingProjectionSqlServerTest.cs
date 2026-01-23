// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingProjectionSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingProjectionRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    #region Scalar properties

    public override async Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_scalar_property_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_property_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_value_type_property_on_null_associate_throws(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Int]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_property_on_null_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Int]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Scalar properties

    #region Structural properties

    public override async Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_nested_on_optional_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_associate_via_optional_navigation(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r0].[RequiredAssociate_OptionalNestedAssociate_Id], [r0].[RequiredAssociate_OptionalNestedAssociate_Int], [r0].[RequiredAssociate_OptionalNestedAssociate_Ints], [r0].[RequiredAssociate_OptionalNestedAssociate_Name], [r0].[RequiredAssociate_OptionalNestedAssociate_String], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootReferencingEntity] AS [r]
LEFT JOIN [RootEntity] AS [r0] ON [r].[RootEntityId] = [r0].[Id]
""");
    }

    public override async Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_unmapped_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_untranslatable_method_on_associate_scalar_property(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Int]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Structural properties

    #region Structural collection properties

    public override async Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_collection(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        // Note that collections are not supported with table splitting, only JSON;
        // the collection selected here is unmapped, and so the test does client evaluation.
        await base.Select_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nested_collection_on_optional_associate(queryTrackingBehavior);

        // Note that collections are not supported with table splitting, only JSON;
        // the collection selected here is unmapped, and so the test does client evaluation.
        AssertSql(
            """
SELECT [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_associate_collection(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_required_associate(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_nested_collection_on_optional_associate(queryTrackingBehavior);

        AssertSql();
    }

    #endregion Structural collection properties

    #region Multiple

    public override async Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_associate_and_target_to_index_based_binding_via_closure(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
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
SELECT [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[OptionalAssociate_RequiredNestedAssociate_Id], [r0].[OptionalAssociate_RequiredNestedAssociate_Int], [r0].[OptionalAssociate_RequiredNestedAssociate_Ints], [r0].[OptionalAssociate_RequiredNestedAssociate_Name], [r0].[OptionalAssociate_RequiredNestedAssociate_String]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    #endregion Subquery

    #region Value types

    public override async Task Select_root_with_value_types(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_with_value_types(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_OptionalNested_Id], [v].[RequiredAssociate_OptionalNested_Int], [v].[RequiredAssociate_OptionalNested_Name], [v].[RequiredAssociate_OptionalNested_String], [v].[RequiredAssociate_RequiredNested_Id], [v].[RequiredAssociate_RequiredNested_Int], [v].[RequiredAssociate_RequiredNested_Name], [v].[RequiredAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
""");
    }

    public override async Task Select_non_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_non_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_OptionalNested_Id], [v].[RequiredAssociate_OptionalNested_Int], [v].[RequiredAssociate_OptionalNested_Name], [v].[RequiredAssociate_OptionalNested_String], [v].[RequiredAssociate_RequiredNested_Id], [v].[RequiredAssociate_RequiredNested_Int], [v].[RequiredAssociate_RequiredNested_Name], [v].[RequiredAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    public override async Task Select_nullable_value_type_with_Value(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_nullable_value_type_with_Value(queryTrackingBehavior);

        AssertSql(
            """
SELECT [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
ORDER BY [v].[Id]
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
