// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class SpatialQueryRelationalTestBase<TFixture> : SpatialQueryTestBase<TFixture>
    where TFixture : SpatialQueryFixtureBase, new()
{
    protected SpatialQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
