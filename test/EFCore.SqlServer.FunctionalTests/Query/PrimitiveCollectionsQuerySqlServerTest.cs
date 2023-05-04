// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class PrimitiveCollectionsQuerySqlServerTest : PrimitiveCollectionsQueryTestBase<
    PrimitiveCollectionsQuerySqlServerTest.PrimitiveCollectionsQuerySqlServerFixture>
{
    public PrimitiveCollectionsQuerySqlServerTest(PrimitiveCollectionsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Inline_collection_of_ints_Contains(bool async)
    {
        await base.Inline_collection_of_ints_Contains(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] = 999 OR [p].[NullableInt] IS NULL
""");
    }

    public override Task Inline_collection_Count_with_zero_values(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Inline_collection_Count_with_zero_values(async),
            RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);

    public override async Task Inline_collection_Count_with_one_value(bool async)
    {
        await base.Inline_collection_Count_with_one_value(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int))) AS [v]([Value])
    WHERE [v].[Value] > [p].[Id]) = 1
""");
    }

    public override async Task Inline_collection_Count_with_two_values(bool async)
    {
        await base.Inline_collection_Count_with_two_values(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int)), (999)) AS [v]([Value])
    WHERE [v].[Value] > [p].[Id]) = 1
""");
    }

    public override async Task Inline_collection_Count_with_three_values(bool async)
    {
        await base.Inline_collection_Count_with_three_values(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int)), (999), (1000)) AS [v]([Value])
    WHERE [v].[Value] > [p].[Id]) = 2
""");
    }

    public override Task Inline_collection_Contains_with_zero_values(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Inline_collection_Contains_with_zero_values(async),
            RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);

    public override async Task Inline_collection_Contains_with_one_value(bool async)
    {
        await base.Inline_collection_Contains_with_one_value(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values(bool async)
    {
        await base.Inline_collection_Contains_with_two_values(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values(bool async)
    {
        await base.Inline_collection_Contains_with_three_values(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        await base.Inline_collection_Contains_with_all_parameters(async);

        // See #30732 for making this better

        AssertSql(
"""
@__p_0='[2,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__p_0) AS [p0]
    WHERE CAST([p0].[value] AS int) = [p].[Id])
""");
    }

    public override async Task Inline_collection_Contains_with_parameter_and_column_based_expression(bool async)
    {
        await base.Inline_collection_Contains_with_parameter_and_column_based_expression(async);

        AssertSql();
    }

    public override async Task Parameter_collection_Count(bool async)
    {
        await base.Parameter_collection_Count(async);

        AssertSql(
"""
@__ids_0='[2,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OpenJson(@__ids_0) AS [i]
    WHERE CAST([i].[value] AS int) > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains(bool async)
    {
        await base.Parameter_collection_of_ints_Contains(async);

        AssertSql(
"""
@__ints_0='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__ints_0) AS [i]
    WHERE CAST([i].[value] AS int) = [p].[Int])
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains(async);

        AssertSql(
"""
@__nullableInts_0='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__nullableInts_0) AS [n]
    WHERE CAST([n].[value] AS int) = [p].[NullableInt] OR ([n].[value] IS NULL AND [p].[NullableInt] IS NULL))
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
"""
@__nullableInts_0='[null,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__nullableInts_0) AS [n]
    WHERE CAST([n].[value] AS int) = [p].[NullableInt] OR ([n].[value] IS NULL AND [p].[NullableInt] IS NULL))
""");
    }

    public override async Task Parameter_collection_of_strings_Contains(bool async)
    {
        await base.Parameter_collection_of_strings_Contains(async);

        AssertSql(
"""
@__strings_0='["10","999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__strings_0) AS [s]
    WHERE [s].[value] = [p].[String] OR ([s].[value] IS NULL AND [p].[String] IS NULL))
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
"""
@__dateTimes_0='["2020-01-10T12:30:00Z","9999-01-01T00:00:00Z"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__dateTimes_0) AS [d]
    WHERE CAST([d].[value] AS datetime) = [p].[DateTime])
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
"""
@__bools_0='[true]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__bools_0) AS [b]
    WHERE CAST([b].[value] AS bit) = [p].[Bool])
""");
    }

    public override async Task Parameter_collection_of_enums_Contains(bool async)
    {
        await base.Parameter_collection_of_enums_Contains(async);

        AssertSql(
"""
@__enums_0='[0,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(@__enums_0) AS [e]
    WHERE CAST([e].[value] AS int) = [p].[Enum])
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson(N'[]') AS [i]
    WHERE CAST([i].[value] AS int) = [p].[Int])
""");
    }

    public override async Task Column_collection_of_ints_Contains(bool async)
    {
        await base.Column_collection_of_ints_Contains(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson([p].[Ints]) AS [i]
    WHERE CAST([i].[value] AS int) = 10)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson([p].[NullableInts]) AS [n]
    WHERE CAST([n].[value] AS int) = 10)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson([p].[NullableInts]) AS [n]
    WHERE [n].[value] IS NULL)
""");
    }

    public override async Task Column_collection_of_bools_Contains(bool async)
    {
        await base.Column_collection_of_bools_Contains(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson([p].[Bools]) AS [b]
    WHERE CAST([b].[value] AS bit) = CAST(1 AS bit))
""");
    }

    [ConditionalFact]
    public virtual async Task Json_representation_of_bool_array()
    {
        await using var context = CreateContext();

        Assert.Equal(
            "[true,false]",
            await context.Database.SqlQuery<string>($"SELECT [Bools] AS [Value] FROM [PrimitiveCollectionsEntity] WHERE [Id] = 1").SingleAsync());
    }

    public override async Task Column_collection_Count_method(bool async)
    {
        await base.Column_collection_Count_method(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OpenJson([p].[Ints]) AS [i]) = 2
""");
    }

    public override async Task Column_collection_Length(bool async)
    {
        await base.Column_collection_Length(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OpenJson([p].[Ints]) AS [i]) = 2
""");
    }

    public override async Task Column_collection_index_int(bool async)
    {
        await base.Column_collection_index_int(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[1]') AS int) = 10
""");
    }

    public override async Task Column_collection_index_string(bool async)
    {
        await base.Column_collection_index_string(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE JSON_VALUE([p].[Strings], '$[1]') = N'10'
""");
    }

    public override async Task Column_collection_index_datetime(bool async)
    {
        await base.Column_collection_index_datetime(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[DateTimes], '$[1]') AS datetime2) = '2020-01-10T12:30:00.0000000Z'
""");
    }

    public override async Task Column_collection_index_beyond_end(bool async)
    {
        await base.Column_collection_index_beyond_end(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[999]') AS int) = 10
""");
    }

    public override async Task Inline_collection_index_Column(bool async)
    {
        await base.Inline_collection_index_Column(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [v].[Value]
    FROM (VALUES (0, CAST(1 AS int)), (1, 2), (2, 3)) AS [v]([_ord], [Value])
    ORDER BY [v].[_ord]
    OFFSET [p].[Int] ROWS FETCH NEXT 1 ROWS ONLY) = 1
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        await base.Parameter_collection_index_Column_equal_Column(async);

        AssertSql(
"""
@__ints_0='[0,2,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(@__ints_0, '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = [p].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        await base.Parameter_collection_index_Column_equal_constant(async);

        AssertSql(
"""
@__ints_0='[1,2,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(@__ints_0, '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = 1
""");
    }

    public override async Task Column_collection_ElementAt(bool async)
    {
        await base.Column_collection_ElementAt(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[1]') AS int) = 10
""");
    }

    public override async Task Column_collection_Skip(bool async)
    {
        await base.Column_collection_Skip(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[key] AS int) AS [c]
        FROM OpenJson([p].[Ints]) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [t]) = 2
""");
    }

    public override async Task Column_collection_Take(bool async)
    {
        await base.Column_collection_Take(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(2) CAST([i].[value] AS int) AS [c], CAST([i].[key] AS int) AS [c0]
        FROM OpenJson([p].[Ints]) AS [i]
        ORDER BY CAST([i].[key] AS int)
    ) AS [t]
    WHERE [t].[c] = 11)
""");
    }

    public override async Task Column_collection_Skip_Take(bool async)
    {
        await base.Column_collection_Skip_Take(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT CAST([i].[value] AS int) AS [c], CAST([i].[key] AS int) AS [c0]
        FROM OpenJson([p].[Ints]) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [t]
    WHERE [t].[c] = 11)
""");
    }

    public override async Task Column_collection_Any(bool async)
    {
        await base.Column_collection_Any(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OpenJson([p].[Ints]) AS [i])
""");
    }

    public override async Task Column_collection_projection_from_top_level(bool async)
    {
        await base.Column_collection_projection_from_top_level(async);

        AssertSql(
"""
SELECT [p].[Ints]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override async Task Column_collection_and_parameter_collection_Join(bool async)
    {
        await base.Column_collection_and_parameter_collection_Join(async);

        AssertSql(
"""
@__ints_0='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OpenJson([p].[Ints]) AS [i]
    INNER JOIN OpenJson(@__ints_0) AS [i0] ON CAST([i].[value] AS int) = [i0].[value]) = 2
""");
    }

    public override async Task Parameter_collection_Concat_column_collection(bool async)
    {
        await base.Parameter_collection_Concat_column_collection(async);

        AssertSql(
"""
@__ints_0='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[value] AS int) AS [value]
        FROM OpenJson(@__ints_0) AS [i]
        UNION ALL
        SELECT CAST([i0].[value] AS int) AS [value]
        FROM OpenJson([p].[Ints]) AS [i0]
    ) AS [t]) = 2
""");
    }

    public override async Task Column_collection_Union_parameter_collection(bool async)
    {
        await base.Column_collection_Union_parameter_collection(async);

        AssertSql(
"""
@__ints_0='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[value] AS int) AS [c]
        FROM OpenJson([p].[Ints]) AS [i]
        UNION
        SELECT CAST([i0].[value] AS int) AS [c]
        FROM OpenJson(@__ints_0) AS [i0]
    ) AS [t]) = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection(bool async)
    {
        await base.Column_collection_Intersect_inline_collection(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[value] AS int) AS [c]
        FROM OpenJson([p].[Ints]) AS [i]
        INTERSECT
        SELECT [v].[Value] AS [c]
        FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
    ) AS [t]) = 2
""");
    }

    public override async Task Inline_collection_Except_column_collection(bool async)
    {
        await base.Inline_collection_Except_column_collection(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [v].[Value]
        FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
        EXCEPT
        SELECT CAST([i].[value] AS int) AS [Value]
        FROM OpenJson([p].[Ints]) AS [i]
    ) AS [t]
    WHERE [t].[Value] % 2 = 1) = 2
""");
    }

    public override async Task Column_collection_equality_parameter_collection(bool async)
    {
        await base.Column_collection_equality_parameter_collection(async);

        AssertSql(
"""
@__ints_0='[1,10]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = @__ints_0
""");
    }

    public override async Task Column_collection_Concat_parameter_collection_equality_inline_collection_not_supported(bool async)
    {
        await base.Column_collection_Concat_parameter_collection_equality_inline_collection_not_supported(async);

        AssertSql();
    }

    public override async Task Column_collection_equality_inline_collection(bool async)
    {
        await base.Column_collection_equality_inline_collection(async);

        AssertSql(
"""
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = N'[1,10]'
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async);

        AssertSql(
"""
@__ints='[10,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [t].[value]
        FROM (
            SELECT CAST([i].[value] AS int) AS [value]
            FROM OpenJson(@__ints) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [t]
        UNION
        SELECT CAST([i0].[value] AS int) AS [value]
        FROM OpenJson([p].[Ints]) AS [i0]
    ) AS [t0]) = 3
""");
    }

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        base.Parameter_collection_in_subquery_and_Convert_as_compiled_query();

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query(async);

        // TODO: the subquery projection contains two extra columns which we should remove
        AssertSql(
"""
@__ints='[10,111]' (Size = 4000)

SELECT COUNT(*)
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[value] AS int) AS [value], CAST([i].[key] AS int) AS [c], CAST([i].[value] AS int) AS [value0]
        FROM OpenJson(@__ints) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [t]
    WHERE [t].[value0] > [p].[Id]) = 1
""");
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        await base.Column_collection_in_subquery_Union_parameter_collection(async);

        AssertSql(
"""
@__ints_0='[10,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [t].[c]
        FROM (
            SELECT CAST([i].[value] AS int) AS [c]
            FROM OpenJson([p].[Ints]) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [t]
        UNION
        SELECT CAST([i0].[value] AS int) AS [c]
        FROM OpenJson(@__ints_0) AS [i0]
    ) AS [t0]) = 3
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private PrimitiveCollectionsContext CreateContext()
        => Fixture.CreateContext();

    public class PrimitiveCollectionsQuerySqlServerFixture : PrimitiveCollectionsQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // Map DateTime to non-default datetime instead of the default datetime2 to exercise type mapping inference
            modelBuilder.Entity<PrimitiveCollectionsEntity>().Property(p => p.DateTime).HasColumnType("datetime");
        }
    }
}
