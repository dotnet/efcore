// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query;

public class CosmosCollectionNavigationTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Collection_navigation_with_skip_take(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("CollectionTest");
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                testDatabase.ConnectionString,
                testDatabase.DatabaseId)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        try
        {
            var order = new Order
            {
                Id = "1",
                Items = new List<OrderItem>
                {
                    new() { Id = "i1", Quantity = 5 },
                    new() { Id = "i2", Quantity = 10 },
                    new() { Id = "i3", Quantity = 15 }
                }
            };

            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            // Test Skip/Take in subquery
            var query = context.Orders
                .Select(o => new
                {
                    OrderId = o.Id,
                    TopItems = o.Items.Skip(1).Take(1)
                });

            // Should throw as Skip/Take not supported in subqueries
            var exception = await Assert.ThrowsAsync<CosmosException>(
                async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));
            
            Assert.Contains("Limit/Offset not supported in subqueries", exception.Message);
        }
        finally
        {
            await testDatabase.DisposeAsync();
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Collection_aggregation_in_projection(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("AggregationTest");
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                testDatabase.ConnectionString,
                testDatabase.DatabaseId)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        try
        {
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

            // Test aggregation in projection
            var query = context.Orders
                .Select(o => new
                {
                    OrderId = o.Id,
                    TotalItems = o.Items.Count,
                    AverageQuantity = o.Items.Average(i => i.Quantity)
                });

            // Should throw as complex aggregations require client evaluation
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

            Assert.Contains("The LINQ expression", exception.Message);

            // Test nested aggregation in where clause
            var complexQuery = context.Orders
                .Where(o => o.Items.Sum(i => i.Quantity) > 
                    context.Orders.Average(x => x.Items.Sum(i => i.Quantity)));

            exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (async ? complexQuery.ToListAsync() : Task.FromResult(complexQuery.ToList())));

            Assert.Contains("The LINQ expression", exception.Message);
        }
        finally
        {
            await testDatabase.DisposeAsync();
        }
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
                .OwnsMany(e => e.Items);
        }
    }

    private class Order
    {
        public string Id { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    private class OrderItem
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
    }
}