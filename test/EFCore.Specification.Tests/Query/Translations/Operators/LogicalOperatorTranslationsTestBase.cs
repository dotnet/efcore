// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class LogicalOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual async Task And()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 && b.String == "Seattle"));

    [ConditionalFact]
    public virtual async Task And_with_bool_property()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Bool && b.String == "Seattle"));

    [ConditionalFact]
    public virtual async Task Or()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 999 || b.String == "Seattle"));

    [ConditionalFact]
    public virtual async Task Or_with_bool_property()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Bool || b.String == "Seattle"));

    [ConditionalFact]
    public virtual async Task Not()
        => await AssertQuery(
            // ReSharper disable once NegativeEqualityExpression
            ss => ss.Set<BasicTypesEntity>().Where(b => !(b.Int == 999)));

    [ConditionalFact]
    public virtual async Task Not_with_bool_property()
        => await AssertQuery(
            // ReSharper disable once NegativeEqualityExpression
            ss => ss.Set<BasicTypesEntity>().Where(b => !b.Bool));
}
