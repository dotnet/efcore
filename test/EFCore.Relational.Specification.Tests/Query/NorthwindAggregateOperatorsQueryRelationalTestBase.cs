// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindAggregateOperatorsQueryRelationalTestBase<TFixture> : NorthwindAggregateOperatorsQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindAggregateOperatorsQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override async Task Last_when_no_order_by(bool async)
        => Assert.Equal(
            RelationalStrings.LastUsedWithoutOrderBy(nameof(Enumerable.Last)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Last_when_no_order_by(async))).Message);

    public override async Task LastOrDefault_when_no_order_by(bool async)
        => Assert.Equal(
            RelationalStrings.LastUsedWithoutOrderBy(nameof(Enumerable.LastOrDefault)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.LastOrDefault_when_no_order_by(async))).Message);

    public override async Task Contains_over_keyless_entity_throws(bool async)
        => Assert.Equal(
            CoreStrings.EntityEqualityOnKeylessEntityNotSupported("Equals", nameof(CustomerQuery)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Contains_over_keyless_entity_throws(async))).Message);

    public override async Task Min_no_data_subquery(bool async)
        => Assert.Equal(
            "Nullable object must have a value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Min_no_data_subquery(async))).Message);

    public override async Task Max_no_data_subquery(bool async)
        => Assert.Equal(
            "Nullable object must have a value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Max_no_data_subquery(async))).Message);

    public override async Task Average_no_data_subquery(bool async)
        => Assert.Equal(
            "Nullable object must have a value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Average_no_data_subquery(async))).Message);

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture,
            RewriteExpectedQueryExpression,
            RewriteServerQueryExpression);
}
