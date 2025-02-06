// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateTimeTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Now(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Now != myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task UtcNow(bool async)
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.UtcNow != myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Today(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTime == DateTime.Today),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Date(bool async)
    {
        var myDatetime = new DateTime(1998, 5, 4);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Date == myDatetime));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AddYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.AddYears(1).Year == 1999));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Year(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Year == 1998));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Month(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Month == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DayOfYear(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.DayOfYear == 124));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Day(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Day == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hour(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Hour == 15));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Minute(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Minute == 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Second(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Second == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Millisecond(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Millisecond == 123));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task TimeOfDay(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.TimeOfDay == TimeSpan.Zero));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task subtract_and_TotalDays(bool async)
    {
        var date = new DateTime(1997, 1, 1);

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => (o.DateTime - date).TotalDays > 365));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parse_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == DateTime.Parse("5/4/1998 15:30:10 PM")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parse_with_parameter(bool async)
    {
        var date = "5/4/1998 15:30:10 PM";

        return AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == DateTime.Parse(date)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task New_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(1998, 5, 4, 15, 30, 10)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task New_with_parameters(bool async)
    {
        var year = 1998;
        var month = 5;
        var date = 4;
        var hour = 15;

        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(year, month, date, hour, 30, 10)));
    }
}
