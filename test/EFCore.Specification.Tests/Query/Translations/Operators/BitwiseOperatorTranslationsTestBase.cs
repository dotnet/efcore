// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

public abstract class BitwiseOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual async Task Or()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int | b.Long) == 7));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.Int | b.Long));
    }

    [ConditionalFact]
    public virtual async Task Or_over_boolean()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 | b.String == "Seattle"));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.Int == 12 | b.String == "Seattle"));
    }

    [ConditionalFact]
    public virtual Task Or_multiple()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int | b.Short | b.Long) == 7));

    [ConditionalFact]
    public virtual async Task And()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int & b.Short) == 2));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.Int & b.Short));
    }

    [ConditionalFact]
    public virtual async Task And_over_boolean()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 & b.String == "Seattle"));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.Int == 8 & b.String == "Seattle"));
    }

    [ConditionalFact]
    public virtual async Task Xor()
    {
        await AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int ^ b.Short) == 1));

        await AssertQueryScalar(ss => ss.Set<BasicTypesEntity>().Select(b => b.Int ^ b.Short));
    }

    [ConditionalFact]
    public virtual Task Xor_over_boolean()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int == b.Short) ^ (b.String == "Seattle")));

    [ConditionalFact]
    public virtual Task Complement()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => ~b.Int == -9));

    [ConditionalFact]
    public virtual Task And_or_over_boolean()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 & b.Short == 12 | b.String == "Seattle"));

    [ConditionalFact]
    public virtual Task Or_with_logical_or()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 | b.Short == 12 || b.String == "Seattle"));

    [ConditionalFact]
    public virtual Task And_with_logical_and()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 & b.Short == 8 && b.String == "Seattle"));

    [ConditionalFact]
    public virtual Task Or_with_logical_and()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 | b.Short == 9 && b.String == "Seattle"));

    [ConditionalFact]
    public virtual Task And_with_logical_or()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 & b.Short == 12 || b.String == "Seattle"));

    [ConditionalFact]
    public virtual Task Left_shift()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int << 1 == 16));

    [ConditionalFact]
    public virtual Task Right_shift()
        => AssertQuery(ss => ss.Set<BasicTypesEntity>().Where(b => b.Int >> 1 == 4));
}
