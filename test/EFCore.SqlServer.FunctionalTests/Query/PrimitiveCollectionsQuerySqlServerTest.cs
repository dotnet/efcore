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
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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
WHERE [p].[NullableInt] IS NULL OR [p].[NullableInt] = 999
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

        AssertSql(
            """
@__i_0='2'
@__j_1='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (@__i_0, @__j_1)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter(bool async)
    {
        await base.Inline_collection_Contains_with_constant_and_parameter(async);

        AssertSql(
            """
@__j_0='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, @__j_0)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_Contains_with_mixed_value_types(async);

        AssertSql(
            """
@__i_0='11'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (999, @__i_0, [p].[Id], [p].[Id] + [p].[Int])
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate(bool async)
    {
        await base.Inline_collection_Contains_as_Any_with_predicate(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All(bool async)
    {
        await base.Inline_collection_negated_Contains_as_All(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] NOT IN (2, 999)
""");
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
    FROM OPENJSON(@__ids_0) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > [p].[Id]) = 1
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
WHERE [p].[Int] IN (
    SELECT [i].[value]
    FROM OPENJSON(@__ints_0) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int(async);

        AssertSql(
            """
@__nullableInts_0='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (
    SELECT [n].[value]
    FROM OPENJSON(@__nullableInts_0) WITH ([value] int '$') AS [n]
)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);

        AssertSql(
            """
@__nullableInts_0='[null,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON(@__nullableInts_0) WITH ([value] int '$') AS [n]
    WHERE [n].[value] = [p].[NullableInt] OR ([n].[value] IS NULL AND [p].[NullableInt] IS NULL))
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
    FROM OPENJSON(@__strings_0) WITH ([value] nvarchar(max) '$') AS [s]
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
WHERE [p].[DateTime] IN (
    SELECT [d].[value]
    FROM OPENJSON(@__dateTimes_0) WITH ([value] datetime '$') AS [d]
)
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
WHERE [p].[Bool] IN (
    SELECT [b].[value]
    FROM OPENJSON(@__bools_0) WITH ([value] bit '$') AS [b]
)
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
WHERE [p].[Enum] IN (
    SELECT [e].[value]
    FROM OPENJSON(@__enums_0) WITH ([value] int '$') AS [e]
)
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (
    SELECT [i].[value]
    FROM OPENJSON(NULL) AS [i]
)
""");
    }

    public override async Task Column_collection_of_ints_Contains(bool async)
    {
        await base.Column_collection_of_ints_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 10 IN (
    SELECT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 10 IN (
    SELECT [n].[value]
    FROM OPENJSON([p].[NullableInts]) WITH ([value] int '$') AS [n]
)
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
    FROM OPENJSON([p].[NullableInts]) WITH ([value] int '$') AS [n]
    WHERE [n].[value] IS NULL)
""");
    }

    public override async Task Column_collection_of_strings_contains_null(bool async)
    {
        await base.Column_collection_of_strings_contains_null(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([p].[Strings]) WITH ([value] nvarchar(max) '$') AS [s]
    WHERE [s].[value] IS NULL)
""");
    }

    public override async Task Column_collection_of_bools_Contains(bool async)
    {
        await base.Column_collection_of_bools_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(1 AS bit) IN (
    SELECT [b].[value]
    FROM OPENJSON([p].[Bools]) WITH ([value] bit '$') AS [b]
)
""");
    }

    [ConditionalFact]
    public virtual async Task Json_representation_of_bool_array()
    {
        await using var context = CreateContext();

        Assert.Equal(
            "[true,false]",
            await context.Database.SqlQuery<string>($"SELECT [Bools] AS [Value] FROM [PrimitiveCollectionsEntity] WHERE [Id] = 1")
                .SingleAsync());
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
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]) = 2
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
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]) = 2
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
        SELECT [i].[key]
        FROM OPENJSON([p].[Ints]) AS [i]
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
WHERE 11 IN (
    SELECT TOP(2) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)
)
""");
    }

    public override async Task Column_collection_Skip_Take(bool async)
    {
        await base.Column_collection_Skip_Take(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)
    OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
)
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        await base.Column_collection_OrderByDescending_ElementAt(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    ORDER BY [i].[value] DESC
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 111
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
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i])
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

    public override async Task Column_collection_Join_parameter_collection(bool async)
    {
        await base.Column_collection_Join_parameter_collection(async);

        AssertSql(
            """
@__ints_0='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    INNER JOIN OPENJSON(@__ints_0) WITH ([value] int '$') AS [i0] ON [i].[value] = [i0].[value]) = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        await base.Inline_collection_Join_ordered_column_collection(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
    INNER JOIN OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i] ON [v].[Value] = [i].[value]) = 2
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
        SELECT [i].[value]
        FROM OPENJSON(@__ints_0) WITH ([value] int '$') AS [i]
        UNION ALL
        SELECT [i0].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i0]
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
        SELECT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
        UNION
        SELECT [i0].[value]
        FROM OPENJSON(@__ints_0) WITH ([value] int '$') AS [i0]
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
        SELECT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
        INTERSECT
        SELECT [v].[Value] AS [value]
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
        SELECT [i].[value] AS [Value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
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

    public override async Task Column_collection_equality_inline_collection_with_parameters(bool async)
    {
        await base.Column_collection_equality_inline_collection_with_parameters(async);

        AssertSql();
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
            SELECT CAST([i].[value] AS int) AS [value], [i].[key]
            FROM OPENJSON(@__ints) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [t]
        UNION
        SELECT [i0].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i0]
    ) AS [t0]) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection(async);

        AssertSql(
"""
@__Skip_0='[111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[value]
        FROM OPENJSON(@__Skip_0) WITH ([value] int '$') AS [s]
        UNION
        SELECT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    ) AS [t]) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_nested(async);

        AssertSql(
"""
@__Skip_0='[111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[value]
        FROM OPENJSON(@__Skip_0) WITH ([value] int '$') AS [s]
        UNION
        SELECT [t1].[value]
        FROM (
            SELECT TOP(20) [t0].[value]
            FROM (
                SELECT DISTINCT [t2].[value]
                FROM (
                    SELECT CAST([i].[value] AS int) AS [value], [i].[key]
                    FROM OPENJSON([p].[Ints]) AS [i]
                    ORDER BY CAST([i].[value] AS int)
                    OFFSET 1 ROWS
                ) AS [t2]
            ) AS [t0]
            ORDER BY [t0].[value] DESC
        ) AS [t1]
    ) AS [t]) = 3
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

        // TODO: the subquery projection contains extra columns which we should remove
        AssertSql(
            """
@__ints='[10,111]' (Size = 4000)

SELECT COUNT(*)
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[value] AS int) AS [value], [i].[key], CAST([i].[key] AS int) AS [c], CAST([i].[value] AS int) AS [value0]
        FROM OPENJSON(@__ints) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [t]
    WHERE [t].[value0] > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async))).Message;

        Assert.Equal(RelationalStrings.SetOperationsRequireAtLeastOneSideWithValidTypeMapping("Union"), message);
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
        SELECT [t].[value]
        FROM (
            SELECT CAST([i].[value] AS int) AS [value], [i].[key]
            FROM OPENJSON([p].[Ints]) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [t]
        UNION
        SELECT [i0].[value]
        FROM OPENJSON(@__ints_0) WITH ([value] int '$') AS [i0]
    ) AS [t0]) = 3
""");
    }

    public override async Task Project_collection_of_ints_simple(bool async)
    {
        await base.Project_collection_of_ints_simple(async);

        AssertSql(
"""
SELECT [p].[Ints]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_collection_of_ints_ordered(bool async)
    {
        await base.Project_collection_of_ints_ordered(async);

        AssertSql(
"""
SELECT [p].[Id], CAST([i].[value] AS int) AS [value], [i].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY OPENJSON([p].[Ints]) AS [i]
ORDER BY [p].[Id], CAST([i].[value] AS int) DESC
""");
    }

    public override async Task Project_collection_of_datetimes_filtered(bool async)
    {
        await base.Project_collection_of_datetimes_filtered(async);

        AssertSql(
"""
SELECT [p].[Id], [t].[value], [t].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([d].[value] AS datetime2) AS [value], [d].[key], CAST([d].[key] AS int) AS [c]
    FROM OPENJSON([p].[DateTimes]) AS [d]
    WHERE DATEPART(day, CAST([d].[value] AS datetime2)) <> 1
) AS [t]
ORDER BY [p].[Id], [t].[c]
""");
    }

    public override async Task Project_collection_of_ints_with_paging(bool async)
    {
        await base.Project_collection_of_ints_with_paging(async);

        AssertSql(
"""
SELECT [p].[Id], [t].[value], [t].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT TOP(20) CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    ORDER BY CAST([n].[key] AS int)
) AS [t]
ORDER BY [p].[Id], [t].[c]
""");
    }

    public override async Task Project_collection_of_ints_with_paging2(bool async)
    {
        await base.Project_collection_of_ints_with_paging2(async);

        AssertSql(
"""
SELECT [p].[Id], [t].[value], [t].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    ORDER BY CAST([n].[value] AS int)
    OFFSET 1 ROWS
) AS [t]
ORDER BY [p].[Id], [t].[value]
""");
    }

    public override async Task Project_collection_of_ints_with_paging3(bool async)
    {
        await base.Project_collection_of_ints_with_paging3(async);

        AssertSql(
"""
SELECT [p].[Id], [t].[value], [t].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    ORDER BY CAST([n].[key] AS int)
    OFFSET 2 ROWS
) AS [t]
ORDER BY [p].[Id], [t].[c]
""");
    }

    public override async Task Project_collection_of_ints_with_distinct(bool async)
    {
        await base.Project_collection_of_ints_with_distinct(async);

        AssertSql(
"""
SELECT [p].[Id], [t].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT DISTINCT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
) AS [t]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_distinct(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_distinct(async);

        AssertSql("");
    }

    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
    {
        await base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(async);

        AssertSql(
"""
SELECT [p].[Id], [t].[value], [t].[key], [t0].[value], [t0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    WHERE 0 = 1
) AS [t]
OUTER APPLY (
    SELECT CAST([n0].[value] AS int) AS [value], [n0].[key], CAST([n0].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n0]
    WHERE [n0].[value] IS NULL
) AS [t0]
ORDER BY [p].[Id], [t].[c], [t].[key], [t0].[c]
""");
    }

    public override async Task Project_multiple_collections(bool async)
    {
        await base.Project_multiple_collections(async);

        AssertSql(
"""
SELECT [p].[Id], CAST([i].[value] AS int) AS [value], [i].[key], CAST([i0].[value] AS int) AS [value], [i0].[key], [t].[value], [t].[key], [t0].[value], [t0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY OPENJSON([p].[Ints]) AS [i]
OUTER APPLY OPENJSON([p].[Ints]) AS [i0]
OUTER APPLY (
    SELECT CAST([d].[value] AS datetime2) AS [value], [d].[key], CAST([d].[key] AS int) AS [c]
    FROM OPENJSON([p].[DateTimes]) AS [d]
    WHERE DATEPART(day, CAST([d].[value] AS datetime2)) <> 1
) AS [t]
OUTER APPLY (
    SELECT CAST([d0].[value] AS datetime2) AS [value], [d0].[key], CAST([d0].[key] AS int) AS [c]
    FROM OPENJSON([p].[DateTimes]) AS [d0]
    WHERE CAST([d0].[value] AS datetime2) > '2000-01-01T00:00:00.0000000'
) AS [t0]
ORDER BY [p].[Id], CAST([i].[key] AS int), [i].[key], CAST([i0].[value] AS int) DESC, [i0].[key], [t].[c], [t].[key], [t0].[c]
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
