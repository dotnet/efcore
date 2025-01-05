// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class TemporalTranslationsSqliteTest : TemporalTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public TemporalTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region DateTime

    public override async Task DateTime_Now(bool async)
    {
        await base.DateTime_Now(async);

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), '0'), '.') <> @myDatetime
""");
    }

    public override async Task DateTime_UtcNow(bool async)
    {
        await base.DateTime_UtcNow(async);

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now'), '0'), '.') <> @myDatetime
""");
    }

    public override async Task DateTime_Today(bool async)
    {
        await base.DateTime_Today(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" = rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime', 'start of day'), '0'), '.')
""");
    }

    public override async Task DateTime_Date(bool async)
    {
        await base.DateTime_Date(async);

        AssertSql(
            """
@myDatetime='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "b"."DateTime", 'start of day'), '0'), '.') = @myDatetime
""");
    }

    public override async Task DateTime_AddYear(bool async)
    {
        await base.DateTime_AddYear(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateTime", CAST(1 AS TEXT) || ' years') AS INTEGER) = 1999
""");
    }

    public override async Task DateTime_Year(bool async)
    {
        await base.DateTime_Year(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateTime") AS INTEGER) = 1998
""");
    }

    public override async Task DateTime_Month(bool async)
    {
        await base.DateTime_Month(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%m', "b"."DateTime") AS INTEGER) = 5
""");
    }

    public override async Task DateTime_DayOfYear(bool async)
    {
        await base.DateTime_DayOfYear(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%j', "b"."DateTime") AS INTEGER) = 124
""");
    }

    public override async Task DateTime_Day(bool async)
    {
        await base.DateTime_Day(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%d', "b"."DateTime") AS INTEGER) = 4
""");
    }

    public override async Task DateTime_Hour(bool async)
    {
        await base.DateTime_Hour(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%H', "b"."DateTime") AS INTEGER) = 15
""");
    }

    public override async Task DateTime_Minute(bool async)
    {
        await base.DateTime_Minute(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%M', "b"."DateTime") AS INTEGER) = 30
""");
    }

    public override async Task DateTime_Second(bool async)
    {
        await base.DateTime_Second(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%S', "b"."DateTime") AS INTEGER) = 10
""");
    }

    public override async Task DateTime_Millisecond(bool async)
    {
        await base.DateTime_Millisecond(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE (CAST(strftime('%f', "b"."DateTime") AS REAL) * 1000.0) % 1000.0 = 123.0
""");
    }

    public override async Task DateTime_TimeOfDay(bool async)
    {
        await base.DateTime_TimeOfDay(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE rtrim(rtrim(strftime('%H:%M:%f', "b"."DateTime"), '0'), '.') = '00:00:00'
""");
    }

    public override Task DateTime_subtract_and_TotalDays(bool async)
        => AssertTranslationFailed(() => base.DateTime_subtract_and_TotalDays(async));

    public override async Task DateTime_Parse_with_constant(bool async)
    {
        await base.DateTime_Parse_with_constant(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" = '1998-05-04 15:30:10'
""");
    }

    public override async Task DateTime_Parse_with_parameter(bool async)
    {
        await base.DateTime_Parse_with_parameter(async);

        AssertSql(
            """
@Parse='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" = @Parse
""");
    }

    public override async Task DateTime_new_with_constant(bool async)
    {
        await base.DateTime_new_with_constant(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" = '1998-05-04 15:30:10'
""");
    }

    public override async Task DateTime_new_with_parameters(bool async)
    {
        await base.DateTime_new_with_parameters(async);

        AssertSql(
            """
@p='1998-05-04T15:30:10.0000000' (DbType = DateTime)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTime" = @p
""");
    }

    #endregion DateTime

    #region DateOnly

    public override async Task DateOnly_Year(bool async)
    {
        await base.DateOnly_Year(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateOnly") AS INTEGER) = 1990
""");
    }

    public override async Task DateOnly_Month(bool async)
    {
        await base.DateOnly_Month(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%m', "b"."DateOnly") AS INTEGER) = 11
""");
    }

    public override async Task DateOnly_Day(bool async)
    {
        await base.DateOnly_Day(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%d', "b"."DateOnly") AS INTEGER) = 10
""");
    }

    public override async Task DateOnly_DayOfYear(bool async)
    {
        await base.DateOnly_DayOfYear(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%j', "b"."DateOnly") AS INTEGER) = 314
""");
    }

    public override async Task DateOnly_DayOfWeek(bool async)
    {
        await base.DateOnly_DayOfWeek(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%w', "b"."DateOnly") AS INTEGER) = 6
""");
    }

    public override async Task DateOnly_AddYears(bool async)
    {
        await base.DateOnly_AddYears(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' years') = '1993-11-10'
""");
    }

    public override async Task DateOnly_AddMonths(bool async)
    {
        await base.DateOnly_AddMonths(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' months') = '1991-02-10'
""");
    }

    public override async Task DateOnly_AddDays(bool async)
    {
        await base.DateOnly_AddDays(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' days') = '1990-11-13'
""");
    }

    public override async Task DateOnly_FromDateTime(bool async)
    {
        await base.DateOnly_FromDateTime(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") = '1998-05-04'
""");
    }

    public override async Task DateOnly_FromDateTime_compared_to_property(bool async)
    {
        await base.DateOnly_FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") = "b"."DateOnly"
""");
    }

    public override async Task DateOnly_FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        await base.DateOnly_FromDateTime_compared_to_constant_and_parameter(async);

        AssertSql(
            """
@dateOnly='10/11/0002' (DbType = Date)

SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") IN (@dateOnly, '1998-05-04')
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_DateOnly_AddYears_Year(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddYears(3).Year == 1993));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateOnly", CAST(3 AS TEXT) || ' years') AS INTEGER) = 1993
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_DateOnly_AddYears_AddMonths(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(m => m.DateOnly.AddYears(3).AddMonths(3) == new DateOnly(1994, 2, 10)));

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' years', CAST(3 AS TEXT) || ' months') = '1994-02-10'
""");
    }

    public override async Task DateOnly_ToDateTime_property_DateOnly_with_constant_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.DateOnly_ToDateTime_property_DateOnly_with_constant_TimeOnly(async));

        AssertSql();
    }

    public override async Task DateOnly_ToDateTime_property_DateOnly_with_property_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.DateOnly_ToDateTime_property_DateOnly_with_property_TimeOnly(async));

        AssertSql();
    }

    public override async Task DateOnly_ToDateTime_constant_DateTime_with_property_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.DateOnly_ToDateTime_constant_DateTime_with_property_TimeOnly(async));

        AssertSql();
    }

    public override async Task DateOnly_ToDateTime_with_complex_DateTime(bool async)
    {
        await AssertTranslationFailed(() => base.DateOnly_ToDateTime_with_complex_DateTime(async));

        AssertSql();
    }

    public override async Task DateOnly_ToDateTime_with_complex_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.DateOnly_ToDateTime_with_complex_TimeOnly(async));

        AssertSql();
    }

    #endregion DateOnly

    #region TimeOnly

    public override async Task TimeOnly_Hour(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Hour(async));

        AssertSql();
    }

    public override async Task TimeOnly_Minute(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Minute(async));

        AssertSql();
    }

    public override async Task TimeOnly_Second(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Second(async));

        AssertSql();
    }

    public override async Task TimeOnly_Millisecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Millisecond(async));

        AssertSql();
    }

    public override async Task TimeOnly_Microsecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Microsecond(async));

        AssertSql();
    }

    public override async Task TimeOnly_Nanosecond(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Nanosecond(async));

        AssertSql();
    }

    public override async Task TimeOnly_AddHours(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_AddHours(async));

        AssertSql();
    }

    public override async Task TimeOnly_AddMinutes(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_AddMinutes(async));

        AssertSql();
    }

    public override async Task TimeOnly_Add_TimeSpan(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_Add_TimeSpan(async));

        AssertSql();
    }

    public override async Task TimeOnly_IsBetween(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_IsBetween(async));

        AssertSql();
    }

    public override async Task TimeOnly_subtract_TimeOnly(bool async)
    {
        // TimeSpan. Issue #18844.
        await AssertTranslationFailed(() => base.TimeOnly_subtract_TimeOnly(async));

        AssertSql();
    }

    public override async Task TimeOnly_FromDateTime_compared_to_property(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.TimeOnly_FromDateTime_compared_to_property(async));

        AssertSql();
    }

    public override async Task TimeOnly_FromDateTime_compared_to_parameter(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.TimeOnly_FromDateTime_compared_to_parameter(async));

        AssertSql();
    }

    public override async Task TimeOnly_FromDateTime_compared_to_constant(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.TimeOnly_FromDateTime_compared_to_constant(async));

        AssertSql();
    }

    public override async Task TimeOnly_FromTimeSpan_compared_to_property(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.TimeOnly_FromTimeSpan_compared_to_property(async));

        AssertSql();
    }

    public override async Task TimeOnly_FromTimeSpan_compared_to_parameter(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.TimeOnly_FromTimeSpan_compared_to_parameter(async));

        AssertSql();
    }

    public override async Task Order_by_TimeOnly_FromTimeSpan(bool async)
    {
        // TimeOnly/DateOnly is not supported. Issue #25103.
        await AssertTranslationFailed(() => base.Order_by_TimeOnly_FromTimeSpan(async));

        AssertSql();
    }

    #endregion TimeOnly

    #region DateTimeOffset

    public override async Task DateTimeOffset_Now(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Now(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_UtcNow(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_UtcNow(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Date(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Date(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Year(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Year(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Month(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Month(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_DayOfYear(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_DayOfYear(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Day(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Day(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Hour(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Hour(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Minute(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Minute(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Second(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Second(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Millisecond(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Millisecond(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Microsecond(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Microsecond(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_Nanosecond(bool async)
    {
        await AssertTranslationFailed(() => base.DateTimeOffset_Nanosecond(async));

        AssertSql();
    }

    public override async Task DateTimeOffset_TimeOfDay(bool async)
    {
        await base.DateTimeOffset_TimeOfDay(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddYears(bool async)
    {
        await base.DateTimeOffset_AddYears(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddMonths(bool async)
    {
        await base.DateTimeOffset_AddMonths(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddDays(bool async)
    {
        await base.DateTimeOffset_AddDays(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddHours(bool async)
    {
        await base.DateTimeOffset_AddHours(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddMinutes(bool async)
    {
        await base.DateTimeOffset_AddMinutes(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddSeconds(bool async)
    {
        await base.DateTimeOffset_AddSeconds(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task DateTimeOffset_AddMilliseconds(bool async)
    {
        await base.DateTimeOffset_AddMilliseconds(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override Task DateTimeOffset_ToUnixTimeMilliseconds(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_ToUnixTimeMilliseconds(async));

    public override Task DateTimeOffset_ToUnixTimeSecond(bool async)
        => AssertTranslationFailed(() => base.DateTimeOffset_ToUnixTimeSecond(async));

    public override async Task DateTimeOffset_milliseconds_parameter_and_constant(bool async)
    {
        await base.DateTimeOffset_milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTimeOffset" = '1902-01-02 10:00:00.1234567+01:30'
""");
    }

    #endregion DateTimeOffset

    #region TimeSpan

    // Translate TimeSpan members, #18844
    public override async Task TimeSpan_Hours(bool async)
    {
        await AssertTranslationFailed(() => base.TimeSpan_Hours(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task TimeSpan_Minutes(bool async)
    {
        await AssertTranslationFailed(() => base.TimeSpan_Minutes(async));

        AssertSql();
    }

    public override async Task TimeSpan_Seconds(bool async)
    {
        await AssertTranslationFailed(() => base.TimeSpan_Seconds(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task TimeSpan_Milliseconds(bool async)
    {
        await AssertTranslationFailed(() => base.TimeSpan_Milliseconds(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task TimeSpan_Microseconds(bool async)
    {
        await AssertTranslationFailed(() => base.TimeSpan_Microseconds(async));

        AssertSql();
    }

    // Translate TimeSpan members, #18844
    public override async Task TimeSpan_Nanoseconds(bool async)
    {
        await AssertTranslationFailed(() => base.TimeSpan_Nanoseconds(async));

        AssertSql();
    }

    #endregion TimeSpan

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
