// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateOnlyTranslationsSqliteTest : DateOnlyTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public DateOnlyTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Year()
    {
        await base.Year();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateOnly") AS INTEGER) = 1990
""");
    }

    public override async Task Month()
    {
        await base.Month();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%m', "b"."DateOnly") AS INTEGER) = 11
""");
    }

    public override async Task Day()
    {
        await base.Day();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%d', "b"."DateOnly") AS INTEGER) = 10
""");
    }

    public override async Task DayOfYear()
    {
        await base.DayOfYear();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%j', "b"."DateOnly") AS INTEGER) = 314
""");
    }

    public override async Task DayOfWeek()
    {
        await base.DayOfWeek();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%w', "b"."DateOnly") AS INTEGER) = 6
""");
    }

    public override async Task DayNumber()
    {
        await base.DayNumber();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(julianday("b"."DateOnly") - julianday('0001-01-01') AS INTEGER) = 726780
""");
    }

    public override async Task AddYears()
    {
        await base.AddYears();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' years', 'floor') = '1993-11-10'
""");
    }

    public override async Task AddMonths()
    {
        await base.AddMonths();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' months', 'floor') = '1991-02-10'
""");
    }

    public override async Task AddDays()
    {
        await base.AddDays();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' days') = '1990-11-13'
""");
    }

    public override async Task DayNumber_subtraction()
    {
        await base.DayNumber_subtraction();

        AssertSql(
            """
@DayNumber='726775'

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(julianday("b"."DateOnly") - julianday('0001-01-01') AS INTEGER) - @DayNumber = 5
""");
    }

    public override async Task FromDateTime()
    {
        await base.FromDateTime();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") = '1998-05-04'
""");
    }

    public override async Task FromDateTime_compared_to_property()
    {
        await base.FromDateTime_compared_to_property();

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") = "b"."DateOnly"
""");
    }

    public override async Task FromDateTime_compared_to_constant_and_parameter()
    {
        await base.FromDateTime_compared_to_constant_and_parameter();

        AssertSql(
            """
@dateOnly='10/11/0002' (DbType = Date)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") IN (@dateOnly, '1998-05-04')
""");
    }

    [Fact]
    public virtual async Task Where_AddYears_Year()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddYears(3).Year == 1993));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateOnly", CAST(3 AS TEXT) || ' years', 'floor') AS INTEGER) = 1993
""");
    }

    [Fact]
    public virtual async Task Where_AddYears_AddMonths()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddYears(3).AddMonths(3) == new DateOnly(1994, 2, 10)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' years', 'floor', CAST(3 AS TEXT) || ' months', 'floor') = '1994-02-10'
""");
    }

    [Fact]
    public virtual async Task AddMonths_past_end_of_month_gives_last_day_of_month()
    {
        // 1991-01-31 + 1 month
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddDays(82).AddMonths(1) == new DateOnly(1991, 2, 28)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(82 AS TEXT) || ' days', CAST(1 AS TEXT) || ' months', 'floor') = '1991-02-28'
""");
    }

    [Fact]
    public virtual async Task AddMonths_negative_past_end_of_month_gives_last_day_of_month()
    {
        // 1990-12-31 - 1 month
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddDays(51).AddMonths(-1) == new DateOnly(1990, 11, 30)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(51 AS TEXT) || ' days', CAST(-1 AS TEXT) || ' months', 'floor') = '1990-11-30'
""");
    }

    [Fact]
    public virtual async Task AddYears_from_leap_day_gives_last_day_of_February()
    {
        // 1992-02-29 + 1 year
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddDays(476).AddYears(1) == new DateOnly(1993, 2, 28)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(476 AS TEXT) || ' days', CAST(1 AS TEXT) || ' years', 'floor') = '1993-02-28'
""");
    }

    public override async Task ToDateTime_property_with_constant_TimeOnly()
    {
        await AssertTranslationFailed(base.ToDateTime_property_with_constant_TimeOnly);

        AssertSql();
    }

    public override async Task ToDateTime_property_with_property_TimeOnly()
    {
        await AssertTranslationFailed(base.ToDateTime_property_with_property_TimeOnly);

        AssertSql();
    }

    public override async Task ToDateTime_constant_DateTime_with_property_TimeOnly()
    {
        await AssertTranslationFailed(base.ToDateTime_constant_DateTime_with_property_TimeOnly);

        AssertSql();
    }

    public override async Task ToDateTime_with_complex_DateTime()
    {
        await AssertTranslationFailed(base.ToDateTime_with_complex_DateTime);

        AssertSql();
    }

    public override async Task ToDateTime_with_complex_TimeOnly()
    {
        await AssertTranslationFailed(base.ToDateTime_with_complex_TimeOnly);

        AssertSql();
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
