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

    #region Subquery

    public override async Task Where_on_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r7].[Id], [r7].[Name], [r7].[OptionalAssociate_Id], [r7].[OptionalAssociate_Int], [r7].[OptionalAssociate_Ints], [r7].[OptionalAssociate_Name], [r7].[OptionalAssociate_String], [r7].[OptionalAssociate_OptionalNestedAssociate_Id], [r7].[OptionalAssociate_OptionalNestedAssociate_Int], [r7].[OptionalAssociate_OptionalNestedAssociate_Ints], [r7].[OptionalAssociate_OptionalNestedAssociate_Name], [r7].[OptionalAssociate_OptionalNestedAssociate_String], [r7].[OptionalAssociate_RequiredNestedAssociate_Id], [r7].[OptionalAssociate_RequiredNestedAssociate_Int], [r7].[OptionalAssociate_RequiredNestedAssociate_Ints], [r7].[OptionalAssociate_RequiredNestedAssociate_Name], [r7].[OptionalAssociate_RequiredNestedAssociate_String], [r7].[RequiredAssociate_Id], [r7].[RequiredAssociate_Int], [r7].[RequiredAssociate_Ints], [r7].[RequiredAssociate_Name], [r7].[RequiredAssociate_String], [r7].[RequiredAssociate_OptionalNestedAssociate_Id], [r7].[RequiredAssociate_OptionalNestedAssociate_Int], [r7].[RequiredAssociate_OptionalNestedAssociate_Ints], [r7].[RequiredAssociate_OptionalNestedAssociate_Name], [r7].[RequiredAssociate_OptionalNestedAssociate_String], [r7].[RequiredAssociate_RequiredNestedAssociate_Id], [r7].[RequiredAssociate_RequiredNestedAssociate_Int], [r7].[RequiredAssociate_RequiredNestedAssociate_Ints], [r7].[RequiredAssociate_RequiredNestedAssociate_Name], [r7].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r3].[RequiredAssociate_Int]
        FROM (
            SELECT [r4].[Id], [r4].[RequiredAssociate_Int], [r4].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r4]
        ) AS [r3]
        WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
        ORDER BY [r3].[Id]) = 8
) AS [r5]
LEFT JOIN (
    SELECT [r6].[Id], [r6].[Name], [r6].[OptionalAssociate_Id], [r6].[OptionalAssociate_Int], [r6].[OptionalAssociate_Ints], [r6].[OptionalAssociate_Name], [r6].[OptionalAssociate_String], [r6].[OptionalAssociate_OptionalNestedAssociate_Id], [r6].[OptionalAssociate_OptionalNestedAssociate_Int], [r6].[OptionalAssociate_OptionalNestedAssociate_Ints], [r6].[OptionalAssociate_OptionalNestedAssociate_Name], [r6].[OptionalAssociate_OptionalNestedAssociate_String], [r6].[OptionalAssociate_RequiredNestedAssociate_Id], [r6].[OptionalAssociate_RequiredNestedAssociate_Int], [r6].[OptionalAssociate_RequiredNestedAssociate_Ints], [r6].[OptionalAssociate_RequiredNestedAssociate_Name], [r6].[OptionalAssociate_RequiredNestedAssociate_String], [r6].[RequiredAssociate_Id], [r6].[RequiredAssociate_Int], [r6].[RequiredAssociate_Ints], [r6].[RequiredAssociate_Name], [r6].[RequiredAssociate_String], [r6].[RequiredAssociate_OptionalNestedAssociate_Id], [r6].[RequiredAssociate_OptionalNestedAssociate_Int], [r6].[RequiredAssociate_OptionalNestedAssociate_Ints], [r6].[RequiredAssociate_OptionalNestedAssociate_Name], [r6].[RequiredAssociate_OptionalNestedAssociate_String], [r6].[RequiredAssociate_RequiredNestedAssociate_Id], [r6].[RequiredAssociate_RequiredNestedAssociate_Int], [r6].[RequiredAssociate_RequiredNestedAssociate_Ints], [r6].[RequiredAssociate_RequiredNestedAssociate_Name], [r6].[RequiredAssociate_RequiredNestedAssociate_String], [r6].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r6]
    WHERE [r6].[row] <= 1
) AS [r7] ON [r5].[Key] = [r7].[Key] OR ([r5].[Key] IS NULL AND [r7].[Key] IS NULL)
""");
    }

    #endregion Subquery

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
