// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindSetOperationsQueryRelationalTestBase<TFixture> : NorthwindSetOperationsQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindSetOperationsQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override async Task Collection_projection_after_set_operation_fails_if_distinct(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Collection_projection_after_set_operation_fails_if_distinct(async))).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    public override async Task Collection_projection_before_set_operation_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Collection_projection_before_set_operation_fails(async))).Message;

        Assert.Equal(RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation, message);
    }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
