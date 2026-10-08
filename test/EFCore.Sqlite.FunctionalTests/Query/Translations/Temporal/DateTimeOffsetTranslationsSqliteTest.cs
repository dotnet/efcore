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

    public override async Task Now()
    {
        await AssertTranslationFailed(() => base.Now());

        AssertSql();
    }

    public override async Task UtcNow()
    {
        await AssertTranslationFailed(() => base.UtcNow());

        AssertSql();
    }

    public override async Task Date()
    {
        await AssertTranslationFailed(() => base.Date());

        AssertSql();
    }

    public override async Task Year()
    {
        await AssertTranslationFailed(() => base.Year());

        AssertSql();
    }

    public override async Task Month()
    {
        await AssertTranslationFailed(() => base.Month());

        AssertSql();
    }

    public override async Task DayOfYear()
    {
        await AssertTranslationFailed(() => base.DayOfYear());

        AssertSql();
    }

    public override async Task Day()
    {
        await AssertTranslationFailed(() => base.Day());

        AssertSql();
    }

    public override async Task Hour()
    {
        await AssertTranslationFailed(() => base.Hour());

        AssertSql();
    }

    public override async Task Minute()
    {
        await AssertTranslationFailed(() => base.Minute());

        AssertSql();
    }

    public override async Task Second()
    {
        await AssertTranslationFailed(() => base.Second());

        AssertSql();
    }

    public override async Task Millisecond()
    {
        await AssertTranslationFailed(() => base.Millisecond());

        AssertSql();
    }

    public override async Task Microsecond()
    {
        await AssertTranslationFailed(() => base.Microsecond());

        AssertSql();
    }

    public override async Task Nanosecond()
    {
        await AssertTranslationFailed(() => base.Nanosecond());

        AssertSql();
    }

    public override async Task TimeOfDay()
    {
        await base.TimeOfDay();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddYears()
    {
        await base.AddYears();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddMonths()
    {
        await base.AddMonths();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddDays()
    {
        await base.AddDays();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddHours()
    {
        await base.AddHours();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddMinutes()
    {
        await base.AddMinutes();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddSeconds()
    {
        await base.AddSeconds();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override async Task AddMilliseconds()
    {
        await base.AddMilliseconds();

        AssertSql(
            """
SELECT "b"."DateTimeOffset"
FROM "BasicTypesEntities" AS "b"
""");
    }

    public override Task ToUnixTimeMilliseconds()
        => AssertTranslationFailed(() => base.ToUnixTimeMilliseconds());

    public override Task ToUnixTimeSecond()
        => AssertTranslationFailed(() => base.ToUnixTimeSecond());

    public override async Task Milliseconds_parameter_and_constant()
    {
        await base.Milliseconds_parameter_and_constant();

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
