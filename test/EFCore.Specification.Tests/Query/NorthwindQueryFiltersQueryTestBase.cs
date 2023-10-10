// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable StaticMemberInGenericType
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable ConvertMethodToExpressionBody
// ReSharper disable StringStartsWithIsCultureSpecific
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindQueryFiltersQueryTestBase<TFixture> : FilteredQueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NorthwindQueryFiltersCustomizer>, new()
{
    protected NorthwindQueryFiltersQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_query(bool async)
    {
        return AssertFilteredCount(
            async,
            ss => ss.Set<Customer>());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Materialized_query(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Customer>());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Find(bool async)
    {
        using var context = Fixture.CreateContext();
        if (async)
        {
            Assert.Null(await context.FindAsync<Customer>("ALFKI"));
        }
        else
        {
            Assert.Null(context.Find<Customer>("ALFKI"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Client_eval(bool async)
    {
        Assert.Equal(
            CoreStrings.TranslationFailed("DbSet<Product>()    .Where(p => NorthwindContext.ClientMethod(p))"),
            RemoveNewLines(
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertFilteredQuery(
                        async,
                        ss => ss.Set<Product>()))).Message));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Materialized_query_parameter(bool async)
    {
        using var context = Fixture.CreateContext();
        context.TenantPrefix = "F";

        if (async)
        {
            Assert.Equal(8, (await context.Customers.ToListAsync()).Count);
        }
        else
        {
            Assert.Equal(8, context.Customers.ToList().Count);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Materialized_query_parameter_new_context(bool async)
    {
        using var context1 = Fixture.CreateContext();
        if (async)
        {
            Assert.Equal(7, (await context1.Customers.ToListAsync()).Count);

            using var context2 = Fixture.CreateContext();
            context2.TenantPrefix = "T";

            Assert.Equal(6, (await context2.Customers.ToListAsync()).Count);
        }
        else
        {
            Assert.Equal(7, context1.Customers.ToList().Count);

            using var context2 = Fixture.CreateContext();
            context2.TenantPrefix = "T";

            Assert.Equal(6, context2.Customers.ToList().Count);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projection_query(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.CustomerID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Projection_query_parameter(bool async)
    {
        using var context = Fixture.CreateContext();
        if (async)
        {
            context.TenantPrefix = "F";

            Assert.Equal(8, (await context.Customers.Select(c => c.CustomerID).ToListAsync()).Count);
        }
        else
        {
            context.TenantPrefix = "F";

            Assert.Equal(8, context.Customers.Select(c => c.CustomerID).ToList().Count);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_query(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Customer>().Include(c => c.Orders),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(x => x.Orders)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_query_opt_out(bool async)
    {
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Include(c => c.Orders).IgnoreQueryFilters(),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Customer>(x => x.Orders)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Included_many_to_one_query2(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Order>().Include(o => o.Customer));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Included_many_to_one_query(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Order>().Include(o => o.Customer),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(x => x.Customer)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_reference_that_itself_has_query_filter_with_another_reference(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<OrderDetail>().Select(od => od.Order));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Included_one_to_many_query_with_client_eval(bool async)
    {
        Assert.Equal(
            CoreStrings.TranslationFailed("DbSet<Product>()    .Where(p => NorthwindContext.ClientMethod(p))"),
            RemoveNewLines(
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertFilteredQuery(
                        async,
                        ss => ss.Set<Product>().Include(p => p.OrderDetails)))).Message));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navs_query(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in c.Orders
                  from od in o.OrderDetails
                  where od.Discount < 10
                  select c);
    }

    [ConditionalFact]
    public virtual void Compiled_query()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using var context1 = Fixture.CreateContext();
        Assert.Equal("BERGS", query(context1, "BERGS").First().CustomerID);

        using var context2 = Fixture.CreateContext();
        Assert.Equal("BLAUS", query(context2, "BLAUS").First().CustomerID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_Equality(bool async)
    {
        return AssertFilteredQuery(
            async,
            ss => ss.Set<Order>());
    }

    private string RemoveNewLines(string message)
        => message.Replace("\n", "").Replace("\r", "");
}
