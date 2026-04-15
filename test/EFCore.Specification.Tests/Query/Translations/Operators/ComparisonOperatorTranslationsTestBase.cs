// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public abstract class ComparisonOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual async Task Equal()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8));

    [ConditionalFact]
    public virtual async Task NotEqual()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int != 8));

    [ConditionalFact]
    public virtual async Task GreaterThan()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int > 8));

    [ConditionalFact]
    public virtual async Task GreaterThanOrEqual()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int >= 8));

    [ConditionalFact]
    public virtual async Task LessThan()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int < 8));

    [ConditionalFact]
    public virtual async Task LessThanOrEqual()
        => await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int <= 8));
}
