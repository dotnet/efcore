// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindWhereQueryRelationalTestBase<TFixture>(TFixture fixture) : NorthwindWhereQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    public override Task Where_bool_client_side_negated(bool async)
        => AssertTranslationFailed(() => base.Where_bool_client_side_negated(async));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task EF_MultipleParameters_with_non_evaluatable_argument_throws(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders == EF.MultipleParameters(c.Orders))));

        Assert.Equal(CoreStrings.EFMethodWithNonEvaluatableArgument("EF.MultipleParameters<T>"), exception.Message);
    }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
