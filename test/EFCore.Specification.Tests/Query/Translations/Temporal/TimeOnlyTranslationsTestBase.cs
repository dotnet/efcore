// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class TimeOnlyTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Hour == 15));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Minute == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Second == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Millisecond == 123));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Microsecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Microsecond == 456));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nanosecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Nanosecond == 400));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddHours(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddHours(3) == new TimeOnly(18, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddMinutes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddMinutes(3) == new TimeOnly(15, 33, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_TimeSpan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Add(new TimeSpan(3, 0, 0)) == new TimeOnly(18, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsBetween(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(new TimeOnly(14, 0, 0), new TimeOnly(16, 0, 0))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Subtract(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly - new TimeOnly(3, 0, 0) == new TimeSpan(12, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromDateTime_compared_to_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == b.TimeOnly));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromDateTime_compared_to_parameter(bool async)
    {
        var time = new TimeOnly(15, 30, 10);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == time));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromDateTime_compared_to_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == new TimeOnly(15, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromTimeSpan_compared_to_property(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromTimeSpan(b.TimeSpan) < b.TimeOnly));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FromTimeSpan_compared_to_parameter(bool async)
    {
        var time = new TimeOnly(1, 2, 3);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(x => TimeOnly.FromTimeSpan(x.TimeSpan) == time));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Order_by_FromTimeSpan(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().OrderBy(x => TimeOnly.FromTimeSpan(x.TimeSpan)),
            assertOrder: true);
}
