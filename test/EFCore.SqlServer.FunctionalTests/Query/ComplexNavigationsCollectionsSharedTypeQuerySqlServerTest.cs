// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsSharedTypeQuerySqlServerTest : ComplexNavigationsCollectionsSharedQueryTypeRelationalTestBase<
        ComplexNavigationsSharedTypeQuerySqlServerFixture>
    {
        public ComplexNavigationsCollectionsSharedTypeQuerySqlServerTest(
            ComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SelectMany_with_Include1(bool async)
        {
            await base.SelectMany_with_Include1(async);

            AssertSql(
                @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [t].[Id0], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[Id0], [t0].[Id00]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [t1].[Id] AS [Id0], [t1].[Id0] AS [Id00]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE ([l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL) AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            await base.SelectMany_with_navigation_and_Distinct(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[Id0]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0]
    FROM [Level1] AS [l2]
    INNER JOIN [Level1] AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE ([l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL) AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
WHERE ([t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL) AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL
ORDER BY [l].[Id], [t].[Id], [t0].[Id]");
        }

        public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
        {
            await base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[Name], [t].[FK], [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[Id0]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Level2_Name] AS [Name], [l0].[OneToMany_Optional_Inverse2Id] AS [FK]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[FK]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l3].[Id] AS [Id0]
    FROM [Level1] AS [l2]
    INNER JOIN [Level1] AS [l3] ON [l2].[Id] = [l3].[Id]
    WHERE ([l2].[OneToOne_Required_PK_Date] IS NOT NULL AND [l2].[Level1_Required_Id] IS NOT NULL) AND [l2].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Name], [t].[FK], [t0].[Id]");
        }

        public override async Task Take_Select_collection_Take(bool async)
        {
            await base.Take_Select_collection_Take(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Name], [t0].[Id], [t0].[Name], [t0].[Level1Id], [t0].[Level2Id], [t0].[Id0], [t0].[Date], [t0].[Name0], [t0].[Id00]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
) AS [t]
OUTER APPLY (
    SELECT [t1].[Id], [t1].[Level2_Name] AS [Name], [t1].[OneToMany_Required_Inverse2Id] AS [Level1Id], [t1].[Level1_Required_Id] AS [Level2Id], [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name] AS [Name0], [t1].[Id0] AS [Id00]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id], [l2].[Id] AS [Id0]
        FROM [Level1] AS [l0]
        INNER JOIN [Level1] AS [l2] ON [l0].[Id] = [l2].[Id]
        WHERE (([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND ([t].[Id] = [l0].[OneToMany_Required_Inverse2Id])
        ORDER BY [l0].[Id]
    ) AS [t1]
    INNER JOIN [Level1] AS [l1] ON [t1].[Level1_Required_Id] = [l1].[Id]
) AS [t0]
ORDER BY [t].[Id], [t0].[Id], [t0].[Id00]");
        }

        public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
        {
            await base.Skip_Take_Select_collection_Skip_Take(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Name], [t0].[Id], [t0].[Name], [t0].[Level1Id], [t0].[Level2Id], [t0].[Id0], [t0].[Date], [t0].[Name0], [t0].[Id00]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [Level1] AS [l]
    ORDER BY [l].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [t]
OUTER APPLY (
    SELECT [t1].[Id], [t1].[Level2_Name] AS [Name], [t1].[OneToMany_Required_Inverse2Id] AS [Level1Id], [t1].[Level1_Required_Id] AS [Level2Id], [l1].[Id] AS [Id0], [l1].[Date], [l1].[Name] AS [Name0], [t1].[Id0] AS [Id00]
    FROM (
        SELECT [l0].[Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Required_Inverse2Id], [l2].[Id] AS [Id0]
        FROM [Level1] AS [l0]
        INNER JOIN [Level1] AS [l2] ON [l0].[Id] = [l2].[Id]
        WHERE (([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL) AND ([t].[Id] = [l0].[OneToMany_Required_Inverse2Id])
        ORDER BY [l0].[Id]
        OFFSET 1 ROWS FETCH NEXT 3 ROWS ONLY
    ) AS [t1]
    INNER JOIN [Level1] AS [l1] ON [t1].[Level1_Required_Id] = [l1].[Id]
) AS [t0]
ORDER BY [t].[Id], [t0].[Id], [t0].[Id00]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
