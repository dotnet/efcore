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

    public override async Task UtcNow(bool async)
    {
        await base.UtcNow(async);

        AssertSql(
            """
@myDatetime=?

SELECT VALUE c
FROM root c
WHERE (GetCurrentDateTime() != @myDatetime)
""");
    }

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

    public override async Task AddYear(bool async)
    {
        await base.AddYear(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", DateTimeAdd("yyyy", 1, c["DateTime"])) = 1999)
""");
    }

    public override async Task Year(bool async)
    {
        await base.Year(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("yyyy", c["DateTime"]) = 1998)
""");
    }

    public override async Task Month(bool async)
    {
        await base.Month(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mm", c["DateTime"]) = 5)
""");
    }

    public override async Task DayOfYear(bool async)
    {
        // DateTime.DayOfYear not supported by Cosmos
        await AssertTranslationFailed(() => base.DayOfYear(async));

        AssertSql();
    }

    public override async Task Day(bool async)
    {
        await base.Day(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("dd", c["DateTime"]) = 4)
""");
    }

    public override async Task Hour(bool async)
    {
        await base.Hour(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("hh", c["DateTime"]) = 15)
""");
    }

    public override async Task Minute(bool async)
    {
        await base.Minute(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("mi", c["DateTime"]) = 30)
""");
    }

    public override async Task Second(bool async)
    {
        await base.Second(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ss", c["DateTime"]) = 10)
""");
    }

    public override async Task Millisecond(bool async)
    {
        await base.Millisecond(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (DateTimePart("ms", c["DateTime"]) = 123)
""");
    }

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

    public override async Task Parse_with_constant(bool async)
    {
        await base.Parse_with_constant(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = "1998-05-04T15:30:10")
""");
    }

    public override async Task Parse_with_parameter(bool async)
    {
        await base.Parse_with_parameter(async);

        AssertSql(
            """
@Parse=?

SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = @Parse)
""");
    }

    public override async Task New_with_constant(bool async)
    {
        await base.New_with_constant(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["DateTime"] = "1998-05-04T15:30:10")
""");
    }

    public override async Task New_with_parameters(bool async)
    {
        await base.New_with_parameters(async);

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
