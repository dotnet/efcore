// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingMiscellaneousSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingMiscellaneousRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_Int] = 8
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[OptionalAssociate_Int] = 8
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Int] = 8
""");
    }

    #region Value types

    public override async Task Where_property_on_non_nullable_value_type()
    {
        await base.Where_property_on_non_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_OptionalNested_Id], [v].[RequiredAssociate_OptionalNested_Int], [v].[RequiredAssociate_OptionalNested_Name], [v].[RequiredAssociate_OptionalNested_String], [v].[RequiredAssociate_RequiredNested_Id], [v].[RequiredAssociate_RequiredNested_Int], [v].[RequiredAssociate_RequiredNested_Name], [v].[RequiredAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[RequiredAssociate_Int] = 8
""");
    }

    public override async Task Where_property_on_nullable_value_type_Value()
    {
        await base.Where_property_on_nullable_value_type_Value();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_OptionalNested_Id], [v].[RequiredAssociate_OptionalNested_Int], [v].[RequiredAssociate_OptionalNested_Name], [v].[RequiredAssociate_OptionalNested_String], [v].[RequiredAssociate_RequiredNested_Id], [v].[RequiredAssociate_RequiredNested_Int], [v].[RequiredAssociate_RequiredNested_Name], [v].[RequiredAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate_Int] = 8
""");
    }

    public override async Task Where_HasValue_on_nullable_value_type()
    {
        await base.Where_HasValue_on_nullable_value_type();

        AssertSql(
            """
SELECT [v].[Id], [v].[Name], [v].[OptionalAssociate_Id], [v].[OptionalAssociate_Int], [v].[OptionalAssociate_Name], [v].[OptionalAssociate_String], [v].[OptionalAssociate_OptionalNested_Id], [v].[OptionalAssociate_OptionalNested_Int], [v].[OptionalAssociate_OptionalNested_Name], [v].[OptionalAssociate_OptionalNested_String], [v].[OptionalAssociate_RequiredNested_Id], [v].[OptionalAssociate_RequiredNested_Int], [v].[OptionalAssociate_RequiredNested_Name], [v].[OptionalAssociate_RequiredNested_String], [v].[RequiredAssociate_Id], [v].[RequiredAssociate_Int], [v].[RequiredAssociate_Name], [v].[RequiredAssociate_String], [v].[RequiredAssociate_OptionalNested_Id], [v].[RequiredAssociate_OptionalNested_Int], [v].[RequiredAssociate_OptionalNested_Name], [v].[RequiredAssociate_OptionalNested_String], [v].[RequiredAssociate_RequiredNested_Id], [v].[RequiredAssociate_RequiredNested_Int], [v].[RequiredAssociate_RequiredNested_Name], [v].[RequiredAssociate_RequiredNested_String]
FROM [ValueRootEntity] AS [v]
WHERE [v].[OptionalAssociate_Id] IS NOT NULL
""");
    }

    #endregion Value types

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT * FROM [RootEntity]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
