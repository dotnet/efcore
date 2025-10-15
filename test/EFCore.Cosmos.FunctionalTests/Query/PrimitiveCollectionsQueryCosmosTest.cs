// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
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

    public override async Task Inline_collection_of_ints_Contains()
    {
        await base.Inline_collection_of_ints_Contains();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Int"] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains()
    {
        await base.Inline_collection_of_nullable_ints_Contains();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["NullableInt"] IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null()
    {
        await base.Inline_collection_of_nullable_ints_Contains_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["NullableInt"] IN (null, 999)
""");
    }

    public override async Task Inline_collection_Count_with_zero_values()
    {
        await base.Inline_collection_Count_with_zero_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [])
    WHERE (a > c["Id"])) = 1)
""");
    }

    public override async Task Inline_collection_Count_with_one_value()
    {
        await base.Inline_collection_Count_with_one_value();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [2])
    WHERE (a > c["Id"])) = 1)
""");
    }

    public override async Task Inline_collection_Count_with_two_values()
    {
        await base.Inline_collection_Count_with_two_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [2, 999])
    WHERE (a > c["Id"])) = 1)
""");
    }

    public override async Task Inline_collection_Count_with_three_values()
    {
        await base.Inline_collection_Count_with_three_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [2, 999, 1000])
    WHERE (a > c["Id"])) = 2)
""");
    }

    public override async Task Inline_collection_Contains_with_zero_values()
    {
        await base.Inline_collection_Contains_with_zero_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Inline_collection_Contains_with_one_value()
    {
        await base.Inline_collection_Contains_with_one_value();

        AssertSql("ReadItem(None, 2)");
    }

    public override async Task Inline_collection_Contains_with_two_values()
    {
        await base.Inline_collection_Contains_with_two_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values()
    {
        await base.Inline_collection_Contains_with_three_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters()
    {
        await base.Inline_collection_Contains_with_all_parameters();

        AssertSql(
            """
@i='2'
@j='999'

SELECT VALUE c
FROM root c
WHERE c["Id"] IN (@i, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter()
    {
        await base.Inline_collection_Contains_with_constant_and_parameter();

        AssertSql(
            """
@j='999'

SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types()
    {
        await base.Inline_collection_Contains_with_mixed_value_types();

        AssertSql(
            """
@i='11'

SELECT VALUE c
FROM root c
WHERE c["Int"] IN (999, @i, c["Id"], (c["Id"] + c["Int"]))
""");
    }

    public override async Task Inline_collection_List_Contains_with_mixed_value_types()
    {
        await base.Inline_collection_List_Contains_with_mixed_value_types();

        AssertSql(
            """
@i='11'

SELECT VALUE c
FROM root c
WHERE c["Int"] IN (999, @i, c["Id"], (c["Id"] + c["Int"]))
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate()
    {
        await base.Inline_collection_Contains_as_Any_with_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Id"] IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All()
    {
        await base.Inline_collection_negated_Contains_as_All();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE c["Id"] NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values()
    {
        await base.Inline_collection_Min_with_two_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
    }

    public override async Task Inline_collection_List_Min_with_two_values()
    {
        await base.Inline_collection_List_Min_with_two_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
    }

    public override async Task Inline_collection_Max_with_two_values()
    {
        await base.Inline_collection_Max_with_two_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
    }

    public override async Task Inline_collection_List_Max_with_two_values()
    {
        await base.Inline_collection_List_Max_with_two_values();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"]])) = 30)
""");
    }

    public override async Task Inline_collection_Min_with_three_values()
    {
        await base.Inline_collection_Min_with_three_values();

        AssertSql(
            """
@i='25'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 25)
""");
    }

    public override async Task Inline_collection_List_Min_with_three_values()
    {
        await base.Inline_collection_List_Min_with_three_values();

        AssertSql(
            """
@i='25'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 25)
""");
    }

    public override async Task Inline_collection_Max_with_three_values()
    {
        await base.Inline_collection_Max_with_three_values();

        AssertSql(
            """
@i='35'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 35)
""");
    }

    public override async Task Inline_collection_List_Max_with_three_values()
    {
        await base.Inline_collection_List_Max_with_three_values();

        AssertSql(
            """
@i='35'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 35)
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Min()
    {
        await base.Inline_collection_of_nullable_value_type_Min();

        AssertSql(
            """
@i='25'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MIN(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 25)
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Max()
    {
        await base.Inline_collection_of_nullable_value_type_Max();

        AssertSql(
            """
@i='35'

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["Int"], @i])) = 35)
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min()
    {
        // Cosmos MIN()/MAX() sort nulls as smaller than ints (https://learn.microsoft.com/azure/cosmos-db/nosql/query/min);
        // since some of the columns included contain null, MIN() returns null as opposed to the smallest number.
        // In relational, aggregate MIN()/MAX() ignores nulls.
        await Assert.ThrowsAsync<EqualException>(base.Inline_collection_of_nullable_value_type_with_null_Min);

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

    public override async Task Inline_collection_of_nullable_value_type_with_null_Max()
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Max();

        AssertSql(
            """
@i=null

SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE MAX(a)
    FROM a IN (SELECT VALUE [30, c["NullableInt"], @i])) = 30)
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Contains()
    {
        await base.Inline_collection_with_single_parameter_element_Contains();

        AssertSql(
            """
ReadItem(None, 2)
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Count()
    {
        await base.Inline_collection_with_single_parameter_element_Count();

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
    }

    public override async Task Inline_collection_Contains_with_EF_Parameter()
    {
        await base.Inline_collection_Contains_with_EF_Parameter();

        AssertSql(
            """
@p='[2,999,1000]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@p, c["Id"])
""");
    }

    public override async Task Inline_collection_Count_with_column_predicate_with_EF_Parameter()
    {
        await base.Inline_collection_Count_with_column_predicate_with_EF_Parameter();

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
    }

    public override async Task Parameter_collection_Count()
    {
        await base.Parameter_collection_Count();

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
    }

    public override async Task Parameter_collection_of_ints_Contains_int()
    {
        await base.Parameter_collection_of_ints_Contains_int();

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
    }

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int()
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int();

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
    }

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int()
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int();

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
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int()
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int();

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
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int();

        AssertSql(
            """
@nullableInts='[10,999]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@nullableInts, c["Int"])
""",
            //
            """
@nullableInts='[10,999]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@nullableInts, c["Int"]))
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int()
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int();

        AssertSql(
            """
@nullableInts='[null,999]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@nullableInts, c["NullableInt"])
""",
            //
            """
@nullableInts='[null,999]'

SELECT VALUE c
FROM root c
WHERE NOT(ARRAY_CONTAINS(@nullableInts, c["NullableInt"]))
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string()
    {
        await base.Parameter_collection_of_strings_Contains_string();

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
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string()
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string();

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
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string()
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string();

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
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string()
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string();

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
    }

    public override async Task Parameter_collection_of_DateTimes_Contains()
    {
        await base.Parameter_collection_of_DateTimes_Contains();

        AssertSql(
            """
@dateTimes='["2020-01-10T12:30:00Z","9999-01-01T00:00:00Z"]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@dateTimes, c["DateTime"])
""");
    }

    public override async Task Parameter_collection_of_bools_Contains()
    {
        await base.Parameter_collection_of_bools_Contains();

        AssertSql(
            """
@bools='[true]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@bools, c["Bool"])
""");
    }

    public override async Task Parameter_collection_of_enums_Contains()
    {
        await base.Parameter_collection_of_enums_Contains();

        AssertSql(
            """
@enums='[0,3]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@enums, c["Enum"])
""");
    }

    public override async Task Parameter_collection_null_Contains()
    {
        await base.Parameter_collection_null_Contains();

        AssertSql(
            """
@ints=null

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@ints, c["Int"])
""");
    }

    public override async Task Parameter_collection_Contains_with_EF_Constant()
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Parameter_collection_Contains_with_EF_Constant);
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any()
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Parameter_collection_Where_with_EF_Constant_Where_Any);
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_EF_Constant()
    {
        // #34327
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_Count_with_column_predicate_with_EF_Constant);
        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    // nothing to test here
    public override Task Parameter_collection_Count_with_huge_number_of_values()
        => base.Parameter_collection_Count_with_huge_number_of_values();

    // nothing to test here
    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values()
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values();

    public override async Task Column_collection_of_ints_Contains()
    {
        await base.Column_collection_of_ints_Contains();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["Ints"], 10)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains()
    {
        await base.Column_collection_of_nullable_ints_Contains();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["NullableInts"], 10)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains_null()
    {
        await base.Column_collection_of_nullable_ints_Contains_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["NullableInts"], null)
""");
    }

    public override async Task Column_collection_of_strings_contains_null()
    {
        await base.Column_collection_of_strings_contains_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["Strings"], null)
""");
    }

    public override async Task Column_collection_of_nullable_strings_contains_null()
    {
        await base.Column_collection_of_nullable_strings_contains_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["NullableStrings"], null)
""");
    }

    public override async Task Column_collection_of_bools_Contains()
    {
        await base.Column_collection_of_bools_Contains();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(c["Bools"], true)
""");
    }

    public override async Task Column_collection_Count_method()
    {
        await base.Column_collection_Count_method();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["Ints"]) = 2)
""");
    }

    public override async Task Column_collection_Length()
    {
        await base.Column_collection_Length();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["Ints"]) = 2)
""");
    }

    public override async Task Column_collection_Count_with_predicate()
    {
        await base.Column_collection_Count_with_predicate();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM i IN c["Ints"]
    WHERE (i > 1)) = 2)
""");
    }

    public override async Task Column_collection_Where_Count()
    {
        await base.Column_collection_Where_Count();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM i IN c["Ints"]
    WHERE (i > 1)) = 2)
""");
    }

    public override async Task Column_collection_index_int()
    {
        await base.Column_collection_index_int();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][1] = 10)
""");
    }

    public override async Task Column_collection_index_string()
    {
        await base.Column_collection_index_string();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Strings"][1] = "10")
""");
    }

    public override async Task Column_collection_index_datetime()
    {
        await base.Column_collection_index_datetime();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTimes"][1] = "2020-01-10T12:30:00Z")
""");
    }

    public override async Task Column_collection_index_beyond_end()
    {
        await base.Column_collection_index_beyond_end();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][999] = 10)
""");
    }

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column()
    {
        await Assert.ThrowsAsync<EqualException>(base.Nullable_reference_column_collection_index_equals_nullable_column);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["NullableStrings"][2] = c["NullableString"])
""");
    }

    public override async Task Non_nullable_reference_column_collection_index_equals_nullable_column()
    {
        await base.Non_nullable_reference_column_collection_index_equals_nullable_column();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((ARRAY_LENGTH(c["Strings"]) > 0) AND (c["Strings"][1] = c["NullableString"]))
""");
    }

    public override async Task Inline_collection_index_Column()
    {
        // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Inline_collection_index_Column);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ([1, 2, 3][c["Int"]] = 1)
""");
    }

    public override async Task Inline_collection_index_Column_with_EF_Constant()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Inline_collection_index_Column_with_EF_Constant());

        Assert.Equal(CoreStrings.EFConstantNotSupported, exception.Message);
    }

    public override async Task Inline_collection_value_index_Column()
    {
        // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Inline_collection_value_index_Column);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ([1, c["Int"], 3][c["Int"]] = 1)
""");
    }

    public override async Task Inline_collection_List_value_index_Column()
    {
        // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Inline_collection_List_value_index_Column);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ([1, c["Int"], 3][c["Int"]] = 1)
""");
    }

    public override async Task Parameter_collection_index_Column_equal_Column()
    {
        // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Parameter_collection_index_Column_equal_Column);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
@ints='[0,2,3]'

SELECT VALUE c
FROM root c
WHERE (@ints[c["Int"]] = c["Int"])
""");
    }

    public override async Task Parameter_collection_index_Column_equal_constant()
    {
        // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Parameter_collection_index_Column_equal_constant);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
@ints='[1,2,3]'

SELECT VALUE c
FROM root c
WHERE (@ints[c["Int"]] = 1)
""");
    }

    public override async Task Column_collection_ElementAt()
    {
        await base.Column_collection_ElementAt();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][1] = 10)
""");
    }

    public override async Task Column_collection_First()
    {
        await base.Column_collection_First();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][0] = 1)
""");
    }

    public override async Task Column_collection_FirstOrDefault()
    {
        await base.Column_collection_FirstOrDefault();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Ints"][0] ?? 0) = 1)
""");
    }

    public override async Task Column_collection_Single()
    {
        await base.Column_collection_Single();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Ints"][0] = 1)
""");
    }

    public override async Task Column_collection_SingleOrDefault()
    {
        await base.Column_collection_SingleOrDefault();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Ints"][0] ?? 0) = 1)
""");
    }

    public override async Task Column_collection_Skip()
    {
        await base.Column_collection_Skip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(c["Ints"], 1)) = 2)
""");
    }

    public override async Task Column_collection_Take()
    {
        await base.Column_collection_Take();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(ARRAY_SLICE(c["Ints"], 0, 2), 11)
""");
    }

    public override async Task Column_collection_Skip_Take()
    {
        await base.Column_collection_Skip_Take();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(ARRAY_SLICE(c["Ints"], 1, 2), 11)
""");
    }

    public override async Task Column_collection_Where_Skip()
    {
        await base.Column_collection_Where_Skip();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)), 1)) = 3)
""");
    }

    public override async Task Column_collection_Where_Take()
    {
        await base.Column_collection_Where_Take();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)), 0, 2)) = 2)
""");
    }

    public override async Task Column_collection_Where_Skip_Take()
    {
        await base.Column_collection_Where_Skip_Take();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_SLICE(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)), 1, 2)) = 1)
""");
    }

    public override async Task Column_collection_Contains_over_subquery()
    {
        await base.Column_collection_Contains_over_subquery();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM i IN c["Ints"]
    WHERE ((i > 1) AND (i = 11)))
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt()
    {
        // 'ORDER BY' is not supported in subqueries.
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Column_collection_OrderByDescending_ElementAt);

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

    public override async Task Column_collection_Where_ElementAt()
    {
        await base.Column_collection_Where_ElementAt();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1))[0] = 11)
""");
    }

    public override async Task Column_collection_Any()
    {
        await base.Column_collection_Any();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["Ints"]) > 0)
""");
    }

    public override async Task Column_collection_Distinct()
    {
        // TODO: Subquery pushdown, #33968
        await AssertTranslationFailed(base.Column_collection_Distinct);

        AssertSql();
    }

    public override async Task Column_collection_SelectMany()
    {
        await base.Column_collection_SelectMany();

        AssertSql(
            """
SELECT VALUE i
FROM root c
JOIN i IN c["Ints"]
""");
    }

    public override async Task Column_collection_SelectMany_with_filter()
    {
        await base.Column_collection_SelectMany_with_filter();

        AssertSql(
            """
SELECT VALUE j
FROM root c
JOIN (
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 1)) j
""");
    }

    public override async Task Column_collection_SelectMany_with_Select_to_anonymous_type()
    {
        // TODO: #34004
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            base.Column_collection_SelectMany_with_Select_to_anonymous_type);

        Assert.Equal(CosmosStrings.ComplexProjectionInSubqueryNotSupported, exception.Message);
    }

    public override async Task Column_collection_projection_from_top_level()
    {
        await base.Column_collection_projection_from_top_level();

        AssertSql(
            """
SELECT VALUE c["Ints"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Column_collection_Join_parameter_collection()
    {
        // Cosmos join support. Issue #16920.
        await AssertTranslationFailed(base.Column_collection_Join_parameter_collection);

        AssertSql();
    }

    public override async Task Inline_collection_Join_ordered_column_collection()
    {
        // Cosmos join support. Issue #16920.
        await AssertTranslationFailed(base.Column_collection_Join_parameter_collection);

        AssertSql();
    }

    public override async Task Parameter_collection_Concat_column_collection()
    {
        await base.Parameter_collection_Concat_column_collection();

        AssertSql(
            """
@ints='[11,111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_CONCAT(@ints, c["Ints"])) = 2)
""");
    }

    public override async Task Parameter_collection_with_type_inference_for_JsonScalarExpression()
    {
        // Member indexer (c.Array[c.SomeMember]) isn't supported by Cosmos
        var exception = await Assert.ThrowsAsync<CosmosException>(
            base.Parameter_collection_with_type_inference_for_JsonScalarExpression);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
@values='["one","two"]'

SELECT VALUE ((c["Id"] != 0) ? @values[(c["Int"] % 2)] : "foo")
FROM root c
""");
    }

    public override async Task Column_collection_Union_parameter_collection()
    {
        await base.Column_collection_Union_parameter_collection();

        AssertSql(
            """
@ints='[11,111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(c["Ints"], @ints)) = 2)
""");
    }

    public override async Task Column_collection_Intersect_inline_collection()
    {
        await base.Column_collection_Intersect_inline_collection();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetIntersect(c["Ints"], [11, 111])) = 2)
""");
    }

    public override async Task Inline_collection_Except_column_collection()
    {
        await AssertTranslationFailedWithDetails(base.Inline_collection_Except_column_collection, CosmosStrings.ExceptNotSupported);

        AssertSql();
    }

    public override async Task Column_collection_Where_Union()
    {
        await base.Column_collection_Where_Union();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i > 100)), [50])) = 2)
""");
    }

    public override async Task Column_collection_equality_parameter_collection()
    {
        await base.Column_collection_equality_parameter_collection();

        AssertSql(
            """
@ints='[1,10]'

SELECT VALUE c
FROM root c
WHERE (c["Ints"] = @ints)
""");
    }

    public override async Task Column_collection_Concat_parameter_collection_equality_inline_collection()
    {
        await base.Column_collection_Concat_parameter_collection_equality_inline_collection();

        AssertSql(
            """
@ints='[1,10]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_CONCAT(c["Ints"], @ints) = [1,11,111,1,10])
""");
    }

    public override async Task Column_collection_equality_inline_collection()
    {
        await base.Column_collection_equality_inline_collection();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Ints"] = [1,10])
""");
    }

    public override async Task Column_collection_equality_inline_collection_with_parameters()
    {
        await base.Column_collection_equality_inline_collection_with_parameters();

        AssertSql(
            """
@i='1'
@j='10'

SELECT VALUE c
FROM root c
WHERE (c["Ints"] = [@i, @j])
""");
    }

    public override async Task Column_collection_Where_equality_inline_collection()
    {
        await base.Column_collection_Where_equality_inline_collection();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"]
    WHERE (i != 11)) = [1,111])
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query()
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query);

        Assert.Equal(SyncNotSupportedMessage, exception.Message);

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection()
    {
        await base.Parameter_collection_in_subquery_Union_column_collection();

        AssertSql(
            """
@Skip='[111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(@Skip, c["Ints"])) = 3)
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested()
    {
        // TODO: Subquery pushdown
        await AssertTranslationFailed(base.Parameter_collection_in_subquery_Union_column_collection_nested);

        AssertSql();
    }

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        // Array indexer over a parameter array ([1,2,3][0]) isn't supported by Cosmos.
        // TODO: general OFFSET/LIMIT support
        AssertTranslationFailed(base.Parameter_collection_in_subquery_and_Convert_as_compiled_query);

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query()
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_in_subquery_Count_as_compiled_query);

        Assert.Equal(SyncNotSupportedMessage, exception.Message);

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query()
    {
        // TODO: #33931
        // The ToList inside the query gets executed separately during shaper generation - and synchronously (even in the async
        // variant of the test), but Cosmos doesn't support sync I/O. So both sync and async variants fail because of unsupported
        // sync I/O.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query);

        Assert.Equal(SyncNotSupportedMessage, exception.Message);

        AssertSql();
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection()
    {
        await base.Column_collection_in_subquery_Union_parameter_collection();

        AssertSql(
            """
@ints='[10,111]'

SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(ARRAY_SLICE(c["Ints"], 1), @ints)) = 3)
""");
    }

    public override async Task Project_collection_of_ints_simple()
    {
        await base.Project_collection_of_ints_simple();

        AssertSql(
            """
SELECT VALUE c["Ints"]
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_collection_of_ints_ordered()
    {
        // 'ORDER BY' is not supported in subqueries.
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Project_collection_of_ints_ordered);

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

    public override async Task Project_collection_of_datetimes_filtered()
    {
        await base.Project_collection_of_datetimes_filtered();

        AssertSql(
            """
SELECT VALUE ARRAY(
    SELECT VALUE d
    FROM d IN c["DateTimes"]
    WHERE (DateTimePart("dd", d) != 1))
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging()
    {
        await base.Project_collection_of_nullable_ints_with_paging();

        AssertSql(
            """
SELECT VALUE ARRAY(
    SELECT VALUE i
    FROM i IN (SELECT VALUE ARRAY_SLICE(c["NullableInts"], 0, 20)))
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging2()
    {
        // 'ORDER BY' is not supported in subqueries.
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Project_collection_of_nullable_ints_with_paging2);

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

        AssertSql(
            """
SELECT VALUE ARRAY_SLICE(ARRAY(
    SELECT VALUE n
    FROM n IN c["NullableInts"]
    ORDER BY n), 1)
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_paging3()
    {
        await base.Project_collection_of_nullable_ints_with_paging3();

        AssertSql(
            """
SELECT VALUE ARRAY_SLICE(c["NullableInts"], 2)
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_collection_of_ints_with_distinct()
    {
        await base.Project_collection_of_ints_with_distinct();

        AssertSql(
            """
SELECT VALUE ARRAY(
    SELECT DISTINCT VALUE i
    FROM i IN c["Ints"])
FROM root c
ORDER BY c["Id"]
""");
    }

    public override async Task Project_collection_of_nullable_ints_with_distinct()
    {
        await base.Project_collection_of_nullable_ints_with_distinct();

        AssertSql(
            """
SELECT VALUE {"c" : [c["String"], "foo"]}
FROM root c
WHERE (c["$type"] = "PrimitiveCollectionsEntity")
""");
    }

    public override async Task Project_collection_of_ints_with_ToList_and_FirstOrDefault()
    {
        await base.Project_collection_of_ints_with_ToList_and_FirstOrDefault();

        AssertSql(
            """
SELECT VALUE ARRAY(
    SELECT VALUE i
    FROM i IN c["Ints"])
FROM root c
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");
    }

    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls()
    {
        await base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls();

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
    }

    public override async Task Project_multiple_collections()
    {
        var exception = await Assert.ThrowsAsync<CosmosException>(base.Project_multiple_collections);

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

    public override async Task Project_primitive_collections_element()
    {
        await base.Project_primitive_collections_element();

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
    }

    public override async Task Project_inline_collection()
    {
        await base.Project_inline_collection();

        // The following should be SELECT VALUE [c["String"], "foo"], #33779
        AssertSql(
            """
SELECT VALUE [c["String"], "foo"]
FROM root c
""");
    }

    // Non-correlated queries not supported by Cosmos
    public override Task Project_inline_collection_with_Union()
        => AssertTranslationFailed(base.Project_inline_collection_with_Union);

    // Non-correlated queries not supported by Cosmos
    public override Task Project_inline_collection_with_Concat()
        => AssertTranslationFailed(base.Project_inline_collection_with_Concat);

    public override async Task Nested_contains_with_Lists_and_no_inferred_type_mapping()
    {
        await base.Nested_contains_with_Lists_and_no_inferred_type_mapping();

        AssertSql(
            """
@strings='["one","two","three"]'
@ints='[1,2,3]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, (ARRAY_CONTAINS(@ints, c["Int"]) ? "one" : "two"))
""");
    }

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping()
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping();

        AssertSql(
            """
@strings='["one","two","three"]'
@ints='[1,2,3]'

SELECT VALUE c
FROM root c
WHERE ARRAY_CONTAINS(@strings, (ARRAY_CONTAINS(@ints, c["Int"]) ? "one" : "two"))
""");
    }

    public override async Task Values_of_enum_casted_to_underlying_value()
    {
        await base.Values_of_enum_casted_to_underlying_value();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM a IN (SELECT VALUE [0, 1, 2, 3])
    WHERE (a = c["Int"])) > 0)
""");
    }

    #region Cosmos-specific tests

    [ConditionalFact]
    public virtual async Task IsDefined()
    {
        await AssertQuery(
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => EF.Functions.IsDefined(e.Ints[2])),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => e.Ints.Length >= 3));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE IS_DEFINED(c["Ints"][2])
""");
    }

    [ConditionalFact]
    public virtual async Task CoalesceUndefined()
    {
        await AssertQuery(
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => EF.Functions.CoalesceUndefined(e.Ints[2], 999) == 999),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(e => e.Ints.Length < 3));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Ints"][2] ?? 999) = 999)
""");
    }

    #endregion Cosmos-specific tests

    public override async Task Parameter_collection_of_structs_Contains_struct()
    {
        // Requires collections of converted elements
        await Assert.ThrowsAsync<InvalidOperationException>(base.Parameter_collection_of_structs_Contains_struct);

        AssertSql();
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct()
    {
        // Requires collections of converted elements
        await Assert.ThrowsAsync<InvalidOperationException>(base.Parameter_collection_of_structs_Contains_nullable_struct);

        AssertSql();
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer()
    {
        // Requires collections of converted elements
        await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer);

        AssertSql();
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct()
    {
        // Requires collections of converted elements
        await Assert.ThrowsAsync<InvalidOperationException>(base.Parameter_collection_of_nullable_structs_Contains_struct);

        AssertSql();
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct()
    {
        // Requires collections of converted elements
        await Assert.ThrowsAsync<InvalidOperationException>(base.Parameter_collection_of_nullable_structs_Contains_nullable_struct);

        AssertSql();
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer()
    {
        // Requires collections of converted elements
        await Assert.ThrowsAsync<InvalidOperationException>(
            base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer);

        AssertSql();
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
                builder.ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            // Requires element type mapping; Issue #34026
            modelBuilder.Entity<PrimitiveCollectionsEntity>().Ignore(e => e.Enums);
        }
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private static readonly string SyncNotSupportedMessage
        = CoreStrings.WarningAsErrorTemplate(
            CosmosEventId.SyncNotSupported.ToString(),
            CosmosResources.LogSyncNotSupported(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
            "CosmosEventId.SyncNotSupported");
}
