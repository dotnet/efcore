// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class ComplexNavigationsCollectionsSplitQuerySqlServerTest
    : ComplexNavigationsCollectionsSplitQueryRelationalTestBase<ComplexNavigationsQuerySqlServerFixture>
{
    public ComplexNavigationsCollectionsSplitQuerySqlServerTest(
        ComplexNavigationsQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Filtered_include_basic_Where(bool async)
    {
        await base.Filtered_include_basic_Where(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 5
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_OrderBy(bool async)
    {
        await base.Filtered_include_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Name]
""");
    }

    public override async Task Filtered_ThenInclude_OrderBy(bool async)
    {
        await base.Filtered_ThenInclude_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Name]
""");
    }

    public override async Task Filtered_include_ThenInclude_OrderBy(bool async)
    {
        await base.Filtered_include_ThenInclude_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Name], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Name], [l0].[Id], [l1].[Name] DESC
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Take(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Take(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Name]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE 1 < [l1].[row]
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Name]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip_Take(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE 1 < [l1].[row] AND [l1].[row] <= 4
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Name]
""");
    }

    public override async Task Filtered_include_Skip_without_OrderBy(bool async)
    {
        await base.Filtered_include_Skip_without_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE 1 < [l1].[row]
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_Take_without_OrderBy(bool async)
    {
        await base.Filtered_include_Take_without_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_on_ThenInclude(bool async)
    {
        await base.Filtered_include_on_ThenInclude(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l2]
    WHERE 1 < [l2].[row] AND [l2].[row] <= 4
) AS [l3] ON [l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l3].[OneToMany_Optional_Inverse3Id], [l3].[Name]
""");
    }

    public override async Task Filtered_include_after_reference_navigation(bool async)
    {
        await base.Filtered_include_after_reference_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l2]
    WHERE 1 < [l2].[row] AND [l2].[row] <= 4
) AS [l3] ON [l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l3].[OneToMany_Optional_Inverse3Id], [l3].[Name]
""");
    }

    public override async Task Filtered_include_after_different_filtered_include_same_level(bool async)
    {
        await base.Filtered_include_after_different_filtered_include_same_level(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l2]
    WHERE [l2].[row] <= 3
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l3].[OneToMany_Optional_Inverse2Id], [l3].[Name]
""",
            //
            """
SELECT [l5].[Id], [l5].[Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Optional_Self_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToMany_Required_Self_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id], [l5].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l4].[Id], [l4].[Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Optional_Self_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToMany_Required_Self_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id], [l4].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse2Id] ORDER BY [l1].[Name] DESC) AS [row]
        FROM [LevelTwo] AS [l1]
        WHERE [l1].[Name] <> N'Bar' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE 1 < [l4].[row]
) AS [l5] ON [l].[Id] = [l5].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[Name] DESC
""");
    }

    public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
    {
        await base.Filtered_include_after_different_filtered_include_different_level(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l2]
    WHERE [l2].[row] <= 3
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l3].[OneToMany_Optional_Inverse2Id], [l3].[Name], [l3].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l3].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l2]
    WHERE [l2].[row] <= 3
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse3Id] ORDER BY [l1].[Name] DESC) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Bar' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE 1 < [l4].[row]
) AS [l5] ON [l3].[Id] = [l5].[OneToMany_Required_Inverse3Id]
ORDER BY [l].[Id], [l3].[OneToMany_Optional_Inverse2Id], [l3].[Name], [l3].[Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[Name] DESC
""");
    }

    public override async Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
    {
        await base.Filtered_include_same_filter_set_on_same_navigation_twice(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id] DESC) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l1]
    WHERE [l1].[row] <= 2
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Id] DESC
""");
    }

    public override async Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
    {
        await base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [l].[Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(2) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Required_Id]
) AS [s]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l].[Id], [s].[Id], [s].[Id0]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l2].[Id], [l1].[Id] AS [Id0]
    FROM (
        SELECT TOP(2) [l0].[Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Required_Id]
) AS [s]
INNER JOIN [LevelThree] AS [l3] ON [s].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task
        Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
    {
        await base
            .Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [l].[Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(2) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Required_Id]
) AS [s]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l].[Id], [s].[Id], [s].[Id0]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l2].[Id], [l1].[Id] AS [Id0]
    FROM (
        SELECT TOP(2) [l0].[Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Required_Id]
) AS [s]
INNER JOIN [LevelThree] AS [l3] ON [s].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Id]
""");
    }

    public override async Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Id]
""");
    }

    public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(1) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [s]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id], [l].[Id], [s].[Id], [s].[Id0]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l2].[Id], [l1].[Id] AS [Id0]
    FROM (
        SELECT TOP(1) [l0].[Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [s]
INNER JOIN (
    SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id]
    FROM [LevelFour] AS [l3]
    WHERE [l3].[Id] > 1
) AS [l4] ON [s].[Id0] = [l4].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
    {
        await base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE [l4].[row] <= 1
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l5].[OneToMany_Optional_Inverse3Id], [l5].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l5].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE [l4].[row] <= 1
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [l5].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l5].[OneToMany_Optional_Inverse3Id], [l5].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l5].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE [l4].[row] <= 1
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l3] ON [l5].[Id] = [l3].[OneToMany_Required_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l5].[OneToMany_Optional_Inverse3Id], [l5].[Id]
""");
    }

    public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
    {
        await base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE [l4].[row] <= 1
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l5].[OneToMany_Optional_Inverse3Id], [l5].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l5].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE [l4].[row] <= 1
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [l5].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l5].[OneToMany_Optional_Inverse3Id], [l5].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l5].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l4]
    WHERE [l4].[row] <= 1
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l3] ON [l5].[Id] = [l3].[OneToMany_Required_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l5].[OneToMany_Optional_Inverse3Id], [l5].[Id]
""");
    }

    public override async Task Filtered_include_variable_used_inside_filter(bool async)
    {
        await base.Filtered_include_variable_used_inside_filter(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
@__prm_0='Foo' (Size = 4000)

SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> @__prm_0 OR [l0].[Name] IS NULL
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Id]
""");
    }

    public override async Task Filtered_include_context_accessed_inside_filter(bool async)
    {
        await base.Filtered_include_context_accessed_inside_filter(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [LevelOne] AS [l]
""",
            //
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
@__p_0='True'

SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE @__p_0 = CAST(1 AS bit)
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Id]
""");
    }

    public override async Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
    {
        await base.Filtered_include_context_accessed_inside_filter_correlated(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE (
            SELECT COUNT(*)
            FROM [LevelOne] AS [l1]
            WHERE [l1].[Id] <> [l0].[Id]) > 1
    ) AS [l2]
    WHERE [l2].[row] <= 3
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l3].[OneToMany_Optional_Inverse2Id], [l3].[Id]
""");
    }

    public override async Task Filtered_include_outer_parameter_used_inside_filter(bool async)
    {
        await base.Filtered_include_outer_parameter_used_inside_filter(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l4].[Id], [l4].[Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Optional_Self_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToMany_Required_Self_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id], [l4].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY [LevelTwo] AS [l4]
ORDER BY [l].[Id], [l4].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l4].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY [LevelTwo] AS [l4]
INNER JOIN [LevelThree] AS [l5] ON [l4].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l4].[Id]
""",
            //
            """
SELECT [l8].[Id], [l8].[Date], [l8].[Level1_Optional_Id], [l8].[Level1_Required_Id], [l8].[Name], [l8].[OneToMany_Optional_Inverse2Id], [l8].[OneToMany_Optional_Self_Inverse2Id], [l8].[OneToMany_Required_Inverse2Id], [l8].[OneToMany_Required_Self_Inverse2Id], [l8].[OneToOne_Optional_PK_Inverse2Id], [l8].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY [LevelTwo] AS [l8]
ORDER BY [l].[Id], [l8].[Id]
""",
            //
            """
SELECT [l9].[Id], [l9].[Level2_Optional_Id], [l9].[Level2_Required_Id], [l9].[Name], [l9].[OneToMany_Optional_Inverse3Id], [l9].[OneToMany_Optional_Self_Inverse3Id], [l9].[OneToMany_Required_Inverse3Id], [l9].[OneToMany_Required_Self_Inverse3Id], [l9].[OneToOne_Optional_PK_Inverse3Id], [l9].[OneToOne_Optional_Self3Id], [l].[Id], [l8].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY [LevelTwo] AS [l8]
INNER JOIN [LevelThree] AS [l9] ON [l8].[Id] = [l9].[OneToMany_Optional_Inverse3Id] AND [l].[Id] <> [l9].[Id]
ORDER BY [l].[Id], [l8].[Id]
""");
    }

    public override async Task Select_nav_prop_collection_one_to_many_required(bool async)
    {
        await base.Select_nav_prop_collection_one_to_many_required(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task
        Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
    {
        await base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(
            async);

        AssertSql(
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l12].[Id], [l13].[Id], [l14].[Id], [l14].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
INNER JOIN (
    SELECT [l3].[Id], [l4].[Id] AS [Id0], [l5].[Id] AS [Id1], [l6].[Id] AS [Id2]
    FROM [LevelFour] AS [l3]
    INNER JOIN [LevelThree] AS [l4] ON [l3].[Level3_Required_Id] = [l4].[Id]
    LEFT JOIN [LevelTwo] AS [l5] ON [l4].[Level2_Optional_Id] = [l5].[Id]
    LEFT JOIN [LevelTwo] AS [l6] ON [l5].[Id] = [l6].[OneToMany_Required_Self_Inverse2Id]
) AS [s] ON [l2].[Id] = [s].[Id2]
LEFT JOIN (
    SELECT [l7].[Id], [l8].[Id] AS [Id0], [l9].[Id] AS [Id1], [l10].[Id] AS [Id2], [l10].[Level2_Optional_Id] AS [Level2_Optional_Id0]
    FROM [LevelFour] AS [l7]
    INNER JOIN [LevelThree] AS [l8] ON [l7].[Level3_Required_Id] = [l8].[Id]
    INNER JOIN [LevelTwo] AS [l9] ON [l8].[Level2_Required_Id] = [l9].[Id]
    LEFT JOIN [LevelThree] AS [l10] ON [l9].[Id] = [l10].[OneToMany_Required_Inverse3Id]
) AS [s0] ON [s].[Id2] = [s0].[Id2]
LEFT JOIN [LevelThree] AS [l11] ON [l2].[OneToMany_Optional_Inverse4Id] = [l11].[Id]
LEFT JOIN [LevelThree] AS [l12] ON [s].[Id2] = [l12].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l13] ON [s0].[Level2_Optional_Id0] = [l13].[Id]
LEFT JOIN [LevelThree] AS [l14] ON [l13].[Id] = [l14].[Level2_Required_Id]
WHERE [l11].[Name] <> N'Foo' OR [l11].[Name] IS NULL
ORDER BY [l12].[Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l13].[Id], [l14].[Id]
""",
            //
            """
SELECT [l18].[Id], [l18].[Date], [l18].[Level1_Optional_Id], [l18].[Level1_Required_Id], [l18].[Name], [l18].[OneToMany_Optional_Inverse2Id], [l18].[OneToMany_Optional_Self_Inverse2Id], [l18].[OneToMany_Required_Inverse2Id], [l18].[OneToMany_Required_Self_Inverse2Id], [l18].[OneToOne_Optional_PK_Inverse2Id], [l18].[OneToOne_Optional_Self2Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l12].[Id], [l13].[Id], [l14].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
INNER JOIN (
    SELECT [l3].[Id], [l4].[Id] AS [Id0], [l5].[Id] AS [Id1], [l6].[Id] AS [Id2]
    FROM [LevelFour] AS [l3]
    INNER JOIN [LevelThree] AS [l4] ON [l3].[Level3_Required_Id] = [l4].[Id]
    LEFT JOIN [LevelTwo] AS [l5] ON [l4].[Level2_Optional_Id] = [l5].[Id]
    LEFT JOIN [LevelTwo] AS [l6] ON [l5].[Id] = [l6].[OneToMany_Required_Self_Inverse2Id]
) AS [s] ON [l2].[Id] = [s].[Id2]
LEFT JOIN (
    SELECT [l7].[Id], [l8].[Id] AS [Id0], [l9].[Id] AS [Id1], [l10].[Id] AS [Id2], [l10].[Level2_Optional_Id] AS [Level2_Optional_Id0]
    FROM [LevelFour] AS [l7]
    INNER JOIN [LevelThree] AS [l8] ON [l7].[Level3_Required_Id] = [l8].[Id]
    INNER JOIN [LevelTwo] AS [l9] ON [l8].[Level2_Required_Id] = [l9].[Id]
    LEFT JOIN [LevelThree] AS [l10] ON [l9].[Id] = [l10].[OneToMany_Required_Inverse3Id]
) AS [s0] ON [s].[Id2] = [s0].[Id2]
LEFT JOIN [LevelThree] AS [l11] ON [l2].[OneToMany_Optional_Inverse4Id] = [l11].[Id]
LEFT JOIN [LevelThree] AS [l12] ON [s].[Id2] = [l12].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l13] ON [s0].[Level2_Optional_Id0] = [l13].[Id]
LEFT JOIN [LevelThree] AS [l14] ON [l13].[Id] = [l14].[Level2_Required_Id]
INNER JOIN (
    SELECT [l17].[Id], [l17].[Date], [l17].[Level1_Optional_Id], [l17].[Level1_Required_Id], [l17].[Name], [l17].[OneToMany_Optional_Inverse2Id], [l17].[OneToMany_Optional_Self_Inverse2Id], [l17].[OneToMany_Required_Inverse2Id], [l17].[OneToMany_Required_Self_Inverse2Id], [l17].[OneToOne_Optional_PK_Inverse2Id], [l17].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l17]
    WHERE [l17].[Id] <> 42
) AS [l18] ON [s].[Id2] = [l18].[OneToMany_Optional_Self_Inverse2Id]
WHERE [l11].[Name] <> N'Foo' OR [l11].[Name] IS NULL
ORDER BY [l12].[Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l13].[Id], [l14].[Id]
""");
    }

    public override async Task Project_collection_navigation(bool async)
    {
        await base.Project_collection_navigation(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Project_collection_navigation_nested(bool async)
    {
        await base.Project_collection_navigation_nested(async);

        AssertSql(
            """
SELECT [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Project_collection_navigation_nested_with_take(bool async)
    {
        await base.Project_collection_navigation_nested_with_take(async);

        AssertSql(
            """
SELECT [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l3].[OneToMany_Optional_Inverse3Id] ORDER BY [l3].[Id]) AS [row]
        FROM [LevelThree] AS [l3]
    ) AS [l4]
    WHERE [l4].[row] <= 50
) AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Project_collection_navigation_using_ef_property(bool async)
    {
        await base.Project_collection_navigation_using_ef_property(async);

        AssertSql(
            """
SELECT [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Project_collection_navigation_nested_anonymous(bool async)
    {
        await base.Project_collection_navigation_nested_anonymous(async);

        AssertSql(
            """
SELECT [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Project_collection_navigation_composed(bool async)
    {
        await base.Project_collection_navigation_composed(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 3
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l2]
    WHERE [l2].[Name] <> N'Foo' OR [l2].[Name] IS NULL
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] < 3
ORDER BY [l].[Id]
""");
    }

    public override async Task Project_collection_and_root_entity(bool async)
    {
        await base.Project_collection_and_root_entity(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Project_collection_and_include(bool async)
    {
        await base.Project_collection_and_include(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Project_navigation_and_collection(bool async)
    {
        await base.Project_navigation_and_collection(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_inside_subquery(bool async)
    {
        await base.Include_inside_subquery(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
WHERE [l].[Id] < 3
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Level1_Optional_Id], [l6].[Level1_Required_Id], [l6].[Name], [l6].[OneToMany_Optional_Inverse2Id], [l6].[OneToMany_Optional_Self_Inverse2Id], [l6].[OneToMany_Required_Inverse2Id], [l6].[OneToMany_Required_Self_Inverse2Id], [l6].[OneToOne_Optional_PK_Inverse2Id], [l6].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l4].[Id], [l4].[Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Optional_Self_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToMany_Required_Self_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id], [l4].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l4]
    WHERE [l4].[Id] > 0
) AS [l6]
WHERE [l].[Id] < 3
ORDER BY [l].[Id], [l6].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l6].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l4].[Id]
    FROM [LevelTwo] AS [l4]
    WHERE [l4].[Id] > 0
) AS [l6]
INNER JOIN [LevelThree] AS [l5] ON [l6].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
WHERE [l].[Id] < 3
ORDER BY [l].[Id], [l6].[Id]
""");
    }

    public override async Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
    {
        await base.Null_check_in_anonymous_type_projection_should_not_be_removed(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[c], [s].[Name], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT CASE
        WHEN [l5].[Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [l5].[Name], [l4].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l4]
    LEFT JOIN [LevelThree] AS [l5] ON [l4].[Id] = [l5].[Level2_Required_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
    {
        await base.Null_check_in_Dto_projection_should_not_be_removed(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[c], [s].[Name], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT CASE
        WHEN [l5].[Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [l5].[Name], [l4].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l4]
    LEFT JOIN [LevelThree] AS [l5] ON [l4].[Id] = [l5].[Level2_Required_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
    {
        await base.SelectMany_navigation_property_followed_by_select_collection_navigation(async);

        AssertSql(
            """
SELECT [l0].[Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
    {
        await base.Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(async);

        AssertSql(
            """
SELECT [l1].[Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(bool async)
    {
        await base.SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Required_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
    {
        await base.Lift_projection_mapping_when_pushing_down_subquery(async);

        AssertSql(
            """
@__p_0='25'

SELECT [l1].[Id], [l3].[Id], [l3].[c]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[c], [l2].[OneToMany_Required_Inverse2Id]
    FROM (
        SELECT [l0].[Id], 1 AS [c], [l0].[OneToMany_Required_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Required_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l2]
    WHERE [l2].[row] <= 1
) AS [l3] ON [l1].[Id] = [l3].[OneToMany_Required_Inverse2Id]
ORDER BY [l1].[Id]
""",
            //
            """
@__p_0='25'

SELECT [l4].[Id], [l5].[Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
) AS [l5]
INNER JOIN [LevelTwo] AS [l4] ON [l5].[Id] = [l4].[OneToMany_Required_Inverse2Id]
ORDER BY [l5].[Id]
""");
    }

    public override async Task Select_subquery_single_nested_subquery(bool async)
    {
        await base.Select_subquery_single_nested_subquery(async);

        AssertSql(
            """
SELECT [l].[Id], [l2].[Id], [l2].[c]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l1].[c], [l1].[Id], [l1].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[Id]
""",
            //
            """
SELECT [l11].[Id], [l].[Id], [l13].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l12].[Id], [l12].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT [l8].[Id], [l8].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l8].[OneToMany_Optional_Inverse2Id] ORDER BY [l8].[Id]) AS [row]
        FROM [LevelTwo] AS [l8]
    ) AS [l12]
    WHERE [l12].[row] <= 1
) AS [l13] ON [l].[Id] = [l13].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l11] ON [l13].[Id] = [l11].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l13].[Id], [l11].[Id]
""");
    }

    public override async Task Select_subquery_single_nested_subquery2(bool async)
    {
        await base.Select_subquery_single_nested_subquery2(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l].[Id], [l26].[Id], [l40].[Id], [l40].[c]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l26] ON [l].[Id] = [l26].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l39].[c], [l39].[Id], [l39].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT 1 AS [c], [l35].[Id], [l35].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l35].[OneToMany_Optional_Inverse3Id] ORDER BY [l35].[Id]) AS [row]
        FROM [LevelThree] AS [l35]
    ) AS [l39]
    WHERE [l39].[row] <= 1
) AS [l40] ON [l26].[Id] = [l40].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l26].[Id], [l40].[Id]
""",
            //
            """
SELECT [l38].[Id], [l].[Id], [l26].[Id], [l40].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l26] ON [l].[Id] = [l26].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l39].[Id], [l39].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l35].[Id], [l35].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l35].[OneToMany_Optional_Inverse3Id] ORDER BY [l35].[Id]) AS [row]
        FROM [LevelThree] AS [l35]
    ) AS [l39]
    WHERE [l39].[row] <= 1
) AS [l40] ON [l26].[Id] = [l40].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l38] ON [l40].[Id] = [l38].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l26].[Id], [l40].[Id], [l38].[Id]
""");
    }

    public override async Task Queryable_in_subquery_works_when_final_projection_is_List(bool async)
    {
        await base.Queryable_in_subquery_works_when_final_projection_is_List(async);

        AssertSql();
    }

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
    {
        await base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l1].[c]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [l0].[Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
) AS [l1]
ORDER BY [l].[Id], [l1].[Id]
""",
            //
            """
SELECT [l29].[Name], [l].[Id], [l28].[Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT TOP(1) [l21].[Id]
    FROM [LevelTwo] AS [l21]
    WHERE [l21].[Name] <> N'Foo' OR [l21].[Name] IS NULL
) AS [l28]
CROSS APPLY (
    SELECT [l26].[Name]
    FROM [LevelOne] AS [l26]
    WHERE EXISTS (
        SELECT 1
        FROM [LevelTwo] AS [l27]
        WHERE [l26].[Id] = [l27].[OneToMany_Optional_Inverse2Id] AND [l27].[Id] = [l28].[Id])
) AS [l29]
ORDER BY [l].[Id], [l28].[Id]
""");
    }

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
    {
        await base.Complex_query_with_let_collection_projection_FirstOrDefault(async);

        AssertSql(
            """
SELECT [l].[Id], [l2].[Id], [l2].[c]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l1].[c], [l1].[Id], [l1].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE [l0].[Name] <> N'Foo' OR [l0].[Name] IS NULL
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[Id]
""",
            //
            """
SELECT [l23].[Name], [l].[Id], [l22].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l21].[Id], [l21].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT [l14].[Id], [l14].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l14].[OneToMany_Optional_Inverse2Id] ORDER BY [l14].[Id]) AS [row]
        FROM [LevelTwo] AS [l14]
        WHERE [l14].[Name] <> N'Foo' OR [l14].[Name] IS NULL
    ) AS [l21]
    WHERE [l21].[row] <= 1
) AS [l22] ON [l].[Id] = [l22].[OneToMany_Optional_Inverse2Id]
CROSS APPLY (
    SELECT [l19].[Name]
    FROM [LevelOne] AS [l19]
    WHERE EXISTS (
        SELECT 1
        FROM [LevelTwo] AS [l20]
        WHERE [l19].[Id] = [l20].[OneToMany_Optional_Inverse2Id] AND [l20].[Id] = [l22].[Id])
) AS [l23]
ORDER BY [l].[Id], [l22].[Id]
""");
    }

    public override async Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
    {
        await base.SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(async);

        AssertSql(
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l12].[Id], [l13].[Id], [l14].[Id], [l14].[Name]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
INNER JOIN (
    SELECT [l3].[Id], [l4].[Id] AS [Id0], [l5].[Id] AS [Id1], [l6].[Id] AS [Id2]
    FROM [LevelFour] AS [l3]
    INNER JOIN [LevelThree] AS [l4] ON [l3].[Level3_Required_Id] = [l4].[Id]
    LEFT JOIN [LevelTwo] AS [l5] ON [l4].[Level2_Optional_Id] = [l5].[Id]
    LEFT JOIN [LevelTwo] AS [l6] ON [l5].[Id] = [l6].[OneToMany_Required_Self_Inverse2Id]
) AS [s] ON [l2].[Id] = [s].[Id2]
LEFT JOIN (
    SELECT [l7].[Id], [l8].[Id] AS [Id0], [l9].[Id] AS [Id1], [l10].[Id] AS [Id2], [l10].[Level2_Optional_Id] AS [Level2_Optional_Id0]
    FROM [LevelFour] AS [l7]
    INNER JOIN [LevelThree] AS [l8] ON [l7].[Level3_Required_Id] = [l8].[Id]
    INNER JOIN [LevelTwo] AS [l9] ON [l8].[Level2_Required_Id] = [l9].[Id]
    LEFT JOIN [LevelThree] AS [l10] ON [l9].[Id] = [l10].[OneToMany_Required_Inverse3Id]
) AS [s0] ON [s].[Id2] = [s0].[Id2]
LEFT JOIN [LevelThree] AS [l11] ON [l2].[OneToMany_Optional_Inverse4Id] = [l11].[Id]
LEFT JOIN [LevelThree] AS [l12] ON [s].[Id2] = [l12].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l13] ON [s0].[Level2_Optional_Id0] = [l13].[Id]
LEFT JOIN [LevelThree] AS [l14] ON [l13].[Id] = [l14].[Level2_Required_Id]
WHERE [l11].[Name] <> N'Foo' OR [l11].[Name] IS NULL
ORDER BY [l12].[Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l13].[Id], [l14].[Id]
""",
            //
            """
SELECT [l18].[Id], [l18].[Date], [l18].[Level1_Optional_Id], [l18].[Level1_Required_Id], [l18].[Name], [l18].[OneToMany_Optional_Inverse2Id], [l18].[OneToMany_Optional_Self_Inverse2Id], [l18].[OneToMany_Required_Inverse2Id], [l18].[OneToMany_Required_Self_Inverse2Id], [l18].[OneToOne_Optional_PK_Inverse2Id], [l18].[OneToOne_Optional_Self2Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l12].[Id], [l13].[Id], [l14].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Required_Inverse4Id]
INNER JOIN (
    SELECT [l3].[Id], [l4].[Id] AS [Id0], [l5].[Id] AS [Id1], [l6].[Id] AS [Id2]
    FROM [LevelFour] AS [l3]
    INNER JOIN [LevelThree] AS [l4] ON [l3].[Level3_Required_Id] = [l4].[Id]
    LEFT JOIN [LevelTwo] AS [l5] ON [l4].[Level2_Optional_Id] = [l5].[Id]
    LEFT JOIN [LevelTwo] AS [l6] ON [l5].[Id] = [l6].[OneToMany_Required_Self_Inverse2Id]
) AS [s] ON [l2].[Id] = [s].[Id2]
LEFT JOIN (
    SELECT [l7].[Id], [l8].[Id] AS [Id0], [l9].[Id] AS [Id1], [l10].[Id] AS [Id2], [l10].[Level2_Optional_Id] AS [Level2_Optional_Id0]
    FROM [LevelFour] AS [l7]
    INNER JOIN [LevelThree] AS [l8] ON [l7].[Level3_Required_Id] = [l8].[Id]
    INNER JOIN [LevelTwo] AS [l9] ON [l8].[Level2_Required_Id] = [l9].[Id]
    LEFT JOIN [LevelThree] AS [l10] ON [l9].[Id] = [l10].[OneToMany_Required_Inverse3Id]
) AS [s0] ON [s].[Id2] = [s0].[Id2]
LEFT JOIN [LevelThree] AS [l11] ON [l2].[OneToMany_Optional_Inverse4Id] = [l11].[Id]
LEFT JOIN [LevelThree] AS [l12] ON [s].[Id2] = [l12].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l13] ON [s0].[Level2_Optional_Id0] = [l13].[Id]
LEFT JOIN [LevelThree] AS [l14] ON [l13].[Id] = [l14].[Level2_Required_Id]
INNER JOIN (
    SELECT [l17].[Id], [l17].[Date], [l17].[Level1_Optional_Id], [l17].[Level1_Required_Id], [l17].[Name], [l17].[OneToMany_Optional_Inverse2Id], [l17].[OneToMany_Optional_Self_Inverse2Id], [l17].[OneToMany_Required_Inverse2Id], [l17].[OneToMany_Required_Self_Inverse2Id], [l17].[OneToOne_Optional_PK_Inverse2Id], [l17].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l17]
    WHERE [l17].[Id] <> 42
) AS [l18] ON [s].[Id2] = [l18].[OneToMany_Optional_Self_Inverse2Id]
WHERE [l11].[Name] <> N'Foo' OR [l11].[Name] IS NULL
ORDER BY [l12].[Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [s0].[Id], [s0].[Id0], [s0].[Id1], [s0].[Id2], [l11].[Id], [l13].[Id], [l14].[Id]
""");
    }

    public override async Task Take_Select_collection_Take(bool async)
    {
        await base.Take_Select_collection_Take(async);

        AssertSql(
            """
@__p_0='1'

SELECT TOP(@__p_0) [l].[Id], [l].[Name]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
@__p_0='1'

SELECT [s].[Id], [s].[Name], [s].[Level1Id], [s].[Level2Id], [s].[Id0], [s].[Date], [s].[Name0], [s].[OneToMany_Optional_Self_Inverse1Id], [s].[OneToMany_Required_Self_Inverse1Id], [s].[OneToOne_Optional_Self1Id], [l9].[Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
) AS [l9]
CROSS APPLY (
    SELECT [l8].[Id], [l8].[Name], [l8].[OneToMany_Required_Inverse2Id] AS [Level1Id], [l8].[Level1_Required_Id] AS [Level2Id], [l7].[Id] AS [Id0], [l7].[Date], [l7].[Name] AS [Name0], [l7].[OneToMany_Optional_Self_Inverse1Id], [l7].[OneToMany_Required_Self_Inverse1Id], [l7].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT TOP(3) [l6].[Id], [l6].[Level1_Required_Id], [l6].[Name], [l6].[OneToMany_Required_Inverse2Id]
        FROM [LevelTwo] AS [l6]
        WHERE [l9].[Id] = [l6].[OneToMany_Required_Inverse2Id]
        ORDER BY [l6].[Id]
    ) AS [l8]
    INNER JOIN [LevelOne] AS [l7] ON [l8].[Level1_Required_Id] = [l7].[Id]
) AS [s]
ORDER BY [l9].[Id], [s].[Id]
""");
    }

    public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
    {
        await base.Skip_Take_Select_collection_Skip_Take(async);

        AssertSql(
            """
@__p_0='1'

SELECT [l].[Id], [l].[Name]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
""",
            //
            """
@__p_0='1'

SELECT [s].[Id], [s].[Name], [s].[Level1Id], [s].[Level2Id], [s].[Id0], [s].[Date], [s].[Name0], [s].[OneToMany_Optional_Self_Inverse1Id], [s].[OneToMany_Required_Self_Inverse1Id], [s].[OneToOne_Optional_Self1Id], [l9].[Id]
FROM (
    SELECT [l].[Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [l9]
CROSS APPLY (
    SELECT [l8].[Id], [l8].[Name], [l8].[OneToMany_Required_Inverse2Id] AS [Level1Id], [l8].[Level1_Required_Id] AS [Level2Id], [l7].[Id] AS [Id0], [l7].[Date], [l7].[Name] AS [Name0], [l7].[OneToMany_Optional_Self_Inverse1Id], [l7].[OneToMany_Required_Self_Inverse1Id], [l7].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l6].[Id], [l6].[Level1_Required_Id], [l6].[Name], [l6].[OneToMany_Required_Inverse2Id]
        FROM [LevelTwo] AS [l6]
        WHERE [l9].[Id] = [l6].[OneToMany_Required_Inverse2Id]
        ORDER BY [l6].[Id]
        OFFSET 1 ROWS FETCH NEXT 3 ROWS ONLY
    ) AS [l8]
    INNER JOIN [LevelOne] AS [l7] ON [l8].[Level1_Required_Id] = [l7].[Id]
) AS [s]
ORDER BY [l9].[Id], [s].[Id]
""");
    }

    public override async Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool async)
    {
        await base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task
        Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(bool async)
    {
        await base
            .Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id0], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id0], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name] AS [Name0], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
    FROM [LevelThree] AS [l1]
    INNER JOIN [LevelTwo] AS [l2] ON [l1].[OneToMany_Required_Inverse3Id] = [l2].[Id]
) AS [s] ON [l0].[Id] = [s].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [s].[Id], [s].[Id0]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id], [s].[Id], [s].[Id0]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], [l2].[Id] AS [Id0]
    FROM [LevelThree] AS [l1]
    INNER JOIN [LevelTwo] AS [l2] ON [l1].[OneToMany_Required_Inverse3Id] = [l2].[Id]
) AS [s] ON [l0].[Id] = [s].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelThree] AS [l3] ON [s].[Id0] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Multiple_complex_includes(bool async)
    {
        await base.Multiple_complex_includes(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Multiple_complex_includes_self_ref(bool async)
    {
        await base.Multiple_complex_includes_self_ref(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Self_Inverse1Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Name], [s].[OneToMany_Optional_Self_Inverse1Id], [s].[OneToMany_Required_Self_Inverse1Id], [s].[OneToOne_Optional_Self1Id], [s].[Id0], [s].[Date0], [s].[Name0], [s].[OneToMany_Optional_Self_Inverse1Id0], [s].[OneToMany_Required_Self_Inverse1Id0], [s].[OneToOne_Optional_Self1Id0], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id], [l3].[Id] AS [Id0], [l3].[Date] AS [Date0], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Self_Inverse1Id] AS [OneToMany_Optional_Self_Inverse1Id0], [l3].[OneToMany_Required_Self_Inverse1Id] AS [OneToMany_Required_Self_Inverse1Id0], [l3].[OneToOne_Optional_Self1Id] AS [OneToOne_Optional_Self1Id0]
    FROM [LevelOne] AS [l2]
    LEFT JOIN [LevelOne] AS [l3] ON [l2].[OneToOne_Optional_Self1Id] = [l3].[Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Self_Inverse1Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_reference_and_collection_order_by(bool async)
    {
        await base.Include_reference_and_collection_order_by(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_reference_ThenInclude_collection_order_by(bool async)
    {
        await base.Include_reference_ThenInclude_collection_order_by(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_collection_then_reference(bool async)
    {
        await base.Include_collection_then_reference(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_with_conditional_order_by(bool async)
    {
        await base.Include_collection_with_conditional_order_by(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY CASE
    WHEN [l].[Name] LIKE N'%03' THEN 1
    ELSE 2
END, [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY CASE
    WHEN [l].[Name] LIKE N'%03' THEN 1
    ELSE 2
END, [l].[Id]
""");
    }

    public override async Task Multiple_complex_include_select(bool async)
    {
        await base.Multiple_complex_include_select(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_nested_with_optional_navigation(bool async)
    {
        await base.Include_nested_with_optional_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l0].[Name] <> N'L2 09' OR [l0].[Name] IS NULL
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id0], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name0], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelThree] AS [l1]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
) AS [s] ON [l0].[Id] = [s].[OneToMany_Required_Inverse3Id]
WHERE [l0].[Name] <> N'L2 09' OR [l0].[Name] IS NULL
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='10'

SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Required_Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id]
""",
            //
            """
@__p_0='0'
@__p_1='10'

SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l1].[Id], [l0].[Id]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id]
""",
            //
            """
@__p_0='0'
@__p_1='10'

SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l1].[Id], [l0].[Id]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[OneToMany_Required_Inverse3Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id]
""");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='10'

SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Required_Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id], [l2].[Id]
""",
            //
            """
@__p_0='0'
@__p_1='10'

SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l1].[Id], [l0].[Id], [l2].[Id]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id], [l2].[Id]
""",
            //
            """
@__p_0='0'
@__p_1='10'

SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id], [l1].[Id], [l0].[Id], [l2].[Id]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l1].[Id] = [l2].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l4] ON [l2].[Id] = [l4].[OneToMany_Required_Inverse3Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id], [l2].[Id]
""");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='10'

SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Required_Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id], [l2].[Id]
""",
            //
            """
@__p_0='0'
@__p_1='10'

SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id], [l1].[Id], [l0].[Id], [l2].[Id]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN [LevelTwo] AS [l0] ON [l1].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Required_Id]
INNER JOIN [LevelFour] AS [l3] ON [l2].[Id] = [l3].[OneToMany_Optional_Inverse4Id]
ORDER BY [l1].[Name], [l1].[Id], [l0].[Id], [l2].[Id]
""");
    }

    public override async Task Multiple_include_with_multiple_optional_navigations(bool async)
    {
        await base.Multiple_include_with_multiple_optional_navigations(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l2].[Id], [l3].[Id], [l4].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l3] ON [l].[Id] = [l3].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l4] ON [l3].[Id] = [l4].[Level2_Optional_Id]
WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [l3].[Id], [l4].[Id]
""",
            //
            """
SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [l3].[Id], [l4].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l3] ON [l].[Id] = [l3].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l4] ON [l3].[Id] = [l4].[Level2_Optional_Id]
INNER JOIN [LevelThree] AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [l3].[Id], [l4].[Id]
""");
    }

    public override async Task SelectMany_with_Include1(bool async)
    {
        await base.SelectMany_with_Include1(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Orderby_SelectMany_with_Include1(bool async)
    {
        await base.Orderby_SelectMany_with_Include1(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task SelectMany_with_Include2(bool async)
    {
        await base.SelectMany_with_Include2(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
""");
    }

    public override async Task SelectMany_with_Include_ThenInclude(bool async)
    {
        await base.SelectMany_with_Include_ThenInclude(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
INNER JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_SelectMany_with_Include(bool async)
    {
        await base.Multiple_SelectMany_with_Include(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
INNER JOIN [LevelFour] AS [l3] ON [l1].[Id] = [l3].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
""");
    }

    public override async Task Required_navigation_with_Include(bool async)
    {
        await base.Required_navigation_with_Include(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id]
FROM [LevelThree] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Level2_Required_Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[OneToMany_Required_Inverse2Id] = [l1].[Id]
""");
    }

    public override async Task Required_navigation_with_Include_ThenInclude(bool async)
    {
        await base.Required_navigation_with_Include_ThenInclude(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id]
FROM [LevelFour] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Level3_Required_Id] = [l0].[Id]
INNER JOIN [LevelTwo] AS [l1] ON [l0].[OneToMany_Required_Inverse3Id] = [l1].[Id]
LEFT JOIN [LevelOne] AS [l2] ON [l1].[OneToMany_Optional_Inverse2Id] = [l2].[Id]
""");
    }

    public override async Task Optional_navigation_with_Include_ThenInclude(bool async)
    {
        await base.Optional_navigation_with_Include_ThenInclude(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id0], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name0], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelThree] AS [l1]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
) AS [s] ON [l0].[Id] = [s].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Multiple_optional_navigation_with_Include(bool async)
    {
        await base.Multiple_optional_navigation_with_Include(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_optional_navigation_with_string_based_Include(bool async)
    {
        await base.Multiple_optional_navigation_with_string_based_Include(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task Optional_navigation_with_order_by_and_Include(bool async)
    {
        await base.Optional_navigation_with_order_by_and_Include(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task Optional_navigation_with_Include_and_order(bool async)
    {
        await base.Optional_navigation_with_Include_and_order(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task SelectMany_with_order_by_and_Include(bool async)
    {
        await base.SelectMany_with_order_by_and_Include(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task SelectMany_with_Include_and_order_by(bool async)
    {
        await base.SelectMany_with_Include_and_order_by(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task SelectMany_with_navigation_and_Distinct(bool async)
    {
        await base.SelectMany_with_navigation_and_Distinct(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
    {
        await base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id] AS [FK]
    FROM [LevelTwo] AS [l0]
) AS [l1] ON [l].[Id] = [l1].[FK]
ORDER BY [l].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id] AS [FK]
    FROM [LevelTwo] AS [l0]
) AS [l1] ON [l].[Id] = [l1].[FK]
INNER JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Include_collection_with_multiple_orderbys_member(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_member(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
ORDER BY [l].[Name], [l].[Level1_Required_Id], [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Level1_Required_Id], [l].[Id]
""");
    }

    public override async Task Include_collection_with_multiple_orderbys_property(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
ORDER BY [l].[Level1_Required_Id], [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Level1_Required_Id], [l].[Name], [l].[Id]
""");
    }

    public override async Task Include_collection_with_multiple_orderbys_methodcall(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_methodcall(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
ORDER BY ABS([l].[Level1_Required_Id]), [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY ABS([l].[Level1_Required_Id]), [l].[Name], [l].[Id]
""");
    }

    public override async Task Include_collection_with_multiple_orderbys_complex(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_complex(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
ORDER BY ABS([l].[Level1_Required_Id]) + 7, [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY ABS([l].[Level1_Required_Id]) + 7, [l].[Name], [l].[Id]
""");
    }

    public override async Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_complex_repeated(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
ORDER BY -[l].[Level1_Required_Id], [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY -[l].[Level1_Required_Id], [l].[Name], [l].[Id]
""");
    }

    public override async Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_complex_repeated_checked(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id]
FROM [LevelTwo] AS [l]
ORDER BY -[l].[Level1_Required_Id], [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelTwo] AS [l]
INNER JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY -[l].[Level1_Required_Id], [l].[Name], [l].[Id]
""");
    }

    public override async Task Include_collection_with_groupby_in_subquery(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery(async);

        AssertSql(
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], [l1].[Name]
FROM (
    SELECT [l].[Name]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Name]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelOne] AS [l0]
    ) AS [l2]
    WHERE [l2].[row] <= 1
) AS [l3] ON [l1].[Name] = [l3].[Name]
ORDER BY [l1].[Name], [l3].[Id]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Level1_Optional_Id], [l6].[Level1_Required_Id], [l6].[Name], [l6].[OneToMany_Optional_Inverse2Id], [l6].[OneToMany_Optional_Self_Inverse2Id], [l6].[OneToMany_Required_Inverse2Id], [l6].[OneToMany_Required_Self_Inverse2Id], [l6].[OneToOne_Optional_PK_Inverse2Id], [l6].[OneToOne_Optional_Self2Id], [l7].[Name], [l9].[Id]
FROM (
    SELECT [l].[Name]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Name]
) AS [l7]
LEFT JOIN (
    SELECT [l8].[Id], [l8].[Name]
    FROM (
        SELECT [l5].[Id], [l5].[Name], ROW_NUMBER() OVER(PARTITION BY [l5].[Name] ORDER BY [l5].[Id]) AS [row]
        FROM [LevelOne] AS [l5]
    ) AS [l8]
    WHERE [l8].[row] <= 1
) AS [l9] ON [l7].[Name] = [l9].[Name]
INNER JOIN [LevelTwo] AS [l6] ON [l9].[Id] = [l6].[OneToMany_Optional_Inverse2Id]
ORDER BY [l7].[Name], [l9].[Id]
""");
    }

    public override async Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery_and_filter_before_groupby(async);

        AssertSql(
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], [l1].[Name]
FROM (
    SELECT [l].[Name]
    FROM [LevelOne] AS [l]
    WHERE [l].[Id] > 3
    GROUP BY [l].[Name]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelOne] AS [l0]
        WHERE [l0].[Id] > 3
    ) AS [l2]
    WHERE [l2].[row] <= 1
) AS [l3] ON [l1].[Name] = [l3].[Name]
ORDER BY [l1].[Name], [l3].[Id]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Level1_Optional_Id], [l6].[Level1_Required_Id], [l6].[Name], [l6].[OneToMany_Optional_Inverse2Id], [l6].[OneToMany_Optional_Self_Inverse2Id], [l6].[OneToMany_Required_Inverse2Id], [l6].[OneToMany_Required_Self_Inverse2Id], [l6].[OneToOne_Optional_PK_Inverse2Id], [l6].[OneToOne_Optional_Self2Id], [l7].[Name], [l9].[Id]
FROM (
    SELECT [l].[Name]
    FROM [LevelOne] AS [l]
    WHERE [l].[Id] > 3
    GROUP BY [l].[Name]
) AS [l7]
LEFT JOIN (
    SELECT [l8].[Id], [l8].[Name]
    FROM (
        SELECT [l5].[Id], [l5].[Name], ROW_NUMBER() OVER(PARTITION BY [l5].[Name] ORDER BY [l5].[Id]) AS [row]
        FROM [LevelOne] AS [l5]
        WHERE [l5].[Id] > 3
    ) AS [l8]
    WHERE [l8].[row] <= 1
) AS [l9] ON [l7].[Name] = [l9].[Name]
INNER JOIN [LevelTwo] AS [l6] ON [l9].[Id] = [l6].[OneToMany_Optional_Inverse2Id]
ORDER BY [l7].[Name], [l9].[Id]
""");
    }

    public override async Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery_and_filter_after_groupby(async);

        AssertSql(
            """
SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], [l1].[Name]
FROM (
    SELECT [l].[Name]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Name]
    HAVING [l].[Name] <> N'Foo' OR [l].[Name] IS NULL
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelOne] AS [l0]
    ) AS [l2]
    WHERE [l2].[row] <= 1
) AS [l3] ON [l1].[Name] = [l3].[Name]
ORDER BY [l1].[Name], [l3].[Id]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Level1_Optional_Id], [l6].[Level1_Required_Id], [l6].[Name], [l6].[OneToMany_Optional_Inverse2Id], [l6].[OneToMany_Optional_Self_Inverse2Id], [l6].[OneToMany_Required_Inverse2Id], [l6].[OneToMany_Required_Self_Inverse2Id], [l6].[OneToOne_Optional_PK_Inverse2Id], [l6].[OneToOne_Optional_Self2Id], [l7].[Name], [l9].[Id]
FROM (
    SELECT [l].[Name]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Name]
    HAVING [l].[Name] <> N'Foo' OR [l].[Name] IS NULL
) AS [l7]
LEFT JOIN (
    SELECT [l8].[Id], [l8].[Name]
    FROM (
        SELECT [l5].[Id], [l5].[Name], ROW_NUMBER() OVER(PARTITION BY [l5].[Name] ORDER BY [l5].[Id]) AS [row]
        FROM [LevelOne] AS [l5]
    ) AS [l8]
    WHERE [l8].[row] <= 1
) AS [l9] ON [l7].[Name] = [l9].[Name]
INNER JOIN [LevelTwo] AS [l6] ON [l9].[Id] = [l6].[OneToMany_Optional_Inverse2Id]
ORDER BY [l7].[Name], [l9].[Id]
""");
    }

    public override async Task Include_reference_collection_order_by_reference_navigation(bool async)
    {
        await base.Include_reference_collection_order_by_reference_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l0].[Id], [l].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Id], [l].[Id]
""");
    }

    public override async Task Include_after_SelectMany(bool async)
    {
        await base.Include_after_SelectMany(async);

        AssertSql(
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
    {
        await base.Include_after_multiple_SelectMany_and_reference_navigation(async);

        AssertSql(
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
INNER JOIN [LevelFour] AS [l3] ON [l2].[Id] = [l3].[OneToMany_Required_Self_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
""");
    }

    public override async Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
    {
        await base.Include_after_SelectMany_and_multiple_reference_navigations(async);

        AssertSql(
            """
SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
INNER JOIN [LevelFour] AS [l3] ON [l2].[Id] = [l3].[OneToMany_Optional_Self_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]
""");
    }

    public override async Task Include_collection(bool async)
    {
        await base.Include_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_followed_by_include_reference(bool async)
    {
        await base.Include_collection_followed_by_include_reference(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_reference_followed_by_include_collection(bool async)
    {
        await base.Include_reference_followed_by_include_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_collection_followed_by_projecting_the_included_collection(bool async)
    {
        await base.Include_collection_followed_by_projecting_the_included_collection(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(bool async)
    {
        await base.Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(bool async)
    {
        await base.Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name1], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_ThenInclude_two_references(bool async)
    {
        await base.Include_collection_ThenInclude_two_references(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name1], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(bool async)
    {
        await base.Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name1], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [s].[Id2], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Name2], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Optional_Self_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToMany_Required_Self_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0], [s].[OneToOne_Optional_Self3Id0], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l3].[Id] AS [Id2], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Name] AS [Name2], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l3].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
    LEFT JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""",
            //
            """
SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id], [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id], [l1].[Id] AS [Id0], [l2].[Id] AS [Id1], [l3].[Id] AS [Id2]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
    LEFT JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelFour] AS [l4] ON [s].[Id2] = [l4].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Include_collection_multiple(bool async)
    {
        await base.Include_collection_multiple(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name1], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [s].[Id2], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Name2], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Optional_Self_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToMany_Required_Self_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0], [s].[OneToOne_Optional_Self3Id0], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l3].[Id] AS [Id2], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Name] AS [Name2], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l3].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
    LEFT JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""",
            //
            """
SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id], [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id], [l1].[Id] AS [Id0], [l2].[Id] AS [Id1], [l3].[Id] AS [Id2]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
    LEFT JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelFour] AS [l4] ON [s].[Id2] = [l4].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(bool async)
    {
        await base.Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Optional_Self_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToMany_Required_Self_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l7].[OneToOne_Optional_Self3Id], [l8].[Id] AS [Id0], [l8].[Level3_Optional_Id], [l8].[Level3_Required_Id], [l8].[Name] AS [Name0], [l8].[OneToMany_Optional_Inverse4Id], [l8].[OneToMany_Optional_Self_Inverse4Id], [l8].[OneToMany_Required_Inverse4Id], [l8].[OneToMany_Required_Self_Inverse4Id], [l8].[OneToOne_Optional_PK_Inverse4Id], [l8].[OneToOne_Optional_Self4Id], [l6].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l6]
    LEFT JOIN [LevelThree] AS [l7] ON [l6].[Id] = [l7].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l8] ON [l7].[Id] = [l8].[Level3_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(bool async)
    {
        await base.Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Date], [s0].[Level1_Optional_Id], [s0].[Level1_Required_Id], [s0].[Name], [s0].[OneToMany_Optional_Inverse2Id], [s0].[OneToMany_Optional_Self_Inverse2Id], [s0].[OneToMany_Required_Inverse2Id], [s0].[OneToMany_Required_Self_Inverse2Id], [s0].[OneToOne_Optional_PK_Inverse2Id], [s0].[OneToOne_Optional_Self2Id], [s0].[Id0], [s0].[Level2_Optional_Id], [s0].[Level2_Required_Id], [s0].[Name0], [s0].[OneToMany_Optional_Inverse3Id], [s0].[OneToMany_Optional_Self_Inverse3Id], [s0].[OneToMany_Required_Inverse3Id], [s0].[OneToMany_Required_Self_Inverse3Id], [s0].[OneToOne_Optional_PK_Inverse3Id], [s0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[OneToOne_Optional_PK_Inverse3Id]
) AS [s0] ON [l].[Id] = [s0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_multiple_with_filter(bool async)
    {
        await base.Include_collection_multiple_with_filter(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
WHERE (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL)) > 0
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name1], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name] AS [Name1], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[Level3_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL)) > 0
ORDER BY [l].[Id]
""");
    }

    public override async Task Including_reference_navigation_and_projecting_collection_navigation(bool async)
    {
        await base.Including_reference_navigation_and_projecting_collection_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
INNER JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(bool async)
    {
        await base.LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(async);

        AssertSql(
            """
@__validIds_0='["L1 01","L1 02"]' (Size = 4000)

SELECT CASE
    WHEN [l0].[Id] IS NULL THEN 0
    ELSE [l0].[Id]
END, [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
WHERE [l].[Name] IN (
    SELECT [v].[value]
    FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v]
)
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
@__validIds_0='["L1 01","L1 02"]' (Size = 4000)

SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Required_Inverse3Id]
WHERE [l].[Name] IN (
    SELECT [v].[value]
    FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v]
)
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Filtered_include_different_filter_set_on_same_navigation_twice(bool async)
    {
        await base.Filtered_include_different_filter_set_on_same_navigation_twice(async);

        AssertSql();
    }

    public override async Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(bool async)
    {
        await base.Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(async);

        AssertSql();
    }

    public override async Task Filtered_include_include_parameter_used_inside_filter_throws(bool async)
    {
        await base.Filtered_include_include_parameter_used_inside_filter_throws(async);

        AssertSql();
    }

    public override async Task Filtered_include_is_considered_loaded(bool async)
    {
        await base.Filtered_include_is_considered_loaded(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Id]
""");
    }

    public override async Task Filtered_include_with_Distinct_throws(bool async)
    {
        await base.Filtered_include_with_Distinct_throws(async);

        AssertSql();
    }

    public override async Task Filtered_include_calling_methods_directly_on_parameter_throws(bool async)
    {
        await base.Filtered_include_calling_methods_directly_on_parameter_throws(async);

        AssertSql();
    }

    public override async Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
    {
        await base.Filtered_include_Take_with_another_Take_on_top_level(async);

        AssertSql(
            """
@__p_0='5'

SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
@__p_0='5'

SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l3].[Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
) AS [l3]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(4) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l3].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l0].[Name] DESC
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Optional_Id]
) AS [s]
ORDER BY [l3].[Id], [s].[Name] DESC
""");
    }

    public override async Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
    {
        await base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async);

        AssertSql(
            """
@__p_0='1'
@__p_1='5'

SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id] DESC
OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
""",
            //
            """
@__p_0='1'
@__p_1='5'

SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l3].[Id]
FROM (
    SELECT [l].[Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id] DESC
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l3]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l3].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l0].[Name] DESC
        OFFSET 1 ROWS FETCH NEXT 4 ROWS ONLY
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Optional_Id]
) AS [s]
ORDER BY [l3].[Id] DESC, [s].[Name] DESC
""");
    }

    public override async Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(
        bool async)
    {
        await base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(async);

        AssertSql(
            """
SELECT TOP(1) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l3].[Id]
FROM (
    SELECT TOP(1) [l].[Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
) AS [l3]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(40) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l3].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Optional_Id]
) AS [s]
ORDER BY [l3].[Id]
""");
    }

    public override async Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(
        bool async)
    {
        await base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(async);

        AssertSql(
            """
@__p_0='30'

SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
@__p_0='30'

SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l3].[Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
) AS [l3]
CROSS APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(40) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE [l3].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
    ) AS [l2]
    LEFT JOIN [LevelThree] AS [l1] ON [l2].[Id] = [l1].[Level2_Optional_Id]
) AS [s]
ORDER BY [l3].[Id]
""");
    }

    public override async Task Projecting_collection_with_FirstOrDefault(bool async)
    {
        await base.Projecting_collection_with_FirstOrDefault(async);

        AssertSql(
            """
SELECT TOP(1) [l].[Id]
FROM [LevelOne] AS [l]
WHERE [l].[Id] = 1
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id]
FROM (
    SELECT TOP(1) [l].[Id]
    FROM [LevelOne] AS [l]
    WHERE [l].[Id] = 1
) AS [l3]
INNER JOIN [LevelTwo] AS [l2] ON [l3].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l3].[Id]
""");
    }

    public override async Task Skip_on_grouping_element(bool async)
    {
        await base.Skip_on_grouping_element(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Name], [l6].[OneToMany_Optional_Self_Inverse1Id], [l6].[OneToMany_Required_Self_Inverse1Id], [l6].[OneToOne_Optional_Self1Id], [l4].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l4]
INNER JOIN (
    SELECT [l5].[Id], [l5].[Date], [l5].[Name], [l5].[OneToMany_Optional_Self_Inverse1Id], [l5].[OneToMany_Required_Self_Inverse1Id], [l5].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l3].[Date] ORDER BY [l3].[Name]) AS [row]
        FROM [LevelOne] AS [l3]
    ) AS [l5]
    WHERE 1 < [l5].[row]
) AS [l6] ON [l4].[Date] = [l6].[Date]
ORDER BY [l4].[Date], [l6].[Date], [l6].[Name]
""");
    }

    public override async Task Take_on_grouping_element(bool async)
    {
        await base.Take_on_grouping_element(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Name], [l6].[OneToMany_Optional_Self_Inverse1Id], [l6].[OneToMany_Required_Self_Inverse1Id], [l6].[OneToOne_Optional_Self1Id], [l4].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l4]
INNER JOIN (
    SELECT [l5].[Id], [l5].[Date], [l5].[Name], [l5].[OneToMany_Optional_Self_Inverse1Id], [l5].[OneToMany_Required_Self_Inverse1Id], [l5].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l3].[Date] ORDER BY [l3].[Name] DESC) AS [row]
        FROM [LevelOne] AS [l3]
    ) AS [l5]
    WHERE [l5].[row] <= 10
) AS [l6] ON [l4].[Date] = [l6].[Date]
ORDER BY [l4].[Date], [l6].[Date], [l6].[Name] DESC
""");
    }

    public override async Task Skip_Take_on_grouping_element(bool async)
    {
        await base.Skip_Take_on_grouping_element(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Name], [l6].[OneToMany_Optional_Self_Inverse1Id], [l6].[OneToMany_Required_Self_Inverse1Id], [l6].[OneToOne_Optional_Self1Id], [l4].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l4]
INNER JOIN (
    SELECT [l5].[Id], [l5].[Date], [l5].[Name], [l5].[OneToMany_Optional_Self_Inverse1Id], [l5].[OneToMany_Required_Self_Inverse1Id], [l5].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l3].[Date] ORDER BY [l3].[Name]) AS [row]
        FROM [LevelOne] AS [l3]
    ) AS [l5]
    WHERE 1 < [l5].[row] AND [l5].[row] <= 6
) AS [l6] ON [l4].[Date] = [l6].[Date]
ORDER BY [l4].[Date], [l6].[Date], [l6].[Name]
""");
    }

    public override async Task Skip_Take_Distinct_on_grouping_element(bool async)
    {
        await base.Skip_Take_Distinct_on_grouping_element(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l8].[Id], [l8].[Date], [l8].[Name], [l8].[OneToMany_Optional_Self_Inverse1Id], [l8].[OneToMany_Required_Self_Inverse1Id], [l8].[OneToOne_Optional_Self1Id], [l7].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l7]
CROSS APPLY (
    SELECT DISTINCT [l6].[Id], [l6].[Date], [l6].[Name], [l6].[OneToMany_Optional_Self_Inverse1Id], [l6].[OneToMany_Required_Self_Inverse1Id], [l6].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l5].[Id], [l5].[Date], [l5].[Name], [l5].[OneToMany_Optional_Self_Inverse1Id], [l5].[OneToMany_Required_Self_Inverse1Id], [l5].[OneToOne_Optional_Self1Id]
        FROM [LevelOne] AS [l5]
        WHERE [l7].[Date] = [l5].[Date]
        ORDER BY [l5].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [l6]
) AS [l8]
ORDER BY [l7].[Date]
""");
    }

    public override async Task Skip_Take_ToList_on_grouping_element(bool async)
    {
        await base.Skip_Take_ToList_on_grouping_element(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l6].[Id], [l6].[Date], [l6].[Name], [l6].[OneToMany_Optional_Self_Inverse1Id], [l6].[OneToMany_Required_Self_Inverse1Id], [l6].[OneToOne_Optional_Self1Id], [l4].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l4]
INNER JOIN (
    SELECT [l5].[Id], [l5].[Date], [l5].[Name], [l5].[OneToMany_Optional_Self_Inverse1Id], [l5].[OneToMany_Required_Self_Inverse1Id], [l5].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l3].[OneToMany_Optional_Self_Inverse1Id], [l3].[OneToMany_Required_Self_Inverse1Id], [l3].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l3].[Date] ORDER BY [l3].[Name]) AS [row]
        FROM [LevelOne] AS [l3]
    ) AS [l5]
    WHERE 1 < [l5].[row] AND [l5].[row] <= 6
) AS [l6] ON [l4].[Date] = [l6].[Date]
ORDER BY [l4].[Date], [l6].[Date], [l6].[Name]
""");
    }

    public override async Task Skip_Take_on_grouping_element_into_non_entity(bool async)
    {
        await base.Skip_Take_on_grouping_element_into_non_entity(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l6].[Name], [l4].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l4]
INNER JOIN (
    SELECT [l5].[Name], [l5].[Date]
    FROM (
        SELECT [l3].[Name], [l3].[Date], ROW_NUMBER() OVER(PARTITION BY [l3].[Date] ORDER BY [l3].[Name]) AS [row]
        FROM [LevelOne] AS [l3]
    ) AS [l5]
    WHERE 1 < [l5].[row] AND [l5].[row] <= 6
) AS [l6] ON [l4].[Date] = [l6].[Date]
ORDER BY [l4].[Date], [l6].[Date], [l6].[Name]
""");
    }

    public override async Task Skip_Take_on_grouping_element_with_collection_include(bool async)
    {
        await base.Skip_Take_on_grouping_element_with_collection_include(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [l9].[Id], [l9].[Date], [l9].[Name], [l9].[OneToMany_Optional_Self_Inverse1Id], [l9].[OneToMany_Required_Self_Inverse1Id], [l9].[OneToOne_Optional_Self1Id], [l7].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l7]
INNER JOIN (
    SELECT [l8].[Id], [l8].[Date], [l8].[Name], [l8].[OneToMany_Optional_Self_Inverse1Id], [l8].[OneToMany_Required_Self_Inverse1Id], [l8].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l5].[Id], [l5].[Date], [l5].[Name], [l5].[OneToMany_Optional_Self_Inverse1Id], [l5].[OneToMany_Required_Self_Inverse1Id], [l5].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l5].[Date] ORDER BY [l5].[Name]) AS [row]
        FROM [LevelOne] AS [l5]
    ) AS [l8]
    WHERE 1 < [l8].[row] AND [l8].[row] <= 6
) AS [l9] ON [l7].[Date] = [l9].[Date]
ORDER BY [l7].[Date], [l9].[Date], [l9].[Name], [l9].[Id]
""");
    }

    public override async Task Skip_Take_on_grouping_element_with_reference_include(bool async)
    {
        await base.Skip_Take_on_grouping_element_with_reference_include(async);

        AssertSql(
            """
SELECT [l].[Date]
FROM [LevelOne] AS [l]
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Name], [s].[OneToMany_Optional_Self_Inverse1Id], [s].[OneToMany_Required_Self_Inverse1Id], [s].[OneToOne_Optional_Self1Id], [s].[Id0], [s].[Date0], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [l10].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    GROUP BY [l].[Date]
) AS [l10]
CROSS APPLY (
    SELECT [l9].[Id], [l9].[Date], [l9].[Name], [l9].[OneToMany_Optional_Self_Inverse1Id], [l9].[OneToMany_Required_Self_Inverse1Id], [l9].[OneToOne_Optional_Self1Id], [l8].[Id] AS [Id0], [l8].[Date] AS [Date0], [l8].[Level1_Optional_Id], [l8].[Level1_Required_Id], [l8].[Name] AS [Name0], [l8].[OneToMany_Optional_Inverse2Id], [l8].[OneToMany_Optional_Self_Inverse2Id], [l8].[OneToMany_Required_Inverse2Id], [l8].[OneToMany_Required_Self_Inverse2Id], [l8].[OneToOne_Optional_PK_Inverse2Id], [l8].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l7].[Id], [l7].[Date], [l7].[Name], [l7].[OneToMany_Optional_Self_Inverse1Id], [l7].[OneToMany_Required_Self_Inverse1Id], [l7].[OneToOne_Optional_Self1Id]
        FROM [LevelOne] AS [l7]
        WHERE [l10].[Date] = [l7].[Date]
        ORDER BY [l7].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [l9]
    LEFT JOIN [LevelTwo] AS [l8] ON [l9].[Id] = [l8].[Level1_Optional_Id]
) AS [s]
ORDER BY [l10].[Date], [s].[Name]
""");
    }

    public override async Task Skip_Take_on_grouping_element_inside_collection_projection(bool async)
    {
        await base.Skip_Take_on_grouping_element_inside_collection_projection(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l15].[Date], [l].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l10].[Date]
    FROM [LevelOne] AS [l10]
    WHERE [l10].[Name] = [l].[Name] OR ([l10].[Name] IS NULL AND [l].[Name] IS NULL)
    GROUP BY [l10].[Date]
) AS [l15]
ORDER BY [l].[Id], [l15].[Date]
""",
            //
            """
SELECT [l17].[Id], [l17].[Date], [l17].[Name], [l17].[OneToMany_Optional_Self_Inverse1Id], [l17].[OneToMany_Required_Self_Inverse1Id], [l17].[OneToOne_Optional_Self1Id], [l].[Id], [l15].[Date]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [l10].[Date]
    FROM [LevelOne] AS [l10]
    WHERE [l10].[Name] = [l].[Name] OR ([l10].[Name] IS NULL AND [l].[Name] IS NULL)
    GROUP BY [l10].[Date]
) AS [l15]
INNER JOIN (
    SELECT [l16].[Id], [l16].[Date], [l16].[Name], [l16].[OneToMany_Optional_Self_Inverse1Id], [l16].[OneToMany_Required_Self_Inverse1Id], [l16].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l14].[Id], [l14].[Date], [l14].[Name], [l14].[OneToMany_Optional_Self_Inverse1Id], [l14].[OneToMany_Required_Self_Inverse1Id], [l14].[OneToOne_Optional_Self1Id], ROW_NUMBER() OVER(PARTITION BY [l14].[Name], [l14].[Date] ORDER BY [l14].[Name]) AS [row]
        FROM [LevelOne] AS [l14]
    ) AS [l16]
    WHERE 1 < [l16].[row] AND [l16].[row] <= 6
) AS [l17] ON ([l].[Name] = [l17].[Name] OR ([l].[Name] IS NULL AND [l17].[Name] IS NULL)) AND [l15].[Date] = [l17].[Date]
ORDER BY [l].[Id], [l15].[Date], [l17].[Name], [l17].[Date]
""");
    }

    public override async Task Collection_projection_over_GroupBy_over_parameter(bool async)
    {
        await base.Collection_projection_over_GroupBy_over_parameter(async);

        AssertSql(
            """
@__validIds_0='["L1 01","L1 02"]' (Size = 4000)

SELECT [l].[Date]
FROM [LevelOne] AS [l]
WHERE [l].[Name] IN (
    SELECT [v].[value]
    FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v]
)
GROUP BY [l].[Date]
ORDER BY [l].[Date]
""",
            //
            """
@__validIds_0='["L1 01","L1 02"]' (Size = 4000)

SELECT [l5].[Id], [l4].[Date]
FROM (
    SELECT [l].[Date]
    FROM [LevelOne] AS [l]
    WHERE [l].[Name] IN (
        SELECT [v].[value]
        FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v]
    )
    GROUP BY [l].[Date]
) AS [l4]
INNER JOIN (
    SELECT [l3].[Id], [l3].[Date]
    FROM [LevelOne] AS [l3]
    WHERE [l3].[Name] IN (
        SELECT [v3].[value]
        FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v3]
    )
) AS [l5] ON [l4].[Date] = [l5].[Date]
ORDER BY [l4].[Date]
""");
    }

    public override async Task Include_partially_added_before_Where_and_then_build_upon(bool async)
    {
        await base.Include_partially_added_before_Where_and_then_build_upon(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
WHERE [l0].[Id] < 3 OR [l1].[Id] > 8
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l3].[Id] AS [Id0], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id]
    FROM [LevelThree] AS [l2]
    LEFT JOIN [LevelFour] AS [l3] ON [l2].[Id] = [l3].[Level3_Optional_Id]
) AS [s] ON [l1].[Id] = [s].[OneToMany_Optional_Inverse3Id]
WHERE [l0].[Id] < 3 OR [l1].[Id] > 8
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(bool async)
    {
        await base.Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
WHERE [l0].[Id] < 3 OR [l1].[Id] > 8
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Optional_Self_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToMany_Required_Self_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id], [l6].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Id]) AS [row]
        FROM [LevelThree] AS [l2]
    ) AS [l5]
    WHERE [l5].[row] <= 3
) AS [l6] ON [l1].[Id] = [l6].[OneToMany_Optional_Inverse3Id]
WHERE [l0].[Id] < 3 OR [l1].[Id] > 8
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l6].[OneToMany_Optional_Inverse3Id], [l6].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l4].[Id] AS [Id0], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name] AS [Name0], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id]
    FROM [LevelThree] AS [l3]
    LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[Level3_Optional_Id]
) AS [s] ON [l1].[Id] = [s].[OneToMany_Required_Inverse3Id]
WHERE [l0].[Id] < 3 OR [l1].[Id] > 8
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task Take_on_correlated_collection_in_projection(bool async)
    {
        await base.Take_on_correlated_collection_in_projection(async);

        AssertSql(
            """
SELECT [l].[Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l4].[Id], [l4].[Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Optional_Self_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToMany_Required_Self_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id], [l4].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l3].[Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse2Id] ORDER BY [l2].[Id]) AS [row]
        FROM [LevelTwo] AS [l2]
    ) AS [l3]
    WHERE [l3].[row] <= 50
) AS [l4] ON [l].[Id] = [l4].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task FirstOrDefault_with_predicate_on_correlated_collection_in_projection(bool async)
    {
        await base.FirstOrDefault_with_predicate_on_correlated_collection_in_projection(async);

        AssertSql(
            """
SELECT [l].[Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id], [l0].[Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id] AND [l].[Id] = [l2].[Id]
""");
    }

    public override async Task SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(
        bool async)
    {
        await base.SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Level1_Required_Id] = [l].[Id] * 2 OR CAST(LEN([l0].[Name]) AS int) = [l0].[Id]
) AS [l1]
ORDER BY [l].[Id], [l1].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [l0].[Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Level1_Required_Id] = [l].[Id] * 2 OR CAST(LEN([l0].[Name]) AS int) = [l0].[Id]
) AS [l1]
INNER JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Complex_query_issue_21665(bool async)
    {
        await base.Complex_query_issue_21665(async);

        AssertSql(
            """
SELECT TOP(1) [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Name], (
    SELECT COUNT(*)
    FROM [LevelOne] AS [l1]
    WHERE [l].[Id] = [l1].[OneToMany_Optional_Self_Inverse1Id]), (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l2]
    WHERE [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]), CASE
    WHEN EXISTS (
        SELECT 1
        FROM [LevelTwo] AS [l3]
        WHERE [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id] AND [l3].[Id] = 2) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
WHERE [l].[Id] = 2
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s0].[Id], [s0].[Date], [s0].[Name], [s0].[OneToMany_Optional_Self_Inverse1Id], [s0].[OneToMany_Required_Self_Inverse1Id], [s0].[OneToOne_Optional_Self1Id], [s0].[ChildCount], [s0].[Level2Name], [s0].[Level2Count], [s0].[IsLevel2There], [s].[Id], [s].[Id0]
FROM (
    SELECT TOP(1) [l].[Id], [l0].[Id] AS [Id0], [l].[Name]
    FROM [LevelOne] AS [l]
    LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
    WHERE [l].[Id] = 2
    ORDER BY [l].[Name]
) AS [s]
CROSS APPLY (
    SELECT [l21].[Id], [l21].[Date], [l21].[Name], [l21].[OneToMany_Optional_Self_Inverse1Id], [l21].[OneToMany_Required_Self_Inverse1Id], [l21].[OneToOne_Optional_Self1Id], (
        SELECT COUNT(*)
        FROM [LevelOne] AS [l22]
        WHERE [l21].[Id] = [l22].[OneToMany_Optional_Self_Inverse1Id]) AS [ChildCount], [l20].[Name] AS [Level2Name], (
        SELECT COUNT(*)
        FROM [LevelTwo] AS [l23]
        WHERE [l21].[Id] = [l23].[OneToMany_Optional_Inverse2Id]) AS [Level2Count], CASE
        WHEN EXISTS (
            SELECT 1
            FROM [LevelTwo] AS [l24]
            WHERE [l21].[Id] = [l24].[OneToMany_Optional_Inverse2Id] AND [l24].[Id] = 2) THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsLevel2There]
    FROM (
        SELECT [l19].[Id], [l19].[Date], [l19].[Name], [l19].[OneToMany_Optional_Self_Inverse1Id], [l19].[OneToMany_Required_Self_Inverse1Id], [l19].[OneToOne_Optional_Self1Id]
        FROM [LevelOne] AS [l19]
        WHERE [s].[Id] = [l19].[OneToMany_Optional_Self_Inverse1Id]
        ORDER BY [l19].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [l21]
    LEFT JOIN [LevelTwo] AS [l20] ON [l21].[Id] = [l20].[Level1_Optional_Id]
) AS [s0]
ORDER BY [s].[Name], [s].[Id], [s].[Id0], [s0].[Name]
""");
    }

    public override async Task SelectMany_over_conditional_null_source(bool async)
    {
        await base.SelectMany_over_conditional_null_source(async);

        AssertSql();
    }

    public override async Task SelectMany_over_conditional_empty_source(bool async)
    {
        await base.SelectMany_over_conditional_empty_source(async);

        AssertSql();
    }

    public override async Task Include_after_Select(bool async)
    {
        await base.Include_after_Select(async);

        AssertSql();
    }

    public override async Task Include_after_SelectMany_and_reference_navigation(bool async)
    {
        await base.Include_after_SelectMany_and_reference_navigation(async);

        AssertSql();
    }

    public override async Task Projecting_collection_after_optional_reference_correlated_with_parent(bool async)
    {
        await base.Projecting_collection_after_optional_reference_correlated_with_parent(async);

        AssertSql(
            """
SELECT [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l4].[ChildId], [l4].[ParentName], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
CROSS APPLY (
    SELECT [l3].[Id] AS [ChildId], [l0].[Name] AS [ParentName]
    FROM [LevelThree] AS [l3]
    WHERE [l0].[Id] IS NOT NULL AND [l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
) AS [l4]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(bool async)
    {
        await base.Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""",
            //
            """
SELECT [l8].[Key], [l8].[Count], [l].[Id], [l0].[Id], [l1].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
CROSS APPLY (
    SELECT [l6].[Name] AS [Key], COUNT(*) AS [Count]
    FROM [LevelThree] AS [l6]
    WHERE [l0].[Id] IS NOT NULL AND [l0].[Id] = [l6].[OneToMany_Optional_Inverse3Id]
    GROUP BY [l6].[Name]
) AS [l8]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_complex_includes_EF_Property(bool async)
    {
        await base.Multiple_complex_includes_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Multiple_complex_includes_self_ref_EF_Property(bool async)
    {
        await base.Multiple_complex_includes_self_ref_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
INNER JOIN [LevelOne] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Self_Inverse1Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Name], [s].[OneToMany_Optional_Self_Inverse1Id], [s].[OneToMany_Required_Self_Inverse1Id], [s].[OneToOne_Optional_Self1Id], [s].[Id0], [s].[Date0], [s].[Name0], [s].[OneToMany_Optional_Self_Inverse1Id0], [s].[OneToMany_Required_Self_Inverse1Id0], [s].[OneToOne_Optional_Self1Id0], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id], [l3].[Id] AS [Id0], [l3].[Date] AS [Date0], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Self_Inverse1Id] AS [OneToMany_Optional_Self_Inverse1Id0], [l3].[OneToMany_Required_Self_Inverse1Id] AS [OneToMany_Required_Self_Inverse1Id0], [l3].[OneToOne_Optional_Self1Id] AS [OneToOne_Optional_Self1Id0]
    FROM [LevelOne] AS [l2]
    LEFT JOIN [LevelOne] AS [l3] ON [l2].[OneToOne_Optional_Self1Id] = [l3].[Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Self_Inverse1Id]
ORDER BY [l].[Id], [l0].[Id]
""");
    }

    public override async Task Include_collection_multiple_with_filter_EF_Property(bool async)
    {
        await base.Include_collection_multiple_with_filter_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
WHERE (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL)) > 0
ORDER BY [l].[Id]
""",
            //
            """
SELECT [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Name1], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Optional_Self_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToMany_Required_Self_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[OneToOne_Optional_Self4Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name] AS [Name1], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[Level3_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL)) > 0
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_basic_Where_EF_Property(bool async)
    {
        await base.Filtered_include_basic_Where_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 5
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_OrderBy_EF_Property(bool async)
    {
        await base.Filtered_include_OrderBy_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Name]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip_Take_EF_Property(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip_Take_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [l1]
    WHERE 1 < [l1].[row] AND [l1].[row] <= 4
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Name]
""");
    }

    public override async Task Filtered_include_on_ThenInclude_EF_Property(bool async)
    {
        await base.Filtered_include_on_ThenInclude_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE [l1].[Name] <> N'Foo' OR [l1].[Name] IS NULL
    ) AS [l2]
    WHERE 1 < [l2].[row] AND [l2].[row] <= 4
) AS [l3] ON [l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l3].[OneToMany_Optional_Inverse3Id], [l3].[Name]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l].[Name], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_nested(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_nested(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l].[Name], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l].[Name], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_reference(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_reference(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l].[Name], [s].[Id], [s].[Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Optional_Self_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToMany_Required_Self_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[OneToOne_Optional_Self2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Name0], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Optional_Self_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToMany_Required_Self_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_multiple(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_multiple(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l].[Name], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id]
""",
            //
            """
SELECT [l].[Name], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Name], [l].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_reference_same_level(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_reference_same_level(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""",
            //
            """
SELECT [l].[Name], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_reference(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_reference(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Name]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_reference_multiple(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_reference_multiple(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[Level1_Required_Id]
ORDER BY [l].[Name]
""");
    }

    public override async Task Project_collection_and_nested_conditional(bool async)
    {
        await base.Project_collection_and_nested_conditional(async);

        AssertSql(
            """
SELECT [l].[Id], CASE
    WHEN [l].[Id] = 1 THEN N'01'
    WHEN [l].[Id] = 2 THEN N'02'
    WHEN [l].[Id] = 3 THEN N'03'
    ELSE NULL
END
FROM [LevelOne] AS [l]
WHERE CASE
    WHEN [l].[Id] = 1 THEN N'01'
    WHEN [l].[Id] = 2 THEN N'02'
    WHEN [l].[Id] = 3 THEN N'03'
    ELSE NULL
END = N'02'
ORDER BY [l].[Id]
""",
            //
            """
SELECT [l2].[Name], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
WHERE CASE
    WHEN [l].[Id] = 1 THEN N'01'
    WHEN [l].[Id] = 2 THEN N'02'
    WHEN [l].[Id] = 3 THEN N'03'
    ELSE NULL
END = N'02'
ORDER BY [l].[Id], [l2].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
