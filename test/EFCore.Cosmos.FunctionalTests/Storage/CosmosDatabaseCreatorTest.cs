// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

#nullable disable

[CosmosCondition(CosmosCondition.DoesNotUseTokenCredential)]
public class CosmosDatabaseCreatorTest
{
    public static readonly IEnumerable<object[]> IsAsyncData = [[true]];

    [ConditionalFact]
    public async Task EnsureCreated_returns_true_when_database_does_not_exist()
    {
        await using var testDatabase = CosmosTestStoreFactory.Instance.Create("NonExistingDatabase");
        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();
        await LockAround(() => creator.EnsureDeletedAsync());
        Assert.True(await LockAround(() => creator.EnsureCreatedAsync()));
    }

    [ConditionalFact]
    public async Task EnsureCreated_returns_true_when_database_exists_but_collections_do_not()
    {
        await using var testDatabase = CosmosTestStoreFactory.Instance.Create("EnsureCreatedTest");
        await testDatabase.InitializeAsync(testDatabase.ServiceProvider, () => new BaseContext(testDatabase));

        using var context = new BloggingContext(testDatabase);
        var creator = context.GetService<IDatabaseCreator>();
        Assert.True(await LockAround(() => creator.EnsureCreatedAsync()));
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public Task EnsureCreated_returns_false_when_database_and_collections_exist(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await using var testDatabase = CosmosTestStoreFactory.Instance.Create("EnsureCreatedReady");
                await testDatabase.InitializeAsync(
                    testDatabase.ServiceProvider, testStore => new BloggingContext((CosmosTestStore)testStore));

                using var context = new BloggingContext(testDatabase);
                var creator = context.GetService<IDatabaseCreator>();

                Assert.False(a ? await LockAround(() => creator.EnsureCreatedAsync()) : creator.EnsureCreated());
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public Task EnsureDeleted_returns_true_when_database_exists(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await using var testDatabase = await CosmosTestStoreFactory.Instance.CreateInitializedAsync("EnsureDeleteBlogging");
                using var context = new BloggingContext(testDatabase);
                var creator = context.GetService<IDatabaseCreator>();

                Assert.True(a ? await LockAround(() => creator.EnsureDeletedAsync()) : creator.EnsureDeleted());
            });

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public Task EnsureDeleted_returns_false_when_database_does_not_exist(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await using var testDatabase = CosmosTestStoreFactory.Instance.Create("EnsureDeleteBlogging");
                using var context = new BloggingContext(testDatabase);
                var creator = context.GetService<IDatabaseCreator>();

                Assert.False(a ? await LockAround(() => creator.EnsureDeletedAsync()) : creator.EnsureDeleted());
            });

    [ConditionalFact]
    public async Task EnsureCreated_throws_for_missing_seed()
    {
        await using var testDatabase = await CosmosTestStoreFactory.Instance.CreateInitializedAsync("EnsureCreatedSeedTest");
        using var context = new BloggingContext(testDatabase, seed: true);

        Assert.Equal(
            CoreStrings.MissingSeeder,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => context.Database.EnsureCreatedAsync())).Message);
    }

    private async Task<bool> LockAround(Func<Task<bool>> action)
    {
        if (TestEnvironment.IsEmulator)
        {
            await CosmosEmulatorTestStore.ContainerCrudSemaphore.WaitAsync();
        }
        try
        {
            return await action();
        }
        finally
        {
            if (TestEnvironment.IsEmulator)
            {
                CosmosEmulatorTestStore.ContainerCrudSemaphore.Release();
            }
        }
    }

    private class BaseContext(CosmosTestStore testStore) : DbContext
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
    }

    private class BloggingContext(CosmosTestStore testStore, bool seed = false) : BaseContext(testStore)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (seed)
            {
                optionsBuilder.UseSeeding((_, __) => { });
            }
        }

        public DbSet<Blog> Blogs { get; set; }
    }

    private class Blog
    {
        public int Id { get; set; }
    }
}
