// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsSqliteTest : DateTimeOffsetTranslationsTestBase<BasicTypesQuerySqliteFixture>
{
    public DateTimeOffsetTranslationsSqliteTest(BasicTypesQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Now(bool async)
    {
        await AssertTranslationFailed(() => base.Now(async));

        AssertSql();
    }

    public override async Task UtcNow(bool async)
    {
        await AssertTranslationFailed(() => base.UtcNow(async));

        AssertSql();
    }

    public override async Task Date(bool async)
    {
        await AssertTranslationFailed(() => base.Date(async));

        AssertSql();
    }

    public override async Task Year(bool async)
    {
        await AssertTranslationFailed(() => base.Year(async));

        AssertSql();
    }

    public override async Task Month(bool async)
    {
        await AssertTranslationFailed(() => base.Month(async));

        AssertSql();
    }

    public override async Task DayOfYear(bool async)
    {
        await AssertTranslationFailed(() => base.DayOfYear(async));

        AssertSql();
    }

    public override async Task Day(bool async)
    {
        await AssertTranslationFailed(() => base.Day(async));

        AssertSql();
    }

    public override async Task Hour(bool async)
    {
        await AssertTranslationFailed(() => base.Hour(async));

        AssertSql();
    }

    public override async Task Minute(bool async)
    {
        await AssertTranslationFailed(() => base.Minute(async));

        AssertSql();
    }

    public override async Task Second(bool async)
    {
        await AssertTranslationFailed(() => base.Second(async));

        AssertSql();
    }

    public override async Task Millisecond(bool async)
    {
        await AssertTranslationFailed(() => base.Millisecond(async));

        AssertSql();
    }

    public override async Task Microsecond(bool async)
    {
        await AssertTranslationFailed(() => base.Microsecond(async));

        AssertSql();
    }

    public override async Task Nanosecond(bool async)
    {
        await AssertTranslationFailed(() => base.Nanosecond(async));

        AssertSql();
    }

    public override async Task TimeOfDay(bool async)
    {
        await base.TimeOfDay(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddYears(bool async)
    {
        await base.AddYears(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddMonths(bool async)
    {
        await base.AddMonths(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddDays(bool async)
    {
        await base.AddDays(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddHours(bool async)
    {
        await base.AddHours(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddMinutes(bool async)
    {
        await base.AddMinutes(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddSeconds(bool async)
    {
        await base.AddSeconds(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddMilliseconds(bool async)
    {
        await base.AddMilliseconds(async);

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override Task ToUnixTimeMilliseconds(bool async)
        => AssertTranslationFailed(() => base.ToUnixTimeMilliseconds(async));

    public override Task ToUnixTimeSecond(bool async)
        => AssertTranslationFailed(() => base.ToUnixTimeSecond(async));

    public override async Task Milliseconds_parameter_and_constant(bool async)
    {
        await base.Milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM "BasicTypesEntities" AS "b"
WHERE "b"."DateTimeOffset" = '1902-01-02 10:00:00.1234567+01:30'
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
