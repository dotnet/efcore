// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class ArithmeticOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Add(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int + 2 == 10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Subtract(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int - 3 == 5));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Multiply(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int * 2 == 16));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Modulo(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int % 3 == 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Minus(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => -b.Int == -8));
}
