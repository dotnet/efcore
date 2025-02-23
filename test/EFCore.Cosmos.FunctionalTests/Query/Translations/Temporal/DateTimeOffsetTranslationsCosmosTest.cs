// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsCosmosTest : DateTimeOffsetTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public DateTimeOffsetTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Now(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Now(async));

        AssertSql();
    }

    public override async Task UtcNow(bool async)
    {
        await base.UtcNow(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTimeOffset"] != GetCurrentDateTime())
""");
    }

    public override async Task Date(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Date(async));

        AssertSql();
    }

    public override async Task Year(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Year(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTimeOffset"]) = 1998)
""");
    }

    public override async Task Month(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Month(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTimeOffset"]) = 5)
""");
    }

    public override async Task DayOfYear(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.DayOfYear(async));

        AssertSql();
    }

    public override async Task Day(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Day(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTimeOffset"]) = 4)
""");
    }

    public override async Task Hour(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Hour(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTimeOffset"]) = 15)
""");
    }

    public override async Task Minute(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Minute(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTimeOffset"]) = 30)
""");
    }

    public override async Task Second(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Second(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTimeOffset"]) = 10)
""");
    }

    public override async Task Millisecond(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Millisecond(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTimeOffset"]) = 123)
""");
    }

    public override async Task Microsecond(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Microsecond(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((DateTimePart("mcs", c["DateTimeOffset"]) % 1000) = 456)
""");
    }

    public override async Task Nanosecond(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.Nanosecond(async));

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((DateTimePart("ns", c["DateTimeOffset"]) % 1000) = 400)
""");
    }

    public override async Task TimeOfDay(bool async)
    {
        await base.TimeOfDay(async);

        AssertSql(
            """
SELECT VALUE c["DateTimeOffset"]
FROM root c
""");
    }

    public override async Task AddYears(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddYears(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("yyyy", 1, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddMonths(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddMonths(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("mm", 1, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddDays(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddSeconds(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("ss", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddHours(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddHours(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("hh", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddMinutes(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddMinutes(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("mi", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddSeconds(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddSeconds(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("ss", 1.0, c["DateTimeOffset"])
FROM root c
""");
    }

    public override async Task AddMilliseconds(bool async)
    {
        // Our persisted representation of DateTimeOffset (xxx+00:00) isn't supported by Cosmos (should be xxxZ). #35310
        await Assert.ThrowsAsync<EqualException>(() => base.AddMilliseconds(async));

        AssertSql(
            """
SELECT VALUE DateTimeAdd("ms", 300.0, c["DateTimeOffset"])
FROM root c
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
