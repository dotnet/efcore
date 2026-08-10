// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeTranslationsCosmosTest : DateTimeTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public DateTimeTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
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
@myDatetime=?

SELECT VALUE c
FROM root c
WHERE (GetCurrentDateTime() != @myDatetime)
""");
    }

    public override async Task Today()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Today());

        AssertSql();
    }

    public override async Task Date()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Date());

        AssertSql();
    }

    public override async Task AddYear()
    {
        await base.AddYear();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", DateTimeAdd("yyyy", 1, c["DateTime"])) = 1999)
""");
    }

    public override async Task Year()
    {
        await base.Year();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTime"]) = 1998)
""");
    }

    public override async Task Month()
    {
        await base.Month();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTime"]) = 5)
""");
    }

    public override async Task DayOfYear()
    {
        // DateTime.DayOfYear not supported by Cosmos
        await AssertTranslationFailed(() => base.DayOfYear());

        AssertSql();
    }

    public override async Task Day()
    {
        await base.Day();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTime"]) = 4)
""");
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTime"]) = 15)
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTime"]) = 30)
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTime"]) = 10)
""");
    }

    public override async Task Millisecond()
    {
        await base.Millisecond();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTime"]) = 123)
""");
    }

    public override async Task TimeOfDay()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.TimeOfDay());

        AssertSql();
    }

    public override async Task subtract_and_TotalDays()
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.subtract_and_TotalDays());

        AssertSql();
    }

    public override async Task Parse_with_constant()
    {
        await base.Parse_with_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = "1998-05-04T15:30:10")
""");
    }

    public override async Task Parse_with_parameter()
    {
        await base.Parse_with_parameter();

        AssertSql(
            """
@Parse=?

SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = @Parse)
""");
    }

    public override async Task New_with_constant()
    {
        await base.New_with_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = "1998-05-04T15:30:10")
""");
    }

    public override async Task New_with_parameters()
    {
        await base.New_with_parameters();

        AssertSql(
            """
@p=?

SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = @p)
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
