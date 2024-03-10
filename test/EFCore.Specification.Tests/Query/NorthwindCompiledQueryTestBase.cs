// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToExpressionBodyWhenPossible
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindCompiledQueryTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindCompiledQueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalFact]
    public virtual void DbSet_query()
    {
        var query = EF.CompileQuery((NorthwindContext context) => context.Customers);

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).Count());
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).ToList().Count);
        }
    }

    [ConditionalFact]
    public virtual void DbSet_query_first()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context) => context.Set<Customer>().OrderBy(c => c.CustomerID).First());

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context).CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Keyless_query()
    {
        var query = EF.CompileQuery((NorthwindContext context) => context.CustomerQueries);

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).Count());
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).ToList().Count);
        }
    }

    [ConditionalFact]
    public virtual void Keyless_query_first()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context) => context.CustomerQueries.OrderBy(c => c.CompanyName).First());

        using (var context = CreateContext())
        {
            Assert.Equal("Alfreds Futterkiste", query(context).CompanyName);
        }
    }

    [ConditionalFact]
    public virtual void Query_ending_with_include()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context)
                => context.Customers.Include(c => c.Orders));

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).ToList().Count);
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).ToList().Count);
        }
    }

    [ConditionalFact]
    public virtual void Untyped_context()
    {
        var query = EF.CompileQuery((DbContext context) => context.Set<Customer>());

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).Count());
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, query(context).ToList().Count);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_single_parameter()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context, "ALFKI").First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", query(context, "ANATR").First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_single_parameter_with_include()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID).Include(c => c.Orders));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context, "ALFKI").First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", query(context, "ANATR").First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void First_query_with_single_parameter()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.First(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context, "ALFKI").CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", query(context, "ANATR").CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_two_parameters()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, object _, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context, null, "ALFKI").First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", query(context, null, "ANATR").First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_three_parameters()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, object _, int __, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context, null, 1, "ALFKI").First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", query(context, null, 1, "ANATR").First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_array_parameter()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string[] args)
                => context.Customers.Where(c => c.CustomerID == args[0]));

        using (var context = CreateContext())
        {
            Assert.Equal(1, query(context, ["ALFKI"]).Count());
        }

        using (var context = CreateContext())
        {
            Assert.Equal(1, query(context, ["ANATR"]).Count());
        }
    }

    [ConditionalFact]
    public virtual void Query_with_contains()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string[] args)
                => context.Customers.Where(c => args.Contains(c.CustomerID)));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context, ["ALFKI"]).First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", query(context, ["ANATR"]).First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Multiple_queries()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context)
                => context.Customers.OrderBy(c => c.CustomerID).Select(c => c.CustomerID).FirstOrDefault()
                + context.Orders.OrderBy(o => o.CustomerID).Select(o => o.CustomerID).FirstOrDefault());

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKIALFKI", query(context));
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKIALFKI", query(context));
        }
    }

    [ConditionalFact]
    public virtual void Query_with_closure()
    {
        var customerID = "ALFKI";

        var query = EF.CompileQuery(
            (NorthwindContext context)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context).First().CustomerID);
        }

        customerID = "ANATR";

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", query(context).First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual void Query_with_closure_null()
    {
        string customerID = null;

        var query = EF.CompileQuery(
            (NorthwindContext context)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Null(query(context).FirstOrDefault());
        }
    }

    [ConditionalFact]
    public virtual async Task DbSet_query_async()
    {
        var query = EF.CompileAsyncQuery((NorthwindContext context) => context.Customers);

        using (var context = CreateContext())
        {
            Assert.Equal(91, (await query(context).ToListAsync()).Count);
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, (await query(context).ToListAsync()).Count);
        }
    }

    [ConditionalFact]
    public virtual async Task DbSet_query_first_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context)
                => context.Customers.OrderBy(c => c.CustomerID).First());

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context)).CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task Keyless_query_async()
    {
        var query = EF.CompileAsyncQuery((NorthwindContext context) => context.CustomerQueries);

        using (var context = CreateContext())
        {
            Assert.Equal(91, (await query(context).ToListAsync()).Count);
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, (await query(context).ToListAsync()).Count);
        }
    }

    [ConditionalFact]
    public virtual async Task Keyless_query_first_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context)
                => context.CustomerQueries.OrderBy(c => c.CompanyName).First());

        using (var context = CreateContext())
        {
            Assert.Equal("Alfreds Futterkiste", (await query(context)).CompanyName);
        }
    }

    [ConditionalFact]
    public virtual async Task Untyped_context_async()
    {
        var query = EF.CompileAsyncQuery((DbContext context) => context.Set<Customer>());

        using (var context = CreateContext())
        {
            Assert.Equal(91, (await query(context).ToListAsync()).Count);
        }

        using (var context = CreateContext())
        {
            Assert.Equal(91, (await query(context).ToListAsync()).Count);
        }
    }

    [ConditionalFact]
    public virtual async Task Query_with_single_parameter_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context, "ALFKI").ToListAsync()).First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", (await query(context, "ANATR").ToListAsync()).First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task First_query_with_single_parameter_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.First(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context, "ALFKI")).CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", (await query(context, "ANATR")).CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task First_query_with_cancellation_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, string customerID, CancellationToken ct)
                => context.Customers.First(c => c.CustomerID == customerID));

        var cancellationToken = default(CancellationToken);

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context, "ALFKI", cancellationToken)).CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", (await query(context, "ANATR", cancellationToken)).CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task Query_with_two_parameters_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, object _, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context, null, "ALFKI").ToListAsync()).First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", (await query(context, null, "ANATR").ToListAsync()).First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task Query_with_three_parameters_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, object _, int __, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context, null, 1, "ALFKI").ToListAsync()).First().CustomerID);
        }

        using (var context = CreateContext())
        {
            Assert.Equal("ANATR", (await query(context, null, 1, "ANATR").ToListAsync()).First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task Query_with_array_parameter_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, string[] args)
                => context.Customers.Where(c => c.CustomerID == args[0]));

        using (var context = CreateContext())
        {
            Assert.Equal(1, await CountAsync(query(context, ["ALFKI"])));
        }

        using (var context = CreateContext())
        {
            Assert.Equal(1, await CountAsync(query(context, ["ANATR"])));
        }
    }

    [ConditionalFact]
    public virtual async Task Query_with_closure_async()
    {
        var customerID = "ALFKI";

        var query = EF.CompileAsyncQuery(
            (NorthwindContext context)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context).ToListAsync()).First().CustomerID);
        }

        customerID = "ANATR";

        using (var context = CreateContext())
        {
            Assert.Equal("ALFKI", (await query(context).ToListAsync()).First().CustomerID);
        }
    }

    [ConditionalFact]
    public virtual async Task Query_with_closure_async_null()
    {
        string customerID = null;

        var query = EF.CompileAsyncQuery(
            (NorthwindContext context)
                => context.Customers.Where(c => c.CustomerID == customerID));

        using (var context = CreateContext())
        {
            Assert.Empty(await query(context).ToListAsync());
        }
    }

    [ConditionalFact]
    public virtual void Compiled_query_when_does_not_end_in_query_operator()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string customerID)
                => context.Customers.Where(c => c.CustomerID == customerID).Count() == 1);

        using (var context = CreateContext())
        {
            Assert.True(query(context, "ALFKI"));
        }
    }

    [ConditionalFact]
    public virtual void Compiled_query_when_using_member_on_context()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context)
                => context.Customers.Where(c => c.CustomerID.StartsWith(context.TenantPrefix)));

        using (var context = CreateContext())
        {
            context.TenantPrefix = "A";

            // Parameter-specific evaluation in ParameterExtractor. Issue #19209.
            // Assert.Equal(6, query(context).Count())
            Assert.Equal(4, query(context).Count());

            context.TenantPrefix = "B";
            Assert.Equal(4, query(context).Count());
        }
    }

    [ConditionalFact]
    public virtual async Task Compiled_query_with_max_parameters()
    {
        var syncEnumerableQuery = EF.CompileQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    string s15)
                => context.Set<Customer>()
                    .Where(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14
                            || c.CustomerID == s15));

        var syncIncludeEnumerableQuery = EF.CompileQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    string s15)
                => context.Set<Customer>()
                    .Where(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14
                            || c.CustomerID == s15)
                    .Include(c => c.Orders));

        var syncSingleResultQuery = EF.CompileQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    string s15)
                => context.Set<Customer>()
                    .Count(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14
                            || c.CustomerID == s15));

        var asyncEnumerableQuery = EF.CompileAsyncQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    string s15)
                => context.Set<Customer>()
                    .Where(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14
                            || c.CustomerID == s15));

        var asyncIncludeEnumerableQuery = EF.CompileAsyncQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    string s15)
                => context.Set<Customer>()
                    .Where(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14
                            || c.CustomerID == s15)
                    .Include(c => c.Orders));

        var asyncSingleResultQuery = EF.CompileAsyncQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    string s15)
                => context.Set<Customer>()
                    .Count(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14
                            || c.CustomerID == s15));

        var asyncSingleResultQueryWithCancellationToken = EF.CompileAsyncQuery(
            (
                    NorthwindContext context,
                    string s1,
                    string s2,
                    string s3,
                    string s4,
                    string s5,
                    string s6,
                    string s7,
                    string s8,
                    string s9,
                    string s10,
                    string s11,
                    string s12,
                    string s13,
                    string s14,
                    CancellationToken ct)
                => context.Set<Customer>()
                    .Count(
                        c => c.CustomerID == s1
                            || c.CustomerID == s2
                            || c.CustomerID == s3
                            || c.CustomerID == s4
                            || c.CustomerID == s5
                            || c.CustomerID == s6
                            || c.CustomerID == s7
                            || c.CustomerID == s8
                            || c.CustomerID == s9
                            || c.CustomerID == s10
                            || c.CustomerID == s11
                            || c.CustomerID == s12
                            || c.CustomerID == s13
                            || c.CustomerID == s14));

        using var context = CreateContext();

        var syncEnumerableResult = syncEnumerableQuery(
            context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
            "CHOPS", "CONSH", "RANDM").ToList();
        Assert.Equal(14, syncEnumerableResult.Count);

        var syncIncludeEnumerableResult = syncIncludeEnumerableQuery(
            context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
            "CHOPS", "CONSH", "RANDM").ToList();
        Assert.Equal(14, syncIncludeEnumerableResult.Count);
        Assert.All(syncIncludeEnumerableResult, t => Assert.NotNull(t.Orders));

        Assert.Equal(
            14,
            syncSingleResultQuery(
                context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
                "CHOPS", "CONSH", "RANDM"));

        var asyncEnumerableResult = await asyncEnumerableQuery(
            context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
            "CHOPS", "CONSH", "RANDM").ToListAsync();
        Assert.Equal(14, asyncEnumerableResult.Count);

        var asyncIncludeEnumerableResult = await asyncIncludeEnumerableQuery(
            context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
            "CHOPS", "CONSH", "RANDM").ToListAsync();
        Assert.Equal(14, asyncIncludeEnumerableResult.Count);
        Assert.All(asyncIncludeEnumerableResult, t => Assert.NotNull(t.Orders));

        Assert.Equal(
            14,
            await asyncSingleResultQuery(
                context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
                "CHOPS", "CONSH", "RANDM"));

        Assert.Equal(
            14,
            await asyncSingleResultQueryWithCancellationToken(
                context, "ALFKI", "ANATR", "ANTON", "AROUT", "BERGS", "BLAUS", "BLONP", "BOLID", "BONAP", "BSBEV", "CACTU", "CENTC",
                "CHOPS", "CONSH", default));
    }

    protected async Task<int> CountAsync<T>(IAsyncEnumerable<T> source)
    {
        var count = 0;
        await foreach (var _ in source)
        {
            count++;
        }

        return count;
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };
}
