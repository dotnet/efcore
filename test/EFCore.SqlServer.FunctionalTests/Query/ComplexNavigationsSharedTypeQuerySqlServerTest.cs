// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsSharedTypeQuerySqlServerTest : ComplexNavigationsSharedQueryTypeRelationalTestBase<
        ComplexNavigationsSharedTypeQuerySqlServerFixture>
    {
        public ComplexNavigationsSharedTypeQuerySqlServerTest(
            ComplexNavigationsSharedTypeQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Simple_level1_include(bool async)
        {
            await base.Simple_level1_include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Id]");
        }

        public override async Task Simple_level1(bool async)
        {
            await base.Simple_level1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name]
FROM [Level1] AS [l]");
        }

        public override async Task Simple_level1_level2_include(bool async)
        {
            await base.Simple_level1_level2_include(async);
            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE ([l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL) AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]");
        }

        public override async Task Simple_level1_level2_GroupBy_Count(bool async)
        {
            await base.Simple_level1_level2_GroupBy_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level3_Name]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE ([l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL) AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
GROUP BY [t0].[Level3_Name]");
        }

        public override async Task Simple_level1_level2_GroupBy_Having_Count(bool async)
        {
            await base.Simple_level1_level2_GroupBy_Having_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level3_Name]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE ([l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL) AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [l5].[Id]
    FROM [Level1] AS [l5]
    INNER JOIN [Level1] AS [l6] ON [l5].[Id] = [l6].[Id]
    WHERE ([l5].[OneToOne_Required_PK_Date] IS NOT NULL AND [l5].[Level1_Required_Id] IS NOT NULL) AND [l5].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [l].[Id] = [t2].[Id]
GROUP BY [t0].[Level3_Name]
HAVING MIN(COALESCE([t2].[Id], 0)) > 0");
        }

        public override async Task Simple_level1_level2_level3_include(bool async)
        {
            await base.Simple_level1_level2_level3_include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Level3_Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t2].[Id], [t2].[Level3_Optional_Id], [t2].[Level3_Required_Id], [t2].[Level4_Name], [t2].[OneToMany_Optional_Inverse4Id], [t2].[OneToMany_Required_Inverse4Id], [t2].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE ([l3].[OneToOne_Required_PK_Date] IS NOT NULL AND [l3].[Level1_Required_Id] IS NOT NULL) AND [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t1] ON [l2].[Id] = [t1].[Id]
    WHERE [l2].[Level2_Required_Id] IS NOT NULL AND [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id]
    FROM [Level1] AS [l5]
    INNER JOIN (
        SELECT [l6].[Id]
        FROM [Level1] AS [l6]
        INNER JOIN (
            SELECT [l7].[Id]
            FROM [Level1] AS [l7]
            INNER JOIN [Level1] AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE ([l7].[OneToOne_Required_PK_Date] IS NOT NULL AND [l7].[Level1_Required_Id] IS NOT NULL) AND [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t4] ON [l6].[Id] = [t4].[Id]
        WHERE [l6].[Level2_Required_Id] IS NOT NULL AND [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[Level3_Required_Id] IS NOT NULL AND [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL
) AS [t2] ON [t0].[Id] = [t2].[Id]");
        }

        public override async Task Nested_group_join_with_take(bool async)
        {
            await base.Nested_group_join_with_take(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t2].[Level2_Name]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [t0].[Id0] AS [Id00]
    FROM [Level1] AS [l]
    LEFT JOIN (
        SELECT [t].[Id] AS [Id0], [t].[Level1_Optional_Id]
        FROM [Level1] AS [l0]
        LEFT JOIN (
            SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[OneToMany_Required_Inverse2Id]
            FROM [Level1] AS [l1]
            INNER JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]
            WHERE ([l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL) AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l0].[Id] = [t].[Id]
        WHERE ([t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL) AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
    ORDER BY [l].[Id]
) AS [t1]
LEFT JOIN (
    SELECT [t3].[Level1_Optional_Id], [t3].[Level2_Name]
    FROM [Level1] AS [l3]
    LEFT JOIN (
        SELECT [l4].[Id], [l4].[OneToOne_Required_PK_Date], [l4].[Level1_Optional_Id], [l4].[Level1_Required_Id], [l4].[Level2_Name], [l4].[OneToMany_Required_Inverse2Id]
        FROM [Level1] AS [l4]
        INNER JOIN [Level1] AS [l5] ON [l4].[Id] = [l5].[Id]
        WHERE ([l4].[OneToOne_Required_PK_Date] IS NOT NULL AND [l4].[Level1_Required_Id] IS NOT NULL) AND [l4].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t3] ON [l3].[Id] = [t3].[Id]
    WHERE ([t3].[OneToOne_Required_PK_Date] IS NOT NULL AND [t3].[Level1_Required_Id] IS NOT NULL) AND [t3].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t2] ON [t1].[Id00] = [t2].[Level1_Optional_Id]
ORDER BY [t1].[Id]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2(async);

            AssertSql(
                @"SELECT [t1].[Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    LEFT JOIN (
        SELECT [t].[Level1_Optional_Id], [t].[Level2_Name]
        FROM [Level1] AS [l0]
        LEFT JOIN (
            SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Required_Inverse2Id]
            FROM [Level1] AS [l1]
            INNER JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]
            WHERE ([l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL) AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
        ) AS [t] ON [l0].[Id] = [t].[Id]
        WHERE ([t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL) AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
    WHERE ([t0].[Level2_Name] <> N'Foo') OR [t0].[Level2_Name] IS NULL
) AS [t1]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT COALESCE(SUM(CASE
    WHEN ([t0].[OneToOne_Required_PK_Date] IS NULL OR [t0].[Level1_Required_Id] IS NULL) OR [t0].[OneToMany_Required_Inverse2Id] IS NULL THEN 0
    ELSE [t0].[Level1_Required_Id]
END), 0)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[OneToMany_Required_Inverse2Id]
        FROM [Level1] AS [l1]
        INNER JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]
        WHERE ([l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL) AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t] ON [l0].[Id] = [t].[Id]
    WHERE ([t].[OneToOne_Required_PK_Date] IS NOT NULL AND [t].[Level1_Required_Id] IS NOT NULL) AND [t].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
