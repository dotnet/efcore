// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

[SqlServerCondition(SqlServerCondition.SupportsFunctions2022 | SqlServerCondition.SupportsJsonType)]
public class PrimitiveCollectionsQuerySqlServerJsonTypeTest : PrimitiveCollectionsQueryRelationalTestBase<
    PrimitiveCollectionsQuerySqlServerJsonTypeTest.PrimitiveCollectionsQuerySqlServerFixture>
{
    public PrimitiveCollectionsQuerySqlServerJsonTypeTest(
        PrimitiveCollectionsQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Inline_collection_with_single_parameter_element_Contains(bool async)
    {
        await base.Inline_collection_with_single_parameter_element_Contains(async);

        AssertSql(
            """
@i='2'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] = @i
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Count(bool async)
    {
        await base.Inline_collection_with_single_parameter_element_Count(async);

        AssertSql(
            """
@i='2'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(@i AS int))) AS [v]([Value])
    WHERE [v].[Value] > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_Contains_with_EF_Constant(bool async)
    {
        await base.Parameter_collection_Contains_with_EF_Constant(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999, 1000)
""");
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any(bool async)
    {
        await base.Parameter_collection_Where_with_EF_Constant_Where_Any(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (VALUES (2), (999), (1000)) AS [i]([Value])
    WHERE [i].[Value] > 0)
""");
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_EF_Constant(bool async)
    {
        await base.Parameter_collection_Count_with_column_predicate_with_EF_Constant(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (2), (999), (1000)) AS [i]([Value])
    WHERE [i].[Value] > [p].[Id]) = 2
""");
    }

    public override async Task Inline_collection_of_ints_Contains(bool async)
    {
        await base.Inline_collection_of_ints_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override async Task Inline_collection_of_nullable_ints_Contains(bool async)
//     {
//         await base.Inline_collection_of_nullable_ints_Contains(async);
//
//         AssertSql(
//             """
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[NullableInt] IN (10, 999)
// """);
//     }
//
//     public override async Task Inline_collection_of_nullable_ints_Contains_null(bool async)
//     {
//         await base.Inline_collection_of_nullable_ints_Contains_null(async);
//
//         AssertSql(
//             """
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[NullableInt] IS NULL OR [p].[NullableInt] = 999
// """);
//     }

    public override async Task Inline_collection_Count_with_zero_values(bool async)
    {
        await base.Inline_collection_Count_with_zero_values(async);

        AssertSql();
    }

    public override async Task Inline_collection_Count_with_one_value(bool async)
    {
        await base.Inline_collection_Count_with_one_value(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int)), (999), (1000)) AS [v]([Value])
    WHERE [v].[Value] > [p].[Id]) = 2
""");
    }

    public override async Task Inline_collection_Contains_with_zero_values(bool async)
    {
        await base.Inline_collection_Contains_with_zero_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Inline_collection_Contains_with_one_value(bool async)
    {
        await base.Inline_collection_Contains_with_one_value(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values(bool async)
    {
        await base.Inline_collection_Contains_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values(bool async)
    {
        await base.Inline_collection_Contains_with_three_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        await base.Inline_collection_Contains_with_all_parameters(async);

        AssertSql(
            """
@i='2'
@j='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (@i, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter(bool async)
    {
        await base.Inline_collection_Contains_with_constant_and_parameter(async);

        AssertSql(
            """
@j='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_Contains_with_mixed_value_types(async);

        AssertSql(
            """
@i='11'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (999, @i, [p].[Id], [p].[Id] + [p].[Int])
""");
    }

    public override async Task Inline_collection_List_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_List_Contains_with_mixed_value_types(async);

        AssertSql(
            """
@i='11'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (999, @i, [p].[Id], [p].[Id] + [p].[Int])
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate(bool async)
    {
        await base.Inline_collection_Contains_as_Any_with_predicate(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All(bool async)
    {
        await base.Inline_collection_negated_Contains_as_All(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values(bool async)
    {
        await base.Inline_collection_Min_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE LEAST(30, [p].[Int]) = 30
""");
    }

    public override async Task Inline_collection_List_Min_with_two_values(bool async)
    {
        await base.Inline_collection_List_Min_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE LEAST(30, [p].[Int]) = 30
""");
    }

    public override async Task Inline_collection_Max_with_two_values(bool async)
    {
        await base.Inline_collection_Max_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE GREATEST(30, [p].[Int]) = 30
""");
    }

    public override async Task Inline_collection_List_Max_with_two_values(bool async)
    {
        await base.Inline_collection_List_Max_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE GREATEST(30, [p].[Int]) = 30
""");
    }

    public override async Task Inline_collection_Min_with_three_values(bool async)
    {
        await base.Inline_collection_Min_with_three_values(async);

        AssertSql(
            """
@i='25'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE LEAST(30, [p].[Int], @i) = 25
""");
    }

    public override async Task Inline_collection_List_Min_with_three_values(bool async)
    {
        await base.Inline_collection_List_Min_with_three_values(async);

        AssertSql(
            """
@i='25'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE LEAST(30, [p].[Int], @i) = 25
""");
    }

    public override async Task Inline_collection_Max_with_three_values(bool async)
    {
        await base.Inline_collection_Max_with_three_values(async);

        AssertSql(
            """
@i='35'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE GREATEST(30, [p].[Int], @i) = 35
""");
    }

    public override async Task Inline_collection_List_Max_with_three_values(bool async)
    {
        await base.Inline_collection_List_Max_with_three_values(async);

        AssertSql(
            """
@i='35'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE GREATEST(30, [p].[Int], @i) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Min(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_Min(async);

        AssertSql(
            """
@i='25' (Nullable = true)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE LEAST(30, [p].[Int], @i) = 25
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Max(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_Max(async);

        AssertSql(
            """
@i='35' (Nullable = true)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE GREATEST(30, [p].[Int], @i) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Min(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE LEAST(30, [p].[NullableInt], NULL) = 30
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Max(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Max(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE GREATEST(30, [p].[NullableInt], NULL) = 30
""");
    }

    public override async Task Parameter_collection_Count(bool async)
    {
        await base.Parameter_collection_Count(async);

        AssertSql(
            """
@ids='[2,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_int(async);

        AssertSql(
            """
@ints='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""",
            //
            """
@ints='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int(async);

        AssertSql(
            """
@ints='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""",
            //
            """
@ints='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int(async);

        AssertSql(
            """
@ints='[10,999]' (Nullable = false) (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""",
            //
            """
@ints='[10,999]' (Nullable = false) (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int(async);

        AssertSql(
            """
@ints='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""",
            //
            """
@ints='[10,999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] NOT IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
) OR [p].[NullableInt] IS NULL
""");
    }

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
//     {
//         await base.Parameter_collection_of_nullable_ints_Contains_int(async);
//
//         AssertSql(
//             """
// @nullableInts='[10,999]' (Size = 4000)
//
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[Int] IN (
//     SELECT [n].[value]
//     FROM OPENJSON(@nullableInts) WITH ([value] int '$') AS [n]
// )
// """,
//             //
//             """
// @nullableInts='[10,999]' (Size = 4000)
//
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[Int] NOT IN (
//     SELECT [n].[value]
//     FROM OPENJSON(@nullableInts) WITH ([value] int '$') AS [n]
// )
// """);
//     }
//
//     public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
//     {
//         await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);
//
//         AssertSql(
//             """
// @nullableInts_without_nulls='[999]' (Size = 4000)
//
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[NullableInt] IN (
//     SELECT [n].[value]
//     FROM OPENJSON(@nullableInts_without_nulls) AS [n]
// ) OR [p].[NullableInt] IS NULL
// """,
//             //
//             """
// @nullableInts_without_nulls='[999]' (Size = 4000)
//
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[NullableInt] NOT IN (
//     SELECT [n].[value]
//     FROM OPENJSON(@nullableInts_without_nulls) AS [n]
// ) AND [p].[NullableInt] IS NOT NULL
// """);
//     }

    public override async Task Parameter_collection_of_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_string(async);

        AssertSql(
            """
@strings='["10","999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
)
""",
            //
            """
@strings='["10","999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] NOT IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings='["10","999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
)
""",
            //
            """
@strings='["10","999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] NOT IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
) OR [p].[NullableString] IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string(async);

        AssertSql(
            """
@strings='["10",null]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
)
""",
            //
            """
@strings_without_nulls='["10"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] NOT IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings_without_nulls) AS [s]
)
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings_without_nulls='["999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings_without_nulls) AS [s]
) OR [p].[NullableString] IS NULL
""",
            //
            """
@strings_without_nulls='["999"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] NOT IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings_without_nulls) AS [s]
) AND [p].[NullableString] IS NOT NULL
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
            """
@dateTimes='["2020-01-10T12:30:00Z","9999-01-01T00:00:00Z"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[DateTime] IN (
    SELECT [d].[value]
    FROM OPENJSON(@dateTimes) WITH ([value] datetime '$') AS [d]
)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
            """
@bools='[true]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Bool] IN (
    SELECT [b].[value]
    FROM OPENJSON(@bools) WITH ([value] bit '$') AS [b]
)
""");
    }

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override async Task Parameter_collection_of_enums_Contains(bool async)
//     {
//         await base.Parameter_collection_of_enums_Contains(async);
//
//         AssertSql(
//             """
// @enums='[0,3]' (Size = 4000)
//
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE [p].[Enum] IN (
//     SELECT [e].[value]
//     FROM OPENJSON(@enums) WITH ([value] int '$') AS [e]
// )
// """);
//     }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 10 IN (
    SELECT [i].[value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
)
""");
    }

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override async Task Column_collection_of_nullable_ints_Contains(bool async)
//     {
//         await base.Column_collection_of_nullable_ints_Contains(async);
//
//         AssertSql(
//             """
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE 10 IN (
//     SELECT [n].[value]
//     FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) WITH ([value] int '$') AS [n]
// )
// """);
//     }
//
//     public override async Task Column_collection_of_nullable_ints_Contains_null(bool async)
//     {
//         await base.Column_collection_of_nullable_ints_Contains_null(async);
//
//         AssertSql(
//             """
// SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
// FROM [PrimitiveCollectionsEntity] AS [p]
// WHERE EXISTS (
//     SELECT 1
//     FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) WITH ([value] int '$') AS [n]
//     WHERE [n].[value] IS NULL)
// """);
//     }

    public override async Task Column_collection_of_strings_contains_null(bool async)
    {
        await base.Column_collection_of_strings_contains_null(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Column_collection_of_nullable_strings_contains_null(bool async)
    {
        await base.Column_collection_of_nullable_strings_contains_null(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON(CAST([p].[NullableStrings] AS nvarchar(max))) WITH ([value] nvarchar(max) '$') AS [n]
    WHERE [n].[value] IS NULL)
""");
    }

    public override async Task Column_collection_of_bools_Contains(bool async)
    {
        await base.Column_collection_of_bools_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(1 AS bit) IN (
    SELECT [b].[value]
    FROM OPENJSON(CAST([p].[Bools] AS nvarchar(max))) WITH ([value] bit '$') AS [b]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]) = 2
""");
    }

    public override async Task Column_collection_Length(bool async)
    {
        await base.Column_collection_Length(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]) = 2
""");
    }

    public override async Task Column_collection_Count_with_predicate(bool async)
    {
        await base.Column_collection_Count_with_predicate(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1) = 2
""");
    }

    public override async Task Column_collection_Where_Count(bool async)
    {
        await base.Column_collection_Where_Count(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1) = 2
""");
    }

    public override async Task Column_collection_index_int(bool async)
    {
        await base.Column_collection_index_int(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[1]') AS int) = 10
""");
    }

    public override async Task Column_collection_index_string(bool async)
    {
        await base.Column_collection_index_string(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE JSON_VALUE([p].[Strings], '$[1]') = N'10'
""");
    }

    public override async Task Column_collection_index_datetime(bool async)
    {
        await base.Column_collection_index_datetime(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[DateTimes], '$[1]') AS datetime2) = '2020-01-10T12:30:00.0000000Z'
""");
    }

    public override async Task Column_collection_index_beyond_end(bool async)
    {
        await base.Column_collection_index_beyond_end(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[999]') AS int) = 10
""");
    }

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        // TODO: This test is incorrect, see #33784
        await base.Nullable_reference_column_collection_index_equals_nullable_column(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE JSON_VALUE([p].[NullableStrings], '$[2]') = [p].[NullableString] OR (JSON_VALUE([p].[NullableStrings], '$[2]') IS NULL AND [p].[NullableString] IS NULL)
""");
    }

    public override async Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        await base.Non_nullable_reference_column_collection_index_equals_nullable_column(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON(CAST([p].[Strings] AS nvarchar(max))) AS [s]) AND JSON_VALUE([p].[Strings], '$[1]') = [p].[NullableString]
""");
    }

    public override async Task Inline_collection_index_Column(bool async)
    {
        await base.Inline_collection_index_Column(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [v].[Value]
    FROM (VALUES (0, CAST(1 AS int)), (1, 2), (2, 3)) AS [v]([_ord], [Value])
    ORDER BY [v].[_ord]
    OFFSET [p].[Int] ROWS FETCH NEXT 1 ROWS ONLY) = 1
""");
    }

    public override async Task Inline_collection_value_index_Column(bool async)
    {
        await base.Inline_collection_value_index_Column(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [v].[Value]
    FROM (VALUES (0, CAST(1 AS int)), (1, [p].[Int]), (2, 3)) AS [v]([_ord], [Value])
    ORDER BY [v].[_ord]
    OFFSET [p].[Int] ROWS FETCH NEXT 1 ROWS ONLY) = 1
""");
    }

    public override async Task Inline_collection_List_value_index_Column(bool async)
    {
        await base.Inline_collection_List_value_index_Column(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [v].[Value]
    FROM (VALUES (0, CAST(1 AS int)), (1, [p].[Int]), (2, 3)) AS [v]([_ord], [Value])
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
@ints='[0,2,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(@ints, '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = [p].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        await base.Parameter_collection_index_Column_equal_constant(async);

        AssertSql(
            """
@ints='[1,2,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(@ints, '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = 1
""");
    }

    public override async Task Column_collection_ElementAt(bool async)
    {
        await base.Column_collection_ElementAt(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[1]') AS int) = 10
""");
    }

    public override async Task Column_collection_First(bool async)
    {
        await base.Column_collection_First(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    ORDER BY CAST([i].[key] AS int)) = 1
""");
    }

    public override async Task Column_collection_FirstOrDefault(bool async)
    {
        await base.Column_collection_FirstOrDefault(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE COALESCE((
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    ORDER BY CAST([i].[key] AS int)), 0) = 1
""");
    }

    public override async Task Column_collection_Single(bool async)
    {
        await base.Column_collection_Single(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    ORDER BY CAST([i].[key] AS int)) = 1
""");
    }

    public override async Task Column_collection_SingleOrDefault(bool async)
    {
        await base.Column_collection_SingleOrDefault(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE COALESCE((
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    ORDER BY CAST([i].[key] AS int)), 0) = 1
""");
    }

    public override async Task Column_collection_Skip(bool async)
    {
        await base.Column_collection_Skip(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [i0]) = 2
""");
    }

    public override async Task Column_collection_Take(bool async)
    {
        await base.Column_collection_Take(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT TOP(2) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    ORDER BY CAST([i].[key] AS int)
)
""");
    }

    public override async Task Column_collection_Skip_Take(bool async)
    {
        await base.Column_collection_Skip_Take(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    ORDER BY CAST([i].[key] AS int)
    OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
)
""");
    }

    public override async Task Column_collection_Where_Skip(bool async)
    {
        await base.Column_collection_Where_Skip(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
        WHERE CAST([i].[value] AS int) > 1
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [i0]) = 3
""");
    }

    public override async Task Column_collection_Where_Take(bool async)
    {
        await base.Column_collection_Where_Take(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT TOP(2) 1 AS empty
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
        WHERE CAST([i].[value] AS int) > 1
        ORDER BY CAST([i].[key] AS int)
    ) AS [i0]) = 2
""");
    }

    public override async Task Column_collection_Where_Skip_Take(bool async)
    {
        await base.Column_collection_Where_Skip_Take(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
        WHERE CAST([i].[value] AS int) > 1
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [i0]) = 1
""");
    }

    public override async Task Column_collection_Contains_over_subquery(bool async)
    {
        await base.Column_collection_Contains_over_subquery(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT [i].[value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1
)
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        await base.Column_collection_OrderByDescending_ElementAt(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [i].[value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    ORDER BY [i].[value] DESC
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 111
""");
    }

    public override async Task Column_collection_Where_ElementAt(bool async)
    {
        await base.Column_collection_Where_ElementAt(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT CAST([i].[value] AS int) AS [value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
    WHERE CAST([i].[value] AS int) > 1
    ORDER BY CAST([i].[key] AS int)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 11
""");
    }

    public override async Task Column_collection_Any(bool async)
    {
        await base.Column_collection_Any(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i])
""");
    }

    public override async Task Column_collection_Distinct(bool async)
    {
        await base.Column_collection_Distinct(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [i].[value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    ) AS [i0]) = 3
""");
    }

    public override async Task Column_collection_SelectMany(bool async)
    {
        await base.Column_collection_SelectMany(async);

        AssertSql(
            """
SELECT [i].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
CROSS APPLY OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
""");
    }

    public override async Task Column_collection_SelectMany_with_filter(bool async)
    {
        await base.Column_collection_SelectMany_with_filter(async);

        AssertSql(
            """
SELECT [i0].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
CROSS APPLY (
    SELECT [i].[value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1
) AS [i0]
""");
    }

    public override async Task Column_collection_SelectMany_with_Select_to_anonymous_type(bool async)
    {
        await base.Column_collection_SelectMany_with_Select_to_anonymous_type(async);

        AssertSql(
            """
SELECT [i].[value] AS [Original], [i].[value] + 1 AS [Incremented]
FROM [PrimitiveCollectionsEntity] AS [p]
CROSS APPLY OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
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
@ints='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    INNER JOIN OPENJSON(@ints) WITH ([value] int '$') AS [i0] ON [i].[value] = [i0].[value]) = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        await base.Inline_collection_Join_ordered_column_collection(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
    INNER JOIN OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i] ON [v].[Value] = [i].[value]) = 2
""");
    }

    public override async Task Parameter_collection_Concat_column_collection(bool async)
    {
        await base.Parameter_collection_Concat_column_collection(async);

        AssertSql(
            """
@ints='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON(@ints) AS [i]
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i0]
    ) AS [u]) = 2
""");
    }

    public override async Task Parameter_collection_with_type_inference_for_JsonScalarExpression(bool async)
    {
        await base.Parameter_collection_with_type_inference_for_JsonScalarExpression(async);

        AssertSql(
            """
@values='["one","two"]' (Size = 4000)

SELECT CASE
    WHEN [p].[Id] <> 0 THEN JSON_VALUE(@values, '$[' + CAST([p].[Int] % 2 AS nvarchar(max)) + ']')
    ELSE N'foo'
END
FROM [PrimitiveCollectionsEntity] AS [p]
""");
    }

    public override async Task Column_collection_Union_parameter_collection(bool async)
    {
        await base.Column_collection_Union_parameter_collection(async);

        AssertSql(
            """
@ints='[11,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
        UNION
        SELECT [i0].[value]
        FROM OPENJSON(@ints) WITH ([value] int '$') AS [i0]
    ) AS [u]) = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection(bool async)
    {
        await base.Column_collection_Intersect_inline_collection(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
        INTERSECT
        SELECT [v].[Value] AS [value]
        FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
    ) AS [i0]) = 2
""");
    }

    public override async Task Inline_collection_Except_column_collection(bool async)
    {
        await base.Inline_collection_Except_column_collection(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [v].[Value]
        FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
        EXCEPT
        SELECT [i].[value] AS [Value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    ) AS [e]
    WHERE [e].[Value] % 2 = 1) = 2
""");
    }

    public override async Task Column_collection_Where_Union(bool async)
    {
        await base.Column_collection_Where_Union(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
        WHERE [i].[value] > 100
        UNION
        SELECT [v].[Value] AS [value]
        FROM (VALUES (CAST(50 AS int))) AS [v]([Value])
    ) AS [u]) = 2
""");
    }

    public override async Task Column_collection_equality_parameter_collection(bool async)
    {
        // TODO:SQLJSON Json type is not comparable
        Assert.Equal(
            "The JSON data type cannot be compared or sorted, except when using the IS NULL operator.",
            (await Assert.ThrowsAsync<SqlException>(() => base.Column_collection_equality_parameter_collection(async))).Message);

        AssertSql(
            """
@ints='[1,10]' (Size = 8000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = @ints
""");
    }

    public override async Task Column_collection_Concat_parameter_collection_equality_inline_collection(bool async)
    {
        await base.Column_collection_Concat_parameter_collection_equality_inline_collection(async);

        AssertSql();
    }

    public override async Task Column_collection_equality_inline_collection(bool async)
    {
        // TODO:SQLJSON Json type is not comparable
        Assert.Equal(
            "The data types json and varchar are incompatible in the equal to operator.",
            (await Assert.ThrowsAsync<SqlException>(() => base.Column_collection_equality_inline_collection(async))).Message);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = '[1,10]'
""");
    }

    public override async Task Column_collection_equality_inline_collection_with_parameters(bool async)
    {
        await base.Column_collection_equality_inline_collection_with_parameters(async);

        AssertSql();
    }

    public override async Task Column_collection_Where_equality_inline_collection(bool async)
    {
        await base.Column_collection_Where_equality_inline_collection(async);

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async);

        AssertSql(
            """
@ints='[10,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i1].[value]
        FROM (
            SELECT CAST([i].[value] AS int) AS [value]
            FROM OPENJSON(@ints) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [i1]
        UNION
        SELECT [i0].[value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i0]
    ) AS [u]) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection(async);

        AssertSql(
            """
@Skip='[111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[value]
        FROM OPENJSON(@Skip) WITH ([value] int '$') AS [s]
        UNION
        SELECT [i].[value]
        FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
    ) AS [u]) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_nested(async);

        AssertSql(
            """
@Skip='[111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[value]
        FROM OPENJSON(@Skip) WITH ([value] int '$') AS [s]
        UNION
        SELECT [i2].[value]
        FROM (
            SELECT TOP(20) [i1].[value]
            FROM (
                SELECT DISTINCT [i0].[value]
                FROM (
                    SELECT [i].[value]
                    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
                    ORDER BY [i].[value]
                    OFFSET 1 ROWS
                ) AS [i0]
            ) AS [i1]
            ORDER BY [i1].[value] DESC
        ) AS [i2]
    ) AS [u]) = 3
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
@ints='[10,111]' (Size = 4000)

SELECT COUNT(*)
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([i].[value] AS int) AS [value0]
        FROM OPENJSON(@ints) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [i0]
    WHERE [i0].[value0] > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async);

        AssertSql();
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        await base.Column_collection_in_subquery_Union_parameter_collection(async);

        AssertSql(
            """
@ints='[10,111]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i1].[value]
        FROM (
            SELECT CAST([i].[value] AS int) AS [value]
            FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [i1]
        UNION
        SELECT [i0].[value]
        FROM OPENJSON(@ints) WITH ([value] int '$') AS [i0]
    ) AS [u]) = 3
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
OUTER APPLY OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
ORDER BY [p].[Id], CAST([i].[value] AS int) DESC
""");
    }

    public override async Task Project_collection_of_datetimes_filtered(bool async)
    {
        await base.Project_collection_of_datetimes_filtered(async);

        AssertSql(
            """
SELECT [p].[Id], [d0].[value], [d0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([d].[value] AS datetime2) AS [value], [d].[key], CAST([d].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[DateTimes] AS nvarchar(max))) AS [d]
    WHERE DATEPART(day, CAST([d].[value] AS datetime2)) <> 1
) AS [d0]
ORDER BY [p].[Id], [d0].[c]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_paging(async);

        AssertSql(
            """
SELECT [p].[Id], [n0].[value], [n0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT TOP(20) CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) AS [n]
    ORDER BY CAST([n].[key] AS int)
) AS [n0]
ORDER BY [p].[Id], [n0].[c]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging2(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_paging2(async);

        AssertSql(
            """
SELECT [p].[Id], [n0].[value], [n0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key]
    FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) AS [n]
    ORDER BY CAST([n].[value] AS int)
    OFFSET 1 ROWS
) AS [n0]
ORDER BY [p].[Id], [n0].[value]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging3(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_paging3(async);

        AssertSql(
            """
SELECT [p].[Id], [n0].[value], [n0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) AS [n]
    ORDER BY CAST([n].[key] AS int)
    OFFSET 2 ROWS
) AS [n0]
ORDER BY [p].[Id], [n0].[c]
""");
    }

    public override async Task Project_collection_of_ints_with_distinct(bool async)
    {
        await base.Project_collection_of_ints_with_distinct(async);

        AssertSql(
            """
SELECT [p].[Id], [i0].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT DISTINCT [i].[value]
    FROM OPENJSON(CAST([p].[Ints] AS nvarchar(max))) WITH ([value] int '$') AS [i]
) AS [i0]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_distinct(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_distinct(async);

        AssertSql("");
    }

    public override async Task Project_collection_of_ints_with_ToList_and_FirstOrDefault(bool async)
    {
        await base.Project_collection_of_ints_with_ToList_and_FirstOrDefault(async);

        AssertSql(
            """
SELECT [p0].[Id], CAST([i].[value] AS int) AS [value], [i].[key]
FROM (
    SELECT TOP(1) [p].[Id], [p].[Ints]
    FROM [PrimitiveCollectionsEntity] AS [p]
    ORDER BY [p].[Id]
) AS [p0]
OUTER APPLY OPENJSON(CAST([p0].[Ints] AS nvarchar(max))) AS [i]
ORDER BY [p0].[Id], CAST([i].[key] AS int)
""");
    }

    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
    {
        await base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(async);

        AssertSql(
            """
SELECT [p].[Id], [n1].[value], [n1].[key], [n2].[value], [n2].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) AS [n]
    WHERE 0 = 1
) AS [n1]
OUTER APPLY (
    SELECT CAST([n0].[value] AS int) AS [value], [n0].[key], CAST([n0].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[NullableInts] AS nvarchar(max))) AS [n0]
    WHERE [n0].[value] IS NULL
) AS [n2]
ORDER BY [p].[Id], [n1].[c], [n1].[key], [n2].[c]
""");
    }

    public override async Task Project_multiple_collections(bool async)
    {
        await base.Project_multiple_collections(async);

        AssertSql(
            """
SELECT [p].[Id], CAST([i].[value] AS int) AS [value], [i].[key], CAST([i0].[value] AS int) AS [value], [i0].[key], [d1].[value], [d1].[key], [d2].[value], [d2].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i]
OUTER APPLY OPENJSON(CAST([p].[Ints] AS nvarchar(max))) AS [i0]
OUTER APPLY (
    SELECT CAST([d].[value] AS datetime2) AS [value], [d].[key], CAST([d].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[DateTimes] AS nvarchar(max))) AS [d]
    WHERE DATEPART(day, CAST([d].[value] AS datetime2)) <> 1
) AS [d1]
OUTER APPLY (
    SELECT CAST([d0].[value] AS datetime2) AS [value], [d0].[key], CAST([d0].[key] AS int) AS [c]
    FROM OPENJSON(CAST([p].[DateTimes] AS nvarchar(max))) AS [d0]
    WHERE CAST([d0].[value] AS datetime2) > '2000-01-01T00:00:00.0000000'
) AS [d2]
ORDER BY [p].[Id], CAST([i].[key] AS int), [i].[key], CAST([i0].[value] AS int) DESC, [i0].[key], [d1].[c], [d1].[key], [d2].[c]
""");
    }

    public override async Task Project_primitive_collections_element(bool async)
    {
        await base.Project_primitive_collections_element(async);

        AssertSql(
            """
SELECT CAST(JSON_VALUE([p].[Ints], '$[0]') AS int) AS [Indexer], CAST(JSON_VALUE([p].[DateTimes], '$[0]') AS datetime2) AS [EnumerableElementAt], JSON_VALUE([p].[Strings], '$[1]') AS [QueryableElementAt]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] < 4
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_inline_collection(bool async)
    {
        await base.Project_inline_collection(async);

        AssertSql(
            """
SELECT [p].[String]
FROM [PrimitiveCollectionsEntity] AS [p]
""");
    }

    public override async Task Project_inline_collection_with_Union(bool async)
    {
        await base.Project_inline_collection_with_Union(async);

        AssertSql(
            """
SELECT [p].[Id], [u].[Value]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT [v].[Value]
    FROM (VALUES ([p].[String])) AS [v]([Value])
    UNION
    SELECT [p0].[String] AS [Value]
    FROM [PrimitiveCollectionsEntity] AS [p0]
) AS [u]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_inline_collection_with_Concat(bool async)
    {
        await base.Project_inline_collection_with_Concat(async);

        AssertSql();
    }

    public override async Task Nested_contains_with_Lists_and_no_inferred_type_mapping(bool async)
    {
        await base.Nested_contains_with_Lists_and_no_inferred_type_mapping(async);

        AssertSql(
            """
@ints='[1,2,3]' (Size = 4000)
@strings='["one","two","three"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CASE
    WHEN [p].[Int] IN (
        SELECT [i].[value]
        FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
    ) THEN N'one'
    ELSE N'two'
END IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
)
""");
    }

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping(async);

        AssertSql(
            """
@ints='[1,2,3]' (Size = 4000)
@strings='["one","two","three"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[String], [p].[Strings]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CASE
    WHEN [p].[Int] IN (
        SELECT [i].[value]
        FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
    ) THEN N'one'
    ELSE N'two'
END IN (
    SELECT [s].[value]
    FROM OPENJSON(@strings) WITH ([value] nvarchar(max) '$') AS [s]
)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private PrimitiveCollectionsContext CreateContext()
        => Fixture.CreateContext();

    public class PrimitiveCollectionsQuerySqlServerFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override string StoreName
            => "PrimitiveCollectionsJsonTypeTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .UseSqlServer(b => b.UseCompatibilityLevel(160))
                .ConfigureWarnings(e => e.Log(SqlServerEventId.JsonTypeExperimental));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<PrimitiveCollectionsEntity>(
                b =>
                {
                    // Map DateTime to non-default datetime instead of the default datetime2 to exercise type mapping inference
                    b.Property(p => p.DateTime).HasColumnType("datetime");
                    b.PrimitiveCollection(e => e.Strings).HasColumnType("json");
                    b.PrimitiveCollection(e => e.Ints).HasColumnType("json");
                    b.PrimitiveCollection(e => e.DateTimes).HasColumnType("json");
                    b.PrimitiveCollection(e => e.Bools).HasColumnType("json");
                    b.PrimitiveCollection(e => e.Ints).HasColumnType("json");
                    b.PrimitiveCollection(e => e.Enums).HasColumnType("json");
                    b.PrimitiveCollection(e => e.NullableStrings).HasColumnType("json");
                    b.PrimitiveCollection(e => e.NullableInts).HasColumnType("json");
                });
        }
    }
}
