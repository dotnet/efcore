// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class CompositeKeysQueryRelationalTestBase<TFixture>(TFixture fixture) : CompositeKeysQueryTestBase<TFixture>(fixture)
    where TFixture : CompositeKeysQueryFixtureBase, new()
{
    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
