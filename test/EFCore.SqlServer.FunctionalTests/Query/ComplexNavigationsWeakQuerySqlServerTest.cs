// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsWeakQuerySqlServerTest : ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQuerySqlServerFixture>
    {
        public ComplexNavigationsWeakQuerySqlServerTest(
            ComplexNavigationsWeakQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Simple_level1_include(bool isAsync)
        {
            await base.Simple_level1_include(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN [Level1] AS [l0] ON [l].[Id] = [l0].[Id]");
        }

        public override async Task Simple_level1(bool isAsync)
        {
            await base.Simple_level1(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name]
FROM [Level1] AS [l]");
        }

        public override async Task Simple_level1_level2_include(bool isAsync)
        {
            await base.Simple_level1_level2_include(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN [Level1] AS [l0] ON [l].[Id] = [l0].[Id]
LEFT JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]");
        }

        public override async Task Simple_level1_level2_GroupBy_Count(bool isAsync)
        {
            await base.Simple_level1_level2_GroupBy_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l1.OneToOne_Required_PK1].*
    FROM [Level1] AS [l1.OneToOne_Required_PK1]
    WHERE [l1.OneToOne_Required_PK1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1.OneToOne_Required_PK1].[Level1_Required_Id] IS NOT NULL AND [l1.OneToOne_Required_PK1].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l1].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l1.OneToOne_Required_PK1.OneToOne_Required_PK2].*
    FROM [Level1] AS [l1.OneToOne_Required_PK1.OneToOne_Required_PK2]
    WHERE ([l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[Level1_Required_Id] IS NOT NULL AND [l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[OneToOne_Required_PK_Date] IS NOT NULL)) AND ([l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[Level2_Required_Id] IS NOT NULL)
) AS [t0] ON [t].[Id] = [t0].[Id]
GROUP BY [t0].[Level3_Name]");
        }

        public override async Task Simple_level1_level2_GroupBy_Having_Count(bool isAsync)
        {
            await base.Simple_level1_level2_GroupBy_Having_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l1]
LEFT JOIN (
    SELECT [l1.OneToOne_Required_PK1].*
    FROM [Level1] AS [l1.OneToOne_Required_PK1]
    WHERE [l1.OneToOne_Required_PK1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1.OneToOne_Required_PK1].[Level1_Required_Id] IS NOT NULL AND [l1.OneToOne_Required_PK1].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l1].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l1.OneToOne_Required_PK1.OneToOne_Required_PK2].*
    FROM [Level1] AS [l1.OneToOne_Required_PK1.OneToOne_Required_PK2]
    WHERE ([l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[Level1_Required_Id] IS NOT NULL AND [l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[OneToOne_Required_PK_Date] IS NOT NULL)) AND ([l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l1.OneToOne_Required_PK1.OneToOne_Required_PK2].[Level2_Required_Id] IS NOT NULL)
) AS [t0] ON [t].[Id] = [t0].[Id]
GROUP BY [t0].[Level3_Name]
HAVING MIN(COALESCE([t].[Id], 0)) > 0");
        }

        public override async Task Simple_level1_level2_level3_include(bool isAsync)
        {
            await base.Simple_level1_level2_level3_include(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Level4_Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN [Level1] AS [l0] ON [l].[Id] = [l0].[Id]
LEFT JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
LEFT JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]");
        }

        public override async Task Nested_group_join_with_take(bool isAsync)
        {
            await base.Nested_group_join_with_take(isAsync);

            AssertSql(
                @"@__p_0='2'

SELECT [t5].[Level2_Name]
FROM (
    SELECT TOP(@__p_0) [t1].*
    FROM [Level1] AS [l1_inner]
    LEFT JOIN (
        SELECT [t0].[Id], [t0].[OneToOne_Required_PK_Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Level2_Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [t]
        LEFT JOIN (
            SELECT [t.OneToOne_Required_PK1].*
            FROM [Level1] AS [t.OneToOne_Required_PK1]
            WHERE [t.OneToOne_Required_PK1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([t.OneToOne_Required_PK1].[Level1_Required_Id] IS NOT NULL AND [t.OneToOne_Required_PK1].[OneToOne_Required_PK_Date] IS NOT NULL)
        ) AS [t0] ON [t].[Id] = [t0].[Id]
        WHERE [t0].[Id] IS NOT NULL
    ) AS [t1] ON [l1_inner].[Id] = [t1].[Level1_Optional_Id]
    ORDER BY [l1_inner].[Id]
) AS [t2]
LEFT JOIN (
    SELECT [t4].*
    FROM [Level1] AS [t3]
    LEFT JOIN (
        SELECT [t.OneToOne_Required_PK10].*
        FROM [Level1] AS [t.OneToOne_Required_PK10]
        WHERE [t.OneToOne_Required_PK10].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([t.OneToOne_Required_PK10].[Level1_Required_Id] IS NOT NULL AND [t.OneToOne_Required_PK10].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t4] ON [t3].[Id] = [t4].[Id]
    WHERE [t4].[Id] IS NOT NULL
) AS [t5] ON [t2].[Id] = [t5].[Level1_Optional_Id]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool isAsync)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2(isAsync);

            AssertSql(
                @"SELECT [t0].[Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    LEFT JOIN (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [l1].[Id] AS [Id0], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        LEFT JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
        WHERE [l1].[Id] IS NOT NULL
    ) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
    LEFT JOIN [Level1] AS [l2] ON [t].[Id] = [l2].[Id]
    WHERE ([l2].[Level2_Name] <> N'Foo') OR [l2].[Level2_Name] IS NULL
) AS [t0]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool isAsync)
        {
            await base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT SUM(CASE
    WHEN [l].[Id] IS NULL THEN 0
    ELSE [l].[Level1_Required_Id]
END)
FROM [Level1] AS [l0]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l2].[Id] AS [Id0], [l2].[OneToOne_Required_PK_Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Level2_Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l1]
    LEFT JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]
    WHERE [l2].[Id] IS NOT NULL
) AS [t] ON [l0].[Id] = [t].[Level1_Optional_Id]
LEFT JOIN [Level1] AS [l] ON [t].[Id] = [l].[Id]");
        }

        public override async Task SelectMany_with_Include1(bool isAsync)
        {
            await base.SelectMany_with_Include1(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[OneToOne_Required_PK_Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Level2_Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l0].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Level3_Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l0]
INNER JOIN [Level1] AS [l] ON [l0].[Id] = [l].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [Level1] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Id], [l].[Id], [l1].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
