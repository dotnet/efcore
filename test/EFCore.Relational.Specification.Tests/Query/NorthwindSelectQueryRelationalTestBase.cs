// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindSelectQueryRelationalTestBase<TFixture> : NorthwindSelectQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindSelectQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
        => AssertTranslationFailed(() => base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async));

    public override Task Reverse_without_explicit_ordering(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Reverse_without_explicit_ordering(async), RelationalStrings.MissingOrderingInSelectExpression);

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
