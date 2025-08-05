// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public class ComplexTableSplittingMiscellaneousSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingMiscellaneousRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_related_property()
    {
        await base.Where_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_Int] = 8
""");
    }

    public override async Task Where_optional_related_property()
    {
        await base.Where_optional_related_property();

        AssertSql();
    }

    public override async Task Where_nested_related_property()
    {
        await base.Where_nested_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_RequiredNested_Int] = 8
""");
    }

    #endregion Simple filters

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
