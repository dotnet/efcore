// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeTranslationsCosmosTest : DateTimeTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public DateTimeTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
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

    public override Task UtcNow(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.UtcNow(a);

                AssertSql(
                    """
@myDatetime=?

SELECT VALUE c
FROM root c
WHERE (GetCurrentDateTime() != @myDatetime)
""");
            });

    public override async Task Today(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Today(async));

        AssertSql();
    }

    public override async Task Date(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Date(async));

        AssertSql();
    }

    public override Task AddYear(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.AddYear(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", DateTimeAdd("yyyy", 1, c["DateTime"])) = 1999)
""");
            });

    public override Task Year(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Year(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTime"]) = 1998)
""");
            });

    public override Task Month(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Month(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTime"]) = 5)
""");
            });

    public override async Task DayOfYear(bool async)
    {
        // DateTime.DayOfYear not supported by Cosmos
        await AssertTranslationFailed(() => base.DayOfYear(async));

        AssertSql();
    }

    public override Task Day(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Day(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTime"]) = 4)
""");
            });

    public override Task Hour(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Hour(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTime"]) = 15)
""");
            });

    public override Task Minute(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Minute(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTime"]) = 30)
""");
            });

    public override Task Second(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Second(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTime"]) = 10)
""");
            });

    public override Task Millisecond(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Millisecond(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTime"]) = 123)
""");
            });

    public override async Task TimeOfDay(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TimeOfDay(async));

        AssertSql();
    }

    public override async Task subtract_and_TotalDays(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.subtract_and_TotalDays(async));

        AssertSql();
    }

    public override Task Parse_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Parse_with_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = "1998-05-04T15:30:10")
""");
            });

    public override Task Parse_with_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Parse_with_parameter(a);

                AssertSql(
                    """
@Parse=?

SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = @Parse)
""");
            });

    public override Task New_with_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.New_with_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = "1998-05-04T15:30:10")
""");
            });

    public override Task New_with_parameters(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.New_with_parameters(a);

                AssertSql(
                    """
@p=?

SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = @p)
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
