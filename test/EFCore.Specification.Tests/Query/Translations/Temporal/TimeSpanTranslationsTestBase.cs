// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class TimeSpanTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual Task Hours()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Hours == 3));

    [Fact]
    public virtual Task Minutes()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Minutes == 4));

    [Fact]
    public virtual Task Seconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Seconds == 5));

    [Fact]
    public virtual Task Milliseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Milliseconds == 678));

    [Fact]
    public virtual Task Microseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Microseconds == 912));

    [Fact]
    public virtual Task Nanoseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Nanoseconds == 400));

    [Fact]
    public virtual Task TotalDays()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalDays > 0.1));

    [Fact]
    public virtual Task TotalHours()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalHours > 2.5));

    [Fact]
    public virtual Task TotalMinutes()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalMinutes > 120.25));

    [Fact]
    public virtual Task TotalSeconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalSeconds > 7215.4));

    [Fact]
    public virtual Task TotalMilliseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalMilliseconds > 7215456.2));

    [Fact]
    public virtual Task TotalMicroseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalMicroseconds > 7215456200.3));

    [Fact]
    public virtual Task TotalNanoseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.TotalNanoseconds > 7215456200300));
}
