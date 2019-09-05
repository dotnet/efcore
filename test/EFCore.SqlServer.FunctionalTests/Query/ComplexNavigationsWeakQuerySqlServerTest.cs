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
               @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l].[Id] = [t].[Id]");
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [t0].[Id] AS [Id0], [t0].[Id0] AS [Id00]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    WHERE [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l2].[Level2_Required_Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]");
        }

        public override async Task Simple_level1_level2_GroupBy_Count(bool isAsync)
        {
            await base.Simple_level1_level2_GroupBy_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [t0].[Id] AS [Id0], [t0].[Id0] AS [Id00]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    WHERE [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l2].[Level2_Required_Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
GROUP BY [t1].[Level3_Name]");
        }

        public override async Task Simple_level1_level2_GroupBy_Having_Count(bool isAsync)
        {
            await base.Simple_level1_level2_GroupBy_Having_Count(isAsync);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [t0].[Id] AS [Id0], [t0].[Id0] AS [Id00]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    WHERE [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l2].[Level2_Required_Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
GROUP BY [t1].[Level3_Name]
HAVING MIN(COALESCE([t].[Id], 0)) > 0");
        }

        public override async Task Simple_level1_level2_level3_include(bool isAsync)
        {
            await base.Simple_level1_level2_level3_include(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t4].[Id], [t4].[Level3_Optional_Id], [t4].[Level3_Required_Id], [t4].[Level4_Name], [t4].[OneToMany_Optional_Inverse4Id], [t4].[OneToMany_Required_Inverse4Id], [t4].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [t0].[Id] AS [Id0], [t0].[Id0] AS [Id00]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    WHERE [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l2].[Level2_Required_Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [l5].[Id], [l5].[Level3_Optional_Id], [l5].[Level3_Required_Id], [l5].[Level4_Name], [l5].[OneToMany_Optional_Inverse4Id], [l5].[OneToMany_Required_Inverse4Id], [l5].[OneToOne_Optional_PK_Inverse4Id], [t3].[Id] AS [Id0], [t3].[Id0] AS [Id00], [t3].[Id00] AS [Id000]
    FROM [Level1] AS [l5]
    INNER JOIN (
        SELECT [l6].[Id], [l6].[Level2_Optional_Id], [l6].[Level2_Required_Id], [l6].[Level3_Name], [l6].[OneToMany_Optional_Inverse3Id], [l6].[OneToMany_Required_Inverse3Id], [l6].[OneToOne_Optional_PK_Inverse3Id], [t2].[Id] AS [Id0], [t2].[Id0] AS [Id00]
        FROM [Level1] AS [l6]
        INNER JOIN (
            SELECT [l7].[Id], [l7].[OneToOne_Required_PK_Date], [l7].[Level1_Optional_Id], [l7].[Level1_Required_Id], [l7].[Level2_Name], [l7].[OneToMany_Optional_Inverse2Id], [l7].[OneToMany_Required_Inverse2Id], [l7].[OneToOne_Optional_PK_Inverse2Id], [l8].[Id] AS [Id0]
            FROM [Level1] AS [l7]
            INNER JOIN [Level1] AS [l8] ON [l7].[Id] = [l8].[Id]
            WHERE [l7].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l7].[Level1_Required_Id] IS NOT NULL AND [l7].[OneToOne_Required_PK_Date] IS NOT NULL)
        ) AS [t2] ON [l6].[Id] = [t2].[Id]
        WHERE [l6].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l6].[Level2_Required_Id] IS NOT NULL
    ) AS [t3] ON [l5].[Id] = [t3].[Id]
    WHERE [l5].[OneToMany_Required_Inverse4Id] IS NOT NULL AND [l5].[Level3_Required_Id] IS NOT NULL
) AS [t4] ON [t1].[Id] = [t4].[Id]");
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
                @"SELECT [t2].[Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    LEFT JOIN (
        SELECT [l0].[Id], [l0].[Date], [l0].[Name], [t].[Id] AS [Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id]
        FROM [Level1] AS [l0]
        LEFT JOIN (
            SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0]
            FROM [Level1] AS [l1]
            INNER JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]
            WHERE [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToOne_Required_PK_Date] IS NOT NULL)
        ) AS [t] ON [l0].[Id] = [t].[Id]
        WHERE [t].[Id] IS NOT NULL
    ) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t1] ON [t0].[Id] = [t1].[Id]
    WHERE ([t1].[Level2_Name] <> N'Foo') OR [t1].[Level2_Name] IS NULL
) AS [t2]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool isAsync)
        {
            await base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT SUM(CASE
    WHEN [t1].[Id] IS NULL THEN 0
    ELSE [t1].[Level1_Required_Id]
END)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Name], [t].[Id] AS [Id0], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id]
    FROM [Level1] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[OneToOne_Required_PK_Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2_Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l2].[Id] AS [Id0]
        FROM [Level1] AS [l1]
        INNER JOIN [Level1] AS [l2] ON [l1].[Id] = [l2].[Id]
        WHERE [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l1].[Level1_Required_Id] IS NOT NULL AND [l1].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t] ON [l0].[Id] = [t].[Id]
    WHERE [t].[Id] IS NOT NULL
) AS [t0] ON [l].[Id] = [t0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
    FROM [Level1] AS [l3]
    INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
    WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t1] ON [t0].[Id] = [t1].[Id]");
        }


        public override async Task SelectMany_with_Include1(bool isAsync)
        {
            await base.SelectMany_with_Include1(isAsync);

            AssertSql(
                @"SELECT [t].[Id], [t].[OneToOne_Required_PK_Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Level2_Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [l].[Id], [t].[Id0], [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Level3_Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[Id0], [t1].[Id00]
FROM [Level1] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Level2_Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l1].[Id] AS [Id0]
    FROM [Level1] AS [l0]
    INNER JOIN [Level1] AS [l1] ON [l0].[Id] = [l1].[Id]
    WHERE [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l0].[Level1_Required_Id] IS NOT NULL AND [l0].[OneToOne_Required_PK_Date] IS NOT NULL)
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Level3_Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [t0].[Id] AS [Id0], [t0].[Id0] AS [Id00]
    FROM [Level1] AS [l2]
    INNER JOIN (
        SELECT [l3].[Id], [l3].[OneToOne_Required_PK_Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Level2_Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    WHERE [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l2].[Level2_Required_Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
