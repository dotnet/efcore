// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexTableSplitting;

public class ComplexTableSplittingPrimitiveCollectionSqlServerTest(
    ComplexTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : ComplexTableSplittingPrimitiveCollectionRelationalTestBase<ComplexTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RequiredAssociate_Ints]) AS [r0]) = 3
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate_Ints], '$[0]') AS int) = 1
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE 3 IN (
    SELECT [r0].[value]
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r0]
)
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE 2 IN (
    SELECT [r0].[value]
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r0]
)
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RequiredAssociate_RequiredNestedAssociate_Ints]) AS [r0]) = 3
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT ISNULL(SUM([r1].[value]), 0)
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r1])
FROM [RootEntity] AS [r]
WHERE (
    SELECT ISNULL(SUM([r0].[value]), 0)
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r0]) >= 6
""");
    }

    public override async Task Any_predicate_on_GroupBy_First()
    {
        await base.Any_predicate_on_GroupBy_First();

        AssertSql(
            """
SELECT [r8].[Id], [r8].[Name], [r8].[OptionalAssociate_Id], [r8].[OptionalAssociate_Int], [r8].[OptionalAssociate_Ints], [r8].[OptionalAssociate_Name], [r8].[OptionalAssociate_String], [r8].[OptionalAssociate_OptionalNestedAssociate_Id], [r8].[OptionalAssociate_OptionalNestedAssociate_Int], [r8].[OptionalAssociate_OptionalNestedAssociate_Ints], [r8].[OptionalAssociate_OptionalNestedAssociate_Name], [r8].[OptionalAssociate_OptionalNestedAssociate_String], [r8].[OptionalAssociate_RequiredNestedAssociate_Id], [r8].[OptionalAssociate_RequiredNestedAssociate_Int], [r8].[OptionalAssociate_RequiredNestedAssociate_Ints], [r8].[OptionalAssociate_RequiredNestedAssociate_Name], [r8].[OptionalAssociate_RequiredNestedAssociate_String], [r8].[RequiredAssociate_Id], [r8].[RequiredAssociate_Int], [r8].[RequiredAssociate_Ints], [r8].[RequiredAssociate_Name], [r8].[RequiredAssociate_String], [r8].[RequiredAssociate_OptionalNestedAssociate_Id], [r8].[RequiredAssociate_OptionalNestedAssociate_Int], [r8].[RequiredAssociate_OptionalNestedAssociate_Ints], [r8].[RequiredAssociate_OptionalNestedAssociate_Name], [r8].[RequiredAssociate_OptionalNestedAssociate_String], [r8].[RequiredAssociate_RequiredNestedAssociate_Id], [r8].[RequiredAssociate_RequiredNestedAssociate_Int], [r8].[RequiredAssociate_RequiredNestedAssociate_Ints], [r8].[RequiredAssociate_RequiredNestedAssociate_Name], [r8].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING 2 IN (
        SELECT [r5].[value]
        FROM OPENJSON((
            SELECT TOP(1) [r3].[RequiredAssociate_Ints]
            FROM (
                SELECT [r4].[Id], [r4].[RequiredAssociate_Ints], [r4].[Id] % 2 AS [Key]
                FROM [RootEntity] AS [r4]
            ) AS [r3]
            WHERE ([r0].[Key] = [r3].[Key] OR ([r0].[Key] IS NULL AND [r3].[Key] IS NULL)) AND [r3].[Id] > 0
            ORDER BY [r3].[Id])) WITH ([value] int '$') AS [r5]
    )
) AS [r6]
LEFT JOIN (
    SELECT [r7].[Id], [r7].[Name], [r7].[OptionalAssociate_Id], [r7].[OptionalAssociate_Int], [r7].[OptionalAssociate_Ints], [r7].[OptionalAssociate_Name], [r7].[OptionalAssociate_String], [r7].[OptionalAssociate_OptionalNestedAssociate_Id], [r7].[OptionalAssociate_OptionalNestedAssociate_Int], [r7].[OptionalAssociate_OptionalNestedAssociate_Ints], [r7].[OptionalAssociate_OptionalNestedAssociate_Name], [r7].[OptionalAssociate_OptionalNestedAssociate_String], [r7].[OptionalAssociate_RequiredNestedAssociate_Id], [r7].[OptionalAssociate_RequiredNestedAssociate_Int], [r7].[OptionalAssociate_RequiredNestedAssociate_Ints], [r7].[OptionalAssociate_RequiredNestedAssociate_Name], [r7].[OptionalAssociate_RequiredNestedAssociate_String], [r7].[RequiredAssociate_Id], [r7].[RequiredAssociate_Int], [r7].[RequiredAssociate_Ints], [r7].[RequiredAssociate_Name], [r7].[RequiredAssociate_String], [r7].[RequiredAssociate_OptionalNestedAssociate_Id], [r7].[RequiredAssociate_OptionalNestedAssociate_Int], [r7].[RequiredAssociate_OptionalNestedAssociate_Ints], [r7].[RequiredAssociate_OptionalNestedAssociate_Name], [r7].[RequiredAssociate_OptionalNestedAssociate_String], [r7].[RequiredAssociate_RequiredNestedAssociate_Id], [r7].[RequiredAssociate_RequiredNestedAssociate_Int], [r7].[RequiredAssociate_RequiredNestedAssociate_Ints], [r7].[RequiredAssociate_RequiredNestedAssociate_Name], [r7].[RequiredAssociate_RequiredNestedAssociate_String], [r7].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r7]
    WHERE [r7].[row] <= 1
) AS [r8] ON [r6].[Key] = [r8].[Key] OR ([r6].[Key] IS NULL AND [r8].[Key] IS NULL)
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
