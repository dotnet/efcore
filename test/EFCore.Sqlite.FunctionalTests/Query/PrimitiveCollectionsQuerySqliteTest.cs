// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class PrimitiveCollectionsQuerySqliteTest : PrimitiveCollectionsQueryRelationalTestBase<
    PrimitiveCollectionsQuerySqliteTest.PrimitiveCollectionsQuerySqlServerFixture>
{
    public PrimitiveCollectionsQuerySqliteTest(PrimitiveCollectionsQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IS NULL OR "p"."NullableInt" = 999
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT CAST(2 AS INTEGER) AS "Value") AS "v"
    WHERE "v"."Value" > "p"."Id") = 1
""");
    }

    public override async Task Inline_collection_Count_with_two_values(bool async)
    {
        await base.Inline_collection_Count_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT CAST(2 AS INTEGER) AS "Value" UNION ALL VALUES (999)) AS "v"
    WHERE "v"."Value" > "p"."Id") = 1
""");
    }

    public override async Task Inline_collection_Count_with_three_values(bool async)
    {
        await base.Inline_collection_Count_with_three_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT CAST(2 AS INTEGER) AS "Value" UNION ALL VALUES (999), (1000)) AS "v"
    WHERE "v"."Value" > "p"."Id") = 2
""");
    }

    public override async Task Inline_collection_Contains_with_zero_values(bool async)
    {
        await base.Inline_collection_Contains_with_zero_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 0
""");
    }

    public override async Task Inline_collection_Contains_with_one_value(bool async)
    {
        await base.Inline_collection_Contains_with_one_value(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values(bool async)
    {
        await base.Inline_collection_Contains_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values(bool async)
    {
        await base.Inline_collection_Contains_with_three_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_EF_Constant(bool async)
    {
        await base.Inline_collection_Contains_with_EF_Constant(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        await base.Inline_collection_Contains_with_all_parameters(async);

        AssertSql(
            """
@__i_0='2'
@__j_1='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (@__i_0, @__j_1)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter(bool async)
    {
        await base.Inline_collection_Contains_with_constant_and_parameter(async);

        AssertSql(
            """
@__j_0='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, @__j_0)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_Contains_with_mixed_value_types(async);

        AssertSql(
            """
@__i_0='11'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (999, @__i_0, "p"."Id", "p"."Id" + "p"."Int")
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate(bool async)
    {
        await base.Inline_collection_Contains_as_Any_with_predicate(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All(bool async)
    {
        await base.Inline_collection_negated_Contains_as_All(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values(bool async)
    {
        await base.Inline_collection_Min_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE min(30, "p"."Int") = 30
""");
    }

    public override async Task Inline_collection_Max_with_two_values(bool async)
    {
        await base.Inline_collection_Max_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE max(30, "p"."Int") = 30
""");
    }

    public override async Task Inline_collection_Min_with_three_values(bool async)
    {
        await base.Inline_collection_Min_with_three_values(async);

        AssertSql(
            """
@__i_0='25'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE min(30, "p"."Int", @__i_0) = 25
""");
    }

    public override async Task Inline_collection_Max_with_three_values(bool async)
    {
        await base.Inline_collection_Max_with_three_values(async);

        AssertSql(
            """
@__i_0='35'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE max(30, "p"."Int", @__i_0) = 35
""");
    }

    public override async Task Parameter_collection_Count(bool async)
    {
        await base.Parameter_collection_Count(async);

        AssertSql(
            """
@__ids_0='[2,999]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each(@__ids_0) AS "i"
    WHERE "i"."value" > "p"."Id") = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_int(async);

        AssertSql(
            """
@__ints_0='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "i"."value"
    FROM json_each(@__ints_0) AS "i"
)
""",
            //
            """
@__ints_0='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (
    SELECT "i"."value"
    FROM json_each(@__ints_0) AS "i"
)
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int(async);

        AssertSql(
            """
@__ints_0='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (
    SELECT "i"."value"
    FROM json_each(@__ints_0) AS "i"
)
""",
            //
            """
@__ints_0='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" NOT IN (
    SELECT "i"."value"
    FROM json_each(@__ints_0) AS "i"
) OR "p"."NullableInt" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int(async);

        AssertSql(
            """
@__nullableInts_0='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "n"."value"
    FROM json_each(@__nullableInts_0) AS "n"
)
""",
            //
            """
@__nullableInts_0='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (
    SELECT "n"."value"
    FROM json_each(@__nullableInts_0) AS "n"
)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);

        AssertSql(
            """
@__nullableInts_0_without_nulls='[999]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (
    SELECT "n"."value"
    FROM json_each(@__nullableInts_0_without_nulls) AS "n"
) OR "p"."NullableInt" IS NULL
""",
            //
            """
@__nullableInts_0_without_nulls='[999]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" NOT IN (
    SELECT "n"."value"
    FROM json_each(@__nullableInts_0_without_nulls) AS "n"
) AND "p"."NullableInt" IS NOT NULL
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_string(async);

        AssertSql(
            """
@__strings_0='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0) AS "s"
)
""",
            //
            """
@__strings_0='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" NOT IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0) AS "s"
)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string(async);

        AssertSql(
            """
@__strings_0='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0) AS "s"
)
""",
            //
            """
@__strings_0='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" NOT IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0) AS "s"
) OR "p"."NullableString" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string(async);

        AssertSql(
            """
@__strings_0='["10",null]' (Size = 11)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0) AS "s"
)
""",
            //
            """
@__strings_0_without_nulls='["10"]' (Size = 6)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" NOT IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0_without_nulls) AS "s"
)
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(async);

        AssertSql(
            """
@__strings_0_without_nulls='["999"]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0_without_nulls) AS "s"
) OR "p"."NullableString" IS NULL
""",
            //
            """
@__strings_0_without_nulls='["999"]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" NOT IN (
    SELECT "s"."value"
    FROM json_each(@__strings_0_without_nulls) AS "s"
) AND "p"."NullableString" IS NOT NULL
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
            """
@__dateTimes_0='["2020-01-10 12:30:00","9999-01-01 00:00:00"]' (Size = 45)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."DateTime" IN (
    SELECT "d"."value"
    FROM json_each(@__dateTimes_0) AS "d"
)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
            """
@__bools_0='[true]' (Size = 6)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Bool" IN (
    SELECT "b"."value"
    FROM json_each(@__bools_0) AS "b"
)
""");
    }

    public override async Task Parameter_collection_of_enums_Contains(bool async)
    {
        await base.Parameter_collection_of_enums_Contains(async);

        AssertSql(
            """
@__enums_0='[0,3]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Enum" IN (
    SELECT "e"."value"
    FROM json_each(@__enums_0) AS "e"
)
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "i"."value"
    FROM json_each(NULL) AS "i"
)
""");
    }

    public override async Task Column_collection_of_ints_Contains(bool async)
    {
        await base.Column_collection_of_ints_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 10 IN (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 10 IN (
    SELECT "n"."value"
    FROM json_each("p"."NullableInts") AS "n"
)
""");
    }

    public override async Task Column_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Column_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE EXISTS (
    SELECT 1
    FROM json_each("p"."NullableInts") AS "n"
    WHERE "n"."value" IS NULL)
""");
    }

    public override async Task Column_collection_of_strings_contains_null(bool async)
    {
        await base.Column_collection_of_strings_contains_null(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 0
""");
    }

    public override async Task Column_collection_of_nullable_strings_contains_null(bool async)
    {
        await base.Column_collection_of_nullable_strings_contains_null(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE EXISTS (
    SELECT 1
    FROM json_each("p"."NullableStrings") AS "n"
    WHERE "n"."value" IS NULL)
""");
    }

    public override async Task Column_collection_of_bools_Contains(bool async)
    {
        await base.Column_collection_of_bools_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 1 IN (
    SELECT "b"."value"
    FROM json_each("p"."Bools") AS "b"
)
""");
    }

    public override async Task Column_collection_Count_method(bool async)
    {
        await base.Column_collection_Count_method(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Ints") = 2
""");
    }

    public override async Task Column_collection_Length(bool async)
    {
        await base.Column_collection_Length(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Ints") = 2
""");
    }

    public override async Task Column_collection_index_int(bool async)
    {
        await base.Column_collection_index_int(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" ->> 1 = 10
""");
    }

    public override async Task Column_collection_index_string(bool async)
    {
        await base.Column_collection_index_string(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Strings" ->> 1 = '10'
""");
    }

    public override async Task Column_collection_index_datetime(bool async)
    {
        await base.Column_collection_index_datetime(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."DateTimes" ->> 1 = '2020-01-10 12:30:00'
""");
    }

    public override async Task Column_collection_index_beyond_end(bool async)
    {
        await base.Column_collection_index_beyond_end(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" ->> 999 = 10
""");
    }

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        await base.Nullable_reference_column_collection_index_equals_nullable_column(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableStrings" ->> 2 = "p"."NullableString" OR ("p"."NullableStrings" ->> 2 IS NULL AND "p"."NullableString" IS NULL)
""");
    }

    public override async Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        await base.Non_nullable_reference_column_collection_index_equals_nullable_column(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Strings") > 0 AND "p"."Strings" ->> 1 = "p"."NullableString"
""");
    }

    public override async Task Inline_collection_index_Column(bool async)
    {
        // SQLite doesn't support correlated subqueries where the outer column is used as the LIMIT/OFFSET (see OFFSET "p"."Int" below)
        await Assert.ThrowsAsync<SqliteException>(() => base.Inline_collection_index_Column(async));

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "v"."Value"
    FROM (SELECT 0 AS "_ord", CAST(1 AS INTEGER) AS "Value" UNION ALL VALUES (1, 2), (2, 3)) AS "v"
    ORDER BY "v"."_ord"
    LIMIT 1 OFFSET "p"."Int") = 1
""");
    }

    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        await base.Parameter_collection_index_Column_equal_Column(async);

        AssertSql(
            """
@__ints_0='[0,2,3]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE @__ints_0 ->> "p"."Int" = "p"."Int"
""");
    }

    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        await base.Parameter_collection_index_Column_equal_constant(async);

        AssertSql(
            """
@__ints_0='[1,2,3]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE @__ints_0 ->> "p"."Int" = 1
""");
    }

    public override async Task Column_collection_ElementAt(bool async)
    {
        await base.Column_collection_ElementAt(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" ->> 1 = 10
""");
    }

    public override async Task Column_collection_Skip(bool async)
    {
        await base.Column_collection_Skip(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM json_each("p"."Ints") AS "i"
        ORDER BY "i"."key"
        LIMIT -1 OFFSET 1
    ) AS "i0") = 2
""");
    }

    public override async Task Column_collection_Take(bool async)
    {
        await base.Column_collection_Take(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 11 IN (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 2
)
""");
    }

    public override async Task Column_collection_Skip_Take(bool async)
    {
        await base.Column_collection_Skip_Take(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 11 IN (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 2 OFFSET 1
)
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        await base.Column_collection_OrderByDescending_ElementAt(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."value" DESC
    LIMIT 1 OFFSET 0) = 111
""");
    }

    public override async Task Column_collection_Any(bool async)
    {
        await base.Column_collection_Any(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Ints") > 0
""");
    }

    public override async Task Column_collection_Distinct(bool async)
    {
        await base.Column_collection_Distinct(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT "i"."value"
        FROM json_each("p"."Ints") AS "i"
    ) AS "i0") = 3
""");
    }

    public override async Task Column_collection_SelectMany(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Column_collection_SelectMany(async))).Message);

    public override async Task Column_collection_projection_from_top_level(bool async)
    {
        await base.Column_collection_projection_from_top_level(async);

        AssertSql(
            """
SELECT "p"."Ints"
FROM "PrimitiveCollectionsEntity" AS "p"
ORDER BY "p"."Id"
""");
    }

    public override async Task Column_collection_Join_parameter_collection(bool async)
    {
        await base.Column_collection_Join_parameter_collection(async);

        AssertSql(
            """
@__ints_0='[11,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each("p"."Ints") AS "i"
    INNER JOIN json_each(@__ints_0) AS "i0" ON "i"."value" = "i0"."value") = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        await base.Inline_collection_Join_ordered_column_collection(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)) AS "v"
    INNER JOIN json_each("p"."Ints") AS "i" ON "v"."Value" = "i"."value") = 2
""");
    }

    [ConditionalTheory]
    public override async Task Parameter_collection_Concat_column_collection(bool async)
    {
        // Issue #32561
        await Assert.ThrowsAsync<EqualException>(
            () => base.Parameter_collection_Concat_column_collection(async));

        AssertSql(
            """
@__ints_0='[11,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM json_each(@__ints_0) AS "i"
        UNION ALL
        SELECT 1
        FROM json_each("p"."Ints") AS "i0"
    ) AS "u") = 2
""");
    }

    public override async Task Column_collection_Union_parameter_collection(bool async)
    {
        await base.Column_collection_Union_parameter_collection(async);

        AssertSql(
            """
@__ints_0='[11,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
        UNION
        SELECT "i0"."value"
        FROM json_each(@__ints_0) AS "i0"
    ) AS "u") = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection(bool async)
    {
        await base.Column_collection_Intersect_inline_collection(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
        INTERSECT
        SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)
    ) AS "i0") = 2
""");
    }

    public override async Task Inline_collection_Except_column_collection(bool async)
    {
        await base.Inline_collection_Except_column_collection(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)
        EXCEPT
        SELECT "i"."value" AS "Value"
        FROM json_each("p"."Ints") AS "i"
    ) AS "e"
    WHERE "e"."Value" % 2 = 1) = 2
""");
    }

    public override async Task Column_collection_equality_parameter_collection(bool async)
    {
        await base.Column_collection_equality_parameter_collection(async);

        AssertSql(
            """
@__ints_0='[1,10]' (Size = 6)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" = @__ints_0
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" = '[1,10]'
""");
    }

    public override async Task Column_collection_equality_inline_collection_with_parameters(bool async)
    {
        await base.Column_collection_equality_inline_collection_with_parameters(async);

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query(async);

        AssertSql(
            """
@__ints='[10,111]' (Size = 8)

SELECT COUNT(*)
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value" AS "value0"
        FROM json_each(@__ints) AS "i"
        ORDER BY "i"."key"
        LIMIT -1 OFFSET 1
    ) AS "i0"
    WHERE "i0"."value0" > "p"."Id") = 1
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_another_parameter_collection_as_compiled_query(async);

        AssertSql();
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(async);

        AssertSql(
            """
@__ints='[10,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i1"."value"
        FROM (
            SELECT "i"."value"
            FROM json_each(@__ints) AS "i"
            ORDER BY "i"."key"
            LIMIT -1 OFFSET 1
        ) AS "i1"
        UNION
        SELECT "i0"."value"
        FROM json_each("p"."Ints") AS "i0"
    ) AS "u") = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection(async);

        AssertSql(
            """
@__Skip_0='[111]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "s"."value"
        FROM json_each(@__Skip_0) AS "s"
        UNION
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
    ) AS "u") = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_nested(async);

        AssertSql(
            """
@__Skip_0='[111]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "s"."value"
        FROM json_each(@__Skip_0) AS "s"
        UNION
        SELECT "i2"."value"
        FROM (
            SELECT "i1"."value"
            FROM (
                SELECT DISTINCT "i0"."value"
                FROM (
                    SELECT "i"."value"
                    FROM json_each("p"."Ints") AS "i"
                    ORDER BY "i"."value"
                    LIMIT -1 OFFSET 1
                ) AS "i0"
            ) AS "i1"
            ORDER BY "i1"."value" DESC
            LIMIT 20
        ) AS "i2"
    ) AS "u") = 3
""");
    }

    public override void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        base.Parameter_collection_in_subquery_and_Convert_as_compiled_query();

        AssertSql();
    }

    public override async Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        await base.Column_collection_in_subquery_Union_parameter_collection(async);

        AssertSql(
            """
@__ints_0='[10,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i1"."value"
        FROM (
            SELECT "i"."value"
            FROM json_each("p"."Ints") AS "i"
            ORDER BY "i"."key"
            LIMIT -1 OFFSET 1
        ) AS "i1"
        UNION
        SELECT "i0"."value"
        FROM json_each(@__ints_0) AS "i0"
    ) AS "u") = 3
""");
    }

    public override async Task Project_collection_of_ints_simple(bool async)
    {
        await base.Project_collection_of_ints_simple(async);

        AssertSql(
            """
SELECT "p"."Ints"
FROM "PrimitiveCollectionsEntity" AS "p"
ORDER BY "p"."Id"
""");
    }

    public override async Task Project_collection_of_ints_ordered(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_ints_ordered(async))).Message);

    public override async Task Project_collection_of_datetimes_filtered(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_datetimes_filtered(async))).Message);

    public override async Task Project_collection_of_nullable_ints_with_paging(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_nullable_ints_with_paging(async))).Message);

    public override async Task Project_collection_of_nullable_ints_with_paging2(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_nullable_ints_with_paging2(async))).Message);

    public override async Task Project_collection_of_nullable_ints_with_paging3(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_nullable_ints_with_paging3(async))).Message);

    public override async Task Project_collection_of_ints_with_distinct(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_ints_with_distinct(async))).Message);

    public override async Task Project_collection_of_nullable_ints_with_distinct(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_nullable_ints_with_distinct(async))).Message);

    public override async Task Project_collection_of_ints_with_ToList_and_FirstOrDefault(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_collection_of_ints_with_ToList_and_FirstOrDefault(async))).Message);

    public override async Task Project_multiple_collections(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_multiple_collections(async))).Message);

    public override async Task Project_primitive_collections_element(bool async)
    {
        await base.Project_primitive_collections_element(async);

        AssertSql(
            """
SELECT "p"."Ints" ->> 0 AS "Indexer", "p"."DateTimes" ->> 0 AS "EnumerableElementAt", "p"."Strings" ->> 1 AS "QueryableElementAt"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" < 4
ORDER BY "p"."Id"
""");
    }

    public override async Task Project_empty_collection_of_nullables_and_collection_only_containing_nulls(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_empty_collection_of_nullables_and_collection_only_containing_nulls(async))).Message);

    public override async Task Nested_contains_with_Lists_and_no_inferred_type_mapping(bool async)
    {
        await base.Nested_contains_with_Lists_and_no_inferred_type_mapping(async);

        AssertSql(
            """
@__ints_0='[1,2,3]' (Size = 7)
@__strings_1='["one","two","three"]' (Size = 21)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE CASE
    WHEN "p"."Int" IN (
        SELECT "i"."value"
        FROM json_each(@__ints_0) AS "i"
    ) THEN 'one'
    ELSE 'two'
END IN (
    SELECT "s"."value"
    FROM json_each(@__strings_1) AS "s"
)
""");
    }

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping(async);

        AssertSql(
            """
@__ints_0='[1,2,3]' (Size = 7)
@__strings_1='["one","two","three"]' (Size = 21)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."String", "p"."Strings"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE CASE
    WHEN "p"."Int" IN (
        SELECT "i"."value"
        FROM json_each(@__ints_0) AS "i"
    ) THEN 'one'
    ELSE 'two'
END IN (
    SELECT "s"."value"
    FROM json_each(@__strings_1) AS "s"
)
""");
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)] // Issue #32896
    public async Task Empty_string_used_for_primitive_collection_throws(bool async)
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = new SimpleContext(connection);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync("INSERT INTO SimpleEntities (List) VALUES ('');");

        var set = context.SimpleEntities;

        var message = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                if (async)
                {
                    await set.FirstAsync();
                }
                else
                {
                    set.First();
                }
            });

        Assert.Equal(CoreStrings.EmptyJsonString, message.Message);
    }

    public class SimpleContext(SqliteConnection connection) : DbContext
    {
        public DbSet<SimpleEntity> SimpleEntities => Set<SimpleEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(connection);
    }

    public record SimpleEntity(int Id, IEnumerable<string> List);

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
            => SqliteTestStoreFactory.Instance;
    }
}
