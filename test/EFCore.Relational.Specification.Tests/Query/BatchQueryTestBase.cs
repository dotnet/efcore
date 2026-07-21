// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class BatchQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    protected BatchQueryTestBase(TFixture fixture)
        : base(fixture)
        => Fixture.TestSqlLoggerFactory.Clear();

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_two_queries_returns_both_result_sets(bool async)
    {
        using var context = CreateContext();

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London").OrderBy(c => c.CustomerID),
                ctx => ctx.Set<Order>().Where(o => o.OrderID < 10260).OrderBy(o => o.OrderID))
            : context.Batch(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London").OrderBy(c => c.CustomerID),
                ctx => ctx.Set<Order>().Where(o => o.OrderID < 10260).OrderBy(o => o.OrderID));

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var orders = async
            ? await batch.NextAsListAsync<Order>()
            : batch.NextAsList<Order>();

        Assert.Equal(6, customers.Count);
        Assert.All(customers, c => Assert.Equal("London", c.City));
        Assert.Equal(12, orders.Count);
        Assert.All(orders, o => Assert.True(o.OrderID < 10260));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_executes_as_single_command(bool async)
    {
        using var context = CreateContext();
        Fixture.TestSqlLoggerFactory.Clear();

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                ctx => ctx.Set<Employee>().Where(e => e.City == "London"))
            : context.Batch(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                ctx => ctx.Set<Employee>().Where(e => e.City == "London"));

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var employees = async
            ? await batch.NextAsListAsync<Employee>()
            : batch.NextAsList<Employee>();

        Assert.Equal(6, customers.Count);
        Assert.Equal(4, employees.Count);
        Assert.Single(Fixture.TestSqlLoggerFactory.SqlStatements);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_parameters_in_each_query(bool async)
    {
        using var context = CreateContext();

        var city = "London";
        var orderId = 10260;

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>().Where(c => c.City == city),
                ctx => ctx.Set<Order>().Where(o => o.OrderID < orderId))
            : context.Batch(
                ctx => ctx.Set<Customer>().Where(c => c.City == city),
                ctx => ctx.Set<Order>().Where(o => o.OrderID < orderId));

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var orders = async
            ? await batch.NextAsListAsync<Order>()
            : batch.NextAsList<Order>();

        Assert.Equal(6, customers.Count);
        Assert.Equal(12, orders.Count);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_same_parameter_name_in_both_queries(bool async)
    {
        using var context = CreateContext();

        using var batch = async
            ? await context.BatchAsync(
                ctx =>
                {
                    var city = "London";
                    return ctx.Set<Customer>().Where(c => c.City == city);
                },
                ctx =>
                {
                    var city = "Seattle";
                    return ctx.Set<Employee>().Where(e => e.City == city);
                })
            : context.Batch(
                ctx =>
                {
                    var city = "London";
                    return ctx.Set<Customer>().Where(c => c.City == city);
                },
                ctx =>
                {
                    var city = "Seattle";
                    return ctx.Set<Employee>().Where(e => e.City == city);
                });

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var employees = async
            ? await batch.NextAsListAsync<Employee>()
            : batch.NextAsList<Employee>();

        Assert.Equal(6, customers.Count);
        Assert.All(customers, c => Assert.Equal("London", c.City));
        Assert.Equal(2, employees.Count);
        Assert.All(employees, e => Assert.Equal("Seattle", e.City));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_collection_include(bool async)
    {
        using var context = CreateContext();

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>().Include(c => c.Orders).Where(c => c.City == "London").OrderBy(c => c.CustomerID),
                ctx => ctx.Set<Employee>().Where(e => e.City == "London"))
            : context.Batch(
                ctx => ctx.Set<Customer>().Include(c => c.Orders).Where(c => c.City == "London").OrderBy(c => c.CustomerID),
                ctx => ctx.Set<Employee>().Where(e => e.City == "London"));

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var employees = async
            ? await batch.NextAsListAsync<Employee>()
            : batch.NextAsList<Employee>();

        Assert.Equal(6, customers.Count);
        Assert.All(customers, c => Assert.True(c.Orders.Count > 0));
        Assert.Equal(4, employees.Count);
    }

    [Fact]
    public virtual void Batch_next_as_enumerable_streams()
    {
        using var context = CreateContext();

        using var batch = context.Batch(
            ctx => ctx.Set<Customer>().Where(c => c.City == "London").OrderBy(c => c.CustomerID),
            ctx => ctx.Set<Employee>().Where(e => e.City == "London"));

        var customerCount = 0;
        foreach (var customer in batch.NextAsEnumerable<Customer>())
        {
            Assert.Equal("London", customer.City);
            customerCount++;
        }

        var employees = batch.NextAsList<Employee>();

        Assert.Equal(6, customerCount);
        Assert.Equal(4, employees.Count);
    }

    [Fact]
    public virtual async Task Batch_next_as_async_enumerable()
    {
        using var context = CreateContext();

        await using var batch = await context.BatchAsync(
            ctx => ctx.Set<Customer>().Where(c => c.City == "London").OrderBy(c => c.CustomerID),
            ctx => ctx.Set<Employee>().Where(e => e.City == "London"));

        var customerCount = 0;
        await foreach (var customer in batch.NextAsAsyncEnumerable<Customer>())
        {
            Assert.Equal("London", customer.City);
            customerCount++;
        }

        var employees = await batch.NextAsListAsync<Employee>();

        Assert.Equal(6, customerCount);
        Assert.Equal(4, employees.Count);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_tracks_entities(bool async)
    {
        using var context = CreateContext();

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                ctx => ctx.Set<Order>().Where(o => o.CustomerID == "AROUT"))
            : context.Batch(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                ctx => ctx.Set<Order>().Where(o => o.CustomerID == "AROUT"));

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var orders = async
            ? await batch.NextAsListAsync<Order>()
            : batch.NextAsList<Order>();

        Assert.Equal(customers.Count, context.ChangeTracker.Entries<Customer>().Count());
        Assert.Equal(orders.Count, context.ChangeTracker.Entries<Order>().Count());

        // Identity resolution: the tracked customer instance is fixed up with the tracked orders
        var arout = customers.Single(c => c.CustomerID == "AROUT");
        Assert.All(orders, o => Assert.Same(arout, o.Customer));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_composed_from_sql_parameters(bool async)
    {
        using var context = CreateContext();

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>()
                    .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), "London")
                    .OrderBy(c => c.CustomerID),
                ctx => ctx.Set<Employee>().Where(e => e.City == "London"))
            : context.Batch(
                ctx => ctx.Set<Customer>()
                    .FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = {0}"), "London")
                    .OrderBy(c => c.CustomerID),
                ctx => ctx.Set<Employee>().Where(e => e.City == "London"));

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var employees = async
            ? await batch.NextAsListAsync<Employee>()
            : batch.NextAsList<Employee>();

        Assert.Equal(6, customers.Count);
        Assert.Equal(4, employees.Count);
    }

    [Fact]
    public virtual void Batch_throws_for_empty()
    {
        using var context = CreateContext();

        Assert.Equal(
            RelationalStrings.BatchQueryEmpty,
            Assert.Throws<InvalidOperationException>(() => context.Batch()).Message);
    }

    [Fact]
    public virtual void Batch_throws_for_split_query()
    {
        using var context = CreateContext();

        Assert.Equal(
            RelationalStrings.BatchQueryUnsupportedQueryType,
            Assert.Throws<InvalidOperationException>(
                () => context.Batch(
                    ctx => ctx.Set<Customer>().Include(c => c.Orders).AsSplitQuery())).Message);
    }

    [Fact]
    public virtual void Batch_throws_for_non_composed_from_sql()
    {
        using var context = CreateContext();

        Assert.Equal(
            RelationalStrings.BatchQueryUnsupportedQueryType,
            Assert.Throws<InvalidOperationException>(
                () => context.Batch(
                    ctx => ctx.Set<Customer>().FromSqlRaw(NormalizeDelimitersInRawString("SELECT * FROM [Customers]")))).Message);
    }

    [Fact]
    public virtual void Batch_throws_for_streaming_group_by()
    {
        using var context = CreateContext();

        Assert.Equal(
            RelationalStrings.BatchQueryUnsupportedQueryType,
            Assert.Throws<InvalidOperationException>(
                () => context.Batch(
                    ctx => ctx.Set<Order>().GroupBy(o => o.CustomerID))).Message);
    }

    [Fact]
    public virtual void Batch_throws_when_result_sets_exhausted()
    {
        using var context = CreateContext();

        using var batch = context.Batch(ctx => ctx.Set<Customer>().Where(c => c.City == "London"));

        var customers = batch.NextAsList<Customer>();
        Assert.Equal(6, customers.Count);

        Assert.Equal(
            RelationalStrings.BatchQueryNoMoreResultSets(1),
            Assert.Throws<InvalidOperationException>(() => batch.NextAsList<Customer>()).Message);
    }

    [Fact]
    public virtual void Batch_throws_on_result_type_mismatch()
    {
        using var context = CreateContext();

        using var batch = context.Batch(ctx => ctx.Set<Customer>().Where(c => c.City == "London"));

        Assert.Equal(
            RelationalStrings.BatchQueryResultTypeMismatch(nameof(Customer), nameof(Order)),
            Assert.Throws<InvalidOperationException>(() => batch.NextAsList<Order>()).Message);
    }

    [Fact]
    public virtual void Batch_disposes_and_releases_connection()
    {
        using var context = CreateContext();
        var connection = context.Database.GetDbConnection();
        var initialState = connection.State;

        var batch = context.Batch(
            ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
            ctx => ctx.Set<Employee>().Where(e => e.City == "London"));

        Assert.Equal(ConnectionState.Open, connection.State);

        var customers = batch.NextAsList<Customer>();
        Assert.Equal(6, customers.Count);

        batch.Dispose();

        // The ref-counted connection returns to its pre-batch state
        Assert.Equal(initialState, connection.State);

        // The context remains usable for further queries
        Assert.Equal(4, context.Set<Employee>().Count(e => e.City == "London"));
    }

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
