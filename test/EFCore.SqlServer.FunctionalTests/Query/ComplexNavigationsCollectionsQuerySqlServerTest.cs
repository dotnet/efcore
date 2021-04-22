// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsQuerySqlServerTest : ComplexNavigationsCollectionsQueryRelationalTestBase<ComplexNavigationsQuerySqlServerFixture>
    {
        public ComplexNavigationsCollectionsQuerySqlServerTest(
            ComplexNavigationsQuerySqlServerFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(bool async)
        {
            await base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(bool async)
        {
            await base.Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [t0].[Id00], [t0].[Date0], [t0].[Level1_Optional_Id0], [t0].[Level1_Required_Id0], [t0].[Name00], [t0].[OneToMany_Optional_Inverse2Id0], [t0].[OneToMany_Optional_Self_Inverse2Id0], [t0].[OneToMany_Required_Inverse2Id0], [t0].[OneToMany_Required_Self_Inverse2Id0], [t0].[OneToOne_Optional_PK_Inverse2Id0], [t0].[OneToOne_Optional_Self2Id0], [t0].[Id1], [t0].[Level2_Optional_Id0], [t0].[Level2_Required_Id0], [t0].[Name1], [t0].[OneToMany_Optional_Inverse3Id0], [t0].[OneToMany_Optional_Self_Inverse3Id0], [t0].[OneToMany_Required_Inverse3Id0], [t0].[OneToMany_Required_Self_Inverse3Id0], [t0].[OneToOne_Optional_PK_Inverse3Id0], [t0].[OneToOne_Optional_Self3Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t].[Id] AS [Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name] AS [Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id0] AS [Id00], [t].[Date] AS [Date0], [t].[Level1_Optional_Id] AS [Level1_Optional_Id0], [t].[Level1_Required_Id] AS [Level1_Required_Id0], [t].[Name0] AS [Name00], [t].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id0], [t].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id0], [t].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id0], [t].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id0], [t].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id0], [t].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id0], [t].[Id1], [t].[Level2_Optional_Id0], [t].[Level2_Required_Id0], [t].[Name1], [t].[OneToMany_Optional_Inverse3Id0], [t].[OneToMany_Optional_Self_Inverse3Id0], [t].[OneToMany_Required_Inverse3Id0], [t].[OneToMany_Required_Self_Inverse3Id0], [t].[OneToOne_Optional_PK_Inverse3Id0], [t].[OneToOne_Optional_Self3Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id0], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name] AS [Name0], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id1], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Name] AS [Name1], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l3].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0]
        FROM [LevelThree] AS [l1]
        INNER JOIN [LevelTwo] AS [l2] ON [l1].[OneToMany_Required_Inverse3Id] = [l2].[Id]
        LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[OneToMany_Optional_Inverse3Id]
    ) AS [t] ON [l0].[Id] = [t].[OneToMany_Optional_Inverse3Id]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0], [t0].[Id00], [t0].[Id1]");
        }

        public override async Task Multiple_complex_includes(bool async)
        {
            await base.Multiple_complex_includes(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Multiple_complex_includes_self_ref(bool async)
        {
            await base.Multiple_complex_includes_self_ref(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id], [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [t].[Id0], [t].[Date0], [t].[Name0], [t].[OneToMany_Optional_Self_Inverse1Id0], [t].[OneToMany_Required_Self_Inverse1Id0], [t].[OneToOne_Optional_Self1Id0]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelOne] AS [l0] ON [l].[OneToOne_Optional_Self1Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Self_Inverse1Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Name], [l2].[OneToMany_Optional_Self_Inverse1Id], [l2].[OneToMany_Required_Self_Inverse1Id], [l2].[OneToOne_Optional_Self1Id], [l3].[Id] AS [Id0], [l3].[Date] AS [Date0], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Self_Inverse1Id] AS [OneToMany_Optional_Self_Inverse1Id0], [l3].[OneToMany_Required_Self_Inverse1Id] AS [OneToMany_Required_Self_Inverse1Id0], [l3].[OneToOne_Optional_Self1Id] AS [OneToOne_Optional_Self1Id0]
    FROM [LevelOne] AS [l2]
    LEFT JOIN [LevelOne] AS [l3] ON [l2].[OneToOne_Optional_Self1Id] = [l3].[Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Self_Inverse1Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Include_reference_and_collection_order_by(bool async)
        {
            await base.Include_reference_and_collection_order_by(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Include_reference_ThenInclude_collection_order_by(bool async)
        {
            await base.Include_reference_ThenInclude_collection_order_by(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Include_collection_then_reference(bool async)
        {
            await base.Include_collection_then_reference(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Include_collection_with_conditional_order_by(bool async)
        {
            await base.Include_collection_with_conditional_order_by(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY CASE
    WHEN [l].[Name] IS NOT NULL AND ([l].[Name] LIKE N'%03') THEN 1
    ELSE 2
END, [l].[Id], [l0].[Id]");
        }

        public override async Task Multiple_complex_include_select(bool async)
        {
            await base.Multiple_complex_include_select(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[Level2_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Include_nested_with_optional_navigation(bool async)
        {
            await base.Include_nested_with_optional_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id0], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id0], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name0], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelThree] AS [l1]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
) AS [t] ON [l0].[Id] = [t].[OneToMany_Required_Inverse3Id]
WHERE ([l0].[Name] <> N'L2 09') OR [l0].[Name] IS NULL
ORDER BY [l].[Id], [l0].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Complex_multi_include_with_order_by_and_paging(bool async)
        {
            await base.Complex_multi_include_with_order_by_and_paging(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToMany_Required_Inverse3Id]
ORDER BY [t].[Name], [t].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(bool async)
        {
            await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelThree] AS [l3] ON [l1].[Id] = [l3].[OneToMany_Required_Inverse3Id]
ORDER BY [t].[Name], [t].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [l3].[Id]");
        }

        public override async Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(bool async)
        {
            await base.Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2(async);

            AssertSql(
                @"@__p_0='0'
@__p_1='10'

SELECT [t].[Id], [t].[Date], [t].[Name], [t].[OneToMany_Optional_Self_Inverse1Id], [t].[OneToMany_Required_Self_Inverse1Id], [t].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
FROM (
    SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Name]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [t].[Name], [t].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override async Task Multiple_include_with_multiple_optional_navigations(bool async)
        {
            await base.Multiple_include_with_multiple_optional_navigations(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l2].[Id], [l3].[Id], [l4].[Id], [l5].[Id], [l5].[Level2_Optional_Id], [l5].[Level2_Required_Id], [l5].[Name], [l5].[OneToMany_Optional_Inverse3Id], [l5].[OneToMany_Optional_Self_Inverse3Id], [l5].[OneToMany_Required_Inverse3Id], [l5].[OneToMany_Required_Self_Inverse3Id], [l5].[OneToOne_Optional_PK_Inverse3Id], [l5].[OneToOne_Optional_Self3Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id], [l3].[Date], [l3].[Level1_Optional_Id], [l3].[Level1_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse2Id], [l3].[OneToMany_Optional_Self_Inverse2Id], [l3].[OneToMany_Required_Inverse2Id], [l3].[OneToMany_Required_Self_Inverse2Id], [l3].[OneToOne_Optional_PK_Inverse2Id], [l3].[OneToOne_Optional_Self2Id], [l4].[Level2_Optional_Id], [l4].[Level2_Required_Id], [l4].[Name], [l4].[OneToMany_Optional_Inverse3Id], [l4].[OneToMany_Optional_Self_Inverse3Id], [l4].[OneToMany_Required_Inverse3Id], [l4].[OneToMany_Required_Self_Inverse3Id], [l4].[OneToOne_Optional_PK_Inverse3Id], [l4].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l3] ON [l].[Id] = [l3].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l4] ON [l3].[Id] = [l4].[Level2_Optional_Id]
LEFT JOIN [LevelThree] AS [l5] ON [l0].[Id] = [l5].[OneToMany_Optional_Inverse3Id]
WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [l3].[Id], [l4].[Id], [l5].[Id]");
        }

        public override async Task SelectMany_with_Include1(bool async)
        {
            await base.SelectMany_with_Include1(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Orderby_SelectMany_with_Include1(bool async)
        {
            await base.Orderby_SelectMany_with_Include1(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task SelectMany_with_Include2(bool async)
        {
            await base.SelectMany_with_Include2(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]");
        }

        public override async Task SelectMany_with_Include_ThenInclude(bool async)
        {
            await base.SelectMany_with_Include_ThenInclude(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l].[Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override async Task Multiple_SelectMany_with_Include(bool async)
        {
            await base.Multiple_SelectMany_with_Include(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l3].[Id], [l3].[Level3_Optional_Id], [l3].[Level3_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse4Id], [l3].[OneToMany_Optional_Self_Inverse4Id], [l3].[OneToMany_Required_Inverse4Id], [l3].[OneToMany_Required_Self_Inverse4Id], [l3].[OneToOne_Optional_PK_Inverse4Id], [l3].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Required_Id]
LEFT JOIN [LevelFour] AS [l3] ON [l1].[Id] = [l3].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [l3].[Id]");
        }

        public override async Task Required_navigation_with_Include(bool async)
        {
            await base.Required_navigation_with_Include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Date], [l0].[Name], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
FROM [LevelThree] AS [l1]
INNER JOIN [LevelTwo] AS [l] ON [l1].[Level2_Required_Id] = [l].[Id]
INNER JOIN [LevelOne] AS [l0] ON [l].[OneToMany_Required_Inverse2Id] = [l0].[Id]");
        }

        public override async Task Required_navigation_with_Include_ThenInclude(bool async)
        {
            await base.Required_navigation_with_Include_ThenInclude(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse3Id], [l].[OneToMany_Optional_Self_Inverse3Id], [l].[OneToMany_Required_Inverse3Id], [l].[OneToMany_Required_Self_Inverse3Id], [l].[OneToOne_Optional_PK_Inverse3Id], [l].[OneToOne_Optional_Self3Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id]
FROM [LevelFour] AS [l2]
INNER JOIN [LevelThree] AS [l] ON [l2].[Level3_Required_Id] = [l].[Id]
INNER JOIN [LevelTwo] AS [l0] ON [l].[OneToMany_Required_Inverse3Id] = [l0].[Id]
LEFT JOIN [LevelOne] AS [l1] ON [l0].[OneToMany_Optional_Inverse2Id] = [l1].[Id]");
        }

        public override async Task Optional_navigation_with_Include_ThenInclude(bool async)
        {
            await base.Optional_navigation_with_Include_ThenInclude(async);

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK1].[Id], [l1.OneToOne_Optional_FK1].[Date], [l1.OneToOne_Optional_FK1].[Level1_Optional_Id], [l1.OneToOne_Optional_FK1].[Level1_Required_Id], [l1.OneToOne_Optional_FK1].[Name], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Self_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Self_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_PK_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK1] ON [l1].[Id] = [l1.OneToOne_Optional_FK1].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK1].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Level2_Optional_Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Level2_Required_Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Name], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Optional_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Required_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToOne_Optional_Self3Id], [l.OneToOne_Optional_FK3].[Id], [l.OneToOne_Optional_FK3].[Level3_Optional_Id], [l.OneToOne_Optional_FK3].[Level3_Required_Id], [l.OneToOne_Optional_FK3].[Name], [l.OneToOne_Optional_FK3].[OneToMany_Optional_Inverse4Id], [l.OneToOne_Optional_FK3].[OneToMany_Optional_Self_Inverse4Id], [l.OneToOne_Optional_FK3].[OneToMany_Required_Inverse4Id], [l.OneToOne_Optional_FK3].[OneToMany_Required_Self_Inverse4Id], [l.OneToOne_Optional_FK3].[OneToOne_Optional_PK_Inverse4Id], [l.OneToOne_Optional_FK3].[OneToOne_Optional_Self4Id]
FROM [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToMany_Optional2]
LEFT JOIN [LevelFour] AS [l.OneToOne_Optional_FK3] ON [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Id] = [l.OneToOne_Optional_FK3].[Level3_Optional_Id]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK10].[Id]
    FROM [LevelOne] AS [l10]
    LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK10] ON [l10].[Id] = [l1.OneToOne_Optional_FK10].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Optional_Inverse3Id] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task Multiple_optional_navigation_with_Include(bool async)
        {
            await base.Multiple_optional_navigation_with_Include(async);

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Level2_Optional_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Level2_Required_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Name], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Optional_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Required_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK1] ON [l1].[Id] = [l1.OneToOne_Optional_FK1].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2] ON [l1.OneToOne_Optional_FK1].[Id] = [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToOne_Optional_PK_Inverse3Id]
ORDER BY [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Level3_Optional_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Level3_Required_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Name], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Required_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Required_Self_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToOne_Optional_Self4Id]
FROM [LevelFour] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK20].[Id]
    FROM [LevelOne] AS [l10]
    LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK10] ON [l10].[Id] = [l1.OneToOne_Optional_FK10].[Level1_Optional_Id]
    LEFT JOIN [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK20] ON [l1.OneToOne_Optional_FK10].[Id] = [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK20].[OneToOne_Optional_PK_Inverse3Id]
) AS [t] ON [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task Multiple_optional_navigation_with_string_based_Include(bool async)
        {
            await base.Multiple_optional_navigation_with_string_based_Include(async);

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Level2_Optional_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Level2_Required_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Name], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Optional_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Required_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK1] ON [l1].[Id] = [l1.OneToOne_Optional_FK1].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2] ON [l1.OneToOne_Optional_FK1].[Id] = [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[OneToOne_Optional_PK_Inverse3Id]
ORDER BY [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Level3_Optional_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Level3_Required_Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[Name], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Required_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Required_Self_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToOne_Optional_Self4Id]
FROM [LevelFour] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK20].[Id]
    FROM [LevelOne] AS [l10]
    LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK10] ON [l10].[Id] = [l1.OneToOne_Optional_FK10].[Level1_Optional_Id]
    LEFT JOIN [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK20] ON [l1.OneToOne_Optional_FK10].[Id] = [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK20].[OneToOne_Optional_PK_Inverse3Id]
) AS [t] ON [l1.OneToOne_Optional_FK1.OneToOne_Optional_PK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task Optional_navigation_with_order_by_and_Include(bool async)
        {
            await base.Optional_navigation_with_order_by_and_Include(async);

            AssertSql(
                @"SELECT [l1.OneToOne_Optional_FK1].[Id], [l1.OneToOne_Optional_FK1].[Date], [l1.OneToOne_Optional_FK1].[Level1_Optional_Id], [l1.OneToOne_Optional_FK1].[Level1_Required_Id], [l1.OneToOne_Optional_FK1].[Name], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Optional_Self_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToMany_Required_Self_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_PK_Inverse2Id], [l1.OneToOne_Optional_FK1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l1]
LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK1] ON [l1].[Id] = [l1.OneToOne_Optional_FK1].[Level1_Optional_Id]
ORDER BY [l1.OneToOne_Optional_FK1].[Name], [l1.OneToOne_Optional_FK1].[Id]",
                //
                @"SELECT [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Level2_Optional_Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Level2_Required_Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[Name], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Optional_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Required_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToOne_Optional_Self3Id]
FROM [LevelThree] AS [l1.OneToOne_Optional_FK1.OneToMany_Optional2]
INNER JOIN (
    SELECT DISTINCT [l1.OneToOne_Optional_FK10].[Id], [l1.OneToOne_Optional_FK10].[Name]
    FROM [LevelOne] AS [l10]
    LEFT JOIN [LevelTwo] AS [l1.OneToOne_Optional_FK10] ON [l10].[Id] = [l1.OneToOne_Optional_FK10].[Level1_Optional_Id]
) AS [t] ON [l1.OneToOne_Optional_FK1.OneToMany_Optional2].[OneToMany_Optional_Inverse3Id] = [t].[Id]
ORDER BY [t].[Name], [t].[Id]");
        }

        public override async Task Optional_navigation_with_Include_and_order(bool async)
        {
            await base.Optional_navigation_with_Include_and_order(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l0]
LEFT JOIN [LevelTwo] AS [l] ON [l0].[Id] = [l].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l0].[Id]");
        }

        public override async Task SelectMany_with_order_by_and_Include(bool async)
        {
            await base.SelectMany_with_order_by_and_Include(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task SelectMany_with_Include_and_order_by(bool async)
        {
            await base.SelectMany_with_Include_and_order_by(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Name], [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            await base.SelectMany_with_navigation_and_Distinct(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [l1].[Id]");
        }

        public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
        {
            await base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id] AS [FK]
    FROM [LevelTwo] AS [l0]
) AS [t] ON [l].[Id] = [t].[FK]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [l1].[Id]");
        }

        public override async Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool async)
        {
            await base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(async);

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [t].[Id], [t].[Id0], [t].[Id1], [t].[Id2], [t0].[Id], [t0].[Id0], [t0].[Id1], [t0].[Id2], [l11].[Id], [l12].[Id], [l13].[Id], [l14].[Id], [t1].[Id], [t1].[Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Optional_Self_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToMany_Required_Self_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[OneToOne_Optional_Self2Id], [l14].[Name]
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
) AS [t] ON [l2].[Id] = [t].[Id2]
LEFT JOIN (
    SELECT [l7].[Id], [l8].[Id] AS [Id0], [l9].[Id] AS [Id1], [l10].[Id] AS [Id2], [l10].[Level2_Optional_Id] AS [Level2_Optional_Id0]
    FROM [LevelFour] AS [l7]
    INNER JOIN [LevelThree] AS [l8] ON [l7].[Level3_Required_Id] = [l8].[Id]
    INNER JOIN [LevelTwo] AS [l9] ON [l8].[Level2_Required_Id] = [l9].[Id]
    LEFT JOIN [LevelThree] AS [l10] ON [l9].[Id] = [l10].[OneToMany_Required_Inverse3Id]
) AS [t0] ON [t].[Id2] = [t0].[Id2]
LEFT JOIN [LevelThree] AS [l11] ON [l2].[OneToMany_Optional_Inverse4Id] = [l11].[Id]
LEFT JOIN [LevelThree] AS [l12] ON [t].[Id2] = [l12].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l13] ON [t0].[Level2_Optional_Id0] = [l13].[Id]
LEFT JOIN [LevelThree] AS [l14] ON [l13].[Id] = [l14].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l15].[Id], [l15].[Date], [l15].[Level1_Optional_Id], [l15].[Level1_Required_Id], [l15].[Name], [l15].[OneToMany_Optional_Inverse2Id], [l15].[OneToMany_Optional_Self_Inverse2Id], [l15].[OneToMany_Required_Inverse2Id], [l15].[OneToMany_Required_Self_Inverse2Id], [l15].[OneToOne_Optional_PK_Inverse2Id], [l15].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l15]
    WHERE [l15].[Id] <> 42
) AS [t1] ON [t].[Id2] = [t1].[OneToMany_Optional_Self_Inverse2Id]
WHERE ([l11].[Name] <> N'Foo') OR [l11].[Name] IS NULL
ORDER BY [l12].[Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [t].[Id], [t].[Id0], [t].[Id1], [t].[Id2], [t0].[Id], [t0].[Id0], [t0].[Id1], [t0].[Id2], [l11].[Id], [l13].[Id], [l14].[Id], [t1].[Id]");
        }

        public override async Task Project_collection_navigation(bool async)
        {
            await base.Project_collection_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]");
        }

        public override async Task Project_collection_navigation_nested(bool async)
        {
            await base.Project_collection_navigation_nested(async);

            AssertSql(
                @"SELECT [l].[Id], [l0].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Project_collection_navigation_using_ef_property(bool async)
        {
            await base.Project_collection_navigation_using_ef_property(async);

            AssertSql(
                @"SELECT [l].[Id], [l0].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Project_collection_navigation_nested_anonymous(bool async)
        {
            await base.Project_collection_navigation_nested_anonymous(async);

            AssertSql(
                @"SELECT [l].[Id], [l0].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Project_collection_navigation_composed(bool async)
        {
            await base.Project_collection_navigation_composed(async);

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
WHERE [l].[Id] < 3
ORDER BY [l].[Id], [t].[Id]");
        }

        public override async Task Project_collection_and_root_entity(bool async)
        {
            await base.Project_collection_and_root_entity(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]");
        }

        public override async Task Project_collection_and_include(bool async)
        {
            await base.Project_collection_and_include(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Project_navigation_and_collection(bool async)
        {
            await base.Project_navigation_and_collection(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Include_inside_subquery(bool async)
        {
            await base.Include_inside_subquery(async);

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
    WHERE [l0].[Id] > 0
) AS [t]
WHERE [l].[Id] < 3
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Include_collection_with_multiple_orderbys_member(bool async)
        {
            await base.Include_collection_with_multiple_orderbys_member(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Name], [l].[Level1_Required_Id], [l].[Id], [l0].[Id]");
        }

        public override async Task Include_collection_with_multiple_orderbys_property(bool async)
        {
            await base.Include_collection_with_multiple_orderbys_property(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Level1_Required_Id], [l].[Name], [l].[Id], [l0].[Id]");
        }

        public override async Task Include_collection_with_multiple_orderbys_methodcall(bool async)
        {
            await base.Include_collection_with_multiple_orderbys_methodcall(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY ABS([l].[Level1_Required_Id]), [l].[Name], [l].[Id], [l0].[Id]");
        }

        public override async Task Include_collection_with_multiple_orderbys_complex(bool async)
        {
            await base.Include_collection_with_multiple_orderbys_complex(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY ABS([l].[Level1_Required_Id]) + 7, [l].[Name], [l].[Id], [l0].[Id]");
        }

        public override async Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
        {
            await base.Include_collection_with_multiple_orderbys_complex_repeated(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Name], [l].[OneToMany_Optional_Inverse2Id], [l].[OneToMany_Optional_Self_Inverse2Id], [l].[OneToMany_Required_Inverse2Id], [l].[OneToMany_Required_Self_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id], [l].[OneToOne_Optional_Self2Id], [l0].[Id], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id]
FROM [LevelTwo] AS [l]
LEFT JOIN [LevelThree] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse3Id]
ORDER BY -[l].[Level1_Required_Id], [l].[Name], [l].[Id], [l0].[Id]");
        }

        public override async Task Include_reference_collection_order_by_reference_navigation(bool async)
        {
            await base.Include_reference_collection_order_by_reference_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l0].[Id], [l].[Id], [l1].[Id]");
        }

        public override async Task Include_after_SelectMany_and_reference_navigation(bool async)
        {
            await base.Include_after_SelectMany_and_reference_navigation(async);

            AssertSql(
                @"SELECT [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Optional_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Required_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l1]
INNER JOIN [LevelTwo] AS [l1.OneToMany_Required1] ON [l1].[Id] = [l1.OneToMany_Required1].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2] ON [l1.OneToMany_Required1].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Optional_Id]
ORDER BY [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Id]",
                //
                @"SELECT [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Level3_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToOne_Optional_Self4Id]
FROM [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3]
INNER JOIN (
    SELECT DISTINCT [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Id]
    FROM [LevelOne] AS [l10]
    INNER JOIN [LevelTwo] AS [l1.OneToMany_Required10] ON [l10].[Id] = [l1.OneToMany_Required10].[OneToMany_Required_Inverse2Id]
    LEFT JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK20] ON [l1.OneToMany_Required10].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Level2_Optional_Id]
) AS [t] ON [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
        {
            await base.Include_after_multiple_SelectMany_and_reference_navigation(async);

            AssertSql(
                @"SELECT [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[Level3_Required_Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[Name], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l1]
INNER JOIN [LevelTwo] AS [l1.OneToMany_Required1] ON [l1].[Id] = [l1.OneToMany_Required1].[OneToMany_Required_Inverse2Id]
INNER JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToMany_Optional2] ON [l1.OneToMany_Required1].[Id] = [l1.OneToMany_Required1.OneToMany_Optional2].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3] ON [l1.OneToMany_Required1.OneToMany_Optional2].[Id] = [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[Level3_Required_Id]
ORDER BY [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3].[Id]",
                //
                @"SELECT [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[Level3_Required_Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[Name], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToOne_Optional_Self4Id]
FROM [LevelFour] AS [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4]
INNER JOIN (
    SELECT DISTINCT [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK30].[Id]
    FROM [LevelOne] AS [l10]
    INNER JOIN [LevelTwo] AS [l1.OneToMany_Required10] ON [l10].[Id] = [l1.OneToMany_Required10].[OneToMany_Required_Inverse2Id]
    INNER JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToMany_Optional20] ON [l1.OneToMany_Required10].[Id] = [l1.OneToMany_Required1.OneToMany_Optional20].[OneToMany_Optional_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK30] ON [l1.OneToMany_Required1.OneToMany_Optional20].[Id] = [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK30].[Level3_Required_Id]
) AS [t] ON [l1.OneToMany_Required1.OneToMany_Optional2.OneToOne_Required_FK3.OneToMany_Required_Self4].[OneToMany_Required_Self_Inverse4Id] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
        {
            await base.Include_after_SelectMany_and_multiple_reference_navigations(async);

            AssertSql(
                @"SELECT [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[Level3_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l1]
INNER JOIN [LevelTwo] AS [l1.OneToMany_Required1] ON [l1].[Id] = [l1.OneToMany_Required1].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2] ON [l1.OneToMany_Required1].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3] ON [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[Level3_Required_Id]
ORDER BY [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3].[Id]",
                //
                @"SELECT [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[Level3_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToOne_Optional_Self4Id]
FROM [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4]
INNER JOIN (
    SELECT DISTINCT [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK30].[Id]
    FROM [LevelOne] AS [l10]
    INNER JOIN [LevelTwo] AS [l1.OneToMany_Required10] ON [l10].[Id] = [l1.OneToMany_Required10].[OneToMany_Required_Inverse2Id]
    LEFT JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK20] ON [l1.OneToMany_Required10].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Level2_Optional_Id]
    LEFT JOIN [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK30] ON [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK30].[Level3_Required_Id]
) AS [t] ON [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4].[OneToMany_Optional_Self_Inverse4Id] = [t].[Id]
ORDER BY [t].[Id]");
        }

        public override async Task Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct(bool async)
        {
            await base.Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct(async);

            AssertSql(
                @"SELECT [l1].[Id], [l1].[Date], [l1].[Name], [l1].[OneToMany_Optional_Self_Inverse1Id], [l1].[OneToMany_Required_Self_Inverse1Id], [l1].[OneToOne_Optional_Self1Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Optional_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Optional_Self_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Required_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToMany_Required_Self_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToOne_Optional_PK_Inverse3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2].[OneToOne_Optional_Self3Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Level3_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l1]
INNER JOIN [LevelTwo] AS [l1.OneToMany_Required1] ON [l1].[Id] = [l1.OneToMany_Required1].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2] ON [l1.OneToMany_Required1].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Level2_Optional_Id]
INNER JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK20] ON [l1.OneToMany_Required1].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3] ON [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional3].[OneToMany_Optional_Inverse4Id]
ORDER BY [l1.OneToMany_Required1.OneToOne_Optional_FK2].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK20].[Id]",
                //
                @"SELECT [l11].[Id], [l11].[Date], [l11].[Name], [l11].[OneToMany_Optional_Self_Inverse1Id], [l11].[OneToMany_Required_Self_Inverse1Id], [l11].[OneToOne_Optional_Self1Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[Level3_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToOne_Optional_Self4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK23].[Id]
FROM [LevelOne] AS [l11]
INNER JOIN [LevelTwo] AS [l1.OneToMany_Required11] ON [l11].[Id] = [l1.OneToMany_Required11].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK23] ON [l1.OneToMany_Required11].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK23].[Level2_Optional_Id]
INNER JOIN [LevelThree] AS [l1.OneToMany_Required1.OneToOne_Optional_FK24] ON [l1.OneToMany_Required11].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK24].[Level2_Optional_Id]
LEFT JOIN [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32] ON [l1.OneToMany_Required1.OneToOne_Optional_FK24].[Id] = [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional32].[OneToMany_Optional_Inverse4Id]
ORDER BY [l1.OneToMany_Required1.OneToOne_Optional_FK24].[Id]",
                //
                @"SELECT [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[Level3_Optional_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[Level3_Required_Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[Name], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[OneToMany_Optional_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[OneToMany_Optional_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[OneToMany_Required_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[OneToMany_Required_Self_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[OneToOne_Optional_PK_Inverse4Id], [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30].[OneToOne_Optional_Self4Id]
FROM [LevelFour] AS [l1.OneToMany_Required1.OneToOne_Optional_FK2.OneToMany_Optional30]");
        }

        public override async Task Null_check_in_anonymous_type_projection_should_not_be_removed(bool async)
        {
            await base.Null_check_in_anonymous_type_projection_should_not_be_removed(async);

            AssertSql(
                @"SELECT [l].[Id], [t].[c], [t].[Name], [t].[Id], [t].[Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [l1].[Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [l1].[Name], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task Null_check_in_Dto_projection_should_not_be_removed(bool async)
        {
            await base.Null_check_in_Dto_projection_should_not_be_removed(async);

            AssertSql(
                @"SELECT [l].[Id], [t].[c], [t].[Name], [t].[Id], [t].[Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT CASE
        WHEN [l1].[Id] IS NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [c], [l1].[Name], [l0].[Id], [l1].[Id] AS [Id0], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Required_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override async Task SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
        {
            await base.SelectMany_navigation_property_followed_by_select_collection_navigation(async);

            AssertSql(
                @"SELECT [l0].[Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(bool async)
        {
            await base.Multiple_SelectMany_navigation_property_followed_by_select_collection_navigation(async);

            AssertSql(
                @"SELECT [l1].[Id], [l].[Id], [l0].[Id], [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
INNER JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[OneToMany_Optional_Inverse4Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override async Task SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(bool async)
        {
            await base.SelectMany_navigation_property_with_include_and_followed_by_select_collection_navigation(async);

            AssertSql(
                @"SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Level2_Optional_Id], [l2].[Level2_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse3Id], [l2].[OneToMany_Optional_Self_Inverse3Id], [l2].[OneToMany_Required_Inverse3Id], [l2].[OneToMany_Required_Self_Inverse3Id], [l2].[OneToOne_Optional_PK_Inverse3Id], [l2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
INNER JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Required_Inverse3Id]
LEFT JOIN [LevelThree] AS [l2] ON [l0].[Id] = [l2].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override void Include15()
        {
            base.Include15();

            AssertSql(
                @"");
        }

        public override void Include16()
        {
            base.Include16();

            AssertSql(
                @"");
        }

        public override void IncludeCollection1()
        {
            base.IncludeCollection1();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]");
        }

        public override void IncludeCollection2()
        {
            base.IncludeCollection2();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override void IncludeCollection3()
        {
            base.IncludeCollection3();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override void IncludeCollection4()
        {
            base.IncludeCollection4();

            AssertSql(
                @"SELECT [l].[Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id]");
        }

        public override void IncludeCollection5()
        {
            base.IncludeCollection5();

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0]");
        }

        public override void IncludeCollection6()
        {
            base.IncludeCollection6();

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id1], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name1], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t].[Id1]");
        }

        public override void IncludeCollection6_1()
        {
            base.IncludeCollection6_1();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id1], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name1], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t].[Id1]");
        }

        public override void IncludeCollection6_2()
        {
            base.IncludeCollection6_2();

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id1], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name1], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id], [t].[Id2], [t].[Level2_Optional_Id0], [t].[Level2_Required_Id0], [t].[Name2], [t].[OneToMany_Optional_Inverse3Id0], [t].[OneToMany_Optional_Self_Inverse3Id0], [t].[OneToMany_Required_Inverse3Id0], [t].[OneToMany_Required_Self_Inverse3Id0], [t].[OneToOne_Optional_PK_Inverse3Id0], [t].[OneToOne_Optional_Self3Id0], [t].[Id3], [t].[Level3_Optional_Id0], [t].[Level3_Required_Id0], [t].[Name3], [t].[OneToMany_Optional_Inverse4Id0], [t].[OneToMany_Optional_Self_Inverse4Id0], [t].[OneToMany_Required_Inverse4Id0], [t].[OneToMany_Required_Self_Inverse4Id0], [t].[OneToOne_Optional_PK_Inverse4Id0], [t].[OneToOne_Optional_Self4Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l3].[Id] AS [Id2], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Name] AS [Name2], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l3].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0], [l4].[Id] AS [Id3], [l4].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l4].[Level3_Required_Id] AS [Level3_Required_Id0], [l4].[Name] AS [Name3], [l4].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l4].[OneToMany_Optional_Self_Inverse4Id] AS [OneToMany_Optional_Self_Inverse4Id0], [l4].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l4].[OneToMany_Required_Self_Inverse4Id] AS [OneToMany_Required_Self_Inverse4Id0], [l4].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [l4].[OneToOne_Optional_Self4Id] AS [OneToOne_Optional_Self4Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
    LEFT JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[Level2_Optional_Id]
    LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[OneToMany_Optional_Inverse4Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t].[Id1], [t].[Id2], [t].[Id3]");
        }

        public override void IncludeCollection6_3()
        {
            base.IncludeCollection6_3();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id1], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name1], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id], [t].[Id2], [t].[Level2_Optional_Id0], [t].[Level2_Required_Id0], [t].[Name2], [t].[OneToMany_Optional_Inverse3Id0], [t].[OneToMany_Optional_Self_Inverse3Id0], [t].[OneToMany_Required_Inverse3Id0], [t].[OneToMany_Required_Self_Inverse3Id0], [t].[OneToOne_Optional_PK_Inverse3Id0], [t].[OneToOne_Optional_Self3Id0], [t].[Id3], [t].[Level3_Optional_Id0], [t].[Level3_Required_Id0], [t].[Name3], [t].[OneToMany_Optional_Inverse4Id0], [t].[OneToMany_Optional_Self_Inverse4Id0], [t].[OneToMany_Required_Inverse4Id0], [t].[OneToMany_Required_Self_Inverse4Id0], [t].[OneToOne_Optional_PK_Inverse4Id0], [t].[OneToOne_Optional_Self4Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l3].[Id] AS [Id2], [l3].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l3].[Level2_Required_Id] AS [Level2_Required_Id0], [l3].[Name] AS [Name2], [l3].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l3].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l3].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l3].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l3].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l3].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0], [l4].[Id] AS [Id3], [l4].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l4].[Level3_Required_Id] AS [Level3_Required_Id0], [l4].[Name] AS [Name3], [l4].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l4].[OneToMany_Optional_Self_Inverse4Id] AS [OneToMany_Optional_Self_Inverse4Id0], [l4].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l4].[OneToMany_Required_Self_Inverse4Id] AS [OneToMany_Required_Self_Inverse4Id0], [l4].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [l4].[OneToOne_Optional_Self4Id] AS [OneToOne_Optional_Self4Id0]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
    LEFT JOIN [LevelThree] AS [l3] ON [l0].[Id] = [l3].[Level2_Optional_Id]
    LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[OneToMany_Optional_Inverse4Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t].[Id1], [t].[Id2], [t].[Id3]");
        }

        public override void IncludeCollection6_4()
        {
            base.IncludeCollection6_4();

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id0], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id], [t].[Id1]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id] AS [Id0], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name] AS [Name0], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l0].[Id] AS [Id1], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l2] ON [l1].[Id] = [l2].[Level3_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id1], [t].[Id], [t].[Id0]");
        }

        public override void IncludeCollection7()
        {
            base.IncludeCollection7();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[OneToOne_Optional_PK_Inverse3Id]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
        }

        public override async Task IncludeCollection8(bool async)
        {
            await base.IncludeCollection8(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t].[Id1], [t].[Level3_Optional_Id], [t].[Level3_Required_Id], [t].[Name1], [t].[OneToMany_Optional_Inverse4Id], [t].[OneToMany_Optional_Self_Inverse4Id], [t].[OneToMany_Required_Inverse4Id], [t].[OneToMany_Required_Self_Inverse4Id], [t].[OneToOne_Optional_PK_Inverse4Id], [t].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [l3].[Id] AS [Id0], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name] AS [Name0], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id], [l4].[Id] AS [Id1], [l4].[Level3_Optional_Id], [l4].[Level3_Required_Id], [l4].[Name] AS [Name1], [l4].[OneToMany_Optional_Inverse4Id], [l4].[OneToMany_Optional_Self_Inverse4Id], [l4].[OneToMany_Required_Inverse4Id], [l4].[OneToMany_Required_Self_Inverse4Id], [l4].[OneToOne_Optional_PK_Inverse4Id], [l4].[OneToOne_Optional_Self4Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN [LevelThree] AS [l3] ON [l2].[Id] = [l3].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN [LevelFour] AS [l4] ON [l3].[Id] = [l4].[Level3_Optional_Id]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
WHERE (
    SELECT COUNT(*)
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToOne_Optional_PK_Inverse3Id]
    WHERE ([l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)) > 0
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t].[Id1]");
        }

        public override async Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
        {
            await base.Lift_projection_mapping_when_pushing_down_subquery(async);

            AssertSql(
                @"@__p_0='25'

SELECT [t].[Id], [t0].[Id], [l1].[Id], [t0].[c]
FROM (
    SELECT TOP(@__p_0) [l].[Id]
    FROM [LevelOne] AS [l]
) AS [t]
LEFT JOIN (
    SELECT [t1].[Id], [t1].[c], [t1].[OneToMany_Required_Inverse2Id]
    FROM (
        SELECT [l0].[Id], 1 AS [c], [l0].[OneToMany_Required_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Required_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t1]
    WHERE [t1].[row] <= 1
) AS [t0] ON [t].[Id] = [t0].[OneToMany_Required_Inverse2Id]
LEFT JOIN [LevelTwo] AS [l1] ON [t].[Id] = [l1].[OneToMany_Required_Inverse2Id]
ORDER BY [t].[Id], [t0].[Id], [l1].[Id]");
        }

        public override async Task Including_reference_navigation_and_projecting_collection_navigation(bool async)
        {
            await base.Including_reference_navigation_and_projecting_collection_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l2] ON [l].[Id] = [l2].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id]");
        }

        public override async Task LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(bool async)
        {
            await base.LeftJoin_with_Any_on_outer_source_and_projecting_collection_from_inner(async);

            AssertSql(
                @"SELECT CASE
    WHEN [l0].[Id] IS NULL THEN 0
    ELSE [l0].[Id]
END, [l].[Id], [l0].[Id], [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Required_Id]
LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Required_Inverse3Id]
WHERE [l].[Name] IN (N'L1 01', N'L1 02')
ORDER BY [l].[Id], [l0].[Id], [l1].[Id]");
        }

        public override async Task Select_subquery_single_nested_subquery(bool async)
        {
            await base.Select_subquery_single_nested_subquery(async);

            AssertSql(
                @"SELECT [l].[Id], [t0].[Id], [t1].[Id], [t0].[c]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id]
    FROM [LevelThree] AS [l1]
) AS [t1] ON [t0].[Id] = [t1].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [t0].[Id], [t1].[Id]");
        }

        public override async Task Select_subquery_single_nested_subquery2(bool async)
        {
            await base.Select_subquery_single_nested_subquery2(async);

            AssertSql(
                @"SELECT [l].[Id], [t2].[Id], [t2].[Id0], [t2].[Id1], [t2].[c]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [t0].[Id] AS [Id0], [t1].[Id] AS [Id1], [t0].[c], [l0].[OneToMany_Optional_Inverse2Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN (
        SELECT [t].[c], [t].[Id], [t].[OneToMany_Optional_Inverse3Id]
        FROM (
            SELECT 1 AS [c], [l1].[Id], [l1].[OneToMany_Optional_Inverse3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Id]) AS [row]
            FROM [LevelThree] AS [l1]
        ) AS [t]
        WHERE [t].[row] <= 1
    ) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[OneToMany_Optional_Inverse4Id]
        FROM [LevelFour] AS [l2]
    ) AS [t1] ON [t0].[Id] = [t1].[OneToMany_Optional_Inverse4Id]
) AS [t2] ON [l].[Id] = [t2].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t2].[Id], [t2].[Id0], [t2].[Id1]");
        }

        public override async Task Filtered_include_basic_Where(bool async)
        {
            await base.Filtered_include_basic_Where(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
    WHERE [l0].[Id] > 5
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Id]");
        }

        public override async Task Filtered_include_OrderBy(bool async)
        {
            await base.Filtered_include_OrderBy(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l0]
) AS [t] ON [l].[Id] = [t].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t].[Name], [t].[Id]");
        }

        public override async Task Filtered_ThenInclude_OrderBy(bool async)
        {
            await base.Filtered_ThenInclude_OrderBy(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t].[Id] AS [Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name] AS [Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
        FROM [LevelThree] AS [l1]
    ) AS [t] ON [l0].[Id] = [t].[OneToMany_Optional_Inverse3Id]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Id], [t0].[Name0], [t0].[Id0]");
        }

        public override async Task Filtered_include_ThenInclude_OrderBy(bool async)
        {
            await base.Filtered_include_ThenInclude_OrderBy(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t].[Id] AS [Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name] AS [Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
        FROM [LevelThree] AS [l1]
    ) AS [t] ON [l0].[Id] = [t].[OneToMany_Optional_Inverse3Id]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[Name], [t0].[Id], [t0].[Name0] DESC, [t0].[Id0]");
        }

        public override async Task Filtered_include_basic_OrderBy_Take(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Take(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name], [t0].[Id]");
        }

        public override async Task Filtered_include_basic_OrderBy_Skip(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Skip(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name], [t0].[Id]");
        }

        public override async Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
        {
            await base.Filtered_include_basic_OrderBy_Skip_Take(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name], [t0].[Id]");
        }

        public override void Filtered_include_Skip_without_OrderBy()
        {
            base.Filtered_include_Skip_without_OrderBy();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
    ) AS [t]
    WHERE 1 < [t].[row]
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Id]");
        }

        public override void Filtered_include_Take_without_OrderBy()
        {
            base.Filtered_include_Take_without_OrderBy();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[OneToMany_Optional_Inverse3Id], [t0].[Name], [t0].[Id]");
        }

        public override async Task Filtered_include_after_reference_navigation(bool async)
        {
            await base.Filtered_include_after_reference_navigation(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t0].[Id], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
LEFT JOIN [LevelTwo] AS [l0] ON [l].[Id] = [l0].[Level1_Optional_Id]
LEFT JOIN (
    SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Optional_Inverse3Id] ORDER BY [l1].[Name]) AS [row]
        FROM [LevelThree] AS [l1]
        WHERE ([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL
    ) AS [t]
    WHERE (1 < [t].[row]) AND ([t].[row] <= 4)
) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse3Id]
ORDER BY [l].[Id], [l0].[Id], [t0].[OneToMany_Optional_Inverse3Id], [t0].[Name], [t0].[Id]");
        }

        public override async Task Filtered_include_after_different_filtered_include_same_level(bool async)
        {
            await base.Filtered_include_after_different_filtered_include_same_level(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t1].[Id], [t1].[Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Optional_Self_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToMany_Required_Self_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Name]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 3
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Optional_Self_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToMany_Required_Self_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[OneToOne_Optional_Self2Id]
    FROM (
        SELECT [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse2Id] ORDER BY [l1].[Name] DESC) AS [row]
        FROM [LevelTwo] AS [l1]
        WHERE ([l1].[Name] <> N'Bar') OR [l1].[Name] IS NULL
    ) AS [t2]
    WHERE 1 < [t2].[row]
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Required_Inverse2Id]
ORDER BY [l].[Id], [t0].[OneToMany_Optional_Inverse2Id], [t0].[Name], [t0].[Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[Name] DESC, [t1].[Id]");
        }

        public override async Task Filtered_include_after_different_filtered_include_different_level(bool async)
        {
            await base.Filtered_include_after_different_filtered_include_different_level(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t2].[Id], [t2].[Date], [t2].[Level1_Optional_Id], [t2].[Level1_Required_Id], [t2].[Name], [t2].[OneToMany_Optional_Inverse2Id], [t2].[OneToMany_Optional_Self_Inverse2Id], [t2].[OneToMany_Required_Inverse2Id], [t2].[OneToMany_Required_Self_Inverse2Id], [t2].[OneToOne_Optional_PK_Inverse2Id], [t2].[OneToOne_Optional_Self2Id], [t2].[Id0], [t2].[Level2_Optional_Id], [t2].[Level2_Required_Id], [t2].[Name0], [t2].[OneToMany_Optional_Inverse3Id], [t2].[OneToMany_Optional_Self_Inverse3Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[OneToMany_Required_Self_Inverse3Id], [t2].[OneToOne_Optional_PK_Inverse3Id], [t2].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name] AS [Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
    FROM (
        SELECT TOP(3) [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l0]
        WHERE ([l].[Id] = [l0].[OneToMany_Optional_Inverse2Id]) AND (([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL)
        ORDER BY [l0].[Name]
    ) AS [t]
    LEFT JOIN (
        SELECT [t1].[Id], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id]
        FROM (
            SELECT [l1].[Id], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], ROW_NUMBER() OVER(PARTITION BY [l1].[OneToMany_Required_Inverse3Id] ORDER BY [l1].[Name] DESC) AS [row]
            FROM [LevelThree] AS [l1]
            WHERE ([l1].[Name] <> N'Bar') OR [l1].[Name] IS NULL
        ) AS [t1]
        WHERE 1 < [t1].[row]
    ) AS [t0] ON [t].[Id] = [t0].[OneToMany_Required_Inverse3Id]
) AS [t2]
ORDER BY [l].[Id], [t2].[Name], [t2].[Id], [t2].[OneToMany_Required_Inverse3Id], [t2].[Name0] DESC, [t2].[Id0]");
        }

        public override async Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
        {
            await base.Filtered_include_same_filter_set_on_same_navigation_twice(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Id1], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [t0].[Level2_Optional_Id0], [t0].[Level2_Required_Id0], [t0].[Name1], [t0].[OneToMany_Optional_Inverse3Id0], [t0].[OneToMany_Optional_Self_Inverse3Id0], [t0].[OneToMany_Required_Inverse3Id0], [t0].[OneToMany_Required_Self_Inverse3Id0], [t0].[OneToOne_Optional_PK_Inverse3Id0], [t0].[OneToOne_Optional_Self3Id0]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l1].[Id] AS [Id1], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l0].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l0].[Level2_Required_Id] AS [Level2_Required_Id0], [l0].[Name] AS [Name1], [l0].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l0].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l0].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l0].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l0].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l0].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0]
    FROM (
        SELECT TOP(2) [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l2]
        WHERE ([l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]) AND (([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL)
        ORDER BY [l2].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[Level2_Required_Id]
    LEFT JOIN [LevelThree] AS [l1] ON [t].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
) AS [t0]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0], [t0].[Id1]");
        }

        public override async Task
            Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
        {
            await base
                .Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Id1], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [t0].[Level2_Optional_Id0], [t0].[Level2_Required_Id0], [t0].[Name1], [t0].[OneToMany_Optional_Inverse3Id0], [t0].[OneToMany_Optional_Self_Inverse3Id0], [t0].[OneToMany_Required_Inverse3Id0], [t0].[OneToMany_Required_Self_Inverse3Id0], [t0].[OneToOne_Optional_PK_Inverse3Id0], [t0].[OneToOne_Optional_Self3Id0]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l1].[Id] AS [Id1], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id], [l0].[Level2_Optional_Id] AS [Level2_Optional_Id0], [l0].[Level2_Required_Id] AS [Level2_Required_Id0], [l0].[Name] AS [Name1], [l0].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id0], [l0].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id0], [l0].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id0], [l0].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id0], [l0].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id0], [l0].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id0]
    FROM (
        SELECT TOP(2) [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l2]
        WHERE ([l].[Id] = [l2].[OneToMany_Optional_Inverse2Id]) AND (([l2].[Name] <> N'Foo') OR [l2].[Name] IS NULL)
        ORDER BY [l2].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[Level2_Required_Id]
    LEFT JOIN [LevelThree] AS [l1] ON [t].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
) AS [t0]
ORDER BY [l].[Id], [t0].[Id], [t0].[Id0], [t0].[Id1]");
        }

        public override async Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
        {
            await base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t1].[Id], [t1].[Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Optional_Self_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToMany_Required_Self_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[OneToOne_Optional_Self2Id], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id], [t1].[Id1], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Name1], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Optional_Self_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToMany_Required_Self_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[OneToOne_Optional_Self4Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [l0].[Id] AS [Id0], [l0].[Level2_Optional_Id], [l0].[Level2_Required_Id], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Inverse3Id], [l0].[OneToMany_Optional_Self_Inverse3Id], [l0].[OneToMany_Required_Inverse3Id], [l0].[OneToMany_Required_Self_Inverse3Id], [l0].[OneToOne_Optional_PK_Inverse3Id], [l0].[OneToOne_Optional_Self3Id], [t0].[Id] AS [Id1], [t0].[Level3_Optional_Id], [t0].[Level3_Required_Id], [t0].[Name] AS [Name1], [t0].[OneToMany_Optional_Inverse4Id], [t0].[OneToMany_Optional_Self_Inverse4Id], [t0].[OneToMany_Required_Inverse4Id], [t0].[OneToMany_Required_Self_Inverse4Id], [t0].[OneToOne_Optional_PK_Inverse4Id], [t0].[OneToOne_Optional_Self4Id]
    FROM (
        SELECT TOP(1) [l1].[Id], [l1].[Date], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Optional_Inverse2Id], [l1].[OneToMany_Optional_Self_Inverse2Id], [l1].[OneToMany_Required_Inverse2Id], [l1].[OneToMany_Required_Self_Inverse2Id], [l1].[OneToOne_Optional_PK_Inverse2Id], [l1].[OneToOne_Optional_Self2Id]
        FROM [LevelTwo] AS [l1]
        WHERE ([l].[Id] = [l1].[OneToMany_Optional_Inverse2Id]) AND (([l1].[Name] <> N'Foo') OR [l1].[Name] IS NULL)
        ORDER BY [l1].[Id]
    ) AS [t]
    LEFT JOIN [LevelThree] AS [l0] ON [t].[Id] = [l0].[OneToOne_Optional_PK_Inverse3Id]
    LEFT JOIN (
        SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id]
        FROM [LevelFour] AS [l2]
        WHERE [l2].[Id] > 1
    ) AS [t0] ON [l0].[Id] = [t0].[OneToMany_Optional_Inverse4Id]
) AS [t1]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Id1]");
        }

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
        {
            await base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t1].[Id], [t1].[Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Optional_Self_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToMany_Required_Self_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[OneToOne_Optional_Self2Id], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id], [t1].[Id00], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Name00], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Optional_Self_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToMany_Required_Self_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[OneToOne_Optional_Self4Id], [t1].[Id1], [t1].[Level3_Optional_Id0], [t1].[Level3_Required_Id0], [t1].[Name1], [t1].[OneToMany_Optional_Inverse4Id0], [t1].[OneToMany_Optional_Self_Inverse4Id0], [t1].[OneToMany_Required_Inverse4Id0], [t1].[OneToMany_Required_Self_Inverse4Id0], [t1].[OneToOne_Optional_PK_Inverse4Id0], [t1].[OneToOne_Optional_Self4Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name] AS [Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [t0].[Id0] AS [Id00], [t0].[Level3_Optional_Id], [t0].[Level3_Required_Id], [t0].[Name0] AS [Name00], [t0].[OneToMany_Optional_Inverse4Id], [t0].[OneToMany_Optional_Self_Inverse4Id], [t0].[OneToMany_Required_Inverse4Id], [t0].[OneToMany_Required_Self_Inverse4Id], [t0].[OneToOne_Optional_PK_Inverse4Id], [t0].[OneToOne_Optional_Self4Id], [t0].[Id1], [t0].[Level3_Optional_Id0], [t0].[Level3_Required_Id0], [t0].[Name1], [t0].[OneToMany_Optional_Inverse4Id0], [t0].[OneToMany_Optional_Self_Inverse4Id0], [t0].[OneToMany_Required_Inverse4Id0], [t0].[OneToMany_Required_Self_Inverse4Id0], [t0].[OneToOne_Optional_PK_Inverse4Id0], [t0].[OneToOne_Optional_Self4Id0]
    FROM [LevelTwo] AS [l0]
    OUTER APPLY (
        SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [l1].[Id] AS [Id0], [l1].[Level3_Optional_Id], [l1].[Level3_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse4Id], [l1].[OneToMany_Optional_Self_Inverse4Id], [l1].[OneToMany_Required_Inverse4Id], [l1].[OneToMany_Required_Self_Inverse4Id], [l1].[OneToOne_Optional_PK_Inverse4Id], [l1].[OneToOne_Optional_Self4Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l2].[Level3_Required_Id] AS [Level3_Required_Id0], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l2].[OneToMany_Optional_Self_Inverse4Id] AS [OneToMany_Optional_Self_Inverse4Id0], [l2].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l2].[OneToMany_Required_Self_Inverse4Id] AS [OneToMany_Required_Self_Inverse4Id0], [l2].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [l2].[OneToOne_Optional_Self4Id] AS [OneToOne_Optional_Self4Id0]
        FROM (
            SELECT TOP(1) [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
            FROM [LevelThree] AS [l3]
            WHERE ([l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]) AND (([l3].[Name] <> N'Foo') OR [l3].[Name] IS NULL)
            ORDER BY [l3].[Id]
        ) AS [t]
        LEFT JOIN [LevelFour] AS [l1] ON [t].[Id] = [l1].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN [LevelFour] AS [l2] ON [t].[Id] = [l2].[OneToMany_Required_Inverse4Id]
    ) AS [t0]
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Id00], [t1].[Id1]");
        }

        public override async Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
        {
            await base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t1].[Id], [t1].[Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Optional_Self_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToMany_Required_Self_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[OneToOne_Optional_Self2Id], [t1].[Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id], [t1].[Id00], [t1].[Level3_Optional_Id], [t1].[Level3_Required_Id], [t1].[Name00], [t1].[OneToMany_Optional_Inverse4Id], [t1].[OneToMany_Optional_Self_Inverse4Id], [t1].[OneToMany_Required_Inverse4Id], [t1].[OneToMany_Required_Self_Inverse4Id], [t1].[OneToOne_Optional_PK_Inverse4Id], [t1].[OneToOne_Optional_Self4Id], [t1].[Id1], [t1].[Level3_Optional_Id0], [t1].[Level3_Required_Id0], [t1].[Name1], [t1].[OneToMany_Optional_Inverse4Id0], [t1].[OneToMany_Optional_Self_Inverse4Id0], [t1].[OneToMany_Required_Inverse4Id0], [t1].[OneToMany_Required_Self_Inverse4Id0], [t1].[OneToOne_Optional_PK_Inverse4Id0], [t1].[OneToOne_Optional_Self4Id0]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [t0].[Id] AS [Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name] AS [Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id], [t0].[Id0] AS [Id00], [t0].[Level3_Optional_Id], [t0].[Level3_Required_Id], [t0].[Name0] AS [Name00], [t0].[OneToMany_Optional_Inverse4Id], [t0].[OneToMany_Optional_Self_Inverse4Id], [t0].[OneToMany_Required_Inverse4Id], [t0].[OneToMany_Required_Self_Inverse4Id], [t0].[OneToOne_Optional_PK_Inverse4Id], [t0].[OneToOne_Optional_Self4Id], [t0].[Id1], [t0].[Level3_Optional_Id0], [t0].[Level3_Required_Id0], [t0].[Name1], [t0].[OneToMany_Optional_Inverse4Id0], [t0].[OneToMany_Optional_Self_Inverse4Id0], [t0].[OneToMany_Required_Inverse4Id0], [t0].[OneToMany_Required_Self_Inverse4Id0], [t0].[OneToOne_Optional_PK_Inverse4Id0], [t0].[OneToOne_Optional_Self4Id0]
    FROM [LevelTwo] AS [l0]
    OUTER APPLY (
        SELECT [t].[Id], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [l1].[Id] AS [Id0], [l1].[Level3_Optional_Id], [l1].[Level3_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse4Id], [l1].[OneToMany_Optional_Self_Inverse4Id], [l1].[OneToMany_Required_Inverse4Id], [l1].[OneToMany_Required_Self_Inverse4Id], [l1].[OneToOne_Optional_PK_Inverse4Id], [l1].[OneToOne_Optional_Self4Id], [l2].[Id] AS [Id1], [l2].[Level3_Optional_Id] AS [Level3_Optional_Id0], [l2].[Level3_Required_Id] AS [Level3_Required_Id0], [l2].[Name] AS [Name1], [l2].[OneToMany_Optional_Inverse4Id] AS [OneToMany_Optional_Inverse4Id0], [l2].[OneToMany_Optional_Self_Inverse4Id] AS [OneToMany_Optional_Self_Inverse4Id0], [l2].[OneToMany_Required_Inverse4Id] AS [OneToMany_Required_Inverse4Id0], [l2].[OneToMany_Required_Self_Inverse4Id] AS [OneToMany_Required_Self_Inverse4Id0], [l2].[OneToOne_Optional_PK_Inverse4Id] AS [OneToOne_Optional_PK_Inverse4Id0], [l2].[OneToOne_Optional_Self4Id] AS [OneToOne_Optional_Self4Id0]
        FROM (
            SELECT TOP(1) [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
            FROM [LevelThree] AS [l3]
            WHERE ([l0].[Id] = [l3].[OneToMany_Optional_Inverse3Id]) AND (([l3].[Name] <> N'Foo') OR [l3].[Name] IS NULL)
            ORDER BY [l3].[Id]
        ) AS [t]
        LEFT JOIN [LevelFour] AS [l1] ON [t].[Id] = [l1].[OneToMany_Optional_Inverse4Id]
        LEFT JOIN [LevelFour] AS [l2] ON [t].[Id] = [l2].[OneToMany_Required_Inverse4Id]
    ) AS [t0]
) AS [t1] ON [l].[Id] = [t1].[OneToMany_Optional_Inverse2Id]
ORDER BY [l].[Id], [t1].[Id], [t1].[Id0], [t1].[Id00], [t1].[Id1]");
        }

        public override void Filtered_include_variable_used_inside_filter()
        {
            base.Filtered_include_variable_used_inside_filter();

            AssertSql(
                @"@__prm_0='Foo' (Size = 4000)

SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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

        public override void Filtered_include_context_accessed_inside_filter()
        {
            base.Filtered_include_context_accessed_inside_filter();

            AssertSql(
                @"SELECT COUNT(*)
FROM [LevelOne] AS [l]",
                //
                @"@__p_0='True'

SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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

        public override void Filtered_include_context_accessed_inside_filter_correlated()
        {
            base.Filtered_include_context_accessed_inside_filter_correlated();

            AssertSql(
                @"SELECT [l].[Id], [l].[Date], [l].[Name], [l].[OneToMany_Optional_Self_Inverse1Id], [l].[OneToMany_Required_Self_Inverse1Id], [l].[OneToOne_Optional_Self1Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id]
FROM [LevelOne] AS [l]
LEFT JOIN (
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

            AssertSql(
                @"SELECT [l].[Id], [t].[Id], [t].[Date], [t].[Level1_Optional_Id], [t].[Level1_Required_Id], [t].[Name], [t].[OneToMany_Optional_Inverse2Id], [t].[OneToMany_Optional_Self_Inverse2Id], [t].[OneToMany_Required_Inverse2Id], [t].[OneToMany_Required_Self_Inverse2Id], [t].[OneToOne_Optional_PK_Inverse2Id], [t].[OneToOne_Optional_Self2Id], [t].[Id0], [t].[Level2_Optional_Id], [t].[Level2_Required_Id], [t].[Name0], [t].[OneToMany_Optional_Inverse3Id], [t].[OneToMany_Optional_Self_Inverse3Id], [t].[OneToMany_Required_Inverse3Id], [t].[OneToMany_Required_Self_Inverse3Id], [t].[OneToOne_Optional_PK_Inverse3Id], [t].[OneToOne_Optional_Self3Id], [t0].[Id], [t0].[Date], [t0].[Level1_Optional_Id], [t0].[Level1_Required_Id], [t0].[Name], [t0].[OneToMany_Optional_Inverse2Id], [t0].[OneToMany_Optional_Self_Inverse2Id], [t0].[OneToMany_Required_Inverse2Id], [t0].[OneToMany_Required_Self_Inverse2Id], [t0].[OneToOne_Optional_PK_Inverse2Id], [t0].[OneToOne_Optional_Self2Id], [t0].[Id0], [t0].[Level2_Optional_Id], [t0].[Level2_Required_Id], [t0].[Name0], [t0].[OneToMany_Optional_Inverse3Id], [t0].[OneToMany_Optional_Self_Inverse3Id], [t0].[OneToMany_Required_Inverse3Id], [t0].[OneToMany_Required_Self_Inverse3Id], [t0].[OneToOne_Optional_PK_Inverse3Id], [t0].[OneToOne_Optional_Self3Id]
FROM [LevelOne] AS [l]
OUTER APPLY (
    SELECT [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id], [l1].[Id] AS [Id0], [l1].[Level2_Optional_Id], [l1].[Level2_Required_Id], [l1].[Name] AS [Name0], [l1].[OneToMany_Optional_Inverse3Id], [l1].[OneToMany_Optional_Self_Inverse3Id], [l1].[OneToMany_Required_Inverse3Id], [l1].[OneToMany_Required_Self_Inverse3Id], [l1].[OneToOne_Optional_PK_Inverse3Id], [l1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l0]
    LEFT JOIN [LevelThree] AS [l1] ON [l0].[Id] = [l1].[OneToMany_Optional_Inverse3Id]
) AS [t]
OUTER APPLY (
    SELECT [l2].[Id], [l2].[Date], [l2].[Level1_Optional_Id], [l2].[Level1_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse2Id], [l2].[OneToMany_Optional_Self_Inverse2Id], [l2].[OneToMany_Required_Inverse2Id], [l2].[OneToMany_Required_Self_Inverse2Id], [l2].[OneToOne_Optional_PK_Inverse2Id], [l2].[OneToOne_Optional_Self2Id], [t1].[Id] AS [Id0], [t1].[Level2_Optional_Id], [t1].[Level2_Required_Id], [t1].[Name] AS [Name0], [t1].[OneToMany_Optional_Inverse3Id], [t1].[OneToMany_Optional_Self_Inverse3Id], [t1].[OneToMany_Required_Inverse3Id], [t1].[OneToMany_Required_Self_Inverse3Id], [t1].[OneToOne_Optional_PK_Inverse3Id], [t1].[OneToOne_Optional_Self3Id]
    FROM [LevelTwo] AS [l2]
    LEFT JOIN (
        SELECT [l3].[Id], [l3].[Level2_Optional_Id], [l3].[Level2_Required_Id], [l3].[Name], [l3].[OneToMany_Optional_Inverse3Id], [l3].[OneToMany_Optional_Self_Inverse3Id], [l3].[OneToMany_Required_Inverse3Id], [l3].[OneToMany_Required_Self_Inverse3Id], [l3].[OneToOne_Optional_PK_Inverse3Id], [l3].[OneToOne_Optional_Self3Id]
        FROM [LevelThree] AS [l3]
        WHERE [l3].[Id] <> [l].[Id]
    ) AS [t1] ON [l2].[Id] = [t1].[OneToMany_Optional_Inverse3Id]
) AS [t0]
ORDER BY [l].[Id], [t].[Id], [t].[Id0], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
        {
            await base.Complex_query_with_let_collection_projection_FirstOrDefault(async);

            AssertSql(
                @"SELECT [l].[Id], [t0].[Id], [t1].[Name], [t1].[Id], [t0].[c]
FROM [LevelOne] AS [l]
LEFT JOIN (
    SELECT [t].[c], [t].[Id], [t].[OneToMany_Optional_Inverse2Id]
    FROM (
        SELECT 1 AS [c], [l0].[Id], [l0].[OneToMany_Optional_Inverse2Id], ROW_NUMBER() OVER(PARTITION BY [l0].[OneToMany_Optional_Inverse2Id] ORDER BY [l0].[Id]) AS [row]
        FROM [LevelTwo] AS [l0]
        WHERE ([l0].[Name] <> N'Foo') OR [l0].[Name] IS NULL
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [l].[Id] = [t0].[OneToMany_Optional_Inverse2Id]
OUTER APPLY (
    SELECT [l1].[Name], [l1].[Id]
    FROM [LevelOne] AS [l1]
    WHERE EXISTS (
        SELECT 1
        FROM [LevelTwo] AS [l2]
        WHERE ([l1].[Id] = [l2].[OneToMany_Optional_Inverse2Id]) AND ([l2].[Id] = [t0].[Id]))
) AS [t1]
ORDER BY [l].[Id], [t0].[Id], [t1].[Id]");
        }

        public override async Task SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(bool async)
        {
            await base.SelectMany_DefaultIfEmpty_multiple_times_with_joins_projecting_a_collection(async);

            AssertSql(
                @"SELECT [l2].[Id], [l2].[Level3_Optional_Id], [l2].[Level3_Required_Id], [l2].[Name], [l2].[OneToMany_Optional_Inverse4Id], [l2].[OneToMany_Optional_Self_Inverse4Id], [l2].[OneToMany_Required_Inverse4Id], [l2].[OneToMany_Required_Self_Inverse4Id], [l2].[OneToOne_Optional_PK_Inverse4Id], [l2].[OneToOne_Optional_Self4Id], [l].[Id], [l0].[Id], [l1].[Id], [t].[Id], [t].[Id0], [t].[Id1], [t].[Id2], [t0].[Id], [t0].[Id0], [t0].[Id1], [t0].[Id2], [l11].[Id], [l12].[Id], [l13].[Id], [l14].[Id], [t1].[Id], [t1].[Date], [t1].[Level1_Optional_Id], [t1].[Level1_Required_Id], [t1].[Name], [t1].[OneToMany_Optional_Inverse2Id], [t1].[OneToMany_Optional_Self_Inverse2Id], [t1].[OneToMany_Required_Inverse2Id], [t1].[OneToMany_Required_Self_Inverse2Id], [t1].[OneToOne_Optional_PK_Inverse2Id], [t1].[OneToOne_Optional_Self2Id], [l14].[Name]
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
) AS [t] ON [l2].[Id] = [t].[Id2]
LEFT JOIN (
    SELECT [l7].[Id], [l8].[Id] AS [Id0], [l9].[Id] AS [Id1], [l10].[Id] AS [Id2], [l10].[Level2_Optional_Id] AS [Level2_Optional_Id0]
    FROM [LevelFour] AS [l7]
    INNER JOIN [LevelThree] AS [l8] ON [l7].[Level3_Required_Id] = [l8].[Id]
    INNER JOIN [LevelTwo] AS [l9] ON [l8].[Level2_Required_Id] = [l9].[Id]
    LEFT JOIN [LevelThree] AS [l10] ON [l9].[Id] = [l10].[OneToMany_Required_Inverse3Id]
) AS [t0] ON [t].[Id2] = [t0].[Id2]
LEFT JOIN [LevelThree] AS [l11] ON [l2].[OneToMany_Optional_Inverse4Id] = [l11].[Id]
LEFT JOIN [LevelThree] AS [l12] ON [t].[Id2] = [l12].[Level2_Optional_Id]
LEFT JOIN [LevelTwo] AS [l13] ON [t0].[Level2_Optional_Id0] = [l13].[Id]
LEFT JOIN [LevelThree] AS [l14] ON [l13].[Id] = [l14].[Level2_Required_Id]
LEFT JOIN (
    SELECT [l15].[Id], [l15].[Date], [l15].[Level1_Optional_Id], [l15].[Level1_Required_Id], [l15].[Name], [l15].[OneToMany_Optional_Inverse2Id], [l15].[OneToMany_Optional_Self_Inverse2Id], [l15].[OneToMany_Required_Inverse2Id], [l15].[OneToMany_Required_Self_Inverse2Id], [l15].[OneToOne_Optional_PK_Inverse2Id], [l15].[OneToOne_Optional_Self2Id]
    FROM [LevelTwo] AS [l15]
    WHERE [l15].[Id] <> 42
) AS [t1] ON [t].[Id2] = [t1].[OneToMany_Optional_Self_Inverse2Id]
WHERE ([l11].[Name] <> N'Foo') OR [l11].[Name] IS NULL
ORDER BY [l12].[Id], [l].[Id], [l0].[Id], [l1].[Id], [l2].[Id], [t].[Id], [t].[Id0], [t].[Id1], [t].[Id2], [t0].[Id], [t0].[Id0], [t0].[Id1], [t0].[Id2], [l11].[Id], [l13].[Id], [l14].[Id], [t1].[Id]");
        }

        public override async Task Take_Select_collection_Take(bool async)
        {
            await base.Take_Select_collection_Take(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Name], [t0].[Id], [t0].[Name], [t0].[Level1Id], [t0].[Level2Id], [t0].[Id0], [t0].[Date], [t0].[Name0], [t0].[OneToMany_Optional_Self_Inverse1Id], [t0].[OneToMany_Required_Self_Inverse1Id], [t0].[OneToOne_Optional_Self1Id]
FROM (
    SELECT TOP(@__p_0) [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
) AS [t]
OUTER APPLY (
    SELECT [t1].[Id], [t1].[Name], [t1].[OneToMany_Required_Inverse2Id] AS [Level1Id], [t1].[Level1_Required_Id] AS [Level2Id], [l0].[Id] AS [Id0], [l0].[Date], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT TOP(3) [l1].[Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Required_Inverse2Id]
        FROM [LevelTwo] AS [l1]
        WHERE [t].[Id] = [l1].[OneToMany_Required_Inverse2Id]
        ORDER BY [l1].[Id]
    ) AS [t1]
    INNER JOIN [LevelOne] AS [l0] ON [t1].[Level1_Required_Id] = [l0].[Id]
) AS [t0]
ORDER BY [t].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
        {
            await base.Skip_Take_Select_collection_Skip_Take(async);

            AssertSql(
                @"@__p_0='1'

SELECT [t].[Id], [t].[Name], [t0].[Id], [t0].[Name], [t0].[Level1Id], [t0].[Level2Id], [t0].[Id0], [t0].[Date], [t0].[Name0], [t0].[OneToMany_Optional_Self_Inverse1Id], [t0].[OneToMany_Required_Self_Inverse1Id], [t0].[OneToOne_Optional_Self1Id]
FROM (
    SELECT [l].[Id], [l].[Name]
    FROM [LevelOne] AS [l]
    ORDER BY [l].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [t]
OUTER APPLY (
    SELECT [t1].[Id], [t1].[Name], [t1].[OneToMany_Required_Inverse2Id] AS [Level1Id], [t1].[Level1_Required_Id] AS [Level2Id], [l0].[Id] AS [Id0], [l0].[Date], [l0].[Name] AS [Name0], [l0].[OneToMany_Optional_Self_Inverse1Id], [l0].[OneToMany_Required_Self_Inverse1Id], [l0].[OneToOne_Optional_Self1Id]
    FROM (
        SELECT [l1].[Id], [l1].[Level1_Required_Id], [l1].[Name], [l1].[OneToMany_Required_Inverse2Id]
        FROM [LevelTwo] AS [l1]
        WHERE [t].[Id] = [l1].[OneToMany_Required_Inverse2Id]
        ORDER BY [l1].[Id]
        OFFSET 1 ROWS FETCH NEXT 3 ROWS ONLY
    ) AS [t1]
    INNER JOIN [LevelOne] AS [l0] ON [t1].[Level1_Required_Id] = [l0].[Id]
) AS [t0]
ORDER BY [t].[Id], [t0].[Id], [t0].[Id0]");
        }

        public override async Task Projecting_collection_with_FirstOrDefault(bool async)
        {
            await base.Projecting_collection_with_FirstOrDefault(async);

            AssertSql(
                @"SELECT [t].[Id], [l0].[Id], [l0].[Date], [l0].[Level1_Optional_Id], [l0].[Level1_Required_Id], [l0].[Name], [l0].[OneToMany_Optional_Inverse2Id], [l0].[OneToMany_Optional_Self_Inverse2Id], [l0].[OneToMany_Required_Inverse2Id], [l0].[OneToMany_Required_Self_Inverse2Id], [l0].[OneToOne_Optional_PK_Inverse2Id], [l0].[OneToOne_Optional_Self2Id]
FROM (
    SELECT TOP(1) [l].[Id]
    FROM [LevelOne] AS [l]
    WHERE [l].[Id] = 1
) AS [t]
LEFT JOIN [LevelTwo] AS [l0] ON [t].[Id] = [l0].[OneToMany_Optional_Inverse2Id]
ORDER BY [t].[Id], [l0].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
