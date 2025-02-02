// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

public class PrimitiveCollectionsQueryCosmosTest : PrimitiveCollectionsQueryTestBase<
    PrimitiveCollectionsQueryCosmosTest.PrimitiveCollectionsQueryCosmosFixture>
{
    public PrimitiveCollectionsQueryCosmosTest(PrimitiveCollectionsQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Inline_collection_of_ints_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_of_ints_Contains(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Int"] IN (10, 999)
""");
            });

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override Task Inline_collection_of_nullable_ints_Contains(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Inline_collection_of_nullable_ints_Contains(a);
//
//                 AssertSql(
//                     """
// SELECT VALUE c
// FROM root c
// WHERE c["NullableInt"] IN (10, 999)
// """);
//             });
//
//     public override Task Inline_collection_of_nullable_ints_Contains_null(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Inline_collection_of_nullable_ints_Contains_null(a);
//
//                 AssertSql(
//                     """
// SELECT VALUE c
// FROM root c
// WHERE c["NullableInt"] IN (null, 999)
// """);
//             });

    public override Task Inline_collection_Count_with_zero_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_zero_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [])
    WHERE (a > c["Id"])) = 1)
""");
            });

    public override Task Inline_collection_Count_with_one_value(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_one_value(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [2])
    WHERE (a > c["Id"])) = 1)
""");
            });

    public override Task Inline_collection_Count_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_two_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [2, 999])
    WHERE (a > c["Id"])) = 1)
""");
            });

    public override Task Inline_collection_Count_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_three_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [2, 999, 1000])
    WHERE (a > c["Id"])) = 2)
""");
            });

    public override Task Inline_collection_Contains_with_zero_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_zero_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Inline_collection_Contains_with_one_value(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_one_value(a);

                AssertSql("ReadItem(None, 2)");
            });

    public override Task Inline_collection_Contains_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_two_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, 999)
""");
            });

    public override Task Inline_collection_Contains_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_three_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, 999, 1000)
""");
            });

    public override Task Inline_collection_Contains_with_all_parameters(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_all_parameters(a);

                AssertSql(
                    """
@i='2'
@j='999'

SELECT VALUE c
FROM root c
WHERE c["Id"] IN (@i, @j)
""");
            });

    public override Task Inline_collection_Contains_with_constant_and_parameter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_constant_and_parameter(a);

                AssertSql(
                    """
@j='999'

SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, @j)
""");
            });

    public override Task Inline_collection_Contains_with_mixed_value_types(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_mixed_value_types(a);

                AssertSql(
                    """
@i='11'

SELECT VALUE c
FROM root c
WHERE c["Int"] IN (999, @i, c["Id"], (c["Id"] + c["Int"]))
""");
            });

    public override Task Inline_collection_List_Contains_with_mixed_value_types(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Contains_with_mixed_value_types(a);

                AssertSql(
                    """
@i='11'

SELECT VALUE c
FROM root c
WHERE c["Int"] IN (999, @i, c["Id"], (c["Id"] + c["Int"]))
""");
            });

    public override Task Inline_collection_Contains_as_Any_with_predicate(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_as_Any_with_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, 999)
""");
            });

    public override Task Inline_collection_negated_Contains_as_All(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_negated_Contains_as_All(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Id"] NOT IN (2, 999)
""");
            });

    public override Task Inline_collection_Min_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Min_with_two_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
            });

    public override Task Inline_collection_List_Min_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Min_with_two_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
            });

    public override Task Inline_collection_Max_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Max_with_two_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
            });

    public override Task Inline_collection_List_Max_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Max_with_two_values(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
            });

    public override Task Inline_collection_Min_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Min_with_three_values(a);

                AssertSql(
                    """
@i='25'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 25)
""");
            });

    public override Task Inline_collection_List_Min_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Min_with_three_values(a);

                AssertSql(
                    """
@i='25'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 25)
""");
            });

    public override Task Inline_collection_Max_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Max_with_three_values(a);

                AssertSql(
                    """
@i='35'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 35)
""");
            });

    public override Task Inline_collection_List_Max_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Max_with_three_values(a);

                AssertSql(
                    """
@i='35'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 35)
""");
            });

    public override Task Inline_collection_of_nullable_value_type_Min(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_of_nullable_value_type_Min(a);

                AssertSql(
                    """
@i='25'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 25)
""");
            });

    public override Task Inline_collection_of_nullable_value_type_Max(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_of_nullable_value_type_Max(a);

                AssertSql(
                    """
@i='35'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 35)
""");
            });

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Cosmos MIN()/MAX() sort nulls as smaller than ints (https://learn.microsoft.com/azure/cosmos-db/nosql/query/min);
            // since some of the columns included contain null, MIN() returns null as opposed to the smallest number.
            // In relational, aggregate MIN()/MAX() ignores nulls.
            await Assert.ThrowsAsync<EqualException>(() => base.Inline_collection_of_nullable_value_type_with_null_Min(async));

            AssertSql(
                """
@i=null

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["NullableInt"], @i])) = 30)
""");
        }
    }

    public override Task Inline_collection_of_nullable_value_type_with_null_Max(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_of_nullable_value_type_with_null_Max(a);

                AssertSql(
                    """
@i=null

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["NullableInt"], @i])) = 30)
""");
            });

    public override Task Inline_collection_with_single_parameter_element_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_with_single_parameter_element_Contains(a);

                AssertSql(
                    """
ReadItem(None, 2)
""");
            });

    public override Task Inline_collection_with_single_parameter_element_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_with_single_parameter_element_Count(a);

                AssertSql(
                    """
@i='2'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [@i])
    WHERE (a > c["Id"])) = 1)
""");
            });

    public override Task Inline_collection_Contains_with_EF_Parameter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_EF_Parameter(async);

                AssertSql(
                    """
@p='[2,999,1000]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@p, c["Id"])
""");
            });

    public override Task Inline_collection_Count_with_column_predicate_with_EF_Parameter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_column_predicate_with_EF_Parameter(async);

                AssertSql(
                    """
@p='[2,999,1000]'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM p IN (SELECT VALUE @p)
    WHERE (p > c["Id"])) = 2)
""");
            });

    public override Task Parameter_collection_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_Count(a);

                AssertSql(
                    """
@ids='[2,999]'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE @ids)
    WHERE (i > c["Id"])) = 1)
""");
            });

    public override Task Parameter_collection_of_ints_Contains_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_ints_Contains_int(a);

                AssertSql(
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ints, c["Int"])
""",
                    //
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ints, c["Int"]))
""");
            });

    public override Task Parameter_collection_HashSet_of_ints_Contains_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_HashSet_of_ints_Contains_int(a);

                AssertSql(
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ints, c["Int"])
""",
                    //
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ints, c["Int"]))
""");
            });

    public override Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_ImmutableArray_of_ints_Contains_int(a);

                AssertSql(
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ints, c["Int"])
""",
                    //
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ints, c["Int"]))
""");
            });

    public override Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_ints_Contains_nullable_int(a);

                AssertSql(
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ints, c["NullableInt"])
""",
                    //
                    """
@ints='[10,999]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@ints, c["NullableInt"]))
""");
            });

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Parameter_collection_of_nullable_ints_Contains_int(a);
//
//                 AssertSql(
//                     """
// @nullableInts='[10,999]'
//
// SELECT VALUE c
// FROM root c
// WHERE ARRAY_CONTAINS(@nullableInts, c["Int"])
// """,
//                     //
//                     """
// @nullableInts='[10,999]'
//
// SELECT VALUE c
// FROM root c
// WHERE NOT(ARRAY_CONTAINS(@nullableInts, c["Int"]))
// """);
//             });
//
//     public override Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(a);
//
//                 AssertSql(
//                     """
// @nullableInts='[null,999]'
//
// SELECT VALUE c
// FROM root c
// WHERE ARRAY_CONTAINS(@nullableInts, c["NullableInt"])
// """,
//                     //
//                     """
// @nullableInts='[null,999]'
//
// SELECT VALUE c
// FROM root c
// WHERE NOT(ARRAY_CONTAINS(@nullableInts, c["NullableInt"]))
// """);
//             });

    public override Task Parameter_collection_of_strings_Contains_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_strings_Contains_string(a);

                AssertSql(
                    """
@strings='["10","999"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, c["String"])
""",
                    //
                    """
@strings='["10","999"]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@strings, c["String"]))
""");
            });

    public override Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_strings_Contains_nullable_string(a);

                AssertSql(
                    """
@strings='["10","999"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, c["NullableString"])
""",
                    //
                    """
@strings='["10","999"]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@strings, c["NullableString"]))
""");
            });

    public override Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_nullable_strings_Contains_string(a);

                AssertSql(
                    """
@strings='["10",null]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, c["String"])
""",
                    //
                    """
@strings='["10",null]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@strings, c["String"]))
""");
            });

    public override Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(a);

                AssertSql(
                    """
@strings='["999",null]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, c["NullableString"])
""",
                    //
                    """
@strings='["999",null]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@strings, c["NullableString"]))
""");
            });

    public override Task Parameter_collection_of_DateTimes_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_DateTimes_Contains(a);

                AssertSql(
                    """
@dateTimes='["2020-01-10T12:30:00Z","9999-01-01T00:00:00Z"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@dateTimes, c["DateTime"])
""");
            });

    public override Task Parameter_collection_of_bools_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_bools_Contains(a);

                AssertSql(
                    """
@bools='[true]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@bools, c["Bool"])
""");
            });

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override Task Parameter_collection_of_enums_Contains(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Parameter_collection_of_enums_Contains(a);
//
//                 AssertSql(
//                     """
// @enums='[0,3]'
//
// SELECT VALUE c
// FROM root c
// WHERE ARRAY_CONTAINS(@enums, c["Enum"])
// """);
//             });

    public override Task Parameter_collection_null_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_null_Contains(a);

                AssertSql(
                    """
@ints=null

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ints, c["Int"])
""");
            });

    public override async Task Parameter_collection_Contains_with_EF_Constant(bool async)
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Parameter_collection_Contains_with_EF_Constant(async));
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any(bool async)
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Parameter_collection_Where_with_EF_Constant_Where_Any(async));
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_EF_Constant(bool async)
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Parameter_collection_Count_with_column_predicate_with_EF_Constant(async));
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override Task Column_collection_of_ints_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_ints_Contains(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["Ints"], 10)
""");
            });

// TODO: The base implementations no longer compile since https://github.com/dotnet/runtime/pull/110197 (Contains overload added with
// optional parameter, not supported in expression trees). #35547 is tracking on the EF side.
//
//     public override Task Column_collection_of_nullable_ints_Contains(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Column_collection_of_nullable_ints_Contains(a);
//
//                 AssertSql(
//                     """
// SELECT VALUE c
// FROM root c
// WHERE ARRAY_CONTAINS(c["NullableInts"], 10)
// """);
//             });
//
//     public override Task Column_collection_of_nullable_ints_Contains_null(bool async)
//         => CosmosTestHelpers.Instance.NoSyncTest(
//             async, async a =>
//             {
//                 await base.Column_collection_of_nullable_ints_Contains_null(a);
//
//                 AssertSql(
//                     """
// SELECT VALUE c
// FROM root c
// WHERE ARRAY_CONTAINS(c["NullableInts"], null)
// """);
//             });

    public override Task Column_collection_of_strings_contains_null(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_strings_contains_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["Strings"], null)
""");
            });

    public override Task Column_collection_of_nullable_strings_contains_null(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_nullable_strings_contains_null(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["NullableStrings"], null)
""");
            });

    public override Task Column_collection_of_bools_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_bools_Contains(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["Bools"], true)
""");
            });

    public override Task Column_collection_Count_method(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Count_method(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["Ints"]) = 2)
""");
            });

    public override Task Column_collection_Length(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Length(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["Ints"]) = 2)
""");
            });

    public override Task Column_collection_Count_with_predicate(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Count_with_predicate(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM i IN c["Ints"]
    WHERE (i > 1)) = 2)
""");
            });

    public override Task Column_collection_Where_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_Count(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM i IN c["Ints"]
    WHERE (i > 1)) = 2)
""");
            });

    public override Task Column_collection_index_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_int(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][1] = 10)
""");
            });

    public override Task Column_collection_index_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_string(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Strings"][1] = "10")
""");
            });

    public override Task Column_collection_index_datetime(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_datetime(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["DateTimes"][1] = "2020-01-10T12:30:00Z")
""");
            });

    public override Task Column_collection_index_beyond_end(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_beyond_end(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][999] = 10)
""");
            });

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<EqualException>(() => base.Nullable_reference_column_collection_index_equals_nullable_column(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["NullableStrings"][2] = c["NullableString"])
""");
        }
    }

    public override Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Non_nullable_reference_column_collection_index_equals_nullable_column(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((ARRAY_LENGTH(c["Strings"]) > 0) AND (c["Strings"][1] = c["NullableString"]))
""");
            });

    public override async Task Inline_collection_index_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Inline_collection_index_Column(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ([1, 2, 3][c["Int"]] = 1)
""");
        }
    }

    public override async Task Inline_collection_value_index_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Inline_collection_value_index_Column(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ([1, c["Int"], 3][c["Int"]] = 1)
""");
        }
    }

    public override async Task Inline_collection_List_value_index_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Inline_collection_List_value_index_Column(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ([1, c["Int"], 3][c["Int"]] = 1)
""");
        }
    }

    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Parameter_collection_index_Column_equal_Column(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
@ints='[0,2,3]'

SELECT VALUE c
FROM root c
WHERE (@ints[c["Int"]] = c["Int"])
""");
        }
    }

    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Parameter_collection_index_Column_equal_constant(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
@ints='[1,2,3]'

SELECT VALUE c
FROM root c
WHERE (@ints[c["Int"]] = 1)
""");
        }
    }

    public override Task Column_collection_ElementAt(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_ElementAt(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][1] = 10)
""");
            });

    public override Task Column_collection_First(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_First(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][0] = 1)
""");
            });

    public override Task Column_collection_FirstOrDefault(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_FirstOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Ints"][0] ?? 0) = 1)
""");
            });

    public override Task Column_collection_Single(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Single(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][0] = 1)
""");
            });

    public override Task Column_collection_SingleOrDefault(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_SingleOrDefault(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Ints"][0] ?? 0) = 1)
""");
            });

    public override Task Column_collection_Skip(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Skip(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(c["Ints"], 1)) = 2)
""");
            });

    public override Task Column_collection_Take(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Take(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(ARRAY_SLICE(c["Ints"], 0, 2), 11)
""");
            });

    public override Task Column_collection_Skip_Take(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Skip_Take(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(ARRAY_SLICE(c["Ints"], 1, 2), 11)
""");
            });

    public override Task Column_collection_Where_Skip(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_Skip(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)), 1)) = 3)
""");
            });

    public override Task Column_collection_Where_Take(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_Take(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)), 0, 2)) = 2)
""");
            });

    public override Task Column_collection_Where_Skip_Take(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_Skip_Take(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)), 1, 2)) = 1)
""");
            });

    public override Task Column_collection_Contains_over_subquery(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Contains_over_subquery(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM i IN c["Ints"]
    WHERE ((i > 1) AND (i = 11)))
""");
            });

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // 'ORDER BY' is not supported in subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Column_collection_OrderByDescending_ElementAt(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    ORDER BY i DESC)[0] = 111)
""");
        }
    }

    public override Task Column_collection_Where_ElementAt(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_ElementAt(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1))[0] = 11)
""");
            });

    public override Task Column_collection_Any(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Any(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["Ints"]) > 0)
""");
            });

    public override async Task Column_collection_Distinct(bool async)
    {
        // TODO: Subquery pushdown, #33968
        await AssertTranslationFailed(() => base.Column_collection_Distinct(async));

        AssertSql();
    }

    public override Task Column_collection_SelectMany(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_SelectMany(a);

                AssertSql(
                    """
SELECT VALUE i
FROM root c
JOIN i IN c["Ints"]
""");
            });

    public override Task Column_collection_SelectMany_with_filter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_SelectMany_with_filter(a);

                AssertSql(
                    """
SELECT VALUE j
FROM root c
JOIN (
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)) j
""");
            });

    public override async Task Column_collection_SelectMany_with_Select_to_anonymous_type(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // TODO: #34004
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Column_collection_SelectMany_with_Select_to_anonymous_type(async));

            Assert.Equal(CosmosStrings.ComplexProjectionInSubqueryNotSupported, exception.Message);
        }
    }

    public override Task Column_collection_projection_from_top_level(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_projection_from_top_level(a);

                AssertSql(
                    """
SELECT VALUE c["Ints"]
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Column_collection_Join_parameter_collection(bool async)
    {
        // Cosmos join support. Issue #16920.
        await AssertTranslationFailed(() => base.Column_collection_Join_parameter_collection(async));

        AssertSql();
    }

    public override async Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        // Cosmos join support. Issue #16920.
        await AssertTranslationFailed(() => base.Column_collection_Join_parameter_collection(async));

        AssertSql();
    }

    public override Task Parameter_collection_Concat_column_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_Concat_column_collection(a);

                AssertSql(
                    """
@ints='[11,111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_CONCAT(@ints, c["Ints"])) = 2)
""");
            });

    public override async Task Parameter_collection_with_type_inference_for_JsonScalarExpression(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
            var exception = await Assert.ThrowsAsync<CosmosException>(
                () => base.Parameter_collection_with_type_inference_for_JsonScalarExpression(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
@values='["one","two"]'

SELECT VALUE ((c["Id"] != 0) ? @values[(c["Int"] % 2)] : "foo")
FROM root c
""");
        }
    }

    public override Task Column_collection_Union_parameter_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Union_parameter_collection(a);

                AssertSql(
                    """
@ints='[11,111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(c["Ints"], @ints)) = 2)
""");
            });

    public override Task Column_collection_Intersect_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Intersect_inline_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetIntersect(c["Ints"], [11, 111])) = 2)
""");
            });

    public override async Task Inline_collection_Except_column_collection(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Inline_collection_Except_column_collection(async),
            CosmosStrings.ExceptNotSupported);

        AssertSql();
    }

    public override Task Column_collection_Where_Union(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_Union(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 100)), [50])) = 2)
""");
            });

    public override Task Column_collection_equality_parameter_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_equality_parameter_collection(a);

                AssertSql(
                    """
@ints='[1,10]'

SELECT VALUE c
FROM root c
WHERE (c["Ints"] = @ints)
""");
            });

    public override Task Column_collection_Concat_parameter_collection_equality_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Concat_parameter_collection_equality_inline_collection(a);

                AssertSql(
                    """
@ints='[1,10]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONCAT(c["Ints"], @ints) = [1,11,111,1,10])
""");
            });

    public override Task Column_collection_equality_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_equality_inline_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Ints"] = [1,10])
""");
            });

    public override Task Column_collection_equality_inline_collection_with_parameters(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_equality_inline_collection_with_parameters(a);

                AssertSql(
                    """
@i='1'
@j='10'

SELECT VALUE c
FROM root c
WHERE (c["Ints"] = [@i, @j])
""");
            });

    public override Task Column_collection_Where_equality_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_equality_inline_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i != 11)) = [1,111])
""");
            });

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(a));

        AssertSql();
    }

    public override Task Parameter_collection_in_subquery_Union_column_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_in_subquery_Union_column_collection(a);

                AssertSql(
                    """
@Skip='[111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(@Skip, c["Ints"])) = 3)
""");
            });

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        // TODO: Subquery pushdown
        await AssertTranslationFailed(() => base.Parameter_collection_in_subquery_Union_column_collection_nested(async));

        AssertSql();
    }

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        // Array indexer over a parameter array ([1,2,3][0]) isn't supported by Cosmos.
        // TODO: general OFFSET/LIMIT support
        AssertTranslationFailed(() => base.Parameter_collection_in_subquery_and_Convert_as_compiled_query());

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Parameter_collection_in_subquery_Count_as_compiled_query(a));

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(a));

        AssertSql();
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Column_collection_in_subquery_Union_parameter_collection(a));

        AssertSql();
    }

    public override Task Project_collection_of_ints_simple(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_ints_simple(a);

                AssertSql(
                    """
SELECT VALUE c["Ints"]
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Project_collection_of_ints_ordered(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // 'ORDER BY' is not supported in subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Project_collection_of_ints_ordered(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    ORDER BY i DESC)
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override Task Project_collection_of_datetimes_filtered(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_datetimes_filtered(a);

                AssertSql(
                    """
SELECT VALUE ARRAY(
    SELECT VALUE d
    FROM d IN c["DateTimes"]
    WHERE (DateTimePart("dd", d) != 1))
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Project_collection_of_nullable_ints_with_paging(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Project_collection_of_nullable_ints_with_paging(a));

        AssertSql();
    }

    public override async Task Project_collection_of_nullable_ints_with_paging2(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Project_collection_of_nullable_ints_with_paging2(a));

        AssertSql();
    }

    public override async Task Project_collection_of_nullable_ints_with_paging3(bool async)
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        await CosmosTestHelpers.Instance.NoSyncTest(
            async: false, a => base.Project_collection_of_nullable_ints_with_paging3(a));

        AssertSql();
    }

    public override Task Project_collection_of_ints_with_distinct(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_ints_with_distinct(a);

                AssertSql(
                    """
SELECT VALUE ARRAY(
    SELECT DISTINCT VALUE i
    FROM i IN c["Ints"])
FROM root c
ORDER BY c["Id"]
""");
            });

    public override Task Project_collection_of_nullable_ints_with_distinct(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_nullable_ints_with_distinct(a);

                AssertSql(
                    """
SELECT VALUE {"c" : [c["String"], "foo"]}
FROM root c
WHERE (c["$type"] = "PrimitiveCollectionsEntity")
""");
            });

    public override Task Project_collection_of_ints_with_ToList_and_FirstOrDefault(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_ints_with_ToList_and_FirstOrDefault(a);

                AssertSql(
                    """
SELECT VALUE ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"])
FROM root c
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(a);

                AssertSql(
                    """
SELECT VALUE
{
    "c" : ARRAY(
        SELECT VALUE n
        FROM n IN c["NullableInts"]
        WHERE false),
    "c0" : ARRAY(
        SELECT VALUE n0
        FROM n0 IN c["NullableInts"]
        WHERE (n0 = null))
}
FROM root c
ORDER BY c["Id"]
""");
            });

    public override async Task Project_multiple_collections(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Project_multiple_collections(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE
{
    "c" : ARRAY(
        SELECT VALUE i
        FROM i IN c["Ints"]),
    "c0" : ARRAY(
        SELECT VALUE i0
        FROM i0 IN c["Ints"]
        ORDER BY i0 DESC),
    "c1" : ARRAY(
        SELECT VALUE d
        FROM d IN c["DateTimes"]
        WHERE (DateTimePart("dd", d) != 1)),
    "c2" : ARRAY(
        SELECT VALUE d0
        FROM d0 IN c["DateTimes"]
        WHERE (d0 > "2000-01-01T00:00:00"))
}
FROM root c
ORDER BY c["Id"]
""");
        }
    }

    public override Task Project_primitive_collections_element(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_primitive_collections_element(a);

                AssertSql(
                    """
SELECT VALUE
{
    "Indexer" : c["Ints"][0],
    "EnumerableElementAt" : c["DateTimes"][0],
    "QueryableElementAt" : c["Strings"][1]
}
FROM root c
WHERE (c["Id"] < 4)
ORDER BY c["Id"]
""");
            });

    public override Task Project_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_inline_collection(a);

                // The following should be SELECT VALUE [c["String"], "foo"], #33779
                AssertSql(
                    """
SELECT VALUE [c["String"], "foo"]
FROM root c
""");
            });

    // Non-correlated queries not supported by Cosmos
    public override async Task Project_inline_collection_with_Union(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await AssertTranslationFailed(() => base.Project_inline_collection_with_Union(async));
        }
    }

    // Non-correlated queries not supported by Cosmos
    public override async Task Project_inline_collection_with_Concat(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await AssertTranslationFailed(() => base.Project_inline_collection_with_Concat(async));
        }
    }

    public override Task Nested_contains_with_Lists_and_no_inferred_type_mapping(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Nested_contains_with_Lists_and_no_inferred_type_mapping(a);

                AssertSql(
                    """
@strings='["one","two","three"]'
@ints='[1,2,3]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, (ARRAY_CONTAINS(@ints, c["Int"]) ? "one" : "two"))
""");
            });

    public override Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Nested_contains_with_arrays_and_no_inferred_type_mapping(a);

                AssertSql(
                    """
@strings='["one","two","three"]'
@ints='[1,2,3]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, (ARRAY_CONTAINS(@ints, c["Int"]) ? "one" : "two"))
""");
            });

    #region Cosmos-specific tests

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsDefined(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => EF.Functions.IsDefined(e.Ints[2])),
                    ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => e.Ints.Length >= 3));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE IS_DEFINED(c["Ints"][2])
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task CoalesceUndefined(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await AssertQuery(
                    a,
                    ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => EF.Functions.CoalesceUndefined(e.Ints[2], 999) == 999),
                    ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => e.Ints.Length < 3));

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Ints"][2] ?? 999) = 999)
""");
            });

    #endregion Cosmos-specific tests

    public override async Task Parameter_collection_of_structs_Contains_struct(bool async)
    {
        // Always throws for sync before getting to the exception to test.
        if (async)
        {
            // Requires collections of converted elements
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Parameter_collection_of_structs_Contains_struct(async));

            AssertSql();
        }
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct(bool async)
    {
        // Always throws for sync before getting to the exception to test.
        if (async)
        {
            // Requires collections of converted elements
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Parameter_collection_of_structs_Contains_nullable_struct(async));

            AssertSql();
        }
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        // Always throws for sync before getting to the exception to test.
        if (async)
        {
            // Requires collections of converted elements
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(async));

            AssertSql();
        }
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct(bool async)
    {
        // Always throws for sync before getting to the exception to test.
        if (async)
        {
            // Requires collections of converted elements
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Parameter_collection_of_nullable_structs_Contains_struct(async));

            AssertSql();
        }
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct(bool async)
    {
        // Always throws for sync before getting to the exception to test.
        if (async)
        {
            // Requires collections of converted elements
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Parameter_collection_of_nullable_structs_Contains_nullable_struct(async));

            AssertSql();
        }
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        // Always throws for sync before getting to the exception to test.
        if (async)
        {
            // Requires collections of converted elements
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(async));

            AssertSql();
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public class PrimitiveCollectionsQueryCosmosFixture : PrimitiveCollectionsQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(
                builder.ConfigureWarnings(
                    w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // Requires element type mapping; Issue #34026
            modelBuilder.Entity<PrimitiveCollectionsEntity>().Ignore(e => e.Enums);
        }
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
