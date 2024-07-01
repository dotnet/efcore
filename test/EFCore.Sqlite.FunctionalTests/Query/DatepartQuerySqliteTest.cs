// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.DatepartModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class DatepartQuerySqliteTest : DatepartQueryRelationalTestBase<DatepartQuerySqliteFixture>
{
    public DatepartQuerySqliteTest(DatepartQuerySqliteFixture fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    #region DATEPART DateTime

    public override Task Select_datetime_microsecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartDate.Millisecond * 1000 + e.StartDate.Microsecond)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_datetime_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_datetime_microsecond_component(async));

    public override async Task OrderBy_datetime_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_datetime_microsecond_component(async));

    public override async Task OrderByDescending_datetime_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_datetime_microsecond_component(async));

    public override Task Select_datetime_nanosecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartDate.Millisecond * 1000000 + e.StartDate.Microsecond * 1000 + e.StartDate.Nanosecond)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_datetime_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_datetime_nanosecond_component(async));

    public override async Task OrderBy_datetime_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_datetime_nanosecond_component(async));

    public override async Task OrderByDescending_datetime_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_datetime_nanosecond_component(async));

    #endregion

    #region DATEPART DateTimeOffset

    public override Task Select_datetimeoffset_microsecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.EndDate.Millisecond * 1000 + e.EndDate.Microsecond)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_datetimeoffset_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_datetimeoffset_microsecond_component(async));

    public override async Task OrderBy_datetimeoffset_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_datetimeoffset_microsecond_component(async));

    public override async Task OrderByDescending_datetimeoffset_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_datetimeoffset_microsecond_component(async));

    public override Task Select_datetimeoffset_nanosecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.EndDate.Millisecond * 1000000 + e.EndDate.Microsecond * 1000 + e.EndDate.Nanosecond)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_datetimeoffset_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_datetimeoffset_nanosecond_component(async));

    public override async Task OrderBy_datetimeoffset_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_datetimeoffset_nanosecond_component(async));

    public override async Task OrderByDescending_datetimeoffset_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_datetimeoffset_nanosecond_component(async));

    #endregion

    #region DATEPART TimeOnly

    public override Task Select_timeonly_microsecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartTime.Millisecond * 1000 + e.StartTime.Microsecond)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_timeonly_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_timeonly_microsecond_component(async));

    public override async Task OrderBy_timeonly_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_timeonly_microsecond_component(async));

    public override async Task OrderByDescending_timeonly_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_timeonly_microsecond_component(async));

    public override Task Select_timeonly_nanosecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.StartTime.Millisecond * 1000000 + e.StartTime.Microsecond * 1000 + e.StartTime.Nanosecond)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_timeonly_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_timeonly_nanosecond_component(async));

    public override async Task OrderBy_timeonly_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_timeonly_nanosecond_component(async));

    public override async Task OrderByDescending_timeonly_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_timeonly_nanosecond_component(async));

    #endregion

    #region DATEPART TimeSpan

    public override Task Select_timespan_microsecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.Duration.Milliseconds * 1000 + e.Duration.Microseconds)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_timespan_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_timespan_microsecond_component(async));

    public override async Task OrderBy_timespan_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_timespan_microsecond_component(async));

    public override async Task OrderByDescending_timespan_microsecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_timespan_microsecond_component(async));

    public override Task Select_timespan_nanosecond_component(bool async)
    {
        AssertQueryScalar(
            async,
            ss => ss.Set<Expedition>().Select(e => e.Duration.Milliseconds * 1000000 + e.Duration.Microseconds * 1000 + e.Duration.Nanoseconds)
            );

        return Task.CompletedTask;
    }

    public override async Task Select_by_timespan_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.Select_by_timespan_nanosecond_component(async));

    public override async Task OrderBy_timespan_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderBy_timespan_nanosecond_component(async));

    public override async Task OrderByDescending_timespan_nanosecond_component(bool async)
        => await AssertTranslationFailed(() => base.OrderByDescending_timespan_nanosecond_component(async));

    #endregion
}
