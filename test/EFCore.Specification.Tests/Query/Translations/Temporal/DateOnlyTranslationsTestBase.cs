// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateOnlyTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Year == 1990));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Month == 11));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.Day == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayOfYear == 314));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DayOfWeek(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayOfWeek == System.DayOfWeek.Saturday));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DayNumber(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayNumber == 726780));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddYears(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddYears(3) == new DateOnly(1993, 11, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddMonths(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddMonths(3) == new DateOnly(1991, 2, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddDays(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.AddDays(3) == new DateOnly(1990, 11, 13)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DayNumber_subtraction(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.DateOnly.DayNumber - new DateOnly(1990, 11, 5).DayNumber == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromDateTime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => DateOnly.FromDateTime(o.DateTime) == new DateOnly(1998, 5, 4)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromDateTime_compared_to_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => DateOnly.FromDateTime(o.DateTime) == o.DateOnly));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromDateTime_compared_to_constant_and_parameter(bool async)
    {
        var dateOnly = new DateOnly(2, 10, 11);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(x => new[] { dateOnly, new DateOnly(1998, 5, 4) }.Contains(DateOnly.FromDateTime(x.DateTime))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToDateTime_property_with_constant_TimeOnly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(o => o.DateOnly.ToDateTime(new TimeOnly(21, 5, 19, 940, 500)) == new DateTime(2020, 1, 1, 21, 5, 19, 940, 500)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToDateTime_property_with_property_TimeOnly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(o => o.DateOnly.ToDateTime(o.TimeOnly) == new DateTime(2020, 1, 1, 15, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToDateTime_constant_DateTime_with_property_TimeOnly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(o => new DateOnly(1990, 11, 10).ToDateTime(o.TimeOnly) == new DateTime(1990, 11, 10, 15, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToDateTime_with_complex_DateTime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(o => o.DateOnly.AddYears(1).ToDateTime(o.TimeOnly) == new DateTime(2021, 1, 1, 15, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToDateTime_with_complex_TimeOnly(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>()
                .Where(o => o.DateOnly.ToDateTime(o.TimeOnly.AddHours(1)) == new DateTime(2020, 1, 1, 16, 30, 10))
                .AsTracking());
}
