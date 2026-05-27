// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateTimeOffsetTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual Task Now()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.Now));

    [Fact]
    public virtual Task UtcNow()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset != DateTimeOffset.UtcNow));

    [Fact]
    public virtual Task Date()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Date > new DateTimeOffset().Date));

    [Fact]
    public virtual Task Year()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Year == 1998));

    [Fact]
    public virtual Task Month()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Month == 5));

    [Fact]
    public virtual Task DayOfYear()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.DayOfYear == 124));

    [Fact]
    public virtual Task Day()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Day == 4));

    [Fact]
    public virtual Task Hour()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Hour == 15));

    [Fact]
    public virtual Task Minute()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Minute == 30));

    [Fact]
    public virtual Task Second()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Second == 10));

    [Fact]
    public virtual Task Millisecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.Millisecond == 123));

    [Fact]
    public virtual Task Microsecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Microsecond == 456));

    [Fact]
    public virtual Task Nanosecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTimeOffset.Nanosecond == 400));

    [Fact]
    public virtual Task TimeOfDay()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.TimeOfDay));

    [Fact]
    public virtual Task DateTime()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.DateTime == new DateTime(1998, 5, 4, 15, 30, 10)));

    [Fact]
    public virtual Task UtcDateTime()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.UtcDateTime == new DateTime(1998, 5, 4, 15, 30, 10)));

    [Fact]
    public virtual Task LocalDateTime()
        // Note: DateTimeOffset.LocalDateTime depends on the machine's local time zone, and the client and server may be in different
        // time zones. Use a comparison far from any timezone boundary so the same rows match regardless of timezone.
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateTimeOffset.LocalDateTime > new DateTime(1999, 1, 1)));

    [Fact]
    public virtual Task AddYears()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddYears(1)));

    [Fact]
    public virtual Task AddMonths()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMonths(1)));

    [Fact]
    public virtual Task AddDays()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddDays(1)));

    [Fact]
    public virtual Task AddHours()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddHours(1)));

    [Fact]
    public virtual Task AddMinutes()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMinutes(1)));

    [Fact]
    public virtual Task AddSeconds()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddSeconds(1)));

    [Fact]
    public virtual Task AddMilliseconds()
        => AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.DateTimeOffset.AddMilliseconds(300)));

    [Fact]
    public virtual Task ToUnixTimeMilliseconds()
    {
        var unixEpochMilliseconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeMilliseconds();

        return AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.DateTimeOffset.ToUnixTimeMilliseconds() == unixEpochMilliseconds));
    }

    [Fact]
    public virtual Task ToUnixTimeSecond()
    {
        var unixEpochSeconds = new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero).ToUnixTimeSeconds();

        return AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.DateTimeOffset.ToUnixTimeSeconds() == unixEpochSeconds));
    }

    [Fact]
    public virtual Task ToOffset()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => b.DateTimeOffset.ToOffset(new TimeSpan(2, 0, 0)) == new DateTimeOffset(1998, 5, 4, 17, 30, 10, new TimeSpan(2, 0, 0))));

    // new DateTimeOffset(DateTime) with Unspecified kind: databases don't have DateTimeKind, so this is always treated
    // as UTC (+00:00). The expected query explicitly uses TimeSpan.Zero to match.
    [Fact]
    public virtual Task Ctor_DateTime()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => new DateTimeOffset(b.DateTime) == new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero)),
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => new DateTimeOffset(b.DateTime, TimeSpan.Zero) == new DateTimeOffset(1998, 5, 4, 15, 30, 10, TimeSpan.Zero)));

    [Fact]
    public virtual Task Ctor_DateTime_TimeSpan()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>()
                .Where(b => b.DateTime.Year > 1)
                .Where(b => new DateTimeOffset(b.DateTime, new TimeSpan(2, 0, 0)) == new DateTimeOffset(1998, 5, 4, 15, 30, 10, new TimeSpan(2, 0, 0))));

    [Fact]
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
