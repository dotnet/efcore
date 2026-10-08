// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Temporal;

public abstract class TimeSpanTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Hours()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Hours == 3));

    [ConditionalFact]
    public virtual Task Minutes()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Minutes == 4));

    [ConditionalFact]
    public virtual Task Seconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Seconds == 5));

    [ConditionalFact]
    public virtual Task Milliseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Milliseconds == 678));

    [ConditionalFact]
    public virtual Task Microseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Microseconds == 912));

    [ConditionalFact]
    public virtual Task Nanoseconds()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.TimeSpan.Nanoseconds == 400));
}
