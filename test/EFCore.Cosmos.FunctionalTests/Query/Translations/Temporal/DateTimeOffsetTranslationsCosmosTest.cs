// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsCosmosTest : DateTimeOffsetTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public DateTimeOffsetTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Now()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Now());

        AssertSql();
    }

    public override async Task UtcNow()
    {
        await base.UtcNow();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTimeOffset"] != GetCurrentDateTime())
""");
    }

    public override async Task Date()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Date());

        AssertSql();
    }

    public override async Task Year()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Year());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTimeOffset"]) = 1998)
""");
    }

    public override async Task Month()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Month());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTimeOffset"]) = 5)
""");
    }

    public override async Task DayOfYear()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DayOfYear());

        AssertSql();
    }

    public override async Task Day()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Day());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTimeOffset"]) = 4)
""");
    }

    public override async Task Hour()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Hour());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTimeOffset"]) = 15)
""");
    }

    public override async Task Minute()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Minute());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTimeOffset"]) = 30)
""");
    }

    public override async Task Second()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Second());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTimeOffset"]) = 10)
""");
    }

    public override async Task Millisecond()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Millisecond());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTimeOffset"]) = 123)
""");
    }

    public override async Task Microsecond()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Microsecond());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((DateTimePart("mcs", c["DateTimeOffset"]) % 1000) = 456)
""");
    }

    public override async Task Nanosecond()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Nanosecond());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((DateTimePart("ns", c["DateTimeOffset"]) % 1000) = 400)
""");
    }

    public override async Task TimeOfDay()
    {
        await base.TimeOfDay();

        AssertSql(
            """
SELECT VALUE c["DateTimeOffset"]
FROM root c
""");
    }

    public override async Task AddYears()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddYears());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("yyyy", 1, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddMonths()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddMonths());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("mm", 1, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddDays()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddSeconds());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("ss", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddHours()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddHours());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("hh", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddMinutes()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddMinutes());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("mi", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddSeconds()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddSeconds());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("ss", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddMilliseconds()
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddMilliseconds());

        AssertSql(
            """
SELECT VALUE DateTimeAdd("ms", 300.0, c["DateTimeOffset"])
FROM root c
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
SELECT VALUE COUNT(1)
FROM root c
WHERE (c["DateTimeOffset"] = "1902-01-02T10:00:00.1234567+01:30")
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
