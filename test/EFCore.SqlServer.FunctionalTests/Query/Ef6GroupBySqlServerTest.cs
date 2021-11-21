// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query;

public class Ef6GroupBySqlServerTest : Ef6GroupByTestBase<Ef6GroupBySqlServerTest.Ef6GroupBySqlServerFixture>
{
    public Ef6GroupBySqlServerTest(Ef6GroupBySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task GroupBy_is_optimized_when_projecting_group_key(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_group_key(async);

        AssertSql(
            @"SELECT [a].[FirstName]
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // [Distinct1].[FirstName] AS [FirstName]
        // FROM ( SELECT DISTINCT
        // [Extent1].[FirstName] AS [FirstName]
        // FROM [dbo].[ArubaOwners] AS [Extent1]
        // )  AS [Distinct1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_group_count(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_group_count(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // [GroupBy1].[A1] AS [C1]
        // FROM ( SELECT
        // 	[Extent1].[FirstName] AS [K1],
        // 	COUNT(1) AS [A1]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	GROUP BY [Extent1].[FirstName]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_expression_containing_group_key(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_expression_containing_group_key(async);

        AssertSql(
            @"SELECT [a].[Id] * 2
FROM [ArubaOwner] AS [a]
GROUP BY [a].[Id]");

        // EF6 SQL:
        // @"SELECT
        // [Extent1].[Id] * 2 AS [C1]
        // FROM [dbo].[ArubaOwners] AS [Extent1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_aggregate_on_the_group(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_aggregate_on_the_group(async);

        AssertSql(
            @"SELECT MAX([a].[Id])
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // [GroupBy1].[A1] AS [C1]
        // FROM ( SELECT
        // 	[Extent1].[FirstName] AS [K1],
        // 	MAX([Extent1].[Id]) AS [A1]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	GROUP BY [Extent1].[FirstName]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_group_aggregate(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_group_aggregate(async);

        AssertSql(
            @"SELECT [a].[FirstName] AS [Key], MAX([a].[Id]) AS [Aggregate]
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // 1 AS [C1],
        // [GroupBy1].[K1] AS [FirstName],
        // [GroupBy1].[A1] AS [C2]
        // FROM ( SELECT
        // 	[Extent1].[FirstName] AS [K1],
        // 	MAX([Extent1].[Id]) AS [A1]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	GROUP BY [Extent1].[FirstName]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_multiple_group_aggregates(
        bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_anonymous_type_containing_group_key_and_multiple_group_aggregates(async);

        AssertSql(
            @"SELECT [a].[FirstName] AS [key1], MAX([a].[Id]) AS [max], MIN([a].[Id] + 2) AS [min]
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // 1 AS [C1],
        // [GroupBy1].[K1] AS [FirstName],
        // [GroupBy1].[A1] AS [C2],
        // [GroupBy1].[A2] AS [C3]
        // FROM ( SELECT
        // 	[Extent1].[K1] AS [K1],
        // 	MAX([Extent1].[A1_0]) AS [A1],
        // 	MIN([Extent1].[A2_0]) AS [A2]
        // 	FROM ( SELECT
        // 		[Extent1].[FirstName] AS [K1],
        // 		[Extent1].[Id] AS [A1_0],
        // 		[Extent1].[Id] + 2 AS [A2_0]
        // 		FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	)  AS [Extent1]
        // 	GROUP BY [K1]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_conditional_expression_containing_group_key(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_conditional_expression_containing_group_key(async);

        AssertSql(
            @"@__p_0='False'

SELECT CASE
    WHEN [a].[FirstName] IS NULL THEN N'is null'
    ELSE N'not null'
END AS [keyIsNull], @__p_0 AS [logicExpression]
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // 1 AS [C1],
        // CASE WHEN ([Distinct1].[FirstName] IS NULL) THEN N'is null' ELSE N'not null' END AS [C2],
        // CASE WHEN (((@p__linq__0 = 1) AND (@p__linq__1 = 1)) OR ((@p__linq__2 = 1) AND (@p__linq__3 = 1))) THEN cast(1 as bit) WHEN ( NOT (((@p__linq__0 = 1) AND (@p__linq__1 = 1)) OR ((@p__linq__2 = 1) AND (@p__linq__3 = 1)))) THEN cast(0 as bit) END AS [C3]
        // FROM ( SELECT DISTINCT
        // 	[Extent1].[FirstName] AS [FirstName]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // )  AS [Distinct1]";
    }

    public override async Task GroupBy_is_optimized_when_filerting_and_projecting_anonymous_type_with_group_key_and_function_aggregate(
        bool async)
    {
        await base.GroupBy_is_optimized_when_filerting_and_projecting_anonymous_type_with_group_key_and_function_aggregate(async);

        AssertSql(
            @"SELECT [a].[FirstName], AVG(CAST([a].[Id] AS float)) AS [AverageId]
FROM [ArubaOwner] AS [a]
WHERE [a].[Id] > 5
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // 1 AS [C1],
        // [GroupBy1].[K1] AS [FirstName],
        // [GroupBy1].[A1] AS [C2]
        // FROM ( SELECT
        // 	[Extent1].[FirstName] AS [K1],
        // 	AVG( CAST( [Extent1].[Id] AS float)) AS [A1]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	WHERE [Extent1].[Id] > 5
        // 	GROUP BY [Extent1].[FirstName]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_function_aggregate_with_expression(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_function_aggregate_with_expression(async);

        AssertSql(
            @"SELECT MAX([a].[Id] * 2)
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // [GroupBy1].[A1] AS [C1]
        // FROM ( SELECT
        // 	[Extent1].[K1] AS [K1],
        // 	MAX([Extent1].[A1_0]) AS [A1]
        // 	FROM ( SELECT
        // 		[Extent1].[FirstName] AS [K1],
        // 		[Extent1].[Id] * 2 AS [A1_0]
        // 		FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	)  AS [Extent1]
        // 	GROUP BY [K1]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_projecting_expression_with_multiple_function_aggregates(bool async)
    {
        await base.GroupBy_is_optimized_when_projecting_expression_with_multiple_function_aggregates(async);

        AssertSql(
            @"SELECT MAX([a].[Id]) - MIN([a].[Id]) AS [maxMinusMin]
FROM [ArubaOwner] AS [a]
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // 1 AS [C1],
        // [GroupBy1].[A1] - [GroupBy1].[A2] AS [C2]
        // FROM ( SELECT
        // 	[Extent1].[FirstName] AS [K1],
        // 	MAX([Extent1].[Id]) AS [A1],
        // 	MIN([Extent1].[Id]) AS [A2]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	GROUP BY [Extent1].[FirstName]
        // )  AS [GroupBy1]";
    }

    public override async Task GroupBy_is_optimized_when_grouping_by_row_and_projecting_column_of_the_key_row(bool async)
    {
        await base.GroupBy_is_optimized_when_grouping_by_row_and_projecting_column_of_the_key_row(async);

        AssertSql(
            @"SELECT [a].[FirstName]
FROM [ArubaOwner] AS [a]
WHERE [a].[Id] < 4
GROUP BY [a].[FirstName]");

        // EF6 SQL:
        // @"SELECT
        // [Distinct1].[FirstName] AS [FirstName]
        // FROM ( SELECT DISTINCT
        // 	[Extent1].[FirstName] AS [FirstName]
        // 	FROM [dbo].[ArubaOwners] AS [Extent1]
        // 	WHERE [Extent1].[Id] < 4
        // )  AS [Distinct1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_doesnt_produce_a_groupby_statement(bool async)
    {
        await base.Grouping_by_all_columns_doesnt_produce_a_groupby_statement(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [Extent1].[Id] AS [Id],
        //     [Extent1].[FirstName] AS [FirstName],
        //     [Extent1].[LastName] AS [LastName],
        //     [Extent1].[Alias] AS [Alias]
        //     FROM [dbo].[ArubaOwners] AS [Extent1]";
    }

    public override async Task Grouping_by_all_columns_with_aggregate_function_works_1(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_1(async);

        AssertSql(
            @"SELECT COUNT(*)
FROM [ArubaOwner] AS [a]
GROUP BY [a].[Id], [a].[FirstName], [a].[LastName], [a].[Alias]");

        // EF6 SQL:
        // @"SELECT
        //     (SELECT
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent2]
        //         WHERE [Extent1].[Id] = [Extent2].[Id]) AS [C1]
        //     FROM [dbo].[ArubaOwners] AS [Extent1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_2(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_2(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     (SELECT
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent2]
        //         WHERE [Extent1].[Id] = [Extent2].[Id]) AS [C1]
        //     FROM [dbo].[ArubaOwners] AS [Extent1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_3(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_3(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     (SELECT
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent2]
        //         WHERE [Extent1].[Id] = [Extent2].[Id]) AS [C1]
        //     FROM [dbo].[ArubaOwners] AS [Extent1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_4(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_4(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [GroupBy1].[K1] AS [Id],
        //     [GroupBy1].[A1] AS [C1]
        //     FROM ( SELECT
        //         [Extent1].[Id] AS [K1],
        //         [Extent1].[FirstName] AS [K2],
        //         [Extent1].[LastName] AS [K3],
        //         [Extent1].[Alias] AS [K4],
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent1]
        //         GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
        //     )  AS [GroupBy1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_5(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_5(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [GroupBy1].[K1] AS [Id],
        //     [GroupBy1].[A1] AS [C1]
        //     FROM ( SELECT
        //         [Extent1].[Id] AS [K1],
        //         [Extent1].[FirstName] AS [K2],
        //         [Extent1].[LastName] AS [K3],
        //         [Extent1].[Alias] AS [K4],
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent1]
        //         GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
        //     )  AS [GroupBy1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_6(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_6(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [GroupBy1].[K1] AS [Id],
        //     [GroupBy1].[K4] AS [Alias],
        //     [GroupBy1].[A1] AS [C1]
        //     FROM ( SELECT
        //         [Extent1].[Id] AS [K1],
        //         [Extent1].[FirstName] AS [K2],
        //         [Extent1].[LastName] AS [K3],
        //         [Extent1].[Alias] AS [K4],
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent1]
        //         GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
        //     )  AS [GroupBy1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_7(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_7(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     (SELECT
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent2]
        //         WHERE [Extent1].[Id] = [Extent2].[Id]) AS [C1]
        //     FROM [dbo].[ArubaOwners] AS [Extent1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_8(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_8(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [GroupBy1].[K1] AS [Id],
        //     [GroupBy1].[A1] AS [C1]
        //     FROM ( SELECT
        //         [Extent1].[Id] AS [K1],
        //         [Extent1].[FirstName] AS [K2],
        //         [Extent1].[LastName] AS [K3],
        //         [Extent1].[Alias] AS [K4],
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent1]
        //         GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
        //     )  AS [GroupBy1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_9(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_9(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [GroupBy1].[K1] AS [Id],
        //     [GroupBy1].[K4] AS [Alias],
        //     [GroupBy1].[A1] AS [C1]
        //     FROM ( SELECT
        //         [Extent1].[Id] AS [K1],
        //         [Extent1].[FirstName] AS [K2],
        //         [Extent1].[LastName] AS [K3],
        //         [Extent1].[Alias] AS [K4],
        //         COUNT(1) AS [A1]
        //         FROM [dbo].[ArubaOwners] AS [Extent1]
        //         GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
        //     )  AS [GroupBy1]";
    }

    [ConditionalTheory(Skip = "Issue #17653")]
    public override async Task Grouping_by_all_columns_with_aggregate_function_works_10(bool async)
    {
        await base.Grouping_by_all_columns_with_aggregate_function_works_10(async);

        AssertSql(
            @"");

        // EF6 SQL:
        // @"SELECT
        //     [GroupBy1].[K1] AS [Id],
        //     [GroupBy1].[A1] AS [C1],
        //     [GroupBy1].[A2] AS [C2]
        //     FROM ( SELECT
        //         [Extent1].[Id] AS [K1],
        //         [Extent1].[FirstName] AS [K2],
        //         [Extent1].[LastName] AS [K3],
        //         [Extent1].[Alias] AS [K4],
        //         SUM([Extent1].[Id]) AS [A1],
        //         COUNT(1) AS [A2]
        //         FROM [dbo].[ArubaOwners] AS [Extent1]
        //         GROUP BY [Extent1].[Id], [Extent1].[FirstName], [Extent1].[LastName], [Extent1].[Alias]
        //     )  AS [GroupBy1]";
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class Ef6GroupBySqlServerFixture : Ef6GroupByFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
