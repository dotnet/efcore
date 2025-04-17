// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query;

public class CosmosAdvancedTranslationTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Math_functions_with_client_evaluation(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("MathTest");
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                testDatabase.ConnectionUri,
                testDatabase.AuthToken,
                testDatabase.Name)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        try
        {
            await context.AddRangeAsync(
                new NumericEntity { Id = "1", Value = 1.5 },
                new NumericEntity { Id = "2", Value = 2.7 },
                new NumericEntity { Id = "3", Value = 3.2 });
            await context.SaveChangesAsync();

            // Test Log2 function which requires client evaluation
            var query = context.Numbers
                .Where(e => Math.Log2(e.Value) > 1.0);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

            Assert.Contains("The LINQ expression", exception.Message);

            // Test complex math expressions
            var complexQuery = context.Numbers
                .Where(e => Math.Round(Math.Pow(e.Value, 2), 2) > 5.0);

            exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (async ? complexQuery.ToListAsync() : Task.FromResult(complexQuery.ToList())));

            Assert.Contains("The LINQ expression", exception.Message);
        }
        finally
        {
            await testDatabase.DisposeAsync();
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task String_operations_with_client_evaluation(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("StringTest");
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                testDatabase.ConnectionUri,
                testDatabase.AuthToken,
                testDatabase.Name)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        try
        {
            await context.AddRangeAsync(
                new StringEntity { Id = "1", Text = "Hello" },
                new StringEntity { Id = "2", Text = "World" });
            await context.SaveChangesAsync();

            // Test string.IsNullOrEmpty
            var query = context.Strings
                .Where(e => !string.IsNullOrEmpty(e.Text));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

            Assert.Contains("The LINQ expression", exception.Message);

            // Test complex string comparisons
            var complexQuery = context.Strings
                .Where(e => e.Text.CompareTo("Test") > 0);

            exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await (async ? complexQuery.ToListAsync() : Task.FromResult(complexQuery.ToList())));

            Assert.Contains("The LINQ expression", exception.Message);
        }
        finally
        {
            await testDatabase.DisposeAsync();
        }
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Regex_operations_with_different_options(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("RegexTest");
        var options = new DbContextOptionsBuilder()
            .UseCosmos(
                testDatabase.ConnectionUri,
                testDatabase.AuthToken,
                testDatabase.Name)
            .Options;

        var context = new TestContext(options);
        await context.Database.EnsureCreatedAsync();

        try
        {
            await context.AddRangeAsync(
                new StringEntity { Id = "1", Text = "Test123" },
                new StringEntity { Id = "2", Text = "123Test" });
            await context.SaveChangesAsync();

            // Test regex with RightToLeft option (not supported)
            var query = context.Strings
                .Where(e => System.Text.RegularExpressions.Regex.IsMatch(
                    e.Text, "^Test", System.Text.RegularExpressions.RegexOptions.RightToLeft));

            await Assert.ThrowsAsync<CosmosException>(
                async () => await (async ? query.ToListAsync() : Task.FromResult(query.ToList())));

            // Test regex with combined options
            var complexQuery = context.Strings
                .Where(e => System.Text.RegularExpressions.Regex.IsMatch(
                    e.Text, 
                    "^test", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | 
                    System.Text.RegularExpressions.RegexOptions.Multiline));

            // This should work as these options are supported
            var results = async 
                ? await complexQuery.ToListAsync()
                : complexQuery.ToList();

            Assert.Single(results);
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