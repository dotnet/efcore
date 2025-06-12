// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query;

public class CosmosTranslationsTest : IAsyncLifetime
{
    private CosmosTestStore _testStore;
    protected TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
    protected ILoggerFactory ListLoggerFactory { get; }

    public CosmosTranslationsTest()
    {
        ListLoggerFactory = new TestSqlLoggerFactory();
    }

    public async Task InitializeAsync()
    {
        _testStore = await CosmosTestStore.CreateInitializedAsync("TranslationTest");
    }

    public async Task DisposeAsync()
    {
        await _testStore.DisposeAsync();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Math_pow_with_round(bool async)
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

        await context.AddRangeAsync(
            new NumericEntity { Id = "1", Value = 1.5 },
            new NumericEntity { Id = "2", Value = 2.7 },
            new NumericEntity { Id = "3", Value = 3.2 });
        await context.SaveChangesAsync();

        // Test POW function with ROUND
        var query = context.Numbers
            .Where(e => Math.Round(Math.Pow(e.Value, 2), 2) > 5.0);

        // Should throw with specific error about client evaluation
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("The LINQ expression", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "NumericEntity") AND (ROUND(POW(c["Value"], 2.0), 2) > 5.0))
"""
        });
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task String_isnullorempty_with_concat(bool async)
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

        await context.AddRangeAsync(
            new StringEntity { Id = "1", Text = "Hello" },
            new StringEntity { Id = "2", Text = "World" });
        await context.SaveChangesAsync();

        // Test string.IsNullOrEmpty with string concatenation
        var query = context.Strings
            .Where(e => !string.IsNullOrEmpty(e.Text + "_suffix"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("The LINQ expression", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "StringEntity") AND (IS_NULL(CONCAT(c["Text"], "_suffix")) = false))
"""
        });
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task String_comparison_with_ordinal_ignorecase(bool async)
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

        await context.AddRangeAsync(
            new StringEntity { Id = "1", Text = "Test" },
            new StringEntity { Id = "2", Text = "test" });
        await context.SaveChangesAsync();

        // Test string comparison with StringComparison.OrdinalIgnoreCase
        var query = context.Strings
            .Where(e => e.Text.Equals("test", StringComparison.OrdinalIgnoreCase));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

        Assert.Contains("The LINQ expression", exception.Message);
        
        TestSqlLoggerFactory.AssertBaseline(new[] 
        {
            """
SELECT VALUE c
FROM root c
WHERE ((c["Discriminator"] = "StringEntity") AND StringEquals(c["Text"], "test", true))
"""
        });
    }

    private class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<NumericEntity> Numbers { get; set; }
        public DbSet<StringEntity> Strings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumericEntity>()
                .HasPartitionKey(e => e.Id);

            modelBuilder.Entity<StringEntity>()
                .HasPartitionKey(e => e.Id);
        }
    }

    private class NumericEntity
    {
        public string Id { get; set; }
        public double Value { get; set; }
    }

    private class StringEntity 
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }
}