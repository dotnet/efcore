// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class ArithmeticOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual async Task Add()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int + 2 == 10));

    [Fact]
    public virtual async Task Subtract()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int - 3 == 5));

    [Fact]
    public virtual async Task Multiply()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int * 2 == 16));

    [Fact]
    public virtual async Task Modulo()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int % 3 == 2));

    [Fact]
    public virtual async Task Minus()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => -b.Int == -8));
}
