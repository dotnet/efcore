// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindMiscellaneousQueryRelationalTestBase<TFixture> : NorthwindMiscellaneousQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindMiscellaneousQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_collection_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(e => e.CustomerID).AsSplitQuery()
                .Select(c => c.Orders),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_collection_then_include_split(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Include(c => c.Orders).ThenInclude(o => o.OrderDetails)
                .OrderBy(e => e.CustomerID).AsSplitQuery().Select(c => c.Orders),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(
                e, a,
                elementAsserter: (eo, ao) => AssertInclude(eo, ao, new ExpectedInclude<Order>(o => o.OrderDetails))));

    public override Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Using_static_string_Equals_with_StringComparison_throws_informative_error(async),
            CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);

    public override Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Using_string_Equals_with_StringComparison_throws_informative_error(async),
            CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison);

    public override Task Random_next_is_not_funcletized_1(bool async)
        => AssertTranslationFailed(() => base.Random_next_is_not_funcletized_1(async));

    public override Task Random_next_is_not_funcletized_2(bool async)
        => AssertTranslationFailed(() => base.Random_next_is_not_funcletized_2(async));

    public override Task Random_next_is_not_funcletized_3(bool async)
        => AssertTranslationFailed(() => base.Random_next_is_not_funcletized_3(async));

    public override Task Random_next_is_not_funcletized_4(bool async)
        => AssertTranslationFailed(() => base.Random_next_is_not_funcletized_4(async));

    public override Task Random_next_is_not_funcletized_5(bool async)
        => AssertTranslationFailed(() => base.Random_next_is_not_funcletized_5(async));

    public override Task Random_next_is_not_funcletized_6(bool async)
        => AssertTranslationFailed(() => base.Random_next_is_not_funcletized_6(async));

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
