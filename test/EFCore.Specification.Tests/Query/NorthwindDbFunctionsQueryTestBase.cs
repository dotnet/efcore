// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindDbFunctionsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindDbFunctionsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_literal(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Like(c.ContactName, "%M%"),
            c => c.ContactName.Contains("M") || c.ContactName.Contains("m"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_identity(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Like(c.ContactName, c.ContactName),
            c => true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_literal_with_escape(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Like(c.ContactName, "!%", "!"),
            c => c.ContactName.Contains("%"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_all_literals(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Like("FOO", "%O%"),
            c => "FOO".Contains("O") || "FOO".Contains("m"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Like_all_literals_with_escape(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Like("%", "!%", "!"),
            c => "%".Contains("%"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_return_less_than_1(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            ss => EF.Functions.Random() < 1,
            c => true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Random_return_greater_than_0(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>(),
            ss => ss.Set<Order>(),
            ss => EF.Functions.Random() >= 0,
            c => true);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
