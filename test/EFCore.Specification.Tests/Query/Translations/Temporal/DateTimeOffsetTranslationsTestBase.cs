// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateTimeOffsetTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Now(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.Now));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task UtcNow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.UtcNow));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Date(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Date > new DateTimeOffset().Date));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Year == 1998));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Month == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.DayOfYear == 124));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Day == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Hour == 15));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Minute == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Second == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Millisecond == 123));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Microsecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Microsecond == 456));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nanosecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Nanosecond == 400));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOfDay(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.TimeOfDay));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddYears(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddYears(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddMonths(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMonths(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddDays(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddDays(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddHours(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddHours(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddMinutes(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMinutes(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddSeconds(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddSeconds(1)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddMilliseconds(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMilliseconds(300)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToUnixTimeMilliseconds(bool async)
    {
        var unixEpochMilliseconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeMilliseconds();

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => b.DateTimeOffset.ToUnixTimeMilliseconds() == unixEpochMilliseconds));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToUnixTimeSecond(bool async)
    {
        var unixEpochSeconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeSeconds();

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => b.DateTimeOffset.ToUnixTimeSeconds() == unixEpochSeconds));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Milliseconds_parameter_and_constant(bool async)
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
}
