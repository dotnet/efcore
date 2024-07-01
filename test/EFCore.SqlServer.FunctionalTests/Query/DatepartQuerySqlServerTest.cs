// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class DatepartQuerySqlServerTest : DatepartQueryRelationalTestBase<DatepartQuerySqlServerFixture>
{
    public DatepartQuerySqlServerTest(
        DatepartQuerySqlServerFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();


    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    #region DATEPART DateTime

    public override async Task Select_datetime_microsecond_component(bool async)
    {
        await base.Select_datetime_microsecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(microsecond, [e].[StartDate])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_datetime_microsecond_component(bool async)
    {
        await base.Select_by_datetime_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(microsecond, [e].[StartDate]) = 111111
            """);
    }

    public override async Task OrderBy_datetime_microsecond_component(bool async)
    {
        await base.OrderBy_datetime_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[StartDate])
            """);
    }

    public override async Task OrderByDescending_datetime_microsecond_component(bool async)
    {
        await base.OrderByDescending_datetime_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[StartDate]) DESC
            """);
    }

    public override async Task Select_datetime_nanosecond_component(bool async)
    {
        await base.Select_datetime_nanosecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(nanosecond, [e].[StartDate])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_datetime_nanosecond_component(bool async)
    {
        await base.Select_by_datetime_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(nanosecond, [e].[StartDate]) = 111111100
            """);
    }

    public override async Task OrderBy_datetime_nanosecond_component(bool async)
    {
        await base.OrderBy_datetime_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[StartDate])
            """);
    }

    public override async Task OrderByDescending_datetime_nanosecond_component(bool async)
    {
        await base.OrderByDescending_datetime_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[StartDate]) DESC
            """);
    }

    #endregion

    #region DATEPART DateTimeOffset

    public override async Task Select_datetimeoffset_microsecond_component(bool async)
    {
        await base.Select_datetimeoffset_microsecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(microsecond, [e].[EndDate])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_datetimeoffset_microsecond_component(bool async)
    {
        await base.Select_by_datetimeoffset_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(microsecond, [e].[EndDate]) = 111111
            """);
    }

    public override async Task OrderBy_datetimeoffset_microsecond_component(bool async)
    {
        await base.OrderBy_datetimeoffset_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[EndDate])
            """);
    }

    public override async Task OrderByDescending_datetimeoffset_microsecond_component(bool async)
    {
        await base.OrderByDescending_datetimeoffset_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[EndDate]) DESC
            """);
    }

    public override async Task Select_datetimeoffset_nanosecond_component(bool async)
    {
        await base.Select_datetimeoffset_nanosecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(nanosecond, [e].[EndDate])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_datetimeoffset_nanosecond_component(bool async)
    {
        await base.Select_by_datetimeoffset_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(nanosecond, [e].[EndDate]) = 111111100
            """);
    }

    public override async Task OrderBy_datetimeoffset_nanosecond_component(bool async)
    {
        await base.OrderBy_datetimeoffset_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[EndDate])
            """);
    }

    public override async Task OrderByDescending_datetimeoffset_nanosecond_component(bool async)
    {
        await base.OrderByDescending_datetimeoffset_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[EndDate]) DESC
            """);
    }

    #endregion

    #region DATEPART TimeOnly

    public override async Task Select_timeonly_microsecond_component(bool async)
    {
        await base.Select_timeonly_microsecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(microsecond, [e].[StartTime])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_timeonly_microsecond_component(bool async)
    {
        await base.Select_by_timeonly_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(microsecond, [e].[StartTime]) = 111111
            """);
    }

    public override async Task OrderBy_timeonly_microsecond_component(bool async)
    {
        await base.OrderBy_timeonly_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[StartTime])
            """);
    }

    public override async Task OrderByDescending_timeonly_microsecond_component(bool async)
    {
        await base.OrderByDescending_timeonly_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[StartTime]) DESC
            """);
    }

    public override async Task Select_timeonly_nanosecond_component(bool async)
    {
        await base.Select_timeonly_nanosecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(nanosecond, [e].[StartTime])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_timeonly_nanosecond_component(bool async)
    {
        await base.Select_by_timeonly_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(nanosecond, [e].[StartTime]) = 111111100
            """);
    }

    public override async Task OrderBy_timeonly_nanosecond_component(bool async)
    {
        await base.OrderBy_timeonly_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[StartTime])
            """);
    }

    public override async Task OrderByDescending_timeonly_nanosecond_component(bool async)
    {
        await base.OrderByDescending_timeonly_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[StartTime]) DESC
            """);
    }

    #endregion

    #region DATEPART TimeSpan

    public override async Task Select_timespan_microsecond_component(bool async)
    {
        await base.Select_timespan_microsecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(microsecond, [e].[Duration])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_timespan_microsecond_component(bool async)
    {
        await base.Select_by_timespan_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(microsecond, [e].[Duration]) = 111111
            """);
    }

    public override async Task OrderBy_timespan_microsecond_component(bool async)
    {
        await base.OrderBy_timespan_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[Duration])
            """);
    }

    public override async Task OrderByDescending_timespan_microsecond_component(bool async)
    {
        await base.OrderByDescending_timespan_microsecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(microsecond, [e].[Duration]) DESC
            """);
    }

    public override async Task Select_timespan_nanosecond_component(bool async)
    {
        await base.Select_timespan_nanosecond_component(async);
        AssertSql(
            """
            SELECT DATEPART(nanosecond, [e].[Duration])
            FROM [Expeditions] AS [e]
            """);
    }

    public override async Task Select_by_timespan_nanosecond_component(bool async)
    {
        await base.Select_by_timespan_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            WHERE DATEPART(nanosecond, [e].[Duration]) = 111111100
            """);
    }

    public override async Task OrderBy_timespan_nanosecond_component(bool async)
    {
        await base.OrderBy_timespan_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[Duration])
            """);
    }

    public override async Task OrderByDescending_timespan_nanosecond_component(bool async)
    {
        await base.OrderByDescending_timespan_nanosecond_component(async);
        AssertSql(
            """
            SELECT [e].[Id]
            FROM [Expeditions] AS [e]
            ORDER BY DATEPART(nanosecond, [e].[Duration]) DESC
            """);
    }

    #endregion
}
