// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingMiscellaneousSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingMiscellaneousRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Where_related_property()
    {
        await base.Where_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_Int] = 8
""");
    }

    public override async Task Where_optional_related_property()
    {
        await base.Where_optional_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalRelated_Int] = 8
""");
    }

    public override async Task Where_nested_related_property()
    {
        await base.Where_nested_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelated_Id], [r].[OptionalRelated_Int], [r].[OptionalRelated_Name], [r].[OptionalRelated_String], [r].[OptionalRelated_OptionalNested_Id], [r].[OptionalRelated_OptionalNested_Int], [r].[OptionalRelated_OptionalNested_Name], [r].[OptionalRelated_OptionalNested_String], [r].[OptionalRelated_RequiredNested_Id], [r].[OptionalRelated_RequiredNested_Int], [r].[OptionalRelated_RequiredNested_Name], [r].[OptionalRelated_RequiredNested_String], [r].[RequiredRelated_Id], [r].[RequiredRelated_Int], [r].[RequiredRelated_Name], [r].[RequiredRelated_String], [r].[RequiredRelated_OptionalNested_Id], [r].[RequiredRelated_OptionalNested_Int], [r].[RequiredRelated_OptionalNested_Name], [r].[RequiredRelated_OptionalNested_String], [r].[RequiredRelated_RequiredNested_Id], [r].[RequiredRelated_RequiredNested_Int], [r].[RequiredRelated_RequiredNested_Name], [r].[RequiredRelated_RequiredNested_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredRelated_RequiredNested_Int] = 8
""");
    }

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String], [v].[RequiredRelated_Id], [v].[RequiredRelated_Int], [v].[RequiredRelated_Name], [v].[RequiredRelated_String], [v].[RequiredRelated_OptionalNested_Id], [v].[RequiredRelated_OptionalNested_Int], [v].[RequiredRelated_OptionalNested_Name], [v].[RequiredRelated_OptionalNested_String], [v].[RequiredRelated_RequiredNested_Id], [v].[RequiredRelated_RequiredNested_Int], [v].[RequiredRelated_RequiredNested_Name], [v].[RequiredRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[RequiredRelated_Int] = 8
""");
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String], [v].[RequiredRelated_Id], [v].[RequiredRelated_Int], [v].[RequiredRelated_Name], [v].[RequiredRelated_String], [v].[RequiredRelated_OptionalNested_Id], [v].[RequiredRelated_OptionalNested_Int], [v].[RequiredRelated_OptionalNested_Name], [v].[RequiredRelated_OptionalNested_String], [v].[RequiredRelated_RequiredNested_Id], [v].[RequiredRelated_RequiredNested_Int], [v].[RequiredRelated_RequiredNested_Name], [v].[RequiredRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalRelated_Int] = 8
""");
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalRelated_Id], [v].[OptionalRelated_Int], [v].[OptionalRelated_Name], [v].[OptionalRelated_String], [v].[OptionalRelated_OptionalNested_Id], [v].[OptionalRelated_OptionalNested_Int], [v].[OptionalRelated_OptionalNested_Name], [v].[OptionalRelated_OptionalNested_String], [v].[OptionalRelated_RequiredNested_Id], [v].[OptionalRelated_RequiredNested_Int], [v].[OptionalRelated_RequiredNested_Name], [v].[OptionalRelated_RequiredNested_String], [v].[RequiredRelated_Id], [v].[RequiredRelated_Int], [v].[RequiredRelated_Name], [v].[RequiredRelated_String], [v].[RequiredRelated_OptionalNested_Id], [v].[RequiredRelated_OptionalNested_Int], [v].[RequiredRelated_OptionalNested_Name], [v].[RequiredRelated_OptionalNested_String], [v].[RequiredRelated_RequiredNested_Id], [v].[RequiredRelated_RequiredNested_Int], [v].[RequiredRelated_RequiredNested_Name], [v].[RequiredRelated_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalRelated_Id] IS NOT NULL
""");
    }

    #endregion Value types

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
