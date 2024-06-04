// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

#nullable disable

public class CosmosDatabaseCreatorTest
{
    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalFact]
    public async Task EnsureCreatedAsync_returns_true_when_database_does_not_exist()
    {
        await using var testDatabase = CosmosTestStore.Create("NonExisting");
        try
        {
            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.True(await creator.EnsureCreatedAsync());
        }
        finally
        {
            await testDatabase.InitializeAsync(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
        }
    }

    [ConditionalFact]
    public async Task EnsureCreatedAsync_returns_true_when_database_exists_but_collections_do_not()
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureCreatedTest");
        try
        {
            using var context = new BloggingContext(testDatabase);
            var creator = context.GetService<IDatabaseCreator>();

            Assert.True(await creator.EnsureCreatedAsync());
        }
        finally
        {
            await testDatabase.InitializeAsync(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
        }
    }

    [ConditionalFact]
    public async Task EnsureCreatedAsync_returns_false_when_database_and_collections_exist()
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureCreatedReady");
        await testDatabase.InitializeAsync(
            testDatabase.ServiceProvider, testStore => new BloggingContext((CosmosTestStore)testStore));

        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.False(await creator.EnsureCreatedAsync());
    }

    [ConditionalFact]
    public async Task EnsureDeletedAsync_returns_true_when_database_exists()
    {
        await using var testDatabase = await CosmosTestStore.CreateInitializedAsync("EnsureDeleteBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.True(await creator.EnsureDeletedAsync());
    }

    [ConditionalFact]
    public async Task EnsureDeletedAsync_returns_false_when_database_does_not_exist()
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureDeleteBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.False(await creator.EnsureDeletedAsync());
    }

    [ConditionalFact]
    public async Task EnsureDeleted_sync_is_not_supported()
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureDeleteBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        CosmosTestHelpers.Instance.AssertSyncNotSupported(() => creator.EnsureDeleted());
    }

    [ConditionalFact]
    public async Task EnsureCreated_sync_is_not_supported()
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureDeleteBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        CosmosTestHelpers.Instance.AssertSyncNotSupported(() => creator.EnsureCreated());
    }

    private class BloggingContext(CosmosTestStore testStore) : DbContext
    {
        private readonly string _connectionUri = testStore.ConnectionUri;
        private readonly string _authToken = testStore.AuthToken;
        private readonly string _name = testStore.Name;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseCosmos(
                    _connectionUri,
                    _authToken,
                    _name,
                    b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
    }

    private class Blog
    {
        public int Id { get; set; }
    }
}
