// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindDbFunctionsQueryRelationalTestBase<TFixture> : NorthwindDbFunctionsQueryTestBase<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    protected NorthwindDbFunctionsQueryRelationalTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collate_case_insensitive(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Collate(c.ContactName, CaseInsensitiveCollation) == "maria anders",
            c => c.ContactName.Equals("maria anders", StringComparison.OrdinalIgnoreCase));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collate_case_sensitive(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Collate(c.ContactName, CaseSensitiveCollation) == "maria anders",
            c => c.ContactName.Equals("maria anders", StringComparison.Ordinal));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collate_case_sensitive_constant(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => c.ContactName == EF.Functions.Collate("maria anders", CaseSensitiveCollation),
            c => c.ContactName.Equals("maria anders", StringComparison.Ordinal));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Least(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => EF.Functions.Least(od.OrderID, 10251) == 10251),
            ss => ss.Set<OrderDetail>().Where(od => Math.Min(od.OrderID, 10251) == 10251));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Greatest(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => EF.Functions.Greatest(od.OrderID, 10251) == 10251),
            ss => ss.Set<OrderDetail>().Where(od => Math.Max(od.OrderID, 10251) == 10251));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Least_with_parameter_array_is_not_supported(bool async)
    {
        var arr = new[] { 1, 2 };

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<OrderDetail>().Where(od => EF.Functions.Least(arr) == 10251)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Greatest_with_parameter_array_is_not_supported(bool async)
    {
        var arr = new[] { 1, 2 };

        await AssertTranslationFailed(
            () =>
                AssertQuery(
                    async,
                    ss => ss.Set<OrderDetail>().Where(od => EF.Functions.Greatest(arr) == 10251)));
    }

    protected abstract string CaseInsensitiveCollation { get; }
    protected abstract string CaseSensitiveCollation { get; }
}
