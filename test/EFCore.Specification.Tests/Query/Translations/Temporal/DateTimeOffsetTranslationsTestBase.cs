// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateTimeOffsetTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Now()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.Now));

    [ConditionalFact]
    public virtual Task UtcNow()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.UtcNow));

    [ConditionalFact]
    public virtual Task Date()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Date > new DateTimeOffset().Date));

    [ConditionalFact]
    public virtual Task Year()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Year == 1998));

    [ConditionalFact]
    public virtual Task Month()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Month == 5));

    [ConditionalFact]
    public virtual Task DayOfYear()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.DayOfYear == 124));

    [ConditionalFact]
    public virtual Task Day()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Day == 4));

    [ConditionalFact]
    public virtual Task Hour()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Hour == 15));

    [ConditionalFact]
    public virtual Task Minute()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Minute == 30));

    [ConditionalFact]
    public virtual Task Second()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Second == 10));

    [ConditionalFact]
    public virtual Task Millisecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Millisecond == 123));

    [ConditionalFact]
    public virtual Task Microsecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Microsecond == 456));

    [ConditionalFact]
    public virtual Task Nanosecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Nanosecond == 400));

    [ConditionalFact]
    public virtual Task TimeOfDay()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.TimeOfDay));

    [ConditionalFact]
    public virtual Task AddYears()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddYears(1)));

    [ConditionalFact]
    public virtual Task AddMonths()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMonths(1)));

    [ConditionalFact]
    public virtual Task AddDays()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddDays(1)));

    [ConditionalFact]
    public virtual Task AddHours()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddHours(1)));

    [ConditionalFact]
    public virtual Task AddMinutes()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMinutes(1)));

    [ConditionalFact]
    public virtual Task AddSeconds()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddSeconds(1)));

    [ConditionalFact]
    public virtual Task AddMilliseconds()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMilliseconds(300)));

    [ConditionalFact]
    public virtual Task ToUnixTimeMilliseconds()
    {
        var unixEpochMilliseconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeMilliseconds();

        return AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.DateTimeOffset.ToUnixTimeMilliseconds() == unixEpochMilliseconds));
    }

    [ConditionalFact]
    public virtual Task ToUnixTimeSecond()
    {
        var unixEpochSeconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeSeconds();

        return AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.DateTimeOffset.ToUnixTimeSeconds() == unixEpochSeconds));
    }

    [ConditionalFact]
    public virtual Task Milliseconds_parameter_and_constant()
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
            ss => ss.Set<BasicTypesEntity>().Where(dynamicWhere),
            ss => ss.Set<BasicTypesEntity>().Where(m => m.DateTimeOffset == dateTimeOffset));
    }
}
