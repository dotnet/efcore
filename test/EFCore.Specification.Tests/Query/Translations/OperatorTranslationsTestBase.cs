// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public abstract class OperatorTranslationsTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : BasicTypesQueryFixtureBase, new()
{
    #region Bitwise
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bitwise_or(bool async)
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
    public virtual async Task Bitwise_or_over_boolean(bool async)
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
    public virtual Task Bitwise_or_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int | b.Short | b.Long) == 7));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Bitwise_and(bool async)
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
    public virtual async Task Bitwise_and_over_boolean(bool async)
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
    public virtual async Task Bitwise_xor(bool async)
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
    public virtual Task Bitwise_xor_over_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => (b.Int == b.Short) ^ (b.String == "Seattle")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_complement(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => ~b.Int == -9));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_and_or_over_boolean(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 & b.Short == 12 | b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_or_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 | b.Short == 12 || b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_and_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 & b.Short == 8 && b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_or_with_logical_and(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 8 | b.Short == 9 && b.String == "Seattle"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Bitwise_and_with_logical_or(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BasicTypesEntity>().Where(b => b.Int == 12 & b.Short == 12 || b.String == "Seattle"));

#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
    #endregion Bitwise
}
