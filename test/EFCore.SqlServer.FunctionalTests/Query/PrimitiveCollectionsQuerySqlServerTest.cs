// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable
using static System.Linq.Expressions.Expression;

public class PrimitiveCollectionsQuerySqlServerTest : PrimitiveCollectionsQueryRelationalTestBase<
    PrimitiveCollectionsQuerySqlServerTest.PrimitiveCollectionsQuerySqlServerFixture>
{
    public override int? NumberOfValuesForHugeParameterCollectionTests { get; } = 5000;

    public PrimitiveCollectionsQuerySqlServerTest(PrimitiveCollectionsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Inline_collection_of_ints_Contains()
    {
        await base.Inline_collection_of_ints_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains()
    {
        await base.Inline_collection_of_nullable_ints_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null()
    {
        await base.Inline_collection_of_nullable_ints_Contains_null();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IS NULL OR [p].[NullableInt] = 999
""");
    }

    public override async Task Inline_collection_Count_with_zero_values()
    {
        await base.Inline_collection_Count_with_zero_values();

        AssertSql();
    }

    public override async Task Inline_collection_Count_with_one_value()
    {
        await base.Inline_collection_Count_with_one_value();

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

    public override async Task Inline_collection_Count_with_two_values()
    {
        await base.Inline_collection_Count_with_two_values();

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

    public override async Task Inline_collection_Count_with_three_values()
    {
        await base.Inline_collection_Count_with_three_values();

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

    public override async Task Inline_collection_Contains_with_zero_values()
    {
        await base.Inline_collection_Contains_with_zero_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Inline_collection_Contains_with_one_value()
    {
        await base.Inline_collection_Contains_with_one_value();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values()
    {
        await base.Inline_collection_Contains_with_two_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values()
    {
        await base.Inline_collection_Contains_with_three_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters()
    {
        await base.Inline_collection_Contains_with_all_parameters();

        AssertSql(
            """
@i='2'
@j='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (@i, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter()
    {
        await base.Inline_collection_Contains_with_constant_and_parameter();

        AssertSql(
            """
@j='999'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types()
    {
        await base.Inline_collection_Contains_with_mixed_value_types();

        AssertSql(
            """
@i='11'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (999, @i, [p].[Id], [p].[Id] + [p].[Int])
""");
    }

    public override async Task Inline_collection_List_Contains_with_mixed_value_types()
    {
        await base.Inline_collection_List_Contains_with_mixed_value_types();

        AssertSql(
            """
@i='11'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (999, @i, [p].[Id], [p].[Id] + [p].[Int])
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate()
    {
        await base.Inline_collection_Contains_as_Any_with_predicate();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All()
    {
        await base.Inline_collection_negated_Contains_as_All();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values()
    {
        await base.Inline_collection_Min_with_two_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_List_Min_with_two_values()
    {
        await base.Inline_collection_List_Min_with_two_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_Max_with_two_values()
    {
        await base.Inline_collection_Max_with_two_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_List_Max_with_two_values()
    {
        await base.Inline_collection_List_Max_with_two_values();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[Int])) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_Min_with_three_values()
    {
        await base.Inline_collection_Min_with_three_values();

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

    public override async Task Inline_collection_List_Min_with_three_values()
    {
        await base.Inline_collection_List_Min_with_three_values();

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

    public override async Task Inline_collection_Max_with_three_values()
    {
        await base.Inline_collection_Max_with_three_values();

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

    public override async Task Inline_collection_List_Max_with_three_values()
    {
        await base.Inline_collection_List_Max_with_three_values();

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

    public override async Task Inline_collection_of_nullable_value_type_Min()
    {
        await base.Inline_collection_of_nullable_value_type_Min();

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

    public override async Task Inline_collection_of_nullable_value_type_Max()
    {
        await base.Inline_collection_of_nullable_value_type_Max();

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

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min()
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Min();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MIN([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[NullableInt]), (NULL)) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Max()
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Max();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT MAX([v].[Value])
    FROM (VALUES (CAST(30 AS int)), ([p].[NullableInt]), (NULL)) AS [v]([Value])) = 30
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Contains()
    {
        await base.Inline_collection_with_single_parameter_element_Contains();

        AssertSql(
            """
@i='2'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] = @i
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Count()
    {
        await base.Inline_collection_with_single_parameter_element_Count();

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

    public override async Task Inline_collection_Contains_with_EF_Parameter()
    {
        await base.Inline_collection_Contains_with_EF_Parameter();

        AssertSql(
            """
@p='[2,999,1000]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (
    SELECT [p0].[value]
    FROM OPENJSON(@p) WITH ([value] int '$') AS [p0]
)
""");
    }

    public override async Task Inline_collection_Contains_with_IEnumerable_EF_Parameter()
    {
        await base.Inline_collection_Contains_with_IEnumerable_EF_Parameter();

        AssertSql(
            """
@Select='["10","a","aa"]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableString] IN (
    SELECT [s].[value]
    FROM OPENJSON(@Select) WITH ([value] nvarchar(max) '$') AS [s]
)
""");
    }

    public override async Task Inline_collection_Contains_with_Nullable_Int_IEnumerable_Array_Containing_Null_EF_Parameter()
    {
        await base.Inline_collection_Contains_with_Nullable_Int_IEnumerable_Array_Containing_Null_EF_Parameter();

        AssertSql(
"""
@data_without_nulls='[1]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (
    SELECT [d].[value]
    FROM OPENJSON(@data_without_nulls) AS [d]
) OR [p].[NullableInt] IS NULL
""");
    }

    public override async Task Inline_collection_Count_with_column_predicate_with_EF_Parameter()
    {
        await base.Inline_collection_Count_with_column_predicate_with_EF_Parameter();

        AssertSql(
            """
@p='[2,999,1000]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(@p) WITH ([value] int '$') AS [p0]
    WHERE [p0].[value] > [p].[Id]) = 2
""");
    }

    public override async Task Inline_collection_in_query_filter()
    {
        await base.Inline_collection_in_query_filter();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(1 AS int)), (2), (3)) AS [v]([Value])
    WHERE [v].[Value] > [t].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_Count()
    {
        await base.Parameter_collection_Count();

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

    public override async Task Parameter_collection_of_ints_Contains_int()
    {
        await base.Parameter_collection_of_ints_Contains_int();

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

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int()
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int();

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

    public override async Task Parameter_collection_FrozenSet_of_ints_Contains_int()
    {
        await base.Parameter_collection_FrozenSet_of_ints_Contains_int();

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

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int()
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int();

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

    public override async Task Parameter_collection_IReadOnlySet_of_ints_Contains_int()
    {
        await base.Parameter_collection_IReadOnlySet_of_ints_Contains_int();

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

    public override async Task Parameter_collection_ReadOnlyCollectionWithContains_of_ints_Contains_int()
    {
        await base.Parameter_collection_ReadOnlyCollectionWithContains_of_ints_Contains_int();

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

    public override async Task Parameter_collection_of_ints_Contains_nullable_int()
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int();

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

    public override async Task Parameter_collection_of_nullable_ints_Contains_int()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int();

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

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int();

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

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int_with_EF_Parameter()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int_with_EF_Parameter();

        AssertSql(
            """
@nullableInts_without_nulls='[999]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[NullableInt] IN (
    SELECT [n].[value]
    FROM OPENJSON(@nullableInts_without_nulls) AS [n]
) OR [p].[NullableInt] IS NULL
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string()
    {
        await base.Parameter_collection_of_strings_Contains_string();

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

    public override async Task Parameter_collection_of_strings_Contains_nullable_string()
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string();

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

    public override async Task Parameter_collection_of_nullable_strings_Contains_string()
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string();

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

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string()
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string();

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

    public override async Task Parameter_collection_of_DateTimes_Contains()
    {
        await base.Parameter_collection_of_DateTimes_Contains();

        AssertSql(
            """
@dateTimes1='2020-01-10T12:30:00.0000000Z' (DbType = DateTime)
@dateTimes2='9999-01-01T00:00:00.0000000Z' (DbType = DateTime)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[DateTime] IN (@dateTimes1, @dateTimes2)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains()
    {
        await base.Parameter_collection_of_bools_Contains();

        AssertSql(
            """
@bools1='True'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Bool] = @bools1
""");
    }

    public override async Task Parameter_collection_of_enums_Contains()
    {
        await base.Parameter_collection_of_enums_Contains();

        AssertSql(
            """
@enums1='0'
@enums2='3'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Enum] IN (@enums1, @enums2)
""");
    }

    public override async Task Parameter_collection_null_Contains()
    {
        await base.Parameter_collection_null_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Parameter_collection_empty_Contains()
    {
        await base.Parameter_collection_empty_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Parameter_collection_empty_Join()
    {
        await base.Parameter_collection_empty_Join();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
INNER JOIN (
    SELECT NULL AS [Value]
    WHERE 0 = 1
) AS [p0] ON [p].[Id] = [p0].[Value]
""");
    }

    public override async Task Parameter_collection_Contains_with_EF_Constant()
    {
        await base.Parameter_collection_Contains_with_EF_Constant();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] IN (2, 999, 1000)
""");
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any()
    {
        await base.Parameter_collection_Where_with_EF_Constant_Where_Any();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (VALUES (CAST(2 AS int)), (999), (1000)) AS [i]([Value])
    WHERE [i].[Value] > 0)
""");
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_EF_Constant()
    {
        await base.Parameter_collection_Count_with_column_predicate_with_EF_Constant();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int)), (999), (1000)) AS [i]([Value])
    WHERE [i].[Value] > [p].[Id]) = 2
""");
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values();

        Assert.Contains("OPENJSON(@ids) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations();

        Assert.Contains("OPENJSON(@ids) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations_same_parameter()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations_same_parameter();

        Assert.Contains("@ids1=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("@ids2=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping();

        Assert.Contains("OPENJSON(@ids) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations_forced_constants()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations_forced_constants();

        Assert.Contains("@ids1=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("@ids2=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_Count_with_huge_number_of_values_over_5_operations_mixed_parameters_constants()
    {
        await base.Parameter_collection_Count_with_huge_number_of_values_over_5_operations_mixed_parameters_constants();

        Assert.Contains("OPENJSON(@ids) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values();

        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations();

        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_same_parameter()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_same_parameter();

        Assert.Contains("@ints1=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("@ints2=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("@ints1=", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
        Assert.Contains("@ints2=", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_2_operations_same_parameter_different_type_mapping();

        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_forced_constants()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_forced_constants();

        Assert.Contains("@ints1=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("@ints2=", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("@ints1=", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
        Assert.Contains("@ints2=", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_mixed_parameters_constants()
    {
        await base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values_over_5_operations_mixed_parameters_constants();

        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
        Assert.Contains("OPENJSON(@ints) WITH ([Value] int '$')", Fixture.TestSqlLoggerFactory.SqlStatements[1], StringComparison.Ordinal);
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_default_mode(ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Count_with_column_predicate_with_default_mode(mode);

        switch (mode)
        {
            case ParameterTranslationMode.Constant:
            {
                AssertSql(
                    """
SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int)), (999)) AS [i]([Value])
    WHERE [i].[Value] > [t].[Id]) = 1
""");
                break;
            }

            case ParameterTranslationMode.Parameter:
            {
                AssertSql(
                    """
@ids='[2,999]' (Size = 4000)

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > [t].[Id]) = 1
""");
                break;
            }

            case ParameterTranslationMode.MultipleParameters:
            {
                AssertSql(
                    """
@ids1='2'
@ids2='999'

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (@ids1), (@ids2)) AS [i]([Value])
    WHERE [i].[Value] > [t].[Id]) = 1
""");
                break;
            }

            default:
                throw new NotImplementedException();
        }
    }

    public override async Task Parameter_collection_Contains_with_default_mode(ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Contains_with_default_mode(mode);

        switch (mode)
        {
            case ParameterTranslationMode.Constant:
            {
                AssertSql(
                    """
SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (2, 999)
""");
                break;
            }

            case ParameterTranslationMode.Parameter:
            {
                AssertSql(
                    """
@ints='[2,999]' (Size = 4000)

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""");
                break;
            }

            case ParameterTranslationMode.MultipleParameters:
            {
                AssertSql(
                    """
@ints1='2'
@ints2='999'

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (@ints1, @ints2)
""");
                break;
            }

            default:
                throw new NotImplementedException();
        }
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Constant(mode);

        AssertSql(
            """
SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(2 AS int)), (999)) AS [i]([Value])
    WHERE [i].[Value] > [t].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_Contains_with_default_mode_EF_Constant(ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Contains_with_default_mode_EF_Constant(mode);

        AssertSql(
            """
SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (2, 999)
""");
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(
        ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_Parameter(mode);

        AssertSql(
            """
@ids='[2,999]' (Size = 4000)

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(@ids) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > [t].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_Contains_with_default_mode_EF_Parameter(ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Contains_with_default_mode_EF_Parameter(mode);

        AssertSql(
            """
@ints='[2,999]' (Size = 4000)

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (
    SELECT [i].[value]
    FROM OPENJSON(@ints) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(
        ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Count_with_column_predicate_with_default_mode_EF_MultipleParameters(mode);

        AssertSql(
            """
@ids1='2'
@ids2='999'

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (@ids1), (@ids2)) AS [i]([Value])
    WHERE [i].[Value] > [t].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_Contains_with_default_mode_EF_MultipleParameters(ParameterTranslationMode mode)
    {
        await base.Parameter_collection_Contains_with_default_mode_EF_MultipleParameters(mode);

        AssertSql(
            """
@ints1='2'
@ints2='999'

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_Contains_parameter_bucketization()
    {
        await base.Parameter_collection_Contains_parameter_bucketization();

        AssertSql(
            """
@ints1='2'
@ints2='999'
@ints3='2'
@ints4='2'
@ints5='2'
@ints6='2'
@ints7='2'
@ints8='2'
@ints9='2'
@ints10='2'
@ints11='2'
@ints12='2'
@ints13='2'
@ints14='2'
@ints15='2'
@ints16='2'
@ints17='2'
@ints18='2'
@ints19='2'
@ints20='2'

SELECT [t].[Id]
FROM [TestEntity] AS [t]
WHERE [t].[Id] IN (@ints1, @ints2, @ints3, @ints4, @ints5, @ints6, @ints7, @ints8, @ints9, @ints10, @ints11, @ints12, @ints13, @ints14, @ints15, @ints16, @ints17, @ints18, @ints19, @ints20)
""");
    }

    public override async Task Static_readonly_collection_List_of_ints_Contains_int()
    {
        await base.Static_readonly_collection_List_of_ints_Contains_int();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""",
            //
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (10, 999)
""");
    }

    public override async Task Static_readonly_collection_FrozenSet_of_ints_Contains_int()
    {
        await base.Static_readonly_collection_FrozenSet_of_ints_Contains_int();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""",
            //
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (10, 999)
""");
    }

    public override async Task Static_readonly_collection_ImmutableArray_of_ints_Contains_int()
    {
        await base.Static_readonly_collection_ImmutableArray_of_ints_Contains_int();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""",
            //
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] NOT IN (10, 999)
""");
    }

    public override async Task Column_collection_of_ints_Contains()
    {
        await base.Column_collection_of_ints_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 10 IN (
    SELECT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains()
    {
        await base.Column_collection_of_nullable_ints_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 10 IN (
    SELECT [n].[value]
    FROM OPENJSON([p].[NullableInts]) WITH ([value] int '$') AS [n]
)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains_null()
    {
        await base.Column_collection_of_nullable_ints_Contains_null();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([p].[NullableInts]) WITH ([value] int '$') AS [n]
    WHERE [n].[value] IS NULL)
""");
    }

    public override async Task Column_collection_of_strings_Contains()
    {
        await base.Column_collection_of_strings_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE N'10' IN (
    SELECT [s].[value]
    FROM OPENJSON([p].[Strings]) WITH ([value] nvarchar(max) '$') AS [s]
)
""");
    }

    public override async Task Column_collection_of_strings_Contains_null()
    {
        await base.Column_collection_of_strings_Contains_null();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 0 = 1
""");
    }

    public override async Task Column_collection_of_nullable_strings_contains_null()
    {
        await base.Column_collection_of_nullable_strings_contains_null();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([p].[NullableStrings]) WITH ([value] nvarchar(max) '$') AS [n]
    WHERE [n].[value] IS NULL)
""");
    }

    public override async Task Column_collection_of_bools_Contains()
    {
        await base.Column_collection_of_bools_Contains();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
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

    public override async Task Column_with_custom_converter()
    {
        await base.Column_with_custom_converter();

        AssertSql(
            """
@ints='1,2,3' (Size = 4000)

SELECT TOP(2) [t].[Id], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE [t].[Ints] = @ints
""");
    }

    public override async Task Column_collection_inside_json_owned_entity()
    {
        await base.Column_collection_inside_json_owned_entity();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Owned]
FROM [TestOwner] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON(JSON_QUERY([t].[Owned], '$.Strings')) AS [s]) = 2
""",
            //
            """
SELECT TOP(2) [t].[Id], [t].[Owned]
FROM [TestOwner] AS [t]
WHERE JSON_VALUE([t].[Owned], '$.Strings[1]') = N'bar'
""");
    }

    public override async Task Parameter_with_inferred_value_converter()
    {
        await base.Parameter_with_inferred_value_converter();

        AssertSql("");
    }

    public override async Task Constant_with_inferred_value_converter()
    {
        await base.Constant_with_inferred_value_converter();

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[PropertyWithValueConverter]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(1 AS int)), (8)) AS [v]([Value])
    WHERE [v].[Value] = [t].[PropertyWithValueConverter]) = 1
""");
    }

    [ConditionalFact]
    public override Task Multidimensional_array_is_not_supported()
        => base.Multidimensional_array_is_not_supported();

    public override async Task Contains_on_Enumerable()
    {
        await base.Contains_on_Enumerable();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

    public override async Task Contains_on_MemoryExtensions()
    {
        await base.Contains_on_MemoryExtensions();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

    public override async Task Contains_with_MemoryExtensions_with_null_comparer()
    {
        await base.Contains_with_MemoryExtensions_with_null_comparer();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Int] IN (10, 999)
""");
    }

    public override async Task Column_collection_Count_method()
    {
        await base.Column_collection_Count_method();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([p].[Ints]) AS [i]) = 2
""");
    }

    public override async Task Column_collection_Length()
    {
        await base.Column_collection_Length();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([p].[Ints]) AS [i]) = 2
""");
    }

    public override async Task Column_collection_Count_with_predicate()
    {
        await base.Column_collection_Count_with_predicate();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1) = 2
""");
    }

    public override async Task Column_collection_Where_Count()
    {
        await base.Column_collection_Where_Count();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1) = 2
""");
    }

    public override async Task Column_collection_index_int()
    {
        await base.Column_collection_index_int();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[1]') AS int) = 10
""");
    }

    public override async Task Column_collection_index_string()
    {
        await base.Column_collection_index_string();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE JSON_VALUE([p].[Strings], '$[1]') = N'10'
""");
    }

    public override async Task Column_collection_index_datetime()
    {
        await base.Column_collection_index_datetime();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[DateTimes], '$[1]') AS datetime2) = '2020-01-10T12:30:00.0000000Z'
""");
    }

    public override async Task Column_collection_index_beyond_end()
    {
        await base.Column_collection_index_beyond_end();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[999]') AS int) = 10
""");
    }

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column()
    {
        // TODO: This test is incorrect, see #33784
        await base.Nullable_reference_column_collection_index_equals_nullable_column();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE JSON_VALUE([p].[NullableStrings], '$[2]') = [p].[NullableString] OR (JSON_VALUE([p].[NullableStrings], '$[2]') IS NULL AND [p].[NullableString] IS NULL)
""");
    }

    public override async Task Non_nullable_reference_column_collection_index_equals_nullable_column()
    {
        await base.Non_nullable_reference_column_collection_index_equals_nullable_column();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([p].[Strings]) AS [s]) AND JSON_VALUE([p].[Strings], '$[1]') = [p].[NullableString]
""");
    }

    public override async Task Inline_collection_index_Column()
    {
        await base.Inline_collection_index_Column();

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

    public override async Task Inline_collection_index_Column_with_EF_Constant()
    {
        await base.Inline_collection_index_Column_with_EF_Constant();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(N'[1,2,3]', '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = 1
""");
    }

    public override async Task Inline_collection_value_index_Column()
    {
        await base.Inline_collection_value_index_Column();

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

    public override async Task Inline_collection_List_value_index_Column()
    {
        await base.Inline_collection_List_value_index_Column();

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

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Parameter_collection_index_Column_equal_Column()
    {
        await base.Parameter_collection_index_Column_equal_Column();

        AssertSql(
            """
@ints='[0,2,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(@ints, '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = [p].[Int]
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Parameter_collection_index_Column_equal_constant()
    {
        await base.Parameter_collection_index_Column_equal_constant();

        AssertSql(
            """
@ints='[1,2,3]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE(@ints, '$[' + CAST([p].[Int] AS nvarchar(max)) + ']') AS int) = 1
""");
    }

    public override async Task Column_collection_ElementAt()
    {
        await base.Column_collection_ElementAt();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE CAST(JSON_VALUE([p].[Ints], '$[1]') AS int) = 10
""");
    }

    public override async Task Column_collection_First()
    {
        await base.Column_collection_First();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)) = 1
""");
    }

    public override async Task Column_collection_FirstOrDefault()
    {
        await base.Column_collection_FirstOrDefault();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE COALESCE((
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)), 0) = 1
""");
    }

    public override async Task Column_collection_Single()
    {
        await base.Column_collection_Single();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)) = 1
""");
    }

    public override async Task Column_collection_SingleOrDefault()
    {
        await base.Column_collection_SingleOrDefault();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE COALESCE((
    SELECT TOP(1) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)), 0) = 1
""");
    }

    public override async Task Column_collection_Skip()
    {
        await base.Column_collection_Skip();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([p].[Ints]) AS [i]
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [i0]) = 2
""");
    }

    public override async Task Column_collection_Take()
    {
        await base.Column_collection_Take();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT TOP(2) CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)
)
""");
    }

    public override async Task Column_collection_Skip_Take()
    {
        await base.Column_collection_Skip_Take();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    ORDER BY CAST([i].[key] AS int)
    OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
)
""");
    }

    public override async Task Column_collection_Where_Skip()
    {
        await base.Column_collection_Where_Skip();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([p].[Ints]) AS [i]
        WHERE CAST([i].[value] AS int) > 1
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS
    ) AS [i0]) = 3
""");
    }

    public override async Task Column_collection_Where_Take()
    {
        await base.Column_collection_Where_Take();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT TOP(2) 1 AS empty
        FROM OPENJSON([p].[Ints]) AS [i]
        WHERE CAST([i].[value] AS int) > 1
        ORDER BY CAST([i].[key] AS int)
    ) AS [i0]) = 2
""");
    }

    public override async Task Column_collection_Where_Skip_Take()
    {
        await base.Column_collection_Where_Skip_Take();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([p].[Ints]) AS [i]
        WHERE CAST([i].[value] AS int) > 1
        ORDER BY CAST([i].[key] AS int)
        OFFSET 1 ROWS FETCH NEXT 2 ROWS ONLY
    ) AS [i0]) = 1
""");
    }

    public override async Task Column_collection_Contains_over_subquery()
    {
        await base.Column_collection_Contains_over_subquery();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE 11 IN (
    SELECT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1
)
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt()
    {
        await base.Column_collection_OrderByDescending_ElementAt();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    ORDER BY [i].[value] DESC
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 111
""");
    }

    public override async Task Column_collection_Where_ElementAt()
    {
        await base.Column_collection_Where_ElementAt();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT CAST([i].[value] AS int) AS [value]
    FROM OPENJSON([p].[Ints]) AS [i]
    WHERE CAST([i].[value] AS int) > 1
    ORDER BY CAST([i].[key] AS int)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 11
""");
    }

    public override async Task Column_collection_Any()
    {
        await base.Column_collection_Any();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([p].[Ints]) AS [i])
""");
    }

    public override async Task Column_collection_Distinct()
    {
        await base.Column_collection_Distinct();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    ) AS [i0]) = 3
""");
    }

    public override async Task Column_collection_SelectMany()
    {
        await base.Column_collection_SelectMany();

        AssertSql(
            """
SELECT [i].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
CROSS APPLY OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
""");
    }

    public override async Task Column_collection_SelectMany_with_filter()
    {
        await base.Column_collection_SelectMany_with_filter();

        AssertSql(
            """
SELECT [i0].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
CROSS APPLY (
    SELECT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    WHERE [i].[value] > 1
) AS [i0]
""");
    }

    public override async Task Column_collection_SelectMany_with_Select_to_anonymous_type()
    {
        await base.Column_collection_SelectMany_with_Select_to_anonymous_type();

        AssertSql(
            """
SELECT [i].[value] AS [Original], [i].[value] + 1 AS [Incremented]
FROM [PrimitiveCollectionsEntity] AS [p]
CROSS APPLY OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
""");
    }

    public override async Task Column_collection_projection_from_top_level()
    {
        await base.Column_collection_projection_from_top_level();

        AssertSql(
            """
SELECT [p].[Ints]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override async Task Column_collection_Join_parameter_collection()
    {
        await base.Column_collection_Join_parameter_collection();

        AssertSql(
            """
@ints1='11'
@ints2='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    INNER JOIN (VALUES (@ints1), (@ints2)) AS [i0]([Value]) ON [i].[value] = [i0].[Value]) = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection()
    {
        await base.Inline_collection_Join_ordered_column_collection();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
    INNER JOIN OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i] ON [v].[Value] = [i].[value]) = 2
""");
    }

    public override async Task Parameter_collection_Concat_column_collection()
    {
        await base.Parameter_collection_Concat_column_collection();

        AssertSql(
            """
@p1='11'
@p2='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM (VALUES (@p1), (@p2)) AS [p0]([Value])
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON([p].[Ints]) AS [i]
    ) AS [u]) = 2
""");
    }

    [SqlServerCondition(SqlServerCondition.SupportsJsonPathExpressions)]
    public override async Task Parameter_collection_with_type_inference_for_JsonScalarExpression()
    {
        await base.Parameter_collection_with_type_inference_for_JsonScalarExpression();

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

    public override async Task Column_collection_Union_parameter_collection()
    {
        await base.Column_collection_Union_parameter_collection();

        AssertSql(
            """
@ints1='11'
@ints2='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
        UNION
        SELECT [i0].[Value] AS [value]
        FROM (VALUES (@ints1), (@ints2)) AS [i0]([Value])
    ) AS [u]) = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection()
    {
        await base.Column_collection_Intersect_inline_collection();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
        INTERSECT
        SELECT [v].[Value] AS [value]
        FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
    ) AS [i0]) = 2
""");
    }

    public override async Task Inline_collection_Except_column_collection()
    {
        await base.Inline_collection_Except_column_collection();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [v].[Value]
        FROM (VALUES (CAST(11 AS int)), (111)) AS [v]([Value])
        EXCEPT
        SELECT [i].[value] AS [Value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    ) AS [e]
    WHERE [e].[Value] % 2 = 1) = 2
""");
    }

    public override async Task Column_collection_Where_Union()
    {
        await base.Column_collection_Where_Union();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i].[value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
        WHERE [i].[value] > 100
        UNION
        SELECT [v].[Value] AS [value]
        FROM (VALUES (CAST(50 AS int))) AS [v]([Value])
    ) AS [u]) = 2
""");
    }

    public override async Task Column_collection_equality_parameter_collection()
    {
        await base.Column_collection_equality_parameter_collection();

        AssertSql(
            """
@ints='[1,10]' (Size = 4000)

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = @ints
""");
    }

    public override async Task Column_collection_Concat_parameter_collection_equality_inline_collection()
    {
        await base.Column_collection_Concat_parameter_collection_equality_inline_collection();

        AssertSql();
    }

    public override async Task Column_collection_equality_inline_collection()
    {
        await base.Column_collection_equality_inline_collection();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Ints] = N'[1,10]'
""");
    }

    public override async Task Column_collection_equality_inline_collection_with_parameters()
    {
        await base.Column_collection_equality_inline_collection_with_parameters();

        AssertSql();
    }

    public override async Task Column_collection_Where_equality_inline_collection()
    {
        await base.Column_collection_Where_equality_inline_collection();

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query();

        AssertSql(
            """
@ints1='10'
@ints2='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i1].[Value]
        FROM (
            SELECT [i].[Value]
            FROM (VALUES (0, @ints1), (1, @ints2)) AS [i]([_ord], [Value])
            ORDER BY [i].[_ord]
            OFFSET 1 ROWS
        ) AS [i1]
        UNION
        SELECT [i0].[value] AS [Value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i0]
    ) AS [u]) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection();

        AssertSql(
            """
@Skip1='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[Value]
        FROM (VALUES (@Skip1)) AS [s]([Value])
        UNION
        SELECT [i].[value] AS [Value]
        FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
    ) AS [u]) = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_nested();

        AssertSql(
            """
@Skip1='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[Value]
        FROM (VALUES (@Skip1)) AS [s]([Value])
        UNION
        SELECT [i2].[value] AS [Value]
        FROM (
            SELECT TOP(20) [i1].[value]
            FROM (
                SELECT DISTINCT [i0].[value]
                FROM (
                    SELECT [i].[value]
                    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
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

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query()
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query();

        // TODO: the subquery projection contains extra columns which we should remove
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
        FROM (VALUES (0, @ints1), (1, @ints2)) AS [i]([_ord], [Value])
        ORDER BY [i].[_ord]
        OFFSET 1 ROWS
    ) AS [i0]
    WHERE [i0].[Value0] > [p].[Id]) = 1
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
    {
        await base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query();

        AssertSql();
    }

    public override async Task Compiled_query_with_uncorrelated_parameter_collection_expression()
    {
        await base.Compiled_query_with_uncorrelated_parameter_collection_expression();

        AssertSql(
            """
SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT NULL AS [Value]
        WHERE 0 = 1
    ) AS [i])
""");
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection()
    {
        await base.Column_collection_in_subquery_Union_parameter_collection();

        AssertSql(
            """
@ints1='10'
@ints2='111'

SELECT [p].[Id], [p].[Bool], [p].[Bools], [p].[DateTime], [p].[DateTimes], [p].[Enum], [p].[Enums], [p].[Int], [p].[Ints], [p].[NullableInt], [p].[NullableInts], [p].[NullableString], [p].[NullableStrings], [p].[NullableWrappedId], [p].[NullableWrappedIdWithNullableComparer], [p].[String], [p].[Strings], [p].[WrappedId]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [i1].[value]
        FROM (
            SELECT CAST([i].[value] AS int) AS [value]
            FROM OPENJSON([p].[Ints]) AS [i]
            ORDER BY CAST([i].[key] AS int)
            OFFSET 1 ROWS
        ) AS [i1]
        UNION
        SELECT [i0].[Value] AS [value]
        FROM (VALUES (@ints1), (@ints2)) AS [i0]([Value])
    ) AS [u]) = 3
""");
    }

    public override async Task Project_collection_of_ints_simple()
    {
        await base.Project_collection_of_ints_simple();

        AssertSql(
            """
SELECT [p].[Ints]
FROM [PrimitiveCollectionsEntity] AS [p]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_collection_of_ints_ordered()
    {
        await base.Project_collection_of_ints_ordered();

        AssertSql(
            """
SELECT [p].[Id], CAST([i].[value] AS int) AS [value], [i].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY OPENJSON([p].[Ints]) AS [i]
ORDER BY [p].[Id], CAST([i].[value] AS int) DESC
""");
    }

    public override async Task Project_collection_of_datetimes_filtered()
    {
        await base.Project_collection_of_datetimes_filtered();

        AssertSql(
            """
SELECT [p].[Id], [d0].[value], [d0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([d].[value] AS datetime2) AS [value], [d].[key], CAST([d].[key] AS int) AS [c]
    FROM OPENJSON([p].[DateTimes]) AS [d]
    WHERE DATEPART(day, CAST([d].[value] AS datetime2)) <> 1
) AS [d0]
ORDER BY [p].[Id], [d0].[c]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging()
    {
        await base.Project_collection_of_nullable_ints_with_paging();

        AssertSql(
            """
SELECT [p].[Id], [n0].[value], [n0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT TOP(20) CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    ORDER BY CAST([n].[key] AS int)
) AS [n0]
ORDER BY [p].[Id], [n0].[c]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging2()
    {
        await base.Project_collection_of_nullable_ints_with_paging2();

        AssertSql(
            """
SELECT [p].[Id], [n0].[value], [n0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    ORDER BY CAST([n].[value] AS int)
    OFFSET 1 ROWS
) AS [n0]
ORDER BY [p].[Id], [n0].[value]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging3()
    {
        await base.Project_collection_of_nullable_ints_with_paging3();

        AssertSql(
            """
SELECT [p].[Id], [n0].[value], [n0].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    ORDER BY CAST([n].[key] AS int)
    OFFSET 2 ROWS
) AS [n0]
ORDER BY [p].[Id], [n0].[c]
""");
    }

    public override async Task Project_collection_of_ints_with_distinct()
    {
        await base.Project_collection_of_ints_with_distinct();

        AssertSql(
            """
SELECT [p].[Id], [i0].[value]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT DISTINCT [i].[value]
    FROM OPENJSON([p].[Ints]) WITH ([value] int '$') AS [i]
) AS [i0]
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_distinct()
    {
        await base.Project_collection_of_nullable_ints_with_distinct();

        AssertSql("");
    }

    public override async Task Project_collection_of_ints_with_ToList_and_FirstOrDefault()
    {
        await base.Project_collection_of_ints_with_ToList_and_FirstOrDefault();

        AssertSql(
            """
SELECT [p0].[Id], CAST([i].[value] AS int) AS [value], [i].[key]
FROM (
    SELECT TOP(1) [p].[Id], [p].[Ints]
    FROM [PrimitiveCollectionsEntity] AS [p]
    ORDER BY [p].[Id]
) AS [p0]
OUTER APPLY OPENJSON([p0].[Ints]) AS [i]
ORDER BY [p0].[Id], CAST([i].[key] AS int)
""");
    }

    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls()
    {
        await base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls();

        AssertSql(
            """
SELECT [p].[Id], [n1].[value], [n1].[key], [n2].[value], [n2].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY (
    SELECT CAST([n].[value] AS int) AS [value], [n].[key], CAST([n].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n]
    WHERE 0 = 1
) AS [n1]
OUTER APPLY (
    SELECT CAST([n0].[value] AS int) AS [value], [n0].[key], CAST([n0].[key] AS int) AS [c]
    FROM OPENJSON([p].[NullableInts]) AS [n0]
    WHERE [n0].[value] IS NULL
) AS [n2]
ORDER BY [p].[Id], [n1].[c], [n1].[key], [n2].[c]
""");
    }

    public override async Task Project_multiple_collections()
    {
        await base.Project_multiple_collections();

        AssertSql(
            """
SELECT [p].[Id], CAST([i].[value] AS int) AS [value], [i].[key], CAST([i0].[value] AS int) AS [value], [i0].[key], [d1].[value], [d1].[key], [d2].[value], [d2].[key]
FROM [PrimitiveCollectionsEntity] AS [p]
OUTER APPLY OPENJSON([p].[Ints]) AS [i]
OUTER APPLY OPENJSON([p].[Ints]) AS [i0]
OUTER APPLY (
    SELECT CAST([d].[value] AS datetime2) AS [value], [d].[key], CAST([d].[key] AS int) AS [c]
    FROM OPENJSON([p].[DateTimes]) AS [d]
    WHERE DATEPART(day, CAST([d].[value] AS datetime2)) <> 1
) AS [d1]
OUTER APPLY (
    SELECT CAST([d0].[value] AS datetime2) AS [value], [d0].[key], CAST([d0].[key] AS int) AS [c]
    FROM OPENJSON([p].[DateTimes]) AS [d0]
    WHERE CAST([d0].[value] AS datetime2) > '2000-01-01T00:00:00.0000000'
) AS [d2]
ORDER BY [p].[Id], CAST([i].[key] AS int), [i].[key], CAST([i0].[value] AS int) DESC, [i0].[key], [d1].[c], [d1].[key], [d2].[c]
""");
    }

    public override async Task Project_primitive_collections_element()
    {
        await base.Project_primitive_collections_element();

        AssertSql(
            """
SELECT CAST(JSON_VALUE([p].[Ints], '$[0]') AS int) AS [Indexer], CAST(JSON_VALUE([p].[DateTimes], '$[0]') AS datetime2) AS [EnumerableElementAt], JSON_VALUE([p].[Strings], '$[1]') AS [QueryableElementAt]
FROM [PrimitiveCollectionsEntity] AS [p]
WHERE [p].[Id] < 4
ORDER BY [p].[Id]
""");
    }

    public override async Task Project_inline_collection()
    {
        await base.Project_inline_collection();

        AssertSql(
            """
SELECT [p].[String]
FROM [PrimitiveCollectionsEntity] AS [p]
""");
    }

    public override async Task Project_inline_collection_with_Union()
    {
        await base.Project_inline_collection_with_Union();

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

    public override async Task Project_inline_collection_with_Concat()
    {
        await base.Project_inline_collection_with_Concat();

        AssertSql();
    }

    public override async Task Project_collection_from_entity_type_with_owned()
    {
        await base.Project_collection_from_entity_type_with_owned();

        AssertSql(
            """
SELECT [t].[Ints]
FROM [TestEntityWithOwned] AS [t]
""");
    }

    public override async Task Subquery_over_primitive_collection_on_inheritance_derived_type()
    {
        await base.Subquery_over_primitive_collection_on_inheritance_derived_type();

        AssertSql(
            """
SELECT [b].[Id], [b].[Discriminator], [b].[Ints]
FROM [BaseType] AS [b]
WHERE EXISTS (
    SELECT 1
    FROM OPENJSON([b].[Ints]) AS [i])
""");
    }

    public override async Task Nested_contains_with_Lists_and_no_inferred_type_mapping()
    {
        await base.Nested_contains_with_Lists_and_no_inferred_type_mapping();

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

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping()
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping();

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

    public override async Task Parameter_collection_of_structs_Contains_struct()
    {
        await base.Parameter_collection_of_structs_Contains_struct();

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

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct()
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct();

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

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer()
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer();

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

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct()
    {
        await base.Parameter_collection_of_nullable_structs_Contains_struct();

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

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct()
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct();

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

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer()
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer();

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

    public override async Task Values_of_enum_casted_to_underlying_value()
    {
        await base.Values_of_enum_casted_to_underlying_value();

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

    [ConditionalFact] // #37605
    public virtual async Task Parameter_collection_with_null_value_Contains_null_2201_values()
    {
        using var context = Fixture.CreateContext();

        var values = Enumerable.Range(1, 2200).Select(i => (int?)i).ToList();
        values.Add(null);

        await AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => values.Contains(e.NullableInt)));

        // No SQL assertion as the SQL is huge
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_string()
    {
        await TestOrderedArray("a", "b");

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [s].[value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = N'a') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_int()
    {
        await TestOrderedArray(1, 2);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS int) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 1) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_long()
    {
        await TestOrderedArray(1L, 2L);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS bigint) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS bigint)) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_short()
    {
        await TestOrderedArray((short)1, (short)2);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS smallint) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS smallint)) = 2
""");
    }

    // On relational databases, byte[] gets mapped to a special binary data type, which isn't queryable as a regular primitive collection.
    [ConditionalFact]
    public virtual async Task Ordered_array_of_byte()
        => await AssertTranslationFailed(() => TestOrderedArray((byte)1, (byte)2));

    [ConditionalFact]
    public virtual async Task Ordered_array_of_double()
    {
        await TestOrderedArray(1d, 2d);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS float) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 1.0E0) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_float()
    {
        await TestOrderedArray(1f, 2f);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS real) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS real)) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_decimal()
    {
        await TestOrderedArray(1m, 2m);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS decimal(18,2)) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 1.0) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_DateTime()
    {
        await TestOrderedArray(new DateTime(2023, 1, 1, 12, 30, 0), new DateTime(2023, 1, 2, 12, 30, 0));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS datetime2) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '2023-01-01T12:30:00.0000000') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_DateOnly()
    {
        await TestOrderedArray(new DateOnly(2023, 1, 1), new DateOnly(2023, 1, 2));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS date) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '2023-01-01') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_TimeOnly()
    {
        await TestOrderedArray(new TimeOnly(12, 30, 0), new TimeOnly(12, 30, 1));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS time) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '12:30:00') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_DateTimeOffset()
    {
        await TestOrderedArray(
            new DateTimeOffset(2023, 1, 1, 12, 30, 0, TimeSpan.FromHours(2)),
            new DateTimeOffset(2023, 1, 2, 12, 30, 0, TimeSpan.FromHours(2)));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS datetimeoffset) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = '2023-01-01T12:30:00.0000000+02:00') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_bool()
    {
        await TestOrderedArray(true, false);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS bit) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = CAST(1 AS bit)) = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_guid()
    {
        await TestOrderedArray(
            new Guid("dc8c903d-d655-4144-a0fd-358099d40ae1"),
            new Guid("008719a5-1999-4798-9cf3-92a78ffa94a2"));

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS uniqueidentifier) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 'dc8c903d-d655-4144-a0fd-358099d40ae1') = 2
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_byte_array()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => TestOrderedArray([1, 2], new byte[] { 3, 4 }));

        Assert.Equal(SqlServerStrings.QueryingOrderedBinaryJsonCollectionsNotSupported, exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Ordered_array_of_enum()
    {
        await TestOrderedArray(OrderedTestEnum.Label1, OrderedTestEnum.Label2);

        AssertSql(
            """
SELECT TOP(2) [t].[Id], [t].[Ints], [t].[SomeArray]
FROM [TestEntity] AS [t]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST([s].[value] AS int) AS [value]
        FROM OPENJSON([t].[SomeArray]) AS [s]
        ORDER BY CAST([s].[key] AS int)
        OFFSET 1 ROWS
    ) AS [s0]
    WHERE [s0].[value] = 0) = 2
""");
    }

    private enum OrderedTestEnum { Label1, Label2 }

    private async Task TestOrderedArray<TElement>(
        TElement value1,
        TElement value2,
        Action<ModelBuilder> onModelCreating = null)
    {
        var arrayClrType = typeof(TElement).MakeArrayType();

        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: onModelCreating ?? (mb => mb.Entity<TestEntity>().Property(arrayClrType, "SomeArray")),
            seed: context =>
            {
                var instance1 = new TestEntity { Id = 1 };
                context.Add(instance1);
                var array1 = new TElement[3];
                array1.SetValue(value1, 0); // We have an extra copy of the first value which we'll Skip, to preserve the ordering
                array1.SetValue(value1, 1);
                array1.SetValue(value1, 2);
                context.Entry(instance1).Property("SomeArray").CurrentValue = array1;

                var instance2 = new TestEntity { Id = 2 };
                context.Add(instance2);
                var array2 = new TElement[3];
                array2.SetValue(value1, 0);
                array2.SetValue(value1, 1);
                array2.SetValue(value2, 2);
                context.Entry(instance2).Property("SomeArray").CurrentValue = array2;

                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var entityParam = Parameter(typeof(TestEntity), "m");
        var efPropertyCall = Call(
            typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)!.MakeGenericMethod(arrayClrType),
            entityParam,
            Constant("SomeArray"));

        var elementParam = Parameter(typeof(TElement), "a");
        var predicate = Lambda<Func<TestEntity, bool>>(
            Equal(
                Call(
                    CountWithPredicateMethod.MakeGenericMethod(typeof(TElement)),
                    Call(
                        SkipMethod.MakeGenericMethod(typeof(TElement)),
                        efPropertyCall,
                        Constant(1)),
                    Lambda(Equal(elementParam, Constant(value1)), elementParam)),
                Constant(2)),
            entityParam);

        // context.Set<TestEntity>().SingleAsync(m => EF.Property<int[]>(m, "SomeArray").Skip(1).Count(a => a == <value1>) == 2)
        var result = await context.Set<TestEntity>().SingleAsync(predicate);
        Assert.Equal(1, result.Id);
    }

    private static readonly MethodInfo CountWithPredicateMethod
        = typeof(Enumerable).GetRuntimeMethods().Single(m => m.Name == nameof(Enumerable.Count) && m.GetParameters().Length == 2);

    private static readonly MethodInfo SkipMethod
        = typeof(Enumerable).GetRuntimeMethods().Single(m => m.Name == nameof(Enumerable.Skip) && m.GetParameters().Length == 2);

    [ConditionalFact]
    public virtual async Task Same_parameter_with_different_type_mappings()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(b =>
            {
                b.Property(typeof(DateTime), "DateTime").HasColumnType("datetime");
                b.Property(typeof(DateTime), "DateTime2").HasColumnType("datetime2");
            }));

        await using var context = contextFactory.CreateDbContext();

        var dateTimes = new[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00) };

        _ = await context.Set<TestEntity>()
            .Where(m =>
                dateTimes.Contains(EF.Property<DateTime>(m, "DateTime"))
                && dateTimes.Contains(EF.Property<DateTime>(m, "DateTime2")))
            .ToArrayAsync();

        AssertSql(
            """
@dateTimes1='2020-01-01T12:30:00.0000000' (DbType = DateTime)
@dateTimes2='2020-01-02T12:30:00.0000000' (DbType = DateTime)
@dateTimes3='2020-01-01T12:30:00.0000000'
@dateTimes4='2020-01-02T12:30:00.0000000'

SELECT [t].[Id], [t].[DateTime], [t].[DateTime2], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE [t].[DateTime] IN (@dateTimes1, @dateTimes2) AND [t].[DateTime2] IN (@dateTimes3, @dateTimes4)
""");
    }

    [ConditionalFact]
    public virtual async Task Same_collection_with_default_type_mapping_and_uninferrable_context()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(b => b.Property(typeof(DateTime), "DateTime")));

        await using var context = contextFactory.CreateDbContext();

        var dateTimes = new DateTime?[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00), null };

        _ = await context.Set<TestEntity>()
            .Where(m => dateTimes.Any(d => d == EF.Property<DateTime>(m, "DateTime") && d != null))
            .ToArrayAsync();

        AssertSql(
            """
@dateTimes1='2020-01-01T12:30:00.0000000'
@dateTimes2='2020-01-02T12:30:00.0000000'
@dateTimes3=NULL (DbType = DateTime2)

SELECT [t].[Id], [t].[DateTime], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE EXISTS (
    SELECT 1
    FROM (VALUES (@dateTimes1), (@dateTimes2), (@dateTimes3)) AS [d]([Value])
    WHERE [d].[Value] = [t].[DateTime] AND [d].[Value] IS NOT NULL)
""");
    }

    [ConditionalFact]
    public virtual async Task Same_collection_with_non_default_type_mapping_and_uninferrable_context()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(b => b.Property(typeof(DateTime), "DateTime").HasColumnType("datetime")));

        await using var context = contextFactory.CreateDbContext();

        var dateTimes = new DateTime?[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00), null };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.Set<TestEntity>()
            .Where(m => dateTimes.Any(d => d == EF.Property<DateTime>(m, "DateTime") && d != null))
            .ToArrayAsync());
        Assert.Equal(RelationalStrings.ConflictingTypeMappingsInferredForColumn("Value"), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Same_collection_with_conflicting_type_mappings_not_supported()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(b =>
            {
                b.Property(typeof(DateTime), "DateTime").HasColumnType("datetime");
                b.Property(typeof(DateTime), "DateTime2").HasColumnType("datetime2");
            }));

        await using var context = contextFactory.CreateDbContext();

        var dateTimes = new[] { new DateTime(2020, 1, 1, 12, 30, 00), new DateTime(2020, 1, 2, 12, 30, 00) };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.Set<TestEntity>()
            .Where(m => dateTimes
                .Any(d => d == EF.Property<DateTime>(m, "DateTime") && d == EF.Property<DateTime>(m, "DateTime2")))
            .ToArrayAsync());
        Assert.Equal(RelationalStrings.ConflictingTypeMappingsInferredForColumn("Value"), exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Infer_inline_collection_type_mapping()
    {
        var contextFactory = await InitializeNonSharedTest<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>(b => b.Property<DateTime>("DateTime").HasColumnType("datetime")));

        await using var context = contextFactory.CreateDbContext();

        _ = await context.Set<TestEntity>()
            .Where(b => new[] { new DateTime(2020, 1, 1), EF.Property<DateTime>(b, "DateTime") }[0] == new DateTime(2020, 1, 1))
            .ToArrayAsync();

        AssertSql(
            """
SELECT [t].[Id], [t].[DateTime], [t].[Ints]
FROM [TestEntity] AS [t]
WHERE (
    SELECT [v].[Value]
    FROM (VALUES (0, CAST('2020-01-01T00:00:00.000' AS datetime)), (1, [t].[DateTime])) AS [v]([_ord], [Value])
    ORDER BY [v].[_ord]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = '2020-01-01T00:00:00.000'
""");
    }

    [ConditionalFact]
    public virtual async Task Ordered_collection_with_split_query()
    {
        var contextFactory = await InitializeNonSharedTest<Context32976>(
            onModelCreating: mb => mb.Entity<Context32976.Principal>(),
            seed: context =>
            {
                context.Add(new Context32976.Principal { Ints = [2, 3, 4] });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        _ = await context.Set<Context32976.Principal>()
            .Where(p => p.Ints.Skip(1).Contains(3))
            .Include(p => p.Dependents)
            .AsSplitQuery()
            .SingleAsync();
    }

    public class Context32976(DbContextOptions options) : DbContext(options)
    {
        public class Principal
        {
            public int Id { get; set; }
            public List<int> Ints { get; set; }
            public List<Dependent> Dependents { get; set; }
        }

        public class Dependent
        {
            public int Id { get; set; }
            public Principal Principal { get; set; }
        }
    }

    [ConditionalFact]
    public virtual async Task Parameter_collection_of_ints_Contains_int_2071_values()
    {
        var ints = Enumerable.Repeat(10, 2071).ToArray();
        await AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)));

        // check that 2071 parameter is the last one (and no error happened)
        Assert.Contains("@ints2071)", Fixture.TestSqlLoggerFactory.SqlStatements[0], StringComparison.Ordinal);
    }

    [ConditionalTheory]
    [InlineData(2098)]
    [InlineData(2099)]
    [InlineData(2100)]
    public virtual Task Parameter_collection_of_ints_Contains_int_parameters_limit(int count)
    {
        var ints = Enumerable.Range(10, count);

        // no exception from SQL Server is a pass
        return AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)));
    }

    [ConditionalTheory]
    [InlineData(2098)]
    [InlineData(2099)]
    [InlineData(2100)]
    public virtual Task Parameter_collection_Count_parameters_limit(int count)
    {
        var ids = Enumerable.Range(1000, count);

        // no exception from SQL Server is a pass
        return AssertQuery(ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ids.Count(i => i > c.Id) > 0));
    }

    protected override DbContextOptionsBuilder SetParameterizedCollectionMode(
        DbContextOptionsBuilder optionsBuilder,
        ParameterTranslationMode parameterizedCollectionMode)
    {
        new SqlServerDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);

        return optionsBuilder;
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private PrimitiveCollectionsContext CreateContext()
        => Fixture.CreateContext();

    public class PrimitiveCollectionsQuerySqlServerFixture : PrimitiveCollectionsQueryFixtureBase, ITestSqlLoggerFactory
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
