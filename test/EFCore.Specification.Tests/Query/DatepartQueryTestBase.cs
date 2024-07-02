// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.DatepartModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class DatepartQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : DatepartQueryFixtureBase, new()
{
    protected DatepartQueryTestBase(TFixture fixture) : base(fixture)
    { }


    protected ExpeditionContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    { }

    #region DATEPART DateTime

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartDate.Microsecond),
            ss => ss.Set<Expedition>().Select(e => e.StartDate.Millisecond * 1000 + e.StartDate.Microsecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_datetime_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.StartDate.Microsecond == 111111).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.StartDate.Millisecond * 1000 + e.StartDate.Microsecond == 111111)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_datetime_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.StartDate.Microsecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.StartDate.Millisecond * 1000 + e.StartDate.Microsecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_datetime_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.StartDate.Microsecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.StartDate.Millisecond * 1000 + e.StartDate.Microsecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetime_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartDate.Nanosecond),
            ss => ss.Set<Expedition>().Select(e => e.StartDate.Millisecond * 1000000 + e.StartDate.Microsecond * 1000 + e.StartDate.Nanosecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_datetime_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.StartDate.Nanosecond == 111111100).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.StartDate.Millisecond * 1000000 + e.StartDate.Microsecond * 1000 + e.StartDate.Nanosecond == 111111100)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_datetime_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.StartDate.Nanosecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.StartDate.Millisecond * 1000000 + e.StartDate.Microsecond * 1000 + e.StartDate.Nanosecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_datetime_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.StartDate.Nanosecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.StartDate.Millisecond * 1000000 + e.StartDate.Microsecond * 1000 + e.StartDate.Nanosecond)
                .Select(e => e.Id)
            );

    #endregion

    #region DATEPART DateTimeOffset

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetimeoffset_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.EndDate.Microsecond),
            ss => ss.Set<Expedition>().Select(e => e.EndDate.Millisecond * 1000 + e.EndDate.Microsecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_datetimeoffset_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.EndDate.Microsecond == 111111).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.EndDate.Millisecond * 1000 + e.EndDate.Microsecond == 111111)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_datetimeoffset_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.EndDate.Microsecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.EndDate.Millisecond * 1000 + e.EndDate.Microsecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_datetimeoffset_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.EndDate.Microsecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.EndDate.Millisecond * 1000 + e.EndDate.Microsecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_datetimeoffset_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.EndDate.Nanosecond),
            ss => ss.Set<Expedition>().Select(e => e.EndDate.Millisecond * 1000000 + e.EndDate.Microsecond * 1000 + e.EndDate.Nanosecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_datetimeoffset_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.EndDate.Nanosecond == 111111100).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.EndDate.Millisecond * 1000000 + e.EndDate.Microsecond * 1000 + e.EndDate.Nanosecond == 111111100)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_datetimeoffset_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.EndDate.Nanosecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.EndDate.Millisecond * 1000000 + e.EndDate.Microsecond * 1000 + e.EndDate.Nanosecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_datetimeoffset_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.EndDate.Nanosecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.EndDate.Millisecond * 1000000 + e.EndDate.Microsecond * 1000 + e.EndDate.Nanosecond)
                .Select(e => e.Id)
            );

    #endregion

    #region DATEPART TimeOnly

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_timeonly_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartTime.Microsecond),
            ss => ss.Set<Expedition>().Select(e => e.StartTime.Millisecond * 1000 + e.StartTime.Microsecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_timeonly_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.StartTime.Microsecond == 111111).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.StartTime.Millisecond * 1000 + e.StartTime.Microsecond == 111111)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_timeonly_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.StartTime.Microsecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.StartTime.Millisecond * 1000 + e.StartTime.Microsecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_timeonly_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.StartTime.Microsecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.StartTime.Millisecond * 1000 + e.StartTime.Microsecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_timeonly_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartTime.Nanosecond),
            ss => ss.Set<Expedition>().Select(e => e.StartTime.Millisecond * 1000000 + e.StartTime.Microsecond * 1000 + e.StartTime.Nanosecond));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_timeonly_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.StartTime.Nanosecond == 111111100).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.StartTime.Millisecond * 1000000 + e.StartTime.Microsecond * 1000 + e.StartTime.Nanosecond == 111111100)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_timeonly_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.StartTime.Nanosecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.StartTime.Millisecond * 1000000 + e.StartTime.Microsecond * 1000 + e.StartTime.Nanosecond)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_timeonly_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.StartTime.Nanosecond).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.StartTime.Millisecond * 1000000 + e.StartTime.Microsecond * 1000 + e.StartTime.Nanosecond)
                .Select(e => e.Id)
            );

    #endregion

    #region DATEPART TimeSpan

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_timespan_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.Duration.Microseconds),
            ss => ss.Set<Expedition>().Select(e => e.Duration.Milliseconds * 1000 + e.Duration.Microseconds));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_timespan_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.Duration.Microseconds == 111111).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.Duration.Milliseconds * 1000 + e.Duration.Microseconds == 111111)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_timespan_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.Duration.Microseconds).Select(e => e.Id),
            ss => ss.Set<Expedition>().OrderBy(e => e.Duration.Milliseconds * 1000 + e.Duration.Microseconds).Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_timespan_microsecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.Duration.Microseconds).Select(e => e.Id),
            ss => ss.Set<Expedition>().OrderByDescending(e => e.Duration.Milliseconds * 1000 + e.Duration.Microseconds).Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_timespan_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.Duration.Nanoseconds),
            ss => ss.Set<Expedition>().Select(e => e.Duration.Milliseconds * 1000000 + e.Duration.Microseconds * 1000 + e.Duration.Nanoseconds));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_by_timespan_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Where(e => e.Duration.Nanoseconds == 111111100).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .Where(e => e.Duration.Milliseconds * 1000000 + e.Duration.Microseconds * 1000 + e.Duration.Nanoseconds == 111111100)
                .Select(e => e.Id));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_timespan_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderBy(e => e.Duration.Nanoseconds).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderBy(e => e.Duration.Milliseconds * 1000000 + e.Duration.Microseconds * 1000 + e.Duration.Nanoseconds)
                .Select(e => e.Id)
            );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderByDescending_timespan_nanosecond_component(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().OrderByDescending(e => e.Duration.Nanoseconds).Select(e => e.Id),
            ss => ss.Set<Expedition>()
                .OrderByDescending(e => e.Duration.Milliseconds * 1000000 + e.Duration.Microseconds * 1000 + e.Duration.Nanoseconds)
                .Select(e => e.Id)
            );

    #endregion
}
