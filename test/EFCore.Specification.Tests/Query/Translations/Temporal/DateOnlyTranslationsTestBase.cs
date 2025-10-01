// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateOnlyTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Year()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Year == 1990));

    [ConditionalFact]
    public virtual Task Month()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Month == 11));

    [ConditionalFact]
    public virtual Task Day()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Day == 10));

    [ConditionalFact]
    public virtual Task DayOfYear()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayOfYear == 314));

    [ConditionalFact]
    public virtual Task DayOfWeek()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayOfWeek == System.DayOfWeek.Saturday));

    [ConditionalFact]
    public virtual Task DayNumber()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayNumber == 726780));

    [ConditionalFact]
    public virtual Task AddYears()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddYears(3) == new DateOnly(1993, 11, 10)));

    [ConditionalFact]
    public virtual Task AddMonths()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddMonths(3) == new DateOnly(1991, 2, 10)));

    [ConditionalFact]
    public virtual Task AddDays()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddDays(3) == new DateOnly(1990, 11, 13)));

    [ConditionalFact]
    public virtual Task DayNumber_subtraction()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayNumber - new DateOnly(1990, 11, 5).DayNumber == 5));

    [ConditionalFact]
    public virtual Task FromDateTime()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => DateOnly.FromDateTime(o.DateTime) == new DateOnly(1998, 5, 4)));

    [ConditionalFact]
    public virtual Task FromDateTime_compared_to_property()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => DateOnly.FromDateTime(o.DateTime) == o.DateOnly));

    [ConditionalFact]
    public virtual Task FromDateTime_compared_to_constant_and_parameter()
    {
        var dateOnly = new DateOnly(2, 10, 11);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(x => new[] { dateOnly, new DateOnly(1998, 5, 4) }.Contains(DateOnly.FromDateTime(x.DateTime))));
    }

    [ConditionalFact]
    public virtual Task ToDateTime_property_with_constant_TimeOnly()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(o => o.DateOnly.ToDateTime(new TimeOnly(21, 5, 19, 940, 500)) == new DateTime(2020, 1, 1, 21, 5, 19, 940, 500)));

    [ConditionalFact]
    public virtual Task ToDateTime_property_with_property_TimeOnly()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(o => o.DateOnly.ToDateTime(o.TimeOnly) == new DateTime(2020, 1, 1, 15, 30, 10)));

    [ConditionalFact]
    public virtual Task ToDateTime_constant_DateTime_with_property_TimeOnly()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(o => new DateOnly(1990, 11, 10).ToDateTime(o.TimeOnly) == new DateTime(1990, 11, 10, 15, 30, 10)));

    [ConditionalFact]
    public virtual Task ToDateTime_with_complex_DateTime()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(o => o.DateOnly.AddYears(1).ToDateTime(o.TimeOnly) == new DateTime(2021, 1, 1, 15, 30, 10)));

    [ConditionalFact]
    public virtual Task ToDateTime_with_complex_TimeOnly()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>()
            .Where(o => o.DateOnly.ToDateTime(o.TimeOnly.AddHours(1)) == new DateTime(2020, 1, 1, 16, 30, 10))
            .AsTracking());
}
