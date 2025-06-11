// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class LogicalOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task And(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 && b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task And_with_bool_property(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Bool && b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Or(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 999 || b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Or_with_bool_property(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Bool || b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Not(bool async)
        => await AssertQuery(
            async,
            // ReSharper disable once NegativeEqualityExpression
            ss => ss.Set<BasicTypesEntity>().Where(b => !(b.Int == 999)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Not_with_bool_property(bool async)
        => await AssertQuery(
            async,
            // ReSharper disable once NegativeEqualityExpression
            ss => ss.Set<BasicTypesEntity>().Where(b => !b.Bool));
}
