// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
// ReSharper disable StringStartsWithIsCultureSpecific

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindIncludeNoTrackingQueryTestBase<TFixture> : NorthwindIncludeQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    private static readonly MethodInfo _asNoTrackingMethodInfo
        = typeof(EntityFrameworkQueryableExtensions)
            .GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking));

    protected NorthwindIncludeNoTrackingQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    // Include with cycles are not allowed in no tracking query.
    public override async Task Include_multi_level_reference_and_collection_predicate(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multi_level_reference_and_collection_predicate(async))).Message);

    public override async Task Include_multi_level_reference_then_include_collection_predicate(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multi_level_reference_then_include_collection_predicate(async))).Message);

    public override async Task Include_multiple_references_and_collection_multi_level(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multiple_references_and_collection_multi_level(async))).Message);

    public override async Task Include_multiple_references_and_collection_multi_level_reverse(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multiple_references_and_collection_multi_level_reverse(async))).Message);

    public override async Task Include_multiple_references_then_include_collection_multi_level(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multiple_references_then_include_collection_multi_level(async))).Message);

    public override async Task Include_multiple_references_then_include_collection_multi_level_reverse(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multiple_references_then_include_collection_multi_level_reverse(async))).Message);

    public override async Task Include_reference_and_collection_order_by(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_reference_and_collection_order_by(async))).Message);

    public override async Task Include_references_and_collection_multi_level(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_references_and_collection_multi_level(async))).Message);

    public override async Task Include_references_and_collection_multi_level_predicate(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_references_and_collection_multi_level_predicate(async))).Message);

    public override async Task Include_references_then_include_collection(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_references_then_include_collection(async))).Message);

    public override async Task Include_references_then_include_collection_multi_level(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_references_then_include_collection_multi_level(async))).Message);

    public override async Task Include_references_then_include_collection_multi_level_predicate(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_references_then_include_collection_multi_level_predicate(async))).Message);

    public override async Task Include_closes_reader(bool async)
    {
        using var context = CreateContext();
        if (async)
        {
            Assert.NotNull(await context.Set<Customer>().Include(c => c.Orders).AsNoTracking().FirstOrDefaultAsync());
            Assert.NotNull(await context.Set<Product>().AsNoTracking().ToListAsync());
        }
        else
        {
            Assert.NotNull(context.Set<Customer>().Include(c => c.Orders).AsNoTracking().FirstOrDefault());
            Assert.NotNull(context.Set<Product>().AsNoTracking().ToList());
        }
    }

    public override async Task Include_collection_dependent_already_tracked(bool async)
    {
        using var context = CreateContext();
        var orders = context.Set<Order>().Where(o => o.CustomerID == "ALFKI").ToList();
        Assert.Equal(6, context.ChangeTracker.Entries().Count());
        Assert.True(orders.All(o => o.Customer.CustomerID == null));

        var customer
            = async
                ? await context.Set<Customer>()
                    .Include(c => c.Orders)
                    .AsNoTracking()
                    .SingleAsync(c => c.CustomerID == "ALFKI")
                : context.Set<Customer>()
                    .Include(c => c.Orders)
                    .AsNoTracking()
                    .Single(c => c.CustomerID == "ALFKI");

        Assert.NotEqual(orders, customer.Orders, ReferenceEqualityComparer.Instance);
        Assert.Equal(6, customer.Orders.Count);
        Assert.True(customer.Orders.All(e => ReferenceEquals(e.Customer, customer)));

        Assert.Equal(6, context.ChangeTracker.Entries().Count());
        Assert.True(orders.All(o => o.Customer.CustomerID == null));
    }

    public override async Task Include_collection_principal_already_tracked(bool async)
    {
        using var context = CreateContext();
        var customer1 = context.Set<Customer>().Single(c => c.CustomerID == "ALFKI");
        Assert.Single(context.ChangeTracker.Entries());

        var customer2
            = async
                ? await context.Set<Customer>()
                    .Include(c => c.Orders)
                    .AsNoTracking()
                    .SingleAsync(c => c.CustomerID == "ALFKI")
                : context.Set<Customer>()
                    .Include(c => c.Orders)
                    .AsNoTracking()
                    .Single(c => c.CustomerID == "ALFKI");

        Assert.NotSame(customer1, customer2);
        Assert.Equal(6, customer2.Orders.Count);
        Assert.True(customer2.Orders.All(o => o.Customer != null));
        Assert.True(customer2.Orders.All(o => ReferenceEquals(o.Customer, customer2)));

        Assert.Single(context.ChangeTracker.Entries());
    }

    public override async Task Include_reference_dependent_already_tracked(bool async)
    {
        using var context = CreateContext();
        var customer = context.Set<Customer>().Single(o => o.CustomerID == "ALFKI");
        Assert.Single(context.ChangeTracker.Entries());

        var orders
            = async
                ? await context.Set<Order>().Include(o => o.Customer).AsNoTracking().Where(o => o.CustomerID == "ALFKI").ToListAsync()
                : context.Set<Order>().Include(o => o.Customer).AsNoTracking().Where(o => o.CustomerID == "ALFKI").ToList();

        Assert.Equal(6, orders.Count);
        Assert.True(orders.All(o => !ReferenceEquals(o.Customer, customer)));
        Assert.True(orders.All(o => o.Customer != null));
        Assert.Single(context.ChangeTracker.Entries());
    }

    public override async Task Include_with_cycle_does_not_throw_when_AsNoTrackingWithIdentityResolution(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multi_level_reference_then_include_collection_predicate(async))).Message);

    public override async Task Include_with_cycle_does_not_throw_when_AsTracking_NoTrackingWithIdentityResolution(bool async)
        => Assert.Equal(
            CoreStrings.IncludeWithCycle("Customer", "Orders"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Include_multi_level_reference_then_include_collection_predicate(async))).Message);

    protected override bool IgnoreEntryCount
        => true;

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        return Expression.Call(
            _asNoTrackingMethodInfo.MakeGenericMethod(serverQueryExpression.Type.TryGetSequenceType()),
            serverQueryExpression);
    }
}
