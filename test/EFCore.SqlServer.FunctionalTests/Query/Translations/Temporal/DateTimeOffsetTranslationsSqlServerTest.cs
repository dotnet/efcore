// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateTimeOffsetTranslationsSqlServerTest : DateTimeOffsetTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public DateTimeOffsetTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Now(bool async)
    {
        await base.Now(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTimeOffset] <> SYSDATETIMEOFFSET()
""");
    }

    public override async Task UtcNow(bool async)
    {
        await base.UtcNow(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTimeOffset] <> CAST(SYSUTCDATETIME() AS datetimeoffset)
""");
    }

    public override async Task Date(bool async)
    {
        await base.Date(async);

        AssertSql(
            """
@Date='0001-01-01T00:00:00.0000000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONVERT(date, [b].[DateTimeOffset]) > @Date
""");
    }

    public override async Task Year(bool async)
    {
        await base.Year(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, [b].[DateTimeOffset]) = 1998
""");
    }

    public override async Task Month(bool async)
    {
        await base.Month(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(month, [b].[DateTimeOffset]) = 5
""");
    }

    public override async Task DayOfYear(bool async)
    {
        await base.DayOfYear(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(dayofyear, [b].[DateTimeOffset]) = 124
""");
    }

    public override async Task Day(bool async)
    {
        await base.Day(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(day, [b].[DateTimeOffset]) = 4
""");
    }

    public override async Task Hour(bool async)
    {
        await base.Hour(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[DateTimeOffset]) = 15
""");
    }

    public override async Task Minute(bool async)
    {
        await base.Minute(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[DateTimeOffset]) = 30
""");
    }

    public override async Task Second(bool async)
    {
        await base.Second(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[DateTimeOffset]) = 10
""");
    }

    public override async Task Millisecond(bool async)
    {
        await base.Millisecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[DateTimeOffset]) = 123
""");
    }

    public override async Task Microsecond(bool async)
    {
        await base.Microsecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[DateTimeOffset]) % 1000 = 456
""");
    }

    public override async Task Nanosecond(bool async)
    {
        await base.Nanosecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[DateTimeOffset]) % 1000 = 400
""");
    }

    public override async Task TimeOfDay(bool async)
    {
        await base.TimeOfDay(async);

        AssertSql(
            """
SELECT CONVERT(time, [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddYears(bool async)
    {
        await base.AddYears(async);

        AssertSql(
            """
SELECT DATEADD(year, CAST(1 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddMonths(bool async)
    {
        await base.AddMonths(async);

        AssertSql(
            """
SELECT DATEADD(month, CAST(1 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddDays(bool async)
    {
        await base.AddDays(async);

        AssertSql(
            """
SELECT DATEADD(day, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddHours(bool async)
    {
        await base.AddHours(async);

        AssertSql(
            """
SELECT DATEADD(hour, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddMinutes(bool async)
    {
        await base.AddMinutes(async);

        AssertSql(
            """
SELECT DATEADD(minute, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddSeconds(bool async)
    {
        await base.AddSeconds(async);

        AssertSql(
            """
SELECT DATEADD(second, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task AddMilliseconds(bool async)
    {
        await base.AddMilliseconds(async);

        AssertSql(
            """
SELECT DATEADD(millisecond, CAST(300.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task ToUnixTimeMilliseconds(bool async)
    {
        await base.ToUnixTimeMilliseconds(async);

        AssertSql(
            """
@unixEpochMilliseconds='894295810000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF_BIG(millisecond, '1970-01-01T00:00:00.0000000+00:00', [b].[DateTimeOffset]) = @unixEpochMilliseconds
""");
    }

    public override async Task ToUnixTimeSecond(bool async)
    {
        await base.ToUnixTimeSecond(async);

        AssertSql(
            """
@unixEpochSeconds='894295810'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF_BIG(second, '1970-01-01T00:00:00.0000000+00:00', [b].[DateTimeOffset]) = @unixEpochSeconds
""");
    }

    public override async Task Milliseconds_parameter_and_constant(bool async)
    {
        await base.Milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTimeOffset] = '1902-01-02T10:00:00.1234567+01:30'
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
