// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NonSharedPrimitiveCollectionsQuerySqliteTest : NonSharedPrimitiveCollectionsQueryRelationalTestBase
{
    #region Support for specific element types

    public override async Task Array_of_int()
    {
        await base.Array_of_int();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 1) = 2
LIMIT 2
""");
    }

    public override async Task Array_of_long()
    {
        await base.Array_of_long();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 1) = 2
LIMIT 2
""");
    }

    public override async Task Array_of_short()
    {
        await base.Array_of_short();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 1) = 2
LIMIT 2
""");
    }

    public override async Task Array_of_double()
    {
        await base.Array_of_double();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 1.0) = 2
LIMIT 2
""");
    }

    public override async Task Array_of_float()
    {
        await base.Array_of_float();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 1) = 2
LIMIT 2
""");
    }

    // The JSON representation for decimal is e.g. 1 (JSON int), whereas our literal representation is "1.0" (string).
    // We can cast the 1 to TEXT, but we'd still get "1" not "1.0".
    public override async Task Array_of_decimal()
    {
        await base.Array_of_decimal();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '1.0') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_DateTime()
    {
        await base.Array_of_DateTime();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '2023-01-01 12:30:00') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_DateTime_with_milliseconds()
    {
        await base.Array_of_DateTime_with_milliseconds();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '2023-01-01 12:30:00.123') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_DateTime_with_microseconds()
    {
        await base.Array_of_DateTime_with_microseconds();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '2023-01-01 12:30:00.123456') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_DateOnly()
    {
        await base.Array_of_DateOnly();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '2023-01-01') = 2
LIMIT 2
""");
    }

    [ConditionalFact(Skip = "Issue #30730: TODO: SQLite is not matching elements here.")]
    public override async Task Array_of_TimeOnly()
    {
        await base.Array_of_TimeOnly();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '12:30:00') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly_with_milliseconds()
    {
        await base.Array_of_TimeOnly_with_milliseconds();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '12:30:00.1230000') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_TimeOnly_with_microseconds()
    {
        await base.Array_of_TimeOnly_with_microseconds();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '12:30:00.1234560') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_DateTimeOffset()
    {
        await base.Array_of_DateTimeOffset();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = '2023-01-01 12:30:00+02:00') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_bool()
    {
        await base.Array_of_bool();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value") = 2
LIMIT 2
""");
    }

    public override async Task Array_of_Guid()
    {
        await base.Array_of_Guid();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 'DC8C903D-D655-4144-A0FD-358099D40AE1') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_byte_array()
    {
        await base.Array_of_byte_array();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE unhex("s"."value") = X'0102') = 2
LIMIT 2
""");
    }

    public override async Task Array_of_enum()
    {
        await base.Array_of_enum();

        AssertSql(
            """
SELECT "t"."Id", "t"."Ints", "t"."SomeArray"
FROM "TestEntity" AS "t"
WHERE (
    SELECT COUNT(*)
    FROM json_each("t"."SomeArray") AS "s"
    WHERE "s"."value" = 0) = 2
LIMIT 2
""");
    }

    #endregion Support for specific element types

    public override async Task Column_collection_inside_json_owned_entity()
    {
        await base.Column_collection_inside_json_owned_entity();

        AssertSql(
            """
SELECT "t"."Id", "t"."Owned"
FROM "TestOwner" AS "t"
WHERE json_array_length("t"."Owned" ->> 'Strings') = 2
LIMIT 2
""",
            //
            """
SELECT "t"."Id", "t"."Owned"
FROM "TestOwner" AS "t"
WHERE "t"."Owned" ->> 'Strings' ->> 1 = 'bar'
LIMIT 2
""");
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;
}
