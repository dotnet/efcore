// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Int"] IN (10, 999))
""");
            });

    public override Task Inline_collection_of_nullable_ints_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_of_nullable_ints_Contains(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["NullableInt"] IN (10, 999))
""");
            });

    public override Task Inline_collection_of_nullable_ints_Contains_null(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_of_nullable_ints_Contains_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["NullableInt"] IN (null, 999))
""");
            });

    public override Task Inline_collection_Count_with_zero_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_zero_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE [])
    WHERE (i > c["Id"])) = 1))
""");
            });

    public override Task Inline_collection_Count_with_one_value(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_one_value(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE [2])
    WHERE (i > c["Id"])) = 1))
""");
            });

    public override Task Inline_collection_Count_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_two_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE [2, 999])
    WHERE (i > c["Id"])) = 1))
""");
            });

    public override Task Inline_collection_Count_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Count_with_three_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE [2, 999, 1000])
    WHERE (i > c["Id"])) = 2))
""");
            });

    public override Task Inline_collection_Contains_with_zero_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_zero_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND false)
""");
            });

    public override Task Inline_collection_Contains_with_one_value(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_one_value(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (2))
""");
            });

    public override Task Inline_collection_Contains_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_two_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (2, 999))
""");
            });

    public override Task Inline_collection_Contains_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_three_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (2, 999, 1000))
""");
            });

    public override Task Inline_collection_Contains_with_EF_Constant(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_EF_Constant(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (2, 999, 1000))
""");
            });

    public override Task Inline_collection_Contains_with_all_parameters(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_all_parameters(a);

                AssertSql(
                    """
@__i_0='2'
@__j_1='999'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (@__i_0, @__j_1))
""");
            });

    public override Task Inline_collection_Contains_with_constant_and_parameter(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_constant_and_parameter(a);

                AssertSql(
                    """
@__j_0='999'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (2, @__j_0))
""");
            });

    public override Task Inline_collection_Contains_with_mixed_value_types(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_with_mixed_value_types(a);

                AssertSql(
                    """
@__i_0='11'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Int"] IN (999, @__i_0, c["Id"], (c["Id"] + c["Int"])))
""");
            });

    public override Task Inline_collection_List_Contains_with_mixed_value_types(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Contains_with_mixed_value_types(a);

                AssertSql(
                    """
@__i_0='11'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Int"] IN (999, @__i_0, c["Id"], (c["Id"] + c["Int"])))
""");
            });

    public override Task Inline_collection_Contains_as_Any_with_predicate(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Contains_as_Any_with_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] IN (2, 999))
""");
            });

    public override Task Inline_collection_negated_Contains_as_All(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_negated_Contains_as_All(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND c["Id"] NOT IN (2, 999))
""");
            });

    public override Task Inline_collection_Min_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Min_with_two_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MIN(i)
    FROM i IN (SELECT VALUE [30, c["Int"]])) = 30))
""");
            });

    public override Task Inline_collection_List_Min_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Min_with_two_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MIN(i)
    FROM i IN (SELECT VALUE [30, c["Int"]])) = 30))
""");
            });

    public override Task Inline_collection_Max_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Max_with_two_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MAX(i)
    FROM i IN (SELECT VALUE [30, c["Int"]])) = 30))
""");
            });

    public override Task Inline_collection_List_Max_with_two_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Max_with_two_values(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MAX(i)
    FROM i IN (SELECT VALUE [30, c["Int"]])) = 30))
""");
            });

    public override Task Inline_collection_Min_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_Min_with_three_values(a);

                AssertSql(
                    """
@__i_0='25'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MIN(i)
    FROM i IN (SELECT VALUE [30, c["Int"], @__i_0])) = 25))
""");
            });

    public override Task Inline_collection_List_Min_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Min_with_three_values(a);

                AssertSql(
                    """
@__i_0='25'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MIN(i)
    FROM i IN (SELECT VALUE [30, c["Int"], @__i_0])) = 25))
""");
            });

    public override Task Inline_collection_Max_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
        await base.Inline_collection_Max_with_three_values(a);

        AssertSql(
            """
@__i_0='35'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MAX(i)
    FROM i IN (SELECT VALUE [30, c["Int"], @__i_0])) = 35))
""");
            });

    public override Task Inline_collection_List_Max_with_three_values(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Inline_collection_List_Max_with_three_values(a);

                AssertSql(
                    """
@__i_0='35'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE MAX(i)
    FROM i IN (SELECT VALUE [30, c["Int"], @__i_0])) = 35))
""");
            });

    public override Task Parameter_collection_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_Count(a);

                AssertSql(
                    """
@__ids_0='[2,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ((
    SELECT VALUE COUNT(1)
    FROM i IN (SELECT VALUE @__ids_0)
    WHERE (i > c["Id"])) = 1))
""");
            });

    public override Task Parameter_collection_of_ints_Contains_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_ints_Contains_int(a);

                AssertSql(
                    """
@__ints_0='[10,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__ints_0, c["Int"]))
""",
                    //
                    """
@__ints_0='[10,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__ints_0, c["Int"])))
""");
            });

    public override Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_ints_Contains_nullable_int(a);

                AssertSql(
                    """
@__ints_0='[10,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__ints_0, c["NullableInt"]))
""",
                    //
                    """
@__ints_0='[10,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__ints_0, c["NullableInt"])))
""");
            });

    public override Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_nullable_ints_Contains_int(a);

                AssertSql(
                    """
@__nullableInts_0='[10,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__nullableInts_0, c["Int"]))
""",
                    //
                    """
@__nullableInts_0='[10,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__nullableInts_0, c["Int"])))
""");
            });

    public override Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(a);

                AssertSql(
                    """
@__nullableInts_0='[null,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__nullableInts_0, c["NullableInt"]))
""",
                    //
                    """
@__nullableInts_0='[null,999]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__nullableInts_0, c["NullableInt"])))
""");
            });

    public override Task Parameter_collection_of_strings_Contains_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_strings_Contains_string(a);

                AssertSql(
                    """
@__strings_0='["10","999"]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__strings_0, c["String"]))
""",
                    //
                    """
@__strings_0='["10","999"]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__strings_0, c["String"])))
""");
            });

    public override Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_strings_Contains_nullable_string(a);

                AssertSql(
                    """
@__strings_0='["10","999"]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__strings_0, c["NullableString"]))
""",
                    //
                    """
@__strings_0='["10","999"]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__strings_0, c["NullableString"])))
""");
            });

    public override Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_nullable_strings_Contains_string(a);

                AssertSql(
                    """
@__strings_0='["10",null]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__strings_0, c["String"]))
""",
                    //
                    """
@__strings_0='["10",null]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__strings_0, c["String"])))
""");
            });

    public override Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(a);

                AssertSql(
                    """
@__strings_0='["999",null]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__strings_0, c["NullableString"]))
""",
                    //
                    """
@__strings_0='["999",null]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND NOT(ARRAY_CONTAINS(@__strings_0, c["NullableString"])))
""");
            });

    public override Task Parameter_collection_of_DateTimes_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_DateTimes_Contains(a);

                AssertSql(
                    """
@__dateTimes_0='["2020-01-10T12:30:00Z","9999-01-01T00:00:00Z"]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__dateTimes_0, c["DateTime"]))
""");
            });

    public override Task Parameter_collection_of_bools_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_bools_Contains(a);

                AssertSql(
                    """
@__bools_0='[true]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__bools_0, c["Bool"]))
""");
            });

    public override Task Parameter_collection_of_enums_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_of_enums_Contains(a);

                AssertSql(
                    """
@__enums_0='[0,3]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__enums_0, c["Enum"]))
""");
            });

    public override Task Parameter_collection_null_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_null_Contains(a);

                AssertSql(
                    """
@__ints_0=null

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__ints_0, c["Int"]))
""");
            });

    public override Task Column_collection_of_ints_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_ints_Contains(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(c["Ints"], 10))
""");
            });

    public override Task Column_collection_of_nullable_ints_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_nullable_ints_Contains(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(c["NullableInts"], 10))
""");
            });

    public override Task Column_collection_of_nullable_ints_Contains_null(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_nullable_ints_Contains_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(c["NullableInts"], null))
""");
            });

    public override Task Column_collection_of_strings_contains_null(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_strings_contains_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(c["Strings"], null))
""");
            });

    public override Task Column_collection_of_nullable_strings_contains_null(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_nullable_strings_contains_null(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(c["NullableStrings"], null))
""");
            });

    public override Task Column_collection_of_bools_Contains(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_of_bools_Contains(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(c["Bools"], true))
""");
            });

    public override Task Column_collection_Count_method(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Count_method(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(c["Ints"]) = 2))
""");
            });

    public override Task Column_collection_Length(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Length(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(c["Ints"]) = 2))
""");
            });

    public override Task Column_collection_index_int(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_int(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Ints"][1] = 10))
""");
            });

    public override Task Column_collection_index_string(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_string(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Strings"][1] = "10"))
""");
            });

    public override Task Column_collection_index_datetime(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_datetime(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["DateTimes"][1] = "2020-01-10T12:30:00Z"))
""");
            });

    public override Task Column_collection_index_beyond_end(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_index_beyond_end(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Ints"][999] = 10))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["NullableStrings"][2] = c["NullableString"]))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (EXISTS (
    SELECT 1
    FROM i IN c["Strings"]) AND (c["Strings"][1] = c["NullableString"])))
""");
            });

    public override async Task Inline_collection_index_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos, and neither is LIMIT/OFFSET within subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Inline_collection_index_Column(async));

            Assert.Contains("The specified query includes 'member indexer' which is currently not supported.", exception.Message);
        }
    }

    public override async Task Inline_collection_value_index_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos, and neither is LIMIT/OFFSET within subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Inline_collection_value_index_Column(async));

            Assert.Contains("The specified query includes 'member indexer' which is currently not supported.", exception.Message);
        }
    }

    public override async Task Inline_collection_List_value_index_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos, and neither is LIMIT/OFFSET within subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Inline_collection_List_value_index_Column(async));

            Assert.Contains("The specified query includes 'member indexer' which is currently not supported.", exception.Message);
        }
    }

    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos, and neither is LIMIT/OFFSET within subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Parameter_collection_index_Column_equal_Column(async));

            Assert.Contains("The specified query includes 'member indexer' which is currently not supported.", exception.Message);
        }
    }

    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos, and neither is LIMIT/OFFSET within subqueries.
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Parameter_collection_index_Column_equal_constant(async));

            Assert.Contains("The specified query includes 'member indexer' which is currently not supported.", exception.Message);
        }
    }

    public override Task Column_collection_ElementAt(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_ElementAt(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Ints"][1] = 10))
""");
            });

    public override async Task Column_collection_Skip(bool async)
    {
        // TODO: Count after Distinct requires subquery pushdown
        await AssertTranslationFailed(() => base.Column_collection_Skip(async));

        AssertSql();
    }

    public override async Task Column_collection_Take(bool async)
    {
        // TODO: IN with subquery
        await AssertTranslationFailed(() => base.Column_collection_Take(async));

        AssertSql();
    }

    public override async Task Column_collection_Skip_Take(bool async)
    {
        // TODO: Count after Distinct requires subquery pushdown
        await AssertTranslationFailed(() => base.Column_collection_Skip_Take(async));

        AssertSql();
    }

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        // TODO: ElementAt over composed query (non-simple array)
        await AssertTranslationFailed(() => base.Column_collection_OrderByDescending_ElementAt(async));

        AssertSql();
    }

    public override Task Column_collection_Any(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Any(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND EXISTS (
    SELECT 1
    FROM i IN c["Ints"]))
""");
            });

    public override async Task Column_collection_Distinct(bool async)
    {
        // TODO: Count after Distinct requires subquery pushdown
        await AssertTranslationFailed(() => base.Column_collection_Distinct(async));

        AssertSql();
    }

    public override async Task Column_collection_SelectMany(bool async)
    {
        // TODO: SelectMany
        await AssertTranslationFailed(() => base.Column_collection_SelectMany(async));

        AssertSql();
    }

    public override Task Column_collection_projection_from_top_level(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_projection_from_top_level(a);

                AssertSql(
                    """
SELECT c["Ints"]
FROM root c
WHERE (c["Discriminator"] = "PrimitiveCollectionsEntity")
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
@__ints_0='[11,111]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(ARRAY_CONCAT(@__ints_0, c["Ints"])) = 2))
""");
            });

    public override Task Column_collection_Union_parameter_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Union_parameter_collection(a);

                AssertSql(
                    """
@__ints_0='[11,111]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(SetUnion(c["Ints"], @__ints_0)) = 2))
""");
            });

    public override Task Column_collection_Intersect_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Intersect_inline_collection(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(SetIntersect(c["Ints"], [11, 111])) = 2))
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
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(SetUnion(ARRAY (
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 100)), [50])) = 2))
""");
            });

    public override Task Column_collection_equality_parameter_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_equality_parameter_collection(a);

                AssertSql(
                    """
@__ints_0='[1,10]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Ints"] = @__ints_0))
""");
            });

    public override Task Column_collection_Concat_parameter_collection_equality_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Concat_parameter_collection_equality_inline_collection(a);

                AssertSql(
                    """
@__ints_0='[1,10]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_CONCAT(c["Ints"], @__ints_0) = [1,11,111,1,10]))
""");
            });

    public override Task Column_collection_equality_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_equality_inline_collection(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Ints"] = [1,10]))
""");
            });

    public override Task Column_collection_equality_inline_collection_with_parameters(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_equality_inline_collection_with_parameters(a);

                AssertSql(
                    """
@__i_0='1'
@__j_1='10'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Ints"] = [@__i_0, @__j_1]))
""");
            });

    public override Task Column_collection_Where_equality_inline_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Column_collection_Where_equality_inline_collection(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY (
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i != 11)) = [1,111]))
""");
            });

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async));

            // Note that even if the query didn't attempt to do offset without limit, Cosmos still doesn't support OFFSET/LIMIT in subqueries,
            // so this test would fail anyway.
            Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);

            AssertSql();
        }
    }

    public override Task Parameter_collection_in_subquery_Union_column_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Parameter_collection_in_subquery_Union_column_collection(a);

                AssertSql(
                    """
@__Skip_0='[111]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (ARRAY_LENGTH(SetUnion(@__Skip_0, c["Ints"])) = 3))
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
        // TODO: Count after Skip requires subquery pushdown
        await AssertTranslationFailed(() => base.Parameter_collection_in_subquery_Count_as_compiled_query(async));

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async));

            // Note that even if the query didn't attempt to do offset without limit, Cosmos still doesn't support OFFSET/LIMIT in
            // subqueries, so this test would fail anyway.
            Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);

            AssertSql();
        }
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Column_collection_in_subquery_Union_parameter_collection(async));

            // Note that even if the query didn't attempt to do offset without limit, Cosmos still doesn't support OFFSET/LIMIT in subqueries,
            // so this test would fail anyway.
            Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);

            AssertSql();
        }
    }

    public override Task Project_collection_of_ints_simple(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_ints_simple(a);

                AssertSql(
                    """
SELECT c["Ints"]
FROM root c
WHERE (c["Discriminator"] = "PrimitiveCollectionsEntity")
ORDER BY c["Id"]
""");
            });

    public override async Task Project_collection_of_ints_ordered(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Project_collection_of_ints_ordered(async));

            Assert.Contains("'ORDER BY' is not supported in subqueries.", exception.Message);
        }
    }

    // TODO: Project out primitive collection subquery: #33797
    public override async Task Project_collection_of_datetimes_filtered(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_collection_of_datetimes_filtered(async));
        }
    }

    public override async Task Project_collection_of_nullable_ints_with_paging(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception =
                await Assert.ThrowsAsync<CosmosException>(() => base.Project_collection_of_nullable_ints_with_paging(async: true));

            Assert.Contains("'OFFSET LIMIT' clause is not supported in subqueries.", exception.Message);
        }
    }

    public override async Task Project_collection_of_nullable_ints_with_paging2(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_nullable_ints_with_paging2(async: true));

            // Note that even if the query didn't attempt to do offset without limit, Cosmos still doesn't support OFFSET/LIMIT in subqueries,
            // so this test would fail anyway.
            Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);

            AssertSql();
        }
    }

    public override async Task Project_collection_of_nullable_ints_with_paging3(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_nullable_ints_with_paging3(async));

            // Note that even if the query didn't attempt to do offset without limit, Cosmos still doesn't support OFFSET/LIMIT in subqueries,
            // so this test would fail anyway.
            Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);

            AssertSql();
        }
    }

    // TODO: Project out primitive collection subquery: #33797
    public override async Task Project_collection_of_ints_with_distinct(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<InvalidCastException>(() => base.Project_collection_of_ints_with_distinct(async));
        }
    }

    public override Task Project_collection_of_nullable_ints_with_distinct(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_collection_of_nullable_ints_with_distinct(a);

                AssertSql(
                    """
SELECT VALUE {"c" : [c["String"], "foo"]}
FROM root c
WHERE (c["Discriminator"] = "PrimitiveCollectionsEntity")
""");
            });

    // TODO: Project out primitive collection subquery: #33797
    public override async Task Project_collection_of_ints_with_ToList_and_FirstOrDefault(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<InvalidCastException>(() => base.Project_collection_of_ints_with_ToList_and_FirstOrDefault(async));
        }
    }

    // TODO: Project out primitive collection subquery: #33797
    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await Assert.ThrowsAsync<InvalidCastException>(
                () => base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(async));
        }
    }

    public override async Task Project_multiple_collections(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // TODO: Project out primitive collection subquery: #33797
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_multiple_collections(async));
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
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND (c["Id"] < 4))
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
SELECT [c["String"], "foo"] AS c
FROM root c
WHERE (c["Discriminator"] = "PrimitiveCollectionsEntity")
""");
            });

    public override async Task Project_inline_collection_with_Union(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // TODO: Project out primitive collection subquery: #33797
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_inline_collection_with_Union(async));
        }
    }

    public override async Task Project_inline_collection_with_Concat(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // TODO: Project out primitive collection subquery: #33797
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Project_inline_collection_with_Concat(async));
        }
    }

    public override Task Nested_contains_with_Lists_and_no_inferred_type_mapping(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Nested_contains_with_Lists_and_no_inferred_type_mapping(a);

                AssertSql(
                    """
@__strings_1='["one","two","three"]'
@__ints_0='[1,2,3]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__strings_1, (ARRAY_CONTAINS(@__ints_0, c["Int"]) ? "one" : "two")))
""");
            });

    public override Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Nested_contains_with_arrays_and_no_inferred_type_mapping(a);

                AssertSql(
                    """
@__strings_1='["one","two","three"]'
@__ints_0='[1,2,3]'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "PrimitiveCollectionsEntity") AND ARRAY_CONTAINS(@__strings_1, (ARRAY_CONTAINS(@__ints_0, c["Int"]) ? "one" : "two")))
""");
            });

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
            => base.AddOptions(builder.ConfigureWarnings(
                w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
