// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class TimeSpanTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hours(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Hours == 3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Minutes(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Minutes == 4));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Seconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Seconds == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Milliseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Milliseconds == 678));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Microseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Microseconds == 912));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Nanoseconds(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Nanoseconds == 400));
}
