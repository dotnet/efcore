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

    public override async Task Year(bool async)
    {
        await base.Year(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%Y', "b"."DateOnly") AS INTEGER) = 1990
""");
    }

    public override async Task Month(bool async)
    {
        await base.Month(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%m', "b"."DateOnly") AS INTEGER) = 11
""");
    }

    public override async Task Day(bool async)
    {
        await base.Day(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%d', "b"."DateOnly") AS INTEGER) = 10
""");
    }

    public override async Task DayOfYear(bool async)
    {
        await base.DayOfYear(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%j', "b"."DateOnly") AS INTEGER) = 314
""");
    }

    public override async Task DayOfWeek(bool async)
    {
        await base.DayOfWeek(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE CAST(strftime('%w', "b"."DateOnly") AS INTEGER) = 6
""");
    }

    public override async Task AddYears(bool async)
    {
        await base.AddYears(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' years') = '1993-11-10'
""");
    }

    public override async Task AddMonths(bool async)
    {
        await base.AddMonths(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' months') = '1991-02-10'
""");
    }

    public override async Task AddDays(bool async)
    {
        await base.AddDays(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateOnly", CAST(3 AS TEXT) || ' days') = '1990-11-13'
""");
    }

    public override async Task FromDateTime(bool async)
    {
        await base.FromDateTime(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") = '1998-05-04'
""");
    }

    public override async Task FromDateTime_compared_to_property(bool async)
    {
        await base.FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT "b"."Id", "b"."Bool", "b"."Byte", "b"."ByteArray", "b"."DateOnly", "b"."DateTime", "b"."DateTimeOffset", "b"."Decimal", "b"."Double", "b"."Enum", "b"."FlagsEnum", "b"."Float", "b"."Guid", "b"."Int", "b"."Long", "b"."Short", "b"."String", "b"."TimeOnly", "b"."TimeSpan"
FROM "BasicTypesEntities" AS "b"
WHERE date("b"."DateTime") = "b"."DateOnly"
""");
    }

    public override async Task FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        await base.FromDateTime_compared_to_constant_and_parameter(async);

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
    public virtual async Task Where_AddYears_Year(bool async)
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
    public virtual async Task Where_AddYears_AddMonths(bool async)
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

    public override async Task ToDateTime_property_with_constant_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_property_with_constant_TimeOnly(async));

        AssertSql();
    }

    public override async Task ToDateTime_property_with_property_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_property_with_property_TimeOnly(async));

        AssertSql();
    }

    public override async Task ToDateTime_constant_DateTime_with_property_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_constant_DateTime_with_property_TimeOnly(async));

        AssertSql();
    }

    public override async Task ToDateTime_with_complex_DateTime(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_with_complex_DateTime(async));

        AssertSql();
    }

    public override async Task ToDateTime_with_complex_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_with_complex_TimeOnly(async));

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
