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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (10, 999)
""");
    }

    public override async Task Inline_collection_of_nullable_ints_Contains_null(bool async)
    {
        await base.Inline_collection_of_nullable_ints_Contains_null(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 0
""");
    }

    public override async Task Inline_collection_Contains_with_one_value(bool async)
    {
        await base.Inline_collection_Contains_with_one_value(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" = 2
""");
    }

    public override async Task Inline_collection_Contains_with_two_values(bool async)
    {
        await base.Inline_collection_Contains_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_Contains_with_three_values(bool async)
    {
        await base.Inline_collection_Contains_with_three_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        await base.Inline_collection_Contains_with_all_parameters(async);

        AssertSql(
            """
@i='2'
@j='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (@i, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_constant_and_parameter(bool async)
    {
        await base.Inline_collection_Contains_with_constant_and_parameter(async);

        AssertSql(
            """
@j='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, @j)
""");
    }

    public override async Task Inline_collection_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_Contains_with_mixed_value_types(async);

        AssertSql(
            """
@i='11'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (999, @i, "p"."Id", "p"."Id" + "p"."Int")
""");
    }

    public override async Task Inline_collection_List_Contains_with_mixed_value_types(bool async)
    {
        await base.Inline_collection_List_Contains_with_mixed_value_types(async);

        AssertSql(
            """
@i='11'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (999, @i, "p"."Id", "p"."Id" + "p"."Int")
""");
    }

    public override async Task Inline_collection_Contains_as_Any_with_predicate(bool async)
    {
        await base.Inline_collection_Contains_as_Any_with_predicate(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999)
""");
    }

    public override async Task Inline_collection_negated_Contains_as_All(bool async)
    {
        await base.Inline_collection_negated_Contains_as_All(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" NOT IN (2, 999)
""");
    }

    public override async Task Inline_collection_Min_with_two_values(bool async)
    {
        await base.Inline_collection_Min_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE min(30, "p"."Int") = 30
""");
    }

    public override async Task Inline_collection_List_Min_with_two_values(bool async)
    {
        await base.Inline_collection_List_Min_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE min(30, "p"."Int") = 30
""");
    }

    public override async Task Inline_collection_Max_with_two_values(bool async)
    {
        await base.Inline_collection_Max_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE max(30, "p"."Int") = 30
""");
    }

    public override async Task Inline_collection_List_Max_with_two_values(bool async)
    {
        await base.Inline_collection_List_Max_with_two_values(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE max(30, "p"."Int") = 30
""");
    }

    public override async Task Inline_collection_Min_with_three_values(bool async)
    {
        await base.Inline_collection_Min_with_three_values(async);

        AssertSql(
            """
@i='25'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE min(30, "p"."Int", @i) = 25
""");
    }

    public override async Task Inline_collection_List_Min_with_three_values(bool async)
    {
        await base.Inline_collection_List_Min_with_three_values(async);

        AssertSql(
            """
@i='25'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE min(30, "p"."Int", @i) = 25
""");
    }

    public override async Task Inline_collection_Max_with_three_values(bool async)
    {
        await base.Inline_collection_Max_with_three_values(async);

        AssertSql(
            """
@i='35'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE max(30, "p"."Int", @i) = 35
""");
    }

    public override async Task Inline_collection_List_Max_with_three_values(bool async)
    {
        await base.Inline_collection_List_Max_with_three_values(async);

        AssertSql(
            """
@i='35'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE max(30, "p"."Int", @i) = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Min(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_Min(async);

        AssertSql(
            """
@i='25' (Nullable = true)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT MIN("v"."Value")
    FROM (SELECT CAST(30 AS INTEGER) AS "Value" UNION ALL VALUES ("p"."Int"), (@i)) AS "v") = 25
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_Max(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_Max(async);

        AssertSql(
            """
@i='35' (Nullable = true)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT MAX("v"."Value")
    FROM (SELECT CAST(30 AS INTEGER) AS "Value" UNION ALL VALUES ("p"."Int"), (@i)) AS "v") = 35
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Min(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Min(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT MIN("v"."Value")
    FROM (SELECT CAST(30 AS INTEGER) AS "Value" UNION ALL VALUES ("p"."NullableInt"), (NULL)) AS "v") = 30
""");
    }

    public override async Task Inline_collection_of_nullable_value_type_with_null_Max(bool async)
    {
        await base.Inline_collection_of_nullable_value_type_with_null_Max(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT MAX("v"."Value")
    FROM (SELECT CAST(30 AS INTEGER) AS "Value" UNION ALL VALUES ("p"."NullableInt"), (NULL)) AS "v") = 30
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Contains(bool async)
    {
        await base.Inline_collection_with_single_parameter_element_Contains(async);

        AssertSql(
            """
@i='2'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" = @i
""");
    }

    public override async Task Inline_collection_with_single_parameter_element_Count(bool async)
    {
        await base.Inline_collection_with_single_parameter_element_Count(async);

        AssertSql(
            """
@i='2'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT CAST(@i AS INTEGER) AS "Value") AS "v"
    WHERE "v"."Value" > "p"."Id") = 1
""");
    }

    public override async Task Inline_collection_Contains_with_EF_Parameter(bool async)
    {
        await base.Inline_collection_Contains_with_EF_Parameter(async);

        AssertSql(
            """
@p='[2,999,1000]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (
    SELECT "p0"."value"
    FROM json_each(@p) AS "p0"
)
""");
    }

    public override async Task Inline_collection_Count_with_column_predicate_with_EF_Parameter(bool async)
    {
        await base.Inline_collection_Count_with_column_predicate_with_EF_Parameter(async);

        AssertSql(
            """
@p='[2,999,1000]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each(@p) AS "p0"
    WHERE "p0"."value" > "p"."Id") = 2
""");
    }

    public override async Task Parameter_collection_Count(bool async)
    {
        await base.Parameter_collection_Count(async);

        AssertSql(
            """
@ids1='2'
@ids2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT @ids1 AS "Value" UNION ALL VALUES (@ids2)) AS "i"
    WHERE "i"."Value" > "p"."Id") = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (@ints1, @ints2)
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int(async);

        AssertSql(
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (@ints1, @ints2)
""",
            //
            """
@ints1='10'
@ints2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" NOT IN (@ints1, @ints2) OR "p"."NullableInt" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int(async);

        AssertSql(
            """
@nullableInts1='10'
@nullableInts2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (@nullableInts1, @nullableInts2)
""",
            //
            """
@nullableInts1='10'
@nullableInts2='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (@nullableInts1, @nullableInts2)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);

        AssertSql(
            """
@nullableInts1='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IS NULL OR "p"."NullableInt" = @nullableInts1
""",
            //
            """
@nullableInts1='999'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IS NOT NULL AND "p"."NullableInt" <> @nullableInts1
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_string(async);

        AssertSql(
            """
@strings1='10' (Size = 2)
@strings2='999' (Size = 3)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" IN (@strings1, @strings2)
""",
            //
            """
@strings1='10' (Size = 2)
@strings2='999' (Size = 3)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" NOT IN (@strings1, @strings2)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings1='10' (Size = 2)
@strings2='999' (Size = 3)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IN (@strings1, @strings2)
""",
            //
            """
@strings1='10' (Size = 2)
@strings2='999' (Size = 3)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" NOT IN (@strings1, @strings2) OR "p"."NullableString" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string(async);

        AssertSql(
            """
@strings1='10' (Size = 2)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" = @strings1
""",
            //
            """
@strings1='10' (Size = 2)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" <> @strings1
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings1='999' (Size = 3)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IS NULL OR "p"."NullableString" = @strings1
""",
            //
            """
@strings1='999' (Size = 3)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IS NOT NULL AND "p"."NullableString" <> @strings1
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
            """
@dateTimes1='2020-01-10T12:30:00.0000000Z' (DbType = DateTime)
@dateTimes2='9999-01-01T00:00:00.0000000Z' (DbType = DateTime)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."DateTime" IN (@dateTimes1, @dateTimes2)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
            """
@bools1='True'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Bool" = @bools1
""");
    }

    public override async Task Parameter_collection_of_enums_Contains(bool async)
    {
        await base.Parameter_collection_of_enums_Contains(async);

        AssertSql(
            """
@enums1='0'
@enums2='3'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Enum" IN (@enums1, @enums2)
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 0
""");
    }

    public override async Task Parameter_collection_Contains_with_EF_Constant(bool async)
    {
        await base.Parameter_collection_Contains_with_EF_Constant(async);

        AssertSql(
    """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Id" IN (2, 999, 1000)
""");
    }

    public override async Task Parameter_collection_Where_with_EF_Constant_Where_Any(bool async)
    {
        await base.Parameter_collection_Where_with_EF_Constant_Where_Any(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE EXISTS (
    SELECT 1
    FROM (SELECT 2 AS "Value" UNION ALL VALUES (999), (1000)) AS "i"
    WHERE "i"."Value" > 0)
""");
    }

    public override async Task Parameter_collection_Count_with_column_predicate_with_EF_Constant(bool async)
    {
        await base.Parameter_collection_Count_with_column_predicate_with_EF_Constant(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT 2 AS "Value" UNION ALL VALUES (999), (1000)) AS "i"
    WHERE "i"."Value" > "p"."Id") = 2
""");
    }

    // nothing to test here
    public override Task Parameter_collection_Count_with_huge_number_of_values(bool async)
        => base.Parameter_collection_Count_with_huge_number_of_values(async);

    // nothing to test here
    public override Task Parameter_collection_of_ints_Contains_int_with_huge_number_of_values(bool async)
        => base.Parameter_collection_of_ints_Contains_int_with_huge_number_of_values(async);

    public override async Task Column_collection_of_ints_Contains(bool async)
    {
        await base.Column_collection_of_ints_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 0
""");
    }

    public override async Task Column_collection_of_nullable_strings_contains_null(bool async)
    {
        await base.Column_collection_of_nullable_strings_contains_null(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Ints") = 2
""");
    }

    public override async Task Column_collection_Length(bool async)
    {
        await base.Column_collection_Length(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Ints") = 2
""");
    }

    public override async Task Column_collection_Count_with_predicate(bool async)
    {
        await base.Column_collection_Count_with_predicate(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each("p"."Ints") AS "i"
    WHERE "i"."value" > 1) = 2
""");
    }

    // #33932
    public override async Task Column_collection_Where_Count(bool async)
    {
        await base.Column_collection_Where_Count(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each("p"."Ints") AS "i"
    WHERE "i"."value" > 1) = 2
""");
    }

    public override async Task Column_collection_index_int(bool async)
    {
        await base.Column_collection_index_int(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" ->> 1 = 10
""");
    }

    public override async Task Column_collection_index_string(bool async)
    {
        await base.Column_collection_index_string(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Strings" ->> 1 = '10'
""");
    }

    public override async Task Column_collection_index_datetime(bool async)
    {
        await base.Column_collection_index_datetime(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."DateTimes" ->> 1 = '2020-01-10 12:30:00'
""");
    }

    public override async Task Column_collection_index_beyond_end(bool async)
    {
        await base.Column_collection_index_beyond_end(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" ->> 999 = 10
""");
    }

    public override async Task Nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        await base.Nullable_reference_column_collection_index_equals_nullable_column(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableStrings" ->> 2 = "p"."NullableString" OR ("p"."NullableStrings" ->> 2 IS NULL AND "p"."NullableString" IS NULL)
""");
    }

    public override async Task Non_nullable_reference_column_collection_index_equals_nullable_column(bool async)
    {
        await base.Non_nullable_reference_column_collection_index_equals_nullable_column(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "v"."Value"
    FROM (SELECT 0 AS "_ord", CAST(1 AS INTEGER) AS "Value" UNION ALL VALUES (1, 2), (2, 3)) AS "v"
    ORDER BY "v"."_ord"
    LIMIT 1 OFFSET "p"."Int") = 1
""");
    }

    public override async Task Inline_collection_value_index_Column(bool async)
    {
        // SQLite doesn't support correlated subqueries where the outer column is used as the LIMIT/OFFSET (see OFFSET "p"."Int" below)
        await Assert.ThrowsAsync<SqliteException>(() => base.Inline_collection_value_index_Column(async));

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "v"."Value"
    FROM (SELECT 0 AS "_ord", CAST(1 AS INTEGER) AS "Value" UNION ALL VALUES (1, "p"."Int"), (2, 3)) AS "v"
    ORDER BY "v"."_ord"
    LIMIT 1 OFFSET "p"."Int") = 1
""");
    }

    public override async Task Inline_collection_List_value_index_Column(bool async)
    {
        // SQLite doesn't support correlated subqueries where the outer column is used as the LIMIT/OFFSET (see OFFSET "p"."Int" below)
        await Assert.ThrowsAsync<SqliteException>(() => base.Inline_collection_List_value_index_Column(async));

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "v"."Value"
    FROM (SELECT 0 AS "_ord", CAST(1 AS INTEGER) AS "Value" UNION ALL VALUES (1, "p"."Int"), (2, 3)) AS "v"
    ORDER BY "v"."_ord"
    LIMIT 1 OFFSET "p"."Int") = 1
""");
    }

    public override async Task Parameter_collection_index_Column_equal_Column(bool async)
    {
        await base.Parameter_collection_index_Column_equal_Column(async);

        AssertSql(
            """
@ints='[0,2,3]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE @ints ->> "p"."Int" = "p"."Int"
""");
    }

    public override async Task Parameter_collection_index_Column_equal_constant(bool async)
    {
        await base.Parameter_collection_index_Column_equal_constant(async);

        AssertSql(
            """
@ints='[1,2,3]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE @ints ->> "p"."Int" = 1
""");
    }

    public override async Task Column_collection_ElementAt(bool async)
    {
        await base.Column_collection_ElementAt(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" ->> 1 = 10
""");
    }

    public override async Task Column_collection_First(bool async)
    {
        await base.Column_collection_First(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 1) = 1
""");
    }

    public override async Task Column_collection_FirstOrDefault(bool async)
    {
        await base.Column_collection_FirstOrDefault(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE COALESCE((
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 1), 0) = 1
""");
    }

    public override async Task Column_collection_Single(bool async)
    {
        await base.Column_collection_Single(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 1) = 1
""");
    }

    public override async Task Column_collection_SingleOrDefault(bool async)
    {
        await base.Column_collection_SingleOrDefault(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE COALESCE((
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 1), 0) = 1
""");
    }

    public override async Task Column_collection_Skip(bool async)
    {
        await base.Column_collection_Skip(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 11 IN (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."key"
    LIMIT 2 OFFSET 1
)
""");
    }

    public override async Task Column_collection_Where_Skip(bool async)
    {
        await base.Column_collection_Where_Skip(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM json_each("p"."Ints") AS "i"
        WHERE "i"."value" > 1
        ORDER BY "i"."key"
        LIMIT -1 OFFSET 1
    ) AS "i0") = 3
""");
    }

    public override async Task Column_collection_Where_Take(bool async)
    {
        await base.Column_collection_Where_Take(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM json_each("p"."Ints") AS "i"
        WHERE "i"."value" > 1
        ORDER BY "i"."key"
        LIMIT 2
    ) AS "i0") = 2
""");
    }

    public override async Task Column_collection_Where_Skip_Take(bool async)
    {
        await base.Column_collection_Where_Skip_Take(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM json_each("p"."Ints") AS "i"
        WHERE "i"."value" > 1
        ORDER BY "i"."key"
        LIMIT 2 OFFSET 1
    ) AS "i0") = 1
""");
    }

    public override async Task Column_collection_Contains_over_subquery(bool async)
    {
        await base.Column_collection_Contains_over_subquery(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE 11 IN (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    WHERE "i"."value" > 1
)
""");
    }

    public override async Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        await base.Column_collection_OrderByDescending_ElementAt(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    ORDER BY "i"."value" DESC
    LIMIT 1 OFFSET 0) = 111
""");
    }

    public override async Task Column_collection_Where_ElementAt(bool async)
    {
        await base.Column_collection_Where_ElementAt(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT "i"."value"
    FROM json_each("p"."Ints") AS "i"
    WHERE "i"."value" > 1
    ORDER BY "i"."key"
    LIMIT 1 OFFSET 0) = 11
""");
    }

    public override async Task Column_collection_Any(bool async)
    {
        await base.Column_collection_Any(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE json_array_length("p"."Ints") > 0
""");
    }

    public override async Task Column_collection_Distinct(bool async)
    {
        await base.Column_collection_Distinct(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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

    public override async Task Column_collection_SelectMany_with_filter(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Column_collection_SelectMany_with_filter(async))).Message);

    public override async Task Column_collection_SelectMany_with_Select_to_anonymous_type(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Column_collection_SelectMany_with_Select_to_anonymous_type(async))).Message);

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
@ints1='11'
@ints2='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each("p"."Ints") AS "i"
    INNER JOIN (SELECT @ints1 AS "Value" UNION ALL VALUES (@ints2)) AS "i0" ON "i"."value" = "i0"."Value") = 2
""");
    }

    public override async Task Inline_collection_Join_ordered_column_collection(bool async)
    {
        await base.Inline_collection_Join_ordered_column_collection(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
@ints1='11'
@ints2='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT * FROM (
            SELECT 1
            FROM (SELECT @ints1 AS "Value" UNION ALL VALUES (@ints2)) AS "i"
        )
        UNION ALL
        SELECT 1
        FROM json_each("p"."Ints") AS "i0"
    ) AS "u") = 2
""");
    }

    public override async Task Parameter_collection_with_type_inference_for_JsonScalarExpression(bool async)
    {
        await base.Parameter_collection_with_type_inference_for_JsonScalarExpression(async);

        AssertSql(
            """
@values='["one","two"]' (Size = 13)

SELECT CASE
    WHEN "p"."Id" <> 0 THEN @values ->> ("p"."Int" % 2)
    ELSE 'foo'
END
FROM "PrimitiveCollectionsEntity" AS "p"
""");
    }

    public override async Task Column_collection_Union_parameter_collection(bool async)
    {
        await base.Column_collection_Union_parameter_collection(async);

        AssertSql(
            """
@ints1='11'
@ints2='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
        UNION
        SELECT * FROM (
            SELECT @ints1 AS "Value" UNION ALL VALUES (@ints2)
        )
    ) AS "u") = 2
""");
    }

    public override async Task Column_collection_Intersect_inline_collection(bool async)
    {
        await base.Column_collection_Intersect_inline_collection(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
        INTERSECT
        SELECT * FROM (
            SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)
        )
    ) AS "i0") = 2
""");
    }

    public override async Task Inline_collection_Except_column_collection(bool async)
    {
        await base.Inline_collection_Except_column_collection(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT * FROM (
            SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)
        )
        EXCEPT
        SELECT "i"."value" AS "Value"
        FROM json_each("p"."Ints") AS "i"
    ) AS "e"
    WHERE "e"."Value" % 2 = 1) = 2
""");
    }

    public override async Task Column_collection_Where_Union(bool async)
    {
        await base.Column_collection_Where_Union(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
        WHERE "i"."value" > 100
        UNION
        SELECT CAST(50 AS INTEGER) AS "Value"
    ) AS "u") = 2
""");
    }

    public override async Task Column_collection_equality_parameter_collection(bool async)
    {
        await base.Column_collection_equality_parameter_collection(async);

        AssertSql(
            """
@ints='[1,10]' (Size = 6)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" = @ints
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
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Ints" = '[1,10]'
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

    public override async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        await base.Parameter_collection_in_subquery_Count_as_compiled_query(async);

        AssertSql(
            """
@ints1='10'
@ints2='111'

SELECT COUNT(*)
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."Value" AS "Value0"
        FROM (SELECT 1 AS "_ord", @ints1 AS "Value" UNION ALL VALUES (2, @ints2)) AS "i"
        ORDER BY "i"."_ord"
        LIMIT -1 OFFSET 1
    ) AS "i0"
    WHERE "i0"."Value0" > "p"."Id") = 1
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
@ints1='10'
@ints2='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i1"."Value"
        FROM (
            SELECT "i"."Value"
            FROM (SELECT 1 AS "_ord", @ints1 AS "Value" UNION ALL VALUES (2, @ints2)) AS "i"
            ORDER BY "i"."_ord"
            LIMIT -1 OFFSET 1
        ) AS "i1"
        UNION
        SELECT "i0"."value" AS "Value"
        FROM json_each("p"."Ints") AS "i0"
    ) AS "u") = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection(async);

        AssertSql(
            """
@Skip1='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT @Skip1 AS "Value"
        UNION
        SELECT "i"."value" AS "Value"
        FROM json_each("p"."Ints") AS "i"
    ) AS "u") = 3
""");
    }

    public override async Task Parameter_collection_in_subquery_Union_column_collection_nested(bool async)
    {
        await base.Parameter_collection_in_subquery_Union_column_collection_nested(async);

        AssertSql(
            """
@Skip1='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT @Skip1 AS "Value"
        UNION
        SELECT "i2"."value" AS "Value"
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
@ints1='10'
@ints2='111'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
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
        SELECT * FROM (
            SELECT @ints1 AS "Value" UNION ALL VALUES (@ints2)
        )
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

    public override async Task Project_inline_collection(bool async)
    {
        await base.Project_inline_collection(async);

        AssertSql(
            """
SELECT "p"."String"
FROM "PrimitiveCollectionsEntity" AS "p"
""");
    }

    public override async Task Project_inline_collection_with_Union(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Project_inline_collection_with_Union(async))).Message);

    public override async Task Project_inline_collection_with_Concat(bool async)
    {
        await base.Project_inline_collection_with_Concat(async);

        AssertSql();
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
@ints1='1'
@ints2='2'
@ints3='3'
@strings1='one' (Size = 3)
@strings2='two' (Size = 3)
@strings3='three' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE CASE
    WHEN "p"."Int" IN (@ints1, @ints2, @ints3) THEN 'one'
    ELSE 'two'
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
@strings1='one' (Size = 3)
@strings2='two' (Size = 3)
@strings3='three' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE CASE
    WHEN "p"."Int" IN (@ints1, @ints2, @ints3) THEN 'one'
    ELSE 'two'
END IN (@strings1, @strings2, @strings3)
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
        public DbSet<SimpleEntity> SimpleEntities
            => Set<SimpleEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(connection);
    }

    public record SimpleEntity(int Id, IEnumerable<string> List);

    public override async Task Parameter_collection_of_structs_Contains_struct(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_struct(async);

        AssertSql(
            """
@values1='22'
@values2='33'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" IN (@values1, @values2)
""",
            //
            """
@values1='11'
@values2='44'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" NOT IN (@values1, @values2)
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct(async);

        AssertSql(
            """
@values1='22'
@values2='33'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" IN (@values1, @values2)
""",
            //
            """
@values1='11'
@values2='44'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" NOT IN (@values1, @values2) OR "p"."NullableWrappedId" IS NULL
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(async);

        AssertSql(
            """
@values1='22'
@values2='33'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedIdWithNullableComparer" IN (@values1, @values2)
""",
            //
            """
@values1='11'
@values2='44'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" NOT IN (@values1, @values2) OR "p"."NullableWrappedId" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_struct(async);

        AssertSql(
            """
@values1='22'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" = @values1
""",
            //
            """
@values1='11'
@values2='44'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" NOT IN (@values1, @values2)
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct(async);

        AssertSql(
            """
@values1='22'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" IS NULL OR "p"."NullableWrappedId" = @values1
""",
            //
            """
@values1='11'
@values2='44'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" NOT IN (@values1, @values2) OR "p"."NullableWrappedId" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(async);

        AssertSql(
            """
@values1='22'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedIdWithNullableComparer" IS NULL OR "p"."NullableWrappedIdWithNullableComparer" = @values1
""",
            //
            """
@values1='11'
@values2='44'

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedIdWithNullableComparer" NOT IN (@values1, @values2) OR "p"."NullableWrappedIdWithNullableComparer" IS NULL
""");
    }

    public override async Task Values_of_enum_casted_to_underlying_value(bool async)
    {
        await base.Values_of_enum_casted_to_underlying_value(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (SELECT CAST(0 AS INTEGER) AS "Value" UNION ALL VALUES (1), (2), (3)) AS "v"
    WHERE "v"."Value" = "p"."Int") > 0
""");
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
            => SqliteTestStoreFactory.Instance;
    }
}
