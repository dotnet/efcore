// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class ComparisonOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Equal(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task NotEqual(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int != 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GreaterThan(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int > 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GreaterThanOrEqual(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int >= 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task LessThan(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int < 8));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task LessThanOrEqual(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int <= 8));
}
