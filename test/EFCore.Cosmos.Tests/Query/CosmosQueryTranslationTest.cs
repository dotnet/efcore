// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query;

public class CosmosQueryTranslationTest : IAsyncLifetime
{
    private CosmosTestStore _testStore;
    protected TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
    protected ILoggerFactory ListLoggerFactory { get; }

    public CosmosQueryTranslationTest()
    {
        ListLoggerFactory = new TestSqlLoggerFactory();
    }

    public async Task InitializeAsync()
    {
        _testStore = await CosmosTestStore.CreateInitializedAsync("QueryTranslationTest");
    }

    public async Task DisposeAsync()
    {
        await _testStore.DisposeAsync();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Subquery_with_client_evaluation(bool async)
    {
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                _testStore.ConnectionUri,
                _testStore.AuthToken,
                _testStore.Name)
            .UseLoggerFactory(ListLoggerFactory)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        await context.AddRangeAsync(
            new TestEntity { Id = "1", Value = 10 },
            new TestEntity { Id = "2", Value = 20 },
            new TestEntity { Id = "3", Value = 30 });
        await context.SaveChangesAsync();

        var query = context.Entities
            .Where(e => e.Value > context.Entities.Average(x => x.Value));

        // Should throw with specific error about client evaluation
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("The LINQ expression", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "TestEntity") AND (c["Value"] > 20))
"""
        });
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Complex_navigation_with_skip_take(bool async)
    {
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                _testStore.ConnectionUri,
                _testStore.AuthToken,
                _testStore.Name)
            .UseLoggerFactory(ListLoggerFactory)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        var parent = new ParentEntity
        {
            Id = "1",
            Children = new List<ChildEntity>
            {
                new() { Id = "c1", Value = 100 },
                new() { Id = "c2", Value = 200 },
                new() { Id = "c3", Value = 300 }
            }
        };

        await context.Parents.AddAsync(parent);
        await context.SaveChangesAsync();

        // Test Skip/Take in subquery with owned type
        var query = context.Parents
            .Select(p => new
            {
                ParentId = p.Id,
                TopChildren = p.Children.OrderBy(c => c.Value).Skip(1).Take(1)
            });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("Limit/Offset not supported in subqueries", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE {
    "ParentId" : c["id"],
    "TopChildren" : (
        SELECT VALUE c
        FROM c IN p["Children"]
        ORDER BY c["Value"]
        OFFSET 1 LIMIT 1
    )
}
FROM root c
WHERE (c["Discriminator"] = "ParentEntity")
"""
        });
    }

    private class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TestEntity> Entities { get; set; }
        public DbSet<ParentEntity> Parents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>()
                .HasPartitionKey(e => e.Id);

            modelBuilder.Entity<ParentEntity>()
                .HasPartitionKey(e => e.Id)
                .OwnsMany(p => p.Children);
        }
    }

    private class TestEntity
    {
        public string Id { get; set; }
        public int Value { get; set; }
    }

    private class ParentEntity
    {
        public string Id { get; set; }
        public List<ChildEntity> Children { get; set; }
    }

    private class ChildEntity
    {
        public string Id { get; set; }
        public int Value { get; set; }
    }
}