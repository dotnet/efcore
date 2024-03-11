// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindNavigationsQueryRelationalTestBase<TFixture> : NorthwindNavigationsQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindNavigationsQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override Task Where_subquery_on_navigation_client_eval(bool async)
        => AssertTranslationFailed(() => base.Where_subquery_on_navigation_client_eval(async));

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
