// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query;

public class CosmosCollectionQueryTest : IAsyncLifetime
{
    private CosmosTestStore _testStore;
    protected TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
    protected ILoggerFactory ListLoggerFactory { get; }

    public CosmosCollectionQueryTest()
    {
        ListLoggerFactory = new TestSqlLoggerFactory();
    }

    public async Task InitializeAsync()
    {
        _testStore = await CosmosTestStore.CreateInitializedAsync("CollectionQueryTest");
    }

    public async Task DisposeAsync()
    {
        await _testStore.DisposeAsync();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Collection_aggregation_in_subquery(bool async)
    {
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                _testStore.ConnectionUri,
                _testStore.AuthToken,
                _testStore.Name)
            .UseLoggerFactory(ListLoggerFactory)
            .Options;

        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        var orders = new[]
        {
            new Order
            {
                Id = "1",
                Items = new List<OrderItem>
                {
                    new() { Id = "i1", Quantity = 5 },
                    new() { Id = "i2", Quantity = 10 }
                }
            },
            new Order
            {
                Id = "2",
                Items = new List<OrderItem>
                {
                    new() { Id = "i3", Quantity = 15 }
                }
            }
        };

        await context.Orders.AddRangeAsync(orders);
        await context.SaveChangesAsync();

        // Test aggregation in projection with correlated subquery
        var query = context.Orders
            .Where(o => o.Items.Average(i => i.Quantity) > 
                context.Orders.SelectMany(x => x.Items).Average(i => i.Quantity));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("The LINQ expression", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (AVG(c["Items"].Quantity) > AVG(ARRAY(SELECT VALUE i.Quantity FROM root o CROSS JOIN i IN o.Items))))
"""
        });
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Collection_complex_grouping_with_aggregates(bool async)
    {
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                _testStore.ConnectionUri,
                _testStore.AuthToken,
                _testStore.Name)
            .UseLoggerFactory(ListLoggerFactory)
            .Options;

        await using var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        var orders = new[]
        {
            new Order
            {
                Id = "1",
                CustomerId = "C1",
                Items = new List<OrderItem>
                {
                    new() { Id = "i1", ProductId = "P1", Quantity = 5 },
                    new() { Id = "i2", ProductId = "P2", Quantity = 10 }
                }
            },
            new Order
            {
                Id = "2",
                CustomerId = "C2",
                Items = new List<OrderItem>
                {
                    new() { Id = "i3", ProductId = "P1", Quantity = 15 }
                }
            }
        };

        await context.Orders.AddRangeAsync(orders);
        await context.SaveChangesAsync();

        // Test grouping by navigation property with multiple aggregates
        var query = context.Orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQuantity = g.Sum(i => i.Quantity),
                AverageQuantity = g.Average(i => i.Quantity),
                OrderCount = g.Count()
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("The LINQ expression", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE {
    "ProductId": g.key,
    "TotalQuantity": g.sum,
    "AverageQuantity": g.avg,
    "OrderCount": g.count
}
FROM (
    SELECT
        i["ProductId"] AS key,
        SUM(i["Quantity"]) AS sum,
        AVG(i["Quantity"]) AS avg,
        COUNT(1) AS count
    FROM root c
    CROSS JOIN i IN c.Items
    WHERE (c["Discriminator"] = "Order")
    GROUP BY i["ProductId"]
) AS g
"""
        });
    }

    private class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasPartitionKey(e => e.Id)
                .OwnsMany(o => o.Items);
        }
    }

    private class Order
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    private class OrderItem
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}