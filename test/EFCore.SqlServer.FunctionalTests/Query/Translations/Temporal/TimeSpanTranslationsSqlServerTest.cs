// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeSpanTranslationsSqlServerTest : TimeSpanTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public TimeSpanTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hours()
    {
        await base.Hours();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[TimeSpan]) = 3
""");
    }

    public override async Task Minutes()
    {
        await base.Minutes();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[TimeSpan]) = 4
""");
    }

    public override async Task Seconds()
    {
        await base.Seconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[TimeSpan]) = 5
""");
    }

    public override async Task Milliseconds()
    {
        await base.Milliseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[TimeSpan]) = 678
""");
    }

    public override async Task Microseconds()
    {
        await base.Microseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[TimeSpan]) % 1000 = 912
""");
    }

    public override async Task Nanoseconds()
    {
        await base.Nanoseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[TimeSpan]) % 1000 = 400
""");
    }

    public override async Task TotalDays()
    {
        await base.TotalDays();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 864000000000.0E0 > 0.10000000000000001E0
""");
    }

    public override async Task TotalHours()
    {
        await base.TotalHours();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 36000000000.0E0 > 2.5E0
""");
    }

    public override async Task TotalMinutes()
    {
        await base.TotalMinutes();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 600000000.0E0 > 120.25E0
""");
    }

    public override async Task TotalSeconds()
    {
        await base.TotalSeconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 10000000.0E0 > 7215.3999999999996E0
""");
    }

    public override async Task TotalMilliseconds()
    {
        await base.TotalMilliseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 10000.0E0 > 7215456.2000000002E0
""");
    }

    public override async Task TotalMicroseconds()
    {
        await base.TotalMicroseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 10.0E0 > 7215456200.3000002E0
""");
    }

    public override async Task TotalNanoseconds()
    {
        await base.TotalNanoseconds();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) * 100.0E0 > 7215456200300.0E0
""");
    }

    // The ticks are divided once, as in .NET, so the projected values are the same doubles.
    [Fact]
    public virtual async Task TotalHours_in_projection()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Select(b => b.TimeSpan.TotalHours));

        AssertSql(
            """
SELECT CAST(CAST(DATEPART(hour, [b].[TimeSpan]) * 3600 + DATEPART(minute, [b].[TimeSpan]) * 60 + DATEPART(second, [b].[TimeSpan]) AS bigint) * CAST(10000000 AS bigint) + DATEPART(nanosecond, [b].[TimeSpan]) / CAST(100 AS bigint) AS float) / 36000000000.0E0
FROM [BasicTypesEntities] AS [b]
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
