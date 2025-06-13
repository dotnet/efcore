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
@ids='[2,999]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each(@ids) AS "i"
    WHERE "i"."value" > "p"."Id") = 1
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_int(async);

        AssertSql(
            """
@ints='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""",
            //
            """
@ints='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""");
    }

    public override async Task Parameter_collection_HashSet_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_HashSet_of_ints_Contains_int(async);

        AssertSql(
            """
@ints='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""",
            //
            """
@ints='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""");
    }

    public override async Task Parameter_collection_ImmutableArray_of_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_ImmutableArray_of_ints_Contains_int(async);

        AssertSql(
            """
@ints='[10,999]' (Nullable = false) (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""",
            //
            """
@ints='[10,999]' (Nullable = false) (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""");
    }

    public override async Task Parameter_collection_of_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_ints_Contains_nullable_int(async);

        AssertSql(
            """
@ints='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
)
""",
            //
            """
@ints='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" NOT IN (
    SELECT "i"."value"
    FROM json_each(@ints) AS "i"
) OR "p"."NullableInt" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_int(async);

        AssertSql(
            """
@nullableInts='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "n"."value"
    FROM json_each(@nullableInts) AS "n"
)
""",
            //
            """
@nullableInts='[10,999]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" NOT IN (
    SELECT "n"."value"
    FROM json_each(@nullableInts) AS "n"
)
""");
    }

    public override async Task Parameter_collection_of_nullable_ints_Contains_nullable_int(bool async)
    {
        await base.Parameter_collection_of_nullable_ints_Contains_nullable_int(async);

        AssertSql(
            """
@nullableInts_without_nulls='[999]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" IN (
    SELECT "n"."value"
    FROM json_each(@nullableInts_without_nulls) AS "n"
) OR "p"."NullableInt" IS NULL
""",
            //
            """
@nullableInts_without_nulls='[999]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableInt" NOT IN (
    SELECT "n"."value"
    FROM json_each(@nullableInts_without_nulls) AS "n"
) AND "p"."NullableInt" IS NOT NULL
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_string(async);

        AssertSql(
            """
@strings='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
)
""",
            //
            """
@strings='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" NOT IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
)
""");
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
)
""",
            //
            """
@strings='["10","999"]' (Size = 12)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" NOT IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
) OR "p"."NullableString" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_string(async);

        AssertSql(
            """
@strings='["10",null]' (Size = 11)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
)
""",
            //
            """
@strings_without_nulls='["10"]' (Size = 6)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."String" NOT IN (
    SELECT "s"."value"
    FROM json_each(@strings_without_nulls) AS "s"
)
""");
    }

    public override async Task Parameter_collection_of_nullable_strings_Contains_nullable_string(bool async)
    {
        await base.Parameter_collection_of_nullable_strings_Contains_nullable_string(async);

        AssertSql(
            """
@strings_without_nulls='["999"]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" IN (
    SELECT "s"."value"
    FROM json_each(@strings_without_nulls) AS "s"
) OR "p"."NullableString" IS NULL
""",
            //
            """
@strings_without_nulls='["999"]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableString" NOT IN (
    SELECT "s"."value"
    FROM json_each(@strings_without_nulls) AS "s"
) AND "p"."NullableString" IS NOT NULL
""");
    }

    public override async Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        await base.Parameter_collection_of_DateTimes_Contains(async);

        AssertSql(
            """
@dateTimes='["2020-01-10 12:30:00","9999-01-01 00:00:00"]' (Size = 45)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."DateTime" IN (
    SELECT "d"."value"
    FROM json_each(@dateTimes) AS "d"
)
""");
    }

    public override async Task Parameter_collection_of_bools_Contains(bool async)
    {
        await base.Parameter_collection_of_bools_Contains(async);

        AssertSql(
            """
@bools='[true]' (Size = 6)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Bool" IN (
    SELECT "b"."value"
    FROM json_each(@bools) AS "b"
)
""");
    }

    public override async Task Parameter_collection_of_enums_Contains(bool async)
    {
        await base.Parameter_collection_of_enums_Contains(async);

        AssertSql(
            """
@enums='[0,3]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Enum" IN (
    SELECT "e"."value"
    FROM json_each(@enums) AS "e"
)
""");
    }

    public override async Task Parameter_collection_null_Contains(bool async)
    {
        await base.Parameter_collection_null_Contains(async);

        AssertSql(
            """
SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."Int" IN (
    SELECT "i"."value"
    FROM json_each(NULL) AS "i"
)
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
@ints='[11,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM json_each("p"."Ints") AS "i"
    INNER JOIN json_each(@ints) AS "i0" ON "i"."value" = "i0"."value") = 2
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
@ints='[11,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1
        FROM json_each(@ints) AS "i"
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
@ints='[11,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value"
        FROM json_each("p"."Ints") AS "i"
        UNION
        SELECT "i0"."value"
        FROM json_each(@ints) AS "i0"
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
        SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)
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
        SELECT CAST(11 AS INTEGER) AS "Value" UNION ALL VALUES (111)
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
@ints='[10,111]' (Size = 8)

SELECT COUNT(*)
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i"."value" AS "value0"
        FROM json_each(@ints) AS "i"
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
@ints='[10,111]' (Size = 8)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "i1"."value"
        FROM (
            SELECT "i"."value"
            FROM json_each(@ints) AS "i"
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
@Skip='[111]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "s"."value"
        FROM json_each(@Skip) AS "s"
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
@Skip='[111]' (Size = 5)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT "s"."value"
        FROM json_each(@Skip) AS "s"
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
@ints='[10,111]' (Size = 8)

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
        SELECT "i0"."value"
        FROM json_each(@ints) AS "i0"
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
@ints='[1,2,3]' (Size = 7)
@strings='["one","two","three"]' (Size = 21)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE CASE
    WHEN "p"."Int" IN (
        SELECT "i"."value"
        FROM json_each(@ints) AS "i"
    ) THEN 'one'
    ELSE 'two'
END IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
)
""");
    }

    public override async Task Nested_contains_with_arrays_and_no_inferred_type_mapping(bool async)
    {
        await base.Nested_contains_with_arrays_and_no_inferred_type_mapping(async);

        AssertSql(
            """
@ints='[1,2,3]' (Size = 7)
@strings='["one","two","three"]' (Size = 21)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE CASE
    WHEN "p"."Int" IN (
        SELECT "i"."value"
        FROM json_each(@ints) AS "i"
    ) THEN 'one'
    ELSE 'two'
END IN (
    SELECT "s"."value"
    FROM json_each(@strings) AS "s"
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
@values='[22,33]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
)
""",
            //
            """
@values='[11,44]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" NOT IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
)
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct(async);

        AssertSql(
            """
@values='[22,33]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
)
""",
            //
            """
@values='[11,44]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" NOT IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
) OR "p"."NullableWrappedId" IS NULL
""");
    }

    public override async Task Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        await base.Parameter_collection_of_structs_Contains_nullable_struct_with_nullable_comparer(async);

        AssertSql(
            """
@values='[22,33]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedIdWithNullableComparer" IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
)
""",
            //
            """
@values='[11,44]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" NOT IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
) OR "p"."NullableWrappedId" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_struct(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_struct(async);

        AssertSql(
            """
@values='[null,22]' (Size = 9)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
)
""",
            //
            """
@values='[11,44]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."WrappedId" NOT IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
)
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct(async);

        AssertSql(
            """
@values_without_nulls='[22]' (Size = 4)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" IN (
    SELECT "v"."value"
    FROM json_each(@values_without_nulls) AS "v"
) OR "p"."NullableWrappedId" IS NULL
""",
            //
            """
@values='[11,44]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedId" NOT IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
) OR "p"."NullableWrappedId" IS NULL
""");
    }

    public override async Task Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(bool async)
    {
        await base.Parameter_collection_of_nullable_structs_Contains_nullable_struct_with_nullable_comparer(async);

        AssertSql(
            """
@values_without_nulls='[22]' (Size = 4)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedIdWithNullableComparer" IN (
    SELECT "v"."value"
    FROM json_each(@values_without_nulls) AS "v"
) OR "p"."NullableWrappedIdWithNullableComparer" IS NULL
""",
            //
            """
@values='[11,44]' (Size = 7)

SELECT "p"."Id", "p"."Bool", "p"."Bools", "p"."DateTime", "p"."DateTimes", "p"."Enum", "p"."Enums", "p"."Int", "p"."Ints", "p"."NullableInt", "p"."NullableInts", "p"."NullableString", "p"."NullableStrings", "p"."NullableWrappedId", "p"."NullableWrappedIdWithNullableComparer", "p"."String", "p"."Strings", "p"."WrappedId"
FROM "PrimitiveCollectionsEntity" AS "p"
WHERE "p"."NullableWrappedIdWithNullableComparer" NOT IN (
    SELECT "v"."value"
    FROM json_each(@values) AS "v"
) OR "p"."NullableWrappedIdWithNullableComparer" IS NULL
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
