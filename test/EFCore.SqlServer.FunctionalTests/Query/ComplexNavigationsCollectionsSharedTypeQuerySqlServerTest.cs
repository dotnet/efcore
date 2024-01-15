﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsCollectionsSharedTypeQuerySqlServerTest
    : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQuerySqlServerFixture>
{
    public ComplexNavigationsCollectionsSharedTypeQuerySqlServerTest(
        ComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
    {
        await base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Id], [s].[Id0], [l5].[Name], [l5].[Id], [s].[c]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT TOP(1) 1 AS [c], [l0].[Id], [l2].[Id] AS [Id0], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Required_Id], [l2].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Required_Inverse2Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l2] ON [l0].[Id] = CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l2].[Level2_Name] <> N'Foo' OR [l2].[Level2_Name] IS NULL)
) AS [s]
OUTER APPLY (
    SELECT [l3].[Name], [l3].[Id]
    FROM [Level1] AS [l3]
    WHERE EXISTS (
        SELECT 1
        FROM [Level1] AS [l4]
        WHERE [l4].[OneToOne_Required_PK_Date] IS NOT NULL AND [l4].[Level1_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l3].[Id] = [l4].[OneToMany_Optional_Inverse2Id] AND [l4].[Id] = CASE
            WHEN [s].[OneToOne_Required_PK_Date] IS NOT NULL AND [s].[Level1_Required_Id] IS NOT NULL AND [s].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [s].[Id0]
        END)
) AS [l5]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task SelectMany_with_Include1(bool async)
    {
        await base.SelectMany_with_Include1(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task SelectMany_with_navigation_and_Distinct(bool async)
    {
        await base.SelectMany_with_navigation_and_Distinct(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Take_Select_collection_Take(bool async)
    {
        await base.Take_Select_collection_Take(async);

        AssertSql(
            """
@__p_0='1'

SELECT [l3].[Id], [l3].[Name], [s].[Id], [s].[Name], [s].[Level1Id], [s].[Level2Id], [s].[Id0], [s].[Date], [s].[Name0], [s].[Id1]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
) AS [l3]
OUTER APPLY (
    SELECT CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END AS [Id], [l2].[Level2_Name] AS [Name], [l2].[OneToMany_Required_Inverse2Id] AS [Level1Id], [l2].[Level1_Required_Id] AS [Level2Id], [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name] AS [Name0], [l2].[Id] AS [Id1], [l2].[c]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l3].[Id] = [l0].[OneToMany_Required_Inverse2Id]
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [l2]
    INNER JOIN [Level1] AS [l1] ON [l2].[Level1_Required_Id] = [l1].[Id]
) AS [s]
ORDER BY [l3].[Id], [s].[c], [s].[Id1]
""");
    }

    public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
    {
        await base.Skip_Take_Select_collection_Skip_Take(async);

        AssertSql(
            """
@__p_0='1'

SELECT [l3].[Id], [l3].[Name], [s].[Id], [s].[Name], [s].[Level1Id], [s].[Level2Id], [s].[Id0], [s].[Date], [s].[Name0], [s].[Id1]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [l3]
OUTER APPLY (
    SELECT CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END AS [Id], [l2].[Level2_Name] AS [Name], [l2].[OneToMany_Required_Inverse2Id] AS [Level1Id], [l2].[Level1_Required_Id] AS [Level2Id], [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name] AS [Name0], [l2].[Id] AS [Id1], [l2].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l3].[Id] = [l0].[OneToMany_Required_Inverse2Id]
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
        OFFSET 1 ROWS FETCH NEXT 3 ROWS ONLY
    ) AS [l2]
    INNER JOIN [Level1] AS [l1] ON [l2].[Level1_Required_Id] = [l1].[Id]
) AS [s]
ORDER BY [l3].[Id], [s].[c], [s].[Id1]
""");
    }

    public override async Task FirstOrDefault_with_predicate_on_correlated_collection_in_projection(bool async)
    {
        await base.FirstOrDefault_with_predicate_on_correlated_collection_in_projection(async);

        AssertSql(
            """
SELECT [l].[Id], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id] AND [l].[Id] = CASE
    WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
END
""");
    }

    public override async Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
    {
        await base.Filtered_include_same_filter_set_on_same_navigation_twice(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END DESC) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [l1]
    WHERE [l1].[row] <= 2
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[c] DESC
""");
    }

    public override async Task Filtered_include_basic_Where(bool async)
    {
        await base.Filtered_include_basic_Where(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l0].[Id] > 5
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Optional_navigation_with_order_by_and_Include(bool async)
    {
        await base.Optional_navigation_with_order_by_and_Include(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l1].[Level2_Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE 1 < [l1].[row]
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Level2_Name]
""");
    }

    public override async Task Multiple_include_with_multiple_optional_navigations(bool async)
    {
        await base.Multiple_include_with_multiple_optional_navigations(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l5].[Id], [l7].[Id], [l9].[Id], [l11].[Id], [l11].[Level2_Optional_Id], [l11].[Level2_Required_Id], [l11].[Level3_Name], [l11].[OneToMany_Optional_Inverse3Id], [l11].[OneToMany_Required_Inverse3Id], [l11].[OneToOne_Optional_PK_Inverse3Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l7].[OneToOne_Required_PK_Date], [l7].[Level1_Optional_Id], [l7].[Level1_Required_Id], [l7].[Level2_Name], [l7].[OneToMany_Optional_Inverse2Id], [l7].[OneToMany_Required_Inverse2Id], [l7].[OneToOne_Optional_PK_Inverse2Id], [l9].[Level2_Optional_Id], [l9].[Level2_Required_Id], [l9].[Level3_Name], [l9].[OneToMany_Optional_Inverse3Id], [l9].[OneToMany_Required_Inverse3Id], [l9].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level3_Name], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l5].[Level2_Optional_Id]
LEFT JOIN (
    SELECT [l6].[Id], [l6].[OneToOne_Required_PK_Date], [l6].[Level1_Optional_Id], [l6].[Level1_Required_Id], [l6].[Level2_Name], [l6].[OneToMany_Optional_Inverse2Id], [l6].[OneToMany_Required_Inverse2Id], [l6].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l6]
    WHERE [l6].[OneToOne_Required_PK_Date] IS NOT NULL AND [l6].[Level1_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l7] ON [l].[Id] = [l7].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l8].[Id], [l8].[Level2_Optional_Id], [l8].[Level2_Required_Id], [l8].[Level3_Name], [l8].[OneToMany_Optional_Inverse3Id], [l8].[OneToMany_Required_Inverse3Id], [l8].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l8]
    WHERE [l8].[Level2_Required_Id] IS NOT NULL AND [l8].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l9] ON CASE
    WHEN [l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l7].[Id]
END = [l9].[Level2_Optional_Id]
LEFT JOIN (
    SELECT [l10].[Id], [l10].[Level2_Optional_Id], [l10].[Level2_Required_Id], [l10].[Level3_Name], [l10].[OneToMany_Optional_Inverse3Id], [l10].[OneToMany_Required_Inverse3Id], [l10].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l10]
    WHERE [l10].[Level2_Required_Id] IS NOT NULL AND [l10].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l11] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l11].[OneToMany_Optional_Inverse3Id]
WHERE [l3].[Level3_Name] <> N'Foo' OR [l3].[Level3_Name] IS NULL
ORDER BY [l].[Id], [l1].[Id], [l3].[Id], [l5].[Id], [l7].[Id], [l9].[Id]
""");
    }

    public override async Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
    {
        await base.Filtered_include_Take_with_another_Take_on_top_level(async);

        AssertSql(
            """
@__p_0='5'

SELECT [l4].[Id], [l4].[Date], [l4].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
) AS [l4]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT TOP(4) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l4].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l0].[Level2_Name] DESC
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[Level2_Optional_Id]
) AS [s]
ORDER BY [l4].[Id], [s].[Level2_Name] DESC, [s].[Id]
""");
    }

    public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
    {
        await base.Filtered_include_after_different_filtered_include_different_level(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY [l0].[Level2_Name]
    ) AS [l2]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
        FROM (
            SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse3Id] ORDER BY [l1].[Level3_Name] DESC) AS [row]
            FROM [Level1] AS [l1]
            WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l1].[Level3_Name] <> N'Bar' OR [l1].[Level3_Name] IS NULL)
        ) AS [l3]
        WHERE 1 < [l3].[row]
    ) AS [l4] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[OneToMany_Required_Inverse3Id]
) AS [s]
ORDER BY [l].[Id], [s].[Level2_Name], [s].[Id], [s].[OneToMany_Required_Inverse3Id], [s].[Level3_Name] DESC
""");
    }

    public override async Task Project_collection_navigation_composed(bool async)
    {
        await base.Project_collection_navigation_composed(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] < 3
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip_Take(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE 1 < [l1].[row] AND [l1].[row] <= 4
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Level2_Name]
""");
    }

    public override async Task Filtered_include_on_ThenInclude(bool async)
    {
        await base.Filtered_include_on_ThenInclude(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Level3_Name]) AS [row]
        FROM [Level1] AS [l2]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
    ) AS [l3]
    WHERE 1 < [l3].[row] AND [l3].[row] <= 4
) AS [l4] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l4].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id], [l4].[OneToMany_Optional_Inverse3Id], [l4].[Level3_Name]
""");
    }

    public override async Task Include_collection_with_conditional_order_by(bool async)
    {
        await base.Include_collection_with_conditional_order_by(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY CASE
    WHEN [l].[Name] LIKE N'%03' THEN 1
    ELSE 2
END, [l].[Id]
""");
    }

    public override async Task Include_nested_with_optional_navigation(bool async)
    {
        await base.Include_nested_with_optional_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id0], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l2]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Required_Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [s] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [s].[OneToMany_Required_Inverse3Id]
WHERE [l1].[Level2_Name] <> N'L2 09' OR [l1].[Level2_Name] IS NULL
ORDER BY [l].[Id], [l1].[Id], [s].[Id]
""");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='10'

SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id], [l7].[Id], [l7].[Level2_Optional_Id], [l7].[Level2_Required_Id], [l7].[Level3_Name], [l7].[OneToMany_Optional_Inverse3Id], [l7].[OneToMany_Required_Inverse3Id], [l7].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Required_PK_Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Level2_Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id], [l8].[Id], [l8].[Level2_Optional_Id], [l8].[Level2_Required_Id], [l8].[Level3_Name], [l8].[OneToMany_Optional_Inverse3Id], [l8].[OneToMany_Required_Inverse3Id], [l8].[OneToOne_Optional_PK_Inverse3Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l3]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l4] ON [l1].[Id] = [l4].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l5]
    WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l7] ON CASE
    WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
END = [l7].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Level3_Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l6]
    WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l8] ON CASE
    WHEN [l4].[OneToOne_Required_PK_Date] IS NOT NULL AND [l4].[Level1_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l4].[Id]
END = [l8].[OneToMany_Required_Inverse3Id]
ORDER BY [l1].[Name], [l1].[Id], [l2].[Id], [l4].[Id], [l7].[Id]
""");
    }

    public override async Task Select_subquery_single_nested_subquery2(bool async)
    {
        await base.Select_subquery_single_nested_subquery2(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id00], [s].[c]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l4].[Id] AS [Id0], [l5].[Id] AS [Id1], [l5].[Id0] AS [Id00], [l4].[c], CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END AS [c0], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l3].[c], [l3].[Id], [l3].[Level2_Required_Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Optional_Inverse3Id]
        FROM (
            SELECT 1 AS [c], [l1].[Id], [l1].[Level2_Required_Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY CASE
                WHEN [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l1].[Id]
            END) AS [row]
            FROM [Level1] AS [l1]
            WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [l3]
        WHERE [l3].[row] <= 1
    ) AS [l4] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l4].[OneToMany_Optional_Inverse3Id]
    LEFT JOIN (
        SELECT CASE
            WHEN [l2].[Level3_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse4Id] IS NOT NULL THEN [l2].[Id]
        END AS [Id], [l2].[Id] AS [Id0], [l2].[OneToMany_Optional_Inverse4Id]
        FROM [Level1] AS [l2]
        WHERE [l2].[Level3_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
    END = [l5].[OneToMany_Optional_Inverse4Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[c0], [s].[Id], [s].[Id0], [s].[Id1]
""");
    }

    public override async Task SelectMany_with_Include_and_order_by(bool async)
    {
        await base.SelectMany_with_Include_and_order_by(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l1].[Level2_Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_complex_includes(bool async)
    {
        await base.Multiple_complex_includes(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Level3_Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l6] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l6].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id0], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[Level2_Optional_Id]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id], [l6].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_then_reference(bool async)
    {
        await base.Include_collection_then_reference(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[Level2_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id]
""");
    }

    public override async Task Include_reference_followed_by_include_collection(bool async)
    {
        await base.Include_reference_followed_by_include_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_optional_navigation_with_string_based_Include(bool async)
    {
        await base.Multiple_optional_navigation_with_string_based_Include(async);

        AssertSql(
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l].[Id], [l1].[Id], [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level3_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END = [l5].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id]
""");
    }

    public override async Task SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(bool async)
    {
        await base.SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l4] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l4].[OneToMany_Required_Inverse3Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    WHERE [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l5].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id], [l4].[Id]
""");
    }

    public override async Task SelectMany_with_Include_ThenInclude(bool async)
    {
        await base.SelectMany_with_Include_ThenInclude(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l].[Id], [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level3_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END = [l5].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id]
""");
    }

    public override async Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[c]
""");
    }

    public override async Task
        Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(bool async)
    {
        await base
            .Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s0].[Id], [s0].[OneToOne_Required_PK_Date], [s0].[Level1_Optional_Id], [s0].[Level1_Required_Id], [s0].[Level2_Name], [s0].[OneToMany_Optional_Inverse2Id], [s0].[OneToMany_Required_Inverse2Id], [s0].[OneToOne_Optional_PK_Inverse2Id], [s0].[Id0], [s0].[Level2_Optional_Id], [s0].[Level2_Required_Id], [s0].[Level3_Name], [s0].[OneToMany_Optional_Inverse3Id], [s0].[OneToMany_Required_Inverse3Id], [s0].[OneToOne_Optional_PK_Inverse3Id], [s0].[Id00], [s0].[OneToOne_Required_PK_Date0], [s0].[Level1_Optional_Id0], [s0].[Level1_Required_Id0], [s0].[Level2_Name0], [s0].[OneToMany_Optional_Inverse2Id0], [s0].[OneToMany_Required_Inverse2Id0], [s0].[OneToOne_Optional_PK_Inverse2Id0], [s0].[Id1], [s0].[Level2_Optional_Id0], [s0].[Level2_Required_Id0], [s0].[Level3_Name0], [s0].[OneToMany_Optional_Inverse3Id0], [s0].[OneToMany_Required_Inverse3Id0], [s0].[OneToOne_Optional_PK_Inverse3Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [s].[Id] AS [Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0] AS [Id00], [s].[OneToOne_Required_PK_Date] AS [OneToOne_Required_PK_Date0], [s].[Level1_Optional_Id] AS [Level1_Optional_Id0], [s].[Level1_Required_Id] AS [Level1_Required_Id0], [s].[Level2_Name] AS [Level2_Name0], [s].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [s].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [s].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [s].[Id1], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Level3_Name0], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l3].[Id] AS [Id0], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id1], [l5].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l5].[Level2_Required_Id] AS [Level2_Required_Id0], [l5].[Level3_Name] AS [Level3_Name0], [l5].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l5].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l5].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0]
        FROM [Level1] AS [l1]
        INNER JOIN (
            SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
            FROM [Level1] AS [l2]
            WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [l3] ON [l1].[OneToMany_Required_Inverse3Id] = CASE
            WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
        END
        LEFT JOIN (
            SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
            FROM [Level1] AS [l4]
            WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
        ) AS [l5] ON CASE
            WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
        END = [l5].[OneToMany_Optional_Inverse3Id]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [s] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [s].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s0] ON [l].[Id] = [s0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s0].[Id], [s0].[Id0], [s0].[Id00]
""");
    }

    public override async Task Include_after_SelectMany(bool async)
    {
        await base.Include_after_SelectMany(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Required_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
    {
        await base.Filtered_include_context_accessed_inside_filter_correlated(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND (
            SELECT COUNT(*)
            FROM [Level1] AS [l1]
            WHERE [l1].[Id] <> [l0].[Id]) > 1
    ) AS [l2]
    WHERE [l2].[row] <= 3
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l3].[OneToMany_Optional_Inverse2Id], [l3].[c]
""");
    }

    public override async Task Skip_Take_on_grouping_element(bool async)
    {
        await base.Skip_Take_on_grouping_element(async);

        AssertSql(
            """
SELECT [l1].[Date], [l3].[Id], [l3].[Date], [l3].[Name]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l2]
    WHERE 1 < [l2].[row] AND [l2].[row] <= 6
) AS [l3] ON [l1].[Date] = [l3].[Date]
ORDER BY [l1].[Date], [l3].[Date], [l3].[Name]
""");
    }

    public override async Task SelectMany_with_Include2(bool async)
    {
        await base.SelectMany_with_Include2(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[Level2_Required_Id]
""");
    }

    public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l5].[Id] AS [Id1], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l2].[c]
    FROM (
        SELECT TOP(1) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level3_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse4Id] IS NOT NULL AND [l4].[Id] > 1
    ) AS [l5] ON CASE
        WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[OneToMany_Optional_Inverse4Id]
) AS [s]
ORDER BY [l].[Id], [s].[c], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Include_collection_with_groupby_in_subquery(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery(async);

        AssertSql(
            """
SELECT [l4].[Id], [l4].[Date], [l4].[Name], [l2].[Name], [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id]
FROM (
    SELECT [l].[Name]
    FROM [Level1] AS [l]
    GROUP BY [l].[Name]
) AS [l2]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Date], [l3].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l3]
    WHERE [l3].[row] <= 1
) AS [l4] ON [l2].[Name] = [l4].[Name]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l5] ON [l4].[Id] = [l5].[OneToMany_Optional_Inverse2Id]
ORDER BY [l2].[Name], [l4].[Id]
""");
    }

    public override async Task Skip_Take_ToList_on_grouping_element(bool async)
    {
        await base.Skip_Take_ToList_on_grouping_element(async);

        AssertSql(
            """
SELECT [l1].[Date], [l3].[Id], [l3].[Date], [l3].[Name]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l2]
    WHERE 1 < [l2].[row] AND [l2].[row] <= 6
) AS [l3] ON [l1].[Date] = [l3].[Date]
ORDER BY [l1].[Date], [l3].[Date], [l3].[Name]
""");
    }

    public override async Task Project_collection_navigation_nested_with_take(bool async)
    {
        await base.Project_collection_navigation_nested_with_take(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Id]) AS [row]
        FROM [Level1] AS [l2]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3]
    WHERE [l3].[row] <= 50
) AS [l4] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l4].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
    {
        await base.Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(async);

        AssertSql(
            """
SELECT CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END, [l].[Id], [l1].[Id], [l3].[Id], [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Required_Id], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level3_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END = [l5].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id]
""");
    }

    public override async Task Skip_Take_on_grouping_element_with_reference_include(bool async)
    {
        await base.Skip_Take_on_grouping_element_with_reference_include(async);

        AssertSql(
            """
SELECT [l4].[Date], [s].[Id], [s].[Date], [s].[Name], [s].[Id0], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l4]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l3].[Id] AS [Id0], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name]
        FROM [Level1] AS [l0]
        WHERE [l4].[Date] = [l0].[Date]
        ORDER BY [l0].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l3] ON [l2].[Id] = [l3].[Level1_Optional_Id]
) AS [s]
ORDER BY [l4].[Date], [s].[Name], [s].[Id]
""");
    }

    public override async Task SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(
        bool async)
    {
        await base.SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [l4].[Id], [l4].[OneToOne_Required_PK_Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Level2_Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l0].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l2] ON [l0].[Id] = CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l2].[Level1_Required_Id] = [l].[Id] * 2 OR CAST(LEN([l2].[Level2_Name]) AS int) = CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END OR ([l2].[Level2_Name] IS NULL AND CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END IS NULL))
) AS [s]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l3]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l4] ON [l].[Id] = [l4].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id0], [s].[Id]
""");
    }

    public override async Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(
        bool async)
    {
        await base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(async);

        AssertSql(
            """
SELECT [l4].[Id], [l4].[Date], [l4].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM (
    SELECT TOP(1) [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
) AS [l4]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT TOP(40) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l4].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[Level2_Optional_Id]
) AS [s]
ORDER BY [l4].[Id], [s].[Id]
""");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='10'

SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l6].[Id], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Level4_Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l2] ON [l1].[Id] = [l2].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    WHERE [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l4] ON CASE
    WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
END = [l4].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l5]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l6] ON CASE
    WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
END = [l6].[OneToMany_Optional_Inverse4Id]
ORDER BY [l1].[Name], [l1].[Id], [l2].[Id], [l4].[Id]
""");
    }

    public override async Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
    {
        await base.SelectMany_navigation_property_followed_by_select_collection_navigation(async);

        AssertSql(
            """
SELECT CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END, [l].[Id], [l1].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery_and_filter_before_groupby(async);

        AssertSql(
            """
SELECT [l4].[Id], [l4].[Date], [l4].[Name], [l2].[Name], [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id]
FROM (
    SELECT [l].[Name]
    FROM [Level1] AS [l]
    WHERE [l].[Id] > 3
    GROUP BY [l].[Name]
) AS [l2]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Date], [l3].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[Id] > 3
    ) AS [l3]
    WHERE [l3].[row] <= 1
) AS [l4] ON [l2].[Name] = [l4].[Name]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l5] ON [l4].[Id] = [l5].[OneToMany_Optional_Inverse2Id]
ORDER BY [l2].[Name], [l4].[Id]
""");
    }

    public override async Task Filtered_include_after_reference_navigation(bool async)
    {
        await base.Filtered_include_after_reference_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Level3_Name]) AS [row]
        FROM [Level1] AS [l2]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
    ) AS [l3]
    WHERE 1 < [l3].[row] AND [l3].[row] <= 4
) AS [l4] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l4].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id], [l4].[OneToMany_Optional_Inverse3Id], [l4].[Level3_Name]
""");
    }

    public override async Task Project_collection_navigation_nested(bool async)
    {
        await base.Project_collection_navigation_nested(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
    {
        await base.Lift_projection_mapping_when_pushing_down_subquery(async);

        AssertSql(
            """
@__p_0='25'

SELECT [l2].[Id], [l4].[Id0], [l5].[Id], [l5].[Id0], [l4].[Id], [l4].[c]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [Level1] AS [l]
) AS [l2]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[c], [l3].[Id0], [l3].[OneToMany_Required_Inverse2Id]
    FROM (
        SELECT CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [Id], 1 AS [c], [l0].[Id] AS [Id0], [l0].[OneToMany_Required_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Required_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l3]
    WHERE [l3].[row] <= 1
) AS [l4] ON [l2].[Id] = [l4].[OneToMany_Required_Inverse2Id]
LEFT JOIN (
    SELECT CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END AS [Id], [l1].[Id] AS [Id0], [l1].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l5] ON [l2].[Id] = [l5].[OneToMany_Required_Inverse2Id]
ORDER BY [l2].[Id], [l4].[Id0]
""");
    }

    public override async Task Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(bool async)
    {
        await base.Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s0].[Id], [s0].[OneToOne_Required_PK_Date], [s0].[Level1_Optional_Id], [s0].[Level1_Required_Id], [s0].[Level2_Name], [s0].[OneToMany_Optional_Inverse2Id], [s0].[OneToMany_Required_Inverse2Id], [s0].[OneToOne_Optional_PK_Inverse2Id], [s0].[Id0], [s0].[Level2_Optional_Id], [s0].[Level2_Required_Id], [s0].[Level3_Name], [s0].[OneToMany_Optional_Inverse3Id], [s0].[OneToMany_Required_Inverse3Id], [s0].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id0], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s0] ON [l].[Id] = [s0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s0].[Id]
""");
    }

    public override async Task Project_collection_navigation_nested_anonymous(bool async)
    {
        await base.Project_collection_navigation_nested_anonymous(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Filtered_include_after_different_filtered_include_same_level(bool async)
    {
        await base.Filtered_include_after_different_filtered_include_same_level(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [l2]
    WHERE [l2].[row] <= 3
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[OneToOne_Required_PK_Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Level2_Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse2Id] ORDER BY [l1].[Level2_Name] DESC) AS [row]
        FROM [Level1] AS [l1]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1].[Level2_Name] <> N'Bar' OR [l1].[Level2_Name] IS NULL)
    ) AS [l4]
    WHERE 1 < [l4].[row]
) AS [l5] ON [l].[Id] = [l5].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l3].[OneToMany_Optional_Inverse2Id], [l3].[Level2_Name], [l3].[Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[Level2_Name] DESC
""");
    }

    public override async Task Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(bool async)
    {
        await base.Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[Id2], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Level3_Name0], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0], [s].[Id3], [s].[Level3_Optional_Id0], [s].[Level3_Required_Id0], [s].[Level4_Name0], [s].[OneToMany_Optional_Inverse4Id0], [s].[OneToMany_Required_Inverse4Id0], [s].[OneToOne_Optional_PK_Inverse4Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l6].[Id] AS [Id2], [l6].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l6].[Level2_Required_Id] AS [Level2_Required_Id0], [l6].[Level3_Name] AS [Level3_Name0], [l6].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l6].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l6].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l8].[Id] AS [Id3], [l8].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l8].[Level3_Required_Id] AS [Level3_Required_Id0], [l8].[Level4_Name] AS [Level4_Name0], [l8].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l8].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l8].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Optional_Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l5]
        WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l6] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l6].[Level2_Optional_Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l7]
        WHERE [l7].[Level3_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l8] ON CASE
        WHEN [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l6].[Id]
    END = [l8].[OneToMany_Optional_Inverse4Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Filtered_include_context_accessed_inside_filter(bool async)
    {
        await base.Filtered_include_context_accessed_inside_filter(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [Level1] AS [l]
""",
            //
            """
@__p_0='True'

SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND @__p_0 = CAST(1 AS bit)
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[c]
""");
    }

    public override async Task Include_collection_followed_by_include_reference(bool async)
    {
        await base.Include_collection_followed_by_include_reference(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id]
""");
    }

    public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
    {
        await base.Complex_multi_include_with_order_by_and_paging(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='10'

SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Level3_Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l1]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l2] ON [l1].[Id] = [l2].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    WHERE [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
END = [l5].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l6] ON CASE
    WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
END = [l6].[OneToMany_Required_Inverse3Id]
ORDER BY [l1].[Name], [l1].[Id], [l2].[Id], [l5].[Id]
""");
    }

    public override async Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
    {
        await base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async);

        AssertSql(
            """
@__p_0='1'
@__p_1='5'

SELECT [l4].[Id], [l4].[Date], [l4].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id] DESC
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [l4]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l4].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
        ORDER BY [l0].[Level2_Name] DESC
        OFFSET 1 ROWS FETCH NEXT 4 ROWS ONLY
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[Level2_Optional_Id]
) AS [s]
ORDER BY [l4].[Id] DESC, [s].[Level2_Name] DESC, [s].[Id]
""");
    }

    public override async Task Skip_Take_on_grouping_element_with_collection_include(bool async)
    {
        await base.Skip_Take_on_grouping_element_with_collection_include(async);

        AssertSql(
            """
SELECT [l2].[Date], [s].[Id], [s].[Date], [s].[Name], [s].[Id0], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l2]
OUTER APPLY (
    SELECT [l3].[Id], [l3].[Date], [l3].[Name], [l4].[Id] AS [Id0], [l4].[OneToOne_Required_PK_Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Level2_Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name]
        FROM [Level1] AS [l0]
        WHERE [l2].[Date] = [l0].[Date]
        ORDER BY [l0].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [l3]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l4] ON [l3].[Id] = [l4].[OneToMany_Optional_Inverse2Id]
) AS [s]
ORDER BY [l2].[Date], [s].[Name], [s].[Id]
""");
    }

    public override async Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
    {
        await base.Null_check_in_anonymous_type_projection_should_not_be_removed(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[c], [s].[Level3_Name], [s].[Id], [s].[Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [l2].[Level2_Required_Id] IS NULL OR [l2].[OneToMany_Required_Inverse3Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [l2].[Level3_Name], [l0].[Id], [l2].[Id] AS [Id0], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Required_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[Level2_Required_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id]
""");
    }

    public override async Task Filtered_include_Take_without_OrderBy(bool async)
    {
        await base.Filtered_include_Take_without_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_followed_by_projecting_the_included_collection(bool async)
    {
        await base.Include_collection_followed_by_projecting_the_included_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(bool async)
    {
        await base.Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filtered_include_ThenInclude_OrderBy(bool async)
    {
        await base.Filtered_include_ThenInclude_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Level2_Name], [s].[Id], [s].[Level3_Name] DESC
""");
    }

    public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
    {
        await base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s0].[Id], [s0].[OneToOne_Required_PK_Date], [s0].[Level1_Optional_Id], [s0].[Level1_Required_Id], [s0].[Level2_Name], [s0].[OneToMany_Optional_Inverse2Id], [s0].[OneToMany_Required_Inverse2Id], [s0].[OneToOne_Optional_PK_Inverse2Id], [s0].[Id0], [s0].[Level2_Optional_Id], [s0].[Level2_Required_Id], [s0].[Level3_Name], [s0].[OneToMany_Optional_Inverse3Id], [s0].[OneToMany_Required_Inverse3Id], [s0].[OneToOne_Optional_PK_Inverse3Id], [s0].[Id00], [s0].[Level3_Optional_Id], [s0].[Level3_Required_Id], [s0].[Level4_Name], [s0].[OneToMany_Optional_Inverse4Id], [s0].[OneToMany_Required_Inverse4Id], [s0].[OneToOne_Optional_PK_Inverse4Id], [s0].[Id1], [s0].[Level3_Optional_Id0], [s0].[Level3_Required_Id0], [s0].[Level4_Name0], [s0].[OneToMany_Optional_Inverse4Id0], [s0].[OneToMany_Required_Inverse4Id0], [s0].[OneToOne_Optional_PK_Inverse4Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [s].[Id] AS [Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0] AS [Id00], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[Id1], [s].[Level3_Optional_Id0], [s].[Level3_Required_Id0], [s].[Level4_Name0], [s].[OneToMany_Optional_Inverse4Id0], [s].[OneToMany_Required_Inverse4Id0], [s].[OneToOne_Optional_PK_Inverse4Id0], [s].[c]
    FROM [Level1] AS [l0]
    OUTER APPLY (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l5].[Id] AS [Id0], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l6].[Id] AS [Id1], [l6].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l6].[Level3_Required_Id] AS [Level3_Required_Id0], [l6].[Level4_Name] AS [Level4_Name0], [l6].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l6].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l6].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [l4].[c]
        FROM (
            SELECT TOP(1) [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], CASE
                WHEN [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l1].[Id]
            END AS [c]
            FROM [Level1] AS [l1]
            WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL AND CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NOT NULL AND (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END = [l1].[OneToMany_Optional_Inverse3Id] OR (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NULL AND [l1].[OneToMany_Optional_Inverse3Id] IS NULL)) AND ([l1].[Level3_Name] <> N'Foo' OR [l1].[Level3_Name] IS NULL)
            ORDER BY CASE
                WHEN [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l1].[Id]
            END
        ) AS [l4]
        LEFT JOIN (
            SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Level4_Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id]
            FROM [Level1] AS [l2]
            WHERE [l2].[Level3_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [l5] ON CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END = [l5].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN (
            SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
            FROM [Level1] AS [l3]
            WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [l6] ON CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END = [l6].[OneToMany_Required_Inverse4Id]
    ) AS [s]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s0] ON [l].[Id] = [s0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s0].[Id], [s0].[c], [s0].[Id0], [s0].[Id00]
""");
    }

    public override async Task Project_collection_and_root_entity(bool async)
    {
        await base.Project_collection_and_root_entity(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_reference_collection_order_by_reference_navigation(bool async)
    {
        await base.Include_reference_collection_order_by_reference_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END, [l].[Id], [l1].[Id]
""");
    }

    public override async Task Multiple_optional_navigation_with_Include(bool async)
    {
        await base.Multiple_optional_navigation_with_Include(async);

        AssertSql(
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l].[Id], [l1].[Id], [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level3_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END = [l5].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id]
""");
    }

    public override async Task SelectMany_with_order_by_and_Include(bool async)
    {
        await base.SelectMany_with_order_by_and_Include(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l1].[Level2_Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(bool async)
    {
        await base.Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id]
""");
    }

    public override async Task Select_nav_prop_collection_one_to_many_required(bool async)
    {
        await base.Select_nav_prop_collection_one_to_many_required(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[c], [l1].[Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END AS [c], [l0].[Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Include_collection(bool async)
    {
        await base.Include_collection(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Multiple_complex_include_select(bool async)
    {
        await base.Multiple_complex_include_select(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Level3_Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l6] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l6].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id0], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[Level2_Optional_Id]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id], [l6].[Id], [s].[Id]
""");
    }

    public override async Task Project_collection_navigation_using_ef_property(bool async)
    {
        await base.Project_collection_navigation_using_ef_property(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
    {
        await base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[c]
""");
    }

    public override async Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(
        bool async)
    {
        await base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(async);

        AssertSql(
            """
@__p_0='30'

SELECT [l4].[Id], [l4].[Date], [l4].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
) AS [l4]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT TOP(40) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l4].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[Level2_Optional_Id]
) AS [s]
ORDER BY [l4].[Id], [s].[Id]
""");
    }

    public override async Task Project_collection_and_include(bool async)
    {
        await base.Project_collection_and_include(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[Id]
""");
    }

    public override async Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
    {
        await base.Complex_query_with_let_collection_projection_FirstOrDefault(async);

        AssertSql(
            """
SELECT [l].[Id], [l4].[Id], [l5].[Name], [l5].[Id], [l4].[c]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l3].[c], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Required_Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
    ) AS [l3]
    WHERE [l3].[row] <= 1
) AS [l4] ON [l].[Id] = [l4].[OneToMany_Optional_Inverse2Id]
OUTER APPLY (
    SELECT [l1].[Name], [l1].[Id]
    FROM [Level1] AS [l1]
    WHERE EXISTS (
        SELECT 1
        FROM [Level1] AS [l2]
        WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l1].[Id] = [l2].[OneToMany_Optional_Inverse2Id] AND [l2].[Id] = CASE
            WHEN [l4].[OneToOne_Required_PK_Date] IS NOT NULL AND [l4].[Level1_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l4].[Id]
        END)
) AS [l5]
ORDER BY [l].[Id], [l4].[Id]
""");
    }

    public override async Task Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(bool async)
    {
        await base.Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id], [l9].[Id], [l9].[Level2_Optional_Id], [l9].[Level2_Required_Id], [l9].[Level3_Name], [l9].[OneToMany_Optional_Inverse3Id], [l9].[OneToMany_Required_Inverse3Id], [l9].[OneToOne_Optional_PK_Inverse3Id], [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l8].[Id], [l8].[Level2_Optional_Id], [l8].[Level2_Required_Id], [l8].[Level3_Name], [l8].[OneToMany_Optional_Inverse3Id], [l8].[OneToMany_Required_Inverse3Id], [l8].[OneToOne_Optional_PK_Inverse3Id], [l8].[c]
    FROM (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l4].[OneToMany_Optional_Inverse3Id] ORDER BY CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END) AS [row], CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END AS [c]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l8]
    WHERE [l8].[row] <= 3
) AS [l9] ON CASE
    WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
END = [l9].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l7].[Id] AS [Id0], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l5]
    LEFT JOIN (
        SELECT [l6].[Id], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Level4_Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l6]
        WHERE [l6].[Level3_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l7] ON CASE
        WHEN [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l5].[Id]
    END = [l7].[Level3_Optional_Id]
    WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [s] ON CASE
    WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
END = [s].[OneToMany_Required_Inverse3Id]
WHERE CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END < 3 OR CASE
    WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
END > 8
ORDER BY [l].[Id], [l1].[Id], [l3].[Id], [l9].[OneToMany_Optional_Inverse3Id], [l9].[c], [l9].[Id], [s].[Id]
""");
    }

    public override async Task Multiple_SelectMany_with_Include(bool async)
    {
        await base.Multiple_SelectMany_with_Include(async);

        AssertSql(
            """
SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l].[Id], [l1].[Id], [l7].[Id], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level3_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l5] ON CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END = [l5].[Level3_Required_Id]
LEFT JOIN (
    SELECT [l6].[Id], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Level4_Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l6]
    WHERE [l6].[Level3_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [l7] ON CASE
    WHEN [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l3].[Id]
END = [l7].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id], [l5].[Id]
""");
    }

    public override async Task Projecting_collection_with_FirstOrDefault(bool async)
    {
        await base.Projecting_collection_with_FirstOrDefault(async);

        AssertSql(
            """
SELECT [l1].[Id], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM (
    SELECT TOP(1) [l].[Id]
    FROM [Level1] AS [l]
    WHERE [l].[Id] = 1
) AS [l1]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l1].[Id]
""");
    }

    public override async Task Orderby_SelectMany_with_Include1(bool async)
    {
        await base.Orderby_SelectMany_with_Include1(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Select_subquery_single_nested_subquery(bool async)
    {
        await base.Select_subquery_single_nested_subquery(async);

        AssertSql(
            """
SELECT [l].[Id], [l3].[Id], [l4].[Id], [l4].[Id0], [l3].[c]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l2].[c], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Required_Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l2]
    WHERE [l2].[row] <= 1
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT CASE
        WHEN [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l1].[Id]
    END AS [Id], [l1].[Id] AS [Id0], [l1].[OneToMany_Optional_Inverse3Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l4] ON CASE
    WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
END = [l4].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l3].[Id], [l4].[Id]
""");
    }

    public override async Task Filtered_include_OrderBy(bool async)
    {
        await base.Filtered_include_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Level2_Name]
""");
    }

    public override async Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
    {
        await base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Id1], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Level3_Name0], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l5].[Id] AS [Id1], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Level3_Name] AS [Level3_Name0], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l2].[c]
    FROM (
        SELECT TOP(2) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[Level2_Required_Id]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l5].[OneToMany_Optional_Inverse3Id]
) AS [s]
ORDER BY [l].[Id], [s].[c], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby(bool async)
    {
        await base.Include_collection_with_groupby_in_subquery_and_filter_after_groupby(async);

        AssertSql(
            """
SELECT [l4].[Id], [l4].[Date], [l4].[Name], [l2].[Name], [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id]
FROM (
    SELECT [l].[Name]
    FROM [Level1] AS [l]
    GROUP BY [l].[Name]
    HAVING [l].[Name] <> N'Foo' OR [l].[Name] IS NULL
) AS [l2]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Date], [l3].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Name] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l3]
    WHERE [l3].[row] <= 1
) AS [l4] ON [l2].[Name] = [l4].[Name]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l5] ON [l4].[Id] = [l5].[OneToMany_Optional_Inverse2Id]
ORDER BY [l2].[Name], [l4].[Id]
""");
    }

    public override async Task Filtered_include_variable_used_inside_filter(bool async)
    {
        await base.Filtered_include_variable_used_inside_filter(async);

        AssertSql(
            """
@__prm_0='Foo' (Size = 4000)

SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level2_Name] <> @__prm_0 OR [l0].[Level2_Name] IS NULL)
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[c]
""");
    }

    public override async Task Filtered_ThenInclude_OrderBy(bool async)
    {
        await base.Filtered_ThenInclude_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Level3_Name]
""");
    }

    public override async Task
        Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
    {
        await base
            .Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Id1], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Level3_Name0], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0], [l5].[Id] AS [Id1], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Level3_Name] AS [Level3_Name0], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l2].[c]
    FROM (
        SELECT TOP(2) [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l0].[Level2_Name] <> N'Foo' OR [l0].[Level2_Name] IS NULL)
        ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END
    ) AS [l2]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l3] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l3].[Level2_Required_Id]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END = [l5].[OneToMany_Optional_Inverse3Id]
) AS [s]
ORDER BY [l].[Id], [s].[c], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filtered_include_Skip_without_OrderBy(bool async)
    {
        await base.Filtered_include_Skip_without_OrderBy(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE 1 < [l1].[row]
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Skip_Take_on_grouping_element_into_non_entity(bool async)
    {
        await base.Skip_Take_on_grouping_element_into_non_entity(async);

        AssertSql(
            """
SELECT [l1].[Date], [l3].[Name], [l3].[Id]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Name], [l2].[Id], [l2].[Date]
    FROM (
        SELECT [l0].[Name], [l0].[Id], [l0].[Date], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l2]
    WHERE 1 < [l2].[row] AND [l2].[row] <= 6
) AS [l3] ON [l1].[Date] = [l3].[Date]
ORDER BY [l1].[Date], [l3].[Date], [l3].[Name]
""");
    }

    public override async Task Include_collection_multiple(bool async)
    {
        await base.Include_collection_multiple(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[Id2], [s].[Level2_Optional_Id0], [s].[Level2_Required_Id0], [s].[Level3_Name0], [s].[OneToMany_Optional_Inverse3Id0], [s].[OneToMany_Required_Inverse3Id0], [s].[OneToOne_Optional_PK_Inverse3Id0], [s].[Id3], [s].[Level3_Optional_Id0], [s].[Level3_Required_Id0], [s].[Level4_Name0], [s].[OneToMany_Optional_Inverse4Id0], [s].[OneToMany_Required_Inverse4Id0], [s].[OneToOne_Optional_PK_Inverse4Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l6].[Id] AS [Id2], [l6].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l6].[Level2_Required_Id] AS [Level2_Required_Id0], [l6].[Level3_Name] AS [Level3_Name0], [l6].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l6].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l6].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l8].[Id] AS [Id3], [l8].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l8].[Level3_Required_Id] AS [Level3_Required_Id0], [l8].[Level4_Name] AS [Level4_Name0], [l8].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l8].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l8].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Optional_Id]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l5]
        WHERE [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l6] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l6].[Level2_Optional_Id]
    LEFT JOIN (
        SELECT [l7].[Id], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l7]
        WHERE [l7].[Level3_Required_Id] IS NOT NULL AND [l7].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l8] ON CASE
        WHEN [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l6].[Id]
    END = [l8].[OneToMany_Optional_Inverse4Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Take_on_correlated_collection_in_projection(bool async)
    {
        await base.Take_on_correlated_collection_in_projection(async);

        AssertSql(
            """
SELECT [l].[Id], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE [l1].[row] <= 50
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Optional_navigation_with_Include_ThenInclude(bool async)
    {
        await base.Optional_navigation_with_Include_ThenInclude(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id0], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l2]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Optional_Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [s] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [s].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id], [s].[Id]
""");
    }

    public override async Task Including_reference_navigation_and_projecting_collection_navigation(bool async)
    {
        await base.Including_reference_navigation_and_projecting_collection_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l5].[Id], [l5].[OneToOne_Required_PK_Date], [l5].[Level1_Optional_Id], [l5].[Level1_Required_Id], [l5].[Level2_Name], [l5].[OneToMany_Optional_Inverse2Id], [l5].[OneToMany_Required_Inverse2Id], [l5].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[Level2_Optional_Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[OneToOne_Required_PK_Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Level2_Name], [l4].[OneToMany_Optional_Inverse2Id], [l4].[OneToMany_Required_Inverse2Id], [l4].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l4]
    WHERE [l4].[OneToOne_Required_PK_Date] IS NOT NULL AND [l4].[Level1_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l5] ON [l].[Id] = [l5].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id]
""");
    }

    public override async Task Optional_navigation_with_Include_and_order(bool async)
    {
        await base.Optional_navigation_with_Include_and_order(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l1].[Level2_Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task Include_partially_added_before_Where_and_then_build_upon(bool async)
    {
        await base.Include_partially_added_before_Where_and_then_build_upon(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id], [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToOne_Optional_PK_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l6].[Id] AS [Id0], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Level4_Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l4]
    LEFT JOIN (
        SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l5]
        WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l6] ON CASE
        WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
    END = [l6].[Level3_Optional_Id]
    WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [s] ON CASE
    WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
END = [s].[OneToMany_Optional_Inverse3Id]
WHERE CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END < 3 OR CASE
    WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
END > 8
ORDER BY [l].[Id], [l1].[Id], [l3].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_multiple_with_filter(bool async)
    {
        await base.Include_collection_multiple_with_filter(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id0], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l7].[Id] AS [Id1], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l6].[Id], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Level4_Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l6]
        WHERE [l6].[Level3_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l7] ON CASE
        WHEN [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l5].[Id]
    END = [l7].[Level3_Optional_Id]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Level3_Name], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)) > 0
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Project_collection_navigation(bool async)
    {
        await base.Project_collection_navigation(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(bool async)
    {
        await base.LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(async);

        AssertSql(
            """
@__validIds_0='["L1 01","L1 02"]' (Size = 4000)

SELECT CASE
    WHEN [s].[OneToOne_Required_PK_Date] IS NULL OR [s].[Level1_Required_Id] IS NULL OR [s].[OneToMany_Required_Inverse2Id] IS NULL THEN 0
    WHEN [s].[OneToOne_Required_PK_Date] IS NOT NULL AND [s].[Level1_Required_Id] IS NOT NULL AND [s].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [s].[Id0]
END, [l].[Id], [s].[Id], [s].[Id0], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l2].[Id] AS [Id0], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Required_Id], [l2].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Required_Id], [l1].[OneToMany_Required_Inverse2Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l2] ON [l0].[Id] = CASE
        WHEN [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l2].[Id]
    END
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[Level1_Required_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    WHERE [l3].[Level2_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l4] ON CASE
    WHEN [s].[OneToOne_Required_PK_Date] IS NOT NULL AND [s].[Level1_Required_Id] IS NOT NULL AND [s].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [s].[Id0]
END = [l4].[OneToMany_Required_Inverse3Id]
WHERE [l].[Name] IN (
    SELECT [v].[value]
    FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v]
)
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool async)
    {
        await base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id]
""");
    }

    public override async Task Take_on_grouping_element(bool async)
    {
        await base.Take_on_grouping_element(async);

        AssertSql(
            """
SELECT [l1].[Date], [l3].[Id], [l3].[Date], [l3].[Name]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name] DESC) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l2]
    WHERE [l2].[row] <= 10
) AS [l3] ON [l1].[Date] = [l3].[Date]
ORDER BY [l1].[Date], [l3].[Date], [l3].[Name] DESC
""");
    }

    public override async Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
    {
        await base.Null_check_in_Dto_projection_should_not_be_removed(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[c], [s].[Level3_Name], [s].[Id], [s].[Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [l2].[Level2_Required_Id] IS NULL OR [l2].[OneToMany_Required_Inverse3Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [l2].[Level3_Name], [l0].[Id], [l2].[Id] AS [Id0], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Required_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[Level2_Required_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Take(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Take(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE [l1].[row] <= 3
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Level2_Name]
""");
    }

    public override async Task Skip_Take_on_grouping_element_inside_collection_projection(bool async)
    {
        await base.Skip_Take_on_grouping_element_inside_collection_projection(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Date], [s].[Id], [s].[Date0], [s].[Name]
FROM [Level1] AS [l]
OUTER APPLY (
    SELECT [l2].[Date], [l4].[Id], [l4].[Date] AS [Date0], [l4].[Name]
    FROM (
        SELECT [l0].[Date]
        FROM [Level1] AS [l0]
        WHERE [l0].[Name] = [l].[Name] OR ([l0].[Name] IS NULL AND [l].[Name] IS NULL)
        GROUP BY [l0].[Date]
    ) AS [l2]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Date], [l3].[Name]
        FROM (
            SELECT [l1].[Id], [l1].[Date], [l1].[Name], ROW_NUMBER() OVER(PARTITION BY [l1].[Date] ORDER BY [l1].[Name]) AS [row]
            FROM [Level1] AS [l1]
            WHERE [l1].[Name] = [l].[Name] OR ([l1].[Name] IS NULL AND [l].[Name] IS NULL)
        ) AS [l3]
        WHERE 1 < [l3].[row] AND [l3].[row] <= 6
    ) AS [l4] ON [l2].[Date] = [l4].[Date]
) AS [s]
ORDER BY [l].[Id], [s].[Date], [s].[Date0], [s].[Name]
""");
    }

    public override async Task Collection_projection_over_GroupBy_over_parameter(bool async)
    {
        await base.Collection_projection_over_GroupBy_over_parameter(async);

        AssertSql(
            """
@__validIds_0='["L1 01","L1 02"]' (Size = 4000)

SELECT [l1].[Date], [l2].[Id]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    WHERE [l].[Name] IN (
        SELECT [v].[value]
        FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v]
    )
    GROUP BY [l].[Date]
) AS [l1]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date]
    FROM [Level1] AS [l0]
    WHERE [l0].[Name] IN (
        SELECT [v0].[value]
        FROM OPENJSON(@__validIds_0) WITH ([value] nvarchar(max) '$') AS [v0]
    )
) AS [l2] ON [l1].[Date] = [l2].[Date]
ORDER BY [l1].[Date]
""");
    }

    public override async Task Filtered_include_is_considered_loaded(bool async)
    {
        await base.Filtered_include_is_considered_loaded(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[c]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END) AS [row], CASE
            WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
        END AS [c]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE [l1].[row] <= 1
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[c]
""");
    }

    public override async Task Skip_on_grouping_element(bool async)
    {
        await base.Skip_on_grouping_element(async);

        AssertSql(
            """
SELECT [l1].[Date], [l3].[Id], [l3].[Date], [l3].[Name]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l1]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], ROW_NUMBER() OVER(PARTITION BY [l0].[Date] ORDER BY [l0].[Name]) AS [row]
        FROM [Level1] AS [l0]
    ) AS [l2]
    WHERE 1 < [l2].[row]
) AS [l3] ON [l1].[Date] = [l3].[Date]
ORDER BY [l1].[Date], [l3].[Date], [l3].[Name]
""");
    }

    public override async Task Include_reference_and_collection_order_by(bool async)
    {
        await base.Include_reference_and_collection_order_by(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task Skip_Take_Distinct_on_grouping_element(bool async)
    {
        await base.Skip_Take_Distinct_on_grouping_element(async);

        AssertSql(
            """
SELECT [l2].[Date], [l3].[Id], [l3].[Date], [l3].[Name]
FROM (
    SELECT [l].[Date]
    FROM [Level1] AS [l]
    GROUP BY [l].[Date]
) AS [l2]
OUTER APPLY (
    SELECT DISTINCT [l1].[Id], [l1].[Date], [l1].[Name]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name]
        FROM [Level1] AS [l0]
        WHERE [l2].[Date] = [l0].[Date]
        ORDER BY [l0].[Name]
        OFFSET 1 ROWS FETCH NEXT 5 ROWS ONLY
    ) AS [l1]
) AS [l3]
ORDER BY [l2].[Date]
""");
    }

    public override async Task Include_collection_ThenInclude_two_references(bool async)
    {
        await base.Include_collection_ThenInclude_two_references(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
    {
        await base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s0].[Id], [s0].[OneToOne_Required_PK_Date], [s0].[Level1_Optional_Id], [s0].[Level1_Required_Id], [s0].[Level2_Name], [s0].[OneToMany_Optional_Inverse2Id], [s0].[OneToMany_Required_Inverse2Id], [s0].[OneToOne_Optional_PK_Inverse2Id], [s0].[Id0], [s0].[Level2_Optional_Id], [s0].[Level2_Required_Id], [s0].[Level3_Name], [s0].[OneToMany_Optional_Inverse3Id], [s0].[OneToMany_Required_Inverse3Id], [s0].[OneToOne_Optional_PK_Inverse3Id], [s0].[Id00], [s0].[Level3_Optional_Id], [s0].[Level3_Required_Id], [s0].[Level4_Name], [s0].[OneToMany_Optional_Inverse4Id], [s0].[OneToMany_Required_Inverse4Id], [s0].[OneToOne_Optional_PK_Inverse4Id], [s0].[Id1], [s0].[Level3_Optional_Id0], [s0].[Level3_Required_Id0], [s0].[Level4_Name0], [s0].[OneToMany_Optional_Inverse4Id0], [s0].[OneToMany_Required_Inverse4Id0], [s0].[OneToOne_Optional_PK_Inverse4Id0]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [s].[Id] AS [Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0] AS [Id00], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[Id1], [s].[Level3_Optional_Id0], [s].[Level3_Required_Id0], [s].[Level4_Name0], [s].[OneToMany_Optional_Inverse4Id0], [s].[OneToMany_Required_Inverse4Id0], [s].[OneToOne_Optional_PK_Inverse4Id0], [s].[c]
    FROM [Level1] AS [l0]
    OUTER APPLY (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l5].[Id] AS [Id0], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [l6].[Id] AS [Id1], [l6].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l6].[Level3_Required_Id] AS [Level3_Required_Id0], [l6].[Level4_Name] AS [Level4_Name0], [l6].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l6].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l6].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [l4].[c]
        FROM (
            SELECT TOP(1) [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], CASE
                WHEN [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l1].[Id]
            END AS [c]
            FROM [Level1] AS [l1]
            WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL AND CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NOT NULL AND (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END = [l1].[OneToMany_Optional_Inverse3Id] OR (CASE
                WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
            END IS NULL AND [l1].[OneToMany_Optional_Inverse3Id] IS NULL)) AND ([l1].[Level3_Name] <> N'Foo' OR [l1].[Level3_Name] IS NULL)
            ORDER BY CASE
                WHEN [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l1].[Id]
            END
        ) AS [l4]
        LEFT JOIN (
            SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Level4_Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id]
            FROM [Level1] AS [l2]
            WHERE [l2].[Level3_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [l5] ON CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END = [l5].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN (
            SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
            FROM [Level1] AS [l3]
            WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
        ) AS [l6] ON CASE
            WHEN [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l4].[Id]
        END = [l6].[OneToMany_Required_Inverse4Id]
    ) AS [s]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s0] ON [l].[Id] = [s0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s0].[Id], [s0].[c], [s0].[Id0], [s0].[Id00]
""");
    }

    public override async Task Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(bool async)
    {
        await base.Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(async);

        AssertSql(
            """
SELECT [l].[Id], [s].[Id], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id0], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id], [s].[Id1]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l4].[Id] AS [Id0], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Level4_Name], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l0].[Id] AS [Id1], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Level4_Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l3]
        WHERE [l3].[Level3_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l4] ON CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END = [l4].[Level3_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [s].[Id1], [s].[Id]
""");
    }

    public override async Task Project_navigation_and_collection(bool async)
    {
        await base.Project_navigation_and_collection(async);

        AssertSql(
            """
SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Include_reference_ThenInclude_collection_order_by(bool async)
    {
        await base.Include_reference_ThenInclude_collection_order_by(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
    {
        await base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async);

        AssertSql();
    }

    public override async Task Multiple_complex_includes_self_ref(bool async)
    {
        await base.Multiple_complex_includes_self_ref(async);

        AssertSql();
    }

    public override async Task
        Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
    {
        await base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(
            async);

        AssertSql();
    }

    public override async Task Include_collection_with_multiple_orderbys_complex(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_complex(async);

        AssertSql();
    }

    public override async Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_complex_repeated(async);

        AssertSql();
    }

    public override async Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_complex_repeated_checked(async);

        AssertSql();
    }

    public override async Task Include_collection_with_multiple_orderbys_member(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_member(async);

        AssertSql();
    }

    public override async Task Include_collection_with_multiple_orderbys_methodcall(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_methodcall(async);

        AssertSql();
    }

    public override async Task Include_collection_with_multiple_orderbys_property(bool async)
    {
        await base.Include_collection_with_multiple_orderbys_property(async);

        AssertSql();
    }

    public override async Task Include_inside_subquery(bool async)
    {
        await base.Include_inside_subquery(async);

        AssertSql();
    }

    public override async Task Filtered_include_outer_parameter_used_inside_filter(bool async)
    {
        await base.Filtered_include_outer_parameter_used_inside_filter(async);

        AssertSql();
    }

    public override async Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
    {
        await base.Include_after_multiple_SelectMany_and_reference_navigation(async);

        AssertSql();
    }

    public override async Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
    {
        await base.Include_after_SelectMany_and_multiple_reference_navigations(async);

        AssertSql();
    }

    public override async Task Required_navigation_with_Include(bool async)
    {
        await base.Required_navigation_with_Include(async);

        AssertSql();
    }

    public override async Task Required_navigation_with_Include_ThenInclude(bool async)
    {
        await base.Required_navigation_with_Include_ThenInclude(async);

        AssertSql();
    }

    public override async Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
    {
        await base.SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(async);

        AssertSql();
    }

    public override async Task Complex_query_issue_21665(bool async)
    {
        await base.Complex_query_issue_21665(async);

        AssertSql();
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

    public override async Task Queryable_in_subquery_works_when_final_projection_is_List(bool async)
    {
        await base.Queryable_in_subquery_works_when_final_projection_is_List(async);

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

    public override async Task Projecting_collection_after_optional_reference_correlated_with_parent(bool async)
    {
        await base.Projecting_collection_after_optional_reference_correlated_with_parent(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Id], [l3].[ChildId], [l3].[ParentName], [l3].[Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
OUTER APPLY (
    SELECT CASE
        WHEN [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l2].[Id]
    END AS [ChildId], [l1].[Level2_Name] AS [ParentName], [l2].[Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END IS NOT NULL AND (CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END = [l2].[OneToMany_Optional_Inverse3Id] OR (CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END IS NULL AND [l2].[OneToMany_Optional_Inverse3Id] IS NULL))
) AS [l3]
ORDER BY [l].[Id], [l1].[Id]
""");
    }

    public override async Task Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(bool async)
    {
        await base.Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(async);

        AssertSql(
            """
SELECT [l].[Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l1].[Id], [l5].[Key], [l5].[Count]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l3] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l3].[Level2_Optional_Id]
OUTER APPLY (
    SELECT [l4].[Level3_Name] AS [Key], COUNT(*) AS [Count]
    FROM [Level1] AS [l4]
    WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL AND CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END IS NOT NULL AND (CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END = [l4].[OneToMany_Optional_Inverse3Id] OR (CASE
        WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
    END IS NULL AND [l4].[OneToMany_Optional_Inverse3Id] IS NULL))
    GROUP BY [l4].[Level3_Name]
) AS [l5]
ORDER BY [l].[Id], [l1].[Id], [l3].[Id]
""");
    }

    public override async Task Multiple_complex_includes_self_ref_EF_Property(bool async)
    {
        await base.Multiple_complex_includes_self_ref_EF_Property(async);

        AssertSql();
    }

    public override async Task Multiple_complex_includes_EF_Property(bool async)
    {
        await base.Multiple_complex_includes_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Level3_Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [l6] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l6].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id0], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[Level2_Optional_Id]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Id], [l6].[Id], [s].[Id]
""");
    }

    public override async Task Include_collection_multiple_with_filter_EF_Property(bool async)
    {
        await base.Include_collection_multiple_with_filter_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id], [s].[Id1], [s].[Level3_Optional_Id], [s].[Level3_Required_Id], [s].[Level4_Name], [s].[OneToMany_Optional_Inverse4Id], [s].[OneToMany_Required_Inverse4Id], [s].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l5].[Id] AS [Id0], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Level3_Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l7].[Id] AS [Id1], [l7].[Level3_Optional_Id], [l7].[Level3_Required_Id], [l7].[Level4_Name], [l7].[OneToMany_Optional_Inverse4Id], [l7].[OneToMany_Required_Inverse4Id], [l7].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l4]
        WHERE [l4].[Level2_Required_Id] IS NOT NULL AND [l4].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l5] ON CASE
        WHEN [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l3].[Id]
    END = [l5].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l6].[Id], [l6].[Level3_Optional_Id], [l6].[Level3_Required_Id], [l6].[Level4_Name], [l6].[OneToMany_Optional_Inverse4Id], [l6].[OneToMany_Required_Inverse4Id], [l6].[OneToOne_Optional_PK_Inverse4Id]
        FROM [Level1] AS [l6]
        WHERE [l6].[Level3_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse4Id] IS NOT NULL
    ) AS [l7] ON CASE
        WHEN [l5].[Level2_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse3Id] IS NOT NULL THEN [l5].[Id]
    END = [l7].[Level3_Optional_Id]
    WHERE [l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
WHERE (
    SELECT COUNT(*)
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Level3_Name], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToOne_Optional_PK_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id] AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)) > 0
ORDER BY [l].[Id], [s].[Id], [s].[Id0]
""");
    }

    public override async Task Filtered_include_basic_Where_EF_Property(bool async)
    {
        await base.Filtered_include_basic_Where_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND [l0].[Id] > 5
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]
""");
    }

    public override async Task Filtered_include_OrderBy_EF_Property(bool async)
    {
        await base.Filtered_include_OrderBy_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l1].[Level2_Name]
""");
    }

    public override async Task Filtered_include_basic_OrderBy_Skip_Take_EF_Property(bool async)
    {
        await base.Filtered_include_basic_OrderBy_Skip_Take_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Level2_Name]) AS [row]
        FROM [Level1] AS [l0]
        WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [l1]
    WHERE 1 < [l1].[row] AND [l1].[row] <= 4
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l2].[OneToMany_Optional_Inverse2Id], [l2].[Level2_Name]
""");
    }

    public override async Task Filtered_include_on_ThenInclude_EF_Property(bool async)
    {
        await base.Filtered_include_on_ThenInclude_EF_Property(async);

        AssertSql(
            """
SELECT [l].[Id], [l].[Date], [l].[Name], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Level3_Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Level3_Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id]
    FROM (
        SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l2].[OneToMany_Optional_Inverse3Id] ORDER BY [l2].[Level3_Name]) AS [row]
        FROM [Level1] AS [l2]
        WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND ([l2].[Level3_Name] <> N'Foo' OR [l2].[Level3_Name] IS NULL)
    ) AS [l3]
    WHERE 1 < [l3].[row] AND [l3].[row] <= 4
) AS [l4] ON CASE
    WHEN [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l1].[Id]
END = [l4].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l1].[Id], [l4].[OneToMany_Optional_Inverse3Id], [l4].[Level3_Name]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_nested(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_nested(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id], [s].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_reference(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_reference(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [s].[Id], [s].[OneToOne_Required_PK_Date], [s].[Level1_Optional_Id], [s].[Level1_Required_Id], [s].[Level2_Name], [s].[OneToMany_Optional_Inverse2Id], [s].[OneToMany_Required_Inverse2Id], [s].[OneToOne_Optional_PK_Inverse2Id], [s].[Id0], [s].[Level2_Optional_Id], [s].[Level2_Required_Id], [s].[Level3_Name], [s].[OneToMany_Optional_Inverse3Id], [s].[OneToMany_Required_Inverse3Id], [s].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
        FROM [Level1] AS [l1]
        WHERE [l1].[Level2_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [l2] ON CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END = [l2].[Level2_Optional_Id]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [s] ON [l].[Id] = [s].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id], [s].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_multiple(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_multiple(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l2] ON [l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l1]
    WHERE [l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Name], [l].[Id], [l2].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_collection_reference_same_level(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_collection_reference_same_level(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l1].[Id], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Name], [l].[Id], [l1].[Id]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_reference(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_reference(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
ORDER BY [l].[Name]
""");
    }

    public override async Task Final_GroupBy_property_entity_Include_reference_multiple(bool async)
    {
        await base.Final_GroupBy_property_entity_Include_reference_multiple(async);

        AssertSql(
            """
SELECT [l].[Name], [l].[Id], [l].[Date], [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l2]
    WHERE [l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l3] ON [l].[Id] = [l3].[Level1_Required_Id]
ORDER BY [l].[Name]
""");
    }

    public override async Task Project_collection_and_nested_conditional(bool async)
    {
        await base.Project_collection_and_nested_conditional(async);

        AssertSql(
            """
SELECT [l].[Id], [l1].[Level2_Name], [l1].[Id], CASE
    WHEN [l].[Id] = 1 THEN N'01'
    WHEN [l].[Id] = 2 THEN N'02'
    WHEN [l].[Id] = 3 THEN N'03'
    ELSE NULL
END
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Level2_Name], [l0].[Id], CASE
        WHEN [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL THEN [l0].[Id]
    END AS [c], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE [l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
WHERE CASE
    WHEN [l].[Id] = 1 THEN N'01'
    WHEN [l].[Id] = 2 THEN N'02'
    WHEN [l].[Id] = 3 THEN N'03'
    ELSE NULL
END = N'02'
ORDER BY [l].[Id], [l1].[c]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
