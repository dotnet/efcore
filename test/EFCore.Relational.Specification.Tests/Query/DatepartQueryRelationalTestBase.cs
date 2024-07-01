// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class DatepartQueryRelationalTestBase<TFixture> : DatepartQueryTestBase<TFixture>
    where TFixture : DatepartQueryFixtureBase, new()
{
    public DatepartQueryRelationalTestBase(TFixture fixture) : base(fixture)
    { }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
