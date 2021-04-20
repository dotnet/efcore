// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsSplitQuerySqlServerTest : ComplexNavigationsCollectionsSplitQueryRelationalTestBase<ComplexNavigationsQuerySqlServerFixture>
    {
        public ComplexNavigationsCollectionsSplitQuerySqlServerTest(
            ComplexNavigationsQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Filtered_include_basic_Where(bool async)
        {
            await base.Filtered_include_basic_Where(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id]");
        }

        public override async Task Filtered_include_OrderBy(bool async)
        {
            await base.Filtered_include_OrderBy(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 0 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]");
        }

        public override async Task Filtered_ThenInclude_OrderBy(bool async)
        {
            await base.Filtered_ThenInclude_OrderBy(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
    ) AS [t]
    WHERE 0 < [t].[row]
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[OneToMany_Optional_Inverse3Id], [t0].[Name]");
        }

        public override async Task Filtered_include_ThenInclude_OrderBy(bool async)
        {
            await base.Filtered_include_ThenInclude_OrderBy(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 0 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]",
                //
                @"SELECT [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id], [l].[Id], [t0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 0 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t2].[Id], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Optional_Self_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToMany_Required_Self_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name] DESC) AS [row]
        FROM [LevelThree] AS [l1]
    ) AS [t2]
    WHERE 0 < [t2].[row]
) AS [t1] ON [t0].[Id] = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[Name] DESC");
        }

        public override async Task Filtered_include_basic_OrderBy_Take(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Take(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]");
        }

        public override async Task Filtered_include_basic_OrderBy_Skip(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Skip(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]");
        }

        public override async Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Skip_Take(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]");
        }

        [ConditionalFact(Skip = "issue #24708")]
        public override void Filtered_include_Skip_without_OrderBy()
        {
            base.Filtered_include_Skip_without_OrderBy();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        [ConditionalFact(Skip = "issue #24708")]
        public override void Filtered_include_Take_without_OrderBy()
        {
            base.Filtered_include_Take_without_OrderBy();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        public override async Task Filtered_include_on_ThenInclude(bool async)
        {
            await base.Filtered_include_on_ThenInclude(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[OneToMany_Optional_Inverse3Id], [t0].[Name]");
        }

        public override async Task Filtered_include_after_reference_navigation(bool async)
        {
            await base.Filtered_include_after_reference_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
ORDER BY [l].[Id], [l0].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
INNER JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[OneToMany_Optional_Inverse3Id], [t0].[Name]");
        }

        public override async Task Filtered_include_after_different_filtered_include_same_level(bool async)
        {
            await base.Filtered_include_after_different_filtered_include_same_level(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Required_Inverse2Id] ORDER BY [l0].[Name] DESC) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Bar') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[Name] DESC");
        }

        public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
        {
            await base.Filtered_include_after_different_filtered_include_different_level(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name]",
                //
                @"SELECT [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id], [l].[Id], [t0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t2].[Id], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Name], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Optional_Self_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToMany_Required_Self_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse3Id] ORDER BY [l1].[Name] DESC) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Bar') OR [l1].[Name] IS NULL
    ) AS [t2]
    WHERE 1 < [t2].[row]
) AS [t1] ON [t0].[Id] = [t1].[OneToMany_Required_Inverse3Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name], [t1].[OneToMany_Required_Inverse3Id], [t1].[Name] DESC");
        }

        public override async Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
        {
            await base.Filtered_include_same_filter_set_on_same_navigation_twice(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id] DESC) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 2
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id] DESC");
        }

        public override async Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
        {
            await base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(2) [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[Level2_Required_Id]
) AS [t0]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]",
                //
                @"SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l].[Id], [t0].[Id], [t0].[Id0]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [t].[Id], [l0].[Id] AS [Id0]
    FROM (
        SELECT TOP(2) [l1].[Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[Level2_Required_Id]
) AS [t0]
INNER JOIN [LevelThree] AS [l2] ON [t0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
        {
            await base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(2) [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[Level2_Required_Id]
) AS [t0]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]",
                //
                @"SELECT [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l].[Id], [t0].[Id], [t0].[Id0]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [t].[Id], [l0].[Id] AS [Id0]
    FROM (
        SELECT TOP(2) [l1].[Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[Level2_Required_Id]
) AS [t0]
INNER JOIN [LevelThree] AS [l2] ON [t0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        public override async Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        public override async Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [l].[Id]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(1) [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[OneToOne_Optional_PK_Inverse3Id]
) AS [t0]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]",
                //
                @"SELECT [t1].[Id], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Optional_Self_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToMany_Required_Self_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[OneToOne_Optional_Self4Id], [l].[Id], [t0].[Id], [t0].[Id0]
FROM [LevelOne] AS [l]
CROSS APPLY (
    SELECT [t].[Id], [l0].[Id] AS [Id0]
    FROM (
        SELECT TOP(1) [l1].[Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[OneToOne_Optional_PK_Inverse3Id]
) AS [t0]
INNER JOIN (
    SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelFour] AS [l2]
    WHERE [l2].[Id] > 1
) AS [t1] ON [t0].[Id0] = [t1].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
        {
            await base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse3Id]",
                //
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [t0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [t0].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse3Id]",
                //
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [t0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [t0].[Id] = [l2].[OneToMany_Required_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse3Id]");
        }

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
        {
            await base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [l].[Id], [l0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse3Id]",
                //
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [t0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [t0].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse3Id]",
                //
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [t0].[Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN (
    SELECT [t].[Id], [t].[OneToMany_Optional_Inverse3Id]
    FROM (
        SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
INNER JOIN [LevelFour] AS [l2] ON [t0].[Id] = [l2].[OneToMany_Required_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[Id], [t0].[OneToMany_Optional_Inverse3Id]");
        }

        [ConditionalFact(Skip = "issue #24708")]
        public override void Filtered_include_variable_used_inside_filter()
        {
            base.Filtered_include_variable_used_inside_filter();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"@__prm_0='Foo' (Size = 4000)

SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> @__prm_0) OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        [ConditionalFact(Skip = "issue #24708")]
        public override void Filtered_include_context_accessed_inside_filter()
        {
            base.Filtered_include_context_accessed_inside_filter();

            AssertSql(
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l]",
                //
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"@__p_0='True'

SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE @__p_0 = CAST(1 AS bit)
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        [ConditionalFact(Skip = "issue #24708")]
        public override void Filtered_include_context_accessed_inside_filter_correlated()
        {
            base.Filtered_include_context_accessed_inside_filter_correlated();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
FROM [LevelOne] AS [l]
ORDER BY [l].[Id]",
                //
                @"SELECT [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [l].[Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE (
            SELECT COUNT(*)
            FROM [LevelOne] AS [l1]
            WHERE [l1].[Id] <> [l0].[Id]) > 1
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        public override async Task Filtered_include_outer_parameter_used_inside_filter(bool async)
        {
            await base.Filtered_include_outer_parameter_used_inside_filter(async);

            AssertSql(" ");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
