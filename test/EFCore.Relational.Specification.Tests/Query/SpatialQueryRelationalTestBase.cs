// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class SpatialQueryRelationalTestBase<TFixture> : SpatialQueryTestBase<TFixture>
    where TFixture : SpatialQueryFixtureBase, new()
{
    protected SpatialQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected virtual bool CanExecuteQueryString
        => false;

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
}
