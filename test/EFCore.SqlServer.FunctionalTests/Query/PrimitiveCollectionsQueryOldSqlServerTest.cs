// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

/// <summary>
///     Runs all primitive collection tests with SQL Server compatibility level 120 (SQL Server 2014), which doesn't support OPENJSON.
///     This exercises the older translation paths for e.g. Contains, to make sure things work for providers with no queryable constant/
///     parameter support.
/// </summary>
public class PrimitiveCollectionsQueryOldSqlServerTest : PrimitiveCollectionsQueryRelationalTestBase<
    PrimitiveCollectionsQueryOldSqlServerTest.PrimitiveCollectionsQueryOldSqlServerFixture>
{
    public override int? NumberOfValuesForHugeParameterCollectionTests { get; } = 5000;

    public PrimitiveCollectionsQueryOldSqlServerTest(
        PrimitiveCollectionsQueryOldSqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IS NULL OR [p].[NullableInt] = 999
""");
    }

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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Inline_collection_Contains_with_one_value(bool async)
    {
        await base.Inline_collection_Contains_with_one_value(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values(bool async)
    {
        await base.Inline_collection_Contains_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values(bool async)
    {
        await base.Inline_collection_Contains_with_three_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (999, @i, [p].[Id], [p].[Id] + [p].[Int])
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate(bool async)
    {
        await base.Inline_collection_Contains_as_Any_with_predicate(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All(bool async)
    {
        await base.Inline_collection_negated_Contains_as_All(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values(bool async)
    {
        await base.Inline_collection_Min_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_List_Min_with_two_values(bool async)
    {
        await base.Inline_collection_List_Min_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_Max_with_two_values(bool async)
    {
        await base.Inline_collection_Max_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_List_Max_with_two_values(bool async)
    {
        await base.Inline_collection_List_Max_with_two_values(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_Min_with_three_values(bool async)
    {
        await base.Inline_collection_Min_with_three_values(async);

        AssertSql(
            """
@i='25'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int]), (@i)) AS [v]([Value])) = 25
""");
    }

    public override async Task Inline_collection_List_Min_with_three_values(bool async)
    {
        await base.Inline_collection_List_Min_with_three_values(async);

        AssertSql(
            """
@i='25'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int]), (@i)) AS [v]([Value])) = 25
""");
    }

    public override async Task Inline_collection_Max_with_three_values(bool async)
    {
        await base.Inline_collection_Max_with_three_values(async);

        AssertSql(
            """
@i='35'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int]), (@i)) AS [v]([Value])) = 35
""");
    }

    public override async Task Inline_collection_List_Max_with_three_values(bool async)
    {
        await base.Inline_collection_List_Max_with_three_values(async);

        AssertSql(
            """
@i='35'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int]), (@i)) AS [v]([Value])) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Min(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_Min(async);

        AssertSql(
            """
@i='25' (Nullable = true)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int]), (@i)) AS [v]([Value])) = 25
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Max(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_Max(async);

        AssertSql(
            """
@i='35' (Nullable = true)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int]), (@i)) AS [v]([Value])) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Min(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[NullableInt]), (NULL)) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Max(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Max(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[NullableInt]), (NULL)) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Contains(bool async)
    {
        await base.Inline_collection_with_single_parameter_element_Contains(async);

        AssertSql(
            """
@i='2'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(@i AS int))) AS [v]([Value])
    WHERE [v].[Value] > [p].[Id]) = 1
""");
    }

    public override Task Inline_collection_Contains_with_EF_Parameter(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Inline_collection_Contains_with_EF_Parameter(async));

    public override Task Inline_collection_Count_with_column_predicate_with_EF_Parameter(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Inline_collection_Count_with_column_predicate_with_EF_Parameter(async));

    public override async Task Parameter_collection_Count(bool async)
    {
        await base.Parameter_collection_Count(async);

        AssertSql(
            """
@ids1='2'
@ids2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (@ids1), (@ids2)) AS [i]([Value])
    WHERE [i].[Value] > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] NOT IN (@ints1, @ints2) OR [p].[NullableInt] IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int(async);

        AssertSql(
            """
@nullableInts1='10'
@nullableInts2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (@nullableInts1, @nullableInts2)
""",
            //
            """
@nullableInts1='10'
@nullableInts2='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (@nullableInts1, @nullableInts2)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);

        AssertSql(
            """
@nullableInts1='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IS NULL OR [p].[NullableInt] = @nullableInts1
""",
            //
            """
@nullableInts1='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IS NOT NULL AND [p].[NullableInt] <> @nullableInts1
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_string(async);

        AssertSql(
            """
@strings1='10' (Size = 4000)
@strings2='999' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] IN (@strings1, @strings2)
""",
            //
            """
@strings1='10' (Size = 4000)
@strings2='999' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] NOT IN (@strings1, @strings2)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings1='10' (Size = 4000)
@strings2='999' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] IN (@strings1, @strings2)
""",
            //
            """
@strings1='10' (Size = 4000)
@strings2='999' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] NOT IN (@strings1, @strings2) OR [p].[NullableString] IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string(async);

        AssertSql(
            """
@strings1='10' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] = @strings1
""",
            //
            """
@strings1='10' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[String] <> @strings1
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings1='999' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] IS NULL OR [p].[NullableString] = @strings1
""",
            //
            """
@strings1='999' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] IS NOT NULL AND [p].[NullableString] <> @strings1
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
            """
@dateTimes1='2020-01-10T12:30:00.0000000Z'
@dateTimes2='9999-01-01T00:00:00.0000000Z'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[DateTime] IN (@dateTimes1, @dateTimes2)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
            """
@bools1='True'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Bool] = @bools1
""");
    }

    public override async Task Parameter_collection_of_enums_Contains(bool async)
    {
        await base.Parameter_collection_of_enums_Contains(async);

        AssertSql(
            """
@enums1='0'
@enums2='3'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Enum] IN (@enums1, @enums2)
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Parameter_collection_Contains_with_EF_Constant(bool async)
    {
        await base.Parameter_collection_Contains_with_EF_Constant(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999, 1000)
""");
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any(bool async)
    {
        await base.Parameter_collection_Where_with_EF_Constant_Where_Any(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (2), (999), (1000)) AS [i]([Value])
    WHERE [i].[Value] > [p].[Id]) = 2
""");
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values(bool async)
    {
        await base.Parameter_collection_Count_with_huge_number_of_values(async);

        Assert.Contains("VALUES", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values(async);

        Assert.DoesNotContain("OPENJSON", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.DoesNotContain("OPENJSON", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override Task Column_collection_of_ints_Contains(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_of_ints_Contains(async));

    public override Task Column_collection_of_nullable_ints_Contains(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_of_nullable_ints_Contains(async));

    public override Task Column_collection_of_nullable_ints_Contains_null(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_of_nullable_ints_Contains_null(async));

    public override Task Column_collection_of_strings_contains_null(bool async)
        => AssertTranslationFailed(() => base.Column_collection_of_strings_contains_null(async));

    public override Task Column_collection_of_nullable_strings_contains_null(bool async)
        => AssertTranslationFailed(() => base.Column_collection_of_strings_contains_null(async));

    public override Task Column_collection_of_bools_Contains(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_of_bools_Contains(async));

    [ConditionalFact]
    public virtual async Task Json_representation_of_bool_array()
    {
        await using var context = CreateContext();

        Assert.Equal(
            "[true,false]",
            await context.Database.SqlQuery<string>($"SELECT [Bools] AS [Value] FROM [PrimitiveCollectionsEntity] WHERE [Id] = 1")
                .SingleAsync());
    }

    public override Task Column_collection_Count_method(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Count_method(async));

    public override Task Column_collection_Length(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Length(async));

    public override Task Column_collection_Count_with_predicate(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Count_with_predicate(async));

    public override Task Column_collection_Where_Count(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Where_Count(async));

    public override Task Column_collection_index_int(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_index_int(async));

    public override Task Column_collection_index_string(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_index_string(async));

    public override Task Column_collection_index_datetime(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_index_datetime(async));

    public override Task Column_collection_index_beyond_end(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_index_beyond_end(async));

    public override Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_index_beyond_end(async));

    public override Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_index_beyond_end(async));

    public override async Task Inline_collection_index_Column(bool async)
    {
        await base.Inline_collection_index_Column(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [v].[Value]
    FROM (VALUES (0, CAST(1 AS int)), (1, [p].[Int]), (2, 3)) AS [v]([_ord], [Value])
    ORDER BY [v].[_ord]
    OFFSET [p].[Int] ROWS FETCH NEXT 1 ROWS ONLY) = 1
""");
    }

    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        await base.Parameter_collection_index_Column_equal_Column(async);

        AssertSql(
            """
@ints1='0'
@ints2='2'
@ints3='3'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [i].[Value]
    FROM (VALUES (1, @ints1), (2, @ints2), (3, @ints3)) AS [i]([_ord], [Value])
    ORDER BY [i].[_ord]
    OFFSET [p].[Int] ROWS FETCH NEXT 1 ROWS ONLY) = [p].[Int]
""");
    }

    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        await base.Parameter_collection_index_Column_equal_constant(async);

        AssertSql(
            """
@ints1='1'
@ints2='2'
@ints3='3'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [i].[Value]
    FROM (VALUES (1, @ints1), (2, @ints2), (3, @ints3)) AS [i]([_ord], [Value])
    ORDER BY [i].[_ord]
    OFFSET [p].[Int] ROWS FETCH NEXT 1 ROWS ONLY) = 1
""");
    }

    public override Task Column_collection_ElementAt(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_ElementAt(async));

    public override Task Column_collection_First(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_First(async));

    public override Task Column_collection_FirstOrDefault(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_FirstOrDefault(async));

    public override Task Column_collection_Single(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Single(async));

    public override Task Column_collection_SingleOrDefault(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_SingleOrDefault(async));

    public override Task Column_collection_Skip(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Skip(async));

    public override Task Column_collection_Take(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Take(async));

    public override Task Column_collection_Skip_Take(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Skip_Take(async));

    public override Task Column_collection_Where_Skip(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Where_Skip(async));

    public override Task Column_collection_Where_Take(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Where_Take(async));

    public override Task Column_collection_Where_Skip_Take(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Where_Skip_Take(async));

    public override Task Column_collection_Contains_over_subquery(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Skip_Take(async));

    public override Task Column_collection_OrderByDescending_ElementAt(bool async)
        => AssertTranslationFailed(() => base.Column_collection_OrderByDescending_ElementAt(async));

    public override Task Column_collection_Where_ElementAt(bool async)
        => AssertTranslationFailed(() => base.Column_collection_Where_ElementAt(async));

    public override Task Column_collection_Any(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Any(async));

    public override Task Column_collection_Distinct(bool async)
        => AssertTranslationFailed(() => base.Column_collection_Distinct(async));

    public override Task Column_collection_SelectMany(bool async)
        => AssertTranslationFailed(() => base.Column_collection_SelectMany(async));

    public override Task Column_collection_SelectMany_with_filter(bool async)
        => AssertTranslationFailed(() => base.Column_collection_SelectMany_with_filter(async));

    public override Task Column_collection_SelectMany_with_Select_to_anonymous_type(bool async)
        => AssertTranslationFailed(() => base.Column_collection_SelectMany_with_Select_to_anonymous_type(async));

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

    public override Task Column_collection_Join_parameter_collection(bool async)
        => AssertTranslationFailed(() => base.Column_collection_Join_parameter_collection(async));

    public override Task Inline_collection_Join_ordered_column_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Inline_collection_Join_ordered_column_collection(async));

    public override Task Parameter_collection_Concat_column_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Parameter_collection_Concat_column_collection(async));

    public override Task Parameter_collection_with_type_inference_for_JsonScalarExpression(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Parameter_collection_Concat_column_collection(async));

    public override Task Column_collection_Union_parameter_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Union_parameter_collection(async));

    public override Task Column_collection_Intersect_inline_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_Intersect_inline_collection(async));

    public override Task Inline_collection_Except_column_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Inline_collection_Except_column_collection(async));

    public override Task Column_collection_Where_Union(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Inline_collection_Except_column_collection(async));

    public override async Task Column_collection_equality_parameter_collection(bool async)
    {
        await base.Column_collection_equality_parameter_collection(async);

        AssertSql(
            """
@ints='[1,10]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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
        await base.Column_collection_equality_inline_collection(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = N'[1,10]'
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

    public override Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async));

    public override Task Parameter_collection_in_subquery_Union_column_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Parameter_collection_in_subquery_Union_column_collection(async));

    public override Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Parameter_collection_in_subquery_Union_column_collection_nested(async));

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        // Base implementation asserts that a different exception is thrown
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query(async);

        AssertSql(
            """
@ints1='10'
@ints2='111'

SELECT COUNT(*)
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[Value] AS [Value0]
        FROM (VALUES (1, @ints1), (2, @ints2)) AS [i]([_ord], [Value])
        ORDER BY [i].[_ord]
        OFFSET 1 ROWS
    ) AS [i0]
    WHERE [i0].[Value0] > [p].[Id]) = 1
""");
    }

    public override Task Column_collection_in_subquery_Union_parameter_collection(bool async)
        => AssertCompatibilityLevelTooLow(() => base.Column_collection_in_subquery_Union_parameter_collection(async));

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async);

        AssertSql();
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

    public override Task Project_collection_of_ints_ordered(bool async)
        // we don't propagate error details from projection
        => AssertTranslationFailed(() => base.Project_collection_of_ints_ordered(async));

    public override Task Project_collection_of_datetimes_filtered(bool async)
        // we don't propagate error details from projection
        => AssertTranslationFailed(() => base.Project_collection_of_datetimes_filtered(async));

    public override async Task Project_collection_of_nullable_ints_with_paging(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_paging(async);

        // client eval
        AssertSql(
            """
SELECT [p].[NullableInts]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override Task Project_collection_of_nullable_ints_with_paging2(bool async)
        // we don't propagate error details from projection
        => AssertTranslationFailed(() => base.Project_collection_of_nullable_ints_with_paging2(async));

    public override async Task Project_collection_of_nullable_ints_with_paging3(bool async)
    {
        await base.Project_collection_of_nullable_ints_with_paging3(async);

        // client eval
        AssertSql(
            """
SELECT [p].[NullableInts]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_collection_of_ints_with_distinct(bool async)
    {
        await base.Project_collection_of_ints_with_distinct(async);

        // client eval
        AssertSql(
            """
SELECT [p].[Ints]
FROM [PrimitiveCollectionsEntity] AS [p]
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
SELECT TOP(1) [p].[Ints]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override Task Project_multiple_collections(bool async)
        // we don't propagate error details from projection
        => AssertTranslationFailed(() => base.Project_multiple_collections(async));

    public override Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
        // we don't propagate error details from projection
        => AssertTranslationFailed(() => base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(async));

    public override async Task Project_primitive_collections_element(bool async)
    {
        await base.Project_primitive_collections_element(async);

        AssertSql(
            """
SELECT [p].[Ints], [p].[DateTimes], [p].[Strings]
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
@ints1='1'
@ints2='2'
@ints3='3'
@strings1='one' (Size = 4000)
@strings2='two' (Size = 4000)
@strings3='three' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CASE
    WHEN [p].[Int] IN (@ints1, @ints2, @ints3) THEN N'one'
    ELSE N'two'
END IN (@strings1, @strings2, @strings3)
""");
    }

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping(async);

        AssertSql(
            """
@ints1='1'
@ints2='2'
@ints3='3'
@strings1='one' (Size = 4000)
@strings2='two' (Size = 4000)
@strings3='three' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CASE
    WHEN [p].[Int] IN (@ints1, @ints2, @ints3) THEN N'one'
    ELSE N'two'
END IN (@strings1, @strings2, @strings3)
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_struct(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_struct(async);

        AssertSql(
            """
@values1='22'
@values2='33'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[WrappedId] IN (@values1, @values2)
""",
            //
            """
@values1='11'
@values2='44'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[WrappedId] NOT IN (@values1, @values2)
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct(async);

        AssertSql(
            """
@values1='22'
@values2='33'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedId] IN (@values1, @values2)
""",
            //
            """
@values1='11'
@values2='44'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedId] NOT IN (@values1, @values2) OR [p].[NullableWrappedId] IS NULL
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(async);

        AssertSql(
            """
@values1='22'
@values2='33'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedIdWithNullableComparer] IN (@values1, @values2)
""",
            //
            """
@values1='11'
@values2='44'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedId] NOT IN (@values1, @values2) OR [p].[NullableWrappedId] IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_struct(async);

        AssertSql(
            """
@values1='22'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[WrappedId] = @values1
""",
            //
            """
@values1='11'
@values2='44'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[WrappedId] NOT IN (@values1, @values2)
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct(async);

        AssertSql(
            """
@values1='22'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedId] IS NULL OR [p].[NullableWrappedId] = @values1
""",
            //
            """
@values1='11'
@values2='44'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedId] NOT IN (@values1, @values2) OR [p].[NullableWrappedId] IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(async);

        AssertSql(
            """
@values1='22'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedIdWithNullableComparer] IS NULL OR [p].[NullableWrappedIdWithNullableComparer] = @values1
""",
            //
            """
@values1='11'
@values2='44'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableWrappedIdWithNullableComparer] NOT IN (@values1, @values2) OR [p].[NullableWrappedIdWithNullableComparer] IS NULL
""");
    }

    public override async Task Values_of_enum_casted_to_underlying_value(bool async)
    {
        await base.Values_of_enum_casted_to_underlying_value(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(0 AS int)), (1), (2), (3)) AS [v]([Value])
    WHERE [v].[Value] = [p].[Int]) > 0
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private Task AssertCompatibilityLevelTooLow(Func<Task> query)
        => AssertTranslationFailedWithDetails(query, SqlServerStrings.CompatibilityLevelTooLowForScalarCollections(120));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private PrimitiveCollectionsContext CreateContext()
        => Fixture.CreateContext();

    public class PrimitiveCollectionsQueryOldSqlServerFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
    {
        // Use a different store name to prevent concurrency issues with the non-old PrimitiveCollectionsQuerySqlServerTest
        protected override string StoreName
            => "OldPrimitiveCollectionsTest";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        // Compatibility level 120 (SQL Server 2014) doesn't support OPENJSON
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseSqlServer(o => o.UseCompatibilityLevel(120));
    }
}
