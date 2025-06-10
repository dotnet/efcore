// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class DateOnlyTranslationsSqlServerTest : DateOnlyTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public DateOnlyTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Year(bool async)
    {
        await base.Year(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, [b].[DateOnly]) = 1990
""");
    }

    public override async Task Month(bool async)
    {
        await base.Month(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(month, [b].[DateOnly]) = 11
""");
    }

    public override async Task Day(bool async)
    {
        await base.Day(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(day, [b].[DateOnly]) = 10
""");
    }

    public override async Task DayOfYear(bool async)
    {
        await base.DayOfYear(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(dayofyear, [b].[DateOnly]) = 314
""");
    }

    public override async Task DayOfWeek(bool async)
    {
        await AssertTranslationFailed(() => base.DayOfWeek(async));

        AssertSql();
    }

    public override async Task DayNumber(bool async)
    {
        await base.DayNumber(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF(day, '0001-01-01', [b].[DateOnly]) = 726780
""");
    }

    public override async Task AddYears(bool async)
    {
        await base.AddYears(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(year, CAST(3 AS int), [b].[DateOnly]) = '1993-11-10'
""");
    }

    public override async Task AddMonths(bool async)
    {
        await base.AddMonths(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(month, CAST(3 AS int), [b].[DateOnly]) = '1991-02-10'
""");
    }

    public override async Task AddDays(bool async)
    {
        await base.AddDays(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(day, CAST(3 AS int), [b].[DateOnly]) = '1990-11-13'
""");
    }

    public override async Task DayNumber_subtraction(bool async)
    {
        await base.DayNumber_subtraction(async);

        AssertSql(
            """
@DayNumber='726775'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF(day, '0001-01-01', [b].[DateOnly]) - @DayNumber = 5
""");
    }

    public override async Task FromDateTime(bool async)
    {
        await base.FromDateTime(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) = '1998-05-04'
""");
    }

    public override async Task FromDateTime_compared_to_property(bool async)
    {
        await base.FromDateTime_compared_to_property(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) = [b].[DateOnly]
""");
    }

    public override async Task FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        await base.FromDateTime_compared_to_constant_and_parameter(async);

        AssertSql(
            """
@dateOnly='10/11/0002' (DbType = Date)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) IN (@dateOnly, '1998-05-04')
""");
    }

    public override async Task ToDateTime_property_with_constant_TimeOnly(bool async)
    {
        await base.ToDateTime_property_with_constant_TimeOnly(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATETIME2FROMPARTS(DATEPART(year, [b].[DateOnly]), DATEPART(month, [b].[DateOnly]), DATEPART(day, [b].[DateOnly]), 21, 5, 19, 9405000, 7) = '2020-01-01T21:05:19.9405000'
""");
    }

    public override async Task ToDateTime_property_with_property_TimeOnly(bool async)
    {
        await base.ToDateTime_property_with_property_TimeOnly(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATETIME2FROMPARTS(DATEPART(year, [b].[DateOnly]), DATEPART(month, [b].[DateOnly]), DATEPART(day, [b].[DateOnly]), DATEPART(hour, [b].[TimeOnly]), DATEPART(minute, [b].[TimeOnly]), DATEPART(second, [b].[TimeOnly]), DATEPART(nanosecond, [b].[TimeOnly]) / 100, 7) = '2020-01-01T15:30:10.0000000'
""");
    }

    public override async Task ToDateTime_constant_DateTime_with_property_TimeOnly(bool async)
    {
        await base.ToDateTime_constant_DateTime_with_property_TimeOnly(async);

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATETIME2FROMPARTS(1990, 11, 10, DATEPART(hour, [b].[TimeOnly]), DATEPART(minute, [b].[TimeOnly]), DATEPART(second, [b].[TimeOnly]), DATEPART(nanosecond, [b].[TimeOnly]) / 100, 7) = '1990-11-10T15:30:10.0000000'
""");
    }

    public override async Task ToDateTime_with_complex_DateTime(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_with_complex_DateTime(async));

        AssertSql();
    }

    public override async Task ToDateTime_with_complex_TimeOnly(bool async)
    {
        await AssertTranslationFailed(() => base.ToDateTime_with_complex_TimeOnly(async));

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
