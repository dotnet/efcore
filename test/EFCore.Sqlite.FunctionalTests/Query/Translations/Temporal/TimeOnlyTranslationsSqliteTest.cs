// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsSqliteTest : TimeOnlyTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TimeOnlyTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%H', "b"."TimeOnly") AS INTEGER) = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%M', "b"."TimeOnly") AS INTEGER) = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%S', "b"."TimeOnly") AS INTEGER) = 10
""");
    }

    public override async Task Millisecond()
    {
        await base.Millisecond();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(substr("b"."TimeOnly" || '0000000', 10, 3) AS INTEGER) = 123
""");
    }

    public override async Task Microsecond()
    {
        await base.Microsecond();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(substr("b"."TimeOnly" || '0000000', 13, 3) AS INTEGER) = 456
""");
    }

    public override async Task Nanosecond()
    {
        await base.Nanosecond();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(substr("b"."TimeOnly" || '0000000', 16, 1) AS INTEGER) * 100 = 400
""");
    }

    public override async Task AddHours()
    {
        await base.AddHours();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time_add("b"."TimeOnly", 108000000000) = '18:30:10'
""");
    }

    public override async Task AddMinutes()
    {
        await base.AddMinutes();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time_add("b"."TimeOnly", 1800000000) = '15:33:10'
""");
    }

    public override async Task Add_TimeSpan()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Add_TimeSpan);

        AssertSql();
    }

    public override async Task IsBetween()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.IsBetween);

        AssertSql();
    }

    public override async Task Subtract()
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(base.Subtract);

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_property()
    {
        await base.FromDateTime_compared_to_property();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time("b"."DateTime") = "b"."TimeOnly"
""");
    }

    public override async Task FromDateTime_compared_to_parameter()
    {
        await base.FromDateTime_compared_to_parameter();

        AssertSql(
            """
@time='15:30' (DbType = Time)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time("b"."DateTime") = @time
""");
    }

    public override async Task FromDateTime_compared_to_constant()
    {
        await base.FromDateTime_compared_to_constant();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time("b"."DateTime") = '15:30:10'
""");
    }

    public override async Task FromTimeSpan_compared_to_property()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromTimeSpan_compared_to_property);

        AssertSql();
    }

    public override async Task FromTimeSpan_compared_to_parameter()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.FromTimeSpan_compared_to_parameter);

        AssertSql();
    }

    public override async Task Order_by_FromTimeSpan()
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(base.Order_by_FromTimeSpan);

        AssertSql();
    }

    [Fact]
    public virtual async Task AddHours_wraps_around_midnight()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddHours(10) == new TimeOnly(1, 30, 10)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time_add("b"."TimeOnly", 360000000000) = '01:30:10'
""");
    }

    [Fact]
    public virtual async Task AddHours_negative_wraps_around_midnight()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddHours(-25) == new TimeOnly(14, 30, 10)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time_add("b"."TimeOnly", -900000000000) = '14:30:10'
""");
    }

    [Fact]
    public virtual async Task AddMinutes_with_fractional_parameter()
    {
        var minutes = 1.5;

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddMinutes(minutes) == new TimeOnly(15, 31, 40)));

        AssertSql(
            """
@minutes='1.5'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE ef_time_add("b"."TimeOnly", CAST(@minutes * 600000000.0 AS INTEGER)) = '15:31:40'
""");
    }

    [Fact]
    public virtual async Task Project_AddHours_keeps_fraction()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => b.TimeOnly.AddHours(10)));

        AssertSql(
            """
SELECT ef_time_add("b"."TimeOnly", 360000000000)
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Project_fraction_parts()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Select(b => new { b.Id, b.TimeOnly.Millisecond, b.TimeOnly.Microsecond, b.TimeOnly.Nanosecond }),
            elementSorter: e => e.Id);

        AssertSql(
            """
SELECT "b"."Id", CAST(substr("b"."TimeOnly" || '0000000', 10, 3) AS INTEGER) AS "Millisecond", CAST(substr("b"."TimeOnly" || '0000000', 13, 3) AS INTEGER) AS "Microsecond", CAST(substr("b"."TimeOnly" || '0000000', 16, 1) AS INTEGER) * 100 AS "Nanosecond"
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual async Task Project_FromDateTime_keeps_fraction()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => TimeOnly.FromDateTime(b.DateTime)));

        AssertSql(
            """
SELECT ef_time("b"."DateTime")
FROM "BasicTypesEntities" AS "b"
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
