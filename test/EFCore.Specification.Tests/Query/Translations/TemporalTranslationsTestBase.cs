// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class TemporalTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    #region DateTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Now(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Now != myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_UtcNow(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.UtcNow != myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Today(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTime == DateTime.Today),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Date(bool async)
    {
        var myDatetime = new DateTime(1998, 5, 4);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Date == myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_AddYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.AddYears(1).Year == 1999));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Year == 1998));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Month == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.DayOfYear == 124));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Day == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Hour == 15));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Minute == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Second == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Millisecond == 123));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_TimeOfDay(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.TimeOfDay == TimeSpan.Zero));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTime_subtract_and_TotalDays(bool async)
    {
        var date = new DateTime(1997, 1, 1);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => (o.DateTime - date).TotalDays > 365));
    }

    #endregion DateTime

    #region DateOnly

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Year == 1990));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Month == 11));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Day == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayOfYear == 314));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_DayOfWeek(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayOfWeek == DayOfWeek.Saturday));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_AddYears(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddYears(3) == new DateOnly(1993, 11, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_AddMonths(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddMonths(3) == new DateOnly(1991, 2, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_AddDays(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddDays(3) == new DateOnly(1990, 11, 13)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_FromDateTime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => DateOnly.FromDateTime(o.DateTime) == new DateOnly(1998, 5, 4)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_FromDateTime_compared_to_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => DateOnly.FromDateTime(o.DateTime) == o.DateOnly));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateOnly_FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        var dateOnly = new DateOnly(2, 10, 11);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(x => new[] { dateOnly, new DateOnly(1998, 5, 4) }.Contains(DateOnly.FromDateTime(x.DateTime))));
    }

    #endregion DateOnly

    #region TimeOnly

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Hour == 15));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Minute == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Second == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Millisecond == 123));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Microsecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Microsecond == 456));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Nanosecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Nanosecond == 400));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_AddHours(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddHours(3) == new TimeOnly(18, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_AddMinutes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddMinutes(3) == new TimeOnly(15, 33, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_Add_TimeSpan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Add(new TimeSpan(3, 0, 0)) == new TimeOnly(18, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_IsBetween(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(new TimeOnly(14, 0, 0), new TimeOnly(16, 0, 0))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_subtract_TimeOnly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly - new TimeOnly(3, 0, 0) == new TimeSpan(12, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_FromDateTime_compared_to_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == b.TimeOnly));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_FromDateTime_compared_to_parameter(bool async)
    {
        var time = new TimeOnly(15, 30, 10);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == time));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_FromDateTime_compared_to_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == new TimeOnly(15, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_FromTimeSpan_compared_to_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromTimeSpan(b.TimeSpan) < b.TimeOnly));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOnly_FromTimeSpan_compared_to_parameter(bool async)
    {
        var time = new TimeOnly(1, 2, 3);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(x => TimeOnly.FromTimeSpan(x.TimeSpan) == time));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_TimeOnly_FromTimeSpan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().OrderBy(x => TimeOnly.FromTimeSpan(x.TimeSpan)),
            assertOrder: true);

    #endregion TimeOnly

    #region DateTimeOffset

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Now(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.Now));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_UtcNow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.UtcNow));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Date(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Date > new DateTimeOffset().Date));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Year == 1998));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Month == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.DayOfYear == 124));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Day == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Hour == 15));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Minute == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Second == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Millisecond == 123));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Microsecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Microsecond == 456));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_Nanosecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Nanosecond == 400));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_TimeOfDay(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.TimeOfDay));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddYears(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddYears(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddMonths(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMonths(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddDays(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddDays(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddHours(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddHours(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddMinutes(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMinutes(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddSeconds(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddSeconds(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_AddMilliseconds(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMilliseconds(300)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_ToUnixTimeMilliseconds(bool async)
    {
        var unixEpochMilliseconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeMilliseconds();

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => b.DateTimeOffset.ToUnixTimeMilliseconds() == unixEpochMilliseconds));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_ToUnixTimeSecond(bool async)
    {
        var unixEpochSeconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeSeconds();

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => b.DateTimeOffset.ToUnixTimeSeconds() == unixEpochSeconds));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DateTimeOffset_milliseconds_parameter_and_constant(bool async)
    {
        var dateTimeOffset = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0));

        // Literal where clause
        var p = Expression.Parameter(typeof(BasicTypesEntity), "i");
        var dynamicWhere = Expression.Lambda<Func<BasicTypesEntity, bool>>(
            Expression.Equal(
                Expression.Property(p, "DateTimeOffset"),
                Expression.Constant(dateTimeOffset)
            ), p);

        return AssertCount(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(dynamicWhere),
            ss => ss.Set<BasicTypesEntity>().Where(m => m.DateTimeOffset == dateTimeOffset));
    }

    #endregion DateTimeOffset

    #region TimeSpan

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Hours(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Hours == 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Minutes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Minutes == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Seconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Seconds == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Milliseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Milliseconds == 678));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Microseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Microseconds == 912));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeSpan_Nanoseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Nanoseconds == 400));

    #endregion TimeSpan
}
