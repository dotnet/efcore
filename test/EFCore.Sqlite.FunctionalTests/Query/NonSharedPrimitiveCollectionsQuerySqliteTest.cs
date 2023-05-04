// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query;

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

    // The JSON representation for decimal is e.g. 1 (JSON int), whereas our literal representation is "1.0" (string). See #30727.
    public override Task Array_of_decimal()
        => AssertTranslationFailed(() => base.Array_of_decimal());

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
    WHERE datetime("s"."value") = '2023-01-01 12:30:00') = 2
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

    // The JSON representation for DateTimeOffset is ISO8601 (2023-01-01T12:30:00+02:00), but our SQL literal representation is
    // 2023-01-01 12:30:00+02:00 (no T).
    // datetime('2023-01-01T12:30:00+02:00') yields '2023-01-01 10:30:00' - converted to UTC, no timezone.
    // See #30727.
    public override Task Array_of_DateTimeOffset()
        => AssertTranslationFailed(() => base.Array_of_DateTimeOffset());

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
    WHERE upper("s"."value") = 'DC8C903D-D655-4144-A0FD-358099D40AE1') = 2
LIMIT 2
""");
    }

    // The JSON representation for new[] { 1, 2 } is AQI= (base64), our SQL literal representation is X'0102'. See #30727.
    public override Task Array_of_byte_array()
        => AssertTranslationFailed(() => base.Array_of_byte_array());

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

    [ConditionalFact] // #30630
    public override async Task Array_of_geometry_is_not_supported()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<TestContext>(
                onConfiguring: options => options.UseSqlite(o => o.UseNetTopologySuite()),
                addServices: s => s.AddEntityFrameworkSqliteNetTopologySuite(),
                onModelCreating: mb => mb.Entity<TestEntity>().Property<Point[]>("Points")));

        Assert.Equal(CoreStrings.PropertyNotMapped("Point[]", "TestEntity", "Points"), exception.Message);
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
