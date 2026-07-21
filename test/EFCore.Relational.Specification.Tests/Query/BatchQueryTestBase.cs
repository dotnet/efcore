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
            ? await context.BatchAsync(CustomersQuery(), EmployeesQuery())
            : context.Batch(CustomersQuery(), EmployeesQuery());

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

        static Expression<Func<NorthwindContext, object>> CustomersQuery()
        {
            var city = "London";
            return ctx => ctx.Set<Customer>().Where(c => c.City == city);
        }

        static Expression<Func<NorthwindContext, object>> EmployeesQuery()
        {
            var city = "Seattle";
            return ctx => ctx.Set<Employee>().Where(e => e.City == city);
        }
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_execute_delete(bool async)
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = async
                    ? await context.Database.BeginTransactionAsync()
                    : context.Database.BeginTransaction();

                using var batch = async
                    ? await context.BatchAsync(
                        ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                        ctx => ctx.Set<OrderDetail>().Where(od => od.OrderID == 10248).ExecuteDelete(),
                        ctx => ctx.Set<OrderDetail>().Where(od => od.OrderID == 10248))
                    : context.Batch(
                        ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                        ctx => ctx.Set<OrderDetail>().Where(od => od.OrderID == 10248).ExecuteDelete(),
                        ctx => ctx.Set<OrderDetail>().Where(od => od.OrderID == 10248));

                var customers = async
                    ? await batch.NextAsListAsync<Customer>()
                    : batch.NextAsList<Customer>();
                var rowsAffected = async
                    ? await batch.NextAsRowsAffectedAsync()
                    : batch.NextAsRowsAffected();
                var remainingDetails = async
                    ? await batch.NextAsListAsync<OrderDetail>()
                    : batch.NextAsList<OrderDetail>();

                Assert.Equal(6, customers.Count);
                Assert.Equal(3, rowsAffected);

                // The query following the delete in the same command sees the deletion
                Assert.Empty(remainingDetails);
            });
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_execute_update(bool async)
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = async
                    ? await context.Database.BeginTransactionAsync()
                    : context.Database.BeginTransaction();

                using var batch = async
                    ? await context.BatchAsync(
                        ctx => ctx.Set<Customer>().Where(c => c.City == "London")
                            .ExecuteUpdate(s => s.SetProperty(c => c.ContactName, "Updated")),
                        ctx => ctx.Set<Customer>().Where(c => c.ContactName == "Updated"))
                    : context.Batch(
                        ctx => ctx.Set<Customer>().Where(c => c.City == "London")
                            .ExecuteUpdate(s => s.SetProperty(c => c.ContactName, "Updated")),
                        ctx => ctx.Set<Customer>().Where(c => c.ContactName == "Updated"));

                var rowsAffected = async
                    ? await batch.NextAsRowsAffectedAsync()
                    : batch.NextAsRowsAffected();
                var updatedCustomers = async
                    ? await batch.NextAsListAsync<Customer>()
                    : batch.NextAsList<Customer>();

                Assert.Equal(6, rowsAffected);

                // The query following the update in the same command sees the updated values
                Assert.Equal(6, updatedCustomers.Count);
                Assert.All(updatedCustomers, c => Assert.Equal("Updated", c.ContactName));
            });
    }

    [Fact]
    public virtual async Task Batch_with_execute_delete_async_form()
    {
        using var context = CreateContext();
        await context.Database.CreateExecutionStrategy().ExecuteAsync(
            async () =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                await using var batch = await context.BatchAsync(
                    ctx => ctx.Set<OrderDetail>().Where(od => od.OrderID == 10248).ExecuteDeleteAsync(CancellationToken.None));

                var rowsAffected = await batch.NextAsRowsAffectedAsync();

                Assert.Equal(3, rowsAffected);
            });
    }

    [Fact]
    public virtual void Batch_throws_when_consuming_rows_affected_as_list()
    {
        using var context = CreateContext();
        context.Database.CreateExecutionStrategy().Execute(
            () =>
            {
                using var transaction = context.Database.BeginTransaction();

                using var batch = context.Batch(ctx => ctx.Set<OrderDetail>().Where(od => od.OrderID == 10248).ExecuteDelete());

                Assert.Equal(
                    RelationalStrings.BatchQueryExpectedRowsAffected,
                    Assert.Throws<InvalidOperationException>(() => batch.NextAsList<OrderDetail>()).Message);
            });
    }

    [Fact]
    public virtual void Batch_throws_when_consuming_query_as_rows_affected()
    {
        using var context = CreateContext();

        using var batch = context.Batch(ctx => ctx.Set<Customer>().Where(c => c.City == "London"));

        Assert.Equal(
            RelationalStrings.BatchQueryExpectedQueryResults,
            Assert.Throws<InvalidOperationException>(() => batch.NextAsRowsAffected()).Message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_with_anonymous_type_projection(bool async)
    {
        using var context = CreateContext();

        var customerStats = BatchQuery.Create((NorthwindContext ctx) => ctx.Set<Customer>()
            .Where(c => c.City == "London")
            .OrderBy(c => c.CustomerID)
            .Select(c => new { c.CustomerID, c.City, OrderCount = c.Orders.Count() }));
        var employeeNames = BatchQuery.Create((NorthwindContext ctx) => ctx.Set<Employee>()
            .Where(e => e.City == "London")
            .Select(e => new { e.EmployeeID, e.FirstName }));

        using var batch = async
            ? await context.BatchAsync(customerStats, employeeNames)
            : context.Batch(customerStats, employeeNames);

        var stats = async
            ? await batch.NextAsListAsync(customerStats)
            : batch.NextAsList(customerStats);
        var names = async
            ? await batch.NextAsListAsync(employeeNames)
            : batch.NextAsList(employeeNames);

        Assert.Equal(6, stats.Count);
        Assert.All(stats, s => Assert.Equal("London", s.City));
        Assert.Equal("AROUT", stats[0].CustomerID);
        Assert.Equal(13, stats[0].OrderCount);
        Assert.Equal(4, names.Count);
        Assert.All(names, n => Assert.False(string.IsNullOrEmpty(n.FirstName)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Batch_query_wrapper_mixed_with_plain_lambda(bool async)
    {
        using var context = CreateContext();

        var orderIds = BatchQuery.Create((NorthwindContext ctx) => ctx.Set<Order>()
            .Where(o => o.CustomerID == "AROUT")
            .OrderBy(o => o.OrderID)
            .Select(o => new { o.OrderID }));

        using var batch = async
            ? await context.BatchAsync(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                orderIds)
            : context.Batch(
                ctx => ctx.Set<Customer>().Where(c => c.City == "London"),
                orderIds);

        var customers = async
            ? await batch.NextAsListAsync<Customer>()
            : batch.NextAsList<Customer>();
        var ids = new List<int>();
        if (async)
        {
            await foreach (var row in batch.NextAsAsyncEnumerable(orderIds))
            {
                ids.Add(row.OrderID);
            }
        }
        else
        {
            foreach (var row in batch.NextAsEnumerable(orderIds))
            {
                ids.Add(row.OrderID);
            }
        }

        Assert.Equal(6, customers.Count);
        Assert.Equal(13, ids.Count);
        Assert.Equal(10355, ids[0]);
    }

    [Fact]
    public virtual void Batch_throws_for_unsupported_expression()
    {
        using var context = CreateContext();

        var message = Assert.Throws<InvalidOperationException>(
            () => context.Batch(ctx => ctx.Set<Customer>().Count())).Message;

        Assert.Contains("cannot be batched", message);
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
