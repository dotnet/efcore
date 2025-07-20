// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public class ComplexTableSplittingProjectionSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingProjectionRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    #region Simple properties

    public override async Task Select_related_property(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_property(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related_property(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_property(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related_property_value_type(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_property_value_type(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Simple properties

    #region Non-collection

    public override async Task Select_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_related_required_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_required_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_required_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_optional_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related_required_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_required_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related_optional_nested(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_optional_nested(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Non-collection

    #region Collection

    public override async Task Select_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_related_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_required_related_nested_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Select_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_optional_related_nested_collection(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    public override async Task SelectMany_related_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_related_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_required_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_required_related_nested_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task SelectMany_optional_related_nested_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.SelectMany_optional_related_nested_collection(async, queryTrackingBehavior);

        AssertSql();
    }

    #endregion Collection

    #region Multiple

    public override async Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_root_duplicated(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
""");
    }

    #endregion Multiple

    #region Subquery

    public override async Task Select_subquery_required_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_required_related_FirstOrDefault(async, queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Select_subquery_optional_related_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Select_subquery_optional_related_FirstOrDefault(async, queryTrackingBehavior);

        AssertSql(
            """
SELECT [r1].[Id], [r1].[Name], [r1].[RequiredRelated_Id], [r1].[RequiredRelated_Int], [r1].[RequiredRelated_Name], [r1].[RequiredRelated_String], [r1].[RequiredRelated_RequiredNested_Id], [r1].[RequiredRelated_RequiredNested_Int], [r1].[RequiredRelated_RequiredNested_Name], [r1].[RequiredRelated_RequiredNested_String], [r1].[c]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT TOP(1) [r0].[Id], [r0].[Name], [r0].[RequiredRelated_Id], [r0].[RequiredRelated_Int], [r0].[RequiredRelated_Name], [r0].[RequiredRelated_String], [r0].[RequiredRelated_RequiredNested_Id], [r0].[RequiredRelated_RequiredNested_Int], [r0].[RequiredRelated_RequiredNested_Name], [r0].[RequiredRelated_RequiredNested_String], 1 AS [c]
    FROM [RootEntity] AS [r0]
    ORDER BY [r0].[Id]
) AS [r1]
""");
    }

    #endregion Subquery

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
