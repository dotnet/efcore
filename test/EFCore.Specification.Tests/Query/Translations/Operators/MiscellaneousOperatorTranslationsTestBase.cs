// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class MiscellaneousOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [Fact]
    public virtual async Task Conditional()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int == 8 ? b.String : "Foo") == "Seattle"));

    [Fact]
    public virtual async Task Conditional_simplifiable_equality()
        // ReSharper disable once MergeConditionalExpression
        => await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(x => (x.Int == 9 ? 9 : x.Int) > 1));

    [Fact]
    public virtual async Task Conditional_simplifiable_inequality()
        // ReSharper disable once MergeConditionalExpression
        => await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(x => (x.Int != 8 ? x.Int : 8) > 1));

    // In relational providers, x == a ? null : x ("un-coalescing conditional") is translated to SQL NULLIF

    [Fact]
    public virtual async Task Conditional_uncoalesce_with_equality_left()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => (x.Int == 9 ? null : x.Int) > 1));

    [Fact]
    public virtual async Task Conditional_uncoalesce_with_equality_right()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => (9 == x.Int ? null : x.Int) > 1));

    [Fact]
    public virtual async Task Conditional_uncoalesce_with_inequality_left()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => (x.Int != 9 ? x.Int : null) > 1));

    [Fact]
    public virtual async Task Conditional_uncoalesce_with_inequality_right()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => (9 != x.Int ? x.Int : null) > 1));

    [Fact]
    public virtual async Task Conditional_uncoalesce_with_string()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(x => (x.String == "Seattle" ? null : x.String) == "London"));

    [Fact]
    public virtual async Task Coalesce()
        => await AssertQuery(ss => ss.Set<NullableBasicTypesEntity>().Where(b => (b.String ?? "Unknown") == "Seattle"));
}
