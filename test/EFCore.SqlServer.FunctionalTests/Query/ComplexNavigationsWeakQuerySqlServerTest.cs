// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsWeakQuerySqlServerTest : ComplexNavigationsWeakQueryRelationalTestBase<
        ComplexNavigationsWeakQuerySqlServerFixture>
    {
        public ComplexNavigationsWeakQuerySqlServerTest(
            ComplexNavigationsWeakQuerySqlServerFixture fixture,
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToOne_Required_PK_Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Level2_Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id]
FROM [Level1] AS [l]");
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToOne_Required_PK_Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Level2_Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Level3_Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id]
FROM [Level1] AS [l]");
        }

        public override async Task Simple_level1_level2_GroupBy_Count(bool async)
        {
            await base.Simple_level1_level2_GroupBy_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l]
GROUP BY [l].[Level3_Name]");
        }

        public override async Task Simple_level1_level2_GroupBy_Having_Count(bool async)
        {
            await base.Simple_level1_level2_GroupBy_Having_Count(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Level1] AS [l]
GROUP BY [l].[Level3_Name]
HAVING MIN(COALESCE([l].[Id], 0)) > 0");
        }

        public override async Task Simple_level1_level2_level3_include(bool async)
        {
            await base.Simple_level1_level2_level3_include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToOne_Required_PK_Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Level2_Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Level3_Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[Level3_Optional_Id], [l].[Level3_Required_Id], [l].[Level4_Name], [l].[OneToMany_Optional_Inverse4Id], [l].[OneToMany_Required_Inverse4Id], [l].[OneToOne_Optional_PK_Inverse4Id]
FROM [Level1] AS [l]");
        }

        public override async Task Nested_group_join_with_take(bool async)
        {
            await base.Nested_group_join_with_take(async);

            AssertSql(
                @"@__p_0='2'

SELECT [t1].[Level2_Name]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [t].[Id0] AS [Id00]
    FROM [Level1] AS [l]
    LEFT JOIN (
        SELECT [l0].[Level1_Optional_Id], [l0].[Id] AS [Id0]
        FROM [Level1] AS [l0]
        WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
    ORDER BY [l].[Id]
) AS [t0]
LEFT JOIN (
    SELECT [l1].[Level1_Optional_Id], [l1].[Level2_Name]
    FROM [Level1] AS [l1]
    WHERE ([l1].[OneToOne_Required_PK_Date] IS NOT NULL AND [l1].[Level1_Required_Id] IS NOT NULL) AND [l1].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t1] ON [t0].[Id00] = [t1].[Level1_Optional_Id]
ORDER BY [t0].[Id]");
        }

        public override async Task Explicit_GroupJoin_in_subquery_with_unrelated_projection2(bool async)
        {
            await base.Explicit_GroupJoin_in_subquery_with_unrelated_projection2(async);

            AssertSql(
                @"SELECT [t0].[Id]
FROM (
    SELECT DISTINCT [l].[Id], [l].[Date], [l].[Name]
    FROM [Level1] AS [l]
    LEFT JOIN (
        SELECT [l0].[Level1_Optional_Id], [l0].[Level2_Name]
        FROM [Level1] AS [l0]
        WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
    ) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]
    WHERE ([t].[Level2_Name] <> N'Foo') OR [t].[Level2_Name] IS NULL
) AS [t0]");
        }

        public override async Task Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
        {
            await base.Result_operator_nav_prop_reference_optional_via_DefaultIfEmpty(async);

            AssertSql(
                @"SELECT COALESCE(SUM(CASE
    WHEN ([t].[OneToOne_Required_PK_Date] IS NULL OR [t].[Level1_Required_Id] IS NULL) OR [t].[OneToMany_Required_Inverse2Id] IS NULL THEN 0
    ELSE [t].[Level1_Required_Id]
END), 0)
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT [l0].[OneToOne_Required_PK_Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[OneToMany_Required_Inverse2Id]
    FROM [Level1] AS [l0]
    WHERE ([l0].[OneToOne_Required_PK_Date] IS NOT NULL AND [l0].[Level1_Required_Id] IS NOT NULL) AND [l0].[OneToMany_Required_Inverse2Id] IS NOT NULL
) AS [t] ON [l].[Id] = [t].[Level1_Optional_Id]");
        }

        public override async Task SelectMany_with_Include1(bool async)
        {
            await base.SelectMany_with_Include1(async);

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
        SELECT [l3].[Id], [l4].[Id] AS [Id0]
        FROM [Level1] AS [l3]
        INNER JOIN [Level1] AS [l4] ON [l3].[Id] = [l4].[Id]
        WHERE [l3].[OneToMany_Required_Inverse2Id] IS NOT NULL AND ([l3].[Level1_Required_Id] IS NOT NULL AND [l3].[OneToOne_Required_PK_Date] IS NOT NULL)
    ) AS [t0] ON [l2].[Id] = [t0].[Id]
    WHERE [l2].[OneToMany_Required_Inverse3Id] IS NOT NULL AND [l2].[Level2_Required_Id] IS NOT NULL
) AS [t1] ON [t].[Id] = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t1].[Id], [t1].[Id0], [t1].[Id00]");
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.SelectMany_with_navigation_and_Distinct(async))).Message;

            Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyOuterElementOfCollectionJoin, message);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
