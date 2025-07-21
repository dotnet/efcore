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

    public override async Task Year()
    {
        await base.Year();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(year, [b].[DateOnly]) = 1990
""");
    }

    public override async Task Month()
    {
        await base.Month();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(month, [b].[DateOnly]) = 11
""");
    }

    public override async Task Day()
    {
        await base.Day();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(day, [b].[DateOnly]) = 10
""");
    }

    public override async Task DayOfYear()
    {
        await base.DayOfYear();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(dayofyear, [b].[DateOnly]) = 314
""");
    }

    public override async Task DayOfWeek()
    {
        await AssertTranslationFailed(() => base.DayOfWeek());

        AssertSql();
    }

    public override async Task DayNumber()
    {
        await base.DayNumber();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF(day, '0001-01-01', [b].[DateOnly]) = 726780
""");
    }

    public override async Task AddYears()
    {
        await base.AddYears();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(year, CAST(3 AS int), [b].[DateOnly]) = '1993-11-10'
""");
    }

    public override async Task AddMonths()
    {
        await base.AddMonths();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(month, CAST(3 AS int), [b].[DateOnly]) = '1991-02-10'
""");
    }

    public override async Task AddDays()
    {
        await base.AddDays();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(day, CAST(3 AS int), [b].[DateOnly]) = '1990-11-13'
""");
    }

    public override async Task DayNumber_subtraction()
    {
        await base.DayNumber_subtraction();

        AssertSql(
            """
@DayNumber='726775'

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEDIFF(day, '0001-01-01', [b].[DateOnly]) - @DayNumber = 5
""");
    }

    public override async Task FromDateTime()
    {
        await base.FromDateTime();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) = '1998-05-04'
""");
    }

    public override async Task FromDateTime_compared_to_property()
    {
        await base.FromDateTime_compared_to_property();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) = [b].[DateOnly]
""");
    }

    public override async Task FromDateTime_compared_to_constant_and_parameter()
    {
        await base.FromDateTime_compared_to_constant_and_parameter();

        AssertSql(
            """
@dateOnly='10/11/0002' (DbType = Date)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS date) IN (@dateOnly, '1998-05-04')
""");
    }

    public override async Task ToDateTime_property_with_constant_TimeOnly()
    {
        await base.ToDateTime_property_with_constant_TimeOnly();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATETIME2FROMPARTS(DATEPART(year, [b].[DateOnly]), DATEPART(month, [b].[DateOnly]), DATEPART(day, [b].[DateOnly]), 21, 5, 19, 9405000, 7) = '2020-01-01T21:05:19.9405000'
""");
    }

    public override async Task ToDateTime_property_with_property_TimeOnly()
    {
        await base.ToDateTime_property_with_property_TimeOnly();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATETIME2FROMPARTS(DATEPART(year, [b].[DateOnly]), DATEPART(month, [b].[DateOnly]), DATEPART(day, [b].[DateOnly]), DATEPART(hour, [b].[TimeOnly]), DATEPART(minute, [b].[TimeOnly]), DATEPART(second, [b].[TimeOnly]), DATEPART(nanosecond, [b].[TimeOnly]) / 100, 7) = '2020-01-01T15:30:10.0000000'
""");
    }

    public override async Task ToDateTime_constant_DateTime_with_property_TimeOnly()
    {
        await base.ToDateTime_constant_DateTime_with_property_TimeOnly();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATETIME2FROMPARTS(1990, 11, 10, DATEPART(hour, [b].[TimeOnly]), DATEPART(minute, [b].[TimeOnly]), DATEPART(second, [b].[TimeOnly]), DATEPART(nanosecond, [b].[TimeOnly]) / 100, 7) = '1990-11-10T15:30:10.0000000'
""");
    }

    public override async Task ToDateTime_with_complex_DateTime()
    {
        await AssertTranslationFailed(() => base.ToDateTime_with_complex_DateTime());

        AssertSql();
    }

    public override async Task ToDateTime_with_complex_TimeOnly()
    {
        await AssertTranslationFailed(() => base.ToDateTime_with_complex_TimeOnly());

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
