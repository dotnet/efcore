// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable FormatStringProblem
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindSplitIncludeNoTrackingQueryTestBase<TFixture> : NorthwindIncludeNoTrackingQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    private static readonly MethodInfo _asSplitIncludeMethodInfo
        = typeof(RelationalQueryableExtensions)
            .GetTypeInfo().GetDeclaredMethod(nameof(RelationalQueryableExtensions.AsSplitQuery));

    protected NorthwindSplitIncludeNoTrackingQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    public override async Task Include_closes_reader(bool async)
    {
        using var context = CreateContext();
        if (async)
        {
            Assert.NotNull(await context.Set<Customer>().Include(c => c.Orders).AsNoTracking().AsSplitQuery().FirstOrDefaultAsync());
            Assert.NotNull(await context.Set<Product>().AsNoTracking().ToListAsync());
        }
        else
        {
            Assert.NotNull(context.Set<Customer>().Include(c => c.Orders).AsNoTracking().AsSplitQuery().FirstOrDefault());
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
                    .AsSplitQuery()
                    .AsNoTracking()
                    .SingleAsync(c => c.CustomerID == "ALFKI")
                : context.Set<Customer>()
                    .Include(c => c.Orders)
                    .AsSplitQuery()
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
                    .AsSplitQuery()
                    .AsNoTracking()
                    .SingleAsync(c => c.CustomerID == "ALFKI")
                : context.Set<Customer>()
                    .Include(c => c.Orders)
                    .AsSplitQuery()
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
                ? await context.Set<Order>().Include(o => o.Customer).AsNoTracking().AsSplitQuery().Where(o => o.CustomerID == "ALFKI")
                    .ToListAsync()
                : context.Set<Order>().Include(o => o.Customer).AsNoTracking().AsSplitQuery().Where(o => o.CustomerID == "ALFKI")
                    .ToList();

        Assert.Equal(6, orders.Count);
        Assert.True(orders.All(o => !ReferenceEquals(o.Customer, customer)));
        Assert.True(orders.All(o => o.Customer != null));
        Assert.Single(context.ChangeTracker.Entries());
    }

    public override async Task Include_collection_with_last_no_orderby(bool async)
        => Assert.Equal(
            RelationalStrings.LastUsedWithoutOrderBy(nameof(Queryable.Last)),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Include_collection_with_last_no_orderby(async))).Message);

    protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
    {
        serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

        return Expression.Call(
            _asSplitIncludeMethodInfo.MakeGenericMethod(serverQueryExpression.Type.GetSequenceType()),
            serverQueryExpression);
    }
}
