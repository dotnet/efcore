// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class TimeOnlyTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual Task Hour()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Hour == 15));

    [Fact]
    public virtual Task Minute()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Minute == 30));

    [Fact]
    public virtual Task Second()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Second == 10));

    [Fact]
    public virtual Task Millisecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Millisecond == 123));

    [Fact]
    public virtual Task Microsecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Microsecond == 456));

    [Fact]
    public virtual Task Nanosecond()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(e => e.TimeOnly.Nanosecond == 400));

    [Fact]
    public virtual Task AddHours()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddHours(3) == new TimeOnly(18, 30, 10)));

    [Fact]
    public virtual Task AddMinutes()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.AddMinutes(3) == new TimeOnly(15, 33, 10)));

    [Fact]
    public virtual Task Add_TimeSpan()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.Add(new TimeSpan(3, 0, 0)) == new TimeOnly(18, 30, 10)));

    [Fact]
    public virtual Task IsBetween()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly.IsBetween(new TimeOnly(14, 0, 0), new TimeOnly(16, 0, 0))));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task IsBetween_midnight(bool async)
        => AssertQuery(async, ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.TimeOnly.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0))).Select(b => b.Id));

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    [InlineData(false, 3)]
    [InlineData(true, 3)]
    public virtual Task IsBetween_parameters(bool async, int range)
    {
        var start = range switch { 0 => new TimeOnly(14, 0), 3 => new TimeOnly(102003L), _ => new TimeOnly(22, 0) };
        var end = range switch { 0 => new TimeOnly(16, 0), 1 => new TimeOnly(2, 0), 3 => new TimeOnly(102005L), _ => start };

        return AssertQuery(async, ss => ss.Set<BasicTypesEntity>()
            .Where(b => b.TimeOnly.IsBetween(start, end)).Select(b => b.Id), assertEmpty: range == 2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task IsBetween_boundaries(bool async)
        => AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Start = b.TimeOnly.IsBetween(new TimeOnly(15, 30, 10), new TimeOnly(16, 0)),
            End = b.TimeOnly.IsBetween(new TimeOnly(14, 0), new TimeOnly(15, 30, 10)),
            WrappedStart = b.TimeOnly.IsBetween(new TimeOnly(15, 30, 10), new TimeOnly(14, 0)),
            WrappedEnd = b.TimeOnly.IsBetween(new TimeOnly(16, 0), new TimeOnly(15, 30, 10)),
            Empty = b.TimeOnly.IsBetween(new TimeOnly(15, 30, 10), new TimeOnly(15, 30, 10))
        }), elementSorter: x => x.Id);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task IsBetween_precision(bool async)
        => AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Start = b.TimeOnly.IsBetween(new TimeOnly(102004L), new TimeOnly(102005L)),
            End = b.TimeOnly.IsBetween(new TimeOnly(102003L), new TimeOnly(102004L))
        }), elementSorter: x => x.Id);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task IsBetween_negated(bool async)
        => AssertQuery(async, ss => ss.Set<BasicTypesEntity>()
            .Where(b => !b.TimeOnly.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0))).Select(b => b.Id));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task IsBetween_nullable(bool async)
        => AssertQuery(async, ss => ss.Set<NullableBasicTypesEntity>().Select(b => new
        {
            b.Id,
            Inside = b.TimeOnly.HasValue && b.TimeOnly.Value.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0)),
            Outside = !b.TimeOnly.HasValue || !b.TimeOnly.Value.IsBetween(new TimeOnly(22, 0), new TimeOnly(2, 0))
        }), elementSorter: x => x.Id);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task IsBetween_column_bounds(bool async)
    {
        var time = new TimeOnly(0, 0);

        return AssertQuery(async, ss => ss.Set<BasicTypesEntity>().Select(b => new
        {
            b.Id,
            Start = time.IsBetween(b.TimeOnly, new TimeOnly(16, 0)),
            End = time.IsBetween(new TimeOnly(22, 0), b.TimeOnly)
        }), elementSorter: x => x.Id);
    }

    [Fact]
    public virtual Task Subtract()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeOnly - new TimeOnly(3, 0, 0) == new TimeSpan(12, 30, 10)));

    [Fact]
    public virtual Task FromDateTime_compared_to_property()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == b.TimeOnly));

    [Fact]
    public virtual Task FromDateTime_compared_to_parameter()
    {
        var time = new TimeOnly(15, 30, 10);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == time));
    }

    [Fact]
    public virtual Task FromDateTime_compared_to_constant()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromDateTime(b.DateTime) == new TimeOnly(15, 30, 10)));

    [Fact]
    public virtual Task FromTimeSpan_compared_to_property()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => TimeOnly.FromTimeSpan(b.TimeSpan) < b.TimeOnly));

    [Fact]
    public virtual Task FromTimeSpan_compared_to_parameter()
    {
        var time = new TimeOnly(1, 2, 3);

        return AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => TimeOnly.FromTimeSpan(x.TimeSpan) == time));
    }

    [Fact]
    public virtual Task Order_by_FromTimeSpan()
        => AssertQuery(
            ss => ss.Set<BasicTypesEntity>().OrderBy(x => TimeOnly.FromTimeSpan(x.TimeSpan)),
            assertOrder: true);
}
