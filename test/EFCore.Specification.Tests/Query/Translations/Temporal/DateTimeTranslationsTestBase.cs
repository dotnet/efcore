// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class DateTimeTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual Task Now()
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.Now != myDatetime));
    }

    [Fact]
    public virtual Task UtcNow()
    {
        var myDatetime = new DateTime(2015, 4, 10);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(c => DateTime.UtcNow != myDatetime));
    }

    [Fact]
    public virtual Task Today()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().Where(e => e.DateTime == DateTime.Today),
            assertEmpty: true);

    [Fact]
    public virtual Task Date()
    {
        var myDatetime = new DateTime(1998, 5, 4);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Date == myDatetime));
    }

    [Fact]
    public virtual Task AddYear()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.AddYears(1).Year == 1999));

    [Fact]
    public virtual Task Year()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Year == 1998));

    [Fact]
    public virtual Task Month()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Month == 5));

    [Fact]
    public virtual Task DayOfYear()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.DayOfYear == 124));

    [Fact]
    public virtual Task Day()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Day == 4));

    [Fact]
    public virtual Task Hour()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Hour == 15));

    [Fact]
    public virtual Task Minute()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Minute == 30));

    [Fact]
    public virtual Task Second()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Second == 10));

    [Fact]
    public virtual Task Millisecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.Millisecond == 123));

    [Fact]
    public virtual Task TimeOfDay()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime.TimeOfDay == TimeSpan.Zero));

    [Fact]
    public virtual Task subtract_and_TotalDays()
    {
        var date = new DateTime(1997, 1, 1);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => (o.DateTime - date).TotalDays > 365));
    }

    [Fact]
    public virtual Task Parse_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == DateTime.Parse("1998-05-04T15:30:10")));

    [Fact]
    public virtual Task Parse_with_parameter()
    {
        var date = "1998-05-04T15:30:10";

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == DateTime.Parse(date)));
    }

    [Fact]
    public virtual Task New_with_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(1998, 5, 4, 15, 30, 10)));

    [Fact]
    public virtual async Task New_with_parameters()
    {
        var year = 1998;
        var month = 5;
        var date = 4;
        var hour = 15;

        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(o => o.DateTime == new DateTime(year, month, date, hour, 30, 10)));
    }
}
