// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindWhereQueryRelationalTestBase<TFixture>(TFixture fixture) : NorthwindWhereQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    public override Task Where_bool_client_side_negated(bool async)
        => AssertTranslationFailed(() => base.Where_bool_client_side_negated(async));

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
