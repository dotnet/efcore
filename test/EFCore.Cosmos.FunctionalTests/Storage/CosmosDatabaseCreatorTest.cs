// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

public class CosmosDatabaseCreatorTest
{
    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalFact]
    public async Task EnsureCreated_returns_true_when_database_does_not_exist()
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
            testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
        }
    }

    [ConditionalFact]
    public async Task EnsureCreated_returns_true_when_database_exists_but_collections_do_not()
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
            testDatabase.Initialize(testDatabase.ServiceProvider, () => new BloggingContext(testDatabase));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task EnsureCreated_returns_false_when_database_and_collections_exist(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureCreatedReady");
        testDatabase.Initialize(testDatabase.ServiceProvider, testStore => new BloggingContext((CosmosTestStore)testStore));

        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.False(async ? await creator.EnsureCreatedAsync() : creator.EnsureCreated());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task EnsureDeleted_returns_true_when_database_exists(bool async)
    {
        await using var testDatabase = CosmosTestStore.CreateInitialized("EnsureDeleteBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.True(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task EnsureDeleted_returns_false_when_database_does_not_exist(bool async)
    {
        await using var testDatabase = CosmosTestStore.Create("EnsureDeleteBlogging");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();

        Assert.False(async ? await creator.EnsureDeletedAsync() : creator.EnsureDeleted());
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
