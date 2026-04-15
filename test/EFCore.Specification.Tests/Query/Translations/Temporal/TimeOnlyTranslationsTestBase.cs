// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class TimeOnlyTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Hour()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Hour == 15));

    [ConditionalFact]
    public virtual Task Minute()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Minute == 30));

    [ConditionalFact]
    public virtual Task Second()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Second == 10));

    [ConditionalFact]
    public virtual Task Millisecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Millisecond == 123));

    [ConditionalFact]
    public virtual Task Microsecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Microsecond == 456));

    [ConditionalFact]
    public virtual Task Nanosecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Nanosecond == 400));

    [ConditionalFact]
    public virtual Task AddHours()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddHours(3) == new TimeOnly(18, 30, 10)));

    [ConditionalFact]
    public virtual Task AddMinutes()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddMinutes(3) == new TimeOnly(15, 33, 10)));

    [ConditionalFact]
    public virtual Task Add_TimeSpan()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Add(new TimeSpan(3, 0, 0)) == new TimeOnly(18, 30, 10)));

    [ConditionalFact]
    public virtual Task IsBetween()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(new TimeOnly(14, 0, 0), new TimeOnly(16, 0, 0))));

    [ConditionalFact]
    public virtual Task Subtract()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly - new TimeOnly(3, 0, 0) == new TimeSpan(12, 30, 10)));

    [ConditionalFact]
    public virtual Task FromDateTime_compared_to_property()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == b.TimeOnly));

    [ConditionalFact]
    public virtual Task FromDateTime_compared_to_parameter()
    {
        var time = new TimeOnly(15, 30, 10);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == time));
    }

    [ConditionalFact]
    public virtual Task FromDateTime_compared_to_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == new TimeOnly(15, 30, 10)));

    [ConditionalFact]
    public virtual Task FromTimeSpan_compared_to_property()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromTimeSpan(b.TimeSpan) < b.TimeOnly));

    [ConditionalFact]
    public virtual Task FromTimeSpan_compared_to_parameter()
    {
        var time = new TimeOnly(1, 2, 3);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => TimeOnly.FromTimeSpan(x.TimeSpan) == time));
    }

    [ConditionalFact]
    public virtual Task Order_by_FromTimeSpan()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(x => TimeOnly.FromTimeSpan(x.TimeSpan)),
            assertOrder: true);
}
