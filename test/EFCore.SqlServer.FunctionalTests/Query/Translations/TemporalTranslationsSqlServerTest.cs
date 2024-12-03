// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class TemporalTranslationsSqlServerTest : TemporalTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public TemporalTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region DateTime

    public override async Task DateTime_Now(bool async)
    {
        await base.DateTime_Now(async);

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GETDATE() <> @myDatetime
""");
    }

    public override async Task DateTime_UtcNow(bool async)
    {
        await base.DateTime_UtcNow(async);

        AssertSql(
            """
@myDatetime='2015-04-10T00:00:00.0000000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE GETUTCDATE() <> @myDatetime
""");
    }

    public override async Task DateTime_Today(bool async)
    {
        await base.DateTime_Today(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTime] = CONVERT(date, GETDATE())
""");
    }

    public override async Task DateTime_Date(bool async)
    {
        await base.DateTime_Date(async);

        AssertSql(
            """
@myDatetime='1998-05-04T00:00:00.0000000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONVERT(date, [b].[DateTime]) = @myDatetime
""");
    }

    public override async Task DateTime_AddYear(bool async)
    {
        await base.DateTime_AddYear(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, DATEADD(year, CAST(1 AS int), [b].[DateTime])) = 1999
""");
    }

    public override async Task DateTime_Year(bool async)
    {
        await base.DateTime_Year(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, [b].[DateTime]) = 1998
""");
    }

    public override async Task DateTime_Month(bool async)
    {
        await base.DateTime_Month(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(month, [b].[DateTime]) = 5
""");
    }

    public override async Task DateTime_DayOfYear(bool async)
    {
        await base.DateTime_DayOfYear(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(dayofyear, [b].[DateTime]) = 124
""");
    }

    public override async Task DateTime_Day(bool async)
    {
        await base.DateTime_Day(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(day, [b].[DateTime]) = 4
""");
    }

    public override async Task DateTime_Hour(bool async)
    {
        await base.DateTime_Hour(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[DateTime]) = 15
""");
    }

    public override async Task DateTime_Minute(bool async)
    {
        await base.DateTime_Minute(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[DateTime]) = 30
""");
    }

    public override async Task DateTime_Second(bool async)
    {
        await base.DateTime_Second(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[DateTime]) = 10
""");
    }

    public override async Task DateTime_Millisecond(bool async)
    {
        await base.DateTime_Millisecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[DateTime]) = 123
""");
    }

    public override async Task DateTime_TimeOfDay(bool async)
    {
        await base.DateTime_TimeOfDay(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONVERT(time, [b].[DateTime]) = '00:00:00'
""");
    }

    public override Task DateTime_subtract_and_TotalDays(bool async)
        => AssertTranslationFailed(() => base.DateTime_subtract_and_TotalDays(async));

    #endregion DateTime

    #region DateOnly

    public override async Task DateOnly_Year(bool async)
    {
        await base.DateOnly_Year(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, [b].[DateOnly]) = 1990
""");
    }

    public override async Task DateOnly_Month(bool async)
    {
        await base.DateOnly_Month(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(month, [b].[DateOnly]) = 11
""");
    }

    public override async Task DateOnly_Day(bool async)
    {
        await base.DateOnly_Day(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(day, [b].[DateOnly]) = 10
""");
    }

    public override async Task DateOnly_DayOfYear(bool async)
    {
        await base.DateOnly_DayOfYear(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(dayofyear, [b].[DateOnly]) = 314
""");
    }

    public override async Task DateOnly_DayOfWeek(bool async)
    {
        await AssertTranslationFailed(() => base.DateOnly_DayOfWeek(async));

        AssertSql();
    }

    public override async Task DateOnly_AddYears(bool async)
    {
        await base.DateOnly_AddYears(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(year, CAST(3 AS int), [b].[DateOnly]) = '1993-11-10'
""");
    }

    public override async Task DateOnly_AddMonths(bool async)
    {
        await base.DateOnly_AddMonths(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(month, CAST(3 AS int), [b].[DateOnly]) = '1991-02-10'
""");
    }

    public override async Task DateOnly_AddDays(bool async)
    {
        await base.DateOnly_AddDays(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(day, CAST(3 AS int), [b].[DateOnly]) = '1990-11-13'
""");
    }

    public override async Task DateOnly_FromDateTime(bool async)
    {
        await base.DateOnly_FromDateTime(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) = '1998-05-04'
""");
    }

    public override async Task DateOnly_FromDateTime_compared_to_property(bool async)
    {
        await base.DateOnly_FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) = [b].[DateOnly]
""");
    }

    public override async Task DateOnly_FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        await base.DateOnly_FromDateTime_compared_to_constant_and_parameter(async);

        AssertSql(
            """
@dateOnly='10/11/0002' (DbType = Date)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) IN (@dateOnly, '1998-05-04')
""");
    }

    #endregion DateOnly

    #region TimeOnly

    public override async Task TimeOnly_Hour(bool async)
    {
        await base.TimeOnly_Hour(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[TimeOnly]) = 15
""");
    }

    public override async Task TimeOnly_Minute(bool async)
    {
        await base.TimeOnly_Minute(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[TimeOnly]) = 30
""");
    }

    public override async Task TimeOnly_Second(bool async)
    {
        await base.TimeOnly_Second(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[TimeOnly]) = 10
""");
    }

    public override async Task TimeOnly_Millisecond(bool async)
    {
        await base.TimeOnly_Millisecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[TimeOnly]) = 123
""");
    }

    public override async Task TimeOnly_Microsecond(bool async)
    {
        await base.TimeOnly_Microsecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[TimeOnly]) % 1000 = 456
""");
    }

    public override async Task TimeOnly_Nanosecond(bool async)
    {
        await base.TimeOnly_Nanosecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[TimeOnly]) % 1000 = 400
""");
    }

    public override async Task TimeOnly_AddHours(bool async)
    {
        await base.TimeOnly_AddHours(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(hour, CAST(3.0E0 AS int), [b].[TimeOnly]) = '18:30:10'
""");
    }

    public override async Task TimeOnly_AddMinutes(bool async)
    {
        await base.TimeOnly_AddMinutes(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(minute, CAST(3.0E0 AS int), [b].[TimeOnly]) = '15:33:10'
""");
    }

    public override async Task TimeOnly_Add_TimeSpan(bool async)
    {
        await AssertTranslationFailed(() => base.TimeOnly_Add_TimeSpan(async));

        AssertSql();
    }

    public override async Task TimeOnly_IsBetween(bool async)
    {
        await base.TimeOnly_IsBetween(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CASE
    WHEN [b].[TimeOnly] >= '14:00:00' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END & CASE
    WHEN [b].[TimeOnly] < '16:00:00' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END = CAST(1 AS bit)
""");
    }

    public override async Task TimeOnly_subtract_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.TimeOnly_subtract_TimeOnly(async));

        AssertSql();
    }

    public override async Task TimeOnly_FromDateTime_compared_to_property(bool async)
    {
        await base.TimeOnly_FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS time) = [b].[TimeOnly]
""");
    }

    public override async Task TimeOnly_FromDateTime_compared_to_parameter(bool async)
    {
        await base.TimeOnly_FromDateTime_compared_to_parameter(async);

        AssertSql(
            """
@time='15:30' (DbType = Time)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS time) = @time
""");
    }

    public override async Task TimeOnly_FromDateTime_compared_to_constant(bool async)
    {
        await base.TimeOnly_FromDateTime_compared_to_constant(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS time) = '15:30:10'
""");
    }

    public override async Task TimeOnly_FromTimeSpan_compared_to_property(bool async)
    {
        await base.TimeOnly_FromTimeSpan_compared_to_property(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[TimeSpan] AS time) < [b].[TimeOnly]
""");
    }

    public override async Task TimeOnly_FromTimeSpan_compared_to_parameter(bool async)
    {
        await base.TimeOnly_FromTimeSpan_compared_to_parameter(async);

        AssertSql(
            """
@time='01:02' (DbType = Time)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[TimeSpan] AS time) = @time
""");
    }

    public override async Task Order_by_TimeOnly_FromTimeSpan(bool async)
    {
        await base.Order_by_TimeOnly_FromTimeSpan(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
ORDER BY CAST([b].[TimeSpan] AS time)
""");
    }

    #endregion TimeOnly

    #region DateTimeOffset

    public override async Task DateTimeOffset_Now(bool async)
    {
        await base.DateTimeOffset_Now(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTimeOffset] <> SYSDATETIMEOFFSET()
""");
    }

    public override async Task DateTimeOffset_UtcNow(bool async)
    {
        await base.DateTimeOffset_UtcNow(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTimeOffset] <> CAST(SYSUTCDATETIME() AS datetimeoffset)
""");
    }

    public override async Task DateTimeOffset_Date(bool async)
    {
        await base.DateTimeOffset_Date(async);

        AssertSql(
            """
@Date='0001-01-01T00:00:00.0000000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CONVERT(date, [b].[DateTimeOffset]) > @Date
""");
    }

    public override async Task DateTimeOffset_Year(bool async)
    {
        await base.DateTimeOffset_Year(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, [b].[DateTimeOffset]) = 1998
""");
    }

    public override async Task DateTimeOffset_Month(bool async)
    {
        await base.DateTimeOffset_Month(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(month, [b].[DateTimeOffset]) = 5
""");
    }

    public override async Task DateTimeOffset_DayOfYear(bool async)
    {
        await base.DateTimeOffset_DayOfYear(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(dayofyear, [b].[DateTimeOffset]) = 124
""");
    }

    public override async Task DateTimeOffset_Day(bool async)
    {
        await base.DateTimeOffset_Day(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(day, [b].[DateTimeOffset]) = 4
""");
    }

    public override async Task DateTimeOffset_Hour(bool async)
    {
        await base.DateTimeOffset_Hour(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[DateTimeOffset]) = 15
""");
    }

    public override async Task DateTimeOffset_Minute(bool async)
    {
        await base.DateTimeOffset_Minute(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[DateTimeOffset]) = 30
""");
    }

    public override async Task DateTimeOffset_Second(bool async)
    {
        await base.DateTimeOffset_Second(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[DateTimeOffset]) = 10
""");
    }

    public override async Task DateTimeOffset_Millisecond(bool async)
    {
        await base.DateTimeOffset_Millisecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[DateTimeOffset]) = 123
""");
    }

    public override async Task DateTimeOffset_Microsecond(bool async)
    {
        await base.DateTimeOffset_Microsecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[DateTimeOffset]) % 1000 = 456
""");
    }

    public override async Task DateTimeOffset_Nanosecond(bool async)
    {
        await base.DateTimeOffset_Nanosecond(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[DateTimeOffset]) % 1000 = 400
""");
    }

    public override async Task DateTimeOffset_TimeOfDay(bool async)
    {
        await base.DateTimeOffset_TimeOfDay(async);

        AssertSql(
            """
SELECT CONVERT(time, [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddYears(bool async)
    {
        await base.DateTimeOffset_AddYears(async);

        AssertSql(
            """
SELECT DATEADD(year, CAST(1 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddMonths(bool async)
    {
        await base.DateTimeOffset_AddMonths(async);

        AssertSql(
            """
SELECT DATEADD(month, CAST(1 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddDays(bool async)
    {
        await base.DateTimeOffset_AddDays(async);

        AssertSql(
            """
SELECT DATEADD(day, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddHours(bool async)
    {
        await base.DateTimeOffset_AddHours(async);

        AssertSql(
            """
SELECT DATEADD(hour, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddMinutes(bool async)
    {
        await base.DateTimeOffset_AddMinutes(async);

        AssertSql(
            """
SELECT DATEADD(minute, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddSeconds(bool async)
    {
        await base.DateTimeOffset_AddSeconds(async);

        AssertSql(
            """
SELECT DATEADD(second, CAST(1.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_AddMilliseconds(bool async)
    {
        await base.DateTimeOffset_AddMilliseconds(async);

        AssertSql(
            """
SELECT DATEADD(millisecond, CAST(300.0E0 AS int), [b].[DateTimeOffset])
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task DateTimeOffset_ToUnixTimeMilliseconds(bool async)
    {
        await base.DateTimeOffset_ToUnixTimeMilliseconds(async);

        AssertSql(
            """
@unixEpochMilliseconds='894295810000'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF_BIG(millisecond, '1970-01-01T00:00:00.0000000+00:00', [b].[DateTimeOffset]) = @unixEpochMilliseconds
""");
    }

    public override async Task DateTimeOffset_ToUnixTimeSecond(bool async)
    {
        await base.DateTimeOffset_ToUnixTimeSecond(async);

        AssertSql(
            """
@unixEpochSeconds='894295810'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF_BIG(second, '1970-01-01T00:00:00.0000000+00:00', [b].[DateTimeOffset]) = @unixEpochSeconds
""");
    }

    public override async Task DateTimeOffset_milliseconds_parameter_and_constant(bool async)
    {
        await base.DateTimeOffset_milliseconds_parameter_and_constant(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [BasicTypesEntities] AS [b]
WHERE [b].[DateTimeOffset] = '1902-01-02T10:00:00.1234567+01:30'
""");
    }

    #endregion DateTimeOffset

    #region TimeSpan

    public override async Task TimeSpan_Hours(bool async)
    {
        await base.TimeSpan_Hours(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[TimeSpan]) = 3
""");
    }

    public override async Task TimeSpan_Minutes(bool async)
    {
        await base.TimeSpan_Minutes(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[TimeSpan]) = 4
""");
    }

    public override async Task TimeSpan_Seconds(bool async)
    {
        await base.TimeSpan_Seconds(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[TimeSpan]) = 5
""");
    }

    public override async Task TimeSpan_Milliseconds(bool async)
    {
        await base.TimeSpan_Milliseconds(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[TimeSpan]) = 678
""");
    }

    public override async Task TimeSpan_Microseconds(bool async)
    {
        await base.TimeSpan_Microseconds(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[TimeSpan]) % 1000 = 912
""");
    }

    public override async Task TimeSpan_Nanoseconds(bool async)
    {
        await base.TimeSpan_Nanoseconds(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[TimeSpan]) % 1000 = 400
""");
    }

    #endregion TimeSpan

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
