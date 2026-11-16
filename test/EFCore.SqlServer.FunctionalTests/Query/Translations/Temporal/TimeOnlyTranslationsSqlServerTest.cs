// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public class TimeOnlyTranslationsSqlServerTest : TimeOnlyTranslationsTestBase<BasicTypesQuerySqlServerFixture>
{
    public TimeOnlyTranslationsSqlServerTest(BasicTypesQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Hour()
    {
        await base.Hour();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(hour, [b].[TimeOnly]) = 15
""");
    }

    public override async Task Minute()
    {
        await base.Minute();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(minute, [b].[TimeOnly]) = 30
""");
    }

    public override async Task Second()
    {
        await base.Second();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(second, [b].[TimeOnly]) = 10
""");
    }

    public override async Task Millisecond()
    {
        await base.Millisecond();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(millisecond, [b].[TimeOnly]) = 123
""");
    }

    public override async Task Microsecond()
    {
        await base.Microsecond();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(microsecond, [b].[TimeOnly]) % 1000 = 456
""");
    }

    public override async Task Nanosecond()
    {
        await base.Nanosecond();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEPART(nanosecond, [b].[TimeOnly]) % 1000 = 400
""");
    }

    public override async Task AddHours()
    {
        await base.AddHours();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(hour, CAST(3.0E0 AS int), [b].[TimeOnly]) = '18:30:10'
""");
    }

    public override async Task AddMinutes()
    {
        await base.AddMinutes();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE DATEADD(minute, CAST(3.0E0 AS int), [b].[TimeOnly]) = '15:33:10'
""");
    }

    public override async Task Add_TimeSpan()
    {
        await AssertTranslationFailed(base.Add_TimeSpan);

        AssertSql();
    }

    public override async Task IsBetween()
    {
        await base.IsBetween();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[TimeOnly] >= '14:00:00' AND [b].[TimeOnly] < '16:00:00'
""");
    }

    [Fact]
    public virtual async Task IsBetween_spanning_midnight_with_parameters()
    {
        var start = new TimeOnly(16, 0, 0);
        var end = new TimeOnly(12, 0, 0);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(start, end)));

        AssertSql(
            """
@start='16:00' (DbType = Time)
@end='12:00' (DbType = Time)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE (@start <= @end AND [b].[TimeOnly] >= @start AND [b].[TimeOnly] < @end) OR (@start > @end AND ([b].[TimeOnly] >= @start OR [b].[TimeOnly] < @end))
""");
    }

    [Fact]
    public virtual async Task IsBetween_with_parameters()
    {
        var start = new TimeOnly(14, 0, 0);
        var end = new TimeOnly(16, 0, 0);

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(start, end)));

        AssertSql(
            """
@start='14:00' (DbType = Time)
@end='16:00' (DbType = Time)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE (@start <= @end AND [b].[TimeOnly] >= @start AND [b].[TimeOnly] < @end) OR (@start > @end AND ([b].[TimeOnly] >= @start OR [b].[TimeOnly] < @end))
""");
    }

    [Fact]
    public virtual async Task IsBetween_spanning_midnight()
    {
        await AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(new TimeOnly(16, 0, 0), new TimeOnly(12, 0, 0))));

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[TimeOnly] >= '16:00:00' OR [b].[TimeOnly] < '12:00:00'
""");
    }

    public override async Task IsBetween_midnight(bool async)
    {
        await base.IsBetween_midnight(async);

        AssertSql(
            """
SELECT [b].[Id]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[TimeOnly] >= '22:00:00' OR [b].[TimeOnly] < '02:00:00'
""");
    }

    public override async Task IsBetween_parameters(bool async, int range)
    {
        await base.IsBetween_parameters(async, range);

        AssertSql(
            $$"""
@start='{{(range == 0 ? "14:00" : range == 3 ? "00:00" : "22:00")}}' (DbType = Time)
@end='{{(range == 0 ? "16:00" : range == 1 ? "02:00" : range == 3 ? "00:00" : "22:00")}}' (DbType = Time)

SELECT [b].[Id]
FROM [BasicTypesEntities] AS [b]
WHERE (@start <= @end AND [b].[TimeOnly] >= @start AND [b].[TimeOnly] < @end) OR (@start > @end AND ([b].[TimeOnly] >= @start OR [b].[TimeOnly] < @end))
""");
    }

    public override async Task IsBetween_boundaries(bool async)
    {
        await base.IsBetween_boundaries(async);

        AssertSql(
            """
SELECT [b].[Id], CASE
    WHEN [b].[TimeOnly] >= '15:30:10' AND [b].[TimeOnly] < '16:00:00' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Start], CASE
    WHEN [b].[TimeOnly] >= '14:00:00' AND [b].[TimeOnly] < '15:30:10' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [End], CASE
    WHEN [b].[TimeOnly] >= '15:30:10' OR [b].[TimeOnly] < '14:00:00' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [WrappedStart], CASE
    WHEN [b].[TimeOnly] >= '16:00:00' OR [b].[TimeOnly] < '15:30:10' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [WrappedEnd], CASE
    WHEN [b].[TimeOnly] >= '15:30:10' AND [b].[TimeOnly] < '15:30:10' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Empty]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task IsBetween_precision(bool async)
    {
        await base.IsBetween_precision(async);

        AssertSql(
            """
SELECT [b].[Id], CASE
    WHEN [b].[TimeOnly] >= '00:00:00.0102004' AND [b].[TimeOnly] < '00:00:00.0102005' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Start], CASE
    WHEN [b].[TimeOnly] >= '00:00:00.0102003' AND [b].[TimeOnly] < '00:00:00.0102004' THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [End]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task IsBetween_negated(bool async)
    {
        await base.IsBetween_negated(async);

        AssertSql(
            """
SELECT [b].[Id]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[TimeOnly] < '22:00:00' AND [b].[TimeOnly] >= '02:00:00'
""");
    }

    public override async Task IsBetween_nullable(bool async)
    {
        await base.IsBetween_nullable(async);

        AssertSql(
            """
SELECT [n].[Id], CASE
    WHEN [n].[TimeOnly] IS NOT NULL AND ([n].[TimeOnly] >= '22:00:00' OR [n].[TimeOnly] < '02:00:00') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Inside], CASE
    WHEN [n].[TimeOnly] IS NULL OR ([n].[TimeOnly] < '22:00:00' AND [n].[TimeOnly] >= '02:00:00') THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Outside]
FROM [NullableBasicTypesEntities] AS [n]
""");
    }

    public override async Task IsBetween_column_bounds(bool async)
    {
        await base.IsBetween_column_bounds(async);

        AssertSql(
            """
@time='00:00' (DbType = Time)

SELECT [b].[Id], CASE
    WHEN ([b].[TimeOnly] <= '16:00:00' AND @time >= [b].[TimeOnly] AND @time < '16:00:00') OR ([b].[TimeOnly] > '16:00:00' AND (@time >= [b].[TimeOnly] OR @time < '16:00:00')) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Start], CASE
    WHEN ('22:00:00' <= [b].[TimeOnly] AND @time >= '22:00:00' AND @time < [b].[TimeOnly]) OR ('22:00:00' > [b].[TimeOnly] AND (@time >= '22:00:00' OR @time < [b].[TimeOnly])) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [End]
FROM [BasicTypesEntities] AS [b]
""");
    }

    public override async Task Subtract()
    {
        await AssertTranslationFailed(base.Subtract);

        AssertSql();
    }

    public override async Task FromDateTime_compared_to_property()
    {
        await base.FromDateTime_compared_to_property();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS time) = [b].[TimeOnly]
""");
    }

    public override async Task FromDateTime_compared_to_parameter()
    {
        await base.FromDateTime_compared_to_parameter();

        AssertSql(
            """
@time='15:30' (DbType = Time)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS time) = @time
""");
    }

    public override async Task FromDateTime_compared_to_constant()
    {
        await base.FromDateTime_compared_to_constant();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE CAST([b].[DateTime] AS time) = '15:30:10'
""");
    }

    public override async Task FromTimeSpan_compared_to_property()
    {
        await base.FromTimeSpan_compared_to_property();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[TimeSpan] < [b].[TimeOnly]
""");
    }

    public override async Task FromTimeSpan_compared_to_parameter()
    {
        await base.FromTimeSpan_compared_to_parameter();

        AssertSql(
            """
@time='01:02' (DbType = Time)

SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
WHERE [b].[TimeSpan] = @time
""");
    }

    public override async Task Order_by_FromTimeSpan()
    {
        await base.Order_by_FromTimeSpan();

        AssertSql(
            """
SELECT [b].[Id], [b].[Bool], [b].[Byte], [b].[ByteArray], [b].[DateOnly], [b].[DateTime], [b].[DateTimeOffset], [b].[Decimal], [b].[Double], [b].[Enum], [b].[FlagsEnum], [b].[Float], [b].[Guid], [b].[Int], [b].[Long], [b].[Short], [b].[String], [b].[TimeOnly], [b].[TimeSpan]
FROM [BasicTypesEntities] AS [b]
ORDER BY [b].[TimeSpan]
""");
    }

    [ConditionalFact(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.IsFunctions2022Supported))]
    public virtual async Task DateTrunc_hour()
    {
        await AssertQueryScalar(
            actualQuery: ss => ss.Set<BasicTypesEntity>().Select(b => EF.Functions.DateTrunc("hour", b.TimeOnly)),
            expectedQuery: ss => ss.Set<BasicTypesEntity>().Select(b => new TimeOnly(b.TimeOnly.Hour, 0)));

        AssertSql(
            """
SELECT DATETRUNC(hour, [b].[TimeOnly])
FROM [BasicTypesEntities] AS [b]
""");
    }

    [ConditionalFact(typeof(SqlServerTestEnvironment), nameof(SqlServerTestEnvironment.IsFunctions2022Supported))]
    public virtual async Task DateTrunc_minute()
    {
        await AssertQueryScalar(
            actualQuery: ss => ss.Set<BasicTypesEntity>().Select(b => EF.Functions.DateTrunc("minute", b.TimeOnly)),
            expectedQuery: ss => ss.Set<BasicTypesEntity>().Select(b => new TimeOnly(b.TimeOnly.Hour, b.TimeOnly.Minute)));

        AssertSql(
            """
SELECT DATETRUNC(minute, [b].[TimeOnly])
FROM [BasicTypesEntities] AS [b]
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
