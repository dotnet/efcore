// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

public abstract class BitwiseOperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Or(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int | b.Long) == 7));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Int | b.Long));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Or_over_boolean(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 | b.String == "Seattle"));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Int == 12 | b.String == "Seattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Or_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int | b.Short | b.Long) == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task And(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int & b.Short) == 2));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Int & b.Short));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task And_over_boolean(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 & b.String == "Seattle"));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Int == 8 & b.String == "Seattle"));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Xor(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int ^ b.Short) == 1));

        await AssertQueryScalar(
            async,
            ss => ss.Set<BasicTypesEntity>().Select(b => b.Int ^ b.Short));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Xor_over_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int == b.Short) ^ (b.String == "Seattle")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Complement(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => ~b.Int == -9));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task And_or_over_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 & b.Short == 12 | b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Or_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 | b.Short == 12 || b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task And_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 & b.Short == 8 && b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Or_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 | b.Short == 9 && b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task And_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 & b.Short == 12 || b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_shift(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int << 1 == 16));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Right_shift(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int >> 1 == 4));
}
